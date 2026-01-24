using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using Mockolate.Exceptions;
using Mockolate.Interactions;
using Mockolate.Internals;
using Mockolate.Parameters;
using Mockolate.Setup;

namespace Mockolate;

/// <summary>
///     The registration class for mocks.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public partial class MockRegistration
{
	/// <inheritdoc cref="MockRegistration" />
	public MockRegistration(MockBehavior behavior, string prefix)
	{
		Behavior = behavior;
		Prefix = prefix;
		Interactions = new MockInteractions();
	}

	/// <inheritdoc cref="MockRegistration" />
	internal MockRegistration(MockBehavior behavior, string prefix, MockInteractions interactions)
	{
		Behavior = behavior;
		Prefix = prefix;
		Interactions = interactions;
	}

	/// <summary>
	///     Gets the behavior settings used by this mock instance.
	/// </summary>
	public MockBehavior Behavior { get; }

	/// <summary>
	///     Gets the prefix string used to identify or categorize items within the context.
	/// </summary>
	public string Prefix { get; }

	/// <summary>
	///     Gets the collection of interactions recorded by the mock object.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public MockInteractions Interactions { get; }

	/// <summary>
	///     Clears all interactions recorded by the mock object.
	/// </summary>
	public void ClearAllInteractions()
		=> Interactions.Clear();

	/// <summary>
	///     Executes the method with <paramref name="methodName" /> and the matching <paramref name="parameters" /> and gets
	///     the setup return value.
	/// </summary>
	public MethodSetupResult<TResult> InvokeMethod<TResult>(string methodName,
		Func<object?[], TResult> defaultValue,
		params NamedParameterValue[] parameters)
	{
		MethodInvocation methodInvocation =
			((IMockInteractions)Interactions).RegisterInteraction(new MethodInvocation(Interactions.GetNextIndex(),
				methodName, parameters));

		IInteractiveMethodSetup? matchingSetup = GetMethodSetup(methodInvocation);
		if (matchingSetup is null)
		{
			if (Behavior.ThrowWhenNotSetup)
			{
				throw new MockNotSetupException(
					$"The method '{methodName}({string.Join(", ", parameters.Select(x => x.Value?.GetType().FormatType() ?? "<null>"))})' was invoked without prior setup.");
			}

			return new MethodSetupResult<TResult>(null, Behavior,
				defaultValue(parameters.Select(x => x.Value).ToArray()));
		}

		return new MethodSetupResult<TResult>(matchingSetup, Behavior,
			matchingSetup.Invoke(methodInvocation, Behavior,
				() => defaultValue(parameters.Select(x => x.Value).ToArray())));
	}

	/// <summary>
	///     Executes the method with <paramref name="methodName" /> and the matching <paramref name="parameters" /> returning
	///     <see langword="void" />.
	/// </summary>
	public MethodSetupResult InvokeMethod(string methodName, params NamedParameterValue[] parameters)
	{
		MethodInvocation methodInvocation =
			((IMockInteractions)Interactions).RegisterInteraction(new MethodInvocation(Interactions.GetNextIndex(),
				methodName, parameters));

		IInteractiveMethodSetup? matchingSetup = GetMethodSetup(methodInvocation);
		if (matchingSetup is null && Behavior.ThrowWhenNotSetup)
		{
			throw new MockNotSetupException(
				$"The method '{methodName}({string.Join(", ", parameters.Select(x => x.Value?.GetType().FormatType() ?? "<null>"))})' was invoked without prior setup.");
		}

		matchingSetup?.Invoke(methodInvocation, Behavior);
		return new MethodSetupResult(matchingSetup, Behavior);
	}

	/// <summary>
	///     Accesses the getter of the property with <paramref name="propertyName" />.
	/// </summary>
	public TResult GetProperty<TResult>(string propertyName, Func<TResult> defaultValueGenerator,
		Func<TResult>? baseValueAccessor)
	{
		IInteraction interaction =
			((IMockInteractions)Interactions).RegisterInteraction(new PropertyGetterAccess(Interactions.GetNextIndex(),
				propertyName));
		IInteractivePropertySetup matchingSetup = GetPropertySetup(propertyName,
			skipBase => skipBase || baseValueAccessor is null
				? defaultValueGenerator()
				: baseValueAccessor.Invoke());
		return matchingSetup.InvokeGetter(interaction, Behavior, defaultValueGenerator);
	}

	/// <summary>
	///     Accesses the setter of the property with <paramref name="propertyName" /> and the matching
	///     <paramref name="value" />.
	/// </summary>
	/// <remarks>
	///     Returns a flag, indicating whether the base class implementation should be skipped.
	/// </remarks>
	public bool SetProperty(string propertyName, object? value)
	{
		IInteraction interaction =
			((IMockInteractions)Interactions).RegisterInteraction(new PropertySetterAccess(Interactions.GetNextIndex(),
				propertyName, value));
		IInteractivePropertySetup matchingSetup = GetPropertySetup(propertyName, _ => null);
		matchingSetup.InvokeSetter(interaction, value, Behavior);
		return matchingSetup.SkipBaseClass() ?? Behavior.SkipBaseClass;
	}

	/// <summary>
	///     Gets the value from the indexer with the given parameters.
	/// </summary>
	public IndexerSetupResult<TResult> GetIndexer<TResult>(params NamedParameterValue[] parameters)
	{
		IndexerGetterAccess interaction = new(Interactions.GetNextIndex(), parameters);
		((IMockInteractions)Interactions).RegisterInteraction(interaction);

		IndexerSetup? matchingSetup = GetIndexerSetup(interaction);
		return new IndexerSetupResult<TResult>(matchingSetup, interaction, Behavior, GetIndexerValue,
			_indexerSetups.UpdateValue);
	}

	/// <summary>
	///     Sets the value of the indexer with the given parameters.
	/// </summary>
	/// <remarks>
	///     Returns a flag, indicating whether the base class implementation should be skipped.
	/// </remarks>
	public bool SetIndexer<TResult>(TResult value, params NamedParameterValue[] parameters)
	{
		IndexerSetterAccess interaction = new(Interactions.GetNextIndex(), parameters, value);
		((IMockInteractions)Interactions).RegisterInteraction(interaction);

		_indexerSetups.UpdateValue(parameters, value);
		IndexerSetup? matchingSetup = GetIndexerSetup(interaction);
		matchingSetup?.InvokeSetter(interaction, value, Behavior);
		return (matchingSetup as IInteractiveIndexerSetup)?.SkipBaseClass() ?? Behavior.SkipBaseClass;
	}

	/// <summary>
	///     Raises the event with <paramref name="eventName" /> and the given <paramref name="parameters" />.
	/// </summary>
	public void Raise(string eventName, params object?[] parameters)
	{
		foreach ((object? target, MethodInfo method) in GetEventHandlers(eventName))
		{
			try
			{
				method.Invoke(target, parameters);
			}
			catch (TargetInvocationException ex) when (ex.InnerException is not null)
			{
				ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
			}
		}
	}

	/// <summary>
	///     Associates the specified event <paramref name="method" /> on the <paramref name="target" /> with the event
	///     identified by the given <paramref name="name" />.
	/// </summary>
	public void AddEvent(string name, object? target, MethodInfo? method)
	{
		if (method is null)
		{
			throw new MockException("The method of an event subscription may not be null.");
		}

		((IMockInteractions)Interactions).RegisterInteraction(new EventSubscription(Interactions.GetNextIndex(), name,
			target, method));
		_eventHandlers.Add(target, method, name);
	}

	/// <summary>
	///     Removes the specified event <paramref name="method" /> on the <paramref name="target" /> from the event identified
	///     by the given <paramref name="name" />.
	/// </summary>
	public void RemoveEvent(string name, object? target, MethodInfo? method)
	{
		if (method is null)
		{
			throw new MockException("The method of an event unsubscription may not be null.");
		}

		((IMockInteractions)Interactions).RegisterInteraction(new EventUnsubscription(Interactions.GetNextIndex(), name,
			target, method));
		_eventHandlers.Remove(target, method, name);
	}

	/// <inheritdoc cref="object.ToString()" />
	[EditorBrowsable(EditorBrowsableState.Never)]
	public override string ToString()
	{
		StringBuilder sb = new();
		if (_methodSetups.Count > 0)
		{
			sb.Append(_methodSetups.Count).Append(_methodSetups.Count == 1 ? " method, " : " methods, ");
		}

		if (_propertySetups.Count > 0)
		{
			sb.Append(_propertySetups.Count).Append(_propertySetups.Count == 1 ? " property, " : " properties, ");
		}

		if (_eventHandlers.Count > 0)
		{
			sb.Append(_eventHandlers.Count).Append(_eventHandlers.Count == 1 ? " event, " : " events, ");
		}

		if (_indexerSetups.Count > 0)
		{
			sb.Append(_indexerSetups.Count).Append(_indexerSetups.Count == 1 ? " indexer, " : " indexers, ");
		}

		if (sb.Length == 0)
		{
			return "(none)";
		}

		sb.Length -= 2;
		return sb.ToString();
	}


	private IEnumerable<(object?, MethodInfo)> GetEventHandlers(string eventName)
	{
		foreach ((object? target, MethodInfo method, string name) in _eventHandlers.Enumerate())
		{
			if (name != eventName)
			{
				continue;
			}

			yield return (target, method);
		}
	}
}
