namespace Mockolate.Verify;

/// <summary>
///     Check which protected indexers were set on the mocked instance <typeparamref name="TMock" />.
/// </summary>
public class ProtectedMockSetIndexer<T, TMock>(MockSetIndexer<T, TMock> mockSetIndexer)
	: MockSetIndexer<T, TMock>(mockSetIndexer.Verify)
{
}
