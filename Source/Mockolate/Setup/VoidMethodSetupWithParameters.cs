using System;
using System.Collections.Generic;
using System.Threading;
using Mockolate.Exceptions;
using Mockolate.Interactions;

namespace Mockolate.Setup;

/// <summary>
///     Setup for a method with parameters returning <see langword="void" />.
/// </summary>
public class VoidMethodSetupWithParameters(string name, With.Parameters match) : MethodSetup
{
	private readonly List<Action> _callbacks = [];
	private readonly List<Action> _returnCallbacks = [];
	private int _currentReturnCallbackIndex = -1;

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute when the method is called.
	/// </summary>
	public VoidMethodSetupWithParameters Callback(Action callback)
	{
		_callbacks.Add(() => callback());
		return this;
	}

	/// <summary>
	///     Registers an iteration in the sequence of method invocations, that does not throw.
	/// </summary>
	public VoidMethodSetupWithParameters DoesNotThrow()
	{
		_returnCallbacks.Add(() => { });
		return this;
	}

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the method is invoked.
	/// </summary>
	public VoidMethodSetupWithParameters Throws(Exception exception)
	{
		_returnCallbacks.Add(() => throw exception);
		return this;
	}

	/// <summary>
	///     Registers a <paramref name="callback" /> that will calculate the exception to throw when the method is invoked.
	/// </summary>
	public VoidMethodSetupWithParameters Throws(Func<Exception> callback)
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
		=> invocation.Name.Equals(name) && match.Matches(invocation.Parameters);

	/// <inheritdoc cref="MethodSetup.SetOutParameter{T}(string, MockBehavior)" />
	protected override T SetOutParameter<T>(string parameterName, MockBehavior behavior)
		=> throw new MockException("The method setup with parameters does not support out parameters.");

	/// <inheritdoc cref="MethodSetup.SetRefParameter{T}(string, T, MockBehavior)" />
	protected override T SetRefParameter<T>(string parameterName, T value, MockBehavior behavior)
		=> throw new MockException("The method setup with parameters does not support ref parameters.");

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
	{
		return $"void {name}({match})";
	}
}
