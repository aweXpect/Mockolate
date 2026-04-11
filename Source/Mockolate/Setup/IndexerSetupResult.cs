using System;
using System.Diagnostics;
using System.Linq;
using Mockolate.Exceptions;
using Mockolate.Interactions;
using Mockolate.Parameters;

namespace Mockolate.Setup;

/// <summary>
///     A result of an indexer setup.
/// </summary>
#if !DEBUG
[DebuggerNonUserCode]
#endif
public class IndexerSetupResult(IInteractiveIndexerSetup? setup, MockBehavior behavior)
{
	/// <summary>
	///     Gets the flag indicating if the base class implementation should be skipped.
	/// </summary>
	public bool SkipBaseClass
		=> setup?.SkipBaseClass() ?? behavior.SkipBaseClass;
}

/// <summary>
///     A result of an indexer setup with return type <typeparamref name="TResult" />.
/// </summary>
#if !DEBUG
[DebuggerNonUserCode]
#endif
public class IndexerSetupResult<TResult>(
	IInteractiveIndexerSetup? setup,
	IndexerGetterAccess indexerAccess,
	MockBehavior behavior,
	Func<IInteractiveIndexerSetup?, Func<TResult>, INamedParameterValue[], TResult> getIndexerValue,
	Action<INamedParameterValue[], TResult> setIndexerValue)
	: IndexerSetupResult(setup, behavior)
{
	private readonly MockBehavior _behavior = behavior;
	private readonly IInteractiveIndexerSetup? _setup = setup;

	/// <summary>
	///     The return value of the setup method.
	/// </summary>
	public TResult GetResult(TResult baseValue)
	{
		TResult value;
		if (_setup is IndexerSetup indexerSetup)
		{
			value = indexerSetup.InvokeGetter(indexerAccess, baseValue, _behavior);
			setIndexerValue(indexerAccess.Parameters, value);
		}
		else
		{
			value = baseValue;
		}

		return getIndexerValue(_setup, GetValue, indexerAccess.Parameters);
		
		[DebuggerNonUserCode]
		TResult GetValue()
		{
			return value;
		}
	}

	/// <summary>
	///     The return value of the setup method.
	/// </summary>
	public TResult GetResult(Func<TResult> defaultValueGenerator)
	{
		TResult value;
		if (_setup is IndexerSetup indexerSetup)
		{
			value = getIndexerValue(_setup, GetInitialValue, indexerAccess.Parameters);
			value = indexerSetup.InvokeGetter(indexerAccess, value, _behavior);
			setIndexerValue(indexerAccess.Parameters, value);
			return value;
		}

		if (_behavior.ThrowWhenNotSetup)
		{
			throw new MockNotSetupException(
				$"The indexer [{string.Join(", ", indexerAccess.Parameters.Select(p => p.ToString()))}] was accessed without prior setup.");
		}

		value = defaultValueGenerator();

		TResult result = getIndexerValue(_setup, GetValue, indexerAccess.Parameters);
		setIndexerValue(indexerAccess.Parameters, result);
		return result;

		[DebuggerNonUserCode]
		TResult GetInitialValue()
		{
			_setup.GetInitialValue(_behavior, defaultValueGenerator, indexerAccess.Parameters, out TResult v);
			return v;
		}
		[DebuggerNonUserCode]
		TResult GetValue()
		{
			return value;
		}
	}
}
