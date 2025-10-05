using System.Linq;
using Mockolate.Checks.Interactions;

namespace Mockolate.Checks;

/// <summary>
///     Check which methods got invoked on the mocked instance for <typeparamref name="T" />.
/// </summary>
public class MockInvoked<T>(Checks checks) : IMockInvoked
{
	/// <inheritdoc cref="IMockInvoked.Method(string, With.Parameter[])" />
	CheckResult IMockInvoked.Method(string methodName, params With.Parameter[] parameters) => new(checks,
		checks.Interactions
			.OfType<MethodInvocation>()
			.Where(method =>
				method.Name.Equals(methodName) &&
				method.Parameters.Length == parameters.Length &&
				!parameters.Where((parameter, i) => !parameter.Matches(method.Parameters[i])).Any())
			.ToArray());

	/// <summary>
	///     A proxy implementation of <see cref="IMockInvoked" /> that forwards all calls to the provided
	///     <paramref name="inner" /> instance.
	/// </summary>
	public class Proxy(IMockInvoked inner, Checks checks) : MockInvoked<T>(checks), IMockInvoked
	{
		/// <inheritdoc cref="IMockInvoked.Method(string, With.Parameter[])" />
		CheckResult IMockInvoked.Method(string methodName, params With.Parameter[] parameters)
			=> inner.Method(methodName, parameters);
	}

	/// <summary>
	///     Check which protected methods got invoked on the mocked instance for <typeparamref name="T" />.
	/// </summary>
	public class Protected(IMockInvoked inner, Checks checks) : MockInvoked<T>(checks), IMockInvoked
	{
		/// <inheritdoc cref="IMockInvoked.Method(string, With.Parameter[])" />
		CheckResult IMockInvoked.Method(string methodName, params With.Parameter[] parameters)
			=> inner.Method(methodName, parameters);
	}
}
