using System;
using System.Diagnostics.CodeAnalysis;
using Mockolate.Interactions;
using Mockolate.Internals;
using Mockolate.Parameters;

namespace Mockolate.Setup;

/// <summary>
///     Base class for method setups.
/// </summary>
public abstract class MethodSetup : IInteractiveMethodSetup
{
	/// <inheritdoc cref="IInteractiveMethodSetup.HasReturnCalls()" />
	bool IInteractiveMethodSetup.HasReturnCalls()
		=> HasReturnCalls();

	/// <inheritdoc cref="IInteractiveMethodSetup.SetOutParameter{T}(string, Func{T})" />
	T IInteractiveMethodSetup.SetOutParameter<T>(string parameterName, Func<T> defaultValueGenerator)
		=> SetOutParameter(parameterName, defaultValueGenerator);

	/// <inheritdoc cref="IInteractiveMethodSetup.SetRefParameter{T}(string, T, MockBehavior)" />
	T IInteractiveMethodSetup.SetRefParameter<T>(string parameterName, T value, MockBehavior behavior)
		=> SetRefParameter(parameterName, value, behavior);

	/// <inheritdoc cref="IInteractiveMethodSetup.Matches(MethodInvocation)" />
	bool IInteractiveMethodSetup.Matches(MethodInvocation methodInvocation)
		=> IsMatch(methodInvocation);

	/// <inheritdoc cref="IInteractiveMethodSetup.SkipBaseClass()" />
	bool? IInteractiveMethodSetup.SkipBaseClass()
		=> GetSkipBaseClass();


	/// <inheritdoc cref="IInteractiveMethodSetup.Invoke{TResult}(MethodInvocation, MockBehavior, Func{TResult})" />
	TResult IInteractiveMethodSetup.Invoke<TResult>(MethodInvocation methodInvocation, MockBehavior behavior,
		Func<TResult> defaultValueGenerator)
	{
		ExecuteCallback(methodInvocation, behavior);
		return GetReturnValue(methodInvocation, behavior, defaultValueGenerator);
	}

	/// <inheritdoc cref="IInteractiveMethodSetup.Invoke(MethodInvocation, MockBehavior)" />
	void IInteractiveMethodSetup.Invoke(MethodInvocation methodInvocation, MockBehavior behavior)
		=> ExecuteCallback(methodInvocation, behavior);

	/// <inheritdoc cref="IInteractiveMethodSetup.TriggerCallbacks(object?[])" />
	public void TriggerCallbacks(object?[] parameters)
		=> TriggerParameterCallbacks(parameters);

	/// <summary>
	///     Gets the flag indicating if the base class implementation should be skipped.
	/// </summary>
	protected abstract bool? GetSkipBaseClass();

	/// <summary>
	///     Gets a value indicating whether this setup has return calls configured.
	/// </summary>
	protected virtual bool HasReturnCalls() => false;

	/// <summary>
	///     Sets an <see langword="out" /> parameter with the specified name and returns its generated value of type
	///     <typeparamref name="T" />.
	/// </summary>
	/// <remarks>
	///     If a setup is configured, the value is generated according to the setup; otherwise, a default value
	///     is generated using the current <paramref name="defaultValueGenerator" />.
	/// </remarks>
	protected abstract T SetOutParameter<T>(string parameterName, Func<T> defaultValueGenerator);

	/// <summary>
	///     Sets an <see langword="ref" /> parameter with the specified name and the initial <paramref name="value" /> and
	///     returns its generated value of type <typeparamref name="T" />.
	/// </summary>
	/// <remarks>
	///     If a setup is configured, the value is generated according to the setup; otherwise, a default value
	///     is generated using the current <paramref name="behavior" />.
	/// </remarks>
	protected abstract T SetRefParameter<T>(string parameterName, T value, MockBehavior behavior);

	/// <summary>
	///     Execute a potentially registered callback.
	/// </summary>
	protected abstract void ExecuteCallback(MethodInvocation invocation, MockBehavior behavior);

	/// <summary>
	///     Gets the registered return value.
	/// </summary>
	protected abstract TResult GetReturnValue<TResult>(MethodInvocation invocation, MockBehavior behavior,
		Func<TResult> defaultValueGenerator);

	/// <summary>
	///     Checks if the <paramref name="invocation" /> matches the setup.
	/// </summary>
	protected abstract bool IsMatch(MethodInvocation invocation);

	/// <summary>
	///     Triggers any configured parameter callbacks for the method setup with the specified <paramref name="parameters" />.
	/// </summary>
	protected abstract void TriggerParameterCallbacks(object?[] parameters);

	/// <summary>
	///     Determines whether the specified collection of named parameters contains a reference parameter of the given name
	///     and type.
	/// </summary>
	protected static bool HasRefParameter<T>(NamedParameter?[] namedParameters, string parameterName,
		[NotNullWhen(true)] out IRefParameter<T>? parameter)
	{
		foreach (NamedParameter? namedParameter in namedParameters)
		{
			if (namedParameter is not null &&
			    namedParameter.Name.Equals(parameterName, StringComparison.Ordinal) &&
			    namedParameter.Parameter is IRefParameter<T> refParameter)
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
	protected static bool HasOutParameter<T>(NamedParameter?[] namedParameters, string parameterName,
		[NotNullWhen(true)] out IOutParameter<T>? parameter)
	{
		foreach (NamedParameter? namedParameter in namedParameters)
		{
			if (namedParameter is not null &&
			    namedParameter.Name.Equals(parameterName, StringComparison.Ordinal) &&
			    namedParameter.Parameter is IOutParameter<T> outParameter)
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
	protected static bool Matches(NamedParameter[] namedParameters, NamedParameterValue[] values)
	{
		if (namedParameters.Length != values.Length)
		{
			return false;
		}

		for (int i = 0; i < namedParameters.Length; i++)
		{
			if (!namedParameters[i].Matches(values[i]))
			{
				return false;
			}
		}

		return true;
	}

	/// <summary>
	///     Triggers the parameter callbacks for each value in the specified array according to
	///     the corresponding named parameter.
	/// </summary>
	protected static void TriggerCallbacks(NamedParameter?[] namedParameters, object?[] values)
	{
		if (namedParameters.Length != values.Length)
		{
			return;
		}

		for (int i = 0; i < namedParameters.Length; i++)
		{
			namedParameters[i]?.Parameter.InvokeCallbacks(values[i]);
		}
	}

	/// <summary>
	///     Attempts to cast the specified value to the type parameter <typeparamref name="T" />,
	///     returning a value that indicates whether the cast was successful.
	/// </summary>
	/// <remarks>
	///     If value is not of type <typeparamref name="T" /> and is not <see langword="null" />,
	///     result is set to the default value for type <typeparamref name="T" /> as provided
	///     by the <paramref name="behavior" />.
	/// </remarks>
	protected static bool TryCast<T>([NotNullWhen(false)] object? value, out T result, MockBehavior behavior)
	{
		if (value is T typedValue)
		{
			result = typedValue;
			return true;
		}

		result = default!;
		return value is null;
	}

	/// <summary>
	///     Returns a formatted string representation of the given <paramref name="type" />.
	/// </summary>
	protected static string FormatType(Type type)
		=> type.FormatType();
}
