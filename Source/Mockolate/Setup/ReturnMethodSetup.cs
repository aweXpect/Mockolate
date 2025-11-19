using System;
using System.Collections.Generic;
using System.Threading;
using Mockolate.Exceptions;
using Mockolate.Interactions;

namespace Mockolate.Setup;

/// <summary>
///     Sets up a method returning <typeparamref name="TReturn" />.
/// </summary>
public class ReturnMethodSetup<TReturn>(string name) : MethodSetup, IReturnMethodSetupCallbackBuilder<TReturn>
{
	private readonly List<Callback<Action<int>>> _callbacks = [];
	private readonly List<Func<TReturn>> _returnCallbacks = [];
	private bool? _callBaseClass;
	private Callback? _currentCallback;
	private int _currentReturnCallbackIndex = -1;

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
	public IReturnMethodSetup<TReturn> Returns(Func<TReturn> callback)
	{
		_returnCallbacks.Add(callback);
		return this;
	}

	/// <summary>
	///     Registers the <paramref name="returnValue" /> for this method.
	/// </summary>
	public IReturnMethodSetup<TReturn> Returns(TReturn returnValue)
	{
		_returnCallbacks.Add(() => returnValue);
		return this;
	}

	/// <summary>
	///     Registers an <typeparamref name="TException" /> to throw when the method is invoked.
	/// </summary>
	public IReturnMethodSetup<TReturn> Throws<TException>()
		where TException : Exception, new()
	{
		_returnCallbacks.Add(() => throw new TException());
		return this;
	}

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the method is invoked.
	/// </summary>
	public IReturnMethodSetup<TReturn> Throws(Exception exception)
	{
		_returnCallbacks.Add(() => throw exception);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	public IReturnMethodSetup<TReturn> Throws(Func<Exception> callback)
	{
		_returnCallbacks.Add(() => throw callback());
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupCallbackBuilder{TReturn}.When(Func{int, bool})" />
	public IReturnMethodSetupCallbackWhenBuilder<TReturn> When(Func<int, bool> predicate)
	{
		_currentCallback?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupCallbackWhenBuilder{TReturn}.For(int)" />
	public IReturnMethodSetup<TReturn> For(int times)
	{
		_currentCallback?.For(x => x < times);
		return this;
	}

	/// <inheritdoc cref="MethodSetup.ExecuteCallback(MethodInvocation, MockBehavior)" />
	protected override void ExecuteCallback(MethodInvocation invocation, MockBehavior behavior)
		=> _callbacks.ForEach(callback => callback.Invoke((invocationCount, @delegate)
			=> @delegate(invocationCount)));

	/// <inheritdoc cref="MethodSetup.GetReturnValue{TResult}(MethodInvocation, MockBehavior)" />
	protected override TResult GetReturnValue<TResult>(MethodInvocation invocation, MockBehavior behavior)
		where TResult : default
	{
		if (_returnCallbacks.Count == 0)
		{
			return behavior.DefaultValue.Generate<TResult>();
		}

		int index = Interlocked.Increment(ref _currentReturnCallbackIndex);
		Func<TReturn> returnCallback = _returnCallbacks[index % _returnCallbacks.Count];

		TReturn returnValue = returnCallback();
		if (returnValue is null)
		{
			return default!;
		}

		if (returnValue is TResult result)
		{
			return result;
		}

		throw new MockException(
			$"The return callback only supports '{FormatType(typeof(TReturn))}' and not '{FormatType(typeof(TResult))}'.");
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

	/// <inheritdoc cref="MethodSetup.SetOutParameter{T}(string, MockBehavior)" />
	protected override T SetOutParameter<T>(string parameterName, MockBehavior behavior)
		=> behavior.DefaultValue.Generate<T>();

	/// <inheritdoc cref="MethodSetup.SetRefParameter{T}(string, T, MockBehavior)" />
	protected override T SetRefParameter<T>(string parameterName, T value, MockBehavior behavior)
		=> value;

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString() => $"{FormatType(typeof(TReturn))} {name}()";
}

/// <summary>
///     Setup for a method with one parameter <typeparamref name="T1" /> returning <typeparamref name="TReturn" />.
/// </summary>
public class ReturnMethodSetup<TReturn, T1> : MethodSetup, IReturnMethodSetupCallbackBuilder<TReturn, T1>
{
	private readonly List<Callback<Action<int, T1>>> _callbacks = [];
	private readonly Match.NamedParameter? _match1;
	private readonly Match.IParameters? _matches;
	private readonly string _name;
	private readonly List<Func<T1, TReturn>> _returnCallbacks = [];
	private bool? _callBaseClass;
	private Callback? _currentCallback;
	private int _currentReturnCallbackIndex = -1;

	/// <inheritdoc cref="ReturnMethodSetup{TReturn, T1}" />
	public ReturnMethodSetup(string name, Match.NamedParameter match1)
	{
		_name = name;
		_match1 = match1;
	}

	/// <inheritdoc cref="ReturnMethodSetup{TReturn, T1}" />
	public ReturnMethodSetup(string name, Match.IParameters matches)
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
	public IReturnMethodSetup<TReturn, T1> Returns(Func<T1, TReturn> callback)
	{
		_returnCallbacks.Add(callback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public IReturnMethodSetup<TReturn, T1> Returns(Func<TReturn> callback)
	{
		_returnCallbacks.Add(_ => callback());
		return this;
	}

	/// <summary>
	///     Registers the <paramref name="returnValue" /> for this method.
	/// </summary>
	public IReturnMethodSetup<TReturn, T1> Returns(TReturn returnValue)
	{
		_returnCallbacks.Add(_ => returnValue);
		return this;
	}

	/// <summary>
	///     Registers an <typeparamref name="TException" /> to throw when the method is invoked.
	/// </summary>
	public IReturnMethodSetup<TReturn, T1> Throws<TException>()
		where TException : Exception, new()
	{
		_returnCallbacks.Add(_ => throw new TException());
		return this;
	}

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the method is invoked.
	/// </summary>
	public IReturnMethodSetup<TReturn, T1> Throws(Exception exception)
	{
		_returnCallbacks.Add(_ => throw exception);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	public IReturnMethodSetup<TReturn, T1> Throws(Func<Exception> callback)
	{
		_returnCallbacks.Add(_ => throw callback());
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	public IReturnMethodSetup<TReturn, T1> Throws(Func<T1, Exception> callback)
	{
		_returnCallbacks.Add(v1 => throw callback(v1));
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupCallbackBuilder{TReturn, T1}.When(Func{int, bool})" />
	public IReturnMethodSetupCallbackWhenBuilder<TReturn, T1> When(Func<int, bool> predicate)
	{
		_currentCallback?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupCallbackWhenBuilder{TReturn, T1}.For(int)" />
	public IReturnMethodSetup<TReturn, T1> For(int times)
	{
		_currentCallback?.For(x => x < times);
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

	/// <inheritdoc cref="MethodSetup.GetReturnValue{TResult}(MethodInvocation, MockBehavior)" />
	protected override TResult GetReturnValue<TResult>(MethodInvocation invocation, MockBehavior behavior)
		where TResult : default
	{
		if (_returnCallbacks.Count == 0)
		{
			return behavior.DefaultValue.Generate<TResult>();
		}

		if (!TryCast(invocation.Parameters[0], out T1 p1, behavior))
		{
			throw new MockException(
				$"The input parameter only supports '{FormatType(typeof(T1))}', but is '{FormatType(invocation.Parameters[0]!.GetType())}'.");
		}

		int index = Interlocked.Increment(ref _currentReturnCallbackIndex);
		Func<T1, TReturn> returnCallback = _returnCallbacks[index % _returnCallbacks.Count];

		TReturn returnValue = returnCallback(p1);
		if (returnValue is null)
		{
			return default!;
		}

		if (returnValue is TResult result)
		{
			return result;
		}

		throw new MockException(
			$"The return callback only supports '{FormatType(typeof(TReturn))}' and not '{FormatType(typeof(TResult))}'.");
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

	/// <inheritdoc cref="MethodSetup.SetOutParameter{T}(string, MockBehavior)" />
	protected override T SetOutParameter<T>(string parameterName, MockBehavior behavior)
	{
		if (_match1 is not null &&
		    HasOutParameter([_match1,], parameterName, out Match.IOutParameter<T>? outParameter))
		{
			return outParameter.GetValue(behavior);
		}

		return behavior.DefaultValue.Generate<T>();
	}

	/// <inheritdoc cref="MethodSetup.SetRefParameter{T}(string, T, MockBehavior)" />
	protected override T SetRefParameter<T>(string parameterName, T value, MockBehavior behavior)
	{
		if (_match1 is not null &&
		    HasRefParameter([_match1,], parameterName, out Match.IRefParameter<T>? refParameter))
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
public class ReturnMethodSetup<TReturn, T1, T2> : MethodSetup, IReturnMethodSetupCallbackBuilder<TReturn, T1, T2>
{
	private readonly List<Callback<Action<int, T1, T2>>> _callbacks = [];
	private readonly Match.NamedParameter? _match1;
	private readonly Match.NamedParameter? _match2;
	private readonly Match.IParameters? _matches;
	private readonly string _name;
	private readonly List<Func<T1, T2, TReturn>> _returnCallbacks = [];
	private bool? _callBaseClass;
	private Callback? _currentCallback;
	private int _currentReturnCallbackIndex = -1;

	/// <inheritdoc cref="ReturnMethodSetup{TReturn, T1, T2}" />
	public ReturnMethodSetup(string name, Match.NamedParameter match1, Match.NamedParameter match2)
	{
		_name = name;
		_match1 = match1;
		_match2 = match2;
	}

	/// <inheritdoc cref="ReturnMethodSetup{TReturn, T1, T2}" />
	public ReturnMethodSetup(string name, Match.IParameters matches)
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
	public IReturnMethodSetup<TReturn, T1, T2> Returns(Func<T1, T2, TReturn> callback)
	{
		_returnCallbacks.Add(callback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public IReturnMethodSetup<TReturn, T1, T2> Returns(Func<TReturn> callback)
	{
		_returnCallbacks.Add((_, _) => callback());
		return this;
	}

	/// <summary>
	///     Registers the <paramref name="returnValue" /> for this method.
	/// </summary>
	public IReturnMethodSetup<TReturn, T1, T2> Returns(TReturn returnValue)
	{
		_returnCallbacks.Add((_, _) => returnValue);
		return this;
	}

	/// <summary>
	///     Registers an <typeparamref name="TException" /> to throw when the method is invoked.
	/// </summary>
	public IReturnMethodSetup<TReturn, T1, T2> Throws<TException>()
		where TException : Exception, new()
	{
		_returnCallbacks.Add((_, _) => throw new TException());
		return this;
	}

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the method is invoked.
	/// </summary>
	public IReturnMethodSetup<TReturn, T1, T2> Throws(Exception exception)
	{
		_returnCallbacks.Add((_, _) => throw exception);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	public IReturnMethodSetup<TReturn, T1, T2> Throws(Func<Exception> callback)
	{
		_returnCallbacks.Add((_, _) => throw callback());
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	public IReturnMethodSetup<TReturn, T1, T2> Throws(Func<T1, T2, Exception> callback)
	{
		_returnCallbacks.Add((v1, v2) => throw callback(v1, v2));
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupCallbackBuilder{TReturn, T1, T2}.When(Func{int, bool})" />
	public IReturnMethodSetupCallbackWhenBuilder<TReturn, T1, T2> When(Func<int, bool> predicate)
	{
		_currentCallback?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupCallbackWhenBuilder{TReturn, T1, T2}.For(int)" />
	public IReturnMethodSetup<TReturn, T1, T2> For(int times)
	{
		_currentCallback?.For(x => x < times);
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

	/// <inheritdoc cref="MethodSetup.GetReturnValue{TResult}(MethodInvocation, MockBehavior)" />
	protected override TResult GetReturnValue<TResult>(MethodInvocation invocation, MockBehavior behavior)
		where TResult : default
	{
		if (_returnCallbacks.Count == 0)
		{
			return behavior.DefaultValue.Generate<TResult>();
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

		int index = Interlocked.Increment(ref _currentReturnCallbackIndex);
		Func<T1, T2, TReturn> returnCallback = _returnCallbacks[index % _returnCallbacks.Count];

		TReturn returnValue = returnCallback(p1, p2);
		if (returnValue is null)
		{
			return default!;
		}

		if (returnValue is TResult result)
		{
			return result;
		}

		throw new MockException(
			$"The return callback only supports '{FormatType(typeof(TReturn))}' and not '{FormatType(typeof(TResult))}'.");
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

	/// <inheritdoc cref="MethodSetup.SetOutParameter{T}(string, MockBehavior)" />
	protected override T SetOutParameter<T>(string parameterName, MockBehavior behavior)
	{
		if (_match1 is not null && _match2 is not null &&
		    HasOutParameter([_match1, _match2,], parameterName, out Match.IOutParameter<T>? outParameter))
		{
			return outParameter.GetValue(behavior);
		}

		return behavior.DefaultValue.Generate<T>();
	}

	/// <inheritdoc cref="MethodSetup.SetRefParameter{T}(string, T, MockBehavior)" />
	protected override T SetRefParameter<T>(string parameterName, T value, MockBehavior behavior)
	{
		if (_match1 is not null && _match2 is not null &&
		    HasRefParameter([_match1, _match2,], parameterName, out Match.IRefParameter<T>? refParameter))
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
public class ReturnMethodSetup<TReturn, T1, T2, T3> : MethodSetup, IReturnMethodSetupCallbackBuilder<TReturn, T1, T2, T3>
{
	private readonly List<Callback<Action<int, T1, T2, T3>>> _callbacks = [];
	private readonly Match.NamedParameter? _match1;
	private readonly Match.NamedParameter? _match2;
	private readonly Match.NamedParameter? _match3;
	private readonly Match.IParameters? _matches;
	private readonly string _name;
	private readonly List<Func<T1, T2, T3, TReturn>> _returnCallbacks = [];
	private bool? _callBaseClass;
	private Callback? _currentCallback;
	private int _currentReturnCallbackIndex = -1;

	/// <inheritdoc cref="ReturnMethodSetup{TReturn, T1, T2, T3}" />
	public ReturnMethodSetup(
		string name,
		Match.NamedParameter match1,
		Match.NamedParameter match2,
		Match.NamedParameter match3)
	{
		_name = name;
		_match1 = match1;
		_match2 = match2;
		_match3 = match3;
	}

	/// <inheritdoc cref="ReturnMethodSetup{TReturn, T1, T2, T3}" />
	public ReturnMethodSetup(string name, Match.IParameters matches)
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
	public IReturnMethodSetup<TReturn, T1, T2, T3> Returns(Func<T1, T2, T3, TReturn> callback)
	{
		_returnCallbacks.Add(callback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public IReturnMethodSetup<TReturn, T1, T2, T3> Returns(Func<TReturn> callback)
	{
		_returnCallbacks.Add((_, _, _) => callback());
		return this;
	}

	/// <summary>
	///     Registers the <paramref name="returnValue" /> for this method.
	/// </summary>
	public IReturnMethodSetup<TReturn, T1, T2, T3> Returns(TReturn returnValue)
	{
		_returnCallbacks.Add((_, _, _) => returnValue);
		return this;
	}

	/// <summary>
	///     Registers an <typeparamref name="TException" /> to throw when the method is invoked.
	/// </summary>
	public IReturnMethodSetup<TReturn, T1, T2, T3> Throws<TException>()
		where TException : Exception, new()
	{
		_returnCallbacks.Add((_, _, _) => throw new TException());
		return this;
	}

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the method is invoked.
	/// </summary>
	public IReturnMethodSetup<TReturn, T1, T2, T3> Throws(Exception exception)
	{
		_returnCallbacks.Add((_, _, _) => throw exception);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	public IReturnMethodSetup<TReturn, T1, T2, T3> Throws(Func<Exception> callback)
	{
		_returnCallbacks.Add((_, _, _) => throw callback());
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	public IReturnMethodSetup<TReturn, T1, T2, T3> Throws(Func<T1, T2, T3, Exception> callback)
	{
		_returnCallbacks.Add((v1, v2, v3) => throw callback(v1, v2, v3));
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupCallbackBuilder{TReturn, T1, T2, T3}.When(Func{int, bool})" />
	public IReturnMethodSetupCallbackWhenBuilder<TReturn, T1, T2, T3> When(Func<int, bool> predicate)
	{
		_currentCallback?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupCallbackWhenBuilder{TReturn, T1, T2, T3}.For(int)" />
	public IReturnMethodSetup<TReturn, T1, T2, T3> For(int times)
	{
		_currentCallback?.For(x => x < times);
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

	/// <inheritdoc cref="MethodSetup.GetReturnValue{TResult}(MethodInvocation, MockBehavior)" />
	protected override TResult GetReturnValue<TResult>(MethodInvocation invocation, MockBehavior behavior)
		where TResult : default
	{
		if (_returnCallbacks.Count == 0)
		{
			return behavior.DefaultValue.Generate<TResult>();
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

		int index = Interlocked.Increment(ref _currentReturnCallbackIndex);
		Func<T1, T2, T3, TReturn> returnCallback = _returnCallbacks[index % _returnCallbacks.Count];

		TReturn returnValue = returnCallback(p1, p2, p3);
		if (returnValue is null)
		{
			return default!;
		}

		if (returnValue is TResult result)
		{
			return result;
		}

		throw new MockException(
			$"The return callback only supports '{FormatType(typeof(TReturn))}' and not '{FormatType(typeof(TResult))}'.");
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

	/// <inheritdoc cref="MethodSetup.SetOutParameter{T}(string, MockBehavior)" />
	protected override T SetOutParameter<T>(string parameterName, MockBehavior behavior)
	{
		if (_match1 is not null && _match2 is not null && _match3 is not null &&
		    HasOutParameter([_match1, _match2, _match3,], parameterName, out Match.IOutParameter<T>? outParameter))
		{
			return outParameter.GetValue(behavior);
		}

		return behavior.DefaultValue.Generate<T>();
	}

	/// <inheritdoc cref="MethodSetup.SetRefParameter{T}(string, T, MockBehavior)" />
	protected override T SetRefParameter<T>(string parameterName, T value, MockBehavior behavior)
	{
		if (_match1 is not null && _match2 is not null && _match3 is not null &&
		    HasRefParameter([_match1, _match2, _match3,], parameterName, out Match.IRefParameter<T>? refParameter))
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
public class ReturnMethodSetup<TReturn, T1, T2, T3, T4> : MethodSetup, IReturnMethodSetupCallbackBuilder<TReturn, T1, T2, T3, T4>
{
	private readonly List<Callback<Action<int, T1, T2, T3, T4>>> _callbacks = [];
	private readonly Match.NamedParameter? _match1;
	private readonly Match.NamedParameter? _match2;
	private readonly Match.NamedParameter? _match3;
	private readonly Match.NamedParameter? _match4;
	private readonly Match.IParameters? _matches;
	private readonly string _name;
	private readonly List<Func<T1, T2, T3, T4, TReturn>> _returnCallbacks = [];
	private bool? _callBaseClass;
	private Callback? _currentCallback;
	private int _currentReturnCallbackIndex = -1;

	/// <inheritdoc cref="ReturnMethodSetup{TReturn, T1, T2, T3, T4}" />
	public ReturnMethodSetup(
		string name,
		Match.NamedParameter match1,
		Match.NamedParameter match2,
		Match.NamedParameter match3,
		Match.NamedParameter match4)
	{
		_name = name;
		_match1 = match1;
		_match2 = match2;
		_match3 = match3;
		_match4 = match4;
	}

	/// <inheritdoc cref="ReturnMethodSetup{TReturn, T1, T2, T3, T4}" />
	public ReturnMethodSetup(string name, Match.IParameters matches)
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
	public IReturnMethodSetup<TReturn, T1, T2, T3, T4> Returns(Func<T1, T2, T3, T4, TReturn> callback)
	{
		_returnCallbacks.Add(callback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public IReturnMethodSetup<TReturn, T1, T2, T3, T4> Returns(Func<TReturn> callback)
	{
		_returnCallbacks.Add((_, _, _, _) => callback());
		return this;
	}

	/// <summary>
	///     Registers the <paramref name="returnValue" /> for this method.
	/// </summary>
	public IReturnMethodSetup<TReturn, T1, T2, T3, T4> Returns(TReturn returnValue)
	{
		_returnCallbacks.Add((_, _, _, _) => returnValue);
		return this;
	}

	/// <summary>
	///     Registers an <typeparamref name="TException" /> to throw when the method is invoked.
	/// </summary>
	public IReturnMethodSetup<TReturn, T1, T2, T3, T4> Throws<TException>()
		where TException : Exception, new()
	{
		_returnCallbacks.Add((_, _, _, _) => throw new TException());
		return this;
	}

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the method is invoked.
	/// </summary>
	public IReturnMethodSetup<TReturn, T1, T2, T3, T4> Throws(Exception exception)
	{
		_returnCallbacks.Add((_, _, _, _) => throw exception);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	public IReturnMethodSetup<TReturn, T1, T2, T3, T4> Throws(Func<Exception> callback)
	{
		_returnCallbacks.Add((_, _, _, _) => throw callback());
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	public IReturnMethodSetup<TReturn, T1, T2, T3, T4> Throws(Func<T1, T2, T3, T4, Exception> callback)
	{
		_returnCallbacks.Add((v1, v2, v3, v4) => throw callback(v1, v2, v3, v4));
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupCallbackBuilder{TReturn, T1, T2, T3, T4}.When(Func{int, bool})" />
	public IReturnMethodSetupCallbackWhenBuilder<TReturn, T1, T2, T3, T4> When(Func<int, bool> predicate)
	{
		_currentCallback?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IReturnMethodSetupCallbackWhenBuilder{TReturn, T1, T2, T3, T4}.For(int)" />
	public IReturnMethodSetup<TReturn, T1, T2, T3, T4> For(int times)
	{
		_currentCallback?.For(x => x < times);
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

	/// <inheritdoc cref="MethodSetup.GetReturnValue{TResult}(MethodInvocation, MockBehavior)" />
	protected override TResult GetReturnValue<TResult>(MethodInvocation invocation, MockBehavior behavior)
		where TResult : default
	{
		if (_returnCallbacks.Count == 0)
		{
			return behavior.DefaultValue.Generate<TResult>();
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

		int index = Interlocked.Increment(ref _currentReturnCallbackIndex);
		Func<T1, T2, T3, T4, TReturn> returnCallback = _returnCallbacks[index % _returnCallbacks.Count];

		TReturn returnValue = returnCallback(p1, p2, p3, p4);
		if (returnValue is null)
		{
			return default!;
		}

		if (returnValue is TResult result)
		{
			return result;
		}

		throw new MockException(
			$"The return callback only supports '{FormatType(typeof(TReturn))}' and not '{FormatType(typeof(TResult))}'.");
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

	/// <inheritdoc cref="MethodSetup.SetOutParameter{T}(string, MockBehavior)" />
	protected override T SetOutParameter<T>(string parameterName, MockBehavior behavior)
	{
		if (_match1 is not null && _match2 is not null && _match3 is not null && _match4 is not null &&
		    HasOutParameter([_match1, _match2, _match3, _match4,], parameterName,
			    out Match.IOutParameter<T>? outParameter))
		{
			return outParameter.GetValue(behavior);
		}

		return behavior.DefaultValue.Generate<T>();
	}

	/// <inheritdoc cref="MethodSetup.SetRefParameter{T}(string, T, MockBehavior)" />
	protected override T SetRefParameter<T>(string parameterName, T value, MockBehavior behavior)
	{
		if (_match1 is not null && _match2 is not null && _match3 is not null && _match4 is not null &&
		    HasRefParameter([_match1, _match2, _match3, _match4,], parameterName,
			    out Match.IRefParameter<T>? refParameter))
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
