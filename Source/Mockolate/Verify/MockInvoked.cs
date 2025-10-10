using System.Linq;
using Mockolate.Interactions;
using Mockolate.Internals;

namespace Mockolate.Verify;

/// <summary>
///     Check which methods got invoked on the mocked instance for <typeparamref name="TMock" />.
/// </summary>
public class MockInvoked<T, TMock>(IMockVerify<TMock> verify) : IMockInvoked<TMock>
{
	/// <inheritdoc cref="IMockInvoked{TMock}.Method(string, With.Parameter[])" />
	VerificationResult<TMock> IMockInvoked<TMock>.Method(string methodName, params With.Parameter[] parameters)
		=> new(verify.Mock, verify.Interactions,
			verify.Interactions.Interactions
				.OfType<MethodInvocation>()
				.Where(method =>
					method.Name.Equals(methodName) &&
					method.Parameters.Length == parameters.Length &&
					!parameters.Where((parameter, i) => !parameter.Matches(method.Parameters[i])).Any())
				.Cast<IInteraction>()
				.ToArray(),
        $"invoked method {methodName.SubstringAfterLast('.')}({string.Join(", ", parameters.Select(x => x.ToString()))})");

	/// <summary>
	///     Check which protected methods got invoked on the mocked instance <typeparamref name="TMock" />.
	/// </summary>
	public class Protected(IMockVerify<TMock> verify) : MockInvoked<T, TMock>(verify)
	{
	}
}
