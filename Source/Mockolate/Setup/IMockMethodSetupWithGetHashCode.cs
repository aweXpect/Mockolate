namespace Mockolate.Setup;

/// <summary>
///     Sets up methods on the mock for <typeparamref name="T" /> when it contains a <see cref="object.GetHashCode()" /> method.
/// </summary>
public interface IMockMethodSetupWithGetHashCode<T> : IMockMethodSetup<T>
{
	/// <summary>
	///     Setup for the method <see cref="object.GetHashCode()"/>.
	/// </summary>
	ReturnMethodSetup<int> GetHashCode();
}
