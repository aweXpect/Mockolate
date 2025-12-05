#if NET8_0_OR_GREATER
using Mockolate.Setup;

namespace Mockolate.Parameters;

/// <summary>
///     Matches a method parameter of type <see cref="System.ReadOnlySpan{T}" /> of <typeparamref name="T" /> against an
///     expectation.
/// </summary>
public interface IReadOnlySpanParameter<T> : IParameter<ReadOnlySpanWrapper<T>>;
#endif
