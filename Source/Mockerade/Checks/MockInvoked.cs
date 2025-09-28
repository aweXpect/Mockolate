using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mockerade.Setup;

namespace Mockerade.Checks;

/// <summary>
///     The invocations of the <see cref="Mock{T}" />
/// </summary>
public class MockInvoked<T>(MockInvocations invocations) : IMockInvoked
{
	/// <summary>
	/// A proxy implementation of <see cref="IMockInvoked"/> that forwards all calls to the provided <paramref name="inner"/> instance.
	/// </summary>
	public class Proxy(IMockInvoked inner, MockInvocations invocations) : MockInvoked<T>(invocations), IMockInvoked
	{
		/// <inheritdoc cref="IMockInvoked.Method(string, With.Parameter[])" />
		Invocation[] IMockInvoked.Method(string methodName, params With.Parameter[] parameters)
			=> inner.Method(methodName, parameters);
	}

	/// <inheritdoc cref="IMockInvoked.Method(string, With.Parameter[])"/>
	Invocation[] IMockInvoked.Method(string methodName, params With.Parameter[] parameters)
	{
		return invocations.Invocations
			.OfType<MethodInvocation>()
			.Where(method =>
				method.Name.Equals(methodName) &&
				method.Parameters.Length == parameters.Length &&
				!parameters.Where((parameter, i) => !parameter.Matches(method.Parameters[i])).Any())
			.ToArray();
	}
}
