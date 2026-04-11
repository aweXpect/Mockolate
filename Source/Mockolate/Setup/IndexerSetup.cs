using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Mockolate.Exceptions;
using Mockolate.Interactions;
using Mockolate.Internals;
using Mockolate.Parameters;

namespace Mockolate.Setup;

/// <summary>
///     Base class for indexer setups.
/// </summary>
#if !DEBUG
[DebuggerNonUserCode]
#endif
public abstract class IndexerSetup : IInteractiveIndexerSetup
{
	/// <inheritdoc cref="IInteractiveIndexerSetup.Matches(IndexerAccess)" />
	bool IInteractiveIndexerSetup.Matches(IndexerAccess indexerAccess)
		=> IsMatch(indexerAccess.Parameters);

	/// <inheritdoc
	///     cref="IInteractiveIndexerSetup.GetInitialValue{TValue}" />
	void IInteractiveIndexerSetup.GetInitialValue<TValue>(MockBehavior behavior, Func<TValue> defaultValueGenerator,
		INamedParameterValue[] parameters,
		[NotNullWhen(true)] out TValue value)
		=> GetInitialValue(behavior, defaultValueGenerator, parameters, out value);

	/// <inheritdoc cref="IInteractiveIndexerSetup.SkipBaseClass()" />
	bool? IInteractiveIndexerSetup.SkipBaseClass()
		=> GetSkipBaseClass();

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
	protected abstract bool IsMatch(INamedParameterValue[] parameters);

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
	protected static bool Matches(NamedParameter[] namedParameters, INamedParameterValue[] values)
	{
		if (namedParameters.Length != values.Length)
		{
			return false;
		}

		for (int i = 0; i < namedParameters.Length; i++)
		{
			if (!namedParameters[i].Matches(values[i]))
			{
				return false;
			}
		}

		for (int i = 0; i < namedParameters.Length; i++)
		{
			namedParameters[i].Parameter.InvokeCallbacks(values[i]);
		}

		return true;
	}

	/// <summary>
	///     Gets the flag indicating if the base class implementation should be skipped.
	/// </summary>
	protected abstract bool? GetSkipBaseClass();

	/// <summary>
	///     Attempts to retrieve the initial <paramref name="value" /> for the <paramref name="parameters" />, if an
	///     initialization is set up.
	/// </summary>
	protected abstract void GetInitialValue<T>(MockBehavior behavior, Func<T> defaultValueGenerator,
		INamedParameterValue[] parameters,
		[NotNullWhen(true)] out T value);

	/// <summary>
	///     Returns a formatted string representation of the given <paramref name="type" />.
	/// </summary>
	protected static string FormatType(Type type)
		=> type.FormatType();

	/// <summary>
	///     Checks if the given <paramref name="namedParameter" /> matches the typed <paramref name="value" />,
	///     using <see cref="ITypedParameter{T}" /> when available to avoid boxing.
	/// </summary>
	protected static bool MatchesParameter<T>(NamedParameter namedParameter, string name, T value)
	{
		if (!string.IsNullOrEmpty(name) &&
		    !namedParameter.Name.Equals(name, StringComparison.Ordinal))
		{
			return false;
		}

		if (namedParameter.Parameter is ITypedParameter<T> typed)
		{
			return typed.MatchesValue(namedParameter.Name, value);
		}

		return namedParameter.Parameter.Matches(new NamedParameterValue<T>(name, value));
	}

	/// <summary>
	///     Invokes the callbacks of the given <paramref name="namedParameter" /> with the typed <paramref name="value" />.
	/// </summary>
	protected static void InvokeCallbacksParameter<T>(NamedParameter namedParameter, string name, T value)
		=> namedParameter.Parameter.InvokeCallbacks(new NamedParameterValue<T>(name, value));
}

