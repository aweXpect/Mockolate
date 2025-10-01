using System;
using Mockerade.Checks;
using Mockerade.Exceptions;

namespace Mockerade.Setup;

/// <summary>
///     Setup for a method returning <see langword="void" />
/// </summary>
public class VoidMethodSetup(string name) : MethodSetup
{
	private Action? _callback;

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	public VoidMethodSetup Callback(Action callback)
	{
		_callback = callback;
		return this;
	}

	/// <inheritdoc cref="MethodSetup.ExecuteCallback(MethodInvocation, MockBehavior)" />
	protected override void ExecuteCallback(MethodInvocation invocation, MockBehavior behavior) => _callback?.Invoke();

	/// <inheritdoc cref="MethodSetup.GetReturnValue{TResult}(MethodInvocation, MockBehavior)" />
	protected override TResult GetReturnValue<TResult>(MethodInvocation invocation, MockBehavior behavior)
		where TResult : default
		=> throw new MockException("The method setup does not support return values.");

	/// <inheritdoc cref="MethodSetup.IsMatch(Invocation)" />
	protected override bool IsMatch(Invocation invocation)
		=> invocation is MethodInvocation methodInvocation && methodInvocation.Name.Equals(name) &&
		   methodInvocation.Parameters.Length == 0;

	/// <inheritdoc cref="MethodSetup.SetOutParameter{T}(string, MockBehavior)" />
	protected internal override T SetOutParameter<T>(string parameterName, MockBehavior behavior)
		=> behavior.DefaultValueGenerator.Generate<T>();

	/// <inheritdoc cref="MethodSetup.SetRefParameter{T}(string, T, MockBehavior)" />
	protected internal override T SetRefParameter<T>(string parameterName, T value, MockBehavior behavior)
		=> value;
}

/// <summary>
///     Setup for a method with one parameter <typeparamref name="T1" /> returning <see langword="void" />.
/// </summary>
public class VoidMethodSetup<T1>(string name, With.NamedParameter match1) : MethodSetup
{
	private Action<T1>? _callback;

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

	/// <inheritdoc cref="MethodSetup.ExecuteCallback(MethodInvocation, MockBehavior)" />
	protected override void ExecuteCallback(MethodInvocation invocation, MockBehavior behavior)
	{
		if (TryCast<T1>(invocation.Parameters[0], out var p1, behavior))
		{
			_callback?.Invoke(p1);
		}
	}

	/// <inheritdoc cref="MethodSetup.GetReturnValue{TResult}(MethodInvocation, MockBehavior)" />
	protected override TResult GetReturnValue<TResult>(MethodInvocation invocation, MockBehavior behavior)
		where TResult : default
		=> throw new MockException("The method setup does not support return values.");

	/// <inheritdoc cref="MethodSetup.IsMatch(Invocation)" />
	protected override bool IsMatch(Invocation invocation)
		=> invocation is MethodInvocation methodInvocation && methodInvocation.Name.Equals(name) &&
		   Matches([match1], methodInvocation.Parameters);

	/// <inheritdoc cref="MethodSetup.SetOutParameter{T}(string, MockBehavior)" />
	protected internal override T SetOutParameter<T>(string parameterName, MockBehavior behavior)
	{
		if (HasOutParameter([match1], parameterName, out With.OutParameter<T>? outParameter))
		{
			return outParameter.GetValue();
		}

		return behavior.DefaultValueGenerator.Generate<T>();
	}

	/// <inheritdoc cref="MethodSetup.SetRefParameter{T}(string, T, MockBehavior)" />
	protected internal override T SetRefParameter<T>(string parameterName, T value, MockBehavior behavior)
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

	/// <inheritdoc cref="MethodSetup.ExecuteCallback(MethodInvocation, MockBehavior)" />
	protected override void ExecuteCallback(MethodInvocation invocation, MockBehavior behavior)
	{
		if (TryCast<T1>(invocation.Parameters[0], out var p1, behavior) &&
			TryCast<T2>(invocation.Parameters[1], out var p2, behavior))
		{
			_callback?.Invoke(p1, p2);
		}
	}

	/// <inheritdoc cref="MethodSetup.GetReturnValue{TResult}(MethodInvocation, MockBehavior)" />
	protected override TResult GetReturnValue<TResult>(MethodInvocation invocation, MockBehavior behavior)
		where TResult : default
		=> throw new MockException("The method setup does not support return values.");

	/// <inheritdoc cref="MethodSetup.IsMatch(Invocation)" />
	protected override bool IsMatch(Invocation invocation)
		=> invocation is MethodInvocation methodInvocation && methodInvocation.Name.Equals(name) &&
		   Matches([match1, match2], methodInvocation.Parameters);

