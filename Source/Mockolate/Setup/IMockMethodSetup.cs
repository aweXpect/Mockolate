using Mockolate.Verify;

namespace Mockolate.Setup;

#pragma warning disable S2326 // Unused type parameters should be removed
/// <summary>
///     Sets up methods on the mock for <typeparamref name="T" />.
/// </summary>
public interface IMockMethodSetup<T> : IMockVerifyVerb<T>
{
}
#pragma warning restore S2326 // Unused type parameters should be removed