/// <summary>
///     Sets up a <typeparamref name="TValue" /> indexer for <typeparamref name="T1" />.
/// </summary>
#if !DEBUG
[DebuggerNonUserCode]
#endif
public class IndexerSetup<TValue, T1>(NamedParameter match1) : IndexerSetup,
	IIndexerSetupCallbackBuilder<TValue, T1>, IIndexerSetupReturnBuilder<TValue, T1>,
	IIndexerGetterSetup<TValue, T1>, IIndexerSetterSetup<TValue, T1>, ITypedIndexerMatch
{
	private readonly List<Callback<Action<int, T1, TValue>>> _getterCallbacks = [];
	private readonly List<Callback<Func<int, T1, TValue, TValue>>> _returnCallbacks = [];
	private readonly List<Callback<Action<int, T1, TValue>>> _setterCallbacks = [];
	private Callback? _currentCallback;
	private int _currentGetterCallbacksIndex;
	private Callback? _currentReturnCallback;
	private int _currentReturnCallbackIndex;
	private int _currentSetterCallbacksIndex;
	private Func<T1, TValue>? _initialization;
	private bool? _skipBaseClass;

	/// <inheritdoc cref="IIndexerGetterSetup{TValue, T1}.Do(Action)" />
	IIndexerSetupCallbackBuilder<TValue, T1> IIndexerGetterSetup<TValue, T1>.Do(Action callback)
	{
		Callback<Action<int, T1, TValue>> currentCallback = new(Delegate);
		_currentCallback = currentCallback;
		_getterCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		void Delegate(int _, T1 p1, TValue currentValue)
		{
			callback();
		}
	}

	/// <inheritdoc cref="IIndexerGetterSetup{TValue, T1}.Do(Action{T1})" />
	IIndexerSetupCallbackBuilder<TValue, T1> IIndexerGetterSetup<TValue, T1>.Do(Action<T1> callback)
	{
		Callback<Action<int, T1, TValue>> currentCallback = new(Delegate);
		_currentCallback = currentCallback;
		_getterCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		void Delegate(int _, T1 p1, TValue currentValue)
		{
			callback(p1);
		}
	}

	/// <inheritdoc cref="IIndexerGetterSetup{TValue, T1}.Do(Action{T1, TValue})" />
	IIndexerSetupCallbackBuilder<TValue, T1> IIndexerGetterSetup<TValue, T1>.Do(Action<T1, TValue> callback)
	{
		Callback<Action<int, T1, TValue>> currentCallback = new(Delegate);
		_currentCallback = currentCallback;
		_getterCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		void Delegate(int _, T1 p1, TValue v)
		{
			callback(p1, v);
		}
	}

	/// <inheritdoc cref="IIndexerGetterSetup{TValue, T1}.Do(Action{int, T1, TValue})" />
	IIndexerSetupCallbackBuilder<TValue, T1> IIndexerGetterSetup<TValue, T1>.Do(Action<int, T1, TValue> callback)
	{
		Callback<Action<int, T1, TValue>> currentCallback = new(callback);
		_currentCallback = currentCallback;
		_getterCallbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IIndexerSetterSetup{TValue, T1}.Do(Action)" />
	IIndexerSetupCallbackBuilder<TValue, T1> IIndexerSetterSetup<TValue, T1>.Do(Action callback)
	{
		Callback<Action<int, T1, TValue>> currentCallback = new(Delegate);
		_currentCallback = currentCallback;
		_setterCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		void Delegate(int _, T1 p1, TValue newValue)
		{
			callback();
		}
	}

	/// <inheritdoc cref="IIndexerSetterSetup{TValue, T1}.Do(Action{TValue})" />
	IIndexerSetupCallbackBuilder<TValue, T1> IIndexerSetterSetup<TValue, T1>.Do(Action<TValue> callback)
	{
		Callback<Action<int, T1, TValue>> currentCallback = new(Delegate);
		_currentCallback = currentCallback;
		_setterCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		void Delegate(int _, T1 p1, TValue v)
		{
			callback(v);
		}
	}

	/// <inheritdoc cref="IIndexerSetterSetup{TValue, T1}.Do(Action{T1, TValue})" />
	IIndexerSetupCallbackBuilder<TValue, T1> IIndexerSetterSetup<TValue, T1>.Do(Action<T1, TValue> callback)
	{
		Callback<Action<int, T1, TValue>> currentCallback = new(Delegate);
		_currentCallback = currentCallback;
		_setterCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		void Delegate(int _, T1 p1, TValue v)
		{
			callback(p1, v);
		}
	}

	/// <inheritdoc cref="IIndexerSetterSetup{TValue, T1}.Do(Action{int, T1, TValue})" />
	IIndexerSetupCallbackBuilder<TValue, T1> IIndexerSetterSetup<TValue, T1>.Do(Action<int, T1, TValue> callback)
	{
		Callback<Action<int, T1, TValue>> currentCallback = new(callback);
		_currentCallback = currentCallback;
		_setterCallbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IIndexerSetup{TValue, T1}.SkippingBaseClass(bool)" />
	public IIndexerSetup<TValue, T1> SkippingBaseClass(bool skipBaseClass = true)
	{
		_skipBaseClass = skipBaseClass;
		return this;
	}

	/// <inheritdoc cref="IIndexerSetup{TValue, T1}.InitializeWith(TValue)" />
	public IIndexerSetup<TValue, T1> InitializeWith(TValue value)
	{
		if (_initialization is not null)
		{
			throw new MockException("The indexer is already initialized. You cannot initialize it twice.");
		}

		_initialization = _ => value;
		return this;
	}

	/// <inheritdoc cref="IIndexerSetup{TValue, T1}.InitializeWith(Func{T1, TValue})" />
	public IIndexerSetup<TValue, T1> InitializeWith(Func<T1, TValue> valueGenerator)
	{
		if (_initialization is not null)
		{
			throw new MockException("The indexer is already initialized. You cannot initialize it twice.");
		}

		_initialization = valueGenerator;
		return this;
	}

	/// <inheritdoc cref="IIndexerSetup{TValue, T1}.OnGet" />
	public IIndexerGetterSetup<TValue, T1> OnGet
		=> this;

	/// <inheritdoc cref="IIndexerSetup{TValue, T1}.OnSet" />
	public IIndexerSetterSetup<TValue, T1> OnSet
		=> this;

	/// <inheritdoc cref="IIndexerSetup{TValue, T1}.Returns(TValue)" />
	public IIndexerSetupReturnBuilder<TValue, T1> Returns(TValue returnValue)
	{
		Callback<Func<int, T1, TValue, TValue>> currentCallback = new(Delegate);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TValue Delegate(int _, T1 p1, TValue currentValue)
		{
			return returnValue;
		}
	}

	/// <inheritdoc cref="IIndexerSetup{TValue, T1}.Returns(Func{TValue})" />
	public IIndexerSetupReturnBuilder<TValue, T1> Returns(Func<TValue> callback)
	{
		Callback<Func<int, T1, TValue, TValue>> currentCallback = new(Delegate);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TValue Delegate(int _, T1 p1, TValue currentValue)
		{
			return callback();
		}
	}

	/// <inheritdoc cref="IIndexerSetup{TValue, T1}.Returns(Func{T1, TValue})" />
	public IIndexerSetupReturnBuilder<TValue, T1> Returns(Func<T1, TValue> callback)
	{
		Callback<Func<int, T1, TValue, TValue>> currentCallback = new(Delegate);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TValue Delegate(int _, T1 p1, TValue currentValue)
		{
			return callback(p1);
		}
	}

	/// <inheritdoc cref="IIndexerSetup{TValue, T1}.Returns(Func{T1, TValue, TValue})" />
	public IIndexerSetupReturnBuilder<TValue, T1> Returns(Func<T1, TValue, TValue> callback)
	{
		Callback<Func<int, T1, TValue, TValue>> currentCallback = new(Delegate);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TValue Delegate(int _, T1 p1, TValue v)
		{
			return callback(p1, v);
		}
	}

	/// <inheritdoc cref="IIndexerSetup{TValue, T1}.Throws{TException}()" />
	public IIndexerSetupReturnBuilder<TValue, T1> Throws<TException>()
		where TException : Exception, new()
	{
		Callback<Func<int, T1, TValue, TValue>> currentCallback = new(Delegate);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TValue Delegate(int _, T1 p1, TValue currentValue)
		{
			throw new TException();
		}
	}

	/// <inheritdoc cref="IIndexerSetup{TValue, T1}.Throws(Exception)" />
	public IIndexerSetupReturnBuilder<TValue, T1> Throws(Exception exception)
	{
		Callback<Func<int, T1, TValue, TValue>> currentCallback = new(Delegate);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TValue Delegate(int _, T1 p1, TValue currentValue)
		{
			throw exception;
		}
	}

	/// <inheritdoc cref="IIndexerSetup{TValue, T1}.Throws(Func{Exception})" />
	public IIndexerSetupReturnBuilder<TValue, T1> Throws(Func<Exception> callback)
	{
		Callback<Func<int, T1, TValue, TValue>> currentCallback = new(Delegate);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TValue Delegate(int _, T1 p1, TValue currentValue)
		{
			throw callback();
		}
	}

	/// <inheritdoc cref="IIndexerSetup{TValue, T1}.Throws(Func{T1, Exception})" />
	public IIndexerSetupReturnBuilder<TValue, T1> Throws(Func<T1, Exception> callback)
	{
		Callback<Func<int, T1, TValue, TValue>> currentCallback = new(Delegate);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TValue Delegate(int _, T1 p1, TValue currentValue)
		{
			throw callback(p1);
		}
	}

	/// <inheritdoc cref="IIndexerSetup{TValue, T1}.Throws(Func{T1, TValue, Exception})" />
	public IIndexerSetupReturnBuilder<TValue, T1> Throws(Func<T1, TValue, Exception> callback)
	{
		Callback<Func<int, T1, TValue, TValue>> currentCallback = new(Delegate);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TValue Delegate(int _, T1 p1, TValue v)
		{
			throw callback(p1, v);
		}
	}

	/// <inheritdoc cref="IIndexerSetupCallbackBuilder{TValue, T1}.When(Func{int, bool})" />
	IIndexerSetupCallbackWhenBuilder<TValue, T1> IIndexerSetupCallbackBuilder<TValue, T1>.When(
		Func<int, bool> predicate)
	{
		_currentCallback?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IIndexerSetupCallbackBuilder{TValue, T1}.InParallel()" />
	IIndexerSetupCallbackBuilder<TValue, T1> IIndexerSetupCallbackBuilder<TValue, T1>.InParallel()
	{
		_currentCallback?.InParallel();
		return this;
	}

	/// <inheritdoc cref="IIndexerSetupCallbackWhenBuilder{TValue,T1}.For(int)" />
	IIndexerSetupCallbackWhenBuilder<TValue, T1> IIndexerSetupCallbackWhenBuilder<TValue, T1>.For(int times)
	{
		_currentCallback?.For(times);
		return this;
	}

	/// <inheritdoc cref="IIndexerSetupCallbackWhenBuilder{TValue,T1}.Only(int)" />
	IIndexerSetup<TValue, T1> IIndexerSetupCallbackWhenBuilder<TValue, T1>.Only(int times)
	{
		_currentCallback?.Only(times);
		return this;
	}

	/// <inheritdoc cref="IIndexerSetupReturnBuilder{TValue, T1}.When(Func{int, bool})" />
	IIndexerSetupReturnWhenBuilder<TValue, T1> IIndexerSetupReturnBuilder<TValue, T1>.When(Func<int, bool> predicate)
	{
		_currentReturnCallback?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IIndexerSetupReturnWhenBuilder{TValue,T1}.For(int)" />
	IIndexerSetupReturnWhenBuilder<TValue, T1> IIndexerSetupReturnWhenBuilder<TValue, T1>.For(int times)
	{
		_currentReturnCallback?.For(times);
		return this;
	}

	/// <inheritdoc cref="IIndexerSetupReturnWhenBuilder{TValue,T1}.Only(int)" />
	IIndexerSetup<TValue, T1> IIndexerSetupReturnWhenBuilder<TValue, T1>.Only(int times)
	{
		_currentReturnCallback?.Only(times);
		return this;
	}

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> $"{FormatType(typeof(TValue))} this[{match1}]";

	/// <inheritdoc cref="IndexerSetup.GetSkipBaseClass()" />
	protected override bool? GetSkipBaseClass()
		=> _skipBaseClass;

	/// <inheritdoc cref="ExecuteGetterCallback{TValue}(IndexerGetterAccess, TValue, MockBehavior)" />
	protected override T ExecuteGetterCallback<T>(IndexerGetterAccess indexerGetterAccess, T value,
		MockBehavior behavior)
	{
		if (TryCast(value, out TValue resultValue, behavior) &&
		    indexerGetterAccess.Parameters.Length == 1 &&
		    indexerGetterAccess.Parameters[0].TryGetValue(out T1 p1))
		{
			bool wasInvoked = false;
			int currentGetterCallbacksIndex = _currentGetterCallbacksIndex;
			for (int i = 0; i < _getterCallbacks.Count; i++)
			{
				Callback<Action<int, T1, TValue>> getterCallback =
					_getterCallbacks[(currentGetterCallbacksIndex + i) % _getterCallbacks.Count];
				if (getterCallback.Invoke(wasInvoked, ref _currentGetterCallbacksIndex, (invocationCount, @delegate)
					    => @delegate(invocationCount, p1, resultValue)))
				{
					wasInvoked = true;
				}
			}

			foreach (Callback<Func<int, T1, TValue, TValue>> _ in _returnCallbacks)
			{
				Callback<Func<int, T1, TValue, TValue>> returnCallback =
					_returnCallbacks[_currentReturnCallbackIndex % _returnCallbacks.Count];
				if (returnCallback.Invoke(ref _currentReturnCallbackIndex, (invocationCount, @delegate)
					    => @delegate(invocationCount, p1, resultValue), out TValue? newValue) &&
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
		    indexerSetterAccess.Parameters[0].TryGetValue(out T1 p1))
		{
			bool wasInvoked = false;
			int currentSetterCallbacksIndex = _currentSetterCallbacksIndex;
			for (int i = 0; i < _setterCallbacks.Count; i++)
			{
				Callback<Action<int, T1, TValue>> setterCallback =
					_setterCallbacks[(currentSetterCallbacksIndex + i) % _setterCallbacks.Count];
				if (setterCallback.Invoke(wasInvoked, ref _currentSetterCallbacksIndex, (invocationCount, @delegate)
					    => @delegate(invocationCount, p1, resultValue)))
				{
					wasInvoked = true;
				}
			}
		}
	}

	/// <inheritdoc cref="IsMatch(INamedParameterValue[])" />
	protected override bool IsMatch(INamedParameterValue[] parameters)
		=> Matches([match1,], parameters);

	/// <inheritdoc cref="ITypedIndexerMatch.MatchesTyped{T1}(string, T1)" />
	bool ITypedIndexerMatch.MatchesTyped<TActual1>(string n1, TActual1 v1)
	{
		if (!MatchesParameter(match1, n1, v1))
		{
			return false;
		}

		InvokeCallbacksParameter(match1, n1, v1);
		return true;
	}

	/// <inheritdoc cref="ITypedIndexerMatch.MatchesTyped{T1,T2}(string, T1, string, T2)" />
	bool ITypedIndexerMatch.MatchesTyped<TActual1, TActual2>(string n1, TActual1 v1, string n2, TActual2 v2) => false;

	/// <inheritdoc cref="ITypedIndexerMatch.MatchesTyped{T1,T2,T3}(string, T1, string, T2, string, T3)" />
	bool ITypedIndexerMatch.MatchesTyped<TActual1, TActual2, TActual3>(
		string n1, TActual1 v1, string n2, TActual2 v2, string n3, TActual3 v3) => false;

	/// <inheritdoc cref="ITypedIndexerMatch.MatchesTyped{T1,T2,T3,T4}(string, T1, string, T2, string, T3, string, T4)" />
	bool ITypedIndexerMatch.MatchesTyped<TActual1, TActual2, TActual3, TActual4>(
		string n1, TActual1 v1, string n2, TActual2 v2, string n3, TActual3 v3, string n4, TActual4 v4) => false;

	/// <inheritdoc cref="IndexerSetup.GetInitialValue{T}" />
	protected override void GetInitialValue<T>(MockBehavior behavior, Func<T> defaultValueGenerator,
		INamedParameterValue[] parameters,
		[NotNullWhen(true)] out T value)
	{
		if (_initialization is not null &&
		    parameters.Length == 1 &&
		    parameters[0].TryGetValue(out T1 p1) &&
		    _initialization.Invoke(p1) is T initialValue)
		{
			value = initialValue;
			return;
		}

		value = defaultValueGenerator();
	}
}

/// <summary>
///     Sets up a <typeparamref name="TValue" /> indexer for <typeparamref name="T1" /> and <typeparamref name="T2" />.
/// </summary>
#if !DEBUG
[DebuggerNonUserCode]
#endif
public class IndexerSetup<TValue, T1, T2>(NamedParameter match1, NamedParameter match2) : IndexerSetup
	, IIndexerSetupCallbackBuilder<TValue, T1, T2>, IIndexerSetupReturnBuilder<TValue, T1, T2>,
	IIndexerGetterSetup<TValue, T1, T2>, IIndexerSetterSetup<TValue, T1, T2>, ITypedIndexerMatch
{
	private readonly List<Callback<Action<int, T1, T2, TValue>>> _getterCallbacks = [];
	private readonly List<Callback<Func<int, T1, T2, TValue, TValue>>> _returnCallbacks = [];
	private readonly List<Callback<Action<int, T1, T2, TValue>>> _setterCallbacks = [];
	private Callback? _currentCallback;
	private int _currentGetterCallbacksIndex;
	private Callback? _currentReturnCallback;
	private int _currentReturnCallbackIndex;
	private int _currentSetterCallbacksIndex;
	private Func<T1, T2, TValue>? _initialization;
	private bool? _skipBaseClass;

	/// <inheritdoc cref="IIndexerGetterSetup{TValue, T1, T2}.Do(Action)" />
	IIndexerSetupCallbackBuilder<TValue, T1, T2> IIndexerGetterSetup<TValue, T1, T2>.Do(Action callback)
	{
		Callback<Action<int, T1, T2, TValue>> currentCallback = new(Delegate);
		_currentCallback = currentCallback;
		_getterCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		void Delegate(int _, T1 p1, T2 p2, TValue currentValue)
		{
			callback();
		}
	}

	/// <inheritdoc cref="IIndexerGetterSetup{TValue, T1, T2}.Do(Action{T1, T2})" />
	IIndexerSetupCallbackBuilder<TValue, T1, T2> IIndexerGetterSetup<TValue, T1, T2>.Do(Action<T1, T2> callback)
	{
		Callback<Action<int, T1, T2, TValue>> currentCallback = new(Delegate);
		_currentCallback = currentCallback;
		_getterCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		void Delegate(int _, T1 p1, T2 p2, TValue currentValue)
		{
			callback(p1, p2);
		}
	}

	/// <inheritdoc cref="IIndexerGetterSetup{TValue, T1, T2}.Do(Action{T1, T2, TValue})" />
	IIndexerSetupCallbackBuilder<TValue, T1, T2> IIndexerGetterSetup<TValue, T1, T2>.Do(Action<T1, T2, TValue> callback)
	{
		Callback<Action<int, T1, T2, TValue>> currentCallback = new(Delegate);
		_currentCallback = currentCallback;
		_getterCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		void Delegate(int _, T1 p1, T2 p2, TValue v)
		{
			callback(p1, p2, v);
		}
	}

	/// <inheritdoc cref="IIndexerGetterSetup{TValue, T1, T2}.Do(Action{int, T1, T2, TValue})" />
	IIndexerSetupCallbackBuilder<TValue, T1, T2> IIndexerGetterSetup<TValue, T1, T2>.Do(
		Action<int, T1, T2, TValue> callback)
	{
		Callback<Action<int, T1, T2, TValue>> currentCallback = new(callback);
		_currentCallback = currentCallback;
		_getterCallbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IIndexerSetterSetup{TValue, T1, T2}.Do(Action)" />
	IIndexerSetupCallbackBuilder<TValue, T1, T2> IIndexerSetterSetup<TValue, T1, T2>.Do(Action callback)
	{
		Callback<Action<int, T1, T2, TValue>> currentCallback = new(Delegate);
		_currentCallback = currentCallback;
		_setterCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		void Delegate(int _, T1 p1, T2 p2, TValue newValue)
		{
			callback();
		}
	}

	/// <inheritdoc cref="IIndexerSetterSetup{TValue, T1, T2}.Do(Action{TValue})" />
	IIndexerSetupCallbackBuilder<TValue, T1, T2> IIndexerSetterSetup<TValue, T1, T2>.Do(Action<TValue> callback)
	{
		Callback<Action<int, T1, T2, TValue>> currentCallback = new(Delegate);
		_currentCallback = currentCallback;
		_setterCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		void Delegate(int _, T1 p1, T2 p2, TValue v)
		{
			callback(v);
		}
	}

	/// <inheritdoc cref="IIndexerSetterSetup{TValue, T1, T2}.Do(Action{T1, T2, TValue})" />
	IIndexerSetupCallbackBuilder<TValue, T1, T2> IIndexerSetterSetup<TValue, T1, T2>.Do(Action<T1, T2, TValue> callback)
	{
		Callback<Action<int, T1, T2, TValue>> currentCallback = new(Delegate);
		_currentCallback = currentCallback;
		_setterCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		void Delegate(int _, T1 p1, T2 p2, TValue v)
		{
			callback(p1, p2, v);
		}
	}

	/// <inheritdoc cref="IIndexerSetterSetup{TValue, T1, T2}.Do(Action{int, T1, T2, TValue})" />
	IIndexerSetupCallbackBuilder<TValue, T1, T2> IIndexerSetterSetup<TValue, T1, T2>.Do(
		Action<int, T1, T2, TValue> callback)
	{
		Callback<Action<int, T1, T2, TValue>> currentCallback = new(callback);
		_currentCallback = currentCallback;
		_setterCallbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2}.SkippingBaseClass(bool)" />
	public IIndexerSetup<TValue, T1, T2> SkippingBaseClass(bool skipBaseClass = true)
	{
		_skipBaseClass = skipBaseClass;
		return this;
	}

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2}.InitializeWith(TValue)" />
	public IIndexerSetup<TValue, T1, T2> InitializeWith(TValue value)
	{
		if (_initialization is not null)
		{
			throw new MockException("The indexer is already initialized. You cannot initialize it twice.");
		}

		_initialization = (_, _) => value;
		return this;
	}

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2}.InitializeWith(Func{T1, T2, TValue})" />
	public IIndexerSetup<TValue, T1, T2> InitializeWith(Func<T1, T2, TValue> valueGenerator)
	{
		if (_initialization is not null)
		{
			throw new MockException("The indexer is already initialized. You cannot initialize it twice.");
		}

		_initialization = valueGenerator;
		return this;
	}

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2}.OnGet" />
	public IIndexerGetterSetup<TValue, T1, T2> OnGet
		=> this;

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2}.OnSet" />
	public IIndexerSetterSetup<TValue, T1, T2> OnSet
		=> this;

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2}.Returns(TValue)" />
	public IIndexerSetupReturnBuilder<TValue, T1, T2> Returns(TValue returnValue)
	{
		Callback<Func<int, T1, T2, TValue, TValue>> currentCallback = new(Delegate);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TValue Delegate(int _, T1 p1, T2 p2, TValue currentValue)
		{
			return returnValue;
		}
	}

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2}.Returns(Func{TValue})" />
	public IIndexerSetupReturnBuilder<TValue, T1, T2> Returns(Func<TValue> callback)
	{
		Callback<Func<int, T1, T2, TValue, TValue>> currentCallback = new(Delegate);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TValue Delegate(int _, T1 p1, T2 p2, TValue currentValue)
		{
			return callback();
		}
	}

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2}.Returns(Func{T1, T2, TValue})" />
	public IIndexerSetupReturnBuilder<TValue, T1, T2> Returns(Func<T1, T2, TValue> callback)
	{
		Callback<Func<int, T1, T2, TValue, TValue>> currentCallback = new(Delegate);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TValue Delegate(int _, T1 p1, T2 p2, TValue currentValue)
		{
			return callback(p1, p2);
		}
	}

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2}.Returns(Func{T1, T2, TValue, TValue})" />
	public IIndexerSetupReturnBuilder<TValue, T1, T2> Returns(Func<T1, T2, TValue, TValue> callback)
	{
		Callback<Func<int, T1, T2, TValue, TValue>> currentCallback = new(Delegate);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TValue Delegate(int _, T1 p1, T2 p2, TValue v)
		{
			return callback(p1, p2, v);
		}
	}

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2}.Throws{TException}()" />
	public IIndexerSetupReturnBuilder<TValue, T1, T2> Throws<TException>()
		where TException : Exception, new()
	{
		Callback<Func<int, T1, T2, TValue, TValue>> currentCallback = new(Delegate);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TValue Delegate(int _, T1 p1, T2 p2, TValue currentValue)
		{
			throw new TException();
		}
	}

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2}.Throws(Exception)" />
	public IIndexerSetupReturnBuilder<TValue, T1, T2> Throws(Exception exception)
	{
		Callback<Func<int, T1, T2, TValue, TValue>> currentCallback = new(Delegate);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TValue Delegate(int _, T1 p1, T2 p2, TValue currentValue)
		{
			throw exception;
		}
	}

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2}.Throws(Func{Exception})" />
	public IIndexerSetupReturnBuilder<TValue, T1, T2> Throws(Func<Exception> callback)
	{
		Callback<Func<int, T1, T2, TValue, TValue>> currentCallback = new(Delegate);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TValue Delegate(int _, T1 p1, T2 p2, TValue currentValue)
		{
			throw callback();
		}
	}

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2}.Throws(Func{T1, T2, Exception})" />
	public IIndexerSetupReturnBuilder<TValue, T1, T2> Throws(Func<T1, T2, Exception> callback)
	{
		Callback<Func<int, T1, T2, TValue, TValue>> currentCallback = new(Delegate);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TValue Delegate(int _, T1 p1, T2 p2, TValue currentValue)
		{
			throw callback(p1, p2);
		}
	}

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2}.Throws(Func{T1, T2, TValue, Exception})" />
	public IIndexerSetupReturnBuilder<TValue, T1, T2> Throws(Func<T1, T2, TValue, Exception> callback)
	{
		Callback<Func<int, T1, T2, TValue, TValue>> currentCallback = new(Delegate);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TValue Delegate(int _, T1 p1, T2 p2, TValue v)
		{
			throw callback(p1, p2, v);
		}
	}

	/// <inheritdoc cref="IIndexerSetupCallbackBuilder{TValue, T1, T2}.When(Func{int, bool})" />
	IIndexerSetupCallbackWhenBuilder<TValue, T1, T2> IIndexerSetupCallbackBuilder<TValue, T1, T2>.When(
		Func<int, bool> predicate)
	{
		_currentCallback?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IIndexerSetupCallbackBuilder{TValue, T1, T2}.InParallel()" />
	IIndexerSetupCallbackBuilder<TValue, T1, T2> IIndexerSetupCallbackBuilder<TValue, T1, T2>.InParallel()
	{
		_currentCallback?.InParallel();
		return this;
	}

	/// <inheritdoc cref="IIndexerSetupCallbackWhenBuilder{TValue,T1, T2}.For(int)" />
	IIndexerSetupCallbackWhenBuilder<TValue, T1, T2> IIndexerSetupCallbackWhenBuilder<TValue, T1, T2>.For(int times)
	{
		_currentCallback?.For(times);
		return this;
	}

	/// <inheritdoc cref="IIndexerSetupCallbackWhenBuilder{TValue,T1, T2}.Only(int)" />
	IIndexerSetup<TValue, T1, T2> IIndexerSetupCallbackWhenBuilder<TValue, T1, T2>.Only(int times)
	{
		_currentCallback?.Only(times);
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
	IIndexerSetupReturnWhenBuilder<TValue, T1, T2> IIndexerSetupReturnWhenBuilder<TValue, T1, T2>.For(int times)
	{
		_currentReturnCallback?.For(times);
		return this;
	}

	/// <inheritdoc cref="IIndexerSetupReturnWhenBuilder{TValue,T1, T2}.Only(int)" />
	IIndexerSetup<TValue, T1, T2> IIndexerSetupReturnWhenBuilder<TValue, T1, T2>.Only(int times)
	{
		_currentReturnCallback?.Only(times);
		return this;
	}

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> $"{FormatType(typeof(TValue))} this[{match1}, {match2}]";

	/// <inheritdoc cref="IndexerSetup.GetSkipBaseClass()" />
	protected override bool? GetSkipBaseClass()
		=> _skipBaseClass;

	/// <inheritdoc cref="ExecuteGetterCallback{TValue}(IndexerGetterAccess, TValue, MockBehavior)" />
	protected override T ExecuteGetterCallback<T>(IndexerGetterAccess indexerGetterAccess, T value,
		MockBehavior behavior)
	{
		if (TryCast(value, out TValue resultValue, behavior) &&
		    indexerGetterAccess.Parameters.Length == 2 &&
		    indexerGetterAccess.Parameters[0].TryGetValue(out T1 p1) &&
		    indexerGetterAccess.Parameters[1].TryGetValue(out T2 p2))
		{
			bool wasInvoked = false;
			int currentGetterCallbacksIndex = _currentGetterCallbacksIndex;
			for (int i = 0; i < _getterCallbacks.Count; i++)
			{
				Callback<Action<int, T1, T2, TValue>> getterCallback =
					_getterCallbacks[(currentGetterCallbacksIndex + i) % _getterCallbacks.Count];
				if (getterCallback.Invoke(wasInvoked, ref _currentGetterCallbacksIndex, (invocationCount, @delegate)
					    => @delegate(invocationCount, p1, p2, resultValue)))
				{
					wasInvoked = true;
				}
			}

			foreach (Callback<Func<int, T1, T2, TValue, TValue>> _ in _returnCallbacks)
			{
				Callback<Func<int, T1, T2, TValue, TValue>> returnCallback =
					_returnCallbacks[_currentReturnCallbackIndex % _returnCallbacks.Count];
				if (returnCallback.Invoke(ref _currentReturnCallbackIndex, (invocationCount, @delegate)
					    => @delegate(invocationCount, p1, p2, resultValue), out TValue? newValue) &&
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
		    indexerSetterAccess.Parameters[0].TryGetValue(out T1 p1) &&
		    indexerSetterAccess.Parameters[1].TryGetValue(out T2 p2))
		{
			bool wasInvoked = false;
			int currentSetterCallbacksIndex = _currentSetterCallbacksIndex;
			for (int i = 0; i < _setterCallbacks.Count; i++)
			{
				Callback<Action<int, T1, T2, TValue>> setterCallback =
					_setterCallbacks[(currentSetterCallbacksIndex + i) % _setterCallbacks.Count];
				if (setterCallback.Invoke(wasInvoked, ref _currentSetterCallbacksIndex, (invocationCount, @delegate)
					    => @delegate(invocationCount, p1, p2, resultValue)))
				{
					wasInvoked = true;
				}
			}
		}
	}

	/// <inheritdoc cref="IsMatch(INamedParameterValue[])" />
	protected override bool IsMatch(INamedParameterValue[] parameters)
		=> Matches([match1, match2,], parameters);

	/// <inheritdoc cref="ITypedIndexerMatch.MatchesTyped{T1}(string, T1)" />
	bool ITypedIndexerMatch.MatchesTyped<TActual1>(string n1, TActual1 v1) => false;

	/// <inheritdoc cref="ITypedIndexerMatch.MatchesTyped{T1,T2}(string, T1, string, T2)" />
	bool ITypedIndexerMatch.MatchesTyped<TActual1, TActual2>(string n1, TActual1 v1, string n2, TActual2 v2)
	{
		if (!MatchesParameter(match1, n1, v1) || !MatchesParameter(match2, n2, v2))
		{
			return false;
		}

		InvokeCallbacksParameter(match1, n1, v1);
		InvokeCallbacksParameter(match2, n2, v2);
		return true;
	}

	/// <inheritdoc cref="ITypedIndexerMatch.MatchesTyped{T1,T2,T3}(string, T1, string, T2, string, T3)" />
	bool ITypedIndexerMatch.MatchesTyped<TActual1, TActual2, TActual3>(
		string n1, TActual1 v1, string n2, TActual2 v2, string n3, TActual3 v3) => false;

	/// <inheritdoc cref="ITypedIndexerMatch.MatchesTyped{T1,T2,T3,T4}(string, T1, string, T2, string, T3, string, T4)" />
	bool ITypedIndexerMatch.MatchesTyped<TActual1, TActual2, TActual3, TActual4>(
		string n1, TActual1 v1, string n2, TActual2 v2, string n3, TActual3 v3, string n4, TActual4 v4) => false;

	/// <inheritdoc cref="IndexerSetup.GetInitialValue{T}" />
	protected override void GetInitialValue<T>(MockBehavior behavior, Func<T> defaultValueGenerator,
		INamedParameterValue[] parameters,
		[NotNullWhen(true)] out T value)
	{
		if (_initialization is not null &&
		    parameters.Length == 2 &&
		    parameters[0].TryGetValue(out T1 p1) &&
		    parameters[1].TryGetValue(out T2 p2) &&
		    _initialization.Invoke(p1, p2) is T initialValue)
		{
			value = initialValue;
			return;
		}

		value = defaultValueGenerator();
	}
}

/// <summary>
///     Sets up a <typeparamref name="TValue" /> indexer for <typeparamref name="T1" />, <typeparamref name="T2" /> and
///     <typeparamref name="T3" />.
/// </summary>
#if !DEBUG
[DebuggerNonUserCode]
#endif
public class IndexerSetup<TValue, T1, T2, T3>(
	NamedParameter match1,
	NamedParameter match2,
	NamedParameter match3) : IndexerSetup,
	IIndexerSetupCallbackBuilder<TValue, T1, T2, T3>, IIndexerSetupReturnBuilder<TValue, T1, T2, T3>,
	IIndexerGetterSetup<TValue, T1, T2, T3>, IIndexerSetterSetup<TValue, T1, T2, T3>, ITypedIndexerMatch
{
	private readonly List<Callback<Action<int, T1, T2, T3, TValue>>> _getterCallbacks = [];
	private readonly List<Callback<Func<int, T1, T2, T3, TValue, TValue>>> _returnCallbacks = [];
	private readonly List<Callback<Action<int, T1, T2, T3, TValue>>> _setterCallbacks = [];
	private Callback? _currentCallback;
	private int _currentGetterCallbacksIndex;
	private Callback? _currentReturnCallback;
	private int _currentReturnCallbackIndex;
	private int _currentSetterCallbacksIndex;
	private Func<T1, T2, T3, TValue>? _initialization;
	private bool? _skipBaseClass;

	/// <inheritdoc cref="IIndexerGetterSetup{TValue, T1, T2, T3}.Do(Action)" />
	IIndexerSetupCallbackBuilder<TValue, T1, T2, T3> IIndexerGetterSetup<TValue, T1, T2, T3>.Do(Action callback)
	{
		Callback<Action<int, T1, T2, T3, TValue>> currentCallback = new(Delegate);
		_currentCallback = currentCallback;
		_getterCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		void Delegate(int _, T1 p1, T2 p2, T3 p3, TValue currentValue)
		{
			callback();
		}
	}

	/// <inheritdoc cref="IIndexerGetterSetup{TValue, T1, T2, T3}.Do(Action{T1, T2, T3})" />
	IIndexerSetupCallbackBuilder<TValue, T1, T2, T3> IIndexerGetterSetup<TValue, T1, T2, T3>.Do(
		Action<T1, T2, T3> callback)
	{
		Callback<Action<int, T1, T2, T3, TValue>> currentCallback = new(Delegate);
		_currentCallback = currentCallback;
		_getterCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		void Delegate(int _, T1 p1, T2 p2, T3 p3, TValue currentValue)
		{
			callback(p1, p2, p3);
		}
	}

	/// <inheritdoc cref="IIndexerGetterSetup{TValue, T1, T2, T3}.Do(Action{T1, T2, T3, TValue})" />
	IIndexerSetupCallbackBuilder<TValue, T1, T2, T3> IIndexerGetterSetup<TValue, T1, T2, T3>.Do(
		Action<T1, T2, T3, TValue> callback)
	{
		Callback<Action<int, T1, T2, T3, TValue>> currentCallback = new(Delegate);
		_currentCallback = currentCallback;
		_getterCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		void Delegate(int _, T1 p1, T2 p2, T3 p3, TValue v)
		{
			callback(p1, p2, p3, v);
		}
	}

	/// <inheritdoc cref="IIndexerGetterSetup{TValue, T1, T2, T3}.Do(Action{int, T1, T2, T3, TValue})" />
	IIndexerSetupCallbackBuilder<TValue, T1, T2, T3> IIndexerGetterSetup<TValue, T1, T2, T3>.Do(
		Action<int, T1, T2, T3, TValue> callback)
	{
		Callback<Action<int, T1, T2, T3, TValue>> currentCallback = new(callback);
		_currentCallback = currentCallback;
		_getterCallbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IIndexerSetterSetup{TValue, T1, T2, T3}.Do(Action)" />
	IIndexerSetupCallbackBuilder<TValue, T1, T2, T3> IIndexerSetterSetup<TValue, T1, T2, T3>.Do(Action callback)
	{
		Callback<Action<int, T1, T2, T3, TValue>> currentCallback = new(Delegate);
		_currentCallback = currentCallback;
		_setterCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		void Delegate(int _, T1 p1, T2 p2, T3 p3, TValue newValue)
		{
			callback();
		}
	}

	/// <inheritdoc cref="IIndexerSetterSetup{TValue, T1, T2, T3}.Do(Action{TValue})" />
	IIndexerSetupCallbackBuilder<TValue, T1, T2, T3> IIndexerSetterSetup<TValue, T1, T2, T3>.Do(Action<TValue> callback)
	{
		Callback<Action<int, T1, T2, T3, TValue>> currentCallback = new(Delegate);
		_currentCallback = currentCallback;
		_setterCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		void Delegate(int _, T1 p1, T2 p2, T3 p3, TValue v)
		{
			callback(v);
		}
	}

	/// <inheritdoc cref="IIndexerSetterSetup{TValue, T1, T2, T3}.Do(Action{T1, T2, T3, TValue})" />
	IIndexerSetupCallbackBuilder<TValue, T1, T2, T3> IIndexerSetterSetup<TValue, T1, T2, T3>.Do(
		Action<T1, T2, T3, TValue> callback)
	{
		Callback<Action<int, T1, T2, T3, TValue>> currentCallback = new(Delegate);
		_currentCallback = currentCallback;
		_setterCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		void Delegate(int _, T1 p1, T2 p2, T3 p3, TValue v)
		{
			callback(p1, p2, p3, v);
		}
	}

	/// <inheritdoc cref="IIndexerSetterSetup{TValue, T1, T2, T3}.Do(Action{int, T1, T2, T3, TValue})" />
	IIndexerSetupCallbackBuilder<TValue, T1, T2, T3> IIndexerSetterSetup<TValue, T1, T2, T3>.Do(
		Action<int, T1, T2, T3, TValue> callback)
	{
		Callback<Action<int, T1, T2, T3, TValue>> currentCallback = new(callback);
		_currentCallback = currentCallback;
		_setterCallbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2, T3}.SkippingBaseClass(bool)" />
	public IIndexerSetup<TValue, T1, T2, T3> SkippingBaseClass(bool skipBaseClass = true)
	{
		_skipBaseClass = skipBaseClass;
		return this;
	}

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2, T3}.InitializeWith(TValue)" />
	public IIndexerSetup<TValue, T1, T2, T3> InitializeWith(TValue value)
	{
		if (_initialization is not null)
		{
			throw new MockException("The indexer is already initialized. You cannot initialize it twice.");
		}

		_initialization = (_, _, _) => value;
		return this;
	}

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2, T3}.InitializeWith(Func{T1, T2, T3, TValue})" />
	public IIndexerSetup<TValue, T1, T2, T3> InitializeWith(Func<T1, T2, T3, TValue> valueGenerator)
	{
		if (_initialization is not null)
		{
			throw new MockException("The indexer is already initialized. You cannot initialize it twice.");
		}

		_initialization = valueGenerator;
		return this;
	}

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2, T3}.OnGet" />
	public IIndexerGetterSetup<TValue, T1, T2, T3> OnGet
		=> this;

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2, T3}.OnSet" />
	public IIndexerSetterSetup<TValue, T1, T2, T3> OnSet
		=> this;

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2, T3}.Returns(TValue)" />
	public IIndexerSetupReturnBuilder<TValue, T1, T2, T3> Returns(TValue returnValue)
	{
		Callback<Func<int, T1, T2, T3, TValue, TValue>> currentCallback = new(Delegate);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TValue Delegate(int _, T1 p1, T2 p2, T3 p3, TValue currentValue)
		{
			return returnValue;
		}
	}

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2, T3}.Returns(Func{TValue})" />
	public IIndexerSetupReturnBuilder<TValue, T1, T2, T3> Returns(Func<TValue> callback)
	{
		Callback<Func<int, T1, T2, T3, TValue, TValue>> currentCallback = new(Delegate);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TValue Delegate(int _, T1 p1, T2 p2, T3 p3, TValue currentValue)
		{
			return callback();
		}
	}

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2, T3}.Returns(Func{T1, T2, T3, TValue})" />
	public IIndexerSetupReturnBuilder<TValue, T1, T2, T3> Returns(Func<T1, T2, T3, TValue> callback)
	{
		Callback<Func<int, T1, T2, T3, TValue, TValue>> currentCallback = new(Delegate);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TValue Delegate(int _, T1 p1, T2 p2, T3 p3, TValue currentValue)
		{
			return callback(p1, p2, p3);
		}
	}

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2, T3}.Returns(Func{T1, T2, T3, TValue, TValue})" />
	public IIndexerSetupReturnBuilder<TValue, T1, T2, T3> Returns(Func<T1, T2, T3, TValue, TValue> callback)
	{
		Callback<Func<int, T1, T2, T3, TValue, TValue>> currentCallback = new(Delegate);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TValue Delegate(int _, T1 p1, T2 p2, T3 p3, TValue v)
		{
			return callback(p1, p2, p3, v);
		}
	}

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2, T3}.Throws{TException}()" />
	public IIndexerSetupReturnBuilder<TValue, T1, T2, T3> Throws<TException>()
		where TException : Exception, new()
	{
		Callback<Func<int, T1, T2, T3, TValue, TValue>> currentCallback = new(Delegate);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TValue Delegate(int _, T1 p1, T2 p2, T3 p3, TValue currentValue)
		{
			throw new TException();
		}
	}

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2, T3}.Throws(Exception)" />
	public IIndexerSetupReturnBuilder<TValue, T1, T2, T3> Throws(Exception exception)
	{
		Callback<Func<int, T1, T2, T3, TValue, TValue>> currentCallback = new(Delegate);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TValue Delegate(int _, T1 p1, T2 p2, T3 p3, TValue currentValue)
		{
			throw exception;
		}
	}

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2, T3}.Throws(Func{Exception})" />
	public IIndexerSetupReturnBuilder<TValue, T1, T2, T3> Throws(Func<Exception> callback)
	{
		Callback<Func<int, T1, T2, T3, TValue, TValue>> currentCallback = new(Delegate);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TValue Delegate(int _, T1 p1, T2 p2, T3 p3, TValue currentValue)
		{
			throw callback();
		}
	}

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2, T3}.Throws(Func{T1, T2, T3, Exception})" />
	public IIndexerSetupReturnBuilder<TValue, T1, T2, T3> Throws(Func<T1, T2, T3, Exception> callback)
	{
		Callback<Func<int, T1, T2, T3, TValue, TValue>> currentCallback = new(Delegate);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TValue Delegate(int _, T1 p1, T2 p2, T3 p3, TValue currentValue)
		{
			throw callback(p1, p2, p3);
		}
	}

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2, T3}.Throws(Func{T1, T2, T3, TValue, Exception})" />
	public IIndexerSetupReturnBuilder<TValue, T1, T2, T3> Throws(Func<T1, T2, T3, TValue, Exception> callback)
	{
		Callback<Func<int, T1, T2, T3, TValue, TValue>> currentCallback = new(Delegate);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TValue Delegate(int _, T1 p1, T2 p2, T3 p3, TValue v)
		{
			throw callback(p1, p2, p3, v);
		}
	}

	/// <inheritdoc cref="IIndexerSetupCallbackBuilder{TValue, T1, T2, T3}.When(Func{int, bool})" />
	IIndexerSetupCallbackWhenBuilder<TValue, T1, T2, T3> IIndexerSetupCallbackBuilder<TValue, T1, T2, T3>.When(
		Func<int, bool> predicate)
	{
		_currentCallback?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IIndexerSetupCallbackBuilder{TValue, T1, T2, T3}.InParallel()" />
	IIndexerSetupCallbackBuilder<TValue, T1, T2, T3> IIndexerSetupCallbackBuilder<TValue, T1, T2, T3>.InParallel()
	{
		_currentCallback?.InParallel();
		return this;
	}

	/// <inheritdoc cref="IIndexerSetupCallbackWhenBuilder{TValue,T1, T2, T3}.For(int)" />
	IIndexerSetupCallbackWhenBuilder<TValue, T1, T2, T3> IIndexerSetupCallbackWhenBuilder<TValue, T1, T2, T3>.
		For(int times)
	{
		_currentCallback?.For(times);
		return this;
	}

	/// <inheritdoc cref="IIndexerSetupCallbackWhenBuilder{TValue,T1, T2, T3}.Only(int)" />
	IIndexerSetup<TValue, T1, T2, T3> IIndexerSetupCallbackWhenBuilder<TValue, T1, T2, T3>.Only(int times)
	{
		_currentCallback?.Only(times);
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
	IIndexerSetupReturnWhenBuilder<TValue, T1, T2, T3> IIndexerSetupReturnWhenBuilder<TValue, T1, T2, T3>.For(int times)
	{
		_currentReturnCallback?.For(times);
		return this;
	}

	/// <inheritdoc cref="IIndexerSetupReturnWhenBuilder{TValue,T1, T2, T3}.Only(int)" />
	IIndexerSetup<TValue, T1, T2, T3> IIndexerSetupReturnWhenBuilder<TValue, T1, T2, T3>.Only(int times)
	{
		_currentReturnCallback?.Only(times);
		return this;
	}

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> $"{FormatType(typeof(TValue))} this[{match1}, {match2}, {match3}]";

	/// <inheritdoc cref="IndexerSetup.GetSkipBaseClass()" />
	protected override bool? GetSkipBaseClass()
		=> _skipBaseClass;

	/// <inheritdoc cref="ExecuteGetterCallback{TValue}(IndexerGetterAccess, TValue, MockBehavior)" />
	protected override T ExecuteGetterCallback<T>(IndexerGetterAccess indexerGetterAccess, T value,
		MockBehavior behavior)
	{
		if (TryCast(value, out TValue resultValue, behavior) &&
		    indexerGetterAccess.Parameters.Length == 3 &&
		    indexerGetterAccess.Parameters[0].TryGetValue(out T1 p1) &&
		    indexerGetterAccess.Parameters[1].TryGetValue(out T2 p2) &&
		    indexerGetterAccess.Parameters[2].TryGetValue(out T3 p3))
		{
			bool wasInvoked = false;
			int currentGetterCallbacksIndex = _currentGetterCallbacksIndex;
			for (int i = 0; i < _getterCallbacks.Count; i++)
			{
				Callback<Action<int, T1, T2, T3, TValue>> getterCallback =
					_getterCallbacks[(currentGetterCallbacksIndex + i) % _getterCallbacks.Count];
				if (getterCallback.Invoke(wasInvoked, ref _currentGetterCallbacksIndex, (invocationCount, @delegate)
					    => @delegate(invocationCount, p1, p2, p3, resultValue)))
				{
					wasInvoked = true;
				}
			}

			foreach (Callback<Func<int, T1, T2, T3, TValue, TValue>> _ in _returnCallbacks)
			{
				Callback<Func<int, T1, T2, T3, TValue, TValue>> returnCallback =
					_returnCallbacks[_currentReturnCallbackIndex % _returnCallbacks.Count];
				if (returnCallback.Invoke(ref _currentReturnCallbackIndex, (invocationCount, @delegate)
					    => @delegate(invocationCount, p1, p2, p3, resultValue), out TValue? newValue) &&
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
		    indexerSetterAccess.Parameters[0].TryGetValue(out T1 p1) &&
		    indexerSetterAccess.Parameters[1].TryGetValue(out T2 p2) &&
		    indexerSetterAccess.Parameters[2].TryGetValue(out T3 p3))
		{
			bool wasInvoked = false;
			int currentSetterCallbacksIndex = _currentSetterCallbacksIndex;
			for (int i = 0; i < _setterCallbacks.Count; i++)
			{
				Callback<Action<int, T1, T2, T3, TValue>> setterCallback =
					_setterCallbacks[(currentSetterCallbacksIndex + i) % _setterCallbacks.Count];
				if (setterCallback.Invoke(wasInvoked, ref _currentSetterCallbacksIndex, (invocationCount, @delegate)
					    => @delegate(invocationCount, p1, p2, p3, resultValue)))
				{
					wasInvoked = true;
				}
			}
		}
	}

	/// <inheritdoc cref="IsMatch(INamedParameterValue[])" />
	protected override bool IsMatch(INamedParameterValue[] parameters)
		=> Matches([match1, match2, match3,], parameters);

	/// <inheritdoc cref="ITypedIndexerMatch.MatchesTyped{T1}(string, T1)" />
	bool ITypedIndexerMatch.MatchesTyped<TActual1>(string n1, TActual1 v1) => false;

	/// <inheritdoc cref="ITypedIndexerMatch.MatchesTyped{T1,T2}(string, T1, string, T2)" />
	bool ITypedIndexerMatch.MatchesTyped<TActual1, TActual2>(string n1, TActual1 v1, string n2, TActual2 v2) => false;

	/// <inheritdoc cref="ITypedIndexerMatch.MatchesTyped{T1,T2,T3}(string, T1, string, T2, string, T3)" />
	bool ITypedIndexerMatch.MatchesTyped<TActual1, TActual2, TActual3>(
		string n1, TActual1 v1, string n2, TActual2 v2, string n3, TActual3 v3)
	{
		if (!MatchesParameter(match1, n1, v1) || !MatchesParameter(match2, n2, v2) ||
		    !MatchesParameter(match3, n3, v3))
		{
			return false;
		}

		InvokeCallbacksParameter(match1, n1, v1);
		InvokeCallbacksParameter(match2, n2, v2);
		InvokeCallbacksParameter(match3, n3, v3);
		return true;
	}

	/// <inheritdoc cref="ITypedIndexerMatch.MatchesTyped{T1,T2,T3,T4}(string, T1, string, T2, string, T3, string, T4)" />
	bool ITypedIndexerMatch.MatchesTyped<TActual1, TActual2, TActual3, TActual4>(
		string n1, TActual1 v1, string n2, TActual2 v2, string n3, TActual3 v3, string n4, TActual4 v4) => false;

	/// <inheritdoc cref="IndexerSetup.GetInitialValue{T}" />
	protected override void GetInitialValue<T>(MockBehavior behavior, Func<T> defaultValueGenerator,
		INamedParameterValue[] parameters,
		[NotNullWhen(true)] out T value)
	{
		if (_initialization is not null &&
		    parameters.Length == 3 &&
		    parameters[0].TryGetValue(out T1 p1) &&
		    parameters[1].TryGetValue(out T2 p2) &&
		    parameters[2].TryGetValue(out T3 p3) &&
		    _initialization.Invoke(p1, p2, p3) is T initialValue)
		{
			value = initialValue;
			return;
		}

		value = defaultValueGenerator();
	}
}

#pragma warning disable S2436 // Types and methods should not have too many generic parameters
/// <summary>
///     Sets up a <typeparamref name="TValue" /> indexer for <typeparamref name="T1" />, <typeparamref name="T2" />,
///     <typeparamref name="T3" /> and <typeparamref name="T4" />.
/// </summary>
#if !DEBUG
[DebuggerNonUserCode]
#endif
public class IndexerSetup<TValue, T1, T2, T3, T4>(
	NamedParameter match1,
	NamedParameter match2,
	NamedParameter match3,
	NamedParameter match4)
	: IndexerSetup,
		IIndexerSetupCallbackBuilder<TValue, T1, T2, T3, T4>, IIndexerSetupReturnBuilder<TValue, T1, T2, T3, T4>,
		IIndexerGetterSetup<TValue, T1, T2, T3, T4>, IIndexerSetterSetup<TValue, T1, T2, T3, T4>, ITypedIndexerMatch
{
	private readonly List<Callback<Action<int, T1, T2, T3, T4, TValue>>> _getterCallbacks = [];
	private readonly List<Callback<Func<int, T1, T2, T3, T4, TValue, TValue>>> _returnCallbacks = [];
	private readonly List<Callback<Action<int, T1, T2, T3, T4, TValue>>> _setterCallbacks = [];
	private Callback? _currentCallback;
	private int _currentGetterCallbacksIndex;
	private Callback? _currentReturnCallback;
	private int _currentReturnCallbackIndex;
	private int _currentSetterCallbacksIndex;
	private Func<T1, T2, T3, T4, TValue>? _initialization;
	private bool? _skipBaseClass;

	/// <inheritdoc cref="IIndexerGetterSetup{TValue, T1, T2, T3, T4}.Do(Action)" />
	IIndexerSetupCallbackBuilder<TValue, T1, T2, T3, T4> IIndexerGetterSetup<TValue, T1, T2, T3, T4>.Do(Action callback)
	{
		Callback<Action<int, T1, T2, T3, T4, TValue>> currentCallback = new(Delegate);
		_currentCallback = currentCallback;
		_getterCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		void Delegate(int _, T1 p1, T2 p2, T3 p3, T4 p4, TValue currentValue)
		{
			callback();
		}
	}

	/// <inheritdoc cref="IIndexerGetterSetup{TValue, T1, T2, T3, T4}.Do(Action{T1, T2, T3, T4})" />
	IIndexerSetupCallbackBuilder<TValue, T1, T2, T3, T4> IIndexerGetterSetup<TValue, T1, T2, T3, T4>.Do(
		Action<T1, T2, T3, T4> callback)
	{
		Callback<Action<int, T1, T2, T3, T4, TValue>> currentCallback = new(Delegate);
		_currentCallback = currentCallback;
		_getterCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		void Delegate(int _, T1 p1, T2 p2, T3 p3, T4 p4, TValue currentValue)
		{
			callback(p1, p2, p3, p4);
		}
	}

	/// <inheritdoc cref="IIndexerGetterSetup{TValue, T1, T2, T3, T4}.Do(Action{T1, T2, T3, T4, TValue})" />
	IIndexerSetupCallbackBuilder<TValue, T1, T2, T3, T4> IIndexerGetterSetup<TValue, T1, T2, T3, T4>.Do(
		Action<T1, T2, T3, T4, TValue> callback)
	{
		Callback<Action<int, T1, T2, T3, T4, TValue>> currentCallback = new(Delegate);
		_currentCallback = currentCallback;
		_getterCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		void Delegate(int _, T1 p1, T2 p2, T3 p3, T4 p4, TValue v)
		{
			callback(p1, p2, p3, p4, v);
		}
	}

	/// <inheritdoc cref="IIndexerGetterSetup{TValue, T1, T2, T3, T4}.Do(Action{int, T1, T2, T3, T4, TValue})" />
	IIndexerSetupCallbackBuilder<TValue, T1, T2, T3, T4> IIndexerGetterSetup<TValue, T1, T2, T3, T4>.Do(
		Action<int, T1, T2, T3, T4, TValue> callback)
	{
		Callback<Action<int, T1, T2, T3, T4, TValue>> currentCallback = new(callback);
		_currentCallback = currentCallback;
		_getterCallbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IIndexerSetterSetup{TValue, T1, T2, T3, T4}.Do(Action)" />
	IIndexerSetupCallbackBuilder<TValue, T1, T2, T3, T4> IIndexerSetterSetup<TValue, T1, T2, T3, T4>.Do(Action callback)
	{
		Callback<Action<int, T1, T2, T3, T4, TValue>> currentCallback = new(Delegate);
		_currentCallback = currentCallback;
		_setterCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		void Delegate(int _, T1 p1, T2 p2, T3 p3, T4 p4, TValue newValue)
		{
			callback();
		}
	}

	/// <inheritdoc cref="IIndexerSetterSetup{TValue, T1, T2, T3, T4}.Do(Action{TValue})" />
	IIndexerSetupCallbackBuilder<TValue, T1, T2, T3, T4> IIndexerSetterSetup<TValue, T1, T2, T3, T4>.Do(
		Action<TValue> callback)
	{
		Callback<Action<int, T1, T2, T3, T4, TValue>> currentCallback = new(Delegate);
		_currentCallback = currentCallback;
		_setterCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		void Delegate(int _, T1 p1, T2 p2, T3 p3, T4 p4, TValue v)
		{
			callback(v);
		}
	}

	/// <inheritdoc cref="IIndexerSetterSetup{TValue, T1, T2, T3, T4}.Do(Action{T1, T2, T3, T4, TValue})" />
	IIndexerSetupCallbackBuilder<TValue, T1, T2, T3, T4> IIndexerSetterSetup<TValue, T1, T2, T3, T4>.Do(
		Action<T1, T2, T3, T4, TValue> callback)
	{
		Callback<Action<int, T1, T2, T3, T4, TValue>> currentCallback = new(Delegate);
		_currentCallback = currentCallback;
		_setterCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		void Delegate(int _, T1 p1, T2 p2, T3 p3, T4 p4, TValue v)
		{
			callback(p1, p2, p3, p4, v);
		}
	}

	/// <inheritdoc cref="IIndexerSetterSetup{TValue, T1, T2, T3, T4}.Do(Action{int, T1, T2, T3, T4, TValue})" />
	IIndexerSetupCallbackBuilder<TValue, T1, T2, T3, T4> IIndexerSetterSetup<TValue, T1, T2, T3, T4>.Do(
		Action<int, T1, T2, T3, T4, TValue> callback)
	{
		Callback<Action<int, T1, T2, T3, T4, TValue>> currentCallback = new(callback);
		_currentCallback = currentCallback;
		_setterCallbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2, T3, T4}.SkippingBaseClass(bool)" />
	public IIndexerSetup<TValue, T1, T2, T3, T4> SkippingBaseClass(bool skipBaseClass = true)
	{
		_skipBaseClass = skipBaseClass;
		return this;
	}

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2, T3, T4}.InitializeWith(TValue)" />
	public IIndexerSetup<TValue, T1, T2, T3, T4> InitializeWith(TValue value)
	{
		if (_initialization is not null)
		{
			throw new MockException("The indexer is already initialized. You cannot initialize it twice.");
		}

		_initialization = (_, _, _, _) => value;
		return this;
	}

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2, T3, T4}.InitializeWith(Func{T1, T2, T3, T4, TValue})" />
	public IIndexerSetup<TValue, T1, T2, T3, T4> InitializeWith(Func<T1, T2, T3, T4, TValue> valueGenerator)
	{
		if (_initialization is not null)
		{
			throw new MockException("The indexer is already initialized. You cannot initialize it twice.");
		}

		_initialization = valueGenerator;
		return this;
	}

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2, T3, T4}.OnGet" />
	public IIndexerGetterSetup<TValue, T1, T2, T3, T4> OnGet
		=> this;

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2, T3, T4}.OnSet" />
	public IIndexerSetterSetup<TValue, T1, T2, T3, T4> OnSet
		=> this;

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2, T3, T4}.Returns(TValue)" />
	public IIndexerSetupReturnBuilder<TValue, T1, T2, T3, T4> Returns(TValue returnValue)
	{
		Callback<Func<int, T1, T2, T3, T4, TValue, TValue>> currentCallback = new(Delegate);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TValue Delegate(int _, T1 p1, T2 p2, T3 p3, T4 p4, TValue currentValue)
		{
			return returnValue;
		}
	}

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2, T3, T4}.Returns(Func{TValue})" />
	public IIndexerSetupReturnBuilder<TValue, T1, T2, T3, T4> Returns(Func<TValue> callback)
	{
		Callback<Func<int, T1, T2, T3, T4, TValue, TValue>> currentCallback = new(Delegate);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TValue Delegate(int _, T1 p1, T2 p2, T3 p3, T4 p4, TValue currentValue)
		{
			return callback();
		}
	}

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2, T3, T4}.Returns(Func{T1, T2, T3, T4, TValue})" />
	public IIndexerSetupReturnBuilder<TValue, T1, T2, T3, T4> Returns(Func<T1, T2, T3, T4, TValue> callback)
	{
		Callback<Func<int, T1, T2, T3, T4, TValue, TValue>> currentCallback = new(Delegate);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TValue Delegate(int _, T1 p1, T2 p2, T3 p3, T4 p4, TValue currentValue)
		{
			return callback(p1, p2, p3, p4);
		}
	}

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2, T3, T4}.Returns(Func{T1, T2, T3, T4, TValue, TValue})" />
	public IIndexerSetupReturnBuilder<TValue, T1, T2, T3, T4> Returns(Func<T1, T2, T3, T4, TValue, TValue> callback)
	{
		Callback<Func<int, T1, T2, T3, T4, TValue, TValue>> currentCallback = new(Delegate);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TValue Delegate(int _, T1 p1, T2 p2, T3 p3, T4 p4, TValue v)
		{
			return callback(p1, p2, p3, p4, v);
		}
	}

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2, T3, T4}.Throws{TException}()" />
	public IIndexerSetupReturnBuilder<TValue, T1, T2, T3, T4> Throws<TException>()
		where TException : Exception, new()
	{
		Callback<Func<int, T1, T2, T3, T4, TValue, TValue>> currentCallback = new(Delegate);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TValue Delegate(int _, T1 p1, T2 p2, T3 p3, T4 p4, TValue currentValue)
		{
			throw new TException();
		}
	}

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2, T3, T4}.Throws(Exception)" />
	public IIndexerSetupReturnBuilder<TValue, T1, T2, T3, T4> Throws(Exception exception)
	{
		Callback<Func<int, T1, T2, T3, T4, TValue, TValue>> currentCallback = new(Delegate);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TValue Delegate(int _, T1 p1, T2 p2, T3 p3, T4 p4, TValue currentValue)
		{
			throw exception;
		}
	}

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2, T3, T4}.Throws(Func{Exception})" />
	public IIndexerSetupReturnBuilder<TValue, T1, T2, T3, T4> Throws(Func<Exception> callback)
	{
		Callback<Func<int, T1, T2, T3, T4, TValue, TValue>> currentCallback = new(Delegate);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TValue Delegate(int _, T1 p1, T2 p2, T3 p3, T4 p4, TValue currentValue)
		{
			throw callback();
		}
	}

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2, T3, T4}.Throws(Func{T1, T2, T3, T4, Exception})" />
	public IIndexerSetupReturnBuilder<TValue, T1, T2, T3, T4> Throws(Func<T1, T2, T3, T4, Exception> callback)
	{
		Callback<Func<int, T1, T2, T3, T4, TValue, TValue>> currentCallback = new(Delegate);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TValue Delegate(int _, T1 p1, T2 p2, T3 p3, T4 p4, TValue currentValue)
		{
			throw callback(p1, p2, p3, p4);
		}
	}

	/// <inheritdoc cref="IIndexerSetup{TValue, T1, T2, T3, T4}.Throws(Func{T1, T2, T3, T4, TValue, Exception})" />
	public IIndexerSetupReturnBuilder<TValue, T1, T2, T3, T4> Throws(Func<T1, T2, T3, T4, TValue, Exception> callback)
	{
		Callback<Func<int, T1, T2, T3, T4, TValue, TValue>> currentCallback = new(Delegate);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;

		[DebuggerNonUserCode]
		TValue Delegate(int _, T1 p1, T2 p2, T3 p3, T4 p4, TValue v)
		{
			throw callback(p1, p2, p3, p4, v);
		}
	}

	/// <inheritdoc cref="IIndexerSetupCallbackBuilder{TValue, T1, T2, T3, T4}.When(Func{int, bool})" />
	IIndexerSetupCallbackWhenBuilder<TValue, T1, T2, T3, T4> IIndexerSetupCallbackBuilder<TValue, T1, T2, T3, T4>.When(
		Func<int, bool> predicate)
	{
		_currentCallback?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IIndexerSetupCallbackBuilder{TValue, T1, T2, T3, T4}.InParallel()" />
	IIndexerSetupCallbackBuilder<TValue, T1, T2, T3, T4> IIndexerSetupCallbackBuilder<TValue, T1, T2, T3, T4>.
		InParallel()
	{
		_currentCallback?.InParallel();
		return this;
	}

	/// <inheritdoc cref="IIndexerSetupCallbackWhenBuilder{TValue,T1, T2, T3, T4}.For(int)" />
	IIndexerSetupCallbackWhenBuilder<TValue, T1, T2, T3, T4> IIndexerSetupCallbackWhenBuilder<TValue, T1, T2, T3, T4>.
		For(int times)
	{
		_currentCallback?.For(times);
		return this;
	}

	/// <inheritdoc cref="IIndexerSetupCallbackWhenBuilder{TValue,T1, T2, T3, T4}.Only(int)" />
	IIndexerSetup<TValue, T1, T2, T3, T4> IIndexerSetupCallbackWhenBuilder<TValue, T1, T2, T3, T4>.Only(int times)
	{
		_currentCallback?.Only(times);
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
	IIndexerSetupReturnWhenBuilder<TValue, T1, T2, T3, T4> IIndexerSetupReturnWhenBuilder<TValue, T1, T2, T3, T4>.
		For(int times)
	{
		_currentReturnCallback?.For(times);
		return this;
	}

	/// <inheritdoc cref="IIndexerSetupReturnWhenBuilder{TValue,T1, T2, T3, T4}.Only(int)" />
	IIndexerSetup<TValue, T1, T2, T3, T4> IIndexerSetupReturnWhenBuilder<TValue, T1, T2, T3, T4>.Only(int times)
	{
		_currentReturnCallback?.Only(times);
		return this;
	}

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> $"{FormatType(typeof(TValue))} this[{match1}, {match2}, {match3}, {match4}]";

	/// <inheritdoc cref="IndexerSetup.GetSkipBaseClass()" />
	protected override bool? GetSkipBaseClass()
		=> _skipBaseClass;

	/// <inheritdoc cref="ExecuteGetterCallback{TValue}(IndexerGetterAccess, TValue, MockBehavior)" />
	protected override T ExecuteGetterCallback<T>(IndexerGetterAccess indexerGetterAccess, T value,
		MockBehavior behavior)
	{
		if (TryCast(value, out TValue resultValue, behavior) &&
		    indexerGetterAccess.Parameters.Length == 4 &&
		    indexerGetterAccess.Parameters[0].TryGetValue(out T1 p1) &&
		    indexerGetterAccess.Parameters[1].TryGetValue(out T2 p2) &&
		    indexerGetterAccess.Parameters[2].TryGetValue(out T3 p3) &&
		    indexerGetterAccess.Parameters[3].TryGetValue(out T4 p4))
		{
			bool wasInvoked = false;
			int currentGetterCallbacksIndex = _currentGetterCallbacksIndex;
			for (int i = 0; i < _getterCallbacks.Count; i++)
			{
				Callback<Action<int, T1, T2, T3, T4, TValue>> getterCallback =
					_getterCallbacks[(currentGetterCallbacksIndex + i) % _getterCallbacks.Count];
				if (getterCallback.Invoke(wasInvoked, ref _currentGetterCallbacksIndex, (invocationCount, @delegate)
					    => @delegate(invocationCount, p1, p2, p3, p4, resultValue)))
				{
					wasInvoked = true;
				}
			}

			foreach (Callback<Func<int, T1, T2, T3, T4, TValue, TValue>> _ in _returnCallbacks)
			{
				Callback<Func<int, T1, T2, T3, T4, TValue, TValue>> returnCallback =
					_returnCallbacks[_currentReturnCallbackIndex % _returnCallbacks.Count];
				if (returnCallback.Invoke(ref _currentReturnCallbackIndex, (invocationCount, @delegate)
					    => @delegate(invocationCount, p1, p2, p3, p4, resultValue), out TValue? newValue) &&
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
		    indexerSetterAccess.Parameters[0].TryGetValue(out T1 p1) &&
		    indexerSetterAccess.Parameters[1].TryGetValue(out T2 p2) &&
		    indexerSetterAccess.Parameters[2].TryGetValue(out T3 p3) &&
		    indexerSetterAccess.Parameters[3].TryGetValue(out T4 p4))
		{
			bool wasInvoked = false;
			int currentSetterCallbacksIndex = _currentSetterCallbacksIndex;
			for (int i = 0; i < _setterCallbacks.Count; i++)
			{
				Callback<Action<int, T1, T2, T3, T4, TValue>> setterCallback =
					_setterCallbacks[(currentSetterCallbacksIndex + i) % _setterCallbacks.Count];
				if (setterCallback.Invoke(wasInvoked, ref _currentSetterCallbacksIndex, (invocationCount, @delegate)
					    => @delegate(invocationCount, p1, p2, p3, p4, resultValue)))
				{
					wasInvoked = true;
				}
			}
		}
	}

	/// <inheritdoc cref="IsMatch(INamedParameterValue[])" />
	protected override bool IsMatch(INamedParameterValue[] parameters)
		=> Matches([match1, match2, match3, match4,], parameters);

	/// <inheritdoc cref="ITypedIndexerMatch.MatchesTyped{T1}(string, T1)" />
	bool ITypedIndexerMatch.MatchesTyped<TActual1>(string n1, TActual1 v1) => false;

	/// <inheritdoc cref="ITypedIndexerMatch.MatchesTyped{T1,T2}(string, T1, string, T2)" />
	bool ITypedIndexerMatch.MatchesTyped<TActual1, TActual2>(string n1, TActual1 v1, string n2, TActual2 v2) => false;

	/// <inheritdoc cref="ITypedIndexerMatch.MatchesTyped{T1,T2,T3}(string, T1, string, T2, string, T3)" />
	bool ITypedIndexerMatch.MatchesTyped<TActual1, TActual2, TActual3>(
		string n1, TActual1 v1, string n2, TActual2 v2, string n3, TActual3 v3) => false;

	/// <inheritdoc cref="ITypedIndexerMatch.MatchesTyped{T1,T2,T3,T4}(string, T1, string, T2, string, T3, string, T4)" />
	bool ITypedIndexerMatch.MatchesTyped<TActual1, TActual2, TActual3, TActual4>(
		string n1, TActual1 v1, string n2, TActual2 v2, string n3, TActual3 v3, string n4, TActual4 v4)
	{
		if (!MatchesParameter(match1, n1, v1) || !MatchesParameter(match2, n2, v2) ||
		    !MatchesParameter(match3, n3, v3) || !MatchesParameter(match4, n4, v4))
		{
			return false;
		}

		InvokeCallbacksParameter(match1, n1, v1);
		InvokeCallbacksParameter(match2, n2, v2);
		InvokeCallbacksParameter(match3, n3, v3);
		InvokeCallbacksParameter(match4, n4, v4);
		return true;
	}

	/// <inheritdoc cref="IndexerSetup.GetInitialValue{T}" />
	protected override void GetInitialValue<T>(MockBehavior behavior, Func<T> defaultValueGenerator,
		INamedParameterValue[] parameters,
		[NotNullWhen(true)] out T value)
	{
		if (_initialization is not null &&
		    parameters.Length == 4 &&
		    parameters[0].TryGetValue(out T1 p1) &&
		    parameters[1].TryGetValue(out T2 p2) &&
		    parameters[2].TryGetValue(out T3 p3) &&
		    parameters[3].TryGetValue(out T4 p4) &&
		    _initialization.Invoke(p1, p2, p3, p4) is T initialValue)
		{
			value = initialValue;
			return;
		}

		value = defaultValueGenerator();
	}
}
#pragma warning restore S2436 // Types and methods should not have too many generic parameters
