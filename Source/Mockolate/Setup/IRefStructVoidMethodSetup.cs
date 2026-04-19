#if NET9_0_OR_GREATER
using System;

namespace Mockolate.Setup;

/// <summary>
///     Sets up a method returning <see langword="void" /> with a single ref struct parameter of
///     type <typeparamref name="T" />.
/// </summary>
/// <remarks>
///     <para>
///         The ref-struct-safe counterpart to <see cref="IVoidMethodSetup{T}" />. The surface is
///         deliberately narrower: only throws and base-class-skip are supported, because
///         <c>Action&lt;T&gt;</c>, <c>Func&lt;T, Exception&gt;</c>, and the
///         <c>CallbackBuilder → ParallelCallbackBuilder → CallbackWhenBuilder</c> chain all require
///         a <typeparamref name="T" /> that can flow through a delegate type parameter — which is
///         impossible for a ref struct.
///     </para>
///     <para>
///         Use <see cref="It.IsAnyRefStruct{T}" /> or <see cref="It.IsRefStruct{T}" /> to supply
///         the matcher at setup time.
///     </para>
/// </remarks>
public interface IRefStructVoidMethodSetup<T> : IMethodSetup
	where T : allows ref struct
{
	/// <summary>
	///     Specifies if calling the base class implementation should be skipped.
	/// </summary>
	/// <remarks>
	///     If set to <see langword="false" /> (the default), the base class implementation gets
	///     called and its effects take place. If not specified explicitly, the mock's
	///     <see cref="MockBehavior.SkipBaseClass" /> is used.
	/// </remarks>
	IRefStructVoidMethodSetup<T> SkippingBaseClass(bool skipBaseClass = true);

	/// <summary>
	///     Configures the setup not to throw when the method is invoked.
	/// </summary>
	IRefStructVoidMethodSetup<T> DoesNotThrow();

	/// <summary>
	///     Configures a new <typeparamref name="TException" /> to be thrown when the method is
	///     invoked with a matching parameter.
	/// </summary>
	IRefStructVoidMethodSetup<T> Throws<TException>()
		where TException : Exception, new();

	/// <summary>
	///     Configures the given <paramref name="exception" /> to be thrown when the method is
	///     invoked with a matching parameter.
	/// </summary>
	IRefStructVoidMethodSetup<T> Throws(Exception exception);

	/// <summary>
	///     Configures <paramref name="exceptionFactory" /> to be invoked when the method is invoked
	///     with a matching parameter; the returned exception is thrown.
	/// </summary>
	IRefStructVoidMethodSetup<T> Throws(Func<Exception> exceptionFactory);
}

/// <summary>
///     Sets up a method returning <see langword="void" /> with two parameters of which at least
///     one is a ref struct. See <see cref="IRefStructVoidMethodSetup{T}" /> for design rationale.
/// </summary>
public interface IRefStructVoidMethodSetup<T1, T2> : IMethodSetup
	where T1 : allows ref struct
	where T2 : allows ref struct
{
	/// <inheritdoc cref="IRefStructVoidMethodSetup{T}.SkippingBaseClass(bool)" />
	IRefStructVoidMethodSetup<T1, T2> SkippingBaseClass(bool skipBaseClass = true);

	/// <inheritdoc cref="IRefStructVoidMethodSetup{T}.DoesNotThrow()" />
	IRefStructVoidMethodSetup<T1, T2> DoesNotThrow();

	/// <inheritdoc cref="IRefStructVoidMethodSetup{T}.Throws{TException}()" />
	IRefStructVoidMethodSetup<T1, T2> Throws<TException>()
		where TException : Exception, new();

	/// <inheritdoc cref="IRefStructVoidMethodSetup{T}.Throws(Exception)" />
	IRefStructVoidMethodSetup<T1, T2> Throws(Exception exception);

	/// <inheritdoc cref="IRefStructVoidMethodSetup{T}.Throws(Func{Exception})" />
	IRefStructVoidMethodSetup<T1, T2> Throws(Func<Exception> exceptionFactory);
}

/// <summary>
///     Sets up a method returning <see langword="void" /> with three parameters of which at least
///     one is a ref struct. See <see cref="IRefStructVoidMethodSetup{T}" /> for design rationale.
/// </summary>
public interface IRefStructVoidMethodSetup<T1, T2, T3> : IMethodSetup
	where T1 : allows ref struct
	where T2 : allows ref struct
	where T3 : allows ref struct
{
	/// <inheritdoc cref="IRefStructVoidMethodSetup{T}.SkippingBaseClass(bool)" />
	IRefStructVoidMethodSetup<T1, T2, T3> SkippingBaseClass(bool skipBaseClass = true);

	/// <inheritdoc cref="IRefStructVoidMethodSetup{T}.DoesNotThrow()" />
	IRefStructVoidMethodSetup<T1, T2, T3> DoesNotThrow();

	/// <inheritdoc cref="IRefStructVoidMethodSetup{T}.Throws{TException}()" />
	IRefStructVoidMethodSetup<T1, T2, T3> Throws<TException>()
		where TException : Exception, new();

	/// <inheritdoc cref="IRefStructVoidMethodSetup{T}.Throws(Exception)" />
	IRefStructVoidMethodSetup<T1, T2, T3> Throws(Exception exception);

	/// <inheritdoc cref="IRefStructVoidMethodSetup{T}.Throws(Func{Exception})" />
	IRefStructVoidMethodSetup<T1, T2, T3> Throws(Func<Exception> exceptionFactory);
}

/// <summary>
///     Sets up a method returning <see langword="void" /> with four parameters of which at least
///     one is a ref struct. See <see cref="IRefStructVoidMethodSetup{T}" /> for design rationale.
/// </summary>
public interface IRefStructVoidMethodSetup<T1, T2, T3, T4> : IMethodSetup
	where T1 : allows ref struct
	where T2 : allows ref struct
	where T3 : allows ref struct
	where T4 : allows ref struct
{
	/// <inheritdoc cref="IRefStructVoidMethodSetup{T}.SkippingBaseClass(bool)" />
	IRefStructVoidMethodSetup<T1, T2, T3, T4> SkippingBaseClass(bool skipBaseClass = true);

	/// <inheritdoc cref="IRefStructVoidMethodSetup{T}.DoesNotThrow()" />
	IRefStructVoidMethodSetup<T1, T2, T3, T4> DoesNotThrow();

	/// <inheritdoc cref="IRefStructVoidMethodSetup{T}.Throws{TException}()" />
	IRefStructVoidMethodSetup<T1, T2, T3, T4> Throws<TException>()
		where TException : Exception, new();

	/// <inheritdoc cref="IRefStructVoidMethodSetup{T}.Throws(Exception)" />
	IRefStructVoidMethodSetup<T1, T2, T3, T4> Throws(Exception exception);

	/// <inheritdoc cref="IRefStructVoidMethodSetup{T}.Throws(Func{Exception})" />
	IRefStructVoidMethodSetup<T1, T2, T3, T4> Throws(Func<Exception> exceptionFactory);
}
#endif
