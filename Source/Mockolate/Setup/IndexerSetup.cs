using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Mockolate.Interactions;

namespace Mockolate.Setup;

/// <summary>
///     Base class for indexer setups.
/// </summary>
public abstract class IndexerSetup : IIndexerSetup
{
	/// <inheritdoc cref="IIndexerSetup.Matches(IInteraction)" />
	bool IIndexerSetup.Matches(IInteraction invocation)
		=> invocation is IndexerGetterAccess getterAccess && IsMatch(getterAccess.Parameters) ||
		   invocation is IndexerSetterAccess setterAccess && IsMatch(setterAccess.Parameters);

	internal void InvokeGetter<TValue>(IndexerGetterAccess getterAccess, TValue value, MockBehavior behavior)
	{
		ExecuteGetterCallback(getterAccess, behavior);
	}

	internal void InvokeSetter<TValue>(IndexerSetterAccess setterAccess, TValue value, MockBehavior behavior)
	{
		ExecuteSetterCallback(setterAccess, value, behavior);
	}

	/// <summary>
	///     Execute a potentially registered getter callback.
	/// </summary>
	protected abstract void ExecuteGetterCallback(IndexerGetterAccess indexerGetterAccess, MockBehavior behavior);

	/// <summary>
	///     Execute a potentially registered setter callback.
	/// </summary>
	protected abstract void ExecuteSetterCallback<T>(IndexerSetterAccess indexerSetterAccess, T value, MockBehavior behavior);

	/// <summary>
	///     Checks if the <paramref name="parameters" /> match the setup.
	/// </summary>
	protected abstract bool IsMatch(object?[] parameters);

	/// <summary>
	///     Attempts to cast the specified value to the type parameter <typeparamref name="T"/>,
	///     returning a value that indicates whether the cast was successful.
	/// </summary>
	/// <remarks>
	///     If value is not of type <typeparamref name="T" /> and is not <see langword="null" />,
	///     result is set to the default value for type <typeparamref name="T" /> as provided
	///     by the <paramref name="behavior" />.
	/// </remarks>
	protected static bool TryCast<T>(object? value, out T result, MockBehavior behavior)
	{
		if (value is T typedValue)
		{
			result = typedValue;
			return true;
		}

		result = behavior.DefaultValueGenerator.Generate<T>();
		return value is null;
	}

	/// <summary>
	///     Determines whether each value in the specified array matches the corresponding parameter according to the
	///     parameter's matching criteria.
	/// </summary>
	/// <remarks>
	///     The method returns false if the lengths of the parameters and values arrays do not match.
	///     Each value is compared to its corresponding parameter using the parameter's matching logic.
	/// </remarks>
	protected static bool Matches(With.Parameter[] parameters, object?[] values)
	{
		if (parameters.Length != values.Length)
		{
			return false;
		}

		for (int i = 0; i < parameters.Length; i++)
		{
			if (!parameters[i].Matches(values[i]))
			{
				return false;
			}
		}

		return true;
	}

	/// <inheritdoc cref="IIndexerSetup.TryGetInitialValue{TValue}(MockBehavior, object?[], out TValue)" />
	bool IIndexerSetup.TryGetInitialValue<TValue>(MockBehavior behavior, object?[] parameters, [NotNullWhen(true)] out TValue value)
		=> TryGetInitialValue(behavior, parameters, out value);

	/// <summary>
	///     Attempts to retrieve the initial <paramref name="value"/> for the <paramref name="parameters"/>, if an initialization is set up.
	/// </summary>
	protected abstract bool TryGetInitialValue<T>(MockBehavior behavior, object?[] parameters, [NotNullWhen(true)] out T value);
}

/// <summary>
///     Sets up a <typeparamref name="TValue"/> indexer for <typeparamref name="T1"/>.
/// </summary>
public class IndexerSetup<TValue, T1>(With.Parameter<T1> match1) : IndexerSetup
{
	private readonly List<Action<T1>> _getterCallbacks = [];
	private readonly List<Action<TValue, T1>> _setterCallbacks = [];
	private Func<T1, TValue>? _initialization;

