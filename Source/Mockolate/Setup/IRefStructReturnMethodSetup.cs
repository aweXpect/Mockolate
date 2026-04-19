#if NET9_0_OR_GREATER
using System;

namespace Mockolate.Setup;

/// <summary>
///     Sets up a method returning <typeparamref name="TReturn" /> with a single ref struct
///     parameter of type <typeparamref name="T" />.
/// </summary>
/// <remarks>
///     <para>
///         The ref-struct-safe counterpart to <see cref="IReturnMethodSetup{TReturn, T}" />. The
///         surface is deliberately narrower: only <c>Returns</c>, <c>Throws</c>, and
///         <c>SkippingBaseClass</c> are supported, because <c>Do(Action&lt;T&gt;)</c>,
///         <c>Func&lt;T, TReturn&gt;</c>, and the full builder chain all require a
///         <typeparamref name="T" /> that can flow through a delegate type parameter — impossible
///         for a ref struct.
///     </para>
///     <para>
///         <typeparamref name="TReturn" /> is not constrained with <c>allows ref struct</c>: the
///         setup stores <c>Func&lt;TReturn&gt;</c> for the return factory, which is illegal for
///         a ref struct <typeparamref name="TReturn" />. Ref-struct-returning methods are a
///         separate design scope.
///     </para>
/// </remarks>
public interface IRefStructReturnMethodSetup<TReturn, T> : IMethodSetup
	where T : allows ref struct
{
	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1}.SkippingBaseClass(bool)" />
	IRefStructReturnMethodSetup<TReturn, T> SkippingBaseClass(bool skipBaseClass = true);

	/// <summary>
	///     Registers the <paramref name="returnValue" /> to be returned when the method is
	///     invoked with a matching parameter.
	/// </summary>
	IRefStructReturnMethodSetup<TReturn, T> Returns(TReturn returnValue);

	/// <summary>
	///     Registers a <paramref name="returnFactory" /> whose return value is used when the
	///     method is invoked with a matching parameter.
	/// </summary>
	IRefStructReturnMethodSetup<TReturn, T> Returns(Func<TReturn> returnFactory);

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1}.Throws{TException}()" />
	IRefStructReturnMethodSetup<TReturn, T> Throws<TException>()
		where TException : Exception, new();

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1}.Throws(Exception)" />
	IRefStructReturnMethodSetup<TReturn, T> Throws(Exception exception);

	/// <inheritdoc cref="IReturnMethodSetup{TReturn, T1}.Throws(Func{Exception})" />
	IRefStructReturnMethodSetup<TReturn, T> Throws(Func<Exception> exceptionFactory);
}

/// <summary>
///     Sets up a method returning <typeparamref name="TReturn" /> with two parameters of which
///     at least one is a ref struct. See <see cref="IRefStructReturnMethodSetup{TReturn, T}" />.
/// </summary>
public interface IRefStructReturnMethodSetup<TReturn, T1, T2> : IMethodSetup
	where T1 : allows ref struct
	where T2 : allows ref struct
{
	/// <inheritdoc cref="IRefStructReturnMethodSetup{TReturn, T}.SkippingBaseClass(bool)" />
	IRefStructReturnMethodSetup<TReturn, T1, T2> SkippingBaseClass(bool skipBaseClass = true);

	/// <inheritdoc cref="IRefStructReturnMethodSetup{TReturn, T}.Returns(TReturn)" />
	IRefStructReturnMethodSetup<TReturn, T1, T2> Returns(TReturn returnValue);

	/// <inheritdoc cref="IRefStructReturnMethodSetup{TReturn, T}.Returns(Func{TReturn})" />
	IRefStructReturnMethodSetup<TReturn, T1, T2> Returns(Func<TReturn> returnFactory);

	/// <inheritdoc cref="IRefStructReturnMethodSetup{TReturn, T}.Throws{TException}()" />
	IRefStructReturnMethodSetup<TReturn, T1, T2> Throws<TException>()
		where TException : Exception, new();

	/// <inheritdoc cref="IRefStructReturnMethodSetup{TReturn, T}.Throws(Exception)" />
	IRefStructReturnMethodSetup<TReturn, T1, T2> Throws(Exception exception);

	/// <inheritdoc cref="IRefStructReturnMethodSetup{TReturn, T}.Throws(Func{Exception})" />
	IRefStructReturnMethodSetup<TReturn, T1, T2> Throws(Func<Exception> exceptionFactory);
}

