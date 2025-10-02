using System;
using System.Threading.Tasks;
using Mockerade.Setup;

namespace Mockerade;

/// <summary>
///     Extensions for setting up asynchronous return values.
/// </summary>
public static class ReturnsAsyncExtensions
{
	/// <summary>
	///     Registers the <see langword="async" /> <paramref name="returnValue" /> for this method.
	/// </summary>
	public static ReturnMethodSetup<Task<TReturn>> ReturnsAsync<TReturn>(this ReturnMethodSetup<Task<TReturn>> setup, TReturn returnValue)
		=> setup.Returns(Task.FromResult(returnValue));

	/// <summary>
	///     Registers an <see langword="async" /> <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public static ReturnMethodSetup<Task<TReturn>> ReturnsAsync<TReturn>(this ReturnMethodSetup<Task<TReturn>> setup, Func<TReturn> callback)
		=> setup.Returns(() => Task.FromResult(callback()));

	/// <summary>
	///     Registers the <see langword="async" /> <paramref name="returnValue" /> for this method.
	/// </summary>
	public static ReturnMethodSetup<Task<TReturn>, T1> ReturnsAsync<TReturn, T1>(this ReturnMethodSetup<Task<TReturn>, T1> setup, TReturn returnValue)
		=> setup.Returns(Task.FromResult(returnValue));

	/// <summary>
	///     Registers an <see langword="async" /> <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public static ReturnMethodSetup<Task<TReturn>, T1> ReturnsAsync<TReturn, T1>(this ReturnMethodSetup<Task<TReturn>, T1> setup, Func<TReturn> callback)
		=> setup.Returns(() => Task.FromResult(callback()));

	/// <summary>
	///     Registers an <see langword="async" /> <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public static ReturnMethodSetup<Task<TReturn>, T1> ReturnsAsync<TReturn, T1>(this ReturnMethodSetup<Task<TReturn>, T1> setup, Func<T1, TReturn> callback)
		=> setup.Returns(v1 => Task.FromResult(callback(v1)));

	/// <summary>
	///     Registers the <see langword="async" /> <paramref name="returnValue" /> for this method.
	/// </summary>
	public static ReturnMethodSetup<Task<TReturn>, T1, T2> ReturnsAsync<TReturn, T1, T2>(this ReturnMethodSetup<Task<TReturn>, T1, T2> setup, TReturn returnValue)
		=> setup.Returns(Task.FromResult(returnValue));

	/// <summary>
	///     Registers an <see langword="async" /> <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public static ReturnMethodSetup<Task<TReturn>, T1, T2> ReturnsAsync<TReturn, T1, T2>(this ReturnMethodSetup<Task<TReturn>, T1, T2> setup, Func<TReturn> callback)
		=> setup.Returns(() => Task.FromResult(callback()));

	/// <summary>
	///     Registers an <see langword="async" /> <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public static ReturnMethodSetup<Task<TReturn>, T1, T2> ReturnsAsync<TReturn, T1, T2>(this ReturnMethodSetup<Task<TReturn>, T1, T2> setup, Func<T1, T2, TReturn> callback)
		=> setup.Returns((v1, v2) => Task.FromResult(callback(v1, v2)));

	/// <summary>
	///     Registers the <see langword="async" /> <paramref name="returnValue" /> for this method.
	/// </summary>
	public static ReturnMethodSetup<Task<TReturn>, T1, T2, T3> ReturnsAsync<TReturn, T1, T2, T3>(this ReturnMethodSetup<Task<TReturn>, T1, T2, T3> setup, TReturn returnValue)
		=> setup.Returns(Task.FromResult(returnValue));

	/// <summary>
	///     Registers an <see langword="async" /> <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public static ReturnMethodSetup<Task<TReturn>, T1, T2, T3> ReturnsAsync<TReturn, T1, T2, T3>(this ReturnMethodSetup<Task<TReturn>, T1, T2, T3> setup, Func<TReturn> callback)
		=> setup.Returns(() => Task.FromResult(callback()));

