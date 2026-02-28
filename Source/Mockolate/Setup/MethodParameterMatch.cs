using System.Linq;
using Mockolate.Interactions;
using Mockolate.Internals;
using Mockolate.Parameters;

namespace Mockolate.Setup;

/// <summary>
///     Matches a method by name and parameters.
/// </summary>
/// <remarks>
///     During verification, the <paramref name="methodName" /> is compared to the method name of the method invocation,
///     and the <paramref name="parameters" /> are matched one by one against the corresponding parameter in the method
///     invocation.
/// </remarks>
public readonly struct MethodParameterMatch(string methodName, NamedParameter[] parameters) : IMethodMatch
{
	/// <inheritdoc cref="IMethodMatch.Matches(MethodInvocation)" />
	public bool Matches(MethodInvocation method)
		=> method.Name.Equals(methodName) &&
		   method.Parameters.Length == parameters.Length &&
		   !parameters
			   .Where((parameter, i) => !parameter.Matches(method.Parameters[i]))
			   .Any();

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> $"{methodName.SubstringAfterLast('.')}({string.Join(", ", parameters.Select(x => x.Parameter.ToString()))})";
}
