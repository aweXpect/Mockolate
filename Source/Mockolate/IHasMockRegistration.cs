namespace Mockolate;

/// <summary>
///     Specifies a class that has a <see cref="MockRegistration" />.
/// </summary>
public interface IHasMockRegistration
{
	/// <summary>
	///     The mock registrations to store setups and interactions with the mock.
	/// </summary>
	MockRegistration Registrations { get; }
}
