namespace Mockolate.Verify;

/// <summary>
///     Verifies the <see cref="Interactions" /> with the mocked subject in the <typeparamref name="T" />.
/// </summary>
public interface IMockVerify<out T> : IInteractiveMock<T>;
