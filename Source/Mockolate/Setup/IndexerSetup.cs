using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Mockolate.Exceptions;
using Mockolate.Interactions;
using Mockolate.Internals;
using Mockolate.Parameters;

#pragma warning disable S2436 // Types and methods should not have too many generic parameters

namespace Mockolate.Setup;

/// <summary>
///     Base class for indexer setups.
/// </summary>
#if !DEBUG
[DebuggerNonUserCode]
#endif
public abstract class IndexerSetup : IInteractiveIndexerSetup
{
	/// <summary>
	///     The mock registry this setup belongs to. Set by <see cref="MockRegistry.SetupIndexer(IndexerSetup)" />.
	/// </summary>
	internal MockRegistry? MockRegistry { get; set; }

	/// <summary>
	///     Transitions the associated mock registry to the given <paramref name="scenario" />, if any.
	/// </summary>
	protected void TransitionScenario(string scenario)
	{
		if (MockRegistry is not null)
		{
			MockRegistry.Scenario = scenario;
		}
	}

	/// <inheritdoc cref="IInteractiveIndexerSetup.Matches(IndexerAccess)" />
	bool IInteractiveIndexerSetup.Matches(IndexerAccess indexerAccess)
		=> MatchesAccess(indexerAccess);

	/// <inheritdoc cref="IInteractiveIndexerSetup.SkipBaseClass()" />
	public abstract bool? SkipBaseClass();

	/// <summary>
	///     Checks if the <paramref name="access" /> matches the setup.
	/// </summary>
	protected abstract bool MatchesAccess(IndexerAccess access);

	/// <summary>
	///     Invokes the getter flow for the given <paramref name="access" /> using <paramref name="baseValue" /> as the seed.
	/// </summary>
	public abstract TResult GetResult<TResult>(IndexerAccess access, MockBehavior behavior, TResult baseValue);

	/// <summary>
	///     Invokes the getter flow for the given <paramref name="access" /> using the <paramref name="defaultValueGenerator" />
	///     when no value has been stored or initialized.
	/// </summary>
	public abstract TResult GetResult<TResult>(IndexerAccess access, MockBehavior behavior,
		Func<TResult> defaultValueGenerator);

	/// <summary>
	///     Invokes the setter flow for the given <paramref name="access" /> with the given <paramref name="value" />.
	/// </summary>
	public abstract void SetResult<TResult>(IndexerAccess access, MockBehavior behavior, TResult value);

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
	///     Returns a formatted string representation of the given <paramref name="type" />.
	/// </summary>
	protected static string FormatType(Type type)
		=> type.FormatType();
}

