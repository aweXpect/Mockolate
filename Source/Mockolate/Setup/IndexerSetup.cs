using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Mockolate.Exceptions;
using Mockolate.Interactions;
using Mockolate.Parameters;

namespace Mockolate.Setup;

/// <summary>
///     Base class for indexer setups.
/// </summary>
public abstract class IndexerSetup : IInteractiveIndexerSetup
{
	/// <inheritdoc cref="IInteractiveIndexerSetup.HasReturnCalls()" />
	bool IInteractiveIndexerSetup.HasReturnCalls()
		=> HasReturnCalls();

	/// <inheritdoc cref="IInteractiveIndexerSetup.Matches(IndexerAccess)" />
	bool IInteractiveIndexerSetup.Matches(IndexerAccess indexerAccess)
		=> IsMatch(indexerAccess.Parameters);

	/// <inheritdoc cref="IInteractiveIndexerSetup.TryGetInitialValue{TValue}(MockBehavior, Func{TValue}, object?[], out TValue)" />
	bool IInteractiveIndexerSetup.TryGetInitialValue<TValue>(MockBehavior behavior, Func<TValue> defaultValueGenerator,
		object?[] parameters,
		[NotNullWhen(true)] out TValue value)
		=> TryGetInitialValue(behavior, defaultValueGenerator, parameters, out value);

	/// <inheritdoc cref="IInteractiveIndexerSetup.CallBaseClass()" />
	bool? IInteractiveIndexerSetup.CallBaseClass()
		=> GetCallBaseClass();

	internal TValue InvokeGetter<TValue>(IndexerGetterAccess getterAccess, TValue value, MockBehavior behavior)
		=> ExecuteGetterCallback(getterAccess, value, behavior);

	internal void InvokeSetter<TValue>(IndexerSetterAccess setterAccess, TValue value, MockBehavior behavior)
		=> ExecuteSetterCallback(setterAccess, value, behavior);

	/// <summary>
	///     Execute a potentially registered getter callback.
	/// </summary>
	protected abstract T ExecuteGetterCallback<T>(IndexerGetterAccess indexerGetterAccess, T value,
		MockBehavior behavior);

	/// <summary>
	///     Execute a potentially registered setter callback.
	/// </summary>
	protected abstract void ExecuteSetterCallback<T>(IndexerSetterAccess indexerSetterAccess, T value,
		MockBehavior behavior);

	/// <summary>
	///     Checks if the <paramref name="parameters" /> match the setup.
	/// </summary>
	protected abstract bool IsMatch(object?[] parameters);

	/// <summary>
	///     Attempts to cast the specified value to the type parameter <typeparamref name="T" />,
	///     returning a value that indicates whether the cast was successful.
	/// </summary>
	/// <remarks>
	///     If value is not of type <typeparamref name="T" /> and is not <see langword="null" />,
	///     result is set to the default value for type <typeparamref name="T" /> as provided
	///     by the <paramref name="behavior" />.
	/// </remarks>
	protected static bool TryCast<T>([NotNullWhen(false)] object? value, out T result, MockBehavior behavior)
	{
		if (value is T typedValue)
		{
			result = typedValue;
			return true;
		}

		result = default!;
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
	protected static bool Matches(IParameter[] parameters, object?[] values)
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

		for (int i = 0; i < parameters.Length; i++)
		{
			parameters[i].InvokeCallbacks(values[i]);
		}

		return true;
	}

	/// <summary>
	///     Gets the flag indicating if the base class implementation should be called, and its return values used as default
	///     values.
	/// </summary>
	protected abstract bool? GetCallBaseClass();

	/// <summary>
	///     Gets a value indicating whether this setup has return calls configured.
	/// </summary>
	protected abstract bool HasReturnCalls();

	/// <summary>
	///     Attempts to retrieve the initial <paramref name="value" /> for the <paramref name="parameters" />, if an
	///     initialization is set up.
	/// </summary>
	protected abstract bool TryGetInitialValue<T>(MockBehavior behavior, Func<T> defaultValueGenerator,
		object?[] parameters,
		[NotNullWhen(true)] out T value);
}