	/// <summary>
	///     Registers an <see langword="async" /> <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public static ReturnMethodSetup<Task<TReturn>, T1, T2, T3> ReturnsAsync<TReturn, T1, T2, T3>(this ReturnMethodSetup<Task<TReturn>, T1, T2, T3> setup, Func<T1, T2, T3, TReturn> callback)
		=> setup.Returns((v1, v2, v3) => Task.FromResult(callback(v1, v2, v3)));

	/// <summary>
	///     Registers the <see langword="async" /> <paramref name="returnValue" /> for this method.
	/// </summary>
	public static ReturnMethodSetup<Task<TReturn>, T1, T2, T3, T4> ReturnsAsync<TReturn, T1, T2, T3, T4>(this ReturnMethodSetup<Task<TReturn>, T1, T2, T3, T4> setup, TReturn returnValue)
		=> setup.Returns(Task.FromResult(returnValue));

	/// <summary>
	///     Registers an <see langword="async" /> <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public static ReturnMethodSetup<Task<TReturn>, T1, T2, T3, T4> ReturnsAsync<TReturn, T1, T2, T3, T4>(this ReturnMethodSetup<Task<TReturn>, T1, T2, T3, T4> setup, Func<TReturn> callback)
		=> setup.Returns(() => Task.FromResult(callback()));

	/// <summary>
	///     Registers an <see langword="async" /> <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public static ReturnMethodSetup<Task<TReturn>, T1, T2, T3, T4> ReturnsAsync<TReturn, T1, T2, T3, T4>(this ReturnMethodSetup<Task<TReturn>, T1, T2, T3, T4> setup, Func<T1, T2, T3, T4, TReturn> callback)
		=> setup.Returns((v1, v2, v3, v4) => Task.FromResult(callback(v1, v2, v3, v4)));

#if NET8_0_OR_GREATER
	/// <summary>
	///     Registers the <see langword="async" /> <paramref name="returnValue" /> for this method.
	/// </summary>
	public static ReturnMethodSetup<ValueTask<TReturn>> ReturnsAsync<TReturn>(this ReturnMethodSetup<ValueTask<TReturn>> setup, TReturn returnValue)
		=> setup.Returns(ValueTask.FromResult(returnValue));

	/// <summary>
	///     Registers an <see langword="async" /> <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public static ReturnMethodSetup<ValueTask<TReturn>> ReturnsAsync<TReturn>(this ReturnMethodSetup<ValueTask<TReturn>> setup, Func<TReturn> callback)
		=> setup.Returns(() => ValueTask.FromResult(callback()));

	/// <summary>
	///     Registers the <see langword="async" /> <paramref name="returnValue" /> for this method.
	/// </summary>
	public static ReturnMethodSetup<ValueTask<TReturn>, T1> ReturnsAsync<TReturn, T1>(this ReturnMethodSetup<ValueTask<TReturn>, T1> setup, TReturn returnValue)
		=> setup.Returns(ValueTask.FromResult(returnValue));

	/// <summary>
	///     Registers an <see langword="async" /> <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public static ReturnMethodSetup<ValueTask<TReturn>, T1> ReturnsAsync<TReturn, T1>(this ReturnMethodSetup<ValueTask<TReturn>, T1> setup, Func<TReturn> callback)
		=> setup.Returns(() => ValueTask.FromResult(callback()));

	/// <summary>
	///     Registers an <see langword="async" /> <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public static ReturnMethodSetup<ValueTask<TReturn>, T1> ReturnsAsync<TReturn, T1>(this ReturnMethodSetup<ValueTask<TReturn>, T1> setup, Func<T1, TReturn> callback)
		=> setup.Returns(v1 => ValueTask.FromResult(callback(v1)));

