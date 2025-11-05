namespace Mockolate.Setup;

/// <summary>
///     Sets up methods on the mock for <typeparamref name="T" /> when it contains a <see cref="object.ToString()" /> and <see cref="object.GetHashCode()" /> method.
/// </summary>
public interface IMockMethodSetupWithToStringWithGetHashCode<T> : IMockMethodSetupWithToString<T>, IMockMethodSetupWithGetHashCode<T>
{
}