/// <summary>
///     Sets up a <typeparamref name="TValue" /> indexer for <typeparamref name="T1" />.
/// </summary>
#if !DEBUG
[DebuggerNonUserCode]
#endif
public class IndexerSetup<TValue, T1>(IParameterMatch<T1> parameter1) : IndexerSetup,
	IIndexerSetupCallbackBuilder<TValue, T1>, IIndexerSetupReturnBuilder<TValue, T1>,
	IIndexerGetterSetup<TValue, T1>, IIndexerSetterSetup<TValue, T1>
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

	IIndexerSetupCallbackBuilder<TValue, T1> IIndexerGetterSetup<TValue, T1>.TransitionTo(string scenario)
	{
		Callback<Action<int, T1, TValue>> currentCallback = new((_, _, _) =>
		{
			if (MockRegistry is not null)
			{
				MockRegistry.Scenario = scenario;
			}
		});
		currentCallback.InParallel();
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

	IIndexerSetupCallbackBuilder<TValue, T1> IIndexerSetterSetup<TValue, T1>.TransitionTo(string scenario)
	{
		Callback<Action<int, T1, TValue>> currentCallback = new((_, _, _) =>
		{
			if (MockRegistry is not null)
			{
				MockRegistry.Scenario = scenario;
			}
		});
		currentCallback.InParallel();
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
		=> $"{FormatType(typeof(TValue))} this[{parameter1}]";

	/// <inheritdoc cref="IndexerSetup.SkipBaseClass()" />
	public override bool? SkipBaseClass()
		=> _skipBaseClass;

	/// <summary>
	///     Check if the setup matches the specified parameter value <paramref name="p1" />.
	/// </summary>
	public virtual bool Matches(T1 p1)
	{
		if (!parameter1.Matches(p1))
		{
			return false;
		}

		parameter1.InvokeCallbacks(p1);
		return true;
	}

	/// <summary>
	///     Check if the setup matches the specified parameter values.
	/// </summary>
	public virtual bool Matches(T1 p1, TValue value)
		=> Matches(p1);

	/// <inheritdoc cref="IndexerSetup.MatchesAccess(IndexerAccess)" />
	protected override bool MatchesAccess(IndexerAccess access)
	{
		if (access is IndexerGetterAccess<T1> getter)
		{
			return Matches(getter.Parameter1);
		}

		if (access is IndexerSetterAccess<T1, TValue> setter)
		{
			return Matches(setter.Parameter1, setter.TypedValue);
		}

		return false;
	}

	/// <inheritdoc cref="IndexerSetup.GetResult{TResult}(IndexerAccess, MockBehavior, TResult)" />
	public override TResult GetResult<TResult>(IndexerAccess access, MockBehavior behavior,
		TResult baseValue)
	{
		if (!TryExtractParameter(access, out T1 p1))
		{
			return baseValue;
		}

		TValue currentValue = TryCast(baseValue, out TValue casted, behavior) ? casted : default!;
		currentValue = ExecuteGetterCallbacks(p1, currentValue);
		currentValue = ExecuteReturnCallbacks(p1, currentValue, out _);
		access.StoreValue(currentValue);
		return TryCast(currentValue, out TResult result, behavior) ? result : baseValue;
	}

	/// <inheritdoc cref="IndexerSetup.GetResult{TResult}(IndexerAccess, MockBehavior, Func{TResult})" />
	public override TResult GetResult<TResult>(IndexerAccess access, MockBehavior behavior,
		Func<TResult> defaultValueGenerator)
	{
		if (!TryExtractParameter(access, out T1 p1))
		{
			return defaultValueGenerator();
		}

		TValue currentValue;
		if (access.TryFindStoredValue(out TValue existing))
		{
			currentValue = existing;
		}
		else if (_initialization is not null)
		{
			currentValue = _initialization.Invoke(p1);
		}
		else
		{
			currentValue = TryCast(defaultValueGenerator(), out TValue casted, behavior) ? casted : default!;
		}

		currentValue = ExecuteGetterCallbacks(p1, currentValue);
		currentValue = ExecuteReturnCallbacks(p1, currentValue, out _);
		access.StoreValue(currentValue);
		return TryCast(currentValue, out TResult result, behavior) ? result : defaultValueGenerator();
	}

	/// <inheritdoc cref="IndexerSetup.SetResult{TResult}(IndexerAccess, MockBehavior, TResult)" />
	public override void SetResult<TResult>(IndexerAccess access, MockBehavior behavior,
		TResult value)
	{
		access.StoreValue(value);
		if (!TryExtractParameter(access, out T1 p1))
		{
			return;
		}

		if (!TryCast(value, out TValue resultValue, behavior))
		{
			return;
		}

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

	private TValue ExecuteGetterCallbacks(T1 p1, TValue currentValue)
	{
		bool wasInvoked = false;
		int currentGetterCallbacksIndex = _currentGetterCallbacksIndex;
		for (int i = 0; i < _getterCallbacks.Count; i++)
		{
			Callback<Action<int, T1, TValue>> getterCallback =
				_getterCallbacks[(currentGetterCallbacksIndex + i) % _getterCallbacks.Count];
			if (getterCallback.Invoke(wasInvoked, ref _currentGetterCallbacksIndex, (invocationCount, @delegate)
				    => @delegate(invocationCount, p1, currentValue)))
			{
				wasInvoked = true;
			}
		}

		return currentValue;
	}

	private TValue ExecuteReturnCallbacks(T1 p1, TValue currentValue, out bool matched)
	{
		matched = false;
		foreach (Callback<Func<int, T1, TValue, TValue>> _ in _returnCallbacks)
		{
			Callback<Func<int, T1, TValue, TValue>> returnCallback =
				_returnCallbacks[_currentReturnCallbackIndex % _returnCallbacks.Count];
			if (returnCallback.Invoke(ref _currentReturnCallbackIndex, (invocationCount, @delegate)
				    => @delegate(invocationCount, p1, currentValue), out TValue? newValue))
			{
				matched = true;
				return newValue!;
			}
		}

		return currentValue;
	}

	private static bool TryExtractParameter(IndexerAccess access, out T1 p1)
	{
		if (access is IndexerGetterAccess<T1> getter)
		{
			p1 = getter.Parameter1;
			return true;
		}

		if (access is IndexerSetterAccess<T1, TValue> setter)
		{
			p1 = setter.Parameter1;
			return true;
		}

		p1 = default!;
		return false;
	}
}

/// <summary>
///     Sets up a <typeparamref name="TValue" /> indexer for <typeparamref name="T1" /> and <typeparamref name="T2" />.
/// </summary>
#if !DEBUG
[DebuggerNonUserCode]
#endif
public class IndexerSetup<TValue, T1, T2>(IParameterMatch<T1> parameter1, IParameterMatch<T2> parameter2) : IndexerSetup,
	IIndexerSetupCallbackBuilder<TValue, T1, T2>, IIndexerSetupReturnBuilder<TValue, T1, T2>,
	IIndexerGetterSetup<TValue, T1, T2>, IIndexerSetterSetup<TValue, T1, T2>
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

	IIndexerSetupCallbackBuilder<TValue, T1, T2> IIndexerGetterSetup<TValue, T1, T2>.TransitionTo(string scenario)
	{
		Callback<Action<int, T1, T2, TValue>> currentCallback = new((_, _, _, _) =>
		{
			if (MockRegistry is not null)
			{
				MockRegistry.Scenario = scenario;
			}
		});
		currentCallback.InParallel();
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

	IIndexerSetupCallbackBuilder<TValue, T1, T2> IIndexerSetterSetup<TValue, T1, T2>.TransitionTo(string scenario)
	{
		Callback<Action<int, T1, T2, TValue>> currentCallback = new((_, _, _, _) =>
		{
			if (MockRegistry is not null)
			{
				MockRegistry.Scenario = scenario;
			}
		});
		currentCallback.InParallel();
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

	/// <inheritdoc cref="IIndexerSetupCallbackWhenBuilder{TValue,T1,T2}.For(int)" />
	IIndexerSetupCallbackWhenBuilder<TValue, T1, T2> IIndexerSetupCallbackWhenBuilder<TValue, T1, T2>.For(int times)
	{
		_currentCallback?.For(times);
		return this;
	}

	/// <inheritdoc cref="IIndexerSetupCallbackWhenBuilder{TValue,T1,T2}.Only(int)" />
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

	/// <inheritdoc cref="IIndexerSetupReturnWhenBuilder{TValue,T1,T2}.For(int)" />
	IIndexerSetupReturnWhenBuilder<TValue, T1, T2> IIndexerSetupReturnWhenBuilder<TValue, T1, T2>.For(int times)
	{
		_currentReturnCallback?.For(times);
		return this;
	}

	/// <inheritdoc cref="IIndexerSetupReturnWhenBuilder{TValue,T1,T2}.Only(int)" />
	IIndexerSetup<TValue, T1, T2> IIndexerSetupReturnWhenBuilder<TValue, T1, T2>.Only(int times)
	{
		_currentReturnCallback?.Only(times);
		return this;
	}

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> $"{FormatType(typeof(TValue))} this[{parameter1}, {parameter2}]";

	/// <inheritdoc cref="IndexerSetup.SkipBaseClass()" />
	public override bool? SkipBaseClass()
		=> _skipBaseClass;

	/// <summary>
	///     Check if the setup matches the specified parameter values.
	/// </summary>
	public virtual bool Matches(T1 p1, T2 p2)
	{
		if (!parameter1.Matches(p1) || !parameter2.Matches(p2))
		{
			return false;
		}

		parameter1.InvokeCallbacks(p1);
		parameter2.InvokeCallbacks(p2);
		return true;
	}

	/// <summary>
	///     Check if the setup matches the specified parameter values.
	/// </summary>
	public virtual bool Matches(T1 p1, T2 p2, TValue value)
		=> Matches(p1, p2);

	/// <inheritdoc cref="IndexerSetup.MatchesAccess(IndexerAccess)" />
	protected override bool MatchesAccess(IndexerAccess access)
	{
		if (access is IndexerGetterAccess<T1, T2> getter)
		{
			return Matches(getter.Parameter1, getter.Parameter2);
		}

		if (access is IndexerSetterAccess<T1, T2, TValue> setter)
		{
			return Matches(setter.Parameter1, setter.Parameter2, setter.TypedValue);
		}

		return false;
	}

	/// <inheritdoc cref="IndexerSetup.GetResult{TResult}(IndexerAccess, MockBehavior, TResult)" />
	public override TResult GetResult<TResult>(IndexerAccess access, MockBehavior behavior,
		TResult baseValue)
	{
		if (!TryExtractParameters(access, out T1 p1, out T2 p2))
		{
			return baseValue;
		}

		TValue currentValue = TryCast(baseValue, out TValue casted, behavior) ? casted : default!;
		currentValue = ExecuteGetterCallbacks(p1, p2, currentValue);
		currentValue = ExecuteReturnCallbacks(p1, p2, currentValue, out _);
		access.StoreValue(currentValue);
		return TryCast(currentValue, out TResult result, behavior) ? result : baseValue;
	}

	/// <inheritdoc cref="IndexerSetup.GetResult{TResult}(IndexerAccess, MockBehavior, Func{TResult})" />
	public override TResult GetResult<TResult>(IndexerAccess access, MockBehavior behavior,
		Func<TResult> defaultValueGenerator)
	{
		if (!TryExtractParameters(access, out T1 p1, out T2 p2))
		{
			return defaultValueGenerator();
		}

		TValue currentValue;
		if (access.TryFindStoredValue(out TValue existing))
		{
			currentValue = existing;
		}
		else if (_initialization is not null)
		{
			currentValue = _initialization.Invoke(p1, p2);
		}
		else
		{
			currentValue = TryCast(defaultValueGenerator(), out TValue casted, behavior) ? casted : default!;
		}

		currentValue = ExecuteGetterCallbacks(p1, p2, currentValue);
		currentValue = ExecuteReturnCallbacks(p1, p2, currentValue, out _);
		access.StoreValue(currentValue);
		return TryCast(currentValue, out TResult result, behavior) ? result : defaultValueGenerator();
	}

	/// <inheritdoc cref="IndexerSetup.SetResult{TResult}(IndexerAccess, MockBehavior, TResult)" />
	public override void SetResult<TResult>(IndexerAccess access, MockBehavior behavior,
		TResult value)
	{
		access.StoreValue(value);
		if (!TryExtractParameters(access, out T1 p1, out T2 p2))
		{
			return;
		}

		if (!TryCast(value, out TValue resultValue, behavior))
		{
			return;
		}

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

	private TValue ExecuteGetterCallbacks(T1 p1, T2 p2, TValue currentValue)
	{
		bool wasInvoked = false;
		int currentGetterCallbacksIndex = _currentGetterCallbacksIndex;
		for (int i = 0; i < _getterCallbacks.Count; i++)
		{
			Callback<Action<int, T1, T2, TValue>> getterCallback =
				_getterCallbacks[(currentGetterCallbacksIndex + i) % _getterCallbacks.Count];
			if (getterCallback.Invoke(wasInvoked, ref _currentGetterCallbacksIndex, (invocationCount, @delegate)
				    => @delegate(invocationCount, p1, p2, currentValue)))
			{
				wasInvoked = true;
			}
		}

		return currentValue;
	}

	private TValue ExecuteReturnCallbacks(T1 p1, T2 p2, TValue currentValue, out bool matched)
	{
		matched = false;
		foreach (Callback<Func<int, T1, T2, TValue, TValue>> _ in _returnCallbacks)
		{
			Callback<Func<int, T1, T2, TValue, TValue>> returnCallback =
				_returnCallbacks[_currentReturnCallbackIndex % _returnCallbacks.Count];
			if (returnCallback.Invoke(ref _currentReturnCallbackIndex, (invocationCount, @delegate)
				    => @delegate(invocationCount, p1, p2, currentValue), out TValue? newValue))
			{
				matched = true;
				return newValue!;
			}
		}

		return currentValue;
	}

	private static bool TryExtractParameters(IndexerAccess access, out T1 p1, out T2 p2)
	{
		if (access is IndexerGetterAccess<T1, T2> getter)
		{
			p1 = getter.Parameter1;
			p2 = getter.Parameter2;
			return true;
		}

		if (access is IndexerSetterAccess<T1, T2, TValue> setter)
		{
			p1 = setter.Parameter1;
			p2 = setter.Parameter2;
			return true;
		}

		p1 = default!;
		p2 = default!;
		return false;
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
	IParameterMatch<T1> parameter1,
	IParameterMatch<T2> parameter2,
	IParameterMatch<T3> parameter3) : IndexerSetup,
	IIndexerSetupCallbackBuilder<TValue, T1, T2, T3>, IIndexerSetupReturnBuilder<TValue, T1, T2, T3>,
	IIndexerGetterSetup<TValue, T1, T2, T3>, IIndexerSetterSetup<TValue, T1, T2, T3>
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

	IIndexerSetupCallbackBuilder<TValue, T1, T2, T3> IIndexerGetterSetup<TValue, T1, T2, T3>.TransitionTo(
		string scenario)
	{
		Callback<Action<int, T1, T2, T3, TValue>> currentCallback = new((_, _, _, _, _) =>
		{
			if (MockRegistry is not null)
			{
				MockRegistry.Scenario = scenario;
			}
		});
		currentCallback.InParallel();
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
	IIndexerSetupCallbackBuilder<TValue, T1, T2, T3> IIndexerSetterSetup<TValue, T1, T2, T3>.Do(
		Action<TValue> callback)
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

	IIndexerSetupCallbackBuilder<TValue, T1, T2, T3> IIndexerSetterSetup<TValue, T1, T2, T3>.TransitionTo(
		string scenario)
	{
		Callback<Action<int, T1, T2, T3, TValue>> currentCallback = new((_, _, _, _, _) =>
		{
			if (MockRegistry is not null)
			{
				MockRegistry.Scenario = scenario;
			}
		});
		currentCallback.InParallel();
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

	/// <inheritdoc cref="IIndexerSetupCallbackWhenBuilder{TValue,T1,T2,T3}.For(int)" />
	IIndexerSetupCallbackWhenBuilder<TValue, T1, T2, T3> IIndexerSetupCallbackWhenBuilder<TValue, T1, T2, T3>.For(
		int times)
	{
		_currentCallback?.For(times);
		return this;
	}

	/// <inheritdoc cref="IIndexerSetupCallbackWhenBuilder{TValue,T1,T2,T3}.Only(int)" />
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

	/// <inheritdoc cref="IIndexerSetupReturnWhenBuilder{TValue,T1,T2,T3}.For(int)" />
	IIndexerSetupReturnWhenBuilder<TValue, T1, T2, T3> IIndexerSetupReturnWhenBuilder<TValue, T1, T2, T3>.For(
		int times)
	{
		_currentReturnCallback?.For(times);
		return this;
	}

	/// <inheritdoc cref="IIndexerSetupReturnWhenBuilder{TValue,T1,T2,T3}.Only(int)" />
	IIndexerSetup<TValue, T1, T2, T3> IIndexerSetupReturnWhenBuilder<TValue, T1, T2, T3>.Only(int times)
	{
		_currentReturnCallback?.Only(times);
		return this;
	}

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> $"{FormatType(typeof(TValue))} this[{parameter1}, {parameter2}, {parameter3}]";

	/// <inheritdoc cref="IndexerSetup.SkipBaseClass()" />
	public override bool? SkipBaseClass()
		=> _skipBaseClass;

	/// <summary>
	///     Check if the setup matches the specified parameter values.
	/// </summary>
	public virtual bool Matches(T1 p1, T2 p2, T3 p3)
	{
		if (!parameter1.Matches(p1) || !parameter2.Matches(p2) || !parameter3.Matches(p3))
		{
			return false;
		}

		parameter1.InvokeCallbacks(p1);
		parameter2.InvokeCallbacks(p2);
		parameter3.InvokeCallbacks(p3);
		return true;
	}

	/// <summary>
	///     Check if the setup matches the specified parameter values.
	/// </summary>
	public virtual bool Matches(T1 p1, T2 p2, T3 p3, TValue value)
		=> Matches(p1, p2, p3);

	/// <inheritdoc cref="IndexerSetup.MatchesAccess(IndexerAccess)" />
	protected override bool MatchesAccess(IndexerAccess access)
	{
		if (access is IndexerGetterAccess<T1, T2, T3> getter)
		{
			return Matches(getter.Parameter1, getter.Parameter2, getter.Parameter3);
		}

		if (access is IndexerSetterAccess<T1, T2, T3, TValue> setter)
		{
			return Matches(setter.Parameter1, setter.Parameter2, setter.Parameter3, setter.TypedValue);
		}

		return false;
	}

	/// <inheritdoc cref="IndexerSetup.GetResult{TResult}(IndexerAccess, MockBehavior, TResult)" />
	public override TResult GetResult<TResult>(IndexerAccess access, MockBehavior behavior,
		TResult baseValue)
	{
		if (!TryExtractParameters(access, out T1 p1, out T2 p2, out T3 p3))
		{
			return baseValue;
		}

		TValue currentValue = TryCast(baseValue, out TValue casted, behavior) ? casted : default!;
		currentValue = ExecuteGetterCallbacks(p1, p2, p3, currentValue);
		currentValue = ExecuteReturnCallbacks(p1, p2, p3, currentValue, out _);
		access.StoreValue(currentValue);
		return TryCast(currentValue, out TResult result, behavior) ? result : baseValue;
	}

	/// <inheritdoc cref="IndexerSetup.GetResult{TResult}(IndexerAccess, MockBehavior, Func{TResult})" />
	public override TResult GetResult<TResult>(IndexerAccess access, MockBehavior behavior,
		Func<TResult> defaultValueGenerator)
	{
		if (!TryExtractParameters(access, out T1 p1, out T2 p2, out T3 p3))
		{
			return defaultValueGenerator();
		}

		TValue currentValue;
		if (access.TryFindStoredValue(out TValue existing))
		{
			currentValue = existing;
		}
		else if (_initialization is not null)
		{
			currentValue = _initialization.Invoke(p1, p2, p3);
		}
		else
		{
			currentValue = TryCast(defaultValueGenerator(), out TValue casted, behavior) ? casted : default!;
		}

		currentValue = ExecuteGetterCallbacks(p1, p2, p3, currentValue);
		currentValue = ExecuteReturnCallbacks(p1, p2, p3, currentValue, out _);
		access.StoreValue(currentValue);
		return TryCast(currentValue, out TResult result, behavior) ? result : defaultValueGenerator();
	}

	/// <inheritdoc cref="IndexerSetup.SetResult{TResult}(IndexerAccess, MockBehavior, TResult)" />
	public override void SetResult<TResult>(IndexerAccess access, MockBehavior behavior,
		TResult value)
	{
		access.StoreValue(value);
		if (!TryExtractParameters(access, out T1 p1, out T2 p2, out T3 p3))
		{
			return;
		}

		if (!TryCast(value, out TValue resultValue, behavior))
		{
			return;
		}

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

	private TValue ExecuteGetterCallbacks(T1 p1, T2 p2, T3 p3, TValue currentValue)
	{
		bool wasInvoked = false;
		int currentGetterCallbacksIndex = _currentGetterCallbacksIndex;
		for (int i = 0; i < _getterCallbacks.Count; i++)
		{
			Callback<Action<int, T1, T2, T3, TValue>> getterCallback =
				_getterCallbacks[(currentGetterCallbacksIndex + i) % _getterCallbacks.Count];
			if (getterCallback.Invoke(wasInvoked, ref _currentGetterCallbacksIndex, (invocationCount, @delegate)
				    => @delegate(invocationCount, p1, p2, p3, currentValue)))
			{
				wasInvoked = true;
			}
		}

		return currentValue;
	}

	private TValue ExecuteReturnCallbacks(T1 p1, T2 p2, T3 p3, TValue currentValue, out bool matched)
	{
		matched = false;
		foreach (Callback<Func<int, T1, T2, T3, TValue, TValue>> _ in _returnCallbacks)
		{
			Callback<Func<int, T1, T2, T3, TValue, TValue>> returnCallback =
				_returnCallbacks[_currentReturnCallbackIndex % _returnCallbacks.Count];
			if (returnCallback.Invoke(ref _currentReturnCallbackIndex, (invocationCount, @delegate)
				    => @delegate(invocationCount, p1, p2, p3, currentValue), out TValue? newValue))
			{
				matched = true;
				return newValue!;
			}
		}

		return currentValue;
	}

	private static bool TryExtractParameters(IndexerAccess access, out T1 p1, out T2 p2, out T3 p3)
	{
		if (access is IndexerGetterAccess<T1, T2, T3> getter)
		{
			p1 = getter.Parameter1;
			p2 = getter.Parameter2;
			p3 = getter.Parameter3;
			return true;
		}

		if (access is IndexerSetterAccess<T1, T2, T3, TValue> setter)
		{
			p1 = setter.Parameter1;
			p2 = setter.Parameter2;
			p3 = setter.Parameter3;
			return true;
		}

		p1 = default!;
		p2 = default!;
		p3 = default!;
		return false;
	}
}

/// <summary>
///     Sets up a <typeparamref name="TValue" /> indexer for <typeparamref name="T1" />, <typeparamref name="T2" />,
///     <typeparamref name="T3" /> and <typeparamref name="T4" />.
/// </summary>
#if !DEBUG
[DebuggerNonUserCode]
#endif
public class IndexerSetup<TValue, T1, T2, T3, T4>(
	IParameterMatch<T1> parameter1,
	IParameterMatch<T2> parameter2,
	IParameterMatch<T3> parameter3,
	IParameterMatch<T4> parameter4) : IndexerSetup,
	IIndexerSetupCallbackBuilder<TValue, T1, T2, T3, T4>, IIndexerSetupReturnBuilder<TValue, T1, T2, T3, T4>,
	IIndexerGetterSetup<TValue, T1, T2, T3, T4>, IIndexerSetterSetup<TValue, T1, T2, T3, T4>
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

	IIndexerSetupCallbackBuilder<TValue, T1, T2, T3, T4> IIndexerGetterSetup<TValue, T1, T2, T3, T4>.TransitionTo(
		string scenario)
	{
		Callback<Action<int, T1, T2, T3, T4, TValue>> currentCallback = new((_, _, _, _, _, _) =>
		{
			if (MockRegistry is not null)
			{
				MockRegistry.Scenario = scenario;
			}
		});
		currentCallback.InParallel();
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

	IIndexerSetupCallbackBuilder<TValue, T1, T2, T3, T4> IIndexerSetterSetup<TValue, T1, T2, T3, T4>.TransitionTo(
		string scenario)
	{
		Callback<Action<int, T1, T2, T3, T4, TValue>> currentCallback = new((_, _, _, _, _, _) =>
		{
			if (MockRegistry is not null)
			{
				MockRegistry.Scenario = scenario;
			}
		});
		currentCallback.InParallel();
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

	/// <inheritdoc cref="IIndexerSetupCallbackWhenBuilder{TValue,T1,T2,T3,T4}.For(int)" />
	IIndexerSetupCallbackWhenBuilder<TValue, T1, T2, T3, T4> IIndexerSetupCallbackWhenBuilder<TValue, T1, T2, T3, T4>.
		For(int times)
	{
		_currentCallback?.For(times);
		return this;
	}

	/// <inheritdoc cref="IIndexerSetupCallbackWhenBuilder{TValue,T1,T2,T3,T4}.Only(int)" />
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

	/// <inheritdoc cref="IIndexerSetupReturnWhenBuilder{TValue,T1,T2,T3,T4}.For(int)" />
	IIndexerSetupReturnWhenBuilder<TValue, T1, T2, T3, T4> IIndexerSetupReturnWhenBuilder<TValue, T1, T2, T3, T4>.For(
		int times)
	{
		_currentReturnCallback?.For(times);
		return this;
	}

	/// <inheritdoc cref="IIndexerSetupReturnWhenBuilder{TValue,T1,T2,T3,T4}.Only(int)" />
	IIndexerSetup<TValue, T1, T2, T3, T4> IIndexerSetupReturnWhenBuilder<TValue, T1, T2, T3, T4>.Only(int times)
	{
		_currentReturnCallback?.Only(times);
		return this;
	}

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> $"{FormatType(typeof(TValue))} this[{parameter1}, {parameter2}, {parameter3}, {parameter4}]";

	/// <inheritdoc cref="IndexerSetup.SkipBaseClass()" />
	public override bool? SkipBaseClass()
		=> _skipBaseClass;

	/// <summary>
	///     Check if the setup matches the specified parameter values.
	/// </summary>
	public virtual bool Matches(T1 p1, T2 p2, T3 p3, T4 p4)
	{
		if (!parameter1.Matches(p1) || !parameter2.Matches(p2) ||
		    !parameter3.Matches(p3) || !parameter4.Matches(p4))
		{
			return false;
		}

		parameter1.InvokeCallbacks(p1);
		parameter2.InvokeCallbacks(p2);
		parameter3.InvokeCallbacks(p3);
		parameter4.InvokeCallbacks(p4);
		return true;
	}

	/// <summary>
	///     Check if the setup matches the specified parameter values.
	/// </summary>
	public virtual bool Matches(T1 p1, T2 p2, T3 p3, T4 p4, TValue value)
		=> Matches(p1, p2, p3, p4);

	/// <inheritdoc cref="IndexerSetup.MatchesAccess(IndexerAccess)" />
	protected override bool MatchesAccess(IndexerAccess access)
	{
		if (access is IndexerGetterAccess<T1, T2, T3, T4> getter)
		{
			return Matches(getter.Parameter1, getter.Parameter2, getter.Parameter3, getter.Parameter4);
		}

		if (access is IndexerSetterAccess<T1, T2, T3, T4, TValue> setter)
		{
			return Matches(setter.Parameter1, setter.Parameter2, setter.Parameter3, setter.Parameter4,
				setter.TypedValue);
		}

		return false;
	}

	/// <inheritdoc cref="IndexerSetup.GetResult{TResult}(IndexerAccess, MockBehavior, TResult)" />
	public override TResult GetResult<TResult>(IndexerAccess access, MockBehavior behavior,
		TResult baseValue)
	{
		if (!TryExtractParameters(access, out T1 p1, out T2 p2, out T3 p3, out T4 p4))
		{
			return baseValue;
		}

		TValue currentValue = TryCast(baseValue, out TValue casted, behavior) ? casted : default!;
		currentValue = ExecuteGetterCallbacks(p1, p2, p3, p4, currentValue);
		currentValue = ExecuteReturnCallbacks(p1, p2, p3, p4, currentValue, out _);
		access.StoreValue(currentValue);
		return TryCast(currentValue, out TResult result, behavior) ? result : baseValue;
	}

	/// <inheritdoc cref="IndexerSetup.GetResult{TResult}(IndexerAccess, MockBehavior, Func{TResult})" />
	public override TResult GetResult<TResult>(IndexerAccess access, MockBehavior behavior,
		Func<TResult> defaultValueGenerator)
	{
		if (!TryExtractParameters(access, out T1 p1, out T2 p2, out T3 p3, out T4 p4))
		{
			return defaultValueGenerator();
		}

		TValue currentValue;
		if (access.TryFindStoredValue(out TValue existing))
		{
			currentValue = existing;
		}
		else if (_initialization is not null)
		{
			currentValue = _initialization.Invoke(p1, p2, p3, p4);
		}
		else
		{
			currentValue = TryCast(defaultValueGenerator(), out TValue casted, behavior) ? casted : default!;
		}

		currentValue = ExecuteGetterCallbacks(p1, p2, p3, p4, currentValue);
		currentValue = ExecuteReturnCallbacks(p1, p2, p3, p4, currentValue, out _);
		access.StoreValue(currentValue);
		return TryCast(currentValue, out TResult result, behavior) ? result : defaultValueGenerator();
	}

	/// <inheritdoc cref="IndexerSetup.SetResult{TResult}(IndexerAccess, MockBehavior, TResult)" />
	public override void SetResult<TResult>(IndexerAccess access, MockBehavior behavior,
		TResult value)
	{
		access.StoreValue(value);
		if (!TryExtractParameters(access, out T1 p1, out T2 p2, out T3 p3, out T4 p4))
		{
			return;
		}

		if (!TryCast(value, out TValue resultValue, behavior))
		{
			return;
		}

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

	private TValue ExecuteGetterCallbacks(T1 p1, T2 p2, T3 p3, T4 p4, TValue currentValue)
	{
		bool wasInvoked = false;
		int currentGetterCallbacksIndex = _currentGetterCallbacksIndex;
		for (int i = 0; i < _getterCallbacks.Count; i++)
		{
			Callback<Action<int, T1, T2, T3, T4, TValue>> getterCallback =
				_getterCallbacks[(currentGetterCallbacksIndex + i) % _getterCallbacks.Count];
			if (getterCallback.Invoke(wasInvoked, ref _currentGetterCallbacksIndex, (invocationCount, @delegate)
				    => @delegate(invocationCount, p1, p2, p3, p4, currentValue)))
			{
				wasInvoked = true;
			}
		}

		return currentValue;
	}

	private TValue ExecuteReturnCallbacks(T1 p1, T2 p2, T3 p3, T4 p4, TValue currentValue, out bool matched)
	{
		matched = false;
		foreach (Callback<Func<int, T1, T2, T3, T4, TValue, TValue>> _ in _returnCallbacks)
		{
			Callback<Func<int, T1, T2, T3, T4, TValue, TValue>> returnCallback =
				_returnCallbacks[_currentReturnCallbackIndex % _returnCallbacks.Count];
			if (returnCallback.Invoke(ref _currentReturnCallbackIndex, (invocationCount, @delegate)
				    => @delegate(invocationCount, p1, p2, p3, p4, currentValue), out TValue? newValue))
			{
				matched = true;
				return newValue!;
			}
		}

		return currentValue;
	}

	private static bool TryExtractParameters(IndexerAccess access,
		out T1 p1, out T2 p2, out T3 p3, out T4 p4)
	{
		if (access is IndexerGetterAccess<T1, T2, T3, T4> getter)
		{
			p1 = getter.Parameter1;
			p2 = getter.Parameter2;
			p3 = getter.Parameter3;
			p4 = getter.Parameter4;
			return true;
		}

		if (access is IndexerSetterAccess<T1, T2, T3, T4, TValue> setter)
		{
			p1 = setter.Parameter1;
			p2 = setter.Parameter2;
			p3 = setter.Parameter3;
			p4 = setter.Parameter4;
			return true;
		}

		p1 = default!;
		p2 = default!;
		p3 = default!;
		p4 = default!;
		return false;
	}
}
