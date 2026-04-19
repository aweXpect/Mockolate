#if NET9_0_OR_GREATER
namespace Mockolate.Parameters;

/// <summary>
///     A <see cref="IParameterMatch{T}" /> that additionally knows how to project a ref struct
///     value of type <typeparamref name="T" /> onto a dictionary-friendly scalar key.
/// </summary>
/// <remarks>
///     <para>
///         Used by <see cref="Mockolate.Setup.RefStructIndexerSetup{TValue, T}" /> to correlate
///         setter writes with getter reads: the ref struct key itself cannot be stored (it may
///         live on the stack and carry inline spans), but the projected value can.
///     </para>
///     <para>
///         This non-generic-in-<c>TProjected</c> base exists so the storage layer can box the
///         projected key without knowing its type. Implementations that know the typed
///         projection additionally implement
///         <see cref="IRefStructProjectionMatch{T, TProjected}" />.
///     </para>
/// </remarks>
public interface IRefStructProjectionMatch<T> : IParameterMatch<T>
	where T : allows ref struct
{
	/// <summary>
	///     Projects <paramref name="value" /> to a boxed scalar key suitable for storage lookup
	///     in a <see cref="System.Collections.Generic.Dictionary{TKey, TValue}" /> keyed on
	///     <see cref="object" />.
	/// </summary>
	object Project(T value);
}

/// <summary>
///     Typed specialization of <see cref="IRefStructProjectionMatch{T}" /> that exposes the
///     projected key in its native type. <typeparamref name="TProjected" /> must be
///     <c>notnull</c> so it is a valid dictionary key.
/// </summary>
public interface IRefStructProjectionMatch<T, TProjected> : IRefStructProjectionMatch<T>
	where T : allows ref struct
	where TProjected : notnull
{
	/// <summary>
	///     Returns the strongly-typed projected key for <paramref name="value" />.
	/// </summary>
	new TProjected Project(T value);
}
#endif
