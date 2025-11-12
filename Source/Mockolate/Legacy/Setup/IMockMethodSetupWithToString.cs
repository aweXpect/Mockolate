using Mockolate.Setup;

namespace Mockolate.Legacy.Setup;

#pragma warning disable S2326 // Unused type parameters should be removed
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
#pragma warning restore S2326 // Unused type parameters should be removed