	/// <summary>
	///     Initializes the indexer with the given <paramref name="value" />.
	/// </summary>
	public IndexerSetup<TValue, T1> InitializeWith(TValue value)
	{
		// TODO: Throw when already initialized!
		_initialization = _ => value;
		return this;
	}

	/// <summary>
	///     Initializes the indexer according to the given <paramref name="valueGenerator" />.
	/// </summary>
	public IndexerSetup<TValue, T1> InitializeWith(Func<T1, TValue> valueGenerator)
	{
		// TODO: Throw when already initialized!
		_initialization = valueGenerator;
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's getter is accessed.
	/// </summary>
	public IndexerSetup<TValue, T1> OnGet(Action callback)
	{
		_getterCallbacks.Add(_ => callback());
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's getter is accessed.
	/// </summary>
	public IndexerSetup<TValue, T1> OnGet(Action<T1> callback)
	{
		_getterCallbacks.Add(callback);
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	public IndexerSetup<TValue, T1> OnSet(Action callback)
	{
		_setterCallbacks.Add((_, _) => callback());
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	public IndexerSetup<TValue, T1> OnSet(Action<TValue> callback)
	{
		_setterCallbacks.Add((v, _) => callback(v));
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	public IndexerSetup<TValue, T1> OnSet(Action<TValue, T1> callback)
	{
		_setterCallbacks.Add(callback);
		return this;
	}

	/// <inheritdoc cref="ExecuteGetterCallback(IndexerGetterAccess, MockBehavior)" />
	protected override void ExecuteGetterCallback(IndexerGetterAccess indexerGetterAccess, MockBehavior behavior)
	{
		if (indexerGetterAccess.Parameters.Length == 1 &&
			TryCast(indexerGetterAccess.Parameters[0], out T1 p1, behavior))
		{
			_getterCallbacks.ForEach(callback => callback.Invoke(p1));
		}
	}

	/// <inheritdoc cref="ExecuteSetterCallback{TValue}(IndexerSetterAccess, TValue, MockBehavior)" />
	protected override void ExecuteSetterCallback<T>(IndexerSetterAccess indexerSetterAccess, T value, MockBehavior behavior)
	{
		if (value is TValue resultValue &&
			indexerSetterAccess.Parameters.Length == 1 &&
			TryCast(indexerSetterAccess.Parameters[0], out T1 p1, behavior))
		{
			_setterCallbacks.ForEach(callback => callback.Invoke(resultValue, p1));
		}
	}

	/// <inheritdoc cref="IsMatch(object?[])" />
	protected override bool IsMatch(object?[] parameters)
		=> Matches([match1], parameters);

	/// <inheritdoc cref="IndexerSetup.TryGetInitialValue{T}(MockBehavior, object?[], out T)" />
	protected override bool TryGetInitialValue<T>(MockBehavior behavior, object?[] parameters, [NotNullWhen(true)] out T value)
	{
		if (_initialization is not null &&
			parameters.Length == 1 &&
			TryCast(parameters[0], out T1 p1, behavior) &&
			_initialization.Invoke(p1) is T initialValue)
		{
			value = initialValue;
			return true;
		}

		value = default!;
		return false;
	}
}

/// <summary>
///     Sets up a <typeparamref name="TValue"/> indexer for <typeparamref name="T1"/> and <typeparamref name="T2"/>.
/// </summary>
public class IndexerSetup<TValue, T1, T2>(With.Parameter<T1> match1, With.Parameter<T2> match2) : IndexerSetup
{
	private readonly List<Action<T1, T2>> _getterCallbacks = [];
	private readonly List<Action<TValue, T1, T2>> _setterCallbacks = [];
	private Func<T1, T2, TValue>? _initialization;

	/// <summary>
	///     Initializes the indexer with the given <paramref name="value" />.
	/// </summary>
	public IndexerSetup<TValue, T1, T2> InitializeWith(TValue value)
	{
		// TODO: Throw when already initialized!
		_initialization = (_, _) => value;
		return this;
	}

	/// <summary>
	///     Initializes the indexer according to the given <paramref name="valueGenerator" />.
	/// </summary>
	public IndexerSetup<TValue, T1, T2> InitializeWith(Func<T1, T2, TValue> valueGenerator)
	{
		// TODO: Throw when already initialized!
		_initialization = valueGenerator;
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's getter is accessed.
	/// </summary>
	public IndexerSetup<TValue, T1, T2> OnGet(Action callback)
	{
		_getterCallbacks.Add((_, _) => callback());
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's getter is accessed.
	/// </summary>
	public IndexerSetup<TValue, T1, T2> OnGet(Action<T1, T2> callback)
	{
		_getterCallbacks.Add(callback);
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	public IndexerSetup<TValue, T1, T2> OnSet(Action callback)
	{
		_setterCallbacks.Add((_, _, _) => callback());
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	public IndexerSetup<TValue, T1, T2> OnSet(Action<TValue> callback)
	{
		_setterCallbacks.Add((v, _, _) => callback(v));
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	public IndexerSetup<TValue, T1, T2> OnSet(Action<TValue, T1, T2> callback)
	{
		_setterCallbacks.Add(callback);
		return this;
	}

	/// <inheritdoc cref="ExecuteGetterCallback(IndexerGetterAccess, MockBehavior)" />
	protected override void ExecuteGetterCallback(IndexerGetterAccess indexerGetterAccess, MockBehavior behavior)
	{
		if (indexerGetterAccess.Parameters.Length == 2 &&
			TryCast(indexerGetterAccess.Parameters[0], out T1 p1, behavior) &&
			TryCast(indexerGetterAccess.Parameters[1], out T2 p2, behavior))
		{
			_getterCallbacks.ForEach(callback => callback.Invoke(p1, p2));
		}
	}

	/// <inheritdoc cref="ExecuteSetterCallback{TValue}(IndexerSetterAccess, TValue, MockBehavior)" />
	protected override void ExecuteSetterCallback<T>(IndexerSetterAccess indexerSetterAccess, T value, MockBehavior behavior)
	{
		if (value is TValue resultValue &&
			indexerSetterAccess.Parameters.Length == 2 &&
			TryCast(indexerSetterAccess.Parameters[0], out T1 p1, behavior) &&
			TryCast(indexerSetterAccess.Parameters[1], out T2 p2, behavior))
		{
			_setterCallbacks.ForEach(callback => callback.Invoke(resultValue, p1, p2));
		}
	}

	/// <inheritdoc cref="IsMatch(object?[])" />
	protected override bool IsMatch(object?[] parameters)
		=> Matches([match1, match2], parameters);

	/// <inheritdoc cref="IndexerSetup.TryGetInitialValue{T}(MockBehavior, object?[], out T)" />
	protected override bool TryGetInitialValue<T>(MockBehavior behavior, object?[] parameters, [NotNullWhen(true)] out T value)
	{
		if (_initialization is not null &&
			parameters.Length == 2 &&
			TryCast(parameters[0], out T1 p1, behavior) &&
			TryCast(parameters[1], out T2 p2, behavior) &&
			_initialization.Invoke(p1, p2) is T initialValue)
		{
			value = initialValue;
			return true;
		}

		value = default!;
		return false;
	}
}

/// <summary>
///     Sets up a <typeparamref name="TValue"/> indexer for <typeparamref name="T1"/>, <typeparamref name="T2"/> and <typeparamref name="T3"/>.
/// </summary>
public class IndexerSetup<TValue, T1, T2, T3>(With.Parameter<T1> match1, With.Parameter<T2> match2, With.Parameter<T3> match3) : IndexerSetup
{
	private readonly List<Action<T1, T2, T3>> _getterCallbacks = [];
	private readonly List<Action<TValue, T1, T2, T3>> _setterCallbacks = [];
	private Func<T1, T2, T3, TValue>? _initialization;

	/// <summary>
	///     Initializes the indexer with the given <paramref name="value" />.
	/// </summary>
	public IndexerSetup<TValue, T1, T2, T3> InitializeWith(TValue value)
	{
		// TODO: Throw when already initialized!
		_initialization = (_, _, _) => value;
		return this;
	}

	/// <summary>
	///     Initializes the indexer according to the given <paramref name="valueGenerator" />.
	/// </summary>
	public IndexerSetup<TValue, T1, T2, T3> InitializeWith(Func<T1, T2, T3, TValue> valueGenerator)
	{
		// TODO: Throw when already initialized!
		_initialization = valueGenerator;
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's getter is accessed.
	/// </summary>
	public IndexerSetup<TValue, T1, T2, T3> OnGet(Action callback)
	{
		_getterCallbacks.Add((_, _, _) => callback());
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's getter is accessed.
	/// </summary>
	public IndexerSetup<TValue, T1, T2, T3> OnGet(Action<T1, T2, T3> callback)
	{
		_getterCallbacks.Add(callback);
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	public IndexerSetup<TValue, T1, T2, T3> OnSet(Action callback)
	{
		_setterCallbacks.Add((_, _, _, _) => callback());
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	public IndexerSetup<TValue, T1, T2, T3> OnSet(Action<TValue> callback)
	{
		_setterCallbacks.Add((v, _, _, _) => callback(v));
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	public IndexerSetup<TValue, T1, T2, T3> OnSet(Action<TValue, T1, T2, T3> callback)
	{
		_setterCallbacks.Add(callback);
		return this;
	}

	/// <inheritdoc cref="ExecuteGetterCallback(IndexerGetterAccess, MockBehavior)" />
	protected override void ExecuteGetterCallback(IndexerGetterAccess indexerGetterAccess, MockBehavior behavior)
	{
		if (indexerGetterAccess.Parameters.Length == 3 &&
			TryCast(indexerGetterAccess.Parameters[0], out T1 p1, behavior) &&
			TryCast(indexerGetterAccess.Parameters[1], out T2 p2, behavior) &&
			TryCast(indexerGetterAccess.Parameters[2], out T3 p3, behavior))
		{
			_getterCallbacks.ForEach(callback => callback.Invoke(p1, p2, p3));
		}
	}

	/// <inheritdoc cref="ExecuteSetterCallback{TValue}(IndexerSetterAccess, TValue, MockBehavior)" />
	protected override void ExecuteSetterCallback<T>(IndexerSetterAccess indexerSetterAccess, T value, MockBehavior behavior)
	{
		if (value is TValue resultValue &&
			indexerSetterAccess.Parameters.Length == 3 &&
			TryCast(indexerSetterAccess.Parameters[0], out T1 p1, behavior) &&
			TryCast(indexerSetterAccess.Parameters[1], out T2 p2, behavior) &&
			TryCast(indexerSetterAccess.Parameters[2], out T3 p3, behavior))
		{
			_setterCallbacks.ForEach(callback => callback.Invoke(resultValue, p1, p2, p3));
		}
	}

	/// <inheritdoc cref="IsMatch(object?[])" />
	protected override bool IsMatch(object?[] parameters)
		=> Matches([match1, match2, match3], parameters);

	/// <inheritdoc cref="IndexerSetup.TryGetInitialValue{T}(MockBehavior, object?[], out T)" />
	protected override bool TryGetInitialValue<T>(MockBehavior behavior, object?[] parameters, [NotNullWhen(true)] out T value)
	{
		if (_initialization is not null &&
			parameters.Length == 3 &&
			TryCast(parameters[0], out T1 p1, behavior) &&
			TryCast(parameters[1], out T2 p2, behavior) &&
			TryCast(parameters[2], out T3 p3, behavior) &&
			_initialization.Invoke(p1, p2, p3) is T initialValue)
		{
			value = initialValue;
			return true;
		}

		value = default!;
		return false;
	}
}
