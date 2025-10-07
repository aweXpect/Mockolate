using System.Linq;
using Mockolate.Checks.Interactions;

namespace Mockolate.Checks;

/// <summary>
///     Check which methods got invoked on the mocked instance for <typeparamref name="TMock" />.
/// </summary>
public class MockInvoked<T, TMock>(Checks checks, TMock mock) : IMockInvoked<TMock>
{
	/// <inheritdoc cref="IMockInvoked{TMock}.Method(string, With.Parameter[])" />
	CheckResult<TMock> IMockInvoked<TMock>.Method(string methodName, params With.Parameter[] parameters)
		=> new(mock, checks,
			checks.Interactions
				.OfType<MethodInvocation>()
				.Where(method =>
					method.Name.Equals(methodName) &&
					method.Parameters.Length == parameters.Length &&
					!parameters.Where((parameter, i) => !parameter.Matches(method.Parameters[i])).Any())
				.Cast<IInteraction>()
				.ToArray());

	/// <summary>
	///     A proxy implementation of <see cref="IMockInvoked{TMock}" /> that forwards all calls to the provided
	///     <paramref name="inner" /> instance.
	/// </summary>
	public class Proxy(IMockInvoked<TMock> inner, Checks checks, TMock mock)
		: MockInvoked<T, TMock>(checks, mock), IMockInvoked<TMock>
	{
		/// <inheritdoc cref="IMockInvoked{TMock}.Method(string, With.Parameter[])" />
		CheckResult<TMock> IMockInvoked<TMock>.Method(string methodName, params With.Parameter[] parameters)
			=> inner.Method(methodName, parameters);
	}

	/// <summary>
	///     Check which protected methods got invoked on the mocked instance <typeparamref name="TMock" />.
	/// </summary>
	public class Protected(IMockInvoked<TMock> inner, Checks checks, TMock mock)
		: MockInvoked<T, TMock>(checks, mock), IMockInvoked<TMock>
	{
		/// <inheritdoc cref="IMockInvoked{TMock}.Method(string, With.Parameter[])" />
		CheckResult<TMock> IMockInvoked<TMock>.Method(string methodName, params With.Parameter[] parameters)
			=> inner.Method(methodName, parameters);
	}
}
