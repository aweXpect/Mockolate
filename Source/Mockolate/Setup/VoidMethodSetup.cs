using System;
using System.Collections.Generic;
using System.Threading;
using Mockolate.Exceptions;
using Mockolate.Interactions;

namespace Mockolate.Setup;

/// <summary>
///     Sets up a method returning <see langword="void" />.
/// </summary>
public class VoidMethodSetup(string name) : MethodSetup
{
	private readonly List<Action> _callbacks = [];
	private readonly List<Action> _returnCallbacks = [];
	private int _currentReturnCallbackIndex = -1;

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	public VoidMethodSetup Callback(Action callback)
	{
		_callbacks.Add(callback);
		return this;
	}

	/// <summary>
	///     Registers an iteration in the sequence of method invocations, that does not throw.
	/// </summary>
	public VoidMethodSetup DoesNotThrow()
	{
		_returnCallbacks.Add(() => { });
		return this;
	}

	/// <summary>
	///     Registers an <typeparamref name="TException" /> to throw when the method is invoked.
	/// </summary>
	public VoidMethodSetup Throws<TException>()
		where TException : Exception, new()
	{
		_returnCallbacks.Add(() => throw new TException());
		return this;
	}

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the method is invoked.
	/// </summary>
	public VoidMethodSetup Throws(Exception exception)
	{
		_returnCallbacks.Add(() => throw exception);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	public VoidMethodSetup Throws(Func<Exception> callback)
	{
		_returnCallbacks.Add(() => throw callback());
		return this;
	}

	/// <inheritdoc cref="MethodSetup.ExecuteCallback(MethodInvocation, MockBehavior)" />
	protected override void ExecuteCallback(MethodInvocation invocation, MockBehavior behavior)
	{
		_callbacks.ForEach(callback => callback.Invoke());
		if (_returnCallbacks.Count > 0)
		{
			int index = Interlocked.Increment(ref _currentReturnCallbackIndex);
			Action? returnCallback = _returnCallbacks[index % _returnCallbacks.Count];
			returnCallback();
		}
	}

	/// <inheritdoc cref="MethodSetup.GetReturnValue{TResult}(MethodInvocation, MockBehavior)" />
	protected override TResult GetReturnValue<TResult>(MethodInvocation invocation, MockBehavior behavior)
		where TResult : default
		=> throw new MockException("The method setup does not support return values.");

	/// <inheritdoc cref="MethodSetup.IsMatch(MethodInvocation)" />
	protected override bool IsMatch(MethodInvocation invocation)
		=> invocation.Name.Equals(name) && invocation.Parameters.Length == 0;

	/// <inheritdoc cref="MethodSetup.SetOutParameter{T}(string, MockBehavior)" />
	protected override T SetOutParameter<T>(string parameterName, MockBehavior behavior)
		=> behavior.DefaultValue.Generate<T>();

	/// <inheritdoc cref="MethodSetup.SetRefParameter{T}(string, T, MockBehavior)" />
	protected override T SetRefParameter<T>(string parameterName, T value, MockBehavior behavior)
		=> value;

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString() => $"void {name}()";
}

/// <summary>
///     Setup for a method with one parameter <typeparamref name="T1" /> returning <see langword="void" />.
/// </summary>
public class VoidMethodSetup<T1> : MethodSetup
{
	private readonly List<Action<T1>> _callbacks = [];
	private readonly Match.NamedParameter? _match1;
	private readonly Match.IParameters? _matches;
	private readonly string _name;
	private readonly List<Action<T1>> _returnCallbacks = [];
	private int _currentReturnCallbackIndex = -1;

	/// <inheritdoc cref="VoidMethodSetup{T1}" />
	public VoidMethodSetup(string name, Match.NamedParameter match1)
	{
		_name = name;
		_match1 = match1;
	}