/// <summary>
///     Sets up a method returning <typeparamref name="TReturn" /> with three parameters of which
///     at least one is a ref struct. See <see cref="IRefStructReturnMethodSetup{TReturn, T}" />.
/// </summary>
public interface IRefStructReturnMethodSetup<TReturn, T1, T2, T3> : IMethodSetup
	where T1 : allows ref struct
	where T2 : allows ref struct
	where T3 : allows ref struct
{
	/// <inheritdoc cref="IRefStructReturnMethodSetup{TReturn, T}.SkippingBaseClass(bool)" />
	IRefStructReturnMethodSetup<TReturn, T1, T2, T3> SkippingBaseClass(bool skipBaseClass = true);

	/// <inheritdoc cref="IRefStructReturnMethodSetup{TReturn, T}.Returns(TReturn)" />
	IRefStructReturnMethodSetup<TReturn, T1, T2, T3> Returns(TReturn returnValue);

	/// <inheritdoc cref="IRefStructReturnMethodSetup{TReturn, T}.Returns(Func{TReturn})" />
	IRefStructReturnMethodSetup<TReturn, T1, T2, T3> Returns(Func<TReturn> returnFactory);

	/// <inheritdoc cref="IRefStructReturnMethodSetup{TReturn, T}.Throws{TException}()" />
	IRefStructReturnMethodSetup<TReturn, T1, T2, T3> Throws<TException>()
		where TException : Exception, new();

	/// <inheritdoc cref="IRefStructReturnMethodSetup{TReturn, T}.Throws(Exception)" />
	IRefStructReturnMethodSetup<TReturn, T1, T2, T3> Throws(Exception exception);

	/// <inheritdoc cref="IRefStructReturnMethodSetup{TReturn, T}.Throws(Func{Exception})" />
	IRefStructReturnMethodSetup<TReturn, T1, T2, T3> Throws(Func<Exception> exceptionFactory);
}

/// <summary>
///     Sets up a method returning <typeparamref name="TReturn" /> with four parameters of which
///     at least one is a ref struct. See <see cref="IRefStructReturnMethodSetup{TReturn, T}" />.
/// </summary>
#pragma warning disable S2436 // Types and methods should not have too many generic parameters
public interface IRefStructReturnMethodSetup<TReturn, T1, T2, T3, T4> : IMethodSetup
	where T1 : allows ref struct
	where T2 : allows ref struct
	where T3 : allows ref struct
	where T4 : allows ref struct
{
	/// <inheritdoc cref="IRefStructReturnMethodSetup{TReturn, T}.SkippingBaseClass(bool)" />
	IRefStructReturnMethodSetup<TReturn, T1, T2, T3, T4> SkippingBaseClass(bool skipBaseClass = true);

	/// <inheritdoc cref="IRefStructReturnMethodSetup{TReturn, T}.Returns(TReturn)" />
	IRefStructReturnMethodSetup<TReturn, T1, T2, T3, T4> Returns(TReturn returnValue);

	/// <inheritdoc cref="IRefStructReturnMethodSetup{TReturn, T}.Returns(Func{TReturn})" />
	IRefStructReturnMethodSetup<TReturn, T1, T2, T3, T4> Returns(Func<TReturn> returnFactory);

	/// <inheritdoc cref="IRefStructReturnMethodSetup{TReturn, T}.Throws{TException}()" />
	IRefStructReturnMethodSetup<TReturn, T1, T2, T3, T4> Throws<TException>()
		where TException : Exception, new();

	/// <inheritdoc cref="IRefStructReturnMethodSetup{TReturn, T}.Throws(Exception)" />
	IRefStructReturnMethodSetup<TReturn, T1, T2, T3, T4> Throws(Exception exception);

	/// <inheritdoc cref="IRefStructReturnMethodSetup{TReturn, T}.Throws(Func{Exception})" />
	IRefStructReturnMethodSetup<TReturn, T1, T2, T3, T4> Throws(Func<Exception> exceptionFactory);
}
#pragma warning restore S2436 // Types and methods should not have too many generic parameters
#endif
