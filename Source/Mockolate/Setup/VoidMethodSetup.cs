using System;
using System.Collections.Generic;
using Mockolate.Exceptions;
using Mockolate.Interactions;
using Mockolate.Parameters;

namespace Mockolate.Setup;

/// <summary>
///     Sets up a method returning <see langword="void" />.
/// </summary>
public class VoidMethodSetup(string name)
	: MethodSetup(new MethodParameterMatch(name, [])),
		IVoidMethodSetupCallbackBuilder, IVoidMethodSetupReturnBuilder
{
	private readonly List<Callback<Action<int>>> _callbacks = [];
	private readonly List<Callback<Action<int>>> _returnCallbacks = [];
	private Callback? _currentCallback;
	private int _currentCallbacksIndex;
	private Callback? _currentReturnCallback;
	private int _currentReturnCallbackIndex;
	private bool? _skipBaseClass;

	/// <inheritdoc cref="IVoidMethodSetup.SkippingBaseClass(bool)" />
	public IVoidMethodSetup SkippingBaseClass(bool skipBaseClass = true)
	{
		_skipBaseClass = skipBaseClass;
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetup.Do(Action)" />
	public IVoidMethodSetupCallbackBuilder Do(Action callback)
	{
		Callback<Action<int>> currentCallback = new(_ => callback());
		_currentCallback = currentCallback;
		_callbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetup.Do(Action{int})" />
	public IVoidMethodSetupCallbackBuilder Do(Action<int> callback)
	{
		Callback<Action<int>> currentCallback = new(callback);
		_currentCallback = currentCallback;
		_callbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetup.DoesNotThrow()" />
	public IVoidMethodSetup DoesNotThrow()
	{
		Callback<Action<int>> currentCallback = new(_ => { });
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetup.Throws{TException}()" />
	public IVoidMethodSetupReturnBuilder Throws<TException>()
		where TException : Exception, new()
	{
		Callback<Action<int>> currentCallback = new(_ => throw new TException());
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetup.Throws(Exception)" />
	public IVoidMethodSetupReturnBuilder Throws(Exception exception)
	{
		Callback<Action<int>> currentCallback = new(_ => throw exception);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetup.Throws(Func{Exception})" />
	public IVoidMethodSetupReturnBuilder Throws(Func<Exception> callback)
	{
		Callback<Action<int>> currentCallback = new(_ => throw callback());
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetupCallbackBuilder.InParallel()" />
	IVoidMethodSetupCallbackBuilder IVoidMethodSetupCallbackBuilder.InParallel()
	{
		_currentCallback?.InParallel();
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetupCallbackBuilder.When(Func{int, bool})" />
	IVoidMethodSetupCallbackWhenBuilder IVoidMethodSetupCallbackBuilder.When(Func<int, bool> predicate)
	{
		_currentCallback?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetupCallbackWhenBuilder.For(int)" />
	IVoidMethodSetupCallbackWhenBuilder IVoidMethodSetupCallbackWhenBuilder.For(int times)
	{
		_currentCallback?.For(times);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetupCallbackWhenBuilder.Only(int)" />
	IVoidMethodSetup IVoidMethodSetupCallbackWhenBuilder.Only(int times)
	{
		_currentCallback?.Only(times);
		return this;
	}


	/// <inheritdoc cref="IVoidMethodSetupReturnBuilder.When(Func{int, bool})" />
	IVoidMethodSetupReturnWhenBuilder IVoidMethodSetupReturnBuilder.When(Func<int, bool> predicate)
	{
		_currentReturnCallback?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetupReturnWhenBuilder.For(int)" />
	IVoidMethodSetupReturnWhenBuilder IVoidMethodSetupReturnWhenBuilder.For(int times)
	{
		_currentReturnCallback?.For(times);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetupReturnWhenBuilder.Only(int)" />
	IVoidMethodSetup IVoidMethodSetupReturnWhenBuilder.Only(int times)
	{
		_currentReturnCallback?.Only(times);
		return this;
	}

	/// <inheritdoc cref="MethodSetup.ExecuteCallback(MethodInvocation, MockBehavior)" />
	protected override void ExecuteCallback(MethodInvocation invocation, MockBehavior behavior)
	{
		bool wasInvoked = false;
		int currentCallbacksIndex = _currentCallbacksIndex;
		for (int i = 0; i < _callbacks.Count; i++)
		{
			Callback<Action<int>> callback =
				_callbacks[(currentCallbacksIndex + i) % _callbacks.Count];
			if (callback.Invoke(wasInvoked, ref _currentCallbacksIndex, (invocationCount, @delegate)
				    => @delegate(invocationCount)))
			{
				wasInvoked = true;
			}
		}

		foreach (Callback<Action<int>> _ in _returnCallbacks)
		{
			Callback<Action<int>> returnCallback =
				_returnCallbacks[_currentReturnCallbackIndex % _returnCallbacks.Count];
			if (returnCallback.Invoke(ref _currentReturnCallbackIndex, (invocationCount, @delegate)
				    => @delegate(invocationCount)))
			{
				return;
			}
		}
	}

	/// <inheritdoc cref="MethodSetup.GetReturnValue{TResult}(MethodInvocation, MockBehavior, Func{TResult})" />
	protected override TResult GetReturnValue<TResult>(MethodInvocation invocation, MockBehavior behavior,
		Func<TResult> defaultValueGenerator)
		where TResult : default
		=> throw new MockException("The method setup does not support return values.");

	/// <inheritdoc cref="MethodSetup.TriggerParameterCallbacks(object?[])" />
	protected override void TriggerParameterCallbacks(object?[] parameters)
	{
		// No parameters to trigger
	}

	/// <inheritdoc cref="MethodSetup.GetSkipBaseClass()" />
	protected override bool? GetSkipBaseClass()
		=> _skipBaseClass;

	/// <inheritdoc cref="MethodSetup.SetOutParameter{T}(string, Func{T})" />
	protected override T SetOutParameter<T>(string parameterName, Func<T> defaultValueGenerator)
		=> defaultValueGenerator();

	/// <inheritdoc cref="MethodSetup.SetRefParameter{T}(string, T, MockBehavior)" />
	protected override T SetRefParameter<T>(string parameterName, T value, MockBehavior behavior)
		=> value;

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString() => $"void {name}()";
}

/// <summary>
///     Setup for a method with one parameter <typeparamref name="T1" /> returning <see langword="void" />.
/// </summary>
public class VoidMethodSetup<T1> : MethodSetup,
	IVoidMethodSetupCallbackBuilder<T1>, IVoidMethodSetupReturnBuilder<T1>
{
	private readonly List<Callback<Action<int, T1>>> _callbacks = [];
	private readonly NamedParameter? _match1;
	private readonly IParameters? _matches;
	private readonly string _name;
	private readonly List<Callback<Action<int, T1>>> _returnCallbacks = [];
	private Callback? _currentCallback;
	private int _currentCallbacksIndex;
	private Callback? _currentReturnCallback;
	private int _currentReturnCallbackIndex;
	private bool? _skipBaseClass;

	/// <inheritdoc cref="VoidMethodSetup{T1}" />
	public VoidMethodSetup(string name, NamedParameter match1)
		: base(new MethodParameterMatch(name, [match1,]))
	{
		_name = name;
		_match1 = match1;
	}

	/// <inheritdoc cref="VoidMethodSetup{T1}" />
	public VoidMethodSetup(string name, IParameters matches)
		: base(new MethodParametersMatch(name, matches))
	{
		_name = name;
		_matches = matches;
	}

	/// <inheritdoc cref="IVoidMethodSetup{T1}.SkippingBaseClass(bool)" />
	public IVoidMethodSetup<T1> SkippingBaseClass(bool skipBaseClass = true)
	{
		_skipBaseClass = skipBaseClass;
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetup{T1}.Do(Action)" />
	public IVoidMethodSetupCallbackBuilder<T1> Do(Action callback)
	{
		Callback<Action<int, T1>> currentCallback = new((_, _) => callback());
		_currentCallback = currentCallback;
		_callbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetup{T1}.Do(Action{T1})" />
	public IVoidMethodSetupCallbackBuilder<T1> Do(Action<T1> callback)
	{
		Callback<Action<int, T1>> currentCallback = new((_, p1) => callback(p1));
		_currentCallback = currentCallback;
		_callbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetup{T1}.Do(Action{int, T1})" />
	public IVoidMethodSetupCallbackBuilder<T1> Do(Action<int, T1> callback)
	{
		Callback<Action<int, T1>> currentCallback = new(callback);
		_currentCallback = currentCallback;
		_callbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetup{T1}.DoesNotThrow()" />
	public IVoidMethodSetup<T1> DoesNotThrow()
	{
		Callback<Action<int, T1>> currentCallback = new((_, _) => { });
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetup{T1}.Throws{TException}()" />
	public IVoidMethodSetupReturnBuilder<T1> Throws<TException>()
		where TException : Exception, new()
	{
		Callback<Action<int, T1>> currentCallback = new((_, _) => throw new TException());
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetup{T1}.Throws(Exception)" />
	public IVoidMethodSetupReturnBuilder<T1> Throws(Exception exception)
	{
		Callback<Action<int, T1>> currentCallback = new((_, _) => throw exception);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetup{T1}.Throws(Func{Exception})" />
	public IVoidMethodSetupReturnBuilder<T1> Throws(Func<Exception> callback)
	{
		Callback<Action<int, T1>> currentCallback = new((_, _) => throw callback());
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetup{T1}.Throws(Func{T1, Exception})" />
	public IVoidMethodSetupReturnBuilder<T1> Throws(Func<T1, Exception> callback)
	{
		Callback<Action<int, T1>> currentCallback = new((_, p1) => throw callback(p1));
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetupCallbackBuilder{T1}.InParallel()" />
	IVoidMethodSetupCallbackBuilder<T1> IVoidMethodSetupCallbackBuilder<T1>.InParallel()
	{
		_currentCallback?.InParallel();
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetupCallbackBuilder{T1}.When(Func{int, bool})" />
	IVoidMethodSetupCallbackWhenBuilder<T1> IVoidMethodSetupCallbackBuilder<T1>.When(Func<int, bool> predicate)
	{
		_currentCallback?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetupCallbackWhenBuilder{T1}.For(int)" />
	IVoidMethodSetupCallbackWhenBuilder<T1> IVoidMethodSetupCallbackWhenBuilder<T1>.For(int times)
	{
		_currentCallback?.For(times);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetupCallbackWhenBuilder{T1}.Only(int)" />
	IVoidMethodSetup<T1> IVoidMethodSetupCallbackWhenBuilder<T1>.Only(int times)
	{
		_currentCallback?.Only(times);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetupReturnBuilder{T1}.When(Func{int, bool})" />
	IVoidMethodSetupReturnWhenBuilder<T1> IVoidMethodSetupReturnBuilder<T1>.When(Func<int, bool> predicate)
	{
		_currentReturnCallback?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetupReturnWhenBuilder{T1}.For(int)" />
	IVoidMethodSetupReturnWhenBuilder<T1> IVoidMethodSetupReturnWhenBuilder<T1>.For(int times)
	{
		_currentReturnCallback?.For(times);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetupReturnWhenBuilder{T1}.Only(int)" />
	IVoidMethodSetup<T1> IVoidMethodSetupReturnWhenBuilder<T1>.Only(int times)
	{
		_currentReturnCallback?.Only(times);
		return this;
	}

	/// <inheritdoc cref="MethodSetup.ExecuteCallback(MethodInvocation, MockBehavior)" />
	protected override void ExecuteCallback(MethodInvocation invocation, MockBehavior behavior)
	{
		if (TryCast(invocation.Parameters[0].Value, out T1 p1, behavior))
		{
			bool wasInvoked = false;
			int currentCallbacksIndex = _currentCallbacksIndex;
			for (int i = 0; i < _callbacks.Count; i++)
			{
				Callback<Action<int, T1>> callback =
					_callbacks[(currentCallbacksIndex + i) % _callbacks.Count];
				if (callback.Invoke(wasInvoked, ref _currentCallbacksIndex, (invocationCount, @delegate)
					    => @delegate(invocationCount, p1)))
				{
					wasInvoked = true;
				}
			}

			foreach (Callback<Action<int, T1>> _ in _returnCallbacks)
			{
				Callback<Action<int, T1>> returnCallback =
					_returnCallbacks[_currentReturnCallbackIndex % _returnCallbacks.Count];
				if (returnCallback.Invoke(ref _currentReturnCallbackIndex, (invocationCount, @delegate)
					    => @delegate(invocationCount, p1)))
				{
					return;
				}
			}
		}
	}

	/// <inheritdoc cref="MethodSetup.GetReturnValue{TResult}(MethodInvocation, MockBehavior, Func{TResult})" />
	protected override TResult GetReturnValue<TResult>(MethodInvocation invocation, MockBehavior behavior,
		Func<TResult> defaultValueGenerator)
		where TResult : default
		=> throw new MockException("The method setup does not support return values.");

	/// <inheritdoc cref="MethodSetup.TriggerParameterCallbacks(object?[])" />
	protected override void TriggerParameterCallbacks(object?[] parameters)
		=> TriggerCallbacks([_match1,], parameters);

	/// <inheritdoc cref="MethodSetup.GetSkipBaseClass()" />
	protected override bool? GetSkipBaseClass()
		=> _skipBaseClass;

	/// <inheritdoc cref="MethodSetup.SetOutParameter{T}(string, Func{T})" />
	protected override T SetOutParameter<T>(string parameterName, Func<T> defaultValueGenerator)
	{
		if (HasOutParameter([_match1,], parameterName, out IOutParameter<T>? outParameter))
		{
			return outParameter.GetValue(defaultValueGenerator);
		}

		return defaultValueGenerator();
	}

	/// <inheritdoc cref="MethodSetup.SetRefParameter{T}(string, T, MockBehavior)" />
	protected override T SetRefParameter<T>(string parameterName, T value, MockBehavior behavior)
	{
		if (HasRefParameter([_match1,], parameterName, out IRefParameter<T>? refParameter))
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
public class VoidMethodSetup<T1, T2> : MethodSetup,
	IVoidMethodSetupCallbackBuilder<T1, T2>, IVoidMethodSetupReturnBuilder<T1, T2>
{
	private readonly List<Callback<Action<int, T1, T2>>> _callbacks = [];
	private readonly NamedParameter? _match1;
	private readonly NamedParameter? _match2;
	private readonly IParameters? _matches;
	private readonly string _name;
	private readonly List<Callback<Action<int, T1, T2>>> _returnCallbacks = [];
	private Callback? _currentCallback;
	private int _currentCallbacksIndex;
	private Callback? _currentReturnCallback;
	private int _currentReturnCallbackIndex;
	private bool? _skipBaseClass;

	/// <inheritdoc cref="VoidMethodSetup{T1, T2}" />
	public VoidMethodSetup(string name, NamedParameter match1, NamedParameter match2)
		: base(new MethodParameterMatch(name, [match1, match2,]))
	{
		_name = name;
		_match1 = match1;
		_match2 = match2;
	}

	/// <inheritdoc cref="VoidMethodSetup{T1, T2}" />
	public VoidMethodSetup(string name, IParameters matches)
		: base(new MethodParametersMatch(name, matches))
	{
		_name = name;
		_matches = matches;
	}

	/// <inheritdoc cref="IVoidMethodSetup{T1, T2}.SkippingBaseClass(bool)" />
	public IVoidMethodSetup<T1, T2> SkippingBaseClass(bool skipBaseClass = true)
	{
		_skipBaseClass = skipBaseClass;
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetup{T1, T2}.Do(Action)" />
	public IVoidMethodSetupCallbackBuilder<T1, T2> Do(Action callback)
	{
		Callback<Action<int, T1, T2>> currentCallback = new((_, _, _) => callback());
		_currentCallback = currentCallback;
		_callbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetup{T1, T2}.Do(Action{T1, T2})" />
	public IVoidMethodSetupCallbackBuilder<T1, T2> Do(Action<T1, T2> callback)
	{
		Callback<Action<int, T1, T2>> currentCallback = new((_, p1, p2) => callback(p1, p2));
		_currentCallback = currentCallback;
		_callbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetup{T1, T2}.Do(Action{int, T1, T2})" />
	public IVoidMethodSetupCallbackBuilder<T1, T2> Do(Action<int, T1, T2> callback)
	{
		Callback<Action<int, T1, T2>> currentCallback = new(callback);
		_currentCallback = currentCallback;
		_callbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetup{T1, T2}.DoesNotThrow()" />
	public IVoidMethodSetup<T1, T2> DoesNotThrow()
	{
		Callback<Action<int, T1, T2>> currentCallback = new((_, _, _) => { });
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetup{T1, T2}.Throws{TException}()" />
	public IVoidMethodSetupReturnBuilder<T1, T2> Throws<TException>()
		where TException : Exception, new()
	{
		Callback<Action<int, T1, T2>> currentCallback = new((_, _, _) => throw new TException());
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetup{T1, T2}.Throws(Exception)" />
	public IVoidMethodSetupReturnBuilder<T1, T2> Throws(Exception exception)
	{
		Callback<Action<int, T1, T2>> currentCallback = new((_, _, _) => throw exception);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetup{T1, T2}.Throws(Func{Exception})" />
	public IVoidMethodSetupReturnBuilder<T1, T2> Throws(Func<Exception> callback)
	{
		Callback<Action<int, T1, T2>> currentCallback = new((_, _, _) => throw callback());
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetup{T1, T2}.Throws(Func{T1, T2, Exception})" />
	public IVoidMethodSetupReturnBuilder<T1, T2> Throws(Func<T1, T2, Exception> callback)
	{
		Callback<Action<int, T1, T2>> currentCallback = new((_, p1, p2) => throw callback(p1, p2));
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetupCallbackBuilder{T1, T2}.InParallel()" />
	IVoidMethodSetupCallbackBuilder<T1, T2> IVoidMethodSetupCallbackBuilder<T1, T2>.InParallel()
	{
		_currentCallback?.InParallel();
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetupCallbackBuilder{T1, T2}.When(Func{int, bool})" />
	IVoidMethodSetupCallbackWhenBuilder<T1, T2> IVoidMethodSetupCallbackBuilder<T1, T2>.When(Func<int, bool> predicate)
	{
		_currentCallback?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetupCallbackWhenBuilder{T1, T2}.For(int)" />
	IVoidMethodSetupCallbackWhenBuilder<T1, T2> IVoidMethodSetupCallbackWhenBuilder<T1, T2>.For(int times)
	{
		_currentCallback?.For(times);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetupCallbackWhenBuilder{T1, T2}.Only(int)" />
	IVoidMethodSetup<T1, T2> IVoidMethodSetupCallbackWhenBuilder<T1, T2>.Only(int times)
	{
		_currentCallback?.Only(times);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetupReturnBuilder{T1, T2}.When(Func{int, bool})" />
	IVoidMethodSetupReturnWhenBuilder<T1, T2> IVoidMethodSetupReturnBuilder<T1, T2>.When(Func<int, bool> predicate)
	{
		_currentReturnCallback?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetupReturnWhenBuilder{T1, T2}.For(int)" />
	IVoidMethodSetupReturnWhenBuilder<T1, T2> IVoidMethodSetupReturnWhenBuilder<T1, T2>.For(int times)
	{
		_currentReturnCallback?.For(times);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetupReturnWhenBuilder{T1, T2}.Only(int)" />
	IVoidMethodSetup<T1, T2> IVoidMethodSetupReturnWhenBuilder<T1, T2>.Only(int times)
	{
		_currentReturnCallback?.Only(times);
		return this;
	}

	/// <inheritdoc cref="MethodSetup.ExecuteCallback(MethodInvocation, MockBehavior)" />
	protected override void ExecuteCallback(MethodInvocation invocation, MockBehavior behavior)
	{
		if (TryCast(invocation.Parameters[0].Value, out T1 p1, behavior) &&
		    TryCast(invocation.Parameters[1].Value, out T2 p2, behavior))
		{
			bool wasInvoked = false;
			int currentCallbacksIndex = _currentCallbacksIndex;
			for (int i = 0; i < _callbacks.Count; i++)
			{
				Callback<Action<int, T1, T2>> callback =
					_callbacks[(currentCallbacksIndex + i) % _callbacks.Count];
				if (callback.Invoke(wasInvoked, ref _currentCallbacksIndex, (invocationCount, @delegate)
					    => @delegate(invocationCount, p1, p2)))
				{
					wasInvoked = true;
				}
			}

			foreach (Callback<Action<int, T1, T2>> _ in _returnCallbacks)
			{
				Callback<Action<int, T1, T2>> returnCallback =
					_returnCallbacks[_currentReturnCallbackIndex % _returnCallbacks.Count];
				if (returnCallback.Invoke(ref _currentReturnCallbackIndex, (invocationCount, @delegate)
					    => @delegate(invocationCount, p1, p2)))
				{
					return;
				}
			}
		}
	}

	/// <inheritdoc cref="MethodSetup.GetReturnValue{TResult}(MethodInvocation, MockBehavior, Func{TResult})" />
	protected override TResult GetReturnValue<TResult>(MethodInvocation invocation, MockBehavior behavior,
		Func<TResult> defaultValueGenerator)
		where TResult : default
		=> throw new MockException("The method setup does not support return values.");

	/// <inheritdoc cref="MethodSetup.TriggerParameterCallbacks(object?[])" />
	protected override void TriggerParameterCallbacks(object?[] parameters)
		=> TriggerCallbacks([_match1, _match2,], parameters);

	/// <inheritdoc cref="MethodSetup.GetSkipBaseClass()" />
	protected override bool? GetSkipBaseClass()
		=> _skipBaseClass;

	/// <inheritdoc cref="MethodSetup.SetOutParameter{T}(string, Func{T})" />
	protected override T SetOutParameter<T>(string parameterName, Func<T> defaultValueGenerator)
	{
		if (HasOutParameter([_match1, _match2,], parameterName, out IOutParameter<T>? outParameter))
		{
			return outParameter.GetValue(defaultValueGenerator);
		}

		return defaultValueGenerator();
	}

	/// <inheritdoc cref="MethodSetup.SetRefParameter{T}(string, T, MockBehavior)" />
	protected override T SetRefParameter<T>(string parameterName, T value, MockBehavior behavior)
	{
		if (HasRefParameter([_match1, _match2,], parameterName, out IRefParameter<T>? refParameter))
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
public class VoidMethodSetup<T1, T2, T3> : MethodSetup,
	IVoidMethodSetupCallbackBuilder<T1, T2, T3>, IVoidMethodSetupReturnBuilder<T1, T2, T3>
{
	private readonly List<Callback<Action<int, T1, T2, T3>>> _callbacks = [];
	private readonly NamedParameter? _match1;
	private readonly NamedParameter? _match2;
	private readonly NamedParameter? _match3;
	private readonly IParameters? _matches;
	private readonly string _name;
	private readonly List<Callback<Action<int, T1, T2, T3>>> _returnCallbacks = [];
	private Callback? _currentCallback;
	private int _currentCallbacksIndex;
	private Callback? _currentReturnCallback;
	private int _currentReturnCallbackIndex;
	private bool? _skipBaseClass;

	/// <inheritdoc cref="VoidMethodSetup{T1, T2, T3}" />
	public VoidMethodSetup(
		string name,
		NamedParameter match1,
		NamedParameter match2,
		NamedParameter match3)
		: base(new MethodParameterMatch(name, [match1, match2, match3,]))
	{
		_name = name;
		_match1 = match1;
		_match2 = match2;
		_match3 = match3;
	}

	/// <inheritdoc cref="VoidMethodSetup{T1, T2, T3}" />
	public VoidMethodSetup(string name, IParameters matches)
		: base(new MethodParametersMatch(name, matches))
	{
		_name = name;
		_matches = matches;
	}

	/// <inheritdoc cref="IVoidMethodSetup{T1, T2, T3}.SkippingBaseClass(bool)" />
	public IVoidMethodSetup<T1, T2, T3> SkippingBaseClass(bool skipBaseClass = true)
	{
		_skipBaseClass = skipBaseClass;
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetup{T1, T2, T3}.Do(Action)" />
	public IVoidMethodSetupCallbackBuilder<T1, T2, T3> Do(Action callback)
	{
		Callback<Action<int, T1, T2, T3>> currentCallback = new((_, _, _, _) => callback());
		_currentCallback = currentCallback;
		_callbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetup{T1, T2, T3}.Do(Action{T1, T2, T3})" />
	public IVoidMethodSetupCallbackBuilder<T1, T2, T3> Do(Action<T1, T2, T3> callback)
	{
		Callback<Action<int, T1, T2, T3>> currentCallback = new((_, p1, p2, p3) => callback(p1, p2, p3));
		_currentCallback = currentCallback;
		_callbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetup{T1, T2, T3}.Do(Action{int, T1, T2, T3})" />
	public IVoidMethodSetupCallbackBuilder<T1, T2, T3> Do(Action<int, T1, T2, T3> callback)
	{
		Callback<Action<int, T1, T2, T3>> currentCallback = new(callback);
		_currentCallback = currentCallback;
		_callbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetup{T1, T2, T3}.DoesNotThrow()" />
	public IVoidMethodSetup<T1, T2, T3> DoesNotThrow()
	{
		Callback<Action<int, T1, T2, T3>> currentCallback = new((_, _, _, _) => { });
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetup{T1, T2, T3}.Throws{TException}()" />
	public IVoidMethodSetupReturnBuilder<T1, T2, T3> Throws<TException>()
		where TException : Exception, new()
	{
		Callback<Action<int, T1, T2, T3>> currentCallback = new((_, _, _, _) => throw new TException());
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetup{T1, T2, T3}.Throws(Exception)" />
	public IVoidMethodSetupReturnBuilder<T1, T2, T3> Throws(Exception exception)
	{
		Callback<Action<int, T1, T2, T3>> currentCallback = new((_, _, _, _) => throw exception);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetup{T1, T2, T3}.Throws(Func{Exception})" />
	public IVoidMethodSetupReturnBuilder<T1, T2, T3> Throws(Func<Exception> callback)
	{
		Callback<Action<int, T1, T2, T3>> currentCallback = new((_, _, _, _) => throw callback());
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetup{T1, T2, T3}.Throws(Func{T1, T2, T3, Exception})" />
	public IVoidMethodSetupReturnBuilder<T1, T2, T3> Throws(Func<T1, T2, T3, Exception> callback)
	{
		Callback<Action<int, T1, T2, T3>> currentCallback = new((_, p1, p2, p3) => throw callback(p1, p2, p3));
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetupCallbackBuilder{T1, T2, T3}.InParallel()" />
	IVoidMethodSetupCallbackBuilder<T1, T2, T3> IVoidMethodSetupCallbackBuilder<T1, T2, T3>.InParallel()
	{
		_currentCallback?.InParallel();
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetupCallbackBuilder{T1, T2, T3}.When(Func{int, bool})" />
	IVoidMethodSetupCallbackWhenBuilder<T1, T2, T3> IVoidMethodSetupCallbackBuilder<T1, T2, T3>.When(
		Func<int, bool> predicate)
	{
		_currentCallback?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetupCallbackWhenBuilder{T1, T2, T3}.For(int)" />
	IVoidMethodSetupCallbackWhenBuilder<T1, T2, T3> IVoidMethodSetupCallbackWhenBuilder<T1, T2, T3>.For(int times)
	{
		_currentCallback?.For(times);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetupCallbackWhenBuilder{T1, T2, T3}.Only(int)" />
	IVoidMethodSetup<T1, T2, T3> IVoidMethodSetupCallbackWhenBuilder<T1, T2, T3>.Only(int times)
	{
		_currentCallback?.Only(times);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetupReturnBuilder{T1, T2, T3}.When(Func{int, bool})" />
	IVoidMethodSetupReturnWhenBuilder<T1, T2, T3> IVoidMethodSetupReturnBuilder<T1, T2, T3>.When(
		Func<int, bool> predicate)
	{
		_currentReturnCallback?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetupReturnWhenBuilder{T1, T2, T3}.For(int)" />
	IVoidMethodSetupReturnWhenBuilder<T1, T2, T3> IVoidMethodSetupReturnWhenBuilder<T1, T2, T3>.For(int times)
	{
		_currentReturnCallback?.For(times);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetupReturnWhenBuilder{T1, T2, T3}.Only(int)" />
	IVoidMethodSetup<T1, T2, T3> IVoidMethodSetupReturnWhenBuilder<T1, T2, T3>.Only(int times)
	{
		_currentReturnCallback?.Only(times);
		return this;
	}

	/// <inheritdoc cref="MethodSetup.ExecuteCallback(MethodInvocation, MockBehavior)" />
	protected override void ExecuteCallback(MethodInvocation invocation, MockBehavior behavior)
	{
		if (TryCast(invocation.Parameters[0].Value, out T1 p1, behavior) &&
		    TryCast(invocation.Parameters[1].Value, out T2 p2, behavior) &&
		    TryCast(invocation.Parameters[2].Value, out T3 p3, behavior))
		{
			bool wasInvoked = false;
			int currentCallbacksIndex = _currentCallbacksIndex;
			for (int i = 0; i < _callbacks.Count; i++)
			{
				Callback<Action<int, T1, T2, T3>> callback =
					_callbacks[(currentCallbacksIndex + i) % _callbacks.Count];
				if (callback.Invoke(wasInvoked, ref _currentCallbacksIndex, (invocationCount, @delegate)
					    => @delegate(invocationCount, p1, p2, p3)))
				{
					wasInvoked = true;
				}
			}

			foreach (Callback<Action<int, T1, T2, T3>> _ in _returnCallbacks)
			{
				Callback<Action<int, T1, T2, T3>> returnCallback =
					_returnCallbacks[_currentReturnCallbackIndex % _returnCallbacks.Count];
				if (returnCallback.Invoke(ref _currentReturnCallbackIndex, (invocationCount, @delegate)
					    => @delegate(invocationCount, p1, p2, p3)))
				{
					return;
				}
			}
		}
	}

	/// <inheritdoc cref="MethodSetup.GetReturnValue{TResult}(MethodInvocation, MockBehavior, Func{TResult})" />
	protected override TResult GetReturnValue<TResult>(MethodInvocation invocation, MockBehavior behavior,
		Func<TResult> defaultValueGenerator)
		where TResult : default
		=> throw new MockException("The method setup does not support return values.");

	/// <inheritdoc cref="MethodSetup.TriggerParameterCallbacks(object?[])" />
	protected override void TriggerParameterCallbacks(object?[] parameters)
		=> TriggerCallbacks([_match1, _match2, _match3,], parameters);

	/// <inheritdoc cref="MethodSetup.GetSkipBaseClass()" />
	protected override bool? GetSkipBaseClass()
		=> _skipBaseClass;

	/// <inheritdoc cref="MethodSetup.SetOutParameter{T}(string, Func{T})" />
	protected override T SetOutParameter<T>(string parameterName, Func<T> defaultValueGenerator)
	{
		if (HasOutParameter([_match1, _match2, _match3,], parameterName, out IOutParameter<T>? outParameter))
		{
			return outParameter.GetValue(defaultValueGenerator);
		}

		return defaultValueGenerator();
	}

	/// <inheritdoc cref="MethodSetup.SetRefParameter{T}(string, T, MockBehavior)" />
	protected override T SetRefParameter<T>(string parameterName, T value, MockBehavior behavior)
	{
		if (HasRefParameter([_match1, _match2, _match3,], parameterName, out IRefParameter<T>? refParameter))
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
public class VoidMethodSetup<T1, T2, T3, T4> : MethodSetup,
	IVoidMethodSetupCallbackBuilder<T1, T2, T3, T4>, IVoidMethodSetupReturnBuilder<T1, T2, T3, T4>
{
	private readonly List<Callback<Action<int, T1, T2, T3, T4>>> _callbacks = [];
	private readonly NamedParameter? _match1;
	private readonly NamedParameter? _match2;
	private readonly NamedParameter? _match3;
	private readonly NamedParameter? _match4;
	private readonly IParameters? _matches;
	private readonly string _name;
	private readonly List<Callback<Action<int, T1, T2, T3, T4>>> _returnCallbacks = [];
	private Callback? _currentCallback;
	private int _currentCallbacksIndex;
	private Callback? _currentReturnCallback;
	private int _currentReturnCallbackIndex;
	private bool? _skipBaseClass;

	/// <inheritdoc cref="VoidMethodSetup{T1, T2, T3, T4}" />
	public VoidMethodSetup(
		string name,
		NamedParameter match1,
		NamedParameter match2,
		NamedParameter match3,
		NamedParameter match4)
		: base(new MethodParameterMatch(name, [match1, match2, match3, match4,]))
	{
		_name = name;
		_match1 = match1;
		_match2 = match2;
		_match3 = match3;
		_match4 = match4;
	}

	/// <inheritdoc cref="VoidMethodSetup{T1, T2, T3, T4}" />
	public VoidMethodSetup(string name, IParameters matches)
		: base(new MethodParametersMatch(name, matches))
	{
		_name = name;
		_matches = matches;
	}

	/// <inheritdoc cref="IVoidMethodSetup{T1, T2, T3, T4}.SkippingBaseClass(bool)" />
	public IVoidMethodSetup<T1, T2, T3, T4> SkippingBaseClass(bool skipBaseClass = true)
	{
		_skipBaseClass = skipBaseClass;
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetup{T1, T2, T3, T4}.Do(Action)" />
	public IVoidMethodSetupCallbackBuilder<T1, T2, T3, T4> Do(Action callback)
	{
		Callback<Action<int, T1, T2, T3, T4>> currentCallback = new((_, _, _, _, _) => callback());
		_currentCallback = currentCallback;
		_callbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetup{T1, T2, T3, T4}.Do(Action{T1, T2, T3, T4})" />
	public IVoidMethodSetupCallbackBuilder<T1, T2, T3, T4> Do(Action<T1, T2, T3, T4> callback)
	{
		Callback<Action<int, T1, T2, T3, T4>> currentCallback = new((_, p1, p2, p3, p4) => callback(p1, p2, p3, p4));
		_currentCallback = currentCallback;
		_callbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetup{T1, T2, T3, T4}.Do(Action{int, T1, T2, T3, T4})" />
	public IVoidMethodSetupCallbackBuilder<T1, T2, T3, T4> Do(Action<int, T1, T2, T3, T4> callback)
	{
		Callback<Action<int, T1, T2, T3, T4>> currentCallback = new(callback);
		_currentCallback = currentCallback;
		_callbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetup{T1, T2, T3, T4}.DoesNotThrow()" />
	public IVoidMethodSetup<T1, T2, T3, T4> DoesNotThrow()
	{
		Callback<Action<int, T1, T2, T3, T4>> currentCallback = new((_, _, _, _, _) => { });
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetup{T1, T2, T3, T4}.Throws{TException}()" />
	public IVoidMethodSetupReturnBuilder<T1, T2, T3, T4> Throws<TException>()
		where TException : Exception, new()
	{
		Callback<Action<int, T1, T2, T3, T4>> currentCallback = new((_, _, _, _, _) => throw new TException());
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetup{T1, T2, T3, T4}.Throws(Exception)" />
	public IVoidMethodSetupReturnBuilder<T1, T2, T3, T4> Throws(Exception exception)
	{
		Callback<Action<int, T1, T2, T3, T4>> currentCallback = new((_, _, _, _, _) => throw exception);
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetup{T1, T2, T3, T4}.Throws(Func{Exception})" />
	public IVoidMethodSetupReturnBuilder<T1, T2, T3, T4> Throws(Func<Exception> callback)
	{
		Callback<Action<int, T1, T2, T3, T4>> currentCallback = new((_, _, _, _, _) => throw callback());
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetup{T1, T2, T3, T4}.Throws(Func{T1, T2, T3, T4, Exception})" />
	public IVoidMethodSetupReturnBuilder<T1, T2, T3, T4> Throws(Func<T1, T2, T3, T4, Exception> callback)
	{
		Callback<Action<int, T1, T2, T3, T4>> currentCallback =
			new((_, p1, p2, p3, p4) => throw callback(p1, p2, p3, p4));
		_currentReturnCallback = currentCallback;
		_returnCallbacks.Add(currentCallback);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetupCallbackBuilder{T1, T2, T3, T4}.InParallel()" />
	IVoidMethodSetupCallbackBuilder<T1, T2, T3, T4> IVoidMethodSetupCallbackBuilder<T1, T2, T3, T4>.InParallel()
	{
		_currentCallback?.InParallel();
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetupCallbackBuilder{T1, T2, T3, T4}.When(Func{int, bool})" />
	IVoidMethodSetupCallbackWhenBuilder<T1, T2, T3, T4> IVoidMethodSetupCallbackBuilder<T1, T2, T3, T4>.When(
		Func<int, bool> predicate)
	{
		_currentCallback?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetupCallbackWhenBuilder{T1, T2, T3, T4}.For(int)" />
	IVoidMethodSetupCallbackWhenBuilder<T1, T2, T3, T4> IVoidMethodSetupCallbackWhenBuilder<T1, T2, T3, T4>.
		For(int times)
	{
		_currentCallback?.For(times);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetupCallbackWhenBuilder{T1, T2, T3, T4}.Only(int)" />
	IVoidMethodSetup<T1, T2, T3, T4> IVoidMethodSetupCallbackWhenBuilder<T1, T2, T3, T4>.Only(int times)
	{
		_currentCallback?.Only(times);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetupReturnBuilder{T1, T2, T3, T4}.When(Func{int, bool})" />
	IVoidMethodSetupReturnWhenBuilder<T1, T2, T3, T4> IVoidMethodSetupReturnBuilder<T1, T2, T3, T4>.When(
		Func<int, bool> predicate)
	{
		_currentReturnCallback?.When(predicate);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetupReturnWhenBuilder{T1, T2, T3, T4}.For(int)" />
	IVoidMethodSetupReturnWhenBuilder<T1, T2, T3, T4> IVoidMethodSetupReturnWhenBuilder<T1, T2, T3, T4>.For(int times)
	{
		_currentReturnCallback?.For(times);
		return this;
	}

	/// <inheritdoc cref="IVoidMethodSetupReturnWhenBuilder{T1, T2, T3, T4}.Only(int)" />
	IVoidMethodSetup<T1, T2, T3, T4> IVoidMethodSetupReturnWhenBuilder<T1, T2, T3, T4>.Only(int times)
	{
		_currentReturnCallback?.Only(times);
		return this;
	}

	/// <inheritdoc cref="MethodSetup.ExecuteCallback(MethodInvocation, MockBehavior)" />
	protected override void ExecuteCallback(MethodInvocation invocation, MockBehavior behavior)
	{
		if (TryCast(invocation.Parameters[0].Value, out T1 p1, behavior) &&
		    TryCast(invocation.Parameters[1].Value, out T2 p2, behavior) &&
		    TryCast(invocation.Parameters[2].Value, out T3 p3, behavior) &&
		    TryCast(invocation.Parameters[3].Value, out T4 p4, behavior))
		{
			bool wasInvoked = false;
			int currentCallbacksIndex = _currentCallbacksIndex;
			for (int i = 0; i < _callbacks.Count; i++)
			{
				Callback<Action<int, T1, T2, T3, T4>> callback =
					_callbacks[(currentCallbacksIndex + i) % _callbacks.Count];
				if (callback.Invoke(wasInvoked, ref _currentCallbacksIndex, (invocationCount, @delegate)
					    => @delegate(invocationCount, p1, p2, p3, p4)))
				{
					wasInvoked = true;
				}
			}

			foreach (Callback<Action<int, T1, T2, T3, T4>> _ in _returnCallbacks)
			{
				Callback<Action<int, T1, T2, T3, T4>> returnCallback =
					_returnCallbacks[_currentReturnCallbackIndex % _returnCallbacks.Count];
				if (returnCallback.Invoke(ref _currentReturnCallbackIndex, (invocationCount, @delegate)
					    => @delegate(invocationCount, p1, p2, p3, p4)))
				{
					return;
				}
			}
		}
	}

	/// <inheritdoc cref="MethodSetup.GetReturnValue{TResult}(MethodInvocation, MockBehavior, Func{TResult})" />
	protected override TResult GetReturnValue<TResult>(MethodInvocation invocation, MockBehavior behavior,
		Func<TResult> defaultValueGenerator)
		where TResult : default
		=> throw new MockException("The method setup does not support return values.");

	/// <inheritdoc cref="MethodSetup.TriggerParameterCallbacks(object?[])" />
	protected override void TriggerParameterCallbacks(object?[] parameters)
		=> TriggerCallbacks([_match1, _match2, _match3, _match4,], parameters);

	/// <inheritdoc cref="MethodSetup.GetSkipBaseClass()" />
	protected override bool? GetSkipBaseClass()
		=> _skipBaseClass;

	/// <inheritdoc cref="MethodSetup.SetOutParameter{T}(string, Func{T})" />
	protected override T SetOutParameter<T>(string parameterName, Func<T> defaultValueGenerator)
	{
		if (HasOutParameter([_match1, _match2, _match3, _match4,], parameterName,
			    out IOutParameter<T>? outParameter))
		{
			return outParameter.GetValue(defaultValueGenerator);
		}

		return defaultValueGenerator();
	}

	/// <inheritdoc cref="MethodSetup.SetRefParameter{T}(string, T, MockBehavior)" />
	protected override T SetRefParameter<T>(string parameterName, T value, MockBehavior behavior)
	{
		if (HasRefParameter([_match1, _match2, _match3, _match4,], parameterName,
			    out IRefParameter<T>? refParameter))
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
