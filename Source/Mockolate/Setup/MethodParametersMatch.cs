using Mockolate.Interactions;
using Mockolate.Internals;
using Mockolate.Parameters;

namespace Mockolate.Setup;

/// <summary>
///     Matches a method by name and parameters.
/// </summary>
/// <remarks>
///     During verification, the <paramref name="methodName" /> is compared to the method name of the method invocation,
///     and the <paramref name="parameters" /> are matched against the parameters in the method invocation.
/// </remarks>
public readonly struct MethodParametersMatch(string methodName, IParameters parameters) : IMethodMatch
{
	/// <inheritdoc cref="IMethodMatch.Matches(MethodInvocation)" />
	public bool Matches(MethodInvocation methodInvocation)
		=> methodInvocation.Name.Equals(methodName) &&
		   parameters.Matches(methodInvocation.Parameters);

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> $"{methodName.SubstringAfterLast('.')}({parameters})";
}
