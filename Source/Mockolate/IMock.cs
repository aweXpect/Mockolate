namespace Mockolate;

/// <summary>
///     The mock interface gives access to the constructor parameters and the mock registry of a mock instance.
/// </summary>
public interface IMock
{
	/// <summary>
	///     The used constructor parameters to create the mock instance.
	/// </summary>
	object?[] ConstructorParameters { get; }

	/// <summary>
	///     The mock registry to store setups and interactions with the mock.
	/// </summary>
	MockRegistry MockRegistry { get; }

	/// <summary>
	///     A string representation of the mock, which includes the type of the mocked object and any additional interfaces it implements.
	/// </summary>
	string ToString();
}
