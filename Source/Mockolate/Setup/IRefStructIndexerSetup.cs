#if NET9_0_OR_GREATER
using System;

namespace Mockolate.Setup;

/// <summary>
///     Combined setup for a ref-struct-keyed indexer that has both a getter and a setter, with a
///     single key of type <typeparamref name="T" /> and value type <typeparamref name="TValue" />.
/// </summary>
/// <remarks>
///     <para>
///         Composes an underlying <see cref="IRefStructIndexerGetterSetup{TValue, T}" /> and an
///         <see cref="IRefStructIndexerSetterSetup{TValue, T}" />. <c>Returns</c> wires the getter;
///         <c>OnSet</c> wires the setter; <c>Throws</c> applies to both accessors (whichever is
///         invoked). <c>SkippingBaseClass</c> forwards to both underlying setups.
///     </para>
///     <para>
///         The two halves do not share state beyond the matcher and the throw slot — a value
///         written via the setter is NOT automatically read back via the getter. That
///         "initialize-via-set" semantic stays out of scope for this commit.
///     </para>
/// </remarks>
public interface IRefStructIndexerSetup<TValue, T> : IMethodSetup
	where T : allows ref struct
{
	/// <inheritdoc cref="IRefStructIndexerGetterSetup{TValue, T}.SkippingBaseClass(bool)" />
	IRefStructIndexerSetup<TValue, T> SkippingBaseClass(bool skipBaseClass = true);

	/// <inheritdoc cref="IRefStructIndexerGetterSetup{TValue, T}.Returns(TValue)" />
	IRefStructIndexerSetup<TValue, T> Returns(TValue returnValue);

	/// <inheritdoc cref="IRefStructIndexerGetterSetup{TValue, T}.Returns(Func{TValue})" />
	IRefStructIndexerSetup<TValue, T> Returns(Func<TValue> returnFactory);

	/// <inheritdoc cref="IRefStructIndexerSetterSetup{TValue, T}.OnSet(Action{TValue})" />
	IRefStructIndexerSetup<TValue, T> OnSet(Action<TValue> callback);

	/// <inheritdoc cref="IRefStructIndexerGetterSetup{TValue, T}.Throws{TException}()" />
	IRefStructIndexerSetup<TValue, T> Throws<TException>()
		where TException : Exception, new();

	/// <inheritdoc cref="IRefStructIndexerGetterSetup{TValue, T}.Throws(Exception)" />
	IRefStructIndexerSetup<TValue, T> Throws(Exception exception);

	/// <inheritdoc cref="IRefStructIndexerGetterSetup{TValue, T}.Throws(Func{Exception})" />
	IRefStructIndexerSetup<TValue, T> Throws(Func<Exception> exceptionFactory);
}

/// <summary>
///     Combined getter + setter setup for a ref-struct-keyed indexer with two keys.
///     See <see cref="IRefStructIndexerSetup{TValue, T}" />.
/// </summary>
public interface IRefStructIndexerSetup<TValue, T1, T2> : IMethodSetup
	where T1 : allows ref struct
	where T2 : allows ref struct
{
	/// <inheritdoc cref="IRefStructIndexerSetup{TValue, T}.SkippingBaseClass(bool)" />
	IRefStructIndexerSetup<TValue, T1, T2> SkippingBaseClass(bool skipBaseClass = true);

	/// <inheritdoc cref="IRefStructIndexerSetup{TValue, T}.Returns(TValue)" />
	IRefStructIndexerSetup<TValue, T1, T2> Returns(TValue returnValue);

	/// <inheritdoc cref="IRefStructIndexerSetup{TValue, T}.Returns(Func{TValue})" />
	IRefStructIndexerSetup<TValue, T1, T2> Returns(Func<TValue> returnFactory);

	/// <inheritdoc cref="IRefStructIndexerSetup{TValue, T}.OnSet(Action{TValue})" />
	IRefStructIndexerSetup<TValue, T1, T2> OnSet(Action<TValue> callback);

	/// <inheritdoc cref="IRefStructIndexerSetup{TValue, T}.Throws{TException}()" />
	IRefStructIndexerSetup<TValue, T1, T2> Throws<TException>()
		where TException : Exception, new();

	/// <inheritdoc cref="IRefStructIndexerSetup{TValue, T}.Throws(Exception)" />
	IRefStructIndexerSetup<TValue, T1, T2> Throws(Exception exception);

	/// <inheritdoc cref="IRefStructIndexerSetup{TValue, T}.Throws(Func{Exception})" />
	IRefStructIndexerSetup<TValue, T1, T2> Throws(Func<Exception> exceptionFactory);
}