/// <summary>
///     Sets up a <typeparamref name="TValue" /> indexer for <typeparamref name="T1" />.
/// </summary>
public class IndexerSetup<TValue, T1>(IParameter match1)
	: IndexerSetup, IIndexerSetupCallbackBuilder<TValue, T1>, IIndexerSetupReturnBuilder<TValue, T1>
{
	private readonly List<Callback<Action<int, T1>>> _getterCallbacks = [];
	private readonly List<Callback<Func<int, TValue, T1, TValue>>> _returnCallbacks = [];
	private readonly List<Callback<Action<int, TValue, T1>>> _setterCallbacks = [];
	private bool? _callBaseClass;
	private Callback? _currentCallback;
	private Callback? _currentReturnCallback;
	private int _currentReturnCallbackIndex;
	private Func<T1, TValue>? _initialization;

	/// <inheritdoc cref="IIndexerSetup{TValue, T1}.CallingBaseClass(bool)" />
	public IIndexerSetup<TValue, T1> CallingBaseClass(bool callBaseClass = true)
	{
		_callBaseClass = callBaseClass;
		return this;
	}

	/// <summary>
	///     Initializes the indexer with the given <paramref name="value" />.
	/// </summary>
	public IIndexerSetup<TValue, T1> InitializeWith(TValue value)
	{
		if (_initialization is not null)
		{
			throw new MockException("The indexer is already initialized. You cannot initialize it twice.");
		}

		_initialization = _ => value;
		return this;
	}

	/// <summary>
	///     Initializes the indexer according to the given <paramref name="valueGenerator" />.
	/// </summary>
	public IIndexerSetup<TValue, T1> InitializeWith(Func<T1, TValue> valueGenerator)
	{
		if (_initialization is not null)
		{
			throw new MockException("The indexer is already initialized. You cannot initialize it twice.");
		}

		_initialization = valueGenerator;
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's getter is accessed.
	/// </summary>
	public IIndexerSetupCallbackBuilder<TValue, T1> OnGet(Action callback)
	{
		Callback<Action<int, T1>> currentCallback = new((_, _) => callback());
		_currentCallback = currentCallback;
		_getterCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's getter is accessed.
	/// </summary>
	public IIndexerSetupCallbackBuilder<TValue, T1> OnGet(Action<T1> callback)
	{
		Callback<Action<int, T1>> currentCallback = new((_, p1) => callback(p1));
		_currentCallback = currentCallback;
		_getterCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's getter is accessed.
	/// </summary>
	public IIndexerSetupCallbackBuilder<TValue, T1> OnGet(Action<int, T1> callback)
	{
		Callback<Action<int, T1>> currentCallback = new(callback);
		_currentCallback = currentCallback;
		_getterCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	public IIndexerSetupCallbackBuilder<TValue, T1> OnSet(Action callback)
	{
		Callback<Action<int, TValue, T1>> currentCallback = new((_, _, _) => callback());
		_currentCallback = currentCallback;
		_setterCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	public IIndexerSetupCallbackBuilder<TValue, T1> OnSet(Action<TValue> callback)
	{
		Callback<Action<int, TValue, T1>> currentCallback = new((_, v, _) => callback(v));
		_currentCallback = currentCallback;
		_setterCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	public IIndexerSetupCallbackBuilder<TValue, T1> OnSet(Action<TValue, T1> callback)
	{
		Callback<Action<int, TValue, T1>> currentCallback = new((_, v, p1) => callback(v, p1));
		_currentCallback = currentCallback;
		_setterCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	public IIndexerSetupCallbackBuilder<TValue, T1> OnSet(Action<int, TValue, T1> callback)
	{
		Callback<Action<int, TValue, T1>> currentCallback = new(callback);
		_currentCallback = currentCallback;
		_setterCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this property.
	/// </summary>
	public IIndexerSetupReturnBuilder<TValue, T1> Returns(Func<TValue, T1, TValue> callback)
	{
		Callback<Func<int, TValue, T1, TValue>> currentCallback = new((_, v, p1) => callback(v, p1));
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this property.
	/// </summary>
	public IIndexerSetupReturnBuilder<TValue, T1> Returns(Func<T1, TValue> callback)
	{
		Callback<Func<int, TValue, T1, TValue>> currentCallback = new((_, _, p1) => callback(p1));
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this property.
	/// </summary>
	public IIndexerSetupReturnBuilder<TValue, T1> Returns(Func<TValue> callback)
	{
		Callback<Func<int, TValue, T1, TValue>> currentCallback = new((_, _, _) => callback());
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers the <paramref name="returnValue" /> for this property.
	/// </summary>
	public IIndexerSetupReturnBuilder<TValue, T1> Returns(TValue returnValue)
	{
		Callback<Func<int, TValue, T1, TValue>> currentCallback = new((_, _, _) => returnValue);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers an <typeparamref name="TException" /> to throw when the property is read.
	/// </summary>
	public IIndexerSetupReturnBuilder<TValue, T1> Throws<TException>()
		where TException : Exception, new()
	{
		Callback<Func<int, TValue, T1, TValue>> currentCallback = new((_, _, _) => throw new TException());
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the property is read.
	/// </summary>
	public IIndexerSetupReturnBuilder<TValue, T1> Throws(Exception exception)
	{
		Callback<Func<int, TValue, T1, TValue>> currentCallback = new((_, _, _) => throw exception);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the property is read.
	/// </summary>
	public IIndexerSetupReturnBuilder<TValue, T1> Throws(Func<Exception> callback)
	{
		Callback<Func<int, TValue, T1, TValue>> currentCallback = new((_, _, _) => throw callback());
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the property is read.
	/// </summary>
	public IIndexerSetupReturnBuilder<TValue, T1> Throws(Func<T1, Exception> callback)
	{
		Callback<Func<int, TValue, T1, TValue>> currentCallback = new((_, _, p1) => throw callback(p1));
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the property is read.
	/// </summary>
	public IIndexerSetupReturnBuilder<TValue, T1> Throws(Func<TValue, T1, Exception> callback)
	{
		Callback<Func<int, TValue, T1, TValue>> currentCallback = new((_, v, p1) => throw callback(v, p1));
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IIndexerSetupCallbackBuilder{TValue, T1}.When(Func{int, bool})" />
	IIndexerSetupCallbackWhenBuilder<TValue, T1> IIndexerSetupCallbackBuilder<TValue, T1>.When(
		Func<int, bool> predicate)
	{
		_currentCallback?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IIndexerSetupCallbackWhenBuilder{TValue,T1}.For(int)" />
	IIndexerSetup<TValue, T1> IIndexerSetupCallbackWhenBuilder<TValue, T1>.For(int times)
	{
		_currentCallback?.For(x => x < times);
		return this;
	}

	/// <inheritdoc cref="IIndexerSetupReturnBuilder{TValue, T1}.When(Func{int, bool})" />
	IIndexerSetupReturnWhenBuilder<TValue, T1> IIndexerSetupReturnBuilder<TValue, T1>.When(Func<int, bool> predicate)
	{
		_currentReturnCallback?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IIndexerSetupReturnWhenBuilder{TValue,T1}.For(int)" />
	IIndexerSetup<TValue, T1> IIndexerSetupReturnWhenBuilder<TValue, T1>.For(int times)
	{
		_currentReturnCallback?.For(x => x < times);
		return this;
	}

	/// <inheritdoc cref="IndexerSetup.GetCallBaseClass()" />
	protected override bool? GetCallBaseClass()
		=> _callBaseClass;

	/// <inheritdoc cref="IndexerSetup.HasReturnCalls()" />
	protected override bool HasReturnCalls()
		=> _returnCallbacks.Count > 0;

	/// <inheritdoc cref="ExecuteGetterCallback{TValue}(IndexerGetterAccess, TValue, MockBehavior)" />
	protected override T ExecuteGetterCallback<T>(IndexerGetterAccess indexerGetterAccess, T value,
		MockBehavior behavior)
	{
		if (TryCast(value, out TValue resultValue, behavior) &&
		    indexerGetterAccess.Parameters.Length == 1 &&
		    TryCast(indexerGetterAccess.Parameters[0], out T1 p1, behavior))
		{
			_getterCallbacks.ForEach(callback => callback.Invoke((invocationCount, @delegate)
				=> @delegate(invocationCount, p1)));
			foreach (Callback<Func<int, TValue, T1, TValue>> _ in _returnCallbacks)
			{
				Callback<Func<int, TValue, T1, TValue>> returnCallback =
					_returnCallbacks[_currentReturnCallbackIndex % _returnCallbacks.Count];
				if (returnCallback.Invoke(ref _currentReturnCallbackIndex, (invocationCount, @delegate)
					    => @delegate(invocationCount, resultValue, p1), out TValue? newValue) &&
				    TryCast(newValue, out T returnValue, behavior))
				{
					return returnValue;
				}
			}
		}

		return value;
	}

	/// <inheritdoc cref="ExecuteSetterCallback{TValue}(IndexerSetterAccess, TValue, MockBehavior)" />
	protected override void ExecuteSetterCallback<T>(IndexerSetterAccess indexerSetterAccess, T value,
		MockBehavior behavior)
	{
		if (TryCast(value, out TValue resultValue, behavior) &&
		    indexerSetterAccess.Parameters.Length == 1 &&
		    TryCast(indexerSetterAccess.Parameters[0], out T1 p1, behavior))
		{
			_setterCallbacks.ForEach(callback => callback.Invoke((invocationCount, @delegate)
				=> @delegate(invocationCount, resultValue, p1)));
		}
	}

	/// <inheritdoc cref="IsMatch(object?[])" />
	protected override bool IsMatch(object?[] parameters)
		=> Matches([match1,], parameters);

	/// <inheritdoc cref="IndexerSetup.TryGetInitialValue{T}(MockBehavior, Func{T}, object?[], out T)" />
	protected override bool TryGetInitialValue<T>(MockBehavior behavior, Func<T> defaultValueGenerator,
		object?[] parameters,
		[NotNullWhen(true)] out T value)
	{
		if (_initialization is not null &&
		    parameters.Length == 1 &&
		    TryCast(parameters[0], out T1 p1, behavior) &&
		    _initialization.Invoke(p1) is T initialValue)
		{
			value = initialValue;
			return true;
		}

		value = defaultValueGenerator();
		return false;
	}
}

/// <summary>
///     Sets up a <typeparamref name="TValue" /> indexer for <typeparamref name="T1" /> and <typeparamref name="T2" />.
/// </summary>
public class IndexerSetup<TValue, T1, T2>(IParameter match1, IParameter match2)
	: IndexerSetup, IIndexerSetupCallbackBuilder<TValue, T1, T2>, IIndexerSetupReturnBuilder<TValue, T1, T2>
{
	private readonly List<Callback<Action<int, T1, T2>>> _getterCallbacks = [];
	private readonly List<Callback<Func<int, TValue, T1, T2, TValue>>> _returnCallbacks = [];
	private readonly List<Callback<Action<int, TValue, T1, T2>>> _setterCallbacks = [];
	private bool? _callBaseClass;
	private Callback? _currentCallback;
	private Callback? _currentReturnCallback;
	private int _currentReturnCallbackIndex;
	private Func<T1, T2, TValue>? _initialization;

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2}.CallingBaseClass(bool)" />
	public IIndexerSetup<TValue, T1, T2> CallingBaseClass(bool callBaseClass = true)
	{
		_callBaseClass = callBaseClass;
		return this;
	}

	/// <summary>
	///     Initializes the indexer with the given <paramref name="value" />.
	/// </summary>
	public IIndexerSetup<TValue, T1, T2> InitializeWith(TValue value)
	{
		if (_initialization is not null)
		{
			throw new MockException("The indexer is already initialized. You cannot initialize it twice.");
		}

		_initialization = (_, _) => value;
		return this;
	}

	/// <summary>
	///     Initializes the indexer according to the given <paramref name="valueGenerator" />.
	/// </summary>
	public IIndexerSetup<TValue, T1, T2> InitializeWith(Func<T1, T2, TValue> valueGenerator)
	{
		if (_initialization is not null)
		{
			throw new MockException("The indexer is already initialized. You cannot initialize it twice.");
		}

		_initialization = valueGenerator;
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's getter is accessed.
	/// </summary>
	public IIndexerSetupCallbackBuilder<TValue, T1, T2> OnGet(Action callback)
	{
		Callback<Action<int, T1, T2>> currentCallback = new((_, _, _) => callback());
		_currentCallback = currentCallback;
		_getterCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's getter is accessed.
	/// </summary>
	public IIndexerSetupCallbackBuilder<TValue, T1, T2> OnGet(Action<T1, T2> callback)
	{
		Callback<Action<int, T1, T2>> currentCallback = new((_, p1, p2) => callback(p1, p2));
		_currentCallback = currentCallback;
		_getterCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's getter is accessed.
	/// </summary>
	public IIndexerSetupCallbackBuilder<TValue, T1, T2> OnGet(Action<int, T1, T2> callback)
	{
		Callback<Action<int, T1, T2>> currentCallback = new(callback);
		_currentCallback = currentCallback;
		_getterCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	public IIndexerSetupCallbackBuilder<TValue, T1, T2> OnSet(Action callback)
	{
		Callback<Action<int, TValue, T1, T2>> currentCallback = new((_, _, _, _) => callback());
		_currentCallback = currentCallback;
		_setterCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	public IIndexerSetupCallbackBuilder<TValue, T1, T2> OnSet(Action<TValue> callback)
	{
		Callback<Action<int, TValue, T1, T2>> currentCallback = new((_, v, _, _) => callback(v));
		_currentCallback = currentCallback;
		_setterCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	public IIndexerSetupCallbackBuilder<TValue, T1, T2> OnSet(Action<TValue, T1, T2> callback)
	{
		Callback<Action<int, TValue, T1, T2>> currentCallback = new((_, v, p1, p2) => callback(v, p1, p2));
		_currentCallback = currentCallback;
		_setterCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	public IIndexerSetupCallbackBuilder<TValue, T1, T2> OnSet(Action<int, TValue, T1, T2> callback)
	{
		Callback<Action<int, TValue, T1, T2>> currentCallback = new(callback);
		_currentCallback = currentCallback;
		_setterCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this property.
	/// </summary>
	public IIndexerSetupReturnBuilder<TValue, T1, T2> Returns(Func<TValue, T1, T2, TValue> callback)
	{
		Callback<Func<int, TValue, T1, T2, TValue>> currentCallback = new((_, v, p1, p2) => callback(v, p1, p2));
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this property.
	/// </summary>
	public IIndexerSetupReturnBuilder<TValue, T1, T2> Returns(Func<T1, T2, TValue> callback)
	{
		Callback<Func<int, TValue, T1, T2, TValue>> currentCallback = new((_, _, p1, p2) => callback(p1, p2));
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this property.
	/// </summary>
	public IIndexerSetupReturnBuilder<TValue, T1, T2> Returns(Func<TValue> callback)
	{
		Callback<Func<int, TValue, T1, T2, TValue>> currentCallback = new((_, _, _, _) => callback());
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers the <paramref name="returnValue" /> for this property.
	/// </summary>
	public IIndexerSetupReturnBuilder<TValue, T1, T2> Returns(TValue returnValue)
	{
		Callback<Func<int, TValue, T1, T2, TValue>> currentCallback = new((_, _, _, _) => returnValue);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers an <typeparamref name="TException" /> to throw when the property is read.
	/// </summary>
	public IIndexerSetupReturnBuilder<TValue, T1, T2> Throws<TException>()
		where TException : Exception, new()
	{
		Callback<Func<int, TValue, T1, T2, TValue>> currentCallback = new((_, _, _, _) => throw new TException());
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the property is read.
	/// </summary>
	public IIndexerSetupReturnBuilder<TValue, T1, T2> Throws(Exception exception)
	{
		Callback<Func<int, TValue, T1, T2, TValue>> currentCallback = new((_, _, _, _) => throw exception);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the property is read.
	/// </summary>
	public IIndexerSetupReturnBuilder<TValue, T1, T2> Throws(Func<Exception> callback)
	{
		Callback<Func<int, TValue, T1, T2, TValue>> currentCallback = new((_, _, _, _) => throw callback());
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the property is read.
	/// </summary>
	public IIndexerSetupReturnBuilder<TValue, T1, T2> Throws(Func<T1, T2, Exception> callback)
	{
		Callback<Func<int, TValue, T1, T2, TValue>> currentCallback =
			new((_, _, p1, p2) => throw callback(p1, p2));
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the property is read.
	/// </summary>
	public IIndexerSetupReturnBuilder<TValue, T1, T2> Throws(Func<TValue, T1, T2, Exception> callback)
	{
		Callback<Func<int, TValue, T1, T2, TValue>> currentCallback =
			new((_, v, p1, p2) => throw callback(v, p1, p2));
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IIndexerSetupCallbackBuilder{TValue, T1, T2}.When(Func{int, bool})" />
	IIndexerSetupCallbackWhenBuilder<TValue, T1, T2> IIndexerSetupCallbackBuilder<TValue, T1, T2>.When(
		Func<int, bool> predicate)
	{
		_currentCallback?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IIndexerSetupCallbackWhenBuilder{TValue,T1, T2}.For(int)" />
	IIndexerSetup<TValue, T1, T2> IIndexerSetupCallbackWhenBuilder<TValue, T1, T2>.For(int times)
	{
		_currentCallback?.For(x => x < times);
		return this;
	}

	/// <inheritdoc cref="IIndexerSetupReturnBuilder{TValue, T1, T2}.When(Func{int, bool})" />
	IIndexerSetupReturnWhenBuilder<TValue, T1, T2> IIndexerSetupReturnBuilder<TValue, T1, T2>.When(
		Func<int, bool> predicate)
	{
		_currentReturnCallback?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IIndexerSetupReturnWhenBuilder{TValue,T1, T2}.For(int)" />
	IIndexerSetup<TValue, T1, T2> IIndexerSetupReturnWhenBuilder<TValue, T1, T2>.For(int times)
	{
		_currentReturnCallback?.For(x => x < times);
		return this;
	}

	/// <inheritdoc cref="IndexerSetup.GetCallBaseClass()" />
	protected override bool? GetCallBaseClass()
		=> _callBaseClass;

	/// <inheritdoc cref="IndexerSetup.HasReturnCalls()" />
	protected override bool HasReturnCalls()
		=> _returnCallbacks.Count > 0;

	/// <inheritdoc cref="ExecuteGetterCallback{TValue}(IndexerGetterAccess, TValue, MockBehavior)" />
	protected override T ExecuteGetterCallback<T>(IndexerGetterAccess indexerGetterAccess, T value,
		MockBehavior behavior)
	{
		if (TryCast(value, out TValue resultValue, behavior) &&
		    indexerGetterAccess.Parameters.Length == 2 &&
		    TryCast(indexerGetterAccess.Parameters[0], out T1 p1, behavior) &&
		    TryCast(indexerGetterAccess.Parameters[1], out T2 p2, behavior))
		{
			_getterCallbacks.ForEach(callback => callback.Invoke((invocationCount, @delegate)
				=> @delegate(invocationCount, p1, p2)));
			foreach (Callback<Func<int, TValue, T1, T2, TValue>> _ in _returnCallbacks)
			{
				Callback<Func<int, TValue, T1, T2, TValue>> returnCallback =
					_returnCallbacks[_currentReturnCallbackIndex % _returnCallbacks.Count];
				if (returnCallback.Invoke(ref _currentReturnCallbackIndex, (invocationCount, @delegate)
					    => @delegate(invocationCount, resultValue, p1, p2), out TValue? newValue) &&
				    TryCast(newValue, out T returnValue, behavior))
				{
					return returnValue;
				}
			}
		}

		return value;
	}

	/// <inheritdoc cref="ExecuteSetterCallback{TValue}(IndexerSetterAccess, TValue, MockBehavior)" />
	protected override void ExecuteSetterCallback<T>(IndexerSetterAccess indexerSetterAccess, T value,
		MockBehavior behavior)
	{
		if (TryCast(value, out TValue resultValue, behavior) &&
		    indexerSetterAccess.Parameters.Length == 2 &&
		    TryCast(indexerSetterAccess.Parameters[0], out T1 p1, behavior) &&
		    TryCast(indexerSetterAccess.Parameters[1], out T2 p2, behavior))
		{
			_setterCallbacks.ForEach(callback => callback.Invoke((invocationCount, @delegate)
				=> @delegate(invocationCount, resultValue, p1, p2)));
		}
	}

	/// <inheritdoc cref="IsMatch(object?[])" />
	protected override bool IsMatch(object?[] parameters)
		=> Matches([match1, match2,], parameters);

	/// <inheritdoc cref="IndexerSetup.TryGetInitialValue{T}(MockBehavior, Func{T}, object?[], out T)" />
	protected override bool TryGetInitialValue<T>(MockBehavior behavior, Func<T> defaultValueGenerator,
		object?[] parameters,
		[NotNullWhen(true)] out T value)
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

		value = defaultValueGenerator();
		return false;
	}
}

/// <summary>
///     Sets up a <typeparamref name="TValue" /> indexer for <typeparamref name="T1" />, <typeparamref name="T2" /> and
///     <typeparamref name="T3" />.
/// </summary>
public class IndexerSetup<TValue, T1, T2, T3>(
	IParameter match1,
	IParameter match2,
	IParameter match3)
	: IndexerSetup, IIndexerSetupCallbackBuilder<TValue, T1, T2, T3>, IIndexerSetupReturnBuilder<TValue, T1, T2, T3>
{
	private readonly List<Callback<Action<int, T1, T2, T3>>> _getterCallbacks = [];
	private readonly List<Callback<Func<int, TValue, T1, T2, T3, TValue>>> _returnCallbacks = [];
	private readonly List<Callback<Action<int, TValue, T1, T2, T3>>> _setterCallbacks = [];
	private bool? _callBaseClass;
	private Callback? _currentCallback;
	private Callback? _currentReturnCallback;
	private int _currentReturnCallbackIndex;
	private Func<T1, T2, T3, TValue>? _initialization;

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2, T3}.CallingBaseClass(bool)" />
	public IIndexerSetup<TValue, T1, T2, T3> CallingBaseClass(bool callBaseClass = true)
	{
		_callBaseClass = callBaseClass;
		return this;
	}

	/// <summary>
	///     Initializes the indexer with the given <paramref name="value" />.
	/// </summary>
	public IIndexerSetup<TValue, T1, T2, T3> InitializeWith(TValue value)
	{
		if (_initialization is not null)
		{
			throw new MockException("The indexer is already initialized. You cannot initialize it twice.");
		}

		_initialization = (_, _, _) => value;
		return this;
	}

	/// <summary>
	///     Initializes the indexer according to the given <paramref name="valueGenerator" />.
	/// </summary>
	public IIndexerSetup<TValue, T1, T2, T3> InitializeWith(Func<T1, T2, T3, TValue> valueGenerator)
	{
		if (_initialization is not null)
		{
			throw new MockException("The indexer is already initialized. You cannot initialize it twice.");
		}

		_initialization = valueGenerator;
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's getter is accessed.
	/// </summary>
	public IIndexerSetupCallbackBuilder<TValue, T1, T2, T3> OnGet(Action callback)
	{
		Callback<Action<int, T1, T2, T3>> currentCallback = new((_, _, _, _) => callback());
		_currentCallback = currentCallback;
		_getterCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's getter is accessed.
	/// </summary>
	public IIndexerSetupCallbackBuilder<TValue, T1, T2, T3> OnGet(Action<T1, T2, T3> callback)
	{
		Callback<Action<int, T1, T2, T3>> currentCallback = new((_, p1, p2, p3) => callback(p1, p2, p3));
		_currentCallback = currentCallback;
		_getterCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's getter is accessed.
	/// </summary>
	public IIndexerSetupCallbackBuilder<TValue, T1, T2, T3> OnGet(Action<int, T1, T2, T3> callback)
	{
		Callback<Action<int, T1, T2, T3>> currentCallback = new(callback);
		_currentCallback = currentCallback;
		_getterCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	public IIndexerSetupCallbackBuilder<TValue, T1, T2, T3> OnSet(Action callback)
	{
		Callback<Action<int, TValue, T1, T2, T3>> currentCallback = new((_, _, _, _, _) => callback());
		_currentCallback = currentCallback;
		_setterCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	public IIndexerSetupCallbackBuilder<TValue, T1, T2, T3> OnSet(Action<TValue> callback)
	{
		Callback<Action<int, TValue, T1, T2, T3>> currentCallback = new((_, v, _, _, _) => callback(v));
		_currentCallback = currentCallback;
		_setterCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	public IIndexerSetupCallbackBuilder<TValue, T1, T2, T3> OnSet(Action<TValue, T1, T2, T3> callback)
	{
		Callback<Action<int, TValue, T1, T2, T3>> currentCallback = new((_, v, p1, p2, p3) => callback(v, p1, p2, p3));
		_currentCallback = currentCallback;
		_setterCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	public IIndexerSetupCallbackBuilder<TValue, T1, T2, T3> OnSet(Action<int, TValue, T1, T2, T3> callback)
	{
		Callback<Action<int, TValue, T1, T2, T3>> currentCallback = new(callback);
		_currentCallback = currentCallback;
		_setterCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this property.
	/// </summary>
	public IIndexerSetupReturnBuilder<TValue, T1, T2, T3> Returns(Func<TValue, T1, T2, T3, TValue> callback)
	{
		Callback<Func<int, TValue, T1, T2, T3, TValue>> currentCallback =
			new((_, v, p1, p2, p3) => callback(v, p1, p2, p3));
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this property.
	/// </summary>
	public IIndexerSetupReturnBuilder<TValue, T1, T2, T3> Returns(Func<T1, T2, T3, TValue> callback)
	{
		Callback<Func<int, TValue, T1, T2, T3, TValue>> currentCallback =
			new((_, _, p1, p2, p3) => callback(p1, p2, p3));
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this property.
	/// </summary>
	public IIndexerSetupReturnBuilder<TValue, T1, T2, T3> Returns(Func<TValue> callback)
	{
		Callback<Func<int, TValue, T1, T2, T3, TValue>> currentCallback = new((_, _, _, _, _) => callback());
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers the <paramref name="returnValue" /> for this property.
	/// </summary>
	public IIndexerSetupReturnBuilder<TValue, T1, T2, T3> Returns(TValue returnValue)
	{
		Callback<Func<int, TValue, T1, T2, T3, TValue>> currentCallback = new((_, _, _, _, _) => returnValue);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers an <typeparamref name="TException" /> to throw when the property is read.
	/// </summary>
	public IIndexerSetupReturnBuilder<TValue, T1, T2, T3> Throws<TException>()
		where TException : Exception, new()
	{
		Callback<Func<int, TValue, T1, T2, T3, TValue>> currentCallback =
			new((_, _, _, _, _) => throw new TException());
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the property is read.
	/// </summary>
	public IIndexerSetupReturnBuilder<TValue, T1, T2, T3> Throws(Exception exception)
	{
		Callback<Func<int, TValue, T1, T2, T3, TValue>> currentCallback = new((_, _, _, _, _) => throw exception);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the property is read.
	/// </summary>
	public IIndexerSetupReturnBuilder<TValue, T1, T2, T3> Throws(Func<Exception> callback)
	{
		Callback<Func<int, TValue, T1, T2, T3, TValue>>
			currentCallback = new((_, _, _, _, _) => throw callback());
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the property is read.
	/// </summary>
	public IIndexerSetupReturnBuilder<TValue, T1, T2, T3> Throws(Func<T1, T2, T3, Exception> callback)
	{
		Callback<Func<int, TValue, T1, T2, T3, TValue>> currentCallback =
			new((_, _, p1, p2, p3) => throw callback(p1, p2, p3));
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the property is read.
	/// </summary>
	public IIndexerSetupReturnBuilder<TValue, T1, T2, T3> Throws(Func<TValue, T1, T2, T3, Exception> callback)
	{
		Callback<Func<int, TValue, T1, T2, T3, TValue>> currentCallback =
			new((_, v, p1, p2, p3) => throw callback(v, p1, p2, p3));
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IIndexerSetupCallbackBuilder{TValue, T1, T2, T3}.When(Func{int, bool})" />
	IIndexerSetupCallbackWhenBuilder<TValue, T1, T2, T3> IIndexerSetupCallbackBuilder<TValue, T1, T2, T3>.When(
		Func<int, bool> predicate)
	{
		_currentCallback?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IIndexerSetupCallbackWhenBuilder{TValue,T1, T2, T3}.For(int)" />
	IIndexerSetup<TValue, T1, T2, T3> IIndexerSetupCallbackWhenBuilder<TValue, T1, T2, T3>.For(int times)
	{
		_currentCallback?.For(x => x < times);
		return this;
	}

	/// <inheritdoc cref="IIndexerSetupReturnBuilder{TValue, T1, T2, T3}.When(Func{int, bool})" />
	IIndexerSetupReturnWhenBuilder<TValue, T1, T2, T3> IIndexerSetupReturnBuilder<TValue, T1, T2, T3>.When(
		Func<int, bool> predicate)
	{
		_currentReturnCallback?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IIndexerSetupReturnWhenBuilder{TValue,T1, T2, T3}.For(int)" />
	IIndexerSetup<TValue, T1, T2, T3> IIndexerSetupReturnWhenBuilder<TValue, T1, T2, T3>.For(int times)
	{
		_currentReturnCallback?.For(x => x < times);
		return this;
	}

	/// <inheritdoc cref="IndexerSetup.GetCallBaseClass()" />
	protected override bool? GetCallBaseClass()
		=> _callBaseClass;

	/// <inheritdoc cref="IndexerSetup.HasReturnCalls()" />
	protected override bool HasReturnCalls()
		=> _returnCallbacks.Count > 0;

	/// <inheritdoc cref="ExecuteGetterCallback{TValue}(IndexerGetterAccess, TValue, MockBehavior)" />
	protected override T ExecuteGetterCallback<T>(IndexerGetterAccess indexerGetterAccess, T value,
		MockBehavior behavior)
	{
		if (TryCast(value, out TValue resultValue, behavior) &&
		    indexerGetterAccess.Parameters.Length == 3 &&
		    TryCast(indexerGetterAccess.Parameters[0], out T1 p1, behavior) &&
		    TryCast(indexerGetterAccess.Parameters[1], out T2 p2, behavior) &&
		    TryCast(indexerGetterAccess.Parameters[2], out T3 p3, behavior))
		{
			_getterCallbacks.ForEach(callback => callback.Invoke((invocationCount, @delegate)
				=> @delegate(invocationCount, p1, p2, p3)));
			foreach (Callback<Func<int, TValue, T1, T2, T3, TValue>> _ in _returnCallbacks)
			{
				Callback<Func<int, TValue, T1, T2, T3, TValue>> returnCallback =
					_returnCallbacks[_currentReturnCallbackIndex % _returnCallbacks.Count];
				if (returnCallback.Invoke(ref _currentReturnCallbackIndex, (invocationCount, @delegate)
					    => @delegate(invocationCount, resultValue, p1, p2, p3), out TValue? newValue) &&
				    TryCast(newValue, out T returnValue, behavior))
				{
					return returnValue;
				}
			}
		}

		return value;
	}

	/// <inheritdoc cref="ExecuteSetterCallback{TValue}(IndexerSetterAccess, TValue, MockBehavior)" />
	protected override void ExecuteSetterCallback<T>(IndexerSetterAccess indexerSetterAccess, T value,
		MockBehavior behavior)
	{
		if (TryCast(value, out TValue resultValue, behavior) &&
		    indexerSetterAccess.Parameters.Length == 3 &&
		    TryCast(indexerSetterAccess.Parameters[0], out T1 p1, behavior) &&
		    TryCast(indexerSetterAccess.Parameters[1], out T2 p2, behavior) &&
		    TryCast(indexerSetterAccess.Parameters[2], out T3 p3, behavior))
		{
			_setterCallbacks.ForEach(callback => callback.Invoke((invocationCount, @delegate)
				=> @delegate(invocationCount, resultValue, p1, p2, p3)));
		}
	}

	/// <inheritdoc cref="IsMatch(object?[])" />
	protected override bool IsMatch(object?[] parameters)
		=> Matches([match1, match2, match3,], parameters);

	/// <inheritdoc cref="IndexerSetup.TryGetInitialValue{T}(MockBehavior, Func{T}, object?[], out T)" />
	protected override bool TryGetInitialValue<T>(MockBehavior behavior, Func<T> defaultValueGenerator,
		object?[] parameters,
		[NotNullWhen(true)] out T value)
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

		value = defaultValueGenerator();
		return false;
	}
}

#pragma warning disable S2436 // Types and methods should not have too many generic parameters
/// <summary>
///     Sets up a <typeparamref name="TValue" /> indexer for <typeparamref name="T1" />, <typeparamref name="T2" />,
///     <typeparamref name="T3" /> and <typeparamref name="T4" />.
/// </summary>
public class IndexerSetup<TValue, T1, T2, T3, T4>(
	IParameter match1,
	IParameter match2,
	IParameter match3,
	IParameter match4)
	: IndexerSetup, IIndexerSetupCallbackBuilder<TValue, T1, T2, T3, T4>,
		IIndexerSetupReturnBuilder<TValue, T1, T2, T3, T4>
{
	private readonly List<Callback<Action<int, T1, T2, T3, T4>>> _getterCallbacks = [];
	private readonly List<Callback<Func<int, TValue, T1, T2, T3, T4, TValue>>> _returnCallbacks = [];
	private readonly List<Callback<Action<int, TValue, T1, T2, T3, T4>>> _setterCallbacks = [];
	private bool? _callBaseClass;
	private Callback? _currentCallback;
	private Callback? _currentReturnCallback;
	private int _currentReturnCallbackIndex;
	private Func<T1, T2, T3, T4, TValue>? _initialization;

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2, T3, T4}.CallingBaseClass(bool)" />
	public IIndexerSetup<TValue, T1, T2, T3, T4> CallingBaseClass(bool callBaseClass = true)
	{
		_callBaseClass = callBaseClass;
		return this;
	}

	/// <summary>
	///     Initializes the indexer with the given <paramref name="value" />.
	/// </summary>
	public IIndexerSetup<TValue, T1, T2, T3, T4> InitializeWith(TValue value)
	{
		if (_initialization is not null)
		{
			throw new MockException("The indexer is already initialized. You cannot initialize it twice.");
		}

		_initialization = (_, _, _, _) => value;
		return this;
	}

	/// <summary>
	///     Initializes the indexer according to the given <paramref name="valueGenerator" />.
	/// </summary>
	public IIndexerSetup<TValue, T1, T2, T3, T4> InitializeWith(Func<T1, T2, T3, T4, TValue> valueGenerator)
	{
		if (_initialization is not null)
		{
			throw new MockException("The indexer is already initialized. You cannot initialize it twice.");
		}

		_initialization = valueGenerator;
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's getter is accessed.
	/// </summary>
	public IIndexerSetupCallbackBuilder<TValue, T1, T2, T3, T4> OnGet(Action callback)
	{
		Callback<Action<int, T1, T2, T3, T4>> currentCallback = new((_, _, _, _, _) => callback());
		_currentCallback = currentCallback;
		_getterCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's getter is accessed.
	/// </summary>
	public IIndexerSetupCallbackBuilder<TValue, T1, T2, T3, T4> OnGet(Action<T1, T2, T3, T4> callback)
	{
		Callback<Action<int, T1, T2, T3, T4>> currentCallback = new((_, p1, p2, p3, p4) => callback(p1, p2, p3, p4));
		_currentCallback = currentCallback;
		_getterCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's getter is accessed.
	/// </summary>
	public IIndexerSetupCallbackBuilder<TValue, T1, T2, T3, T4> OnGet(Action<int, T1, T2, T3, T4> callback)
	{
		Callback<Action<int, T1, T2, T3, T4>> currentCallback = new(callback);
		_currentCallback = currentCallback;
		_getterCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	public IIndexerSetupCallbackBuilder<TValue, T1, T2, T3, T4> OnSet(Action callback)
	{
		Callback<Action<int, TValue, T1, T2, T3, T4>> currentCallback = new((_, _, _, _, _, _) => callback());
		_currentCallback = currentCallback;
		_setterCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	public IIndexerSetupCallbackBuilder<TValue, T1, T2, T3, T4> OnSet(Action<TValue> callback)
	{
		Callback<Action<int, TValue, T1, T2, T3, T4>> currentCallback = new((_, v, _, _, _, _) => callback(v));
		_currentCallback = currentCallback;
		_setterCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	public IIndexerSetupCallbackBuilder<TValue, T1, T2, T3, T4> OnSet(Action<TValue, T1, T2, T3, T4> callback)
	{
		Callback<Action<int, TValue, T1, T2, T3, T4>> currentCallback =
			new((_, v, p1, p2, p3, p4) => callback(v, p1, p2, p3, p4));
		_currentCallback = currentCallback;
		_setterCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a callback to be invoked whenever the indexer's setter is accessed.
	/// </summary>
	public IIndexerSetupCallbackBuilder<TValue, T1, T2, T3, T4> OnSet(Action<int, TValue, T1, T2, T3, T4> callback)
	{
		Callback<Action<int, TValue, T1, T2, T3, T4>> currentCallback = new(callback);
		_currentCallback = currentCallback;
		_setterCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this property.
	/// </summary>
	public IIndexerSetupReturnBuilder<TValue, T1, T2, T3, T4> Returns(Func<TValue, T1, T2, T3, T4, TValue> callback)
	{
		Callback<Func<int, TValue, T1, T2, T3, T4, TValue>> currentCallback =
			new((_, v, p1, p2, p3, p4) => callback(v, p1, p2, p3, p4));
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this property.
	/// </summary>
	public IIndexerSetupReturnBuilder<TValue, T1, T2, T3, T4> Returns(Func<T1, T2, T3, T4, TValue> callback)
	{
		Callback<Func<int, TValue, T1, T2, T3, T4, TValue>> currentCallback =
			new((_, _, p1, p2, p3, p4) => callback(p1, p2, p3, p4));
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this property.
	/// </summary>
	public IIndexerSetupReturnBuilder<TValue, T1, T2, T3, T4> Returns(Func<TValue> callback)
	{
		Callback<Func<int, TValue, T1, T2, T3, T4, TValue>> currentCallback =
			new((_, _, _, _, _, _) => callback());
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers the <paramref name="returnValue" /> for this property.
	/// </summary>
	public IIndexerSetupReturnBuilder<TValue, T1, T2, T3, T4> Returns(TValue returnValue)
	{
		Callback<Func<int, TValue, T1, T2, T3, T4, TValue>> currentCallback =
			new((_, _, _, _, _, _) => returnValue);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers an <typeparamref name="TException" /> to throw when the property is read.
	/// </summary>
	public IIndexerSetupReturnBuilder<TValue, T1, T2, T3, T4> Throws<TException>()
		where TException : Exception, new()
	{
		Callback<Func<int, TValue, T1, T2, T3, T4, TValue>> currentCallback =
			new((_, _, _, _, _, _) => throw new TException());
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the property is read.
	/// </summary>
	public IIndexerSetupReturnBuilder<TValue, T1, T2, T3, T4> Throws(Exception exception)
	{
		Callback<Func<int, TValue, T1, T2, T3, T4, TValue>> currentCallback =
			new((_, _, _, _, _, _) => throw exception);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the property is read.
	/// </summary>
	public IIndexerSetupReturnBuilder<TValue, T1, T2, T3, T4> Throws(Func<Exception> callback)
	{
		Callback<Func<int, TValue, T1, T2, T3, T4, TValue>> currentCallback =
			new((_, _, _, _, _, _) => throw callback());
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the property is read.
	/// </summary>
	public IIndexerSetupReturnBuilder<TValue, T1, T2, T3, T4> Throws(Func<T1, T2, T3, T4, Exception> callback)
	{
		Callback<Func<int, TValue, T1, T2, T3, T4, TValue>> currentCallback =
			new((_, _, p1, p2, p3, p4) => throw callback(p1, p2, p3, p4));
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the property is read.
	/// </summary>
	public IIndexerSetupReturnBuilder<TValue, T1, T2, T3, T4> Throws(Func<TValue, T1, T2, T3, T4, Exception> callback)
	{
		Callback<Func<int, TValue, T1, T2, T3, T4, TValue>> currentCallback =
			new((_, v, p1, p2, p3, p4) => throw callback(v, p1, p2, p3, p4));
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IIndexerSetupCallbackBuilder{TValue, T1, T2, T3, T4}.When(Func{int, bool})" />
	IIndexerSetupCallbackWhenBuilder<TValue, T1, T2, T3, T4> IIndexerSetupCallbackBuilder<TValue, T1, T2, T3, T4>.When(
		Func<int, bool> predicate)
	{
		_currentCallback?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IIndexerSetupCallbackWhenBuilder{TValue,T1, T2, T3, T4}.For(int)" />
	IIndexerSetup<TValue, T1, T2, T3, T4> IIndexerSetupCallbackWhenBuilder<TValue, T1, T2, T3, T4>.For(int times)
	{
		_currentCallback?.For(x => x < times);
		return this;
	}

	/// <inheritdoc cref="IIndexerSetupReturnBuilder{TValue, T1, T2, T3, T4}.When(Func{int, bool})" />
	IIndexerSetupReturnWhenBuilder<TValue, T1, T2, T3, T4> IIndexerSetupReturnBuilder<TValue, T1, T2, T3, T4>.When(
		Func<int, bool> predicate)
	{
		_currentReturnCallback?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IIndexerSetupReturnWhenBuilder{TValue,T1, T2, T3, T4}.For(int)" />
	IIndexerSetup<TValue, T1, T2, T3, T4> IIndexerSetupReturnWhenBuilder<TValue, T1, T2, T3, T4>.For(int times)
	{
		_currentReturnCallback?.For(x => x < times);
		return this;
	}

	/// <inheritdoc cref="IndexerSetup.GetCallBaseClass()" />
	protected override bool? GetCallBaseClass()
		=> _callBaseClass;

	/// <inheritdoc cref="IndexerSetup.HasReturnCalls()" />
	protected override bool HasReturnCalls()
		=> _returnCallbacks.Count > 0;

	/// <inheritdoc cref="ExecuteGetterCallback{TValue}(IndexerGetterAccess, TValue, MockBehavior)" />
	protected override T ExecuteGetterCallback<T>(IndexerGetterAccess indexerGetterAccess, T value,
		MockBehavior behavior)
	{
		if (TryCast(value, out TValue resultValue, behavior) &&
		    indexerGetterAccess.Parameters.Length == 4 &&
		    TryCast(indexerGetterAccess.Parameters[0], out T1 p1, behavior) &&
		    TryCast(indexerGetterAccess.Parameters[1], out T2 p2, behavior) &&
		    TryCast(indexerGetterAccess.Parameters[2], out T3 p3, behavior) &&
		    TryCast(indexerGetterAccess.Parameters[3], out T4 p4, behavior))
		{
			_getterCallbacks.ForEach(callback => callback.Invoke((invocationCount, @delegate)
				=> @delegate(invocationCount, p1, p2, p3, p4)));
			foreach (Callback<Func<int, TValue, T1, T2, T3, T4, TValue>> _ in _returnCallbacks)
			{
				Callback<Func<int, TValue, T1, T2, T3, T4, TValue>> returnCallback =
					_returnCallbacks[_currentReturnCallbackIndex % _returnCallbacks.Count];
				if (returnCallback.Invoke(ref _currentReturnCallbackIndex, (invocationCount, @delegate)
					    => @delegate(invocationCount, resultValue, p1, p2, p3, p4), out TValue? newValue) &&
				    TryCast(newValue, out T returnValue, behavior))
				{
					return returnValue;
				}
			}
		}

		return value;
	}

	/// <inheritdoc cref="ExecuteSetterCallback{TValue}(IndexerSetterAccess, TValue, MockBehavior)" />
	protected override void ExecuteSetterCallback<T>(IndexerSetterAccess indexerSetterAccess, T value,
		MockBehavior behavior)
	{
		if (TryCast(value, out TValue resultValue, behavior) &&
		    indexerSetterAccess.Parameters.Length == 4 &&
		    TryCast(indexerSetterAccess.Parameters[0], out T1 p1, behavior) &&
		    TryCast(indexerSetterAccess.Parameters[1], out T2 p2, behavior) &&
		    TryCast(indexerSetterAccess.Parameters[2], out T3 p3, behavior) &&
		    TryCast(indexerSetterAccess.Parameters[3], out T4 p4, behavior))
		{
			_setterCallbacks.ForEach(callback => callback.Invoke((invocationCount, @delegate)
				=> @delegate(invocationCount, resultValue, p1, p2, p3, p4)));
		}
	}

	/// <inheritdoc cref="IsMatch(object?[])" />
	protected override bool IsMatch(object?[] parameters)
		=> Matches([match1, match2, match3, match4,], parameters);

	/// <inheritdoc cref="IndexerSetup.TryGetInitialValue{T}(MockBehavior, Func{T}, object?[], out T)" />
	protected override bool TryGetInitialValue<T>(MockBehavior behavior, Func<T> defaultValueGenerator,
		object?[] parameters,
		[NotNullWhen(true)] out T value)
	{
		if (_initialization is not null &&
		    parameters.Length == 4 &&
		    TryCast(parameters[0], out T1 p1, behavior) &&
		    TryCast(parameters[1], out T2 p2, behavior) &&
		    TryCast(parameters[2], out T3 p3, behavior) &&
		    TryCast(parameters[3], out T4 p4, behavior) &&
		    _initialization.Invoke(p1, p2, p3, p4) is T initialValue)
		{
			value = initialValue;
			return true;
		}

		value = defaultValueGenerator();
		return false;
	}
}
#pragma warning restore S2436 // Types and methods should not have too many generic parameters
