namespace Mockolate.Interactions;

/// <summary>
///     Marker interface for method interactions with the mock.
/// </summary>
public interface IMethodInteraction : IInteraction
{
	/// <summary>
	///     The name of the method.
	/// </summary>
	string Name { get; }
}
