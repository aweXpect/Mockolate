#if NET9_0_OR_GREATER
namespace Mockolate.Parameters;

/// <summary>
///     Matches a <see langword="ref" /> parameter of a ref struct type
///     <typeparamref name="T" /> against an expectation.
/// </summary>
/// <remarks>
///     The ref-struct-safe counterpart to <see cref="IRefParameter{T}" />. The surface is
///     deliberately narrower: no <c>Do(Action&lt;T&gt;)</c> callback because
///     <see cref="System.Action{T}" /> cannot carry the <c>allows ref struct</c> anti-constraint.
///     Use a non-ref-struct <see cref="IRefParameter{T}" /> for types that are not ref structs.
/// </remarks>
public interface IRefStructRefParameter<T>
	where T : allows ref struct
{
	/// <summary>
	///     Retrieves the value to which the <see langword="ref" /> parameter should be set,
	///     given the caller's current <paramref name="value" />.
	/// </summary>
	T GetValue(T value);
}
#endif
