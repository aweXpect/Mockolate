namespace Mockolate;

/// <summary>
///     A subject of type <typeparamref name="T" /> that can be mocked.
/// </summary>
public interface IMockSubject<T> : IHasMockRegistration
{
	/// <summary>
	///     The underlying mock.
	/// </summary>
	Mock<T> Mock { get; }
}

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

#pragma warning disable S2326 // Unused type parameters should be removed
/// <summary>
///     Marker interface for mock interfaces used to fluently combine requests.
/// </summary>
public interface IInteractiveMock<out T>;
#pragma warning restore S2326 // Unused type parameters should be removed
