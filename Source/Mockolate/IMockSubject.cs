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
