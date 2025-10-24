namespace Mockolate.Verify;

/// <summary>
///     Check which protected events were unsubscribed from on the mocked instance <typeparamref name="TMock" />.
/// </summary>
public class ProtectedMockUnsubscribedFrom<T, TMock>(MockUnsubscribedFrom<T, TMock> unsubscribedFrom)
	: MockUnsubscribedFrom<T, TMock>(unsubscribedFrom.Verify)
{
}
