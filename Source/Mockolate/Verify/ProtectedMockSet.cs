namespace Mockolate.Verify;

/// <summary>
///     Check which protected properties were set on the mocked instance <typeparamref name="TMock" />.
/// </summary>
public class ProtectedMockSet<T, TMock>(MockSet<T, TMock> mockSet)
	: MockSet<T, TMock>(mockSet.Verify)
{
}
