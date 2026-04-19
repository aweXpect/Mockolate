#if NET9_0_OR_GREATER
using System;

namespace Mockolate.Setup;

/// <summary>
///     Sets up the getter of an indexer returning <typeparamref name="TValue" /> with a single
///     ref struct index parameter of type <typeparamref name="T" />.
/// </summary>
/// <remarks>
///     <para>
///         The ref-struct-safe counterpart to <see cref="IIndexerGetterSetup{TValue, T}" />.
///         Surface mirrors <see cref="IRefStructReturnMethodSetup{TReturn, T}" />: the indexer
///         getter is structurally a method <c>TValue get_Item(T key)</c>, so the narrow surface
///         (<c>SkippingBaseClass</c> / <c>Returns</c> / <c>Throws</c>) carries over directly.
///     </para>
///     <para>
///         <strong>Setter side is deferred.</strong> A separate <c>IRefStructIndexerSetterSetup</c>
///         would be needed to register setter actions; for now, generated mock code can simply
///         forward setter calls to the base class or no-op when the key matches a ref-struct
///         getter setup.
///     </para>
///     <para>
///         <typeparamref name="TValue" /> is not anti-constrained for the same reason as
///         <c>TReturn</c> in <see cref="IRefStructReturnMethodSetup{TReturn, T}" /> — the setup
///         stores <c>Func&lt;TValue&gt;</c>, illegal for a ref struct.
///     </para>
/// </remarks>
public interface IRefStructIndexerGetterSetup<TValue, T> : IMethodSetup
	where T : allows ref struct
{
	/// <summary>
	///     Specifies if calling the base class implementation should be skipped on indexer read.
	/// </summary>
	IRefStructIndexerGetterSetup<TValue, T> SkippingBaseClass(bool skipBaseClass = true);

	/// <summary>
	///     Registers the <paramref name="returnValue" /> to be returned when the indexer is read
	///     with a matching key.
	/// </summary>
	IRefStructIndexerGetterSetup<TValue, T> Returns(TValue returnValue);

	/// <summary>
	///     Registers a <paramref name="returnFactory" /> whose return value is used when the
	///     indexer is read with a matching key.
	/// </summary>
	IRefStructIndexerGetterSetup<TValue, T> Returns(Func<TValue> returnFactory);

	/// <summary>
	///     Configures a new <typeparamref name="TException" /> to throw when the indexer is read
	///     with a matching key.
	/// </summary>
	IRefStructIndexerGetterSetup<TValue, T> Throws<TException>()
		where TException : Exception, new();

	/// <summary>
	///     Configures the given <paramref name="exception" /> to throw when the indexer is read
	///     with a matching key.
	/// </summary>
	IRefStructIndexerGetterSetup<TValue, T> Throws(Exception exception);

	/// <summary>
	///     Configures <paramref name="exceptionFactory" /> to be invoked to produce the exception
	///     to throw when the indexer is read with a matching key.
	/// </summary>
	IRefStructIndexerGetterSetup<TValue, T> Throws(Func<Exception> exceptionFactory);
}

/// <summary>
///     Ref-struct-safe indexer getter setup for arity 2. See <see cref="IRefStructIndexerGetterSetup{TValue, T}" />.
/// </summary>
public interface IRefStructIndexerGetterSetup<TValue, T1, T2> : IMethodSetup
	where T1 : allows ref struct
	where T2 : allows ref struct
{
	/// <inheritdoc cref="IRefStructIndexerGetterSetup{TValue, T}.SkippingBaseClass(bool)" />
	IRefStructIndexerGetterSetup<TValue, T1, T2> SkippingBaseClass(bool skipBaseClass = true);

	/// <inheritdoc cref="IRefStructIndexerGetterSetup{TValue, T}.Returns(TValue)" />
	IRefStructIndexerGetterSetup<TValue, T1, T2> Returns(TValue returnValue);

	/// <inheritdoc cref="IRefStructIndexerGetterSetup{TValue, T}.Returns(Func{TValue})" />
	IRefStructIndexerGetterSetup<TValue, T1, T2> Returns(Func<TValue> returnFactory);

	/// <inheritdoc cref="IRefStructIndexerGetterSetup{TValue, T}.Throws{TException}()" />
	IRefStructIndexerGetterSetup<TValue, T1, T2> Throws<TException>()
		where TException : Exception, new();

	/// <inheritdoc cref="IRefStructIndexerGetterSetup{TValue, T}.Throws(Exception)" />
	IRefStructIndexerGetterSetup<TValue, T1, T2> Throws(Exception exception);

	/// <inheritdoc cref="IRefStructIndexerGetterSetup{TValue, T}.Throws(Func{Exception})" />
	IRefStructIndexerGetterSetup<TValue, T1, T2> Throws(Func<Exception> exceptionFactory);
}

