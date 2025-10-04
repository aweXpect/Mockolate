using System;
using System.Collections.Generic;
using System.Threading;
using Mockolate.Checks;
using Mockolate.Exceptions;

namespace Mockolate.Setup;

/// <summary>
///     Setup for a method returning <see langword="void" />.
/// </summary>
public class VoidMethodSetup(string name) : MethodSetup
{
	private Action? _callback;
	private List<Action> _returnCallbacks = [];
	int _currentReturnCallbackIndex = -1;

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	public VoidMethodSetup Callback(Action callback)
	{
		_callback = callback;
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
	///     Registers an <paramref name="exception"/> to throw when the method is invoked.
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
		_callback?.Invoke();
		if (_returnCallbacks.Count > 0)
		{
			var index = Interlocked.Increment(ref _currentReturnCallbackIndex);
			var returnCallback = _returnCallbacks[index % _returnCallbacks.Count];
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
		=> behavior.DefaultValueGenerator.Generate<T>();

	/// <inheritdoc cref="MethodSetup.SetRefParameter{T}(string, T, MockBehavior)" />
	protected override T SetRefParameter<T>(string parameterName, T value, MockBehavior behavior)
		=> value;
}

/// <summary>
///     Setup for a method with one parameter <typeparamref name="T1" /> returning <see langword="void" />.
/// </summary>
public class VoidMethodSetup<T1>(string name, With.NamedParameter match1) : MethodSetup
{
	private Action<T1>? _callback;
	private List<Action<T1>> _returnCallbacks = [];
	int _currentReturnCallbackIndex = -1;

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	public VoidMethodSetup<T1> Callback(Action callback)
	{
		_callback = _ => callback();
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	public VoidMethodSetup<T1> Callback(Action<T1> callback)
	{
		_callback = callback;
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
	///     Registers an <paramref name="exception"/> to throw when the method is invoked.
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
		if (TryCast<T1>(invocation.Parameters[0], out var p1, behavior))
		{
			_callback?.Invoke(p1);
			if (_returnCallbacks.Count > 0)
			{
				var index = Interlocked.Increment(ref _currentReturnCallbackIndex);
				var returnCallback = _returnCallbacks[index % _returnCallbacks.Count];
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
		=> invocation.Name.Equals(name) && Matches([match1], invocation.Parameters);

	/// <inheritdoc cref="MethodSetup.SetOutParameter{T}(string, MockBehavior)" />
	protected override T SetOutParameter<T>(string parameterName, MockBehavior behavior)
	{
		if (HasOutParameter([match1], parameterName, out With.OutParameter<T>? outParameter))
		{
			return outParameter.GetValue();
		}

		return behavior.DefaultValueGenerator.Generate<T>();
	}

	/// <inheritdoc cref="MethodSetup.SetRefParameter{T}(string, T, MockBehavior)" />
	protected override T SetRefParameter<T>(string parameterName, T value, MockBehavior behavior)
	{
		if (HasRefParameter([match1], parameterName, out With.RefParameter<T>? refParameter))
		{
			return refParameter.GetValue(value);
		}

		return value;
	}
}

/// <summary>
///     Setup for a method with two parameters <typeparamref name="T1" /> and <typeparamref name="T2" /> returning <see langword="void" />.
/// </summary>
public class VoidMethodSetup<T1, T2>(string name, With.NamedParameter match1, With.NamedParameter match2) : MethodSetup
{
	private Action<T1, T2>? _callback;
	private List<Action<T1, T2>> _returnCallbacks = [];
	int _currentReturnCallbackIndex = -1;

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	public VoidMethodSetup<T1, T2> Callback(Action callback)
	{
		_callback = (_,_) => callback();
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	public VoidMethodSetup<T1, T2> Callback(Action<T1, T2> callback)
	{
		_callback = callback;
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
	///     Registers an <paramref name="exception"/> to throw when the method is invoked.
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
		if (TryCast<T1>(invocation.Parameters[0], out var p1, behavior) &&
			TryCast<T2>(invocation.Parameters[1], out var p2, behavior))
		{
			_callback?.Invoke(p1, p2);
			if (_returnCallbacks.Count > 0)
			{
				var index = Interlocked.Increment(ref _currentReturnCallbackIndex);
				var returnCallback = _returnCallbacks[index % _returnCallbacks.Count];
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
		=> invocation.Name.Equals(name) && Matches([match1, match2], invocation.Parameters);

	/// <inheritdoc cref="MethodSetup.SetOutParameter{T}(string, MockBehavior)" />
	protected override T SetOutParameter<T>(string parameterName, MockBehavior behavior)
	{
		if (HasOutParameter([match1, match2], parameterName, out With.OutParameter<T>? outParameter))
		{
			return outParameter.GetValue();
		}

		return behavior.DefaultValueGenerator.Generate<T>();
	}

	/// <inheritdoc cref="MethodSetup.SetRefParameter{T}(string, T, MockBehavior)" />
	protected override T SetRefParameter<T>(string parameterName, T value, MockBehavior behavior)
	{
		if (HasRefParameter([match1, match2], parameterName, out With.RefParameter<T>? refParameter))
		{
			return refParameter.GetValue(value);
		}

		return value;
	}
}

/// <summary>
///     Setup for a method with three parameters <typeparamref name="T1" />, <typeparamref name="T2" /> and <typeparamref name="T3" /> returning <see langword="void" />.
/// </summary>
public class VoidMethodSetup<T1, T2, T3>(string name, With.NamedParameter match1, With.NamedParameter match2, With.NamedParameter match3) : MethodSetup
{
	private Action<T1, T2, T3>? _callback;
	private List<Action<T1, T2, T3>> _returnCallbacks = [];
	int _currentReturnCallbackIndex = -1;

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	public VoidMethodSetup<T1, T2, T3> Callback(Action callback)
	{
		_callback = (_, _, _) => callback();
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	public VoidMethodSetup<T1, T2, T3> Callback(Action<T1, T2, T3> callback)
	{
		_callback = callback;
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
	///     Registers an <paramref name="exception"/> to throw when the method is invoked.
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
		if (TryCast<T1>(invocation.Parameters[0], out var p1, behavior) &&
			TryCast<T2>(invocation.Parameters[1], out var p2, behavior) &&
			TryCast<T3>(invocation.Parameters[2], out var p3, behavior))
		{
			_callback?.Invoke(p1, p2, p3);
			if (_returnCallbacks.Count > 0)
			{
				var index = Interlocked.Increment(ref _currentReturnCallbackIndex);
				var returnCallback = _returnCallbacks[index % _returnCallbacks.Count];
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
		=> invocation.Name.Equals(name) && Matches([match1, match2, match3], invocation.Parameters);

	/// <inheritdoc cref="MethodSetup.SetOutParameter{T}(string, MockBehavior)" />
	protected override T SetOutParameter<T>(string parameterName, MockBehavior behavior)
	{
		if (HasOutParameter([match1, match2, match3], parameterName, out With.OutParameter<T>? outParameter))
		{
			return outParameter.GetValue();
		}

		return behavior.DefaultValueGenerator.Generate<T>();
	}

	/// <inheritdoc cref="MethodSetup.SetRefParameter{T}(string, T, MockBehavior)" />
	protected override T SetRefParameter<T>(string parameterName, T value, MockBehavior behavior)
	{
		if (HasRefParameter([match1, match2, match3], parameterName, out With.RefParameter<T>? refParameter))
		{
			return refParameter.GetValue(value);
		}

		return value;
	}
}

/// <summary>
///     Setup for a method with four parameters <typeparamref name="T1" />, <typeparamref name="T2" />, <typeparamref name="T3" /> and <typeparamref name="T4" /> returning <see langword="void" />.
/// </summary>
public class VoidMethodSetup<T1, T2, T3, T4>(string name, With.NamedParameter match1, With.NamedParameter match2, With.NamedParameter match3, With.NamedParameter match4) : MethodSetup
{
	private Action<T1, T2, T3, T4>? _callback;
	private List<Action<T1, T2, T3, T4>> _returnCallbacks = [];
	int _currentReturnCallbackIndex = -1;

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	public VoidMethodSetup<T1, T2, T3, T4> Callback(Action callback)
	{
		_callback = (_, _, _, _) => callback();
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	public VoidMethodSetup<T1, T2, T3, T4> Callback(Action<T1, T2, T3, T4> callback)
	{
		_callback = callback;
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
	///     Registers an <paramref name="exception"/> to throw when the method is invoked.
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
		if (TryCast<T1>(invocation.Parameters[0], out var p1, behavior) &&
			TryCast<T2>(invocation.Parameters[1], out var p2, behavior) &&
			TryCast<T3>(invocation.Parameters[2], out var p3, behavior) &&
			TryCast<T4>(invocation.Parameters[3], out var p4, behavior))
		{
			_callback?.Invoke(p1, p2, p3, p4);
			if (_returnCallbacks.Count > 0)
			{
				var index = Interlocked.Increment(ref _currentReturnCallbackIndex);
				var returnCallback = _returnCallbacks[index % _returnCallbacks.Count];
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
		=> invocation.Name.Equals(name) && Matches([match1, match2, match3, match4], invocation.Parameters);

	/// <inheritdoc cref="MethodSetup.SetOutParameter{T}(string, MockBehavior)" />
	protected override T SetOutParameter<T>(string parameterName, MockBehavior behavior)
	{
		if (HasOutParameter([match1, match2, match3, match4], parameterName, out With.OutParameter<T>? outParameter))
		{
			return outParameter.GetValue();
		}

		return behavior.DefaultValueGenerator.Generate<T>();
	}

	/// <inheritdoc cref="MethodSetup.SetRefParameter{T}(string, T, MockBehavior)" />
	protected override T SetRefParameter<T>(string parameterName, T value, MockBehavior behavior)
	{
		if (HasRefParameter([match1, match2, match3, match4], parameterName, out With.RefParameter<T>? refParameter))
		{
			return refParameter.GetValue(value);
		}

		return value;
	}
}