	/// <inheritdoc cref="VoidMethodSetup{T1}" />
	public VoidMethodSetup(string name, Match.IParameters matches)
	{
		_name = name;
		_matches = matches;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	public VoidMethodSetup<T1> Callback(Action callback)
	{
		_callbacks.Add(_ => callback());
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	public VoidMethodSetup<T1> Callback(Action<T1> callback)
	{
		_callbacks.Add(callback);
		return this;
	}

	/// <summary>
	///     Registers an iteration in the sequence of method invocations, that does not throw.
	/// </summary>
	public VoidMethodSetup<T1> DoesNotThrow()
	{
		_returnCallbacks.Add(_ => { });
		return this;
	}

	/// <summary>
	///     Registers an <typeparamref name="TException" /> to throw when the method is invoked.
	/// </summary>
	public VoidMethodSetup<T1> Throws<TException>()
		where TException : Exception, new()
	{
		_returnCallbacks.Add(_ => throw new TException());
		return this;
	}

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the method is invoked.
	/// </summary>
	public VoidMethodSetup<T1> Throws(Exception exception)
	{
		_returnCallbacks.Add(_ => throw exception);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	public VoidMethodSetup<T1> Throws(Func<Exception> callback)
	{
		_returnCallbacks.Add(_ => throw callback());
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	public VoidMethodSetup<T1> Throws(Func<T1, Exception> callback)
	{
		_returnCallbacks.Add(v1 => throw callback(v1));
		return this;
	}

	/// <inheritdoc cref="MethodSetup.ExecuteCallback(MethodInvocation, MockBehavior)" />
	protected override void ExecuteCallback(MethodInvocation invocation, MockBehavior behavior)
	{
		if (TryCast(invocation.Parameters[0], out T1 p1, behavior))
		{
			_callbacks.ForEach(callback => callback.Invoke(p1));
			if (_returnCallbacks.Count > 0)
			{
				int index = Interlocked.Increment(ref _currentReturnCallbackIndex);
				Action<T1>? returnCallback = _returnCallbacks[index % _returnCallbacks.Count];
				returnCallback(p1);
			}
		}
	}

	/// <inheritdoc cref="MethodSetup.GetReturnValue{TResult}(MethodInvocation, MockBehavior)" />
	protected override TResult GetReturnValue<TResult>(MethodInvocation invocation, MockBehavior behavior)
		where TResult : default
		=> throw new MockException("The method setup does not support return values.");

	/// <inheritdoc cref="MethodSetup.IsMatch(MethodInvocation)" />
	protected override bool IsMatch(MethodInvocation invocation)
		=> invocation.Name.Equals(_name) &&
		   (_matches is not null
			   ? _matches.Matches(invocation.Parameters)
			   : Matches([_match1!,], invocation.Parameters));

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
			? $"void {_name}({_matches})"
			: $"void {_name}({_match1})";
}

/// <summary>
///     Setup for a method with two parameters <typeparamref name="T1" /> and <typeparamref name="T2" /> returning
///     <see langword="void" />.
/// </summary>
public class VoidMethodSetup<T1, T2> : MethodSetup
{
	private readonly List<Action<T1, T2>> _callbacks = [];
	private readonly Match.NamedParameter? _match1;
	private readonly Match.NamedParameter? _match2;
	private readonly Match.IParameters? _matches;
	private readonly string _name;
	private readonly List<Action<T1, T2>> _returnCallbacks = [];
	private int _currentReturnCallbackIndex = -1;

	/// <inheritdoc cref="VoidMethodSetup{T1, T2}" />
	public VoidMethodSetup(string name, Match.NamedParameter match1, Match.NamedParameter match2)
	{
		_name = name;
		_match1 = match1;
		_match2 = match2;
	}

	/// <inheritdoc cref="VoidMethodSetup{T1, T2}" />
	public VoidMethodSetup(string name, Match.IParameters matches)
	{
		_name = name;
		_matches = matches;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	public VoidMethodSetup<T1, T2> Callback(Action callback)
	{
		_callbacks.Add((_, _) => callback());
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	public VoidMethodSetup<T1, T2> Callback(Action<T1, T2> callback)
	{
		_callbacks.Add(callback);
		return this;
	}

	/// <summary>
	///     Registers an iteration in the sequence of method invocations, that does not throw.
	/// </summary>
	public VoidMethodSetup<T1, T2> DoesNotThrow()
	{
		_returnCallbacks.Add((_, _) => { });
		return this;
	}

	/// <summary>
	///     Registers an <typeparamref name="TException" /> to throw when the method is invoked.
	/// </summary>
	public VoidMethodSetup<T1, T2> Throws<TException>()
		where TException : Exception, new()
	{
		_returnCallbacks.Add((_, _) => throw new TException());
		return this;
	}

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the method is invoked.
	/// </summary>
	public VoidMethodSetup<T1, T2> Throws(Exception exception)
	{
		_returnCallbacks.Add((_, _) => throw exception);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	public VoidMethodSetup<T1, T2> Throws(Func<Exception> callback)
	{
		_returnCallbacks.Add((_, _) => throw callback());
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	public VoidMethodSetup<T1, T2> Throws(Func<T1, T2, Exception> callback)
	{
		_returnCallbacks.Add((v1, v2) => throw callback(v1, v2));
		return this;
	}

	/// <inheritdoc cref="MethodSetup.ExecuteCallback(MethodInvocation, MockBehavior)" />
	protected override void ExecuteCallback(MethodInvocation invocation, MockBehavior behavior)
	{
		if (TryCast(invocation.Parameters[0], out T1 p1, behavior) &&
		    TryCast(invocation.Parameters[1], out T2 p2, behavior))
		{
			_callbacks.ForEach(callback => callback.Invoke(p1, p2));
			if (_returnCallbacks.Count > 0)
			{
				int index = Interlocked.Increment(ref _currentReturnCallbackIndex);
				Action<T1, T2>? returnCallback = _returnCallbacks[index % _returnCallbacks.Count];
				returnCallback(p1, p2);
			}
		}
	}

	/// <inheritdoc cref="MethodSetup.GetReturnValue{TResult}(MethodInvocation, MockBehavior)" />
	protected override TResult GetReturnValue<TResult>(MethodInvocation invocation, MockBehavior behavior)
		where TResult : default
		=> throw new MockException("The method setup does not support return values.");

	/// <inheritdoc cref="MethodSetup.IsMatch(MethodInvocation)" />
	protected override bool IsMatch(MethodInvocation invocation)
		=> invocation.Name.Equals(_name) &&
		   (_matches is not null
			   ? _matches.Matches(invocation.Parameters)
			   : Matches([_match1!, _match2!,], invocation.Parameters));

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
			? $"void {_name}({_matches})"
			: $"void {_name}({_match1}, {_match2})";
}

/// <summary>
///     Setup for a method with three parameters <typeparamref name="T1" />, <typeparamref name="T2" /> and
///     <typeparamref name="T3" /> returning <see langword="void" />.
/// </summary>
public class VoidMethodSetup<T1, T2, T3> : MethodSetup
{
	private readonly List<Action<T1, T2, T3>> _callbacks = [];
	private readonly Match.NamedParameter? _match1;
	private readonly Match.NamedParameter? _match2;
	private readonly Match.NamedParameter? _match3;
	private readonly Match.IParameters? _matches;
	private readonly string _name;
	private readonly List<Action<T1, T2, T3>> _returnCallbacks = [];
	private int _currentReturnCallbackIndex = -1;

	/// <inheritdoc cref="VoidMethodSetup{T1, T2, T3}" />
	public VoidMethodSetup(
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

	/// <inheritdoc cref="VoidMethodSetup{T1, T2, T3}" />
	public VoidMethodSetup(string name, Match.IParameters matches)
	{
		_name = name;
		_matches = matches;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	public VoidMethodSetup<T1, T2, T3> Callback(Action callback)
	{
		_callbacks.Add((_, _, _) => callback());
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	public VoidMethodSetup<T1, T2, T3> Callback(Action<T1, T2, T3> callback)
	{
		_callbacks.Add(callback);
		return this;
	}

	/// <summary>
	///     Registers an iteration in the sequence of method invocations, that does not throw.
	/// </summary>
	public VoidMethodSetup<T1, T2, T3> DoesNotThrow()
	{
		_returnCallbacks.Add((_, _, _) => { });
		return this;
	}

	/// <summary>
	///     Registers an <typeparamref name="TException" /> to throw when the method is invoked.
	/// </summary>
	public VoidMethodSetup<T1, T2, T3> Throws<TException>()
		where TException : Exception, new()
	{
		_returnCallbacks.Add((_, _, _) => throw new TException());
		return this;
	}

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the method is invoked.
	/// </summary>
	public VoidMethodSetup<T1, T2, T3> Throws(Exception exception)
	{
		_returnCallbacks.Add((_, _, _) => throw exception);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	public VoidMethodSetup<T1, T2, T3> Throws(Func<Exception> callback)
	{
		_returnCallbacks.Add((_, _, _) => throw callback());
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	public VoidMethodSetup<T1, T2, T3> Throws(Func<T1, T2, T3, Exception> callback)
	{
		_returnCallbacks.Add((v1, v2, v3) => throw callback(v1, v2, v3));
		return this;
	}

	/// <inheritdoc cref="MethodSetup.ExecuteCallback(MethodInvocation, MockBehavior)" />
	protected override void ExecuteCallback(MethodInvocation invocation, MockBehavior behavior)
	{
		if (TryCast(invocation.Parameters[0], out T1 p1, behavior) &&
		    TryCast(invocation.Parameters[1], out T2 p2, behavior) &&
		    TryCast(invocation.Parameters[2], out T3 p3, behavior))
		{
			_callbacks.ForEach(callback => callback.Invoke(p1, p2, p3));
			if (_returnCallbacks.Count > 0)
			{
				int index = Interlocked.Increment(ref _currentReturnCallbackIndex);
				Action<T1, T2, T3>? returnCallback = _returnCallbacks[index % _returnCallbacks.Count];
				returnCallback(p1, p2, p3);
			}
		}
	}

	/// <inheritdoc cref="MethodSetup.GetReturnValue{TResult}(MethodInvocation, MockBehavior)" />
	protected override TResult GetReturnValue<TResult>(MethodInvocation invocation, MockBehavior behavior)
		where TResult : default
		=> throw new MockException("The method setup does not support return values.");

	/// <inheritdoc cref="MethodSetup.IsMatch(MethodInvocation)" />
	protected override bool IsMatch(MethodInvocation invocation)
		=> invocation.Name.Equals(_name) &&
		   (_matches is not null
			   ? _matches.Matches(invocation.Parameters)
			   : Matches([_match1!, _match2!, _match3!,], invocation.Parameters));

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
			? $"void {_name}({_matches})"
			: $"void {_name}({_match1}, {_match2}, {_match3})";
}

/// <summary>
///     Setup for a method with four parameters <typeparamref name="T1" />, <typeparamref name="T2" />,
///     <typeparamref name="T3" /> and <typeparamref name="T4" /> returning <see langword="void" />.
/// </summary>
public class VoidMethodSetup<T1, T2, T3, T4> : MethodSetup
{
	private readonly List<Action<T1, T2, T3, T4>> _callbacks = [];
	private readonly Match.NamedParameter? _match1;
	private readonly Match.NamedParameter? _match2;
	private readonly Match.NamedParameter? _match3;
	private readonly Match.NamedParameter? _match4;
	private readonly Match.IParameters? _matches;
	private readonly string _name;
	private readonly List<Action<T1, T2, T3, T4>> _returnCallbacks = [];
	private int _currentReturnCallbackIndex = -1;

	/// <inheritdoc cref="VoidMethodSetup{T1, T2, T3, T4}" />
	public VoidMethodSetup(
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

	/// <inheritdoc cref="VoidMethodSetup{T1, T2, T3, T4}" />
	public VoidMethodSetup(string name, Match.IParameters matches)
	{
		_name = name;
		_matches = matches;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	public VoidMethodSetup<T1, T2, T3, T4> Callback(Action callback)
	{
		_callbacks.Add((_, _, _, _) => callback());
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	public VoidMethodSetup<T1, T2, T3, T4> Callback(Action<T1, T2, T3, T4> callback)
	{
		_callbacks.Add(callback);
		return this;
	}

	/// <summary>
	///     Registers an iteration in the sequence of method invocations, that does not throw.
	/// </summary>
	public VoidMethodSetup<T1, T2, T3, T4> DoesNotThrow()
	{
		_returnCallbacks.Add((_, _, _, _) => { });
		return this;
	}

	/// <summary>
	///     Registers an <typeparamref name="TException" /> to throw when the method is invoked.
	/// </summary>
	public VoidMethodSetup<T1, T2, T3, T4> Throws<TException>()
		where TException : Exception, new()
	{
		_returnCallbacks.Add((_, _, _, _) => throw new TException());
		return this;
	}

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the method is invoked.
	/// </summary>
	public VoidMethodSetup<T1, T2, T3, T4> Throws(Exception exception)
	{
		_returnCallbacks.Add((_, _, _, _) => throw exception);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	public VoidMethodSetup<T1, T2, T3, T4> Throws(Func<Exception> callback)
	{
		_returnCallbacks.Add((_, _, _, _) => throw callback());
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	public VoidMethodSetup<T1, T2, T3, T4> Throws(Func<T1, T2, T3, T4, Exception> callback)
	{
		_returnCallbacks.Add((v1, v2, v3, v4) => throw callback(v1, v2, v3, v4));
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
			_callbacks.ForEach(callback => callback.Invoke(p1, p2, p3, p4));
			if (_returnCallbacks.Count > 0)
			{
				int index = Interlocked.Increment(ref _currentReturnCallbackIndex);
				Action<T1, T2, T3, T4>? returnCallback = _returnCallbacks[index % _returnCallbacks.Count];
				returnCallback(p1, p2, p3, p4);
			}
		}
	}

	/// <inheritdoc cref="MethodSetup.GetReturnValue{TResult}(MethodInvocation, MockBehavior)" />
	protected override TResult GetReturnValue<TResult>(MethodInvocation invocation, MockBehavior behavior)
		where TResult : default
		=> throw new MockException("The method setup does not support return values.");

	/// <inheritdoc cref="MethodSetup.IsMatch(MethodInvocation)" />
	protected override bool IsMatch(MethodInvocation invocation)
		=> invocation.Name.Equals(_name) &&
		   (_matches is not null
			   ? _matches.Matches(invocation.Parameters)
			   : Matches([_match1!, _match2!, _match3!, _match4!,], invocation.Parameters));

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
			? $"void {_name}({_matches})"
			: $"void {_name}({_match1}, {_match2}, {_match3}, {_match4})";
}
