using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Mockolate.Events;
using Mockolate.Exceptions;
using Mockolate.Interactions;
using Mockolate.Internals;
using Mockolate.Setup;

namespace Mockolate;

/// <summary>
///     Provides a base class for creating and managing mock objects of a specified type, supporting setup, event raising,
///     and behavior configuration.
/// </summary>
[DebuggerDisplay("Setup: {Setup}, {_interactions.Count} interactions")]
public abstract class MockBase<T> : IMock
{
	private readonly MockBehavior _behavior;
	private readonly MockInteractions _interactions;
	private readonly MockSetup<T> _setup;

	/// <inheritdoc cref="MockBase{T}" />
	protected MockBase(MockBehavior behavior, string prefix)
	{
		_behavior = behavior;
		_interactions = new MockInteractions();
		_setup = new(this, prefix);
		Raise = new MockRaises<T>(_setup, _interactions);
	}

	/// <summary>
	///     Exposes the mocked subject instance of type <typeparamref name="T" />.
	/// </summary>
	public abstract T Subject { get; }

	/// <summary>
	///     Raise events on the mock for <typeparamref name="T" />.
	/// </summary>
	public MockRaises<T> Raise { get; }

	/// <summary>
	///     Sets up the mock for <typeparamref name="T" />.
	/// </summary>
	public IMockSetup<T> Setup => _setup;

	/// <summary>
	///     Implicitly converts the mock to the mocked object instance.
	/// </summary>
	/// <remarks>
	///     This does not work implicitly (but only with an explicit cast) for interfaces due to
	///     a limitation of the C# language.
	/// </remarks>
	public static implicit operator T(MockBase<T> mock)
	{
		return mock.Subject;
	}

	/// <summary>
	///     Attempts to cast the specified value to the type parameter <typeparamref name="TValue" />,
	///     returning a value that indicates whether the cast was successful.
	/// </summary>
	protected bool TryCast<TValue>([NotNullWhen(false)] object? value, out TValue result)
	{
		if (value is TValue typedValue)
		{
			result = typedValue;
			return true;
		}

		result = _behavior.DefaultValue.Generate<TValue>();
		return value is null;
	}

	#region IMock

	/// <summary>
	///     Gets the behavior settings used by this mock instance.
	/// </summary>
	MockBehavior IMock.Behavior => _behavior;

	/// <inheritdoc cref="IMock.Interactions" />
	MockInteractions IMock.Interactions
		=> _interactions;

	/// <inheritdoc cref="IMock.Setup" />
	IMockSetup IMock.Setup
		=> _setup;

	/// <inheritdoc cref="IMock.Raise" />
	IMockRaises IMock.Raise
		=> Raise;

	/// <inheritdoc cref="IMock.Execute{TResult}(string, object?[])" />
	MethodSetupResult<TResult> IMock.Execute<TResult>(string methodName, params object?[]? parameters)
	{
		MockInteractions interactions = ((IMock)this).Interactions;
		parameters ??= [null,];
		MethodInvocation methodInvocation =
			((IMockInteractions)interactions).RegisterInteraction(new MethodInvocation(interactions.GetNextIndex(),
				methodName, parameters));

		MethodSetup? matchingSetup = _setup.GetMethodSetup(methodInvocation);
		if (matchingSetup is null)
		{
			if (_behavior.ThrowWhenNotSetup)
			{
				throw new MockNotSetupException(
					$"The method '{methodName}({string.Join(", ", parameters.Select(x => x?.GetType().FormatType() ?? "<null>"))})' was invoked without prior setup.");
			}

			return new MethodSetupResult<TResult>(null, _behavior,
				_behavior.DefaultValue.Generate<TResult>());
		}

		return new MethodSetupResult<TResult>(matchingSetup, _behavior,
			matchingSetup.Invoke<TResult>(methodInvocation, _behavior));
	}

