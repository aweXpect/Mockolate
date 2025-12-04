using System;
using System.Linq;
using Mockolate.Exceptions;
using Mockolate.Interactions;

namespace Mockolate.Setup;

/// <summary>
///     A result of a method setup invocation.
/// </summary>
public class IndexerSetupResult(IInteractiveIndexerSetup? setup, MockBehavior behavior)
{
	/// <summary>
	///     Gets the flag indicating if the base class implementation should be called, and its return values used as default
	///     values.
	/// </summary>
	public bool CallBaseClass
		=> setup?.CallBaseClass() ?? behavior.CallBaseClass;
}

/// <summary>
///     A result of a method setup invocation with return type <typeparamref name="TResult" />.
/// </summary>
public class IndexerSetupResult<TResult>(
	IInteractiveIndexerSetup? setup,
	IndexerGetterAccess indexerAccess,
	MockBehavior behavior,
	Func<IInteractiveIndexerSetup?, Func<TResult>, object?[], TResult> getIndexerValue,
	Action<object?[], TResult> setIndexerValue)
	: IndexerSetupResult(setup, behavior)
{
	private readonly MockBehavior _behavior = behavior;
	private readonly IInteractiveIndexerSetup? _setup = setup;

	/// <summary>
	///     The return value of the setup method.
	/// </summary>
	public TResult GetResult(TResult baseValue, Func<TResult> defaultValueGenerator)
	{
		TResult value;
		if (_setup is IndexerSetup indexerSetup)
		{
			value = indexerSetup.InvokeGetter(indexerAccess, baseValue ?? defaultValueGenerator(),
				_behavior);
			setIndexerValue(indexerAccess.Parameters, value);
		}
		else if (_behavior.ThrowWhenNotSetup)
		{
			throw new MockNotSetupException(
				$"The indexer [{string.Join(", ", indexerAccess.Parameters.Select(p => p?.ToString() ?? "null"))}] was accessed without prior setup.");
		}
		else
		{
			value = baseValue ?? defaultValueGenerator();
		}

		return getIndexerValue(_setup, () => value, indexerAccess.Parameters);
	}

	/// <summary>
	///     The return value of the setup method.
	/// </summary>
	public TResult GetResult(Func<TResult> defaultValueGenerator)
	{
		TResult value;
		if (_setup is IndexerSetup indexerSetup)
		{
			if (_setup.TryGetInitialValue(_behavior, defaultValueGenerator, indexerAccess.Parameters, out value))
			{
				value = indexerSetup.InvokeGetter(indexerAccess, value, _behavior);
			}
			else
			{
				value = indexerSetup.InvokeGetter(indexerAccess, defaultValueGenerator(), _behavior);
			}

			if (_setup.HasReturnCalls())
			{
				setIndexerValue(indexerAccess.Parameters, value);
				return value;
			}
		}
		else if (_behavior.ThrowWhenNotSetup)
		{
			throw new MockNotSetupException(
				$"The indexer [{string.Join(", ", indexerAccess.Parameters.Select(p => p?.ToString() ?? "null"))}] was accessed without prior setup.");
		}
		else
		{
			value = defaultValueGenerator();
		}

		TResult result = getIndexerValue(_setup, () => value, indexerAccess.Parameters);
		setIndexerValue(indexerAccess.Parameters, result);
		return result;
	}
}
