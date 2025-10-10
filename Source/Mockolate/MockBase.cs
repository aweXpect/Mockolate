using System.Diagnostics;
using System.Linq;
using Mockolate.Checks;
using Mockolate.Events;
using Mockolate.Exceptions;
using Mockolate.Interactions;
using Mockolate.Setup;

namespace Mockolate;

/// <summary>
///     Provides a base class for creating and managing mock objects of a specified type, supporting setup, event raising,
///     and behavior configuration.
/// </summary>
[DebuggerDisplay("Setup: {Setup}, {_interactions.Count} interactions")]
public abstract class MockBase<T> : IMock
{
	private readonly MockInteractions _interactions;
	private readonly MockBehavior _behavior;

	/// <inheritdoc cref="MockBase{T}" />
	protected MockBase(MockBehavior behavior)
	{
		_behavior = behavior;
		_interactions = new();
		Setup = new MockSetups<T>(this);
		Raise = new MockRaises<T>(Setup, _interactions);
		Check = new MockCheck(_interactions);
	}

	/// <summary>
	///     Additional checks on the mocked instance.
	/// </summary>
	public MockCheck Check { get; }

	/// <summary>
	///     Exposes the mocked object instance of type <typeparamref name="T" />.
	/// </summary>
	public abstract T Object { get; }

	/// <summary>
	///     Raise events on the mock for <typeparamref name="T" />.
	/// </summary>
	public MockRaises<T> Raise { get; }

	/// <summary>
	///     Sets up the mock for <typeparamref name="T" />.
	/// </summary>
	public MockSetups<T> Setup { get; }

	/// <summary>
	///     Implicitly converts the mock to the mocked object instance.
	/// </summary>
	/// <remarks>
	///     This does not work implicitly (but only with an explicit cast) for interfaces due to
	///     a limitation of the C# language.
	/// </remarks>
	public static implicit operator T(MockBase<T> mock) => mock.Object;

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
		=> Setup;

	/// <inheritdoc cref="IMock.Raise" />
	IMockRaises IMock.Raise
		=> Raise;

	/// <inheritdoc cref="IMock.Execute{TResult}(string, object?[])" />
	MethodSetupResult<TResult> IMock.Execute<TResult>(string methodName, params object?[]? parameters)
	{
		MockInteractions interactions = ((IMock)this).Interactions;
		parameters ??= [null,];
		IInteraction interaction =
			((IMockInteractions)interactions).RegisterInteraction(new MethodInvocation(interactions.GetNextIndex(), methodName, parameters));

		MethodSetup? matchingSetup = Setup.GetMethodSetup(interaction);
		if (matchingSetup is null)
		{
			if (_behavior.ThrowWhenNotSetup)
			{
				throw new MockNotSetupException(
					$"The method '{methodName}({string.Join(", ", parameters.Select(x => x?.GetType().ToString() ?? "<null>"))})' was invoked without prior setup.");
			}

			return new MethodSetupResult<TResult>(null, _behavior,
				_behavior.DefaultValueGenerator.Generate<TResult>());
		}

		return new MethodSetupResult<TResult>(matchingSetup, _behavior,
			matchingSetup.Invoke<TResult>(interaction, _behavior));
	}

	/// <inheritdoc cref="IMock.Execute(string, object?[])" />
	MethodSetupResult IMock.Execute(string methodName, params object?[]? parameters)
	{
		MockInteractions interactions = ((IMock)this).Interactions;
		parameters ??= [null,];
		IInteraction interaction =
			((IMockInteractions)interactions).RegisterInteraction(new MethodInvocation(interactions.GetNextIndex(), methodName, parameters));

		MethodSetup? matchingSetup = Setup.GetMethodSetup(interaction);
		if (matchingSetup is null && _behavior.ThrowWhenNotSetup)
		{
			throw new MockNotSetupException(
				$"The method '{methodName}({string.Join(", ", parameters.Select(x => x?.GetType().ToString() ?? "<null>"))})' was invoked without prior setup.");
		}

		matchingSetup?.Invoke(interaction, _behavior);
		return new MethodSetupResult(matchingSetup, _behavior);
	}

	/// <inheritdoc cref="IMock.Get{TResult}(string)" />
	TResult IMock.Get<TResult>(string propertyName)
	{
		MockInteractions? interactions = ((IMock)this).Interactions;
		IInteraction interaction =
			((IMockInteractions)interactions).RegisterInteraction(new PropertyGetterAccess(interactions.GetNextIndex(), propertyName));
		PropertySetup matchingSetup = Setup.GetPropertySetup(propertyName);
		return matchingSetup.InvokeGetter<TResult>(interaction);
	}

	/// <inheritdoc cref="IMock.Set(string, object?)" />
	void IMock.Set(string propertyName, object? value)
	{
		MockInteractions interactions = ((IMock)this).Interactions;
		IInteraction interaction =
			((IMockInteractions)interactions).RegisterInteraction(new PropertySetterAccess(interactions.GetNextIndex(), propertyName, value));
		PropertySetup matchingSetup = Setup.GetPropertySetup(propertyName);
		matchingSetup.InvokeSetter(interaction, value);
	}

	/// <inheritdoc cref="IMock.GetIndexer{TResult}(object?[])" />
	TResult IMock.GetIndexer<TResult>(params object?[] parameters)
	{
		MockInteractions? interactions = ((IMock)this).Interactions;
		IndexerGetterAccess interaction = new IndexerGetterAccess(interactions.GetNextIndex(), parameters);
		((IMockInteractions)interactions).RegisterInteraction(interaction);

		IndexerSetup? matchingSetup = Setup.GetIndexerSetup(interaction);
		TResult value = Setup.GetIndexerValue<TResult>(matchingSetup, parameters);
		matchingSetup?.InvokeGetter(interaction, value, _behavior);
		return value;
	}

	/// <inheritdoc cref="IMock.SetIndexer{TResult}(TResult, object?[])" />
	void IMock.SetIndexer<TResult>(TResult value, params object?[] parameters)
	{
		MockInteractions? interactions = ((IMock)this).Interactions;
		IndexerSetterAccess interaction = new IndexerSetterAccess(interactions.GetNextIndex(), parameters, value);
		((IMockInteractions)interactions).RegisterInteraction(interaction);

		((IMockSetup)Setup).SetIndexerValue(parameters, value);
		IndexerSetup? matchingSetup = Setup.GetIndexerSetup(interaction);
		matchingSetup?.InvokeSetter(interaction, value, _behavior);
	}

	#endregion IMock

	/// <summary>
	///     Attempts to cast the specified value to the type parameter <typeparamref name="TValue"/>,
	///     returning a value that indicates whether the cast was successful.
	/// </summary>
	protected bool TryCast<TValue>(object? value, out TValue result)
	{
		if (value is TValue typedValue)
		{
			result = typedValue;
			return true;
		}

		result = _behavior.DefaultValueGenerator.Generate<TValue>();
		return value is null;
	}
}
#pragma warning restore S2436 // Types and methods should not have too many generic parameters