	/// <inheritdoc cref="IMock.Execute(string, object?[])" />
	MethodSetupResult IMock.Execute(string methodName, params object?[]? parameters)
	{
		MockInteractions interactions = ((IMock)this).Interactions;
		parameters ??= [null,];
		MethodInvocation methodInvocation =
			((IMockInteractions)interactions).RegisterInteraction(new MethodInvocation(interactions.GetNextIndex(),
				methodName, parameters));

		MethodSetup? matchingSetup = _setup.GetMethodSetup(methodInvocation);
		if (matchingSetup is null && _behavior.ThrowWhenNotSetup)
		{
			throw new MockNotSetupException(
				$"The method '{methodName}({string.Join(", ", parameters.Select(x => x?.GetType().FormatType() ?? "<null>"))})' was invoked without prior setup.");
		}

		matchingSetup?.Invoke(methodInvocation, _behavior);
		return new MethodSetupResult(matchingSetup, _behavior);
	}

	/// <inheritdoc cref="IMock.Get{TResult}(string, Func{TResult})" />
	TResult IMock.Get<TResult>(string propertyName, Func<TResult>? defaultValueGenerator)
	{
		MockInteractions? interactions = ((IMock)this).Interactions;
		IInteraction interaction =
			((IMockInteractions)interactions).RegisterInteraction(new PropertyGetterAccess(interactions.GetNextIndex(),
				propertyName));
		PropertySetup matchingSetup = _setup.GetPropertySetup(propertyName,
			defaultValueGenerator is null ? null : () => defaultValueGenerator());
		return matchingSetup.InvokeGetter<TResult>(interaction, _behavior);
	}

	/// <inheritdoc cref="IMock.Set(string, object?)" />
	void IMock.Set(string propertyName, object? value)
	{
		MockInteractions interactions = ((IMock)this).Interactions;
		IInteraction interaction =
			((IMockInteractions)interactions).RegisterInteraction(new PropertySetterAccess(interactions.GetNextIndex(),
				propertyName, value));
		PropertySetup matchingSetup = _setup.GetPropertySetup(propertyName, null);
		matchingSetup.InvokeSetter(interaction, value, _behavior);
	}

	/// <inheritdoc cref="IMock.GetIndexer{TResult}(Func{TResult}, object?[])" />
	TResult IMock.GetIndexer<TResult>(Func<TResult>? defaultValueGenerator, params object?[] parameters)
	{
		MockInteractions? interactions = ((IMock)this).Interactions;
		IndexerGetterAccess interaction = new(interactions.GetNextIndex(), parameters);
		((IMockInteractions)interactions).RegisterInteraction(interaction);

		IndexerSetup? matchingSetup = _setup.GetIndexerSetup(interaction);
		TResult initialValue = _setup.GetIndexerValue(matchingSetup, defaultValueGenerator, parameters);
		if (matchingSetup is not null)
		{
			TResult? value = matchingSetup.InvokeGetter(interaction, initialValue, _behavior);
			if (!Equals(initialValue, value))
			{
				((IMockSetup)Setup).SetIndexerValue(parameters, value);
			}

			return value;
		}

		return initialValue;
	}

	/// <inheritdoc cref="IMock.SetIndexer{TResult}(TResult, object?[])" />
	void IMock.SetIndexer<TResult>(TResult value, params object?[] parameters)
	{
		MockInteractions? interactions = ((IMock)this).Interactions;
		IndexerSetterAccess interaction = new(interactions.GetNextIndex(), parameters, value);
		((IMockInteractions)interactions).RegisterInteraction(interaction);

		((IMockSetup)Setup).SetIndexerValue(parameters, value);
		IndexerSetup? matchingSetup = _setup.GetIndexerSetup(interaction);
		matchingSetup?.InvokeSetter(interaction, value, _behavior);
	}

	#endregion IMock
}
#pragma warning restore S2436 // Types and methods should not have too many generic parameters