/// <summary>
///     Ref-struct-safe indexer getter setup for arity 3. See <see cref="IRefStructIndexerGetterSetup{TValue, T}" />.
/// </summary>
public interface IRefStructIndexerGetterSetup<TValue, T1, T2, T3> : IMethodSetup
	where T1 : allows ref struct
	where T2 : allows ref struct
	where T3 : allows ref struct
{
	/// <inheritdoc cref="IRefStructIndexerGetterSetup{TValue, T}.SkippingBaseClass(bool)" />
	IRefStructIndexerGetterSetup<TValue, T1, T2, T3> SkippingBaseClass(bool skipBaseClass = true);

	/// <inheritdoc cref="IRefStructIndexerGetterSetup{TValue, T}.Returns(TValue)" />
	IRefStructIndexerGetterSetup<TValue, T1, T2, T3> Returns(TValue returnValue);

	/// <inheritdoc cref="IRefStructIndexerGetterSetup{TValue, T}.Returns(Func{TValue})" />
	IRefStructIndexerGetterSetup<TValue, T1, T2, T3> Returns(Func<TValue> returnFactory);

	/// <inheritdoc cref="IRefStructIndexerGetterSetup{TValue, T}.Throws{TException}()" />
	IRefStructIndexerGetterSetup<TValue, T1, T2, T3> Throws<TException>()
		where TException : Exception, new();

	/// <inheritdoc cref="IRefStructIndexerGetterSetup{TValue, T}.Throws(Exception)" />
	IRefStructIndexerGetterSetup<TValue, T1, T2, T3> Throws(Exception exception);

	/// <inheritdoc cref="IRefStructIndexerGetterSetup{TValue, T}.Throws(Func{Exception})" />
	IRefStructIndexerGetterSetup<TValue, T1, T2, T3> Throws(Func<Exception> exceptionFactory);
}

/// <summary>
///     Ref-struct-safe indexer getter setup for arity 4. See <see cref="IRefStructIndexerGetterSetup{TValue, T}" />.
/// </summary>
public interface IRefStructIndexerGetterSetup<TValue, T1, T2, T3, T4> : IMethodSetup
	where T1 : allows ref struct
	where T2 : allows ref struct
	where T3 : allows ref struct
	where T4 : allows ref struct
{
	/// <inheritdoc cref="IRefStructIndexerGetterSetup{TValue, T}.SkippingBaseClass(bool)" />
	IRefStructIndexerGetterSetup<TValue, T1, T2, T3, T4> SkippingBaseClass(bool skipBaseClass = true);

	/// <inheritdoc cref="IRefStructIndexerGetterSetup{TValue, T}.Returns(TValue)" />
	IRefStructIndexerGetterSetup<TValue, T1, T2, T3, T4> Returns(TValue returnValue);

	/// <inheritdoc cref="IRefStructIndexerGetterSetup{TValue, T}.Returns(Func{TValue})" />
	IRefStructIndexerGetterSetup<TValue, T1, T2, T3, T4> Returns(Func<TValue> returnFactory);

	/// <inheritdoc cref="IRefStructIndexerGetterSetup{TValue, T}.Throws{TException}()" />
	IRefStructIndexerGetterSetup<TValue, T1, T2, T3, T4> Throws<TException>()
		where TException : Exception, new();

	/// <inheritdoc cref="IRefStructIndexerGetterSetup{TValue, T}.Throws(Exception)" />
	IRefStructIndexerGetterSetup<TValue, T1, T2, T3, T4> Throws(Exception exception);

	/// <inheritdoc cref="IRefStructIndexerGetterSetup{TValue, T}.Throws(Func{Exception})" />
	IRefStructIndexerGetterSetup<TValue, T1, T2, T3, T4> Throws(Func<Exception> exceptionFactory);
}
#endif
