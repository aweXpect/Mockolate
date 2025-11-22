namespace Mockolate;

/// <summary>
///     A mock for type <typeparamref name="T" />.
/// </summary>
public partial class Mock<T> : MockSetup<T>
{
	/// <inheritdoc cref="Mock{T}" />
	public Mock(T subject, MockRegistration mockRegistration) : base(subject, mockRegistration)
	{
	}
}
