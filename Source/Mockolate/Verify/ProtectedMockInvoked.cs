namespace Mockolate.Verify;

/// <summary>
///     Check which protected methods got invoked on the mocked instance <typeparamref name="TMock" />.
/// </summary>
public class ProtectedMockInvoked<T, TMock>(MockInvoked<T, TMock> mockInvoked)
	: MockInvoked<T, TMock>(mockInvoked.Verify)
{
}
