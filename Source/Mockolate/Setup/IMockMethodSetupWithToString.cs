namespace Mockolate.Setup;

/// <summary>
///     Sets up methods on the mock for <typeparamref name="T" /> when it contains a <see cref="object.ToString()" /> method.
/// </summary>
public interface IMockMethodSetupWithToString<T> : IMockMethodSetup<T>
{
	/// <summary>
	///     Setup for the method <see cref="object.ToString()"/>.
	/// </summary>
	ReturnMethodSetup<string> ToString();
}
