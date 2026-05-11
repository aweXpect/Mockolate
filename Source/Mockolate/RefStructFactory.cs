#if NET9_0_OR_GREATER
namespace Mockolate;

/// <summary>
///     Produces a ref struct value of type <typeparamref name="T" />.
/// </summary>
/// <remarks>
///     Unlike <see cref="System.Func{TResult}" />, this delegate carries the
///     <c>allows ref struct</c> anti-constraint on the return type, so
///     <typeparamref name="T" /> can be a ref struct such as <see cref="System.Span{T}" />
///     or a user-defined <c>ref struct</c>. Used by
///     <see cref="It.IsOut{T}(RefStructFactory{T}, string)" /> to produce the value assigned to
///     a caller's <see langword="out" /> variable when the mock is invoked.
/// </remarks>
public delegate T RefStructFactory<out T>()
	where T : allows ref struct;
#endif
