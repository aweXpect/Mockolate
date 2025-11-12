using Mockolate.Interactions;

namespace Mockolate;

/// <summary>
///     A mock for type <typeparamref name="T" />.
/// </summary>
public partial class Mock<T> : IHasMockRegistration
{
	/// <inheritdoc cref="Mock{T}" />
	public Mock(T subject, MockRegistration mockRegistration)
	{
		Subject = subject;
		Registrations = mockRegistration;
	}

	/// <inheritdoc cref="IHasMockRegistration.Registrations" />
	public MockRegistration Registrations { get; }

	/// <summary>
	///     The mock subject.
	/// </summary>
	public T Subject { get; }

	/// <summary>
	///     The registered interactions on the mock.
	/// </summary>
	public MockInteractions Interactions => Registrations.Interactions;
}
