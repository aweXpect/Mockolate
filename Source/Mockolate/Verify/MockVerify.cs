using System.Linq;
using Mockolate.Checks;
using Mockolate.Interactions;
using Mockolate.Internals;

namespace Mockolate.Verify;

/// <summary>
///     Verifies the <paramref name="interactions"/> with the mocked subject in the <typeparamref name="TMock"/> <paramref name="mock"/>.
/// </summary>
public class MockVerify<T, TMock>(MockInteractions interactions, TMock mock) : IMockVerify<TMock>
{
	/// <inheritdoc cref="IMockVerify{TMock}.Interactions" />
	MockInteractions IMockVerify<TMock>.Interactions
		=> interactions;

	/// <inheritdoc cref="IMockVerify{TMock}.Mock" />
	TMock IMockVerify<TMock>.Mock
		=> mock;

	/// <summary>
	///     Verifies the protected interactions with the mocked subject in the <typeparamref name="TMock"/> mock.
	/// </summary>
	public class Protected(IMockVerify<TMock> verify)
		: MockVerify<T, TMock>(verify.Interactions, verify.Mock)
	{
	}
}
