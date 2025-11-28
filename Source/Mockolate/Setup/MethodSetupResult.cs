using System;

namespace Mockolate.Setup;

/// <summary>
///     A result of a method setup invocation.
/// </summary>
public class MethodSetupResult(IMethodSetup? setup, MockBehavior behavior)
{
	/// <summary>
	///     Flag indicating if the method setup result has an underlying setup.
	/// </summary>
	public bool HasSetupResult
		=> setup is not null && setup.HasReturnCalls();

	/// <summary>
	///     Gets the flag indicating if the base class implementation should be called, and its return values
	///     used as default values.
	/// </summary>
	public bool CallBaseClass
		=> setup?.CallBaseClass() ?? behavior.CallBaseClass;

	/// <summary>
	///     Sets an <see langword="out" /> parameter with the specified name and returns its generated value of type
	///     <typeparamref name="T" />.
	/// </summary>
	/// <remarks>
	///     If a setup is configured, the value is generated according to the setup; otherwise, a default value
	///     is generated using the current behavior.
	/// </remarks>
	public T SetOutParameter<T>(string parameterName, Func<T> defaultValueGenerator)
	{
		if (setup is not null)
		{
			return setup.SetOutParameter(parameterName, defaultValueGenerator);
		}

		return defaultValueGenerator();
	}

	/// <summary>
	///     Sets an <see langword="ref" /> parameter with the specified name and the initial <paramref name="value" /> and
	///     returns its generated value of type <typeparamref name="T" />.
	/// </summary>
	public T SetRefParameter<T>(string parameterName, T value)
	{
		if (setup is not null)
		{
			return setup.SetRefParameter(parameterName, value, behavior);
		}

		return value;
	}

	/// <summary>
	///     Triggers any configured parameter callbacks for the method setup with the specified <paramref name="parameters" />.
	/// </summary>
	public void TriggerCallbacks(params object?[]? parameters)
		=> setup?.TriggerCallbacks(parameters ?? [null,]);
}

/// <summary>
///     A result of a method setup invocation with return type <typeparamref name="TResult" />.
/// </summary>
public class MethodSetupResult<TResult>(IMethodSetup? setup, MockBehavior behavior, TResult result)
	: MethodSetupResult(setup, behavior)
{
	/// <summary>
	///     The return value of the setup method.
	/// </summary>
	public TResult Result { get; } = result;
}
