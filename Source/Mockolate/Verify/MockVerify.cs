using Mockolate.Interactions;

namespace Mockolate.Verify;

/// <summary>
///     Verifies the <paramref name="interactions"/> with the mocked subject in the <typeparamref name="TMock"/> <paramref name="mock"/>.
/// </summary>
public class MockVerify<T, TMock>(MockInteractions interactions, TMock mock) : IMockVerify<TMock>
{
	/// <summary>
	///     Gets a value indicating whether all expected interactions have been verified.
	/// </summary>
	public bool ThatAllInteractionsAreVerified() => !interactions.HasMissingVerifications;

	/// <inheritdoc cref="IMockVerify.Interactions" />
	MockInteractions IMockVerify.Interactions
		=> interactions;

	/// <inheritdoc cref="IMockVerify{TMock}.Mock" />
	TMock IMockVerify<TMock>.Mock
		=> mock;
}

/// <summary>
///     Verifies the protected interactions with the mocked subject in the <typeparamref name="TMock"/> mock.
/// </summary>
public class ProtectedMockVerify<T, TMock>(IMockVerify<TMock> verify)
	: MockVerify<T, TMock>(verify.Interactions, verify.Mock)
{
}
