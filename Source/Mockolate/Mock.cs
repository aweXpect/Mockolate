namespace Mockolate;

/// <summary>
///     A mock for type <typeparamref name="T" />.
/// </summary>
public partial class Mock<T> : MockSetup<T>
{
	/// <inheritdoc cref="Mock{T}" />
	public Mock(T subject, MockRegistration mockRegistration, object?[]? constructorParameters = null) : base(subject,
		mockRegistration)
	{
		ConstructorParameters = constructorParameters ?? [];
	}

	/// <summary>
	///     The constructor parameters provided when creating the mock.
	/// </summary>
	public object?[] ConstructorParameters { get; }
}
