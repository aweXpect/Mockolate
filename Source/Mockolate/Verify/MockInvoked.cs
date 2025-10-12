using System.Linq;
using Mockolate.Interactions;
using Mockolate.Internals;

namespace Mockolate.Verify;

/// <summary>
///     Check which methods got invoked on the mocked instance for <typeparamref name="TMock" />.
/// </summary>
public class MockInvoked<T, TMock>(MockVerify<T, TMock> verify) : IMockInvoked<MockVerify<T, TMock>>
{
	/// <inheritdoc cref="IMockInvoked{TMock}.Method(string, With.Parameter[])" />
	VerificationResult<MockVerify<T, TMock>> IMockInvoked<MockVerify<T, TMock>>.Method(string methodName, params With.Parameter[] parameters)
	{
		MockInteractions interactions = ((IMockVerify<TMock>)verify).Interactions;
		return new(verify, interactions,
			interactions.Interactions
				.OfType<MethodInvocation>()
				.Where(method =>
					method.Name.Equals(methodName) &&
					method.Parameters.Length == parameters.Length &&
					!parameters.Where((parameter, i) => !parameter.Matches(method.Parameters[i])).Any())
				.Cast<IInteraction>()
				.ToArray(),
		$"invoked method {methodName.SubstringAfterLast('.')}({string.Join(", ", parameters.Select(x => x.ToString()))})");
	}

	/// <summary>
	///     Check which protected methods got invoked on the mocked instance <typeparamref name="TMock" />.
	/// </summary>
	public class Protected(MockVerify<T, TMock> verify) : MockInvoked<T, TMock>(verify)
	{
	}
}
