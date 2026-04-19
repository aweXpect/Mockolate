#if NET9_0_OR_GREATER
using System;

namespace Mockolate.Setup;

/// <summary>
///     Sets up the setter of a ref-struct-keyed indexer with a single key of type
///     <typeparamref name="T" /> and value type <typeparamref name="TValue" />.
/// </summary>
/// <remarks>
///     <para>
///         The ref-struct-safe counterpart to the setter half of
///         <see cref="IIndexerSetup{TValue, T1}" />. The surface is deliberately narrow: only
///         throws, <c>OnSet</c> (an <c>Action&lt;TValue&gt;</c> callback — <typeparamref name="TValue" />
///         is not anti-constrained, so an <c>Action&lt;TValue&gt;</c> is legal), and base-class-skip
///         are supported. The full callback/builder chain requires the ref-struct key
///         <typeparamref name="T" /> to flow through delegate type parameters, which is illegal.
///     </para>
///     <para>
///         Use <see cref="It.IsAnyRefStruct{T}" /> or <see cref="It.IsRefStruct{T}" /> to supply
///         the key matcher at setup time.
///     </para>
/// </remarks>
public interface IRefStructIndexerSetterSetup<TValue, T> : IMethodSetup
	where T : allows ref struct
{
	/// <inheritdoc cref="IRefStructVoidMethodSetup{T}.SkippingBaseClass(bool)" />
	IRefStructIndexerSetterSetup<TValue, T> SkippingBaseClass(bool skipBaseClass = true);

	/// <summary>
	///     Registers a callback that runs when the indexer setter is invoked with a matching key.
	///     The callback receives the value being written.
	/// </summary>
	IRefStructIndexerSetterSetup<TValue, T> OnSet(Action<TValue> callback);

	/// <inheritdoc cref="IRefStructVoidMethodSetup{T}.Throws{TException}()" />
	IRefStructIndexerSetterSetup<TValue, T> Throws<TException>()
		where TException : Exception, new();

	/// <inheritdoc cref="IRefStructVoidMethodSetup{T}.Throws(Exception)" />
	IRefStructIndexerSetterSetup<TValue, T> Throws(Exception exception);

	/// <inheritdoc cref="IRefStructVoidMethodSetup{T}.Throws(Func{Exception})" />
	IRefStructIndexerSetterSetup<TValue, T> Throws(Func<Exception> exceptionFactory);
}

/// <summary>
///     Setter setup for a ref-struct-keyed indexer with two keys.
///     See <see cref="IRefStructIndexerSetterSetup{TValue, T}" />.
/// </summary>
public interface IRefStructIndexerSetterSetup<TValue, T1, T2> : IMethodSetup
	where T1 : allows ref struct
	where T2 : allows ref struct
{
	/// <inheritdoc cref="IRefStructIndexerSetterSetup{TValue, T}.SkippingBaseClass(bool)" />
	IRefStructIndexerSetterSetup<TValue, T1, T2> SkippingBaseClass(bool skipBaseClass = true);

	/// <inheritdoc cref="IRefStructIndexerSetterSetup{TValue, T}.OnSet(Action{TValue})" />
	IRefStructIndexerSetterSetup<TValue, T1, T2> OnSet(Action<TValue> callback);

	/// <inheritdoc cref="IRefStructIndexerSetterSetup{TValue, T}.Throws{TException}()" />
	IRefStructIndexerSetterSetup<TValue, T1, T2> Throws<TException>()
		where TException : Exception, new();

	/// <inheritdoc cref="IRefStructIndexerSetterSetup{TValue, T}.Throws(Exception)" />
	IRefStructIndexerSetterSetup<TValue, T1, T2> Throws(Exception exception);

	/// <inheritdoc cref="IRefStructIndexerSetterSetup{TValue, T}.Throws(Func{Exception})" />
	IRefStructIndexerSetterSetup<TValue, T1, T2> Throws(Func<Exception> exceptionFactory);
}

/// <summary>
///     Setter setup for a ref-struct-keyed indexer with three keys.
///     See <see cref="IRefStructIndexerSetterSetup{TValue, T}" />.
/// </summary>
public interface IRefStructIndexerSetterSetup<TValue, T1, T2, T3> : IMethodSetup
	where T1 : allows ref struct
	where T2 : allows ref struct
	where T3 : allows ref struct
{
	/// <inheritdoc cref="IRefStructIndexerSetterSetup{TValue, T}.SkippingBaseClass(bool)" />
	IRefStructIndexerSetterSetup<TValue, T1, T2, T3> SkippingBaseClass(bool skipBaseClass = true);

	/// <inheritdoc cref="IRefStructIndexerSetterSetup{TValue, T}.OnSet(Action{TValue})" />
	IRefStructIndexerSetterSetup<TValue, T1, T2, T3> OnSet(Action<TValue> callback);

	/// <inheritdoc cref="IRefStructIndexerSetterSetup{TValue, T}.Throws{TException}()" />
	IRefStructIndexerSetterSetup<TValue, T1, T2, T3> Throws<TException>()
		where TException : Exception, new();

	/// <inheritdoc cref="IRefStructIndexerSetterSetup{TValue, T}.Throws(Exception)" />
	IRefStructIndexerSetterSetup<TValue, T1, T2, T3> Throws(Exception exception);

	/// <inheritdoc cref="IRefStructIndexerSetterSetup{TValue, T}.Throws(Func{Exception})" />
	IRefStructIndexerSetterSetup<TValue, T1, T2, T3> Throws(Func<Exception> exceptionFactory);
}

/// <summary>
///     Setter setup for a ref-struct-keyed indexer with four keys.
///     See <see cref="IRefStructIndexerSetterSetup{TValue, T}" />.
/// </summary>
#pragma warning disable S2436 // Types and methods should not have too many generic parameters
public interface IRefStructIndexerSetterSetup<TValue, T1, T2, T3, T4> : IMethodSetup
	where T1 : allows ref struct
	where T2 : allows ref struct
	where T3 : allows ref struct
	where T4 : allows ref struct
{
	/// <inheritdoc cref="IRefStructIndexerSetterSetup{TValue, T}.SkippingBaseClass(bool)" />
	IRefStructIndexerSetterSetup<TValue, T1, T2, T3, T4> SkippingBaseClass(bool skipBaseClass = true);

	/// <inheritdoc cref="IRefStructIndexerSetterSetup{TValue, T}.OnSet(Action{TValue})" />
	IRefStructIndexerSetterSetup<TValue, T1, T2, T3, T4> OnSet(Action<TValue> callback);

	/// <inheritdoc cref="IRefStructIndexerSetterSetup{TValue, T}.Throws{TException}()" />
	IRefStructIndexerSetterSetup<TValue, T1, T2, T3, T4> Throws<TException>()
		where TException : Exception, new();

	/// <inheritdoc cref="IRefStructIndexerSetterSetup{TValue, T}.Throws(Exception)" />
	IRefStructIndexerSetterSetup<TValue, T1, T2, T3, T4> Throws(Exception exception);

	/// <inheritdoc cref="IRefStructIndexerSetterSetup{TValue, T}.Throws(Func{Exception})" />
	IRefStructIndexerSetterSetup<TValue, T1, T2, T3, T4> Throws(Func<Exception> exceptionFactory);
}
#pragma warning restore S2436 // Types and methods should not have too many generic parameters
#endif
