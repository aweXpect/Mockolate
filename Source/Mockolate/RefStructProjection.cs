#if NET9_0_OR_GREATER
namespace Mockolate;

/// <summary>
///     Projects a ref struct value of type <typeparamref name="T" /> onto a
///     dictionary-friendly key of type <typeparamref name="TProjected" />.
/// </summary>
/// <remarks>
///     <para>
///         Used by <see cref="It.IsRefStructBy{T, TProjected}(RefStructProjection{T, TProjected})" />
///         and its predicated overload to correlate writes and reads on a ref-struct-keyed
///         indexer. The ref struct key itself cannot be stored in a dictionary (it may live on
///         the stack and carry inline <c>Span&lt;T&gt;</c> fields); the projection turns it into
///         a stable scalar that can.
///     </para>
///     <para>
///         Unlike <see cref="System.Func{T, TResult}" />, this delegate carries the
///         <c>allows ref struct</c> anti-constraint on the input so <typeparamref name="T" />
///         can itself be a ref struct.
///     </para>
/// </remarks>
public delegate TProjected RefStructProjection<in T, out TProjected>(T value)
	where T : allows ref struct;
#endif