	/// <inheritdoc cref="MethodSetup.SetOutParameter{T}(string, MockBehavior)" />
	protected internal override T SetOutParameter<T>(string parameterName, MockBehavior behavior)
	{
		if (HasOutParameter([match1, match2], parameterName, out With.OutParameter<T>? outParameter))
		{
			return outParameter.GetValue();
		}

		return behavior.DefaultValueGenerator.Generate<T>();
	}

	/// <inheritdoc cref="MethodSetup.SetRefParameter{T}(string, T, MockBehavior)" />
	protected internal override T SetRefParameter<T>(string parameterName, T value, MockBehavior behavior)
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

	/// <inheritdoc cref="MethodSetup.ExecuteCallback(MethodInvocation, MockBehavior)" />
	protected override void ExecuteCallback(MethodInvocation invocation, MockBehavior behavior)
	{
		if (TryCast<T1>(invocation.Parameters[0], out var p1, behavior) &&
			TryCast<T2>(invocation.Parameters[1], out var p2, behavior) &&
			TryCast<T3>(invocation.Parameters[2], out var p3, behavior))
		{
			_callback?.Invoke(p1, p2, p3);
		}
	}

	/// <inheritdoc cref="MethodSetup.GetReturnValue{TResult}(MethodInvocation, MockBehavior)" />
	protected override TResult GetReturnValue<TResult>(MethodInvocation invocation, MockBehavior behavior)
		where TResult : default
		=> throw new MockException("The method setup does not support return values.");

	/// <inheritdoc cref="MethodSetup.IsMatch(Invocation)" />
	protected override bool IsMatch(Invocation invocation)
		=> invocation is MethodInvocation methodInvocation && methodInvocation.Name.Equals(name) &&
		   Matches([match1, match2, match3], methodInvocation.Parameters);

	/// <inheritdoc cref="MethodSetup.SetOutParameter{T}(string, MockBehavior)" />
	protected internal override T SetOutParameter<T>(string parameterName, MockBehavior behavior)
	{
		if (HasOutParameter([match1, match2, match3], parameterName, out With.OutParameter<T>? outParameter))
		{
			return outParameter.GetValue();
		}

		return behavior.DefaultValueGenerator.Generate<T>();
	}

	/// <inheritdoc cref="MethodSetup.SetRefParameter{T}(string, T, MockBehavior)" />
	protected internal override T SetRefParameter<T>(string parameterName, T value, MockBehavior behavior)
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

	/// <inheritdoc cref="MethodSetup.ExecuteCallback(MethodInvocation, MockBehavior)" />
	protected override void ExecuteCallback(MethodInvocation invocation, MockBehavior behavior)
	{
		if (TryCast<T1>(invocation.Parameters[0], out var p1, behavior) &&
			TryCast<T2>(invocation.Parameters[1], out var p2, behavior) &&
			TryCast<T3>(invocation.Parameters[2], out var p3, behavior) &&
			TryCast<T4>(invocation.Parameters[3], out var p4, behavior))
		{
			_callback?.Invoke(p1, p2, p3, p4);
		}
	}

	/// <inheritdoc cref="MethodSetup.GetReturnValue{TResult}(MethodInvocation, MockBehavior)" />
	protected override TResult GetReturnValue<TResult>(MethodInvocation invocation, MockBehavior behavior)
		where TResult : default
		=> throw new MockException("The method setup does not support return values.");

	/// <inheritdoc cref="MethodSetup.IsMatch(Invocation)" />
	protected override bool IsMatch(Invocation invocation)
		=> invocation is MethodInvocation methodInvocation && methodInvocation.Name.Equals(name) &&
		   Matches([match1, match2, match3, match4], methodInvocation.Parameters);

	/// <inheritdoc cref="MethodSetup.SetOutParameter{T}(string, MockBehavior)" />
	protected internal override T SetOutParameter<T>(string parameterName, MockBehavior behavior)
	{
		if (HasOutParameter([match1, match2, match3, match4], parameterName, out With.OutParameter<T>? outParameter))
		{
			return outParameter.GetValue();
		}

		return behavior.DefaultValueGenerator.Generate<T>();
	}

	/// <inheritdoc cref="MethodSetup.SetRefParameter{T}(string, T, MockBehavior)" />
	protected internal override T SetRefParameter<T>(string parameterName, T value, MockBehavior behavior)
	{
		if (HasRefParameter([match1, match2, match3, match4], parameterName, out With.RefParameter<T>? refParameter))
		{
			return refParameter.GetValue(value);
		}

		return value;
	}
}
