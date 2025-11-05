namespace Mockolate.Setup;

#pragma warning disable S2326 // Unused type parameters should be removed
/// <summary>
///     Sets up methods on the mock for <typeparamref name="T" /> when it contains a <see cref="object.ToString()" /> and <see cref="object.GetHashCode()" /> method.
/// </summary>
public interface IMockMethodSetupWithToStringWithGetHashCode<T> : IMockMethodSetupWithToString<T>, IMockMethodSetupWithGetHashCode<T>
{
}
#pragma warning restore S2326 // Unused type parameters should be removed
