#if NET9_0_OR_GREATER
namespace Mockolate;

/// <summary>
///     A predicate that matches a ref struct value of type <typeparamref name="T" />.
/// </summary>
/// <remarks>
///     Unlike <see cref="System.Predicate{T}" />, this delegate carries the
///     <c>allows ref struct</c> anti-constraint, so <typeparamref name="T" /> can be a
///     ref struct such as <see cref="System.ReadOnlySpan{T}" /> or
///     <see cref="System.Text.Json.Utf8JsonReader" />.
/// </remarks>
public delegate bool RefStructPredicate<in T>(T value)
	where T : allows ref struct;
#endif