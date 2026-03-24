using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Mockolate.Exceptions;
using Mockolate.Interactions;
using Mockolate.Internals;
using Mockolate.Parameters;
using Mockolate.Setup;

namespace Mockolate;

public partial class MockRegistry
{
	/// <summary>
	///     Gets the collection of interactions recorded by the mock object.
	/// </summary>
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
					$"The method '{methodName}({string.Join(", ", parameters.Select(TypeStringOrNullSelector))})' was invoked without prior setup.");
			}

			return new MethodSetupResult<TResult>(null, Behavior,
				defaultValue(parameters.Select(ValueSelector).ToArray()));
		}

		return new MethodSetupResult<TResult>(matchingSetup, Behavior,
			matchingSetup.Invoke(methodInvocation, Behavior,
				() => defaultValue(parameters.Select(ValueSelector).ToArray())));

		[DebuggerNonUserCode]
		object? ValueSelector(NamedParameterValue x)
		{
			return x.Value;
		}

		[DebuggerNonUserCode]
		string TypeStringOrNullSelector(NamedParameterValue x)
		{
			return x.Value?.GetType().FormatType() ?? "<null>";
		}
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
		IInteractivePropertySetup matchingSetup = GetPropertySetup(propertyName, DefaultValueGenerator);
		return matchingSetup.InvokeGetter(interaction, Behavior, defaultValueGenerator);

		[DebuggerNonUserCode]
		object? DefaultValueGenerator(bool skipBase)
		{
			return skipBase || baseValueAccessor is null
				? defaultValueGenerator()
				: baseValueAccessor.Invoke();
		}
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
		IInteractivePropertySetup matchingSetup = GetPropertySetup(propertyName, NoDefaultValue);
		matchingSetup.InvokeSetter(interaction, value, Behavior);
		return matchingSetup.SkipBaseClass() ?? Behavior.SkipBaseClass;

		[DebuggerNonUserCode]
		static object? NoDefaultValue(bool _)
		{
			return null;
		}
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
			Setup.Indexers.UpdateValue);
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

		Setup.Indexers.UpdateValue(parameters, value);
		IndexerSetup? matchingSetup = GetIndexerSetup(interaction);
		matchingSetup?.InvokeSetter(interaction, value, Behavior);
		return (matchingSetup as IInteractiveIndexerSetup)?.SkipBaseClass() ?? Behavior.SkipBaseClass;
	}

	/// <summary>
	///     Raises the event with <paramref name="eventName" /> and the given <paramref name="parameters" />.
	/// </summary>
	public void Raise(string eventName, params object?[] parameters)
	{
		foreach ((object? target, MethodInfo method) in GetEventHandlers())
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

		IEnumerable<(object?, MethodInfo)> GetEventHandlers()
		{
			foreach ((object? target, MethodInfo method, string name) in Setup.Events.Enumerate())
			{
				if (name != eventName)
				{
					continue;
				}

				yield return (target, method);
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
		Setup.Events.Add(target, method, name);
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
		Setup.Events.Remove(target, method, name);
	}
}
