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
#endif
