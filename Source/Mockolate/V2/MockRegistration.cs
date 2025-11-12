using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Mockolate.Exceptions;
using Mockolate.Interactions;
using Mockolate.Internals;
using Mockolate.Setup;

namespace Mockolate.V2;

[DebuggerDisplay("{ToString()}")]
public partial class MockRegistration : IMockRegistration, IMock
{
	/// <inheritdoc cref="MockBase{T}" />
	public MockRegistration(MockBehavior behavior, string prefix)
	{
		Behavior = behavior;
		Prefix = prefix;
		Interactions = new MockInteractions();
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
	public MockInteractions Interactions { get; }
	
	/// <inheritdoc cref="object.ToString()" />
	[EditorBrowsable(EditorBrowsableState.Never)]
	public override string ToString()
	{
		StringBuilder? sb = new();
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

		if (sb.Length < 2)
		{
			return "(none)";
		}

		sb.Length -= 2;
		return sb.ToString();
	}
	
	/// <inheritdoc cref="IMockRegistration.Execute{TResult}(string, object?[])" />
	public MethodSetupResult<TResult> Execute<TResult>(string methodName, params object?[]? parameters)
	{
		parameters ??= [null,];
		MethodInvocation methodInvocation =
			((IMockInteractions)Interactions).RegisterInteraction(new MethodInvocation(Interactions.GetNextIndex(),
				methodName, parameters));

		MethodSetup? matchingSetup = GetMethodSetup(methodInvocation);
		if (matchingSetup is null)
		{
			if (Behavior.ThrowWhenNotSetup)
			{
				throw new MockNotSetupException(
					$"The method '{methodName}({string.Join(", ", parameters.Select(x => x?.GetType()?.FormatType() ?? "<null>"))})' was invoked without prior setup.");
			}

			return new MethodSetupResult<TResult>(null, Behavior,
				Behavior.DefaultValue.Generate<TResult>());
		}

		return new MethodSetupResult<TResult>(matchingSetup, Behavior,
			matchingSetup.Invoke<TResult>(methodInvocation, Behavior));
	}

	/// <inheritdoc cref="IMockRegistration.Execute(string, object?[])" />
	public MethodSetupResult Execute(string methodName, params object?[]? parameters)
	{
		parameters ??= [null,];
		MethodInvocation methodInvocation =
			((IMockInteractions)Interactions).RegisterInteraction(new MethodInvocation(Interactions.GetNextIndex(),
				methodName, parameters));

		MethodSetup? matchingSetup = GetMethodSetup(methodInvocation);
		if (matchingSetup is null && Behavior.ThrowWhenNotSetup)
		{
			throw new MockNotSetupException(
				$"The method '{methodName}({string.Join(", ", parameters.Select(x => x?.GetType().FormatType() ?? "<null>"))})' was invoked without prior setup.");
		}

		matchingSetup?.Invoke(methodInvocation, Behavior);
		return new MethodSetupResult(matchingSetup, Behavior);
	}

	/// <inheritdoc cref="IMockRegistration.Get{TResult}(string, Func{TResult})" />
	public TResult Get<TResult>(string propertyName, Func<TResult>? defaultValueGenerator)
	{
		IInteraction interaction =
			((IMockInteractions)Interactions).RegisterInteraction(new PropertyGetterAccess(Interactions.GetNextIndex(),
				propertyName));
		PropertySetup matchingSetup = GetPropertySetup(propertyName,
			defaultValueGenerator is null ? null : () => defaultValueGenerator());
		return matchingSetup.InvokeGetter<TResult>(interaction, Behavior);
	}

	/// <inheritdoc cref="IMockRegistration.Set(string, object?)" />
	public void Set(string propertyName, object? value)
	{
		IInteraction interaction =
			((IMockInteractions)Interactions).RegisterInteraction(new PropertySetterAccess(Interactions.GetNextIndex(),
				propertyName, value));
		PropertySetup matchingSetup = GetPropertySetup(propertyName, null);
		matchingSetup.InvokeSetter(interaction, value, Behavior);
	}

	/// <inheritdoc cref="IMockRegistration.GetIndexer{TResult}(Func{TResult}, object?[])" />
	public TResult GetIndexer<TResult>(Func<TResult>? defaultValueGenerator, params object?[] parameters)
	{
		IndexerGetterAccess interaction = new(Interactions.GetNextIndex(), parameters);
		((IMockInteractions)Interactions).RegisterInteraction(interaction);

		IndexerSetup? matchingSetup = GetIndexerSetup(interaction);
		TResult initialValue = GetIndexerValue(matchingSetup, defaultValueGenerator, parameters);
		if (matchingSetup is not null)
		{
			TResult? value = matchingSetup.InvokeGetter(interaction, initialValue, Behavior);
			if (!Equals(initialValue, value))
			{
				SetupIndexerValue(parameters, value);
			}

			return value;
		}

		return initialValue;
	}

	/// <inheritdoc cref="IMockRegistration.SetIndexer{TResult}(TResult, object?[])" />
	public void SetIndexer<TResult>(TResult value, params object?[] parameters)
	{
		MockInteractions? interactions = ((IMockRegistration)this).Interactions;
		IndexerSetterAccess interaction = new(interactions.GetNextIndex(), parameters, value);
		((IMockInteractions)interactions).RegisterInteraction(interaction);

		SetupIndexerValue(parameters, value);
		IndexerSetup? matchingSetup = GetIndexerSetup(interaction);
		matchingSetup?.InvokeSetter(interaction, value, Behavior);
	}
	
	/// <inheritdoc cref="IMockRegistration.Raise" />
	public void Raise(string eventName, params object?[] parameters)
	{
		foreach ((object? target, MethodInfo? method) in GetEventHandlers(eventName))
		{
			method.Invoke(target, parameters);
		}
	}

	/// <inheritdoc cref="IMockRegistration.AddEvent(string, object?, MethodInfo?)" />
	public void AddEvent(string name, object? target, MethodInfo? method)
	{
		if (method is null)
		{
			throw new MockException("The method of an event subscription may not be null.");
		}

		((IMockInteractions)Interactions).RegisterInteraction(new EventSubscription(Interactions.GetNextIndex(), name,
			target, method));
		AddEvent(name, target, method);
	}

	/// <inheritdoc cref="IMockRegistration.RemoveEvent(string, object?, MethodInfo?)" />
	public void RemoveEvent(string name, object? target, MethodInfo? method)
	{
		if (method is null)
		{
			throw new MockException("The method of an event unsubscription may not be null.");
		}

		((IMockInteractions)Interactions).RegisterInteraction(new EventUnsubscription(Interactions.GetNextIndex(), name,
			target, method));
		RemoveEvent(name, target, method);
	}


	private IEnumerable<(object?, MethodInfo)> GetEventHandlers(string eventName)
	{
		foreach ((object? target, MethodInfo? method, string? name) in _eventHandlers.Enumerate())
		{
			if (name != eventName)
			{
				continue;
			}

			yield return (target, method);
		}
	}
}
