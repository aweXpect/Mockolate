using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Mockolate.Setup;

namespace Mockolate;

/// <summary>
///     Extensions for setting up asynchronous return values.
/// </summary>
public static class ReturnsAsyncExtensions
{
#pragma warning disable S2436 // Types and methods should not have too many generic parameters
	/// <summary>
	///     Registers the <see langword="async" /> <paramref name="returnValue" /> for this method.
	/// </summary>
	public static ReturnMethodSetup<Task<TReturn>> ReturnsAsync<TReturn>(this ReturnMethodSetup<Task<TReturn>> setup,
		TReturn returnValue)
		=> setup.Returns(Task.FromResult(returnValue));

	/// <summary>
	///     Registers an <see langword="async" /> <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public static ReturnMethodSetup<Task<TReturn>> ReturnsAsync<TReturn>(this ReturnMethodSetup<Task<TReturn>> setup,
		Func<TReturn> callback)
		=> setup.Returns(() => Task.FromResult(callback()));

	/// <summary>
	///     Registers the <see langword="async" /> <paramref name="returnValue" /> for this method.
	/// </summary>
	public static ReturnMethodSetup<Task<TReturn>, T1> ReturnsAsync<TReturn, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T1>(
		this ReturnMethodSetup<Task<TReturn>, T1> setup, TReturn returnValue)
		=> setup.Returns(Task.FromResult(returnValue));

	/// <summary>
	///     Registers an <see langword="async" /> <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public static ReturnMethodSetup<Task<TReturn>, T1> ReturnsAsync<TReturn, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T1>(
		this ReturnMethodSetup<Task<TReturn>, T1> setup, Func<TReturn> callback)
		=> setup.Returns(() => Task.FromResult(callback()));

	/// <summary>
	///     Registers an <see langword="async" /> <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public static ReturnMethodSetup<Task<TReturn>, T1> ReturnsAsync<TReturn, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T1>(
		this ReturnMethodSetup<Task<TReturn>, T1> setup, Func<T1, TReturn> callback)
		=> setup.Returns(v1 => Task.FromResult(callback(v1)));

	/// <summary>
	///     Registers the <see langword="async" /> <paramref name="returnValue" /> for this method.
	/// </summary>
	public static ReturnMethodSetup<Task<TReturn>, T1, T2> ReturnsAsync<TReturn, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T1, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T2>(
		this ReturnMethodSetup<Task<TReturn>, T1, T2> setup, TReturn returnValue)
		=> setup.Returns(Task.FromResult(returnValue));

	/// <summary>
	///     Registers an <see langword="async" /> <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public static ReturnMethodSetup<Task<TReturn>, T1, T2> ReturnsAsync<TReturn, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T1, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T2>(
		this ReturnMethodSetup<Task<TReturn>, T1, T2> setup, Func<TReturn> callback)
		=> setup.Returns(() => Task.FromResult(callback()));

	/// <summary>
	///     Registers an <see langword="async" /> <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public static ReturnMethodSetup<Task<TReturn>, T1, T2> ReturnsAsync<TReturn, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T1, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T2>(
		this ReturnMethodSetup<Task<TReturn>, T1, T2> setup, Func<T1, T2, TReturn> callback)
		=> setup.Returns((v1, v2) => Task.FromResult(callback(v1, v2)));

	/// <summary>
	///     Registers the <see langword="async" /> <paramref name="returnValue" /> for this method.
	/// </summary>
	public static ReturnMethodSetup<Task<TReturn>, T1, T2, T3> ReturnsAsync<TReturn, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T1, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T2, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T3>(
		this ReturnMethodSetup<Task<TReturn>, T1, T2, T3> setup, TReturn returnValue)
		=> setup.Returns(Task.FromResult(returnValue));

	/// <summary>
	///     Registers an <see langword="async" /> <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public static ReturnMethodSetup<Task<TReturn>, T1, T2, T3> ReturnsAsync<TReturn, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T1, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T2, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T3>(
		this ReturnMethodSetup<Task<TReturn>, T1, T2, T3> setup, Func<TReturn> callback)
		=> setup.Returns(() => Task.FromResult(callback()));

	/// <summary>
	///     Registers an <see langword="async" /> <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public static ReturnMethodSetup<Task<TReturn>, T1, T2, T3> ReturnsAsync<TReturn, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T1, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T2, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T3>(
		this ReturnMethodSetup<Task<TReturn>, T1, T2, T3> setup, Func<T1, T2, T3, TReturn> callback)
		=> setup.Returns((v1, v2, v3) => Task.FromResult(callback(v1, v2, v3)));

	/// <summary>
	///     Registers the <see langword="async" /> <paramref name="returnValue" /> for this method.
	/// </summary>
	public static ReturnMethodSetup<Task<TReturn>, T1, T2, T3, T4> ReturnsAsync<TReturn, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T1, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T2, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T3, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T4>(
		this ReturnMethodSetup<Task<TReturn>, T1, T2, T3, T4> setup, TReturn returnValue)
		=> setup.Returns(Task.FromResult(returnValue));

	/// <summary>
	///     Registers an <see langword="async" /> <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public static ReturnMethodSetup<Task<TReturn>, T1, T2, T3, T4> ReturnsAsync<TReturn, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T1, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T2, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T3, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T4>(
		this ReturnMethodSetup<Task<TReturn>, T1, T2, T3, T4> setup, Func<TReturn> callback)
		=> setup.Returns(() => Task.FromResult(callback()));

	/// <summary>
	///     Registers an <see langword="async" /> <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public static ReturnMethodSetup<Task<TReturn>, T1, T2, T3, T4> ReturnsAsync<TReturn, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T1, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T2, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T3, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T4>(
		this ReturnMethodSetup<Task<TReturn>, T1, T2, T3, T4> setup, Func<T1, T2, T3, T4, TReturn> callback)
		=> setup.Returns((v1, v2, v3, v4) => Task.FromResult(callback(v1, v2, v3, v4)));

