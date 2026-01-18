using System;
using System.Threading.Tasks;
using Mockolate.Setup;

namespace Mockolate;

#pragma warning disable S2436 // Types and methods should not have too many generic parameters
/// <summary>
///     Extensions for setting up return values and throwing exceptions for <see langword="async" /> methods.
/// </summary>
public static class ReturnsThrowsAsyncExtensions
{
	/// <summary>
	///     Registers the <see langword="async" /> <paramref name="returnValue" /> for this method.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<Task<TReturn>> ReturnsAsync<TReturn>(
		this IReturnMethodSetup<Task<TReturn>> setup,
		TReturn returnValue)
		=> setup.Returns(Task.FromResult(returnValue));

	/// <summary>
	///     Registers an <see langword="async" /> <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<Task<TReturn>> ReturnsAsync<TReturn>(
		this IReturnMethodSetup<Task<TReturn>> setup,
		Func<TReturn> callback)
		=> setup.Returns(() => Task.FromResult(callback()));

	/// <summary>
	///     Registers the <see langword="async" /> <paramref name="returnValue" /> for this method.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<Task<TReturn>, T1> ReturnsAsync<TReturn, T1>(
		this IReturnMethodSetup<Task<TReturn>, T1> setup, TReturn returnValue)
		=> setup.Returns(Task.FromResult(returnValue));

	/// <summary>
	///     Registers an <see langword="async" /> <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<Task<TReturn>, T1> ReturnsAsync<TReturn, T1>(
		this IReturnMethodSetup<Task<TReturn>, T1> setup, Func<TReturn> callback)
		=> setup.Returns(() => Task.FromResult(callback()));

	/// <summary>
	///     Registers an <see langword="async" /> <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<Task<TReturn>, T1> ReturnsAsync<TReturn, T1>(
		this IReturnMethodSetup<Task<TReturn>, T1> setup, Func<T1, TReturn> callback)
		=> setup.Returns(v1 => Task.FromResult(callback(v1)));

	/// <summary>
	///     Registers the <see langword="async" /> <paramref name="returnValue" /> for this method.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<Task<TReturn>, T1, T2> ReturnsAsync<TReturn, T1, T2>(
		this IReturnMethodSetup<Task<TReturn>, T1, T2> setup, TReturn returnValue)
		=> setup.Returns(Task.FromResult(returnValue));

	/// <summary>
	///     Registers an <see langword="async" /> <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<Task<TReturn>, T1, T2> ReturnsAsync<TReturn, T1, T2>(
		this IReturnMethodSetup<Task<TReturn>, T1, T2> setup, Func<TReturn> callback)
		=> setup.Returns(() => Task.FromResult(callback()));

	/// <summary>
	///     Registers an <see langword="async" /> <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<Task<TReturn>, T1, T2> ReturnsAsync<TReturn, T1, T2>(
		this IReturnMethodSetup<Task<TReturn>, T1, T2> setup, Func<T1, T2, TReturn> callback)
		=> setup.Returns((v1, v2) => Task.FromResult(callback(v1, v2)));

	/// <summary>
	///     Registers the <see langword="async" /> <paramref name="returnValue" /> for this method.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<Task<TReturn>, T1, T2, T3> ReturnsAsync<TReturn, T1, T2, T3>(
		this IReturnMethodSetup<Task<TReturn>, T1, T2, T3> setup, TReturn returnValue)
		=> setup.Returns(Task.FromResult(returnValue));

	/// <summary>
	///     Registers an <see langword="async" /> <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<Task<TReturn>, T1, T2, T3> ReturnsAsync<TReturn, T1, T2, T3>(
		this IReturnMethodSetup<Task<TReturn>, T1, T2, T3> setup, Func<TReturn> callback)
		=> setup.Returns(() => Task.FromResult(callback()));

	/// <summary>
	///     Registers an <see langword="async" /> <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<Task<TReturn>, T1, T2, T3> ReturnsAsync<TReturn, T1, T2, T3>(
		this IReturnMethodSetup<Task<TReturn>, T1, T2, T3> setup, Func<T1, T2, T3, TReturn> callback)
		=> setup.Returns((v1, v2, v3) => Task.FromResult(callback(v1, v2, v3)));

	/// <summary>
	///     Registers the <see langword="async" /> <paramref name="returnValue" /> for this method.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<Task<TReturn>, T1, T2, T3, T4> ReturnsAsync<TReturn, T1, T2, T3, T4>(
		this IReturnMethodSetup<Task<TReturn>, T1, T2, T3, T4> setup, TReturn returnValue)
		=> setup.Returns(Task.FromResult(returnValue));

	/// <summary>
	///     Registers an <see langword="async" /> <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<Task<TReturn>, T1, T2, T3, T4> ReturnsAsync<TReturn, T1, T2, T3, T4>(
		this IReturnMethodSetup<Task<TReturn>, T1, T2, T3, T4> setup, Func<TReturn> callback)
		=> setup.Returns(() => Task.FromResult(callback()));

	/// <summary>
	///     Registers an <see langword="async" /> <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<Task<TReturn>, T1, T2, T3, T4> ReturnsAsync<TReturn, T1, T2, T3, T4>(
		this IReturnMethodSetup<Task<TReturn>, T1, T2, T3, T4> setup, Func<T1, T2, T3, T4, TReturn> callback)
		=> setup.Returns((v1, v2, v3, v4) => Task.FromResult(callback(v1, v2, v3, v4)));

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the <see langword="async" /> method is awaited.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<Task<TReturn>> ThrowsAsync<TReturn>(
		this IReturnMethodSetup<Task<TReturn>> setup,
		Exception exception)
		=> setup.Returns(Task.FromException<TReturn>(exception));

	/// <summary>
	///     Registers a <paramref name="callback" /> that calculates the exception to throw when the <see langword="async" />
	///     method is awaited.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<Task<TReturn>> ThrowsAsync<TReturn>(
		this IReturnMethodSetup<Task<TReturn>> setup,
		Func<Exception> callback)
		=> setup.Returns(() => Task.FromException<TReturn>(callback()));

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the <see langword="async" /> method is awaited.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<Task<TReturn>, T1> ThrowsAsync<TReturn, T1>(
		this IReturnMethodSetup<Task<TReturn>, T1> setup,
		Exception exception)
		=> setup.Returns(Task.FromException<TReturn>(exception));

	/// <summary>
	///     Registers a <paramref name="callback" /> that calculates the exception to throw when the <see langword="async" />
	///     method is awaited.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<Task<TReturn>, T1> ThrowsAsync<TReturn, T1>(
		this IReturnMethodSetup<Task<TReturn>, T1> setup,
		Func<Exception> callback)
		=> setup.Returns(() => Task.FromException<TReturn>(callback()));

	/// <summary>
	///     Registers a <paramref name="callback" /> that calculates the exception to throw when the <see langword="async" />
	///     method is awaited.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<Task<TReturn>, T1> ThrowsAsync<TReturn, T1>(
		this IReturnMethodSetup<Task<TReturn>, T1> setup,
		Func<T1, Exception> callback)
		=> setup.Returns(p1 => Task.FromException<TReturn>(callback(p1)));

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the <see langword="async" /> method is awaited.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<Task<TReturn>, T1, T2> ThrowsAsync<TReturn, T1, T2>(
		this IReturnMethodSetup<Task<TReturn>, T1, T2> setup,
		Exception exception)
		=> setup.Returns(Task.FromException<TReturn>(exception));

	/// <summary>
	///     Registers a <paramref name="callback" /> that calculates the exception to throw when the <see langword="async" />
	///     method is awaited.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<Task<TReturn>, T1, T2> ThrowsAsync<TReturn, T1, T2>(
		this IReturnMethodSetup<Task<TReturn>, T1, T2> setup,
		Func<Exception> callback)
		=> setup.Returns(() => Task.FromException<TReturn>(callback()));

	/// <summary>
	///     Registers a <paramref name="callback" /> that calculates the exception to throw when the <see langword="async" />
	///     method is awaited.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<Task<TReturn>, T1, T2> ThrowsAsync<TReturn, T1, T2>(
		this IReturnMethodSetup<Task<TReturn>, T1, T2> setup,
		Func<T1, T2, Exception> callback)
		=> setup.Returns((p1, p2) => Task.FromException<TReturn>(callback(p1, p2)));

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the <see langword="async" /> method is awaited.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<Task<TReturn>, T1, T2, T3> ThrowsAsync<TReturn, T1, T2, T3>(
		this IReturnMethodSetup<Task<TReturn>, T1, T2, T3> setup,
		Exception exception)
		=> setup.Returns(Task.FromException<TReturn>(exception));

	/// <summary>
	///     Registers a <paramref name="callback" /> that calculates the exception to throw when the <see langword="async" />
	///     method is awaited.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<Task<TReturn>, T1, T2, T3> ThrowsAsync<TReturn, T1, T2, T3>(
		this IReturnMethodSetup<Task<TReturn>, T1, T2, T3> setup,
		Func<Exception> callback)
		=> setup.Returns(() => Task.FromException<TReturn>(callback()));

	/// <summary>
	///     Registers a <paramref name="callback" /> that calculates the exception to throw when the <see langword="async" />
	///     method is awaited.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<Task<TReturn>, T1, T2, T3> ThrowsAsync<TReturn, T1, T2, T3>(
		this IReturnMethodSetup<Task<TReturn>, T1, T2, T3> setup,
		Func<T1, T2, T3, Exception> callback)
		=> setup.Returns((p1, p2, p3) => Task.FromException<TReturn>(callback(p1, p2, p3)));

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the <see langword="async" /> method is awaited.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<Task<TReturn>, T1, T2, T3, T4> ThrowsAsync<TReturn, T1, T2, T3, T4>(
		this IReturnMethodSetup<Task<TReturn>, T1, T2, T3, T4> setup,
		Exception exception)
		=> setup.Returns(Task.FromException<TReturn>(exception));

	/// <summary>
	///     Registers a <paramref name="callback" /> that calculates the exception to throw when the <see langword="async" />
	///     method is awaited.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<Task<TReturn>, T1, T2, T3, T4> ThrowsAsync<TReturn, T1, T2, T3, T4>(
		this IReturnMethodSetup<Task<TReturn>, T1, T2, T3, T4> setup,
		Func<Exception> callback)
		=> setup.Returns(() => Task.FromException<TReturn>(callback()));

	/// <summary>
	///     Registers a <paramref name="callback" /> that calculates the exception to throw when the <see langword="async" />
	///     method is awaited.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<Task<TReturn>, T1, T2, T3, T4> ThrowsAsync<TReturn, T1, T2, T3, T4>(
		this IReturnMethodSetup<Task<TReturn>, T1, T2, T3, T4> setup,
		Func<T1, T2, T3, T4, Exception> callback)
		=> setup.Returns((p1, p2, p3, p4) => Task.FromException<TReturn>(callback(p1, p2, p3, p4)));

