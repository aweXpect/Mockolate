using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using Mockolate.Exceptions;
using Mockolate.Interactions;

namespace Mockolate.Setup;

/// <summary>
///     Sets up a method returning <typeparamref name="TReturn" />.
/// </summary>
public class ReturnMethodSetup<TReturn>(string name) : MethodSetup
{
	private readonly List<Action> _callbacks = [];
	private readonly List<Func<TReturn>> _returnCallbacks = [];
	private int _currentReturnCallbackIndex = -1;

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	public ReturnMethodSetup<TReturn> Callback(Action callback)
	{
		_callbacks.Add(callback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public ReturnMethodSetup<TReturn> Returns(Func<TReturn> callback)
	{
		_returnCallbacks.Add(callback);
		return this;
	}

	/// <summary>
	///     Registers the <paramref name="returnValue" /> for this method.
	/// </summary>
	public ReturnMethodSetup<TReturn> Returns(TReturn returnValue)
	{
		_returnCallbacks.Add(() => returnValue);
		return this;
	}

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the method is invoked.
	/// </summary>
	public ReturnMethodSetup<TReturn> Throws(Exception exception)
	{
		_returnCallbacks.Add(() => throw exception);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	public ReturnMethodSetup<TReturn> Throws(Func<Exception> callback)
	{
		_returnCallbacks.Add(() => throw callback());
		return this;
	}

	/// <inheritdoc cref="MethodSetup.ExecuteCallback(MethodInvocation, MockBehavior)" />
	protected override void ExecuteCallback(MethodInvocation invocation, MockBehavior behavior)
		=> _callbacks.ForEach(callback => callback.Invoke());

	/// <inheritdoc cref="MethodSetup.GetReturnValue{TResult}(MethodInvocation, MockBehavior)" />
	protected override TResult GetReturnValue<TResult>(MethodInvocation invocation, MockBehavior behavior)
		where TResult : default
	{
		if (_returnCallbacks.Count > 0)
		{
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
		}

		throw new MockException(
			$"The return callback only supports '{FormatType(typeof(TReturn))}' and not '{FormatType(typeof(TResult))}'.");
	}

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
	public override string ToString() => $"{FormatType(typeof(TReturn))} {name}()";
}

/// <summary>
///     Setup for a method with one parameter <typeparamref name="T1" /> returning <typeparamref name="TReturn" />.
/// </summary>
public class ReturnMethodSetup<TReturn, T1> : MethodSetup
{
	private readonly List<Action<T1>> _callbacks = [];
	private readonly List<Func<T1, TReturn>> _returnCallbacks = [];
	private readonly string _name;
	private readonly Match.IParameters? _matches;
	private readonly Match.NamedParameter? _match1;
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

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	public ReturnMethodSetup<TReturn, T1> Callback(Action callback)
	{
		_callbacks.Add(_ => callback());
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	public ReturnMethodSetup<TReturn, T1> Callback(Action<T1> callback)
	{
		_callbacks.Add(callback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public ReturnMethodSetup<TReturn, T1> Returns(Func<T1, TReturn> callback)
	{
		_returnCallbacks.Add(callback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public ReturnMethodSetup<TReturn, T1> Returns(Func<TReturn> callback)
	{
		_returnCallbacks.Add(_ => callback());
		return this;
	}

	/// <summary>
	///     Registers the <paramref name="returnValue" /> for this method.
	/// </summary>
	public ReturnMethodSetup<TReturn, T1> Returns(TReturn returnValue)
	{
		_returnCallbacks.Add(_ => returnValue);
		return this;
	}

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the method is invoked.
	/// </summary>
	public ReturnMethodSetup<TReturn, T1> Throws(Exception exception)
	{
		_returnCallbacks.Add(_ => throw exception);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	public ReturnMethodSetup<TReturn, T1> Throws(Func<Exception> callback)
	{
		_returnCallbacks.Add(_ => throw callback());
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	public ReturnMethodSetup<TReturn, T1> Throws(Func<T1, Exception> callback)
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
			? $"{FormatType(typeof(TReturn))} {_name}({_matches})"
			: $"{FormatType(typeof(TReturn))} {_name}({_match1})";
}

/// <summary>
///     Setup for a method with two parameters <typeparamref name="T1" /> and <typeparamref name="T2" /> returning
///     <typeparamref name="TReturn" />.
/// </summary>
public class ReturnMethodSetup<TReturn, T1, T2> : MethodSetup
{
	private readonly List<Action<T1, T2>> _callbacks = [];
	private readonly List<Func<T1, T2, TReturn>> _returnCallbacks = [];
	private readonly string _name;
	private readonly Match.IParameters? _matches;
	private readonly Match.NamedParameter? _match1;
	private readonly Match.NamedParameter? _match2;
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

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	public ReturnMethodSetup<TReturn, T1, T2> Callback(Action callback)
	{
		_callbacks.Add((_, _) => callback());
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	public ReturnMethodSetup<TReturn, T1, T2> Callback(Action<T1, T2> callback)
	{
		_callbacks.Add(callback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public ReturnMethodSetup<TReturn, T1, T2> Returns(Func<T1, T2, TReturn> callback)
	{
		_returnCallbacks.Add(callback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public ReturnMethodSetup<TReturn, T1, T2> Returns(Func<TReturn> callback)
	{
		_returnCallbacks.Add((_, _) => callback());
		return this;
	}

	/// <summary>
	///     Registers the <paramref name="returnValue" /> for this method.
	/// </summary>
	public ReturnMethodSetup<TReturn, T1, T2> Returns(TReturn returnValue)
	{
		_returnCallbacks.Add((_, _) => returnValue);
		return this;
	}

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the method is invoked.
	/// </summary>
	public ReturnMethodSetup<TReturn, T1, T2> Throws(Exception exception)
	{
		_returnCallbacks.Add((_, _) => throw exception);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	public ReturnMethodSetup<TReturn, T1, T2> Throws(Func<Exception> callback)
	{
		_returnCallbacks.Add((_, _) => throw callback());
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	public ReturnMethodSetup<TReturn, T1, T2> Throws(Func<T1, T2, Exception> callback)
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
			? $"{FormatType(typeof(TReturn))} {_name}({_matches})"
			: $"{FormatType(typeof(TReturn))} {_name}({_match1}, {_match2})";
}

/// <summary>
///     Setup for a method with three parameters <typeparamref name="T1" />, <typeparamref name="T2" /> and
///     <typeparamref name="T3" /> returning <typeparamref name="TReturn" />.
/// </summary>
public class ReturnMethodSetup<TReturn, T1, T2, T3> : MethodSetup
{
	private readonly List<Action<T1, T2, T3>> _callbacks = [];
	private readonly List<Func<T1, T2, T3, TReturn>> _returnCallbacks = [];
	private readonly string _name;
	private readonly Match.IParameters? _matches;
	private readonly Match.NamedParameter? _match1;
	private readonly Match.NamedParameter? _match2;
	private readonly Match.NamedParameter? _match3;
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

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	public ReturnMethodSetup<TReturn, T1, T2, T3> Callback(Action callback)
	{
		_callbacks.Add((_, _, _) => callback());
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	public ReturnMethodSetup<TReturn, T1, T2, T3> Callback(Action<T1, T2, T3> callback)
	{
		_callbacks.Add(callback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public ReturnMethodSetup<TReturn, T1, T2, T3> Returns(Func<T1, T2, T3, TReturn> callback)
	{
		_returnCallbacks.Add(callback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public ReturnMethodSetup<TReturn, T1, T2, T3> Returns(Func<TReturn> callback)
	{
		_returnCallbacks.Add((_, _, _) => callback());
		return this;
	}

	/// <summary>
	///     Registers the <paramref name="returnValue" /> for this method.
	/// </summary>
	public ReturnMethodSetup<TReturn, T1, T2, T3> Returns(TReturn returnValue)
	{
		_returnCallbacks.Add((_, _, _) => returnValue);
		return this;
	}

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the method is invoked.
	/// </summary>
	public ReturnMethodSetup<TReturn, T1, T2, T3> Throws(Exception exception)
	{
		_returnCallbacks.Add((_, _, _) => throw exception);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	public ReturnMethodSetup<TReturn, T1, T2, T3> Throws(Func<Exception> callback)
	{
		_returnCallbacks.Add((_, _, _) => throw callback());
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	public ReturnMethodSetup<TReturn, T1, T2, T3> Throws(Func<T1, T2, T3, Exception> callback)
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
			? $"{FormatType(typeof(TReturn))} {_name}({_matches})"
			: $"{FormatType(typeof(TReturn))} {_name}({_match1}, {_match2}, {_match3})";
}

#pragma warning disable S2436 // Types and methods should not have too many generic parameters
/// <summary>
///     Setup for a method with four parameters <typeparamref name="T1" />, <typeparamref name="T2" />,
///     <typeparamref name="T3" /> and <typeparamref name="T4" /> returning <typeparamref name="TReturn" />.
/// </summary>
public class ReturnMethodSetup<TReturn, T1, T2, T3, T4> : MethodSetup
{
	private readonly List<Action<T1, T2, T3, T4>> _callbacks = [];
	private readonly List<Func<T1, T2, T3, T4, TReturn>> _returnCallbacks = [];
	private readonly string _name;
	private readonly Match.IParameters? _matches;
	private readonly Match.NamedParameter? _match1;
	private readonly Match.NamedParameter? _match2;
	private readonly Match.NamedParameter? _match3;
	private readonly Match.NamedParameter? _match4;
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

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	public ReturnMethodSetup<TReturn, T1, T2, T3, T4> Callback(Action callback)
	{
		_callbacks.Add((_, _, _, _) => callback());
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	public ReturnMethodSetup<TReturn, T1, T2, T3, T4> Callback(Action<T1, T2, T3, T4> callback)
	{
		_callbacks.Add(callback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public ReturnMethodSetup<TReturn, T1, T2, T3, T4> Returns(Func<T1, T2, T3, T4, TReturn> callback)
	{
		_returnCallbacks.Add(callback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public ReturnMethodSetup<TReturn, T1, T2, T3, T4> Returns(Func<TReturn> callback)
	{
		_returnCallbacks.Add((_, _, _, _) => callback());
		return this;
	}

	/// <summary>
	///     Registers the <paramref name="returnValue" /> for this method.
	/// </summary>
	public ReturnMethodSetup<TReturn, T1, T2, T3, T4> Returns(TReturn returnValue)
	{
		_returnCallbacks.Add((_, _, _, _) => returnValue);
		return this;
	}

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the method is invoked.
	/// </summary>
	public ReturnMethodSetup<TReturn, T1, T2, T3, T4> Throws(Exception exception)
	{
		_returnCallbacks.Add((_, _, _, _) => throw exception);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	public ReturnMethodSetup<TReturn, T1, T2, T3, T4> Throws(Func<Exception> callback)
	{
		_returnCallbacks.Add((_, _, _, _) => throw callback());
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	public ReturnMethodSetup<TReturn, T1, T2, T3, T4> Throws(Func<T1, T2, T3, T4, Exception> callback)
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
		   (_matches is not null
			   ? _matches.Matches(invocation.Parameters)
			   : Matches([_match1!, _match2!, _match3!, _match4!,], invocation.Parameters));

	/// <inheritdoc cref="MethodSetup.SetOutParameter{T}(string, MockBehavior)" />
	protected override T SetOutParameter<T>(string parameterName, MockBehavior behavior)
	{
		if (_match1 is not null && _match2 is not null && _match3 is not null && _match4 is not null &&
			HasOutParameter([_match1, _match2, _match3, _match4,], parameterName, out Match.IOutParameter<T>? outParameter))
		{
			return outParameter.GetValue(behavior);
		}

		return behavior.DefaultValue.Generate<T>();
	}

	/// <inheritdoc cref="MethodSetup.SetRefParameter{T}(string, T, MockBehavior)" />
	protected override T SetRefParameter<T>(string parameterName, T value, MockBehavior behavior)
	{
		if (_match1 is not null && _match2 is not null && _match3 is not null && _match4 is not null &&
			HasRefParameter([_match1, _match2, _match3, _match4,], parameterName, out Match.IRefParameter<T>? refParameter))
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