	/// <summary>
	///     Registers the <see langword="async" /> <paramref name="returnValue" /> for this method.
	/// </summary>
	public static ReturnMethodSetup<ValueTask<TReturn>, T1, T2> ReturnsAsync<TReturn, T1, T2>(this ReturnMethodSetup<ValueTask<TReturn>, T1, T2> setup, TReturn returnValue)
		=> setup.Returns(ValueTask.FromResult(returnValue));

	/// <summary>
	///     Registers an <see langword="async" /> <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public static ReturnMethodSetup<ValueTask<TReturn>, T1, T2> ReturnsAsync<TReturn, T1, T2>(this ReturnMethodSetup<ValueTask<TReturn>, T1, T2> setup, Func<TReturn> callback)
		=> setup.Returns(() => ValueTask.FromResult(callback()));

	/// <summary>
	///     Registers an <see langword="async" /> <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public static ReturnMethodSetup<ValueTask<TReturn>, T1, T2> ReturnsAsync<TReturn, T1, T2>(this ReturnMethodSetup<ValueTask<TReturn>, T1, T2> setup, Func<T1, T2, TReturn> callback)
		=> setup.Returns((v1, v2) => ValueTask.FromResult(callback(v1, v2)));

	/// <summary>
	///     Registers the <see langword="async" /> <paramref name="returnValue" /> for this method.
	/// </summary>
	public static ReturnMethodSetup<ValueTask<TReturn>, T1, T2, T3> ReturnsAsync<TReturn, T1, T2, T3>(this ReturnMethodSetup<ValueTask<TReturn>, T1, T2, T3> setup, TReturn returnValue)
		=> setup.Returns(ValueTask.FromResult(returnValue));

	/// <summary>
	///     Registers an <see langword="async" /> <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public static ReturnMethodSetup<ValueTask<TReturn>, T1, T2, T3> ReturnsAsync<TReturn, T1, T2, T3>(this ReturnMethodSetup<ValueTask<TReturn>, T1, T2, T3> setup, Func<TReturn> callback)
		=> setup.Returns(() => ValueTask.FromResult(callback()));

	/// <summary>
	///     Registers an <see langword="async" /> <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public static ReturnMethodSetup<ValueTask<TReturn>, T1, T2, T3> ReturnsAsync<TReturn, T1, T2, T3>(this ReturnMethodSetup<ValueTask<TReturn>, T1, T2, T3> setup, Func<T1, T2, T3, TReturn> callback)
		=> setup.Returns((v1, v2, v3) => ValueTask.FromResult(callback(v1, v2, v3)));

	/// <summary>
	///     Registers the <see langword="async" /> <paramref name="returnValue" /> for this method.
	/// </summary>
	public static ReturnMethodSetup<ValueTask<TReturn>, T1, T2, T3, T4> ReturnsAsync<TReturn, T1, T2, T3, T4>(this ReturnMethodSetup<ValueTask<TReturn>, T1, T2, T3, T4> setup, TReturn returnValue)
		=> setup.Returns(ValueTask.FromResult(returnValue));

	/// <summary>
	///     Registers an <see langword="async" /> <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public static ReturnMethodSetup<ValueTask<TReturn>, T1, T2, T3, T4> ReturnsAsync<TReturn, T1, T2, T3, T4>(this ReturnMethodSetup<ValueTask<TReturn>, T1, T2, T3, T4> setup, Func<TReturn> callback)
		=> setup.Returns(() => ValueTask.FromResult(callback()));

	/// <summary>
	///     Registers an <see langword="async" /> <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public static ReturnMethodSetup<ValueTask<TReturn>, T1, T2, T3, T4> ReturnsAsync<TReturn, T1, T2, T3, T4>(this ReturnMethodSetup<ValueTask<TReturn>, T1, T2, T3, T4> setup, Func<T1, T2, T3, T4, TReturn> callback)
		=> setup.Returns((v1, v2, v3, v4) => ValueTask.FromResult(callback(v1, v2, v3, v4)));
#endif
}
