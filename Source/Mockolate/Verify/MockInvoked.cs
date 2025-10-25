using System.Linq;
using Mockolate.Interactions;
using Mockolate.Internals;

namespace Mockolate.Verify;

/// <summary>
///     Check which methods got invoked on the mocked instance for <typeparamref name="TMock" />.
/// </summary>
public class MockInvoked<T, TMock>(MockVerify<T, TMock> verify) : IMockInvoked<MockVerify<T, TMock>>
{
	internal MockVerify<T, TMock> Verify { get; } = verify;

	/// <inheritdoc cref="IMockInvoked{TMock}.Method(string, With.Parameter[])" />
	VerificationResult<MockVerify<T, TMock>> IMockInvoked<MockVerify<T, TMock>>.Method(string methodName, params With.Parameter[] parameters)
	{
		MockInteractions interactions = ((IMockVerify<TMock>)Verify).Interactions;
		return new(Verify, interactions,
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

	/// <inheritdoc cref="IMockInvoked{TMock}.Method(string, With.Parameters)" />
	VerificationResult<MockVerify<T, TMock>> IMockInvoked<MockVerify<T, TMock>>.Method(string methodName, With.Parameters parameters)
	{
		MockInteractions interactions = ((IMockVerify<TMock>)Verify).Interactions;
		return new(Verify, interactions,
			interactions.Interactions
				.OfType<MethodInvocation>()
				.Where(method =>
					method.Name.Equals(methodName) &&
					parameters.Matches(method.Parameters))
				.Cast<IInteraction>()
				.ToArray(),
		$"invoked method {methodName.SubstringAfterLast('.')}({parameters})");
	}
}