#if NET8_0_OR_GREATER
#pragma warning disable CA2012 // Use ValueTasks correctly
	/// <summary>
	///     Registers the <see langword="async" /> <paramref name="returnValue" /> for this method.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<ValueTask<TReturn>> ReturnsAsync<TReturn>(
		this IReturnMethodSetup<ValueTask<TReturn>> setup, TReturn returnValue)
		=> setup.Returns(ValueTask.FromResult(returnValue));

	/// <summary>
	///     Registers an <see langword="async" /> <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<ValueTask<TReturn>> ReturnsAsync<TReturn>(
		this IReturnMethodSetup<ValueTask<TReturn>> setup, Func<TReturn> callback)
		=> setup.Returns(() => ValueTask.FromResult(callback()));

	/// <summary>
	///     Registers the <see langword="async" /> <paramref name="returnValue" /> for this method.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<ValueTask<TReturn>, T1> ReturnsAsync<TReturn, T1>(
		this IReturnMethodSetup<ValueTask<TReturn>, T1> setup, TReturn returnValue)
		=> setup.Returns(ValueTask.FromResult(returnValue));

	/// <summary>
	///     Registers an <see langword="async" /> <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<ValueTask<TReturn>, T1> ReturnsAsync<TReturn, T1>(
		this IReturnMethodSetup<ValueTask<TReturn>, T1> setup, Func<TReturn> callback)
		=> setup.Returns(() => ValueTask.FromResult(callback()));

	/// <summary>
	///     Registers an <see langword="async" /> <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<ValueTask<TReturn>, T1> ReturnsAsync<TReturn, T1>(
		this IReturnMethodSetup<ValueTask<TReturn>, T1> setup, Func<T1, TReturn> callback)
		=> setup.Returns(v1 => ValueTask.FromResult(callback(v1)));

	/// <summary>
	///     Registers the <see langword="async" /> <paramref name="returnValue" /> for this method.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<ValueTask<TReturn>, T1, T2> ReturnsAsync<TReturn, T1, T2>(
		this IReturnMethodSetup<ValueTask<TReturn>, T1, T2> setup, TReturn returnValue)
		=> setup.Returns(ValueTask.FromResult(returnValue));

	/// <summary>
	///     Registers an <see langword="async" /> <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<ValueTask<TReturn>, T1, T2> ReturnsAsync<TReturn, T1, T2>(
		this IReturnMethodSetup<ValueTask<TReturn>, T1, T2> setup, Func<TReturn> callback)
		=> setup.Returns(() => ValueTask.FromResult(callback()));

	/// <summary>
	///     Registers an <see langword="async" /> <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<ValueTask<TReturn>, T1, T2> ReturnsAsync<TReturn, T1, T2>(
		this IReturnMethodSetup<ValueTask<TReturn>, T1, T2> setup, Func<T1, T2, TReturn> callback)
		=> setup.Returns((v1, v2) => ValueTask.FromResult(callback(v1, v2)));

	/// <summary>
	///     Registers the <see langword="async" /> <paramref name="returnValue" /> for this method.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<ValueTask<TReturn>, T1, T2, T3> ReturnsAsync<TReturn, T1, T2, T3>(
		this IReturnMethodSetup<ValueTask<TReturn>, T1, T2, T3> setup, TReturn returnValue)
		=> setup.Returns(ValueTask.FromResult(returnValue));

	/// <summary>
	///     Registers an <see langword="async" /> <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<ValueTask<TReturn>, T1, T2, T3> ReturnsAsync<TReturn, T1, T2, T3>(
		this IReturnMethodSetup<ValueTask<TReturn>, T1, T2, T3> setup, Func<TReturn> callback)
		=> setup.Returns(() => ValueTask.FromResult(callback()));

	/// <summary>
	///     Registers an <see langword="async" /> <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<ValueTask<TReturn>, T1, T2, T3> ReturnsAsync<TReturn, T1, T2, T3>(
		this IReturnMethodSetup<ValueTask<TReturn>, T1, T2, T3> setup, Func<T1, T2, T3, TReturn> callback)
		=> setup.Returns((v1, v2, v3) => ValueTask.FromResult(callback(v1, v2, v3)));

	/// <summary>
	///     Registers the <see langword="async" /> <paramref name="returnValue" /> for this method.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<ValueTask<TReturn>, T1, T2, T3, T4> ReturnsAsync<TReturn, T1, T2, T3,
		T4>(
		this IReturnMethodSetup<ValueTask<TReturn>, T1, T2, T3, T4> setup, TReturn returnValue)
		=> setup.Returns(ValueTask.FromResult(returnValue));

	/// <summary>
	///     Registers an <see langword="async" /> <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<ValueTask<TReturn>, T1, T2, T3, T4> ReturnsAsync<TReturn, T1, T2, T3,
		T4>(
		this IReturnMethodSetup<ValueTask<TReturn>, T1, T2, T3, T4> setup, Func<TReturn> callback)
		=> setup.Returns(() => ValueTask.FromResult(callback()));

	/// <summary>
	///     Registers an <see langword="async" /> <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<ValueTask<TReturn>, T1, T2, T3, T4> ReturnsAsync<TReturn, T1, T2, T3,
		T4>(
		this IReturnMethodSetup<ValueTask<TReturn>, T1, T2, T3, T4> setup, Func<T1, T2, T3, T4, TReturn> callback)
		=> setup.Returns((v1, v2, v3, v4) => ValueTask.FromResult(callback(v1, v2, v3, v4)));

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the <see langword="async" /> method is awaited.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<ValueTask<TReturn>> ThrowsAsync<TReturn>(
		this IReturnMethodSetup<ValueTask<TReturn>> setup,
		Exception exception)
		=> setup.Returns(ValueTask.FromException<TReturn>(exception));

	/// <summary>
	///     Registers a <paramref name="callback" /> that calculates the exception to throw when the <see langword="async" />
	///     method is awaited.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<ValueTask<TReturn>> ThrowsAsync<TReturn>(
		this IReturnMethodSetup<ValueTask<TReturn>> setup,
		Func<Exception> callback)
		=> setup.Returns(() => ValueTask.FromException<TReturn>(callback()));

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the <see langword="async" /> method is awaited.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<ValueTask<TReturn>, T1> ThrowsAsync<TReturn, T1>(
		this IReturnMethodSetup<ValueTask<TReturn>, T1> setup,
		Exception exception)
		=> setup.Returns(ValueTask.FromException<TReturn>(exception));

	/// <summary>
	///     Registers a <paramref name="callback" /> that calculates the exception to throw when the <see langword="async" />
	///     method is awaited.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<ValueTask<TReturn>, T1> ThrowsAsync<TReturn, T1>(
		this IReturnMethodSetup<ValueTask<TReturn>, T1> setup,
		Func<Exception> callback)
		=> setup.Returns(() => ValueTask.FromException<TReturn>(callback()));

	/// <summary>
	///     Registers a <paramref name="callback" /> that calculates the exception to throw when the <see langword="async" />
	///     method is awaited.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<ValueTask<TReturn>, T1> ThrowsAsync<TReturn, T1>(
		this IReturnMethodSetup<ValueTask<TReturn>, T1> setup,
		Func<T1, Exception> callback)
		=> setup.Returns(p1 => ValueTask.FromException<TReturn>(callback(p1)));

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the <see langword="async" /> method is awaited.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<ValueTask<TReturn>, T1, T2> ThrowsAsync<TReturn, T1, T2>(
		this IReturnMethodSetup<ValueTask<TReturn>, T1, T2> setup,
		Exception exception)
		=> setup.Returns(ValueTask.FromException<TReturn>(exception));

	/// <summary>
	///     Registers a <paramref name="callback" /> that calculates the exception to throw when the <see langword="async" />
	///     method is awaited.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<ValueTask<TReturn>, T1, T2> ThrowsAsync<TReturn, T1, T2>(
		this IReturnMethodSetup<ValueTask<TReturn>, T1, T2> setup,
		Func<Exception> callback)
		=> setup.Returns(() => ValueTask.FromException<TReturn>(callback()));

	/// <summary>
	///     Registers a <paramref name="callback" /> that calculates the exception to throw when the <see langword="async" />
	///     method is awaited.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<ValueTask<TReturn>, T1, T2> ThrowsAsync<TReturn, T1, T2>(
		this IReturnMethodSetup<ValueTask<TReturn>, T1, T2> setup,
		Func<T1, T2, Exception> callback)
		=> setup.Returns((p1, p2) => ValueTask.FromException<TReturn>(callback(p1, p2)));

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the <see langword="async" /> method is awaited.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<ValueTask<TReturn>, T1, T2, T3> ThrowsAsync<TReturn, T1, T2, T3>(
		this IReturnMethodSetup<ValueTask<TReturn>, T1, T2, T3> setup,
		Exception exception)
		=> setup.Returns(ValueTask.FromException<TReturn>(exception));

	/// <summary>
	///     Registers a <paramref name="callback" /> that calculates the exception to throw when the <see langword="async" />
	///     method is awaited.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<ValueTask<TReturn>, T1, T2, T3> ThrowsAsync<TReturn, T1, T2, T3>(
		this IReturnMethodSetup<ValueTask<TReturn>, T1, T2, T3> setup,
		Func<Exception> callback)
		=> setup.Returns(() => ValueTask.FromException<TReturn>(callback()));

	/// <summary>
	///     Registers a <paramref name="callback" /> that calculates the exception to throw when the <see langword="async" />
	///     method is awaited.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<ValueTask<TReturn>, T1, T2, T3> ThrowsAsync<TReturn, T1, T2, T3>(
		this IReturnMethodSetup<ValueTask<TReturn>, T1, T2, T3> setup,
		Func<T1, T2, T3, Exception> callback)
		=> setup.Returns((p1, p2, p3) => ValueTask.FromException<TReturn>(callback(p1, p2, p3)));

	/// <summary>
	///     Registers an <paramref name="exception" /> to throw when the <see langword="async" /> method is awaited.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<ValueTask<TReturn>, T1, T2, T3, T4>
		ThrowsAsync<TReturn, T1, T2, T3, T4>(this IReturnMethodSetup<ValueTask<TReturn>, T1, T2, T3, T4> setup,
			Exception exception)
		=> setup.Returns(ValueTask.FromException<TReturn>(exception));

	/// <summary>
	///     Registers a <paramref name="callback" /> that calculates the exception to throw when the <see langword="async" />
	///     method is awaited.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<ValueTask<TReturn>, T1, T2, T3, T4>
		ThrowsAsync<TReturn, T1, T2, T3, T4>(this IReturnMethodSetup<ValueTask<TReturn>, T1, T2, T3, T4> setup,
			Func<Exception> callback)
		=> setup.Returns(() => ValueTask.FromException<TReturn>(callback()));

	/// <summary>
	///     Registers a <paramref name="callback" /> that calculates the exception to throw when the <see langword="async" />
	///     method is awaited.
	/// </summary>
	public static IReturnMethodSetupReturnBuilder<ValueTask<TReturn>, T1, T2, T3, T4>
		ThrowsAsync<TReturn, T1, T2, T3, T4>(this IReturnMethodSetup<ValueTask<TReturn>, T1, T2, T3, T4> setup,
			Func<T1, T2, T3, T4, Exception> callback)
		=> setup.Returns((p1, p2, p3, p4) => ValueTask.FromException<TReturn>(callback(p1, p2, p3, p4)));
#pragma warning restore CA2012 // Use ValueTasks correctly
#endif
}
#pragma warning restore S2436 // Types and methods should not have too many generic parameters
