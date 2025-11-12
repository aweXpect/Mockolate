using Mockolate.Setup;

namespace Mockolate.Legacy.Setup;

#pragma warning disable S2326 // Unused type parameters should be removed
/// <summary>
///     Sets up methods on the mock for <typeparamref name="T" /> when it contains a <see cref="object.ToString()" /> and <see cref="object.Equals(object)" /> method.
/// </summary>
public interface IMockMethodSetupWithToStringWithEquals<T> : IMockMethodSetupWithToString<T>, IMockMethodSetupWithEquals<T>
{
}
#pragma warning restore S2326 // Unused type parameters should be removed
