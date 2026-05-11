#if NET9_0_OR_GREATER
namespace Mockolate.Parameters;

/// <summary>
///     Matches an <see langword="out" /> parameter of a ref struct type
///     <typeparamref name="T" /> against an expectation.
/// </summary>
/// <remarks>
///     The ref-struct-safe counterpart to <see cref="IOutParameter{T}" />. The surface is
///     deliberately narrower: no <c>Do(Action&lt;T&gt;)</c> callback because
///     <see cref="System.Action{T}" /> cannot carry the <c>allows ref struct</c> anti-constraint.
///     Use a non-ref-struct <see cref="IOutParameter{T}" /> for types that are not ref structs.
/// </remarks>
public interface IOutRefStructParameter<T>
	where T : allows ref struct
{
	/// <summary>
	///     Tries to get the value to which the <see langword="out" /> parameter should be set.
	/// </summary>
	/// <remarks>
	///     When the method returns <see langword="true" />, <paramref name="value" /> is the value
	///     the mock should write back to the <see langword="out" /> parameter. When it returns
	///     <see langword="false" />, the mock writes <see langword="default" />(<typeparamref name="T" />)
	///     instead.
	/// </remarks>
	bool TryGetValue(out T value);
}
#endif