/// <summary>
///     Combined getter + setter setup for a ref-struct-keyed indexer with three keys.
///     See <see cref="IRefStructIndexerSetup{TValue, T}" />.
/// </summary>
public interface IRefStructIndexerSetup<TValue, T1, T2, T3> : IMethodSetup
	where T1 : allows ref struct
	where T2 : allows ref struct
	where T3 : allows ref struct
{
	/// <inheritdoc cref="IRefStructIndexerSetup{TValue, T}.SkippingBaseClass(bool)" />
	IRefStructIndexerSetup<TValue, T1, T2, T3> SkippingBaseClass(bool skipBaseClass = true);

	/// <inheritdoc cref="IRefStructIndexerSetup{TValue, T}.Returns(TValue)" />
	IRefStructIndexerSetup<TValue, T1, T2, T3> Returns(TValue returnValue);

	/// <inheritdoc cref="IRefStructIndexerSetup{TValue, T}.Returns(Func{TValue})" />
	IRefStructIndexerSetup<TValue, T1, T2, T3> Returns(Func<TValue> returnFactory);

	/// <inheritdoc cref="IRefStructIndexerSetup{TValue, T}.OnSet(Action{TValue})" />
	IRefStructIndexerSetup<TValue, T1, T2, T3> OnSet(Action<TValue> callback);

	/// <inheritdoc cref="IRefStructIndexerSetup{TValue, T}.Throws{TException}()" />
	IRefStructIndexerSetup<TValue, T1, T2, T3> Throws<TException>()
		where TException : Exception, new();

	/// <inheritdoc cref="IRefStructIndexerSetup{TValue, T}.Throws(Exception)" />
	IRefStructIndexerSetup<TValue, T1, T2, T3> Throws(Exception exception);

	/// <inheritdoc cref="IRefStructIndexerSetup{TValue, T}.Throws(Func{Exception})" />
	IRefStructIndexerSetup<TValue, T1, T2, T3> Throws(Func<Exception> exceptionFactory);
}

/// <summary>
///     Combined getter + setter setup for a ref-struct-keyed indexer with four keys.
///     See <see cref="IRefStructIndexerSetup{TValue, T}" />.
/// </summary>
public interface IRefStructIndexerSetup<TValue, T1, T2, T3, T4> : IMethodSetup
	where T1 : allows ref struct
	where T2 : allows ref struct
	where T3 : allows ref struct
	where T4 : allows ref struct
{
	/// <inheritdoc cref="IRefStructIndexerSetup{TValue, T}.SkippingBaseClass(bool)" />
	IRefStructIndexerSetup<TValue, T1, T2, T3, T4> SkippingBaseClass(bool skipBaseClass = true);

	/// <inheritdoc cref="IRefStructIndexerSetup{TValue, T}.Returns(TValue)" />
	IRefStructIndexerSetup<TValue, T1, T2, T3, T4> Returns(TValue returnValue);

	/// <inheritdoc cref="IRefStructIndexerSetup{TValue, T}.Returns(Func{TValue})" />
	IRefStructIndexerSetup<TValue, T1, T2, T3, T4> Returns(Func<TValue> returnFactory);

	/// <inheritdoc cref="IRefStructIndexerSetup{TValue, T}.OnSet(Action{TValue})" />
	IRefStructIndexerSetup<TValue, T1, T2, T3, T4> OnSet(Action<TValue> callback);

	/// <inheritdoc cref="IRefStructIndexerSetup{TValue, T}.Throws{TException}()" />
	IRefStructIndexerSetup<TValue, T1, T2, T3, T4> Throws<TException>()
		where TException : Exception, new();

	/// <inheritdoc cref="IRefStructIndexerSetup{TValue, T}.Throws(Exception)" />
	IRefStructIndexerSetup<TValue, T1, T2, T3, T4> Throws(Exception exception);

	/// <inheritdoc cref="IRefStructIndexerSetup{TValue, T}.Throws(Func{Exception})" />
	IRefStructIndexerSetup<TValue, T1, T2, T3, T4> Throws(Func<Exception> exceptionFactory);
}
#endif
