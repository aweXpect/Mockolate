namespace Mockolate;

/// <summary>
///     The mock interface gives access to the constructor parameters and the mock registrations of a mock instance.
/// </summary>
public interface IMock
{
	/// <summary>
	///     The used constructor parameters to create the mock instance.
	/// </summary>
	object?[] ConstructorParameters { get; }

	/// <summary>
	///     The mock registrations to store setups and interactions with the mock.
	/// </summary>
	MockRegistration Registrations { get; }
}
