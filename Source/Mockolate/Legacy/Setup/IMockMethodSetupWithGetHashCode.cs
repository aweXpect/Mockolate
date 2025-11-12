using Mockolate.Setup;

namespace Mockolate.Legacy.Setup;

#pragma warning disable S2326 // Unused type parameters should be removed
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
#pragma warning restore S2326 // Unused type parameters should be removed
