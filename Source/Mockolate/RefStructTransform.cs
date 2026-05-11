#if NET9_0_OR_GREATER
namespace Mockolate;

/// <summary>
///     Transforms a ref struct value of type <typeparamref name="T" /> into a replacement of
///     the same type.
/// </summary>
/// <remarks>
///     Unlike <see cref="System.Func{T, TResult}" />, this delegate carries the
///     <c>allows ref struct</c> anti-constraint, so <typeparamref name="T" /> can be a ref
///     struct. Used by <see cref="It.IsRef{T}(RefStructTransform{T}, string)" /> to compute the
///     replacement value for a caller's <see langword="ref" /> variable when the mock is invoked.
/// </remarks>
public delegate T RefStructTransform<T>(T value)
	where T : allows ref struct;
#endif
