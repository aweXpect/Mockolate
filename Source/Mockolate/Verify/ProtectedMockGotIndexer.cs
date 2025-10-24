namespace Mockolate.Verify;

/// <summary>
///     Check which protected indexers got read on the mocked instance <typeparamref name="TMock" />.
/// </summary>
public class ProtectedMockGotIndexer<T, TMock>(MockGotIndexer<T, TMock> mockGotIndexer)
	: MockGotIndexer<T, TMock>(mockGotIndexer.Verify)
{
}
