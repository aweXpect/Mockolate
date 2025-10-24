namespace Mockolate.Verify;

/// <summary>
///     Check which protected properties got read on the mocked instance <typeparamref name="TMock" />.
/// </summary>
public class ProtectedMockGot<T, TMock>(MockGot<T, TMock> mockGot)
	: MockGot<T, TMock>(mockGot.Verify)
{
}
