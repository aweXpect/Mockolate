using System.Diagnostics.CodeAnalysis;
using Mockolate.Interactions;

namespace Mockolate.Setup;

/// <summary>
///     Interface for hiding some implementation details of <see cref="MethodSetup" />.
/// </summary>
public interface IMethodSetup
{
	/// <summary>
	///     Checks if the <paramref name="invocation" /> matches the setup.
	/// </summary>
	bool Matches(IInteraction invocation);

	/// <summary>
	///     Sets an <see langword="out" /> parameter with the specified name and returns its generated value of type
	///     <typeparamref name="T" />.
	/// </summary>
	/// <remarks>
	///     If a setup is configured, the value is generated according to the setup; otherwise, a default value
	///     is generated using the current <paramref name="behavior" />.
	/// </remarks>
	T SetOutParameter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(string parameterName, MockBehavior behavior);

	/// <summary>
	///     Sets an <see langword="ref" /> parameter with the specified name and the initial <paramref name="value" /> and
	///     returns its generated value of type <typeparamref name="T" />.
	/// </summary>
	/// <remarks>
	///     If a setup is configured, the value is generated according to the setup; otherwise, a default value
	///     is generated using the current <paramref name="behavior" />.
	/// </remarks>
	T SetRefParameter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(string parameterName, T value, MockBehavior behavior);
}
