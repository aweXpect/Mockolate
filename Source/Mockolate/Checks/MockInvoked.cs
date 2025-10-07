using System.Linq;
using Mockolate.Interactions;
using Mockolate.Internals;

namespace Mockolate.Checks;

/// <summary>
///     Check which methods got invoked on the mocked instance for <typeparamref name="TMock" />.
/// </summary>
public class MockInvoked<T, TMock>(MockInteractions interactions, TMock mock) : IMockInvoked<TMock>
{
	/// <inheritdoc cref="IMockInvoked{TMock}.Method(string, With.Parameter[])" />
	CheckResult<TMock> IMockInvoked<TMock>.Method(string methodName, params With.Parameter[] parameters)
		=> new(mock, interactions,
			interactions.Interactions
				.OfType<MethodInvocation>()
				.Where(method =>
					method.Name.Equals(methodName) &&
					method.Parameters.Length == parameters.Length &&
					!parameters.Where((parameter, i) => !parameter.Matches(method.Parameters[i])).Any())
				.Cast<IInteraction>()
				.ToArray(),
        $"invoked method {methodName.SubstringAfterLast('.')}({string.Join(", ", parameters.Select(x => x.ToString()))})");

	/// <summary>
	///     A proxy implementation of <see cref="IMockInvoked{TMock}" /> that forwards all calls to the provided
	///     <paramref name="inner" /> instance.
	/// </summary>
	public class Proxy(IMockInvoked<TMock> inner, MockInteractions interactions, TMock mock)
		: MockInvoked<T, TMock>(interactions, mock), IMockInvoked<TMock>
	{
		/// <inheritdoc cref="IMockInvoked{TMock}.Method(string, With.Parameter[])" />
		CheckResult<TMock> IMockInvoked<TMock>.Method(string methodName, params With.Parameter[] parameters)
			=> inner.Method(methodName, parameters);
	}

	/// <summary>
	///     Check which protected methods got invoked on the mocked instance <typeparamref name="TMock" />.
	/// </summary>
	public class Protected(IMockInvoked<TMock> inner, MockInteractions interactions, TMock mock)
		: MockInvoked<T, TMock>(interactions, mock), IMockInvoked<TMock>
	{
		/// <inheritdoc cref="IMockInvoked{TMock}.Method(string, With.Parameter[])" />
		CheckResult<TMock> IMockInvoked<TMock>.Method(string methodName, params With.Parameter[] parameters)
			=> inner.Method(methodName, parameters);
	}
}
