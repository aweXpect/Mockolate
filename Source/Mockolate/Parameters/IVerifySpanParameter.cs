#if NET8_0_OR_GREATER
namespace Mockolate.Parameters;

/// <summary>
///     Matches a method parameter of type <see cref="System.Span{T}" /> of <typeparamref name="T" /> against an
///     expectation.
/// </summary>
public interface IVerifySpanParameter<T> : ISpanParameter<T>;
#endif
