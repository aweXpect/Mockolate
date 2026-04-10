using System;
using System.Diagnostics;
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
[DebuggerNonUserCode]
public readonly struct MethodParameterMatch(string methodName, NamedParameter[] parameters)
	: IMethodMatch, ITypedMethodMatch
{
	/// <inheritdoc cref="IMethodMatch.Matches(MethodInvocation)" />
	public bool Matches(MethodInvocation methodInvocation)
	{
		if (!methodInvocation.Name.Equals(methodName) ||
		    methodInvocation.Parameters.Length != parameters.Length)
		{
			return false;
		}

		for (int i = 0; i < parameters.Length; i++)
		{
			if (!parameters[i].Matches(methodInvocation.Parameters[i]))
			{
				return false;
			}
		}

		return true;
	}

	/// <inheritdoc cref="ITypedMethodMatch.MatchesTyped{T1}" />
	bool ITypedMethodMatch.MatchesTyped<T1>(string callMethodName, string n1, T1 v1)
	{
		if (!callMethodName.Equals(methodName) || parameters.Length != 1)
		{
			return false;
		}

		return MatchesParameter(parameters[0], n1, v1);
	}

	/// <inheritdoc cref="ITypedMethodMatch.MatchesTyped{T1,T2}" />
	bool ITypedMethodMatch.MatchesTyped<T1, T2>(
		string callMethodName, string n1, T1 v1, string n2, T2 v2)
	{
		if (!callMethodName.Equals(methodName) || parameters.Length != 2)
		{
			return false;
		}

		return MatchesParameter(parameters[0], n1, v1)
		    && MatchesParameter(parameters[1], n2, v2);
	}

	/// <inheritdoc cref="ITypedMethodMatch.MatchesTyped{T1,T2,T3}" />
	bool ITypedMethodMatch.MatchesTyped<T1, T2, T3>(
		string callMethodName, string n1, T1 v1, string n2, T2 v2, string n3, T3 v3)
	{
		if (!callMethodName.Equals(methodName) || parameters.Length != 3)
		{
			return false;
		}

		return MatchesParameter(parameters[0], n1, v1)
		    && MatchesParameter(parameters[1], n2, v2)
		    && MatchesParameter(parameters[2], n3, v3);
	}

	/// <inheritdoc cref="ITypedMethodMatch.MatchesTyped{T1,T2,T3,T4}" />
	bool ITypedMethodMatch.MatchesTyped<T1, T2, T3, T4>(
		string callMethodName, string n1, T1 v1, string n2, T2 v2, string n3, T3 v3, string n4, T4 v4)
	{
		if (!callMethodName.Equals(methodName) || parameters.Length != 4)
		{
			return false;
		}

		return MatchesParameter(parameters[0], n1, v1)
		    && MatchesParameter(parameters[1], n2, v2)
		    && MatchesParameter(parameters[2], n3, v3)
		    && MatchesParameter(parameters[3], n4, v4);
	}

	private static bool MatchesParameter<T>(NamedParameter namedParameter, string name, T value)
	{
		if (!string.IsNullOrEmpty(name) &&
		    !namedParameter.Name.Equals(name, StringComparison.Ordinal))
		{
			return false;
		}

		if (namedParameter.Parameter is ITypedParameter<T> typed)
		{
			return typed.MatchesValue(namedParameter.Name, value);
		}

		// Fallback for IParameter implementations that don't implement ITypedParameter<T>
		// (e.g., custom matchers, Web extension matchers).
		return namedParameter.Parameter.Matches(new NamedParameterValue<T>(name, value));
	}

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> $"{methodName.SubstringAfterLast('.')}({string.Join(", ", parameters.Select(x => x.Parameter.ToString()))})";
}
