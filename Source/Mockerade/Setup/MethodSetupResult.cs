namespace Mockerade.Setup;

/// <summary>
/// A result of a method setup invocation.
/// </summary>
public class MethodSetupResult(IMethodSetup? setup, MockBehavior behavior)
{
	/// <summary>
	/// Sets an <see langword="out" /> parameter with the specified name and returns its generated value of type <typeparamref name="T"/>.
	/// </summary>
	/// <remarks>
	/// If a setup is configured, the value is generated according to the setup; otherwise, a default value
	/// is generated using the current behavior.
	/// </remarks>
	public T SetOutParameter<T>(string parameterName)
	{
		if (setup is not null)
		{
			return setup.SetOutParameter<T>(parameterName, behavior);
		}

		return behavior.DefaultValueGenerator.Generate<T>();
	}

	/// <summary>
	/// Sets an <see langword="ref" /> parameter with the specified name and the initial <paramref name="value"/> and returns its generated value of type <typeparamref name="T"/>.
	/// </summary>
	public T SetRefParameter<T>(string parameterName, T value)
	{
		if (setup is not null)
		{
			return setup.SetRefParameter<T>(parameterName, value, behavior);
		}

		return behavior.DefaultValueGenerator.Generate<T>();
	}
}

/// <summary>
/// A result of a method setup invocation with return type <typeparamref name="TResult"/>.
/// </summary>
public class MethodSetupResult<TResult>(IMethodSetup? setup, MockBehavior behavior, TResult result) : MethodSetupResult(setup, behavior)
{
	/// <summary>
	/// The return value of the setup method.
	/// </summary>
	public TResult Result { get; } = result;
}
