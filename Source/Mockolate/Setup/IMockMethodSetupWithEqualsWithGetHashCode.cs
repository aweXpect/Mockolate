namespace Mockolate.Setup;

/// <summary>
///     Sets up methods on the mock for <typeparamref name="T" /> when it contains a <see cref="object.Equals(object)" /> and <see cref="object.GetHashCode()" /> method.
/// </summary>
public interface IMockMethodSetupWithEqualsWithGetHashCode<T> : IMockMethodSetupWithEquals<T>, IMockMethodSetupWithGetHashCode<T>
{
}
