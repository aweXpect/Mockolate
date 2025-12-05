using System;
using System.Collections.Generic;
using Mockolate.Exceptions;
using Mockolate.Interactions;
using Mockolate.Parameters;

namespace Mockolate.Setup;

/// <summary>
///     Sets up a method returning <typeparamref name="TReturn" />.
/// </summary>
public class ReturnMethodSetup<TReturn>(string name) : MethodSetup,
	IReturnMethodSetupCallbackBuilder<TReturn>, IReturnMethodSetupReturnBuilder<TReturn>
{
	private readonly List<Callback<Action<int>>> _callbacks = [];
	private readonly List<Callback<Func<int, TReturn>>> _returnCallbacks = [];
	private bool? _callBaseClass;
	private Callback? _currentCallback;
	private Callback? _currentReturnCallback;
	private int _currentReturnCallbackIndex;

	/// <inheritdoc cref="IReturnMethodSetup{TReturn}.CallingBaseClass(bool)" />
	public IReturnMethodSetup<TReturn> CallingBaseClass(bool callBaseClass = true)
	{
		_callBaseClass = callBaseClass;
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	public IReturnMethodSetupCallbackBuilder<TReturn> Do(Action callback)
	{
		Callback<Action<int>> currentCallback = new(_ => callback());
		_currentCallback = currentCallback;
		_callbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	public IReturnMethodSetupCallbackBuilder<TReturn> Do(Action<int> callback)
	{
		Callback<Action<int>> currentCallback = new(callback);
		_currentCallback = currentCallback;
		_callbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public IReturnMethodSetupReturnBuilder<TReturn> Returns(Func<TReturn> callback)
	{
		Callback<Func<int, TReturn>> currentCallback = new(_ => callback());
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers the <paramref name="returnValue" /> for this method.
	/// </summary>
	public IReturnMethodSetupReturnBuilder<TReturn> Returns(TReturn returnValue)
	{
		Callback<Func<int, TReturn>> currentCallback = new(_ => returnValue);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers an <typeparamref name="TException" /> to throw when the method is invoked.
	/// </summary>
	public IReturnMethodSetupReturnBuilder<TReturn> Throws<TException>()
		where TException : Exception, new()
	{
		Callback<Func<int, TReturn>> currentCallback = new(_ => throw new TException());
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the method is invoked.
	/// </summary>
	public IReturnMethodSetupReturnBuilder<TReturn> Throws(Exception exception)
	{
		Callback<Func<int, TReturn>> currentCallback = new(_ => throw exception);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	public IReturnMethodSetupReturnBuilder<TReturn> Throws(Func<Exception> callback)
	{
		Callback<Func<int, TReturn>> currentCallback = new(_ => throw callback());
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupCallbackBuilder{TReturn}.When(Func{int, bool})" />
	IReturnMethodSetupCallbackWhenBuilder<TReturn> IReturnMethodSetupCallbackBuilder<TReturn>.When(
		Func<int, bool> predicate)
	{
		_currentCallback?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupCallbackWhenBuilder{TReturn}.For(int)" />
	IReturnMethodSetup<TReturn> IReturnMethodSetupCallbackWhenBuilder<TReturn>.For(int times)
	{
		_currentCallback?.For(x => x < times);
		return this;
	}


	/// <inheritdoc cref="IReturnMethodSetupReturnBuilder{TReturn}.When(Func{int, bool})" />
	IReturnMethodSetupReturnWhenBuilder<TReturn> IReturnMethodSetupReturnBuilder<TReturn>.When(
		Func<int, bool> predicate)
	{
		_currentReturnCallback?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupReturnWhenBuilder{TReturn}.For(int)" />
	IReturnMethodSetup<TReturn> IReturnMethodSetupReturnWhenBuilder<TReturn>.For(int times)
	{
		_currentReturnCallback?.For(x => x < times);
		return this;
	}

	/// <inheritdoc cref="MethodSetup.ExecuteCallback(MethodInvocation, MockBehavior)" />
	protected override void ExecuteCallback(MethodInvocation invocation, MockBehavior behavior)
		=> _callbacks.ForEach(callback => callback.Invoke((invocationCount, @delegate)
			=> @delegate(invocationCount)));

	/// <inheritdoc cref="MethodSetup.GetReturnValue{TResult}(MethodInvocation, MockBehavior, Func{TResult})" />
	protected override TResult GetReturnValue<TResult>(MethodInvocation invocation, MockBehavior behavior,
		Func<TResult> defaultValueGenerator)
		where TResult : default
	{
		foreach (Callback<Func<int, TReturn>> _ in _returnCallbacks)
		{
			Callback<Func<int, TReturn>> returnCallback =
				_returnCallbacks[_currentReturnCallbackIndex % _returnCallbacks.Count];
			if (returnCallback.Invoke(ref _currentReturnCallbackIndex, (invocationCount, @delegate)
				    => @delegate(invocationCount), out TReturn? newValue))
			{
				if (newValue is null)
				{
					return default!;
				}

				if (!TryCast(newValue, out TResult returnValue, behavior))
				{
					throw new MockException(
						$"The return callback only supports '{FormatType(typeof(TReturn))}' and not '{FormatType(typeof(TResult))}'.");
				}

				return returnValue;
			}
		}

		return defaultValueGenerator();
	}

	/// <inheritdoc cref="MethodSetup.IsMatch(MethodInvocation)" />
	protected override bool IsMatch(MethodInvocation invocation)
		=> invocation.Name.Equals(name) && invocation.Parameters.Length == 0;

	/// <inheritdoc cref="MethodSetup.TriggerParameterCallbacks(object?[])" />
	protected override void TriggerParameterCallbacks(object?[] parameters)
	{
		// No parameters to trigger
	}

	/// <inheritdoc cref="MethodSetup.GetCallBaseClass()" />
	protected override bool? GetCallBaseClass()
		=> _callBaseClass;

	/// <inheritdoc cref="MethodSetup.HasReturnCalls()" />
	protected override bool HasReturnCalls()
		=> _returnCallbacks.Count > 0;

	/// <inheritdoc cref="MethodSetup.SetOutParameter{T}(string, Func{T})" />
	protected override T SetOutParameter<T>(string parameterName, Func<T> defaultValueGenerator)
		=> defaultValueGenerator();

	/// <inheritdoc cref="MethodSetup.SetRefParameter{T}(string, T, MockBehavior)" />
	protected override T SetRefParameter<T>(string parameterName, T value, MockBehavior behavior)
		=> value;

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString() => $"{FormatType(typeof(TReturn))} {name}()";
}

/// <summary>
///     Setup for a method with one parameter <typeparamref name="T1" /> returning <typeparamref name="TReturn" />.
/// </summary>
public class ReturnMethodSetup<TReturn, T1> : MethodSetup,
	IReturnMethodSetupCallbackBuilder<TReturn, T1>, IReturnMethodSetupReturnBuilder<TReturn, T1>
{
	private readonly List<Callback<Action<int, T1>>> _callbacks = [];
	private readonly NamedParameter? _match1;
	private readonly IParameters? _matches;
	private readonly string _name;
	private readonly List<Callback<Func<int, T1, TReturn>>> _returnCallbacks = [];
	private bool? _callBaseClass;
	private Callback? _currentCallback;
	private Callback? _currentReturnCallback;
	private int _currentReturnCallbackIndex;

	/// <inheritdoc cref="ReturnMethodSetup{TReturn, T1}" />
	public ReturnMethodSetup(string name, NamedParameter match1)
	{
		_name = name;
		_match1 = match1;
	}

	/// <inheritdoc cref="ReturnMethodSetup{TReturn, T1}" />
	public ReturnMethodSetup(string name, IParameters matches)
	{
		_name = name;
		_matches = matches;
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1}.CallingBaseClass(bool)" />
	public IReturnMethodSetup<TReturn, T1> CallingBaseClass(bool callBaseClass = true)
	{
		_callBaseClass = callBaseClass;
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	public IReturnMethodSetupCallbackBuilder<TReturn, T1> Do(Action callback)
	{
		Callback<Action<int, T1>> currentCallback = new((_, _) => callback());
		_currentCallback = currentCallback;
		_callbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	public IReturnMethodSetupCallbackBuilder<TReturn, T1> Do(Action<T1> callback)
	{
		Callback<Action<int, T1>> currentCallback = new((_, p1) => callback(p1));
		_currentCallback = currentCallback;
		_callbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	public IReturnMethodSetupCallbackBuilder<TReturn, T1> Do(Action<int, T1> callback)
	{
		Callback<Action<int, T1>> currentCallback = new(callback);
		_currentCallback = currentCallback;
		_callbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public IReturnMethodSetupReturnBuilder<TReturn, T1> Returns(Func<T1, TReturn> callback)
	{
		Callback<Func<int, T1, TReturn>> currentCallback = new((_, p1) => callback(p1));
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public IReturnMethodSetupReturnBuilder<TReturn, T1> Returns(Func<TReturn> callback)
	{
		Callback<Func<int, T1, TReturn>> currentCallback = new((_, _) => callback());
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers the <paramref name="returnValue" /> for this method.
	/// </summary>
	public IReturnMethodSetupReturnBuilder<TReturn, T1> Returns(TReturn returnValue)
	{
		Callback<Func<int, T1, TReturn>> currentCallback = new((_, _) => returnValue);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers an <typeparamref name="TException" /> to throw when the method is invoked.
	/// </summary>
	public IReturnMethodSetupReturnBuilder<TReturn, T1> Throws<TException>()
		where TException : Exception, new()
	{
		Callback<Func<int, T1, TReturn>> currentCallback = new((_, _) => throw new TException());
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the method is invoked.
	/// </summary>
	public IReturnMethodSetupReturnBuilder<TReturn, T1> Throws(Exception exception)
	{
		Callback<Func<int, T1, TReturn>> currentCallback = new((_, _) => throw exception);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	public IReturnMethodSetupReturnBuilder<TReturn, T1> Throws(Func<Exception> callback)
	{
		Callback<Func<int, T1, TReturn>> currentCallback = new((_, _) => throw callback());
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	public IReturnMethodSetupReturnBuilder<TReturn, T1> Throws(Func<T1, Exception> callback)
	{
		Callback<Func<int, T1, TReturn>> currentCallback = new((_, p1) => throw callback(p1));
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupCallbackBuilder{TReturn, T1}.When(Func{int, bool})" />
	IReturnMethodSetupCallbackWhenBuilder<TReturn, T1> IReturnMethodSetupCallbackBuilder<TReturn, T1>.When(
		Func<int, bool> predicate)
	{
		_currentCallback?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupCallbackWhenBuilder{TReturn, T1}.For(int)" />
	IReturnMethodSetup<TReturn, T1> IReturnMethodSetupCallbackWhenBuilder<TReturn, T1>.For(int times)
	{
		_currentCallback?.For(x => x < times);
		return this;
	}


	/// <inheritdoc cref="IReturnMethodSetupReturnBuilder{TReturn, T1}.When(Func{int, bool})" />
	IReturnMethodSetupReturnWhenBuilder<TReturn, T1> IReturnMethodSetupReturnBuilder<TReturn, T1>.When(
		Func<int, bool> predicate)
	{
		_currentReturnCallback?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupReturnWhenBuilder{TReturn, T1}.For(int)" />
	IReturnMethodSetup<TReturn, T1> IReturnMethodSetupReturnWhenBuilder<TReturn, T1>.For(int times)
	{
		_currentReturnCallback?.For(x => x < times);
		return this;
	}

	/// <inheritdoc cref="MethodSetup.ExecuteCallback(MethodInvocation, MockBehavior)" />
	protected override void ExecuteCallback(MethodInvocation invocation, MockBehavior behavior)
	{
		if (TryCast(invocation.Parameters[0], out T1 p1, behavior))
		{
			_callbacks.ForEach(callback => callback.Invoke((invocationCount, @delegate)
				=> @delegate(invocationCount, p1)));
		}
	}

	/// <inheritdoc cref="MethodSetup.GetReturnValue{TResult}(MethodInvocation, MockBehavior, Func{TResult})" />
	protected override TResult GetReturnValue<TResult>(MethodInvocation invocation, MockBehavior behavior,
		Func<TResult> defaultValueGenerator)
		where TResult : default
	{
		if (_returnCallbacks.Count == 0)
		{
			return defaultValueGenerator();
		}

		if (!TryCast(invocation.Parameters[0], out T1 p1, behavior))
		{
			throw new MockException(
				$"The input parameter only supports '{FormatType(typeof(T1))}', but is '{FormatType(invocation.Parameters[0]!.GetType())}'.");
		}

		foreach (Callback<Func<int, T1, TReturn>> _ in _returnCallbacks)
		{
			Callback<Func<int, T1, TReturn>> returnCallback =
				_returnCallbacks[_currentReturnCallbackIndex % _returnCallbacks.Count];
			if (returnCallback.Invoke(ref _currentReturnCallbackIndex, (invocationCount, @delegate)
				    => @delegate(invocationCount, p1), out TReturn? newValue))
			{
				if (newValue is null)
				{
					return default!;
				}

				if (!TryCast(newValue, out TResult returnValue, behavior))
				{
					throw new MockException(
						$"The return callback only supports '{FormatType(typeof(TReturn))}' and not '{FormatType(typeof(TResult))}'.");
				}

				return returnValue;
			}
		}

		return defaultValueGenerator();
	}

	/// <inheritdoc cref="MethodSetup.IsMatch(MethodInvocation)" />
	protected override bool IsMatch(MethodInvocation invocation)
		=> invocation.Name.Equals(_name) &&
		   (_matches?.Matches(invocation.Parameters)
		    ?? Matches([_match1!,], invocation.Parameters));

	/// <inheritdoc cref="MethodSetup.TriggerParameterCallbacks(object?[])" />
	protected override void TriggerParameterCallbacks(object?[] parameters)
		=> TriggerCallbacks([_match1,], parameters);

	/// <inheritdoc cref="MethodSetup.GetCallBaseClass()" />
	protected override bool? GetCallBaseClass()
		=> _callBaseClass;

	/// <inheritdoc cref="MethodSetup.HasReturnCalls()" />
	protected override bool HasReturnCalls()
		=> _returnCallbacks.Count > 0;

	/// <inheritdoc cref="MethodSetup.SetOutParameter{T}(string, Func{T})" />
	protected override T SetOutParameter<T>(string parameterName, Func<T> defaultValueGenerator)
	{
		if (_match1 is not null &&
		    HasOutParameter([_match1,], parameterName, out IOutParameter<T>? outParameter))
		{
			return outParameter.GetValue(defaultValueGenerator);
		}

		return defaultValueGenerator();
	}

	/// <inheritdoc cref="MethodSetup.SetRefParameter{T}(string, T, MockBehavior)" />
	protected override T SetRefParameter<T>(string parameterName, T value, MockBehavior behavior)
	{
		if (_match1 is not null &&
		    HasRefParameter([_match1,], parameterName, out IRefParameter<T>? refParameter))
		{
			return refParameter.GetValue(value);
		}

		return value;
	}

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> _matches is not null
			? $"{FormatType(typeof(TReturn))} {_name}({_matches})"
			: $"{FormatType(typeof(TReturn))} {_name}({_match1})";
}

/// <summary>
///     Setup for a method with two parameters <typeparamref name="T1" /> and <typeparamref name="T2" /> returning
///     <typeparamref name="TReturn" />.
/// </summary>
public class ReturnMethodSetup<TReturn, T1, T2> : MethodSetup,
	IReturnMethodSetupCallbackBuilder<TReturn, T1, T2>, IReturnMethodSetupReturnBuilder<TReturn, T1, T2>
{
	private readonly List<Callback<Action<int, T1, T2>>> _callbacks = [];
	private readonly NamedParameter? _match1;
	private readonly NamedParameter? _match2;
	private readonly IParameters? _matches;
	private readonly string _name;
	private readonly List<Callback<Func<int, T1, T2, TReturn>>> _returnCallbacks = [];
	private bool? _callBaseClass;
	private Callback? _currentCallback;
	private Callback? _currentReturnCallback;
	private int _currentReturnCallbackIndex;

	/// <inheritdoc cref="ReturnMethodSetup{TReturn, T1, T2}" />
	public ReturnMethodSetup(string name, NamedParameter match1, NamedParameter match2)
	{
		_name = name;
		_match1 = match1;
		_match2 = match2;
	}

	/// <inheritdoc cref="ReturnMethodSetup{TReturn, T1, T2}" />
	public ReturnMethodSetup(string name, IParameters matches)
	{
		_name = name;
		_matches = matches;
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1, T2}.CallingBaseClass(bool)" />
	public IReturnMethodSetup<TReturn, T1, T2> CallingBaseClass(bool callBaseClass = true)
	{
		_callBaseClass = callBaseClass;
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	public IReturnMethodSetupCallbackBuilder<TReturn, T1, T2> Do(Action callback)
	{
		Callback<Action<int, T1, T2>> currentCallback = new((_, _, _) => callback());
		_currentCallback = currentCallback;
		_callbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	public IReturnMethodSetupCallbackBuilder<TReturn, T1, T2> Do(Action<T1, T2> callback)
	{
		Callback<Action<int, T1, T2>> currentCallback = new((_, p1, p2) => callback(p1, p2));
		_currentCallback = currentCallback;
		_callbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	public IReturnMethodSetupCallbackBuilder<TReturn, T1, T2> Do(Action<int, T1, T2> callback)
	{
		Callback<Action<int, T1, T2>> currentCallback = new(callback);
		_currentCallback = currentCallback;
		_callbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public IReturnMethodSetupReturnBuilder<TReturn, T1, T2> Returns(Func<T1, T2, TReturn> callback)
	{
		Callback<Func<int, T1, T2, TReturn>> currentCallback = new((_, p1, p2) => callback(p1, p2));
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public IReturnMethodSetupReturnBuilder<TReturn, T1, T2> Returns(Func<TReturn> callback)
	{
		Callback<Func<int, T1, T2, TReturn>> currentCallback = new((_, _, _) => callback());
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers the <paramref name="returnValue" /> for this method.
	/// </summary>
	public IReturnMethodSetupReturnBuilder<TReturn, T1, T2> Returns(TReturn returnValue)
	{
		Callback<Func<int, T1, T2, TReturn>> currentCallback = new((_, _, _) => returnValue);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers an <typeparamref name="TException" /> to throw when the method is invoked.
	/// </summary>
	public IReturnMethodSetupReturnBuilder<TReturn, T1, T2> Throws<TException>()
		where TException : Exception, new()
	{
		Callback<Func<int, T1, T2, TReturn>> currentCallback = new((_, _, _) => throw new TException());
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the method is invoked.
	/// </summary>
	public IReturnMethodSetupReturnBuilder<TReturn, T1, T2> Throws(Exception exception)
	{
		Callback<Func<int, T1, T2, TReturn>> currentCallback = new((_, _, _) => throw exception);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	public IReturnMethodSetupReturnBuilder<TReturn, T1, T2> Throws(Func<Exception> callback)
	{
		Callback<Func<int, T1, T2, TReturn>> currentCallback = new((_, _, _) => throw callback());
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	public IReturnMethodSetupReturnBuilder<TReturn, T1, T2> Throws(Func<T1, T2, Exception> callback)
	{
		Callback<Func<int, T1, T2, TReturn>> currentCallback = new((_, p1, p2) => throw callback(p1, p2));
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupCallbackBuilder{TReturn, T1, T2}.When(Func{int, bool})" />
	IReturnMethodSetupCallbackWhenBuilder<TReturn, T1, T2> IReturnMethodSetupCallbackBuilder<TReturn, T1, T2>.When(
		Func<int, bool> predicate)
	{
		_currentCallback?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupCallbackWhenBuilder{TReturn, T1, T2}.For(int)" />
	IReturnMethodSetup<TReturn, T1, T2> IReturnMethodSetupCallbackWhenBuilder<TReturn, T1, T2>.For(int times)
	{
		_currentCallback?.For(x => x < times);
		return this;
	}


	/// <inheritdoc cref="IReturnMethodSetupReturnBuilder{TReturn, T1, T2}.When(Func{int, bool})" />
	IReturnMethodSetupReturnWhenBuilder<TReturn, T1, T2> IReturnMethodSetupReturnBuilder<TReturn, T1, T2>.When(
		Func<int, bool> predicate)
	{
		_currentReturnCallback?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupReturnWhenBuilder{TReturn, T1, T2}.For(int)" />
	IReturnMethodSetup<TReturn, T1, T2> IReturnMethodSetupReturnWhenBuilder<TReturn, T1, T2>.For(int times)
	{
		_currentReturnCallback?.For(x => x < times);
		return this;
	}

	/// <inheritdoc cref="MethodSetup.ExecuteCallback(MethodInvocation, MockBehavior)" />
	protected override void ExecuteCallback(MethodInvocation invocation, MockBehavior behavior)
	{
		if (TryCast(invocation.Parameters[0], out T1 p1, behavior) &&
		    TryCast(invocation.Parameters[1], out T2 p2, behavior))
		{
			_callbacks.ForEach(callback => callback.Invoke((invocationCount, @delegate)
				=> @delegate(invocationCount, p1, p2)));
		}
	}

	/// <inheritdoc cref="MethodSetup.GetReturnValue{TResult}(MethodInvocation, MockBehavior, Func{TResult})" />
	protected override TResult GetReturnValue<TResult>(MethodInvocation invocation, MockBehavior behavior,
		Func<TResult> defaultValueGenerator)
		where TResult : default
	{
		if (_returnCallbacks.Count == 0)
		{
			return defaultValueGenerator();
		}

		if (!TryCast(invocation.Parameters[0], out T1 p1, behavior))
		{
			throw new MockException(
				$"The input parameter 1 only supports '{FormatType(typeof(T1))}', but is '{FormatType(invocation.Parameters[0]!.GetType())}'.");
		}

		if (!TryCast(invocation.Parameters[1], out T2 p2, behavior))
		{
			throw new MockException(
				$"The input parameter 2 only supports '{FormatType(typeof(T2))}', but is '{FormatType(invocation.Parameters[1]!.GetType())}'.");
		}

		foreach (Callback<Func<int, T1, T2, TReturn>> _ in _returnCallbacks)
		{
			Callback<Func<int, T1, T2, TReturn>> returnCallback =
				_returnCallbacks[_currentReturnCallbackIndex % _returnCallbacks.Count];
			if (returnCallback.Invoke(ref _currentReturnCallbackIndex, (invocationCount, @delegate)
				    => @delegate(invocationCount, p1, p2), out TReturn? newValue))
			{
				if (newValue is null)
				{
					return default!;
				}

				if (!TryCast(newValue, out TResult returnValue, behavior))
				{
					throw new MockException(
						$"The return callback only supports '{FormatType(typeof(TReturn))}' and not '{FormatType(typeof(TResult))}'.");
				}

				return returnValue;
			}
		}

		return defaultValueGenerator();
	}

	/// <inheritdoc cref="MethodSetup.IsMatch(MethodInvocation)" />
	protected override bool IsMatch(MethodInvocation invocation)
		=> invocation.Name.Equals(_name) &&
		   (_matches?.Matches(invocation.Parameters)
		    ?? Matches([_match1!, _match2!,], invocation.Parameters));

	/// <inheritdoc cref="MethodSetup.TriggerParameterCallbacks(object?[])" />
	protected override void TriggerParameterCallbacks(object?[] parameters)
		=> TriggerCallbacks([_match1, _match2,], parameters);

	/// <inheritdoc cref="MethodSetup.GetCallBaseClass()" />
	protected override bool? GetCallBaseClass()
		=> _callBaseClass;

	/// <inheritdoc cref="MethodSetup.HasReturnCalls()" />
	protected override bool HasReturnCalls()
		=> _returnCallbacks.Count > 0;

	/// <inheritdoc cref="MethodSetup.SetOutParameter{T}(string, Func{T})" />
	protected override T SetOutParameter<T>(string parameterName, Func<T> defaultValueGenerator)
	{
		if (_match1 is not null && _match2 is not null &&
		    HasOutParameter([_match1, _match2,], parameterName, out IOutParameter<T>? outParameter))
		{
			return outParameter.GetValue(defaultValueGenerator);
		}

		return defaultValueGenerator();
	}

	/// <inheritdoc cref="MethodSetup.SetRefParameter{T}(string, T, MockBehavior)" />
	protected override T SetRefParameter<T>(string parameterName, T value, MockBehavior behavior)
	{
		if (_match1 is not null && _match2 is not null &&
		    HasRefParameter([_match1, _match2,], parameterName, out IRefParameter<T>? refParameter))
		{
			return refParameter.GetValue(value);
		}

		return value;
	}

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> _matches is not null
			? $"{FormatType(typeof(TReturn))} {_name}({_matches})"
			: $"{FormatType(typeof(TReturn))} {_name}({_match1}, {_match2})";
}

/// <summary>
///     Setup for a method with three parameters <typeparamref name="T1" />, <typeparamref name="T2" /> and
///     <typeparamref name="T3" /> returning <typeparamref name="TReturn" />.
/// </summary>
public class ReturnMethodSetup<TReturn, T1, T2, T3> : MethodSetup,
	IReturnMethodSetupCallbackBuilder<TReturn, T1, T2, T3>, IReturnMethodSetupReturnBuilder<TReturn, T1, T2, T3>
{
	private readonly List<Callback<Action<int, T1, T2, T3>>> _callbacks = [];
	private readonly NamedParameter? _match1;
	private readonly NamedParameter? _match2;
	private readonly NamedParameter? _match3;
	private readonly IParameters? _matches;
	private readonly string _name;
	private readonly List<Callback<Func<int, T1, T2, T3, TReturn>>> _returnCallbacks = [];
	private bool? _callBaseClass;
	private Callback? _currentCallback;
	private Callback? _currentReturnCallback;
	private int _currentReturnCallbackIndex;

	/// <inheritdoc cref="ReturnMethodSetup{TReturn, T1, T2, T3}" />
	public ReturnMethodSetup(
		string name,
		NamedParameter match1,
		NamedParameter match2,
		NamedParameter match3)
	{
		_name = name;
		_match1 = match1;
		_match2 = match2;
		_match3 = match3;
	}

	/// <inheritdoc cref="ReturnMethodSetup{TReturn, T1, T2, T3}" />
	public ReturnMethodSetup(string name, IParameters matches)
	{
		_name = name;
		_matches = matches;
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1, T2, T3}.CallingBaseClass(bool)" />
	public IReturnMethodSetup<TReturn, T1, T2, T3> CallingBaseClass(bool callBaseClass = true)
	{
		_callBaseClass = callBaseClass;
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	public IReturnMethodSetupCallbackBuilder<TReturn, T1, T2, T3> Do(Action callback)
	{
		Callback<Action<int, T1, T2, T3>> currentCallback = new((_, _, _, _) => callback());
		_currentCallback = currentCallback;
		_callbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	public IReturnMethodSetupCallbackBuilder<TReturn, T1, T2, T3> Do(Action<T1, T2, T3> callback)
	{
		Callback<Action<int, T1, T2, T3>> currentCallback = new((_, p1, p2, p3) => callback(p1, p2, p3));
		_currentCallback = currentCallback;
		_callbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	public IReturnMethodSetupCallbackBuilder<TReturn, T1, T2, T3> Do(Action<int, T1, T2, T3> callback)
	{
		Callback<Action<int, T1, T2, T3>> currentCallback = new(callback);
		_currentCallback = currentCallback;
		_callbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public IReturnMethodSetupReturnBuilder<TReturn, T1, T2, T3> Returns(Func<T1, T2, T3, TReturn> callback)
	{
		Callback<Func<int, T1, T2, T3, TReturn>> currentCallback = new((_, p1, p2, p3) => callback(p1, p2, p3));
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public IReturnMethodSetupReturnBuilder<TReturn, T1, T2, T3> Returns(Func<TReturn> callback)
	{
		Callback<Func<int, T1, T2, T3, TReturn>> currentCallback = new((_, _, _, _) => callback());
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers the <paramref name="returnValue" /> for this method.
	/// </summary>
	public IReturnMethodSetupReturnBuilder<TReturn, T1, T2, T3> Returns(TReturn returnValue)
	{
		Callback<Func<int, T1, T2, T3, TReturn>> currentCallback = new((_, _, _, _) => returnValue);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers an <typeparamref name="TException" /> to throw when the method is invoked.
	/// </summary>
	public IReturnMethodSetupReturnBuilder<TReturn, T1, T2, T3> Throws<TException>()
		where TException : Exception, new()
	{
		Callback<Func<int, T1, T2, T3, TReturn>> currentCallback = new((_, _, _, _) => throw new TException());
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the method is invoked.
	/// </summary>
	public IReturnMethodSetupReturnBuilder<TReturn, T1, T2, T3> Throws(Exception exception)
	{
		Callback<Func<int, T1, T2, T3, TReturn>> currentCallback = new((_, _, _, _) => throw exception);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	public IReturnMethodSetupReturnBuilder<TReturn, T1, T2, T3> Throws(Func<Exception> callback)
	{
		Callback<Func<int, T1, T2, T3, TReturn>> currentCallback = new((_, _, _, _) => throw callback());
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	public IReturnMethodSetupReturnBuilder<TReturn, T1, T2, T3> Throws(Func<T1, T2, T3, Exception> callback)
	{
		Callback<Func<int, T1, T2, T3, TReturn>> currentCallback =
			new((_, p1, p2, p3) => throw callback(p1, p2, p3));
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupCallbackBuilder{TReturn, T1, T2, T3}.When(Func{int, bool})" />
	IReturnMethodSetupCallbackWhenBuilder<TReturn, T1, T2, T3> IReturnMethodSetupCallbackBuilder<TReturn, T1, T2, T3>.
		When(Func<int, bool> predicate)
	{
		_currentCallback?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupCallbackWhenBuilder{TReturn, T1, T2, T3}.For(int)" />
	IReturnMethodSetup<TReturn, T1, T2, T3> IReturnMethodSetupCallbackWhenBuilder<TReturn, T1, T2, T3>.For(int times)
	{
		_currentCallback?.For(x => x < times);
		return this;
	}


	/// <inheritdoc cref="IReturnMethodSetupReturnBuilder{TReturn, T1, T2, T3}.When(Func{int, bool})" />
	IReturnMethodSetupReturnWhenBuilder<TReturn, T1, T2, T3> IReturnMethodSetupReturnBuilder<TReturn, T1, T2, T3>.When(
		Func<int, bool> predicate)
	{
		_currentReturnCallback?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupReturnWhenBuilder{TReturn, T1, T2, T3}.For(int)" />
	IReturnMethodSetup<TReturn, T1, T2, T3> IReturnMethodSetupReturnWhenBuilder<TReturn, T1, T2, T3>.For(int times)
	{
		_currentReturnCallback?.For(x => x < times);
		return this;
	}

	/// <inheritdoc cref="MethodSetup.ExecuteCallback(MethodInvocation, MockBehavior)" />
	protected override void ExecuteCallback(MethodInvocation invocation, MockBehavior behavior)
	{
		if (TryCast(invocation.Parameters[0], out T1 p1, behavior) &&
		    TryCast(invocation.Parameters[1], out T2 p2, behavior) &&
		    TryCast(invocation.Parameters[2], out T3 p3, behavior))
		{
			_callbacks.ForEach(callback => callback.Invoke((invocationCount, @delegate)
				=> @delegate(invocationCount, p1, p2, p3)));
		}
	}

	/// <inheritdoc cref="MethodSetup.GetReturnValue{TResult}(MethodInvocation, MockBehavior, Func{TResult})" />
	protected override TResult GetReturnValue<TResult>(MethodInvocation invocation, MockBehavior behavior,
		Func<TResult> defaultValueGenerator)
		where TResult : default
	{
		if (_returnCallbacks.Count == 0)
		{
			return defaultValueGenerator();
		}

		if (!TryCast(invocation.Parameters[0], out T1 p1, behavior))
		{
			throw new MockException(
				$"The input parameter 1 only supports '{FormatType(typeof(T1))}', but is '{FormatType(invocation.Parameters[0]!.GetType())}'.");
		}

		if (!TryCast(invocation.Parameters[1], out T2 p2, behavior))
		{
			throw new MockException(
				$"The input parameter 2 only supports '{FormatType(typeof(T2))}', but is '{FormatType(invocation.Parameters[1]!.GetType())}'.");
		}

		if (!TryCast(invocation.Parameters[2], out T3 p3, behavior))
		{
			throw new MockException(
				$"The input parameter 3 only supports '{FormatType(typeof(T3))}', but is '{FormatType(invocation.Parameters[2]!.GetType())}'.");
		}

		foreach (Callback<Func<int, T1, T2, T3, TReturn>> _ in _returnCallbacks)
		{
			Callback<Func<int, T1, T2, T3, TReturn>> returnCallback =
				_returnCallbacks[_currentReturnCallbackIndex % _returnCallbacks.Count];
			if (returnCallback.Invoke(ref _currentReturnCallbackIndex, (invocationCount, @delegate)
				    => @delegate(invocationCount, p1, p2, p3), out TReturn? newValue))
			{
				if (newValue is null)
				{
					return default!;
				}

				if (!TryCast(newValue, out TResult returnValue, behavior))
				{
					throw new MockException(
						$"The return callback only supports '{FormatType(typeof(TReturn))}' and not '{FormatType(typeof(TResult))}'.");
				}

				return returnValue;
			}
		}

		return defaultValueGenerator();
	}

	/// <inheritdoc cref="MethodSetup.IsMatch(MethodInvocation)" />
	protected override bool IsMatch(MethodInvocation invocation)
		=> invocation.Name.Equals(_name) &&
		   (_matches?.Matches(invocation.Parameters)
		    ?? Matches([_match1!, _match2!, _match3!,], invocation.Parameters));

	/// <inheritdoc cref="MethodSetup.TriggerParameterCallbacks(object?[])" />
	protected override void TriggerParameterCallbacks(object?[] parameters)
		=> TriggerCallbacks([_match1, _match2, _match3,], parameters);

	/// <inheritdoc cref="MethodSetup.GetCallBaseClass()" />
	protected override bool? GetCallBaseClass()
		=> _callBaseClass;

	/// <inheritdoc cref="MethodSetup.HasReturnCalls()" />
	protected override bool HasReturnCalls()
		=> _returnCallbacks.Count > 0;

	/// <inheritdoc cref="MethodSetup.SetOutParameter{T}(string, Func{T})" />
	protected override T SetOutParameter<T>(string parameterName, Func<T> defaultValueGenerator)
	{
		if (_match1 is not null && _match2 is not null && _match3 is not null &&
		    HasOutParameter([_match1, _match2, _match3,], parameterName, out IOutParameter<T>? outParameter))
		{
			return outParameter.GetValue(defaultValueGenerator);
		}

		return defaultValueGenerator();
	}

	/// <inheritdoc cref="MethodSetup.SetRefParameter{T}(string, T, MockBehavior)" />
	protected override T SetRefParameter<T>(string parameterName, T value, MockBehavior behavior)
	{
		if (_match1 is not null && _match2 is not null && _match3 is not null &&
		    HasRefParameter([_match1, _match2, _match3,], parameterName, out IRefParameter<T>? refParameter))
		{
			return refParameter.GetValue(value);
		}

		return value;
	}

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> _matches is not null
			? $"{FormatType(typeof(TReturn))} {_name}({_matches})"
			: $"{FormatType(typeof(TReturn))} {_name}({_match1}, {_match2}, {_match3})";
}

#pragma warning disable S2436 // Types and methods should not have too many generic parameters
/// <summary>
///     Setup for a method with four parameters <typeparamref name="T1" />, <typeparamref name="T2" />,
///     <typeparamref name="T3" /> and <typeparamref name="T4" /> returning <typeparamref name="TReturn" />.
/// </summary>
public class ReturnMethodSetup<TReturn, T1, T2, T3, T4> : MethodSetup,
	IReturnMethodSetupCallbackBuilder<TReturn, T1, T2, T3, T4>, IReturnMethodSetupReturnBuilder<TReturn, T1, T2, T3, T4>
{
	private readonly List<Callback<Action<int, T1, T2, T3, T4>>> _callbacks = [];
	private readonly NamedParameter? _match1;
	private readonly NamedParameter? _match2;
	private readonly NamedParameter? _match3;
	private readonly NamedParameter? _match4;
	private readonly IParameters? _matches;
	private readonly string _name;
	private readonly List<Callback<Func<int, T1, T2, T3, T4, TReturn>>> _returnCallbacks = [];
	private bool? _callBaseClass;
	private Callback? _currentCallback;
	private Callback? _currentReturnCallback;
	private int _currentReturnCallbackIndex;

	/// <inheritdoc cref="ReturnMethodSetup{TReturn, T1, T2, T3, T4}" />
	public ReturnMethodSetup(
		string name,
		NamedParameter match1,
		NamedParameter match2,
		NamedParameter match3,
		NamedParameter match4)
	{
		_name = name;
		_match1 = match1;
		_match2 = match2;
		_match3 = match3;
		_match4 = match4;
	}

	/// <inheritdoc cref="ReturnMethodSetup{TReturn, T1, T2, T3, T4}" />
	public ReturnMethodSetup(string name, IParameters matches)
	{
		_name = name;
		_matches = matches;
	}

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1, T2, T3, T4}.CallingBaseClass(bool)" />
	public IReturnMethodSetup<TReturn, T1, T2, T3, T4> CallingBaseClass(bool callBaseClass = true)
	{
		_callBaseClass = callBaseClass;
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	public IReturnMethodSetupCallbackBuilder<TReturn, T1, T2, T3, T4> Do(Action callback)
	{
		Callback<Action<int, T1, T2, T3, T4>> currentCallback = new((_, _, _, _, _) => callback());
		_currentCallback = currentCallback;
		_callbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	public IReturnMethodSetupCallbackBuilder<TReturn, T1, T2, T3, T4> Do(Action<T1, T2, T3, T4> callback)
	{
		Callback<Action<int, T1, T2, T3, T4>> currentCallback = new((_, p1, p2, p3, p4) => callback(p1, p2, p3, p4));
		_currentCallback = currentCallback;
		_callbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	public IReturnMethodSetupCallbackBuilder<TReturn, T1, T2, T3, T4> Do(Action<int, T1, T2, T3, T4> callback)
	{
		Callback<Action<int, T1, T2, T3, T4>> currentCallback = new(callback);
		_currentCallback = currentCallback;
		_callbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public IReturnMethodSetupReturnBuilder<TReturn, T1, T2, T3, T4> Returns(Func<T1, T2, T3, T4, TReturn> callback)
	{
		Callback<Func<int, T1, T2, T3, T4, TReturn>> currentCallback =
			new((_, p1, p2, p3, p4) => callback(p1, p2, p3, p4));
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public IReturnMethodSetupReturnBuilder<TReturn, T1, T2, T3, T4> Returns(Func<TReturn> callback)
	{
		Callback<Func<int, T1, T2, T3, T4, TReturn>> currentCallback = new((_, _, _, _, _) => callback());
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers the <paramref name="returnValue" /> for this method.
	/// </summary>
	public IReturnMethodSetupReturnBuilder<TReturn, T1, T2, T3, T4> Returns(TReturn returnValue)
	{
		Callback<Func<int, T1, T2, T3, T4, TReturn>> currentCallback = new((_, _, _, _, _) => returnValue);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers an <typeparamref name="TException" /> to throw when the method is invoked.
	/// </summary>
	public IReturnMethodSetupReturnBuilder<TReturn, T1, T2, T3, T4> Throws<TException>()
		where TException : Exception, new()
	{
		Callback<Func<int, T1, T2, T3, T4, TReturn>> currentCallback =
			new((_, _, _, _, _) => throw new TException());
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the method is invoked.
	/// </summary>
	public IReturnMethodSetupReturnBuilder<TReturn, T1, T2, T3, T4> Throws(Exception exception)
	{
		Callback<Func<int, T1, T2, T3, T4, TReturn>> currentCallback = new((_, _, _, _, _) => throw exception);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	public IReturnMethodSetupReturnBuilder<TReturn, T1, T2, T3, T4> Throws(Func<Exception> callback)
	{
		Callback<Func<int, T1, T2, T3, T4, TReturn>> currentCallback = new((_, _, _, _, _) => throw callback());
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	public IReturnMethodSetupReturnBuilder<TReturn, T1, T2, T3, T4> Throws(Func<T1, T2, T3, T4, Exception> callback)
	{
		Callback<Func<int, T1, T2, T3, T4, TReturn>> currentCallback =
			new((_, p1, p2, p3, p4) => throw callback(p1, p2, p3, p4));
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupCallbackBuilder{TReturn, T1, T2, T3, T4}.When(Func{int, bool})" />
	IReturnMethodSetupCallbackWhenBuilder<TReturn, T1, T2, T3, T4>
		IReturnMethodSetupCallbackBuilder<TReturn, T1, T2, T3, T4>.When(Func<int, bool> predicate)
	{
		_currentCallback?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupCallbackWhenBuilder{TReturn, T1, T2, T3, T4}.For(int)" />
	IReturnMethodSetup<TReturn, T1, T2, T3, T4> IReturnMethodSetupCallbackWhenBuilder<TReturn, T1, T2, T3, T4>.
		For(int times)
	{
		_currentCallback?.For(x => x < times);
		return this;
	}


	/// <inheritdoc cref="IReturnMethodSetupReturnBuilder{TReturn, T1, T2, T3, T4}.When(Func{int, bool})" />
	IReturnMethodSetupReturnWhenBuilder<TReturn, T1, T2, T3, T4>
		IReturnMethodSetupReturnBuilder<TReturn, T1, T2, T3, T4>.When(Func<int, bool> predicate)
	{
		_currentReturnCallback?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupReturnWhenBuilder{TReturn, T1, T2, T3, T4}.For(int)" />
	IReturnMethodSetup<TReturn, T1, T2, T3, T4> IReturnMethodSetupReturnWhenBuilder<TReturn, T1, T2, T3, T4>.
		For(int times)
	{
		_currentReturnCallback?.For(x => x < times);
		return this;
	}

	/// <inheritdoc cref="MethodSetup.ExecuteCallback(MethodInvocation, MockBehavior)" />
	protected override void ExecuteCallback(MethodInvocation invocation, MockBehavior behavior)
	{
		if (TryCast(invocation.Parameters[0], out T1 p1, behavior) &&
		    TryCast(invocation.Parameters[1], out T2 p2, behavior) &&
		    TryCast(invocation.Parameters[2], out T3 p3, behavior) &&
		    TryCast(invocation.Parameters[3], out T4 p4, behavior))
		{
			_callbacks.ForEach(callback => callback.Invoke((invocationCount, @delegate)
				=> @delegate(invocationCount, p1, p2, p3, p4)));
		}
	}

	/// <inheritdoc cref="MethodSetup.GetReturnValue{TResult}(MethodInvocation, MockBehavior, Func{TResult})" />
	protected override TResult GetReturnValue<TResult>(MethodInvocation invocation, MockBehavior behavior,
		Func<TResult> defaultValueGenerator)
		where TResult : default
	{
		if (_returnCallbacks.Count == 0)
		{
			return defaultValueGenerator();
		}

		if (!TryCast(invocation.Parameters[0], out T1 p1, behavior))
		{
			throw new MockException(
				$"The input parameter 1 only supports '{FormatType(typeof(T1))}', but is '{FormatType(invocation.Parameters[0]!.GetType())}'.");
		}

		if (!TryCast(invocation.Parameters[1], out T2 p2, behavior))
		{
			throw new MockException(
				$"The input parameter 2 only supports '{FormatType(typeof(T2))}', but is '{FormatType(invocation.Parameters[1]!.GetType())}'.");
		}

		if (!TryCast(invocation.Parameters[2], out T3 p3, behavior))
		{
			throw new MockException(
				$"The input parameter 3 only supports '{FormatType(typeof(T3))}', but is '{FormatType(invocation.Parameters[2]!.GetType())}'.");
		}

		if (!TryCast(invocation.Parameters[3], out T4 p4, behavior))
		{
			throw new MockException(
				$"The input parameter 4 only supports '{FormatType(typeof(T4))}', but is '{FormatType(invocation.Parameters[3]!.GetType())}'.");
		}

		foreach (Callback<Func<int, T1, T2, T3, T4, TReturn>> _ in _returnCallbacks)
		{
			Callback<Func<int, T1, T2, T3, T4, TReturn>> returnCallback =
				_returnCallbacks[_currentReturnCallbackIndex % _returnCallbacks.Count];
			if (returnCallback.Invoke(ref _currentReturnCallbackIndex, (invocationCount, @delegate)
				    => @delegate(invocationCount, p1, p2, p3, p4), out TReturn? newValue))
			{
				if (newValue is null)
				{
					return default!;
				}

				if (!TryCast(newValue, out TResult returnValue, behavior))
				{
					throw new MockException(
						$"The return callback only supports '{FormatType(typeof(TReturn))}' and not '{FormatType(typeof(TResult))}'.");
				}

				return returnValue;
			}
		}

		return defaultValueGenerator();
	}

	/// <inheritdoc cref="MethodSetup.IsMatch(MethodInvocation)" />
	protected override bool IsMatch(MethodInvocation invocation)
		=> invocation.Name.Equals(_name) &&
		   (_matches?.Matches(invocation.Parameters)
		    ?? Matches([_match1!, _match2!, _match3!, _match4!,], invocation.Parameters));

	/// <inheritdoc cref="MethodSetup.TriggerParameterCallbacks(object?[])" />
	protected override void TriggerParameterCallbacks(object?[] parameters)
		=> TriggerCallbacks([_match1, _match2, _match3, _match4,], parameters);

	/// <inheritdoc cref="MethodSetup.GetCallBaseClass()" />
	protected override bool? GetCallBaseClass()
		=> _callBaseClass;

	/// <inheritdoc cref="MethodSetup.HasReturnCalls()" />
	protected override bool HasReturnCalls()
		=> _returnCallbacks.Count > 0;

	/// <inheritdoc cref="MethodSetup.SetOutParameter{T}(string, Func{T})" />
	protected override T SetOutParameter<T>(string parameterName, Func<T> defaultValueGenerator)
	{
		if (_match1 is not null && _match2 is not null && _match3 is not null && _match4 is not null &&
		    HasOutParameter([_match1, _match2, _match3, _match4,], parameterName,
			    out IOutParameter<T>? outParameter))
		{
			return outParameter.GetValue(defaultValueGenerator);
		}

		return defaultValueGenerator();
	}

	/// <inheritdoc cref="MethodSetup.SetRefParameter{T}(string, T, MockBehavior)" />
	protected override T SetRefParameter<T>(string parameterName, T value, MockBehavior behavior)
	{
		if (_match1 is not null && _match2 is not null && _match3 is not null && _match4 is not null &&
		    HasRefParameter([_match1, _match2, _match3, _match4,], parameterName,
			    out IRefParameter<T>? refParameter))
		{
			return refParameter.GetValue(value);
		}

		return value;
	}

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> _matches is not null
			? $"{FormatType(typeof(TReturn))} {_name}({_matches})"
			: $"{FormatType(typeof(TReturn))} {_name}({_match1}, {_match2}, {_match3}, {_match4})";
}
#pragma warning restore S2436 // Types and methods should not have too many generic parameters
