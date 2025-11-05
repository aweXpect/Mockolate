namespace Mockolate.Setup;

/// <summary>
///     Sets up methods on the mock for <typeparamref name="T" /> when it contains a <see cref="object.ToString()" /> and <see cref="object.Equals(object)" /> method.
/// </summary>
public interface IMockMethodSetupWithToStringWithEquals<T> : IMockMethodSetupWithToString<T>, IMockMethodSetupWithEquals<T>
{
}
