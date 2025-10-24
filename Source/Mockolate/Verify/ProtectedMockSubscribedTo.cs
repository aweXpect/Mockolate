namespace Mockolate.Verify;

/// <summary>
///     Check which protected events were subscribed to on the mocked instance <typeparamref name="TMock" />.
/// </summary>
public class ProtectedMockSubscribedTo<T, TMock>(MockSubscribedTo<T, TMock> subscribedTo)
	: MockSubscribedTo<T, TMock>(subscribedTo.Verify)
{
}
