namespace Mockolate.Verify;

#pragma warning disable S2326 // Unused type parameters should be removed
/// <summary>
///     Verifies the <see cref="Interactions" /> with the mocked subject in the <typeparamref name="T" />.
/// </summary>
public interface IMockVerify<out T> : IInteractiveMock<T>;
#pragma warning restore S2326 // Unused type parameters should be removed
