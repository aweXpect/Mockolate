using System;
using System.Diagnostics.CodeAnalysis;
using Mockolate.Exceptions;
using Mockolate.Interactions;
using Mockolate.Internals;

namespace Mockolate.Setup;

/// <summary>
///     Base class for method setups.
/// </summary>
public abstract class MethodSetup : IMethodSetup
{
	/// <inheritdoc cref="IMethodSetup.SetOutParameter{T}(string, MockBehavior)" />
	T IMethodSetup.SetOutParameter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(string parameterName, MockBehavior behavior)
		=> SetOutParameter<T>(parameterName, behavior);

	/// <inheritdoc cref="IMethodSetup.SetRefParameter{T}(string, T, MockBehavior)" />
	T IMethodSetup.SetRefParameter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(string parameterName, T value, MockBehavior behavior)
		=> SetRefParameter(parameterName, value, behavior);

	/// <inheritdoc cref="IMethodSetup.Matches(IInteraction)" />
	bool IMethodSetup.Matches(IInteraction invocation)
		=> invocation is MethodInvocation methodInvocation && IsMatch(methodInvocation);

	internal TResult Invoke<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TResult>(IInteraction invocation, MockBehavior behavior)
	{
		if (invocation is MethodInvocation methodInvocation)
		{
			ExecuteCallback(methodInvocation, behavior);
			return GetReturnValue<TResult>(methodInvocation, behavior);
		}

		throw new MockException("Invalid registered invocation for a method.");
	}

	internal void Invoke(IInteraction invocation, MockBehavior behavior)
	{
		if (invocation is MethodInvocation methodInvocation)
		{
			ExecuteCallback(methodInvocation, behavior);
		}
	}

	/// <summary>
	///     Sets an <see langword="out" /> parameter with the specified name and returns its generated value of type
	///     <typeparamref name="T" />.
	/// </summary>
	/// <remarks>
	///     If a setup is configured, the value is generated according to the setup; otherwise, a default value
	///     is generated using the current <paramref name="behavior" />.
	/// </remarks>
	protected abstract T SetOutParameter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(string parameterName, MockBehavior behavior);

	/// <summary>
	///     Sets an <see langword="ref" /> parameter with the specified name and the initial <paramref name="value" /> and
	///     returns its generated value of type <typeparamref name="T" />.
	/// </summary>
	/// <remarks>
	///     If a setup is configured, the value is generated according to the setup; otherwise, a default value
	///     is generated using the current <paramref name="behavior" />.
	/// </remarks>
	protected abstract T SetRefParameter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(string parameterName, T value, MockBehavior behavior);

	/// <summary>
	///     Execute a potentially registered callback.
	/// </summary>
	protected abstract void ExecuteCallback(MethodInvocation invocation, MockBehavior behavior);

	/// <summary>
	///     Gets the registered return value.
	/// </summary>
	protected abstract TResult GetReturnValue<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TResult>(MethodInvocation invocation, MockBehavior behavior);

	/// <summary>
	///     Checks if the <paramref name="invocation" /> matches the setup.
	/// </summary>
	protected abstract bool IsMatch(MethodInvocation invocation);

	/// <summary>
	///     Determines whether the specified collection of named parameters contains a reference parameter of the given name
	///     and type.
	/// </summary>
	protected static bool HasRefParameter<T>(With.NamedParameter[] namedParameters, string parameterName,
		[NotNullWhen(true)] out With.RefParameter<T>? parameter)
	{
		foreach (With.NamedParameter? namedParameter in namedParameters)
		{
			if (namedParameter.Name.Equals(parameterName, StringComparison.Ordinal) &&
			    namedParameter.Parameter is With.RefParameter<T> refParameter)
			{
				parameter = refParameter;
				return true;
			}
		}

		parameter = null;
		return false;
	}

	/// <summary>
	///     Determines whether the specified collection of named parameters contains an out parameter with the given name and
	///     type.
	/// </summary>
	protected static bool HasOutParameter<T>(With.NamedParameter[] namedParameters, string parameterName,
		[NotNullWhen(true)] out With.OutParameter<T>? parameter)
	{
		foreach (With.NamedParameter? namedParameter in namedParameters)
		{
			if (namedParameter.Name.Equals(parameterName, StringComparison.Ordinal) &&
			    namedParameter.Parameter is With.OutParameter<T> outParameter)
			{
				parameter = outParameter;
				return true;
			}
		}

		parameter = null;
		return false;
	}

	/// <summary>
	///     Determines whether each value in the specified array matches the corresponding named parameter according to the
	///     parameter's matching criteria.
	/// </summary>
	/// <remarks>
	///     The method returns false if the lengths of the namedParameters and values arrays do not match.
	///     Each value is compared to its corresponding named parameter using the parameter's matching logic.
	/// </remarks>
	protected static bool Matches(With.NamedParameter[] namedParameters, object?[] values)
	{
		if (namedParameters.Length != values.Length)
		{
			return false;
		}

		for (int i = 0; i < namedParameters.Length; i++)
		{
			if (!namedParameters[i].Parameter.Matches(values[i]))
			{
				return false;
			}
		}

		return true;
	}

	/// <summary>
	///     Attempts to cast the specified value to the type parameter <typeparamref name="T"/>,
	///     returning a value that indicates whether the cast was successful.
	/// </summary>
	/// <remarks>
	///     If value is not of type <typeparamref name="T" /> and is not <see langword="null" />,
	///     result is set to the default value for type <typeparamref name="T" /> as provided
	///     by the <paramref name="behavior" />.
	/// </remarks>
	protected static bool TryCast<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>([NotNullWhen(false)] object? value, out T result, MockBehavior behavior)
	{
		if (value is T typedValue)
		{
			result = typedValue;
			return true;
		}

		result = behavior.DefaultValueGenerator.Generate<T>();
		return value is null;
	}

	/// <summary>
	///     Returns a formatted string representation of the given <paramref name="type" />.
	/// </summary>
	protected static string FormatType(Type? type)
		=> type?.FormatType() ?? "null";
}