#if NET8_0_OR_GREATER
#pragma warning disable CA2012 // Use ValueTasks correctly
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
	public static ReturnMethodSetup<ValueTask<TReturn>, T1> ReturnsAsync<TReturn, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T1>(this ReturnMethodSetup<ValueTask<TReturn>, T1> setup, TReturn returnValue)
		=> setup.Returns(ValueTask.FromResult(returnValue));

	/// <summary>
	///     Registers an <see langword="async" /> <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public static ReturnMethodSetup<ValueTask<TReturn>, T1> ReturnsAsync<TReturn, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T1>(this ReturnMethodSetup<ValueTask<TReturn>, T1> setup, Func<TReturn> callback)
		=> setup.Returns(() => ValueTask.FromResult(callback()));

	/// <summary>
	///     Registers an <see langword="async" /> <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public static ReturnMethodSetup<ValueTask<TReturn>, T1> ReturnsAsync<TReturn, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T1>(this ReturnMethodSetup<ValueTask<TReturn>, T1> setup, Func<T1, TReturn> callback)
		=> setup.Returns(v1 => ValueTask.FromResult(callback(v1)));

	/// <summary>
	///     Registers the <see langword="async" /> <paramref name="returnValue" /> for this method.
	/// </summary>
	public static ReturnMethodSetup<ValueTask<TReturn>, T1, T2> ReturnsAsync<TReturn, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T1, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T2>(this ReturnMethodSetup<ValueTask<TReturn>, T1, T2> setup, TReturn returnValue)
		=> setup.Returns(ValueTask.FromResult(returnValue));

	/// <summary>
	///     Registers an <see langword="async" /> <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public static ReturnMethodSetup<ValueTask<TReturn>, T1, T2> ReturnsAsync<TReturn, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T1, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T2>(this ReturnMethodSetup<ValueTask<TReturn>, T1, T2> setup, Func<TReturn> callback)
		=> setup.Returns(() => ValueTask.FromResult(callback()));

	/// <summary>
	///     Registers an <see langword="async" /> <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public static ReturnMethodSetup<ValueTask<TReturn>, T1, T2> ReturnsAsync<TReturn, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T1, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T2>(this ReturnMethodSetup<ValueTask<TReturn>, T1, T2> setup, Func<T1, T2, TReturn> callback)
		=> setup.Returns((v1, v2) => ValueTask.FromResult(callback(v1, v2)));

	/// <summary>
	///     Registers the <see langword="async" /> <paramref name="returnValue" /> for this method.
	/// </summary>
	public static ReturnMethodSetup<ValueTask<TReturn>, T1, T2, T3> ReturnsAsync<TReturn, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T1, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T2, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T3>(this ReturnMethodSetup<ValueTask<TReturn>, T1, T2, T3> setup, TReturn returnValue)
		=> setup.Returns(ValueTask.FromResult(returnValue));

	/// <summary>
	///     Registers an <see langword="async" /> <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public static ReturnMethodSetup<ValueTask<TReturn>, T1, T2, T3> ReturnsAsync<TReturn, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T1, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T2, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T3>(this ReturnMethodSetup<ValueTask<TReturn>, T1, T2, T3> setup, Func<TReturn> callback)
		=> setup.Returns(() => ValueTask.FromResult(callback()));

	/// <summary>
	///     Registers an <see langword="async" /> <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public static ReturnMethodSetup<ValueTask<TReturn>, T1, T2, T3> ReturnsAsync<TReturn, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T1, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T2, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T3>(this ReturnMethodSetup<ValueTask<TReturn>, T1, T2, T3> setup, Func<T1, T2, T3, TReturn> callback)
		=> setup.Returns((v1, v2, v3) => ValueTask.FromResult(callback(v1, v2, v3)));

	/// <summary>
	///     Registers the <see langword="async" /> <paramref name="returnValue" /> for this method.
	/// </summary>
	public static ReturnMethodSetup<ValueTask<TReturn>, T1, T2, T3, T4> ReturnsAsync<TReturn, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T1, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T2, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T3, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T4>(this ReturnMethodSetup<ValueTask<TReturn>, T1, T2, T3, T4> setup, TReturn returnValue)
		=> setup.Returns(ValueTask.FromResult(returnValue));

	/// <summary>
	///     Registers an <see langword="async" /> <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public static ReturnMethodSetup<ValueTask<TReturn>, T1, T2, T3, T4> ReturnsAsync<TReturn, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T1, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T2, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T3, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T4>(this ReturnMethodSetup<ValueTask<TReturn>, T1, T2, T3, T4> setup, Func<TReturn> callback)
		=> setup.Returns(() => ValueTask.FromResult(callback()));

	/// <summary>
	///     Registers an <see langword="async" /> <paramref name="callback" /> to setup the return value for this method.
	/// </summary>
	public static ReturnMethodSetup<ValueTask<TReturn>, T1, T2, T3, T4> ReturnsAsync<TReturn, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T1, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T2, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T3, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T4>(this ReturnMethodSetup<ValueTask<TReturn>, T1, T2, T3, T4> setup, Func<T1, T2, T3, T4, TReturn> callback)
		=> setup.Returns((v1, v2, v3, v4) => ValueTask.FromResult(callback(v1, v2, v3, v4)));
#pragma warning restore CA2012 // Use ValueTasks correctly
#endif
#pragma warning restore S2436 // Types and methods should not have too many generic parameters
}
