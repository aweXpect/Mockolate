using System;
using System.Linq;
using Mockolate.Exceptions;
using Mockolate.Interactions;
using Mockolate.Internals;
using Mockolate.Setup;

namespace Mockolate.V2;

public partial class Mock<T>
{
	/// <inheritdoc cref="IInteractiveMock.Execute{TResult}(string, object?[])" />
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

	/// <inheritdoc cref="IInteractiveMock.Execute(string, object?[])" />
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

	/// <inheritdoc cref="IInteractiveMock.Get{TResult}(string, Func{TResult})" />
	public TResult Get<TResult>(string propertyName, Func<TResult>? defaultValueGenerator)
	{
		IInteraction interaction =
			((IMockInteractions)Interactions).RegisterInteraction(new PropertyGetterAccess(Interactions.GetNextIndex(),
				propertyName));
		PropertySetup matchingSetup = GetPropertySetup(propertyName,
			defaultValueGenerator is null ? null : () => defaultValueGenerator());
		return matchingSetup.InvokeGetter<TResult>(interaction, Behavior);
	}

	/// <inheritdoc cref="IInteractiveMock.Set(string, object?)" />
	public void Set(string propertyName, object? value)
	{
		IInteraction interaction =
			((IMockInteractions)Interactions).RegisterInteraction(new PropertySetterAccess(Interactions.GetNextIndex(),
				propertyName, value));
		PropertySetup matchingSetup = GetPropertySetup(propertyName, null);
		matchingSetup.InvokeSetter(interaction, value, Behavior);
	}

	/// <inheritdoc cref="IInteractiveMock.GetIndexer{TResult}(Func{TResult}, object?[])" />
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
				SetIndexerValue(parameters, value);
			}

			return value;
		}

		return initialValue;
	}

	/// <inheritdoc cref="IInteractiveMock.SetIndexer{TResult}(TResult, object?[])" />
	public void SetIndexer<TResult>(TResult value, params object?[] parameters)
	{
		MockInteractions? interactions = ((IInteractiveMock)this).Interactions;
		IndexerSetterAccess interaction = new(interactions.GetNextIndex(), parameters, value);
		((IMockInteractions)interactions).RegisterInteraction(interaction);

		SetIndexerValue(parameters, value);
		IndexerSetup? matchingSetup = GetIndexerSetup(interaction);
		matchingSetup?.InvokeSetter(interaction, value, Behavior);
	}
}
