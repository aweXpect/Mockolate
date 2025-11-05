namespace Mockolate.Setup;

#pragma warning disable S2326 // Unused type parameters should be removed
/// <summary>
///     Sets up methods on the mock for <typeparamref name="T" /> when it contains a <see cref="object.Equals(object)" /> method.
/// </summary>
public interface IMockMethodSetupWithEquals<T> : IMockMethodSetup<T>
{
	/// <summary>
	///     Setup for the method <see cref="object.Equals(object?)"/> with the given <paramref name="obj"/>.
	/// </summary>
	ReturnMethodSetup<bool, object?> Equals(With.Parameter<object?> obj);
}
#pragma warning restore S2326 // Unused type parameters should be removed
