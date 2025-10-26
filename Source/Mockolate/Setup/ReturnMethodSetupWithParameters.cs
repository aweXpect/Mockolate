using System;
using System.Collections.Generic;
using System.Threading;
using Mockolate.Exceptions;
using Mockolate.Interactions;

namespace Mockolate.Setup;

/// <summary>
///     Sets up a method with parameters returning <typeparamref name="TReturn" />.
/// </summary>
public class ReturnMethodSetupWithParameters<TReturn>(string name, With.Parameters match) : MethodSetup
{
	private readonly List<Action> _callbacks = [];
	private readonly List<Func<TReturn>> _returnCallbacks = [];
	private int _currentReturnCallbackIndex = -1;

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	public ReturnMethodSetupWithParameters<TReturn> Callback(Action callback)
	{
		_callbacks.Add(callback);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public ReturnMethodSetupWithParameters<TReturn> Returns(Func<TReturn> callback)
	{
		_returnCallbacks.Add(callback);
		return this;
	}

	/// <summary>
	///     Registers the <paramref name="returnValue" /> for this method.
	/// </summary>
	public ReturnMethodSetupWithParameters<TReturn> Returns(TReturn returnValue)
	{
		_returnCallbacks.Add(() => returnValue);
		return this;
	}

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the method is invoked.
	/// </summary>
	public ReturnMethodSetupWithParameters<TReturn> Throws(Exception exception)
	{
		_returnCallbacks.Add(() => throw exception);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	public ReturnMethodSetupWithParameters<TReturn> Throws(Func<Exception> callback)
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
		=> invocation.Name.Equals(name) && match.Matches(invocation.Parameters);

	/// <inheritdoc cref="MethodSetup.SetOutParameter{T}(string, MockBehavior)" />
	protected override T SetOutParameter<T>(string parameterName, MockBehavior behavior)
		=> behavior.DefaultValue.Generate<T>();

	/// <inheritdoc cref="MethodSetup.SetRefParameter{T}(string, T, MockBehavior)" />
	protected override T SetRefParameter<T>(string parameterName, T value, MockBehavior behavior)
		=> value;

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
	{
		return $"{FormatType(typeof(TReturn))} {name}({match})";
	}
}
#pragma warning restore S2436 // Types and methods should not have too many generic parameters
