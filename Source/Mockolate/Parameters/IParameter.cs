using System;

namespace Mockolate.Parameters;

/// <summary>
///     Matches a method parameter against an expectation.
/// </summary>
public interface IParameter
{
	/// <summary>
	///     Checks if the <paramref name="value" /> matches the expectation.
	/// </summary>
	bool Matches(object? value);

	/// <summary>
	///     Invokes the callbacks registered for this parameter match.
	/// </summary>
	void InvokeCallbacks(object? value);
}

/// <summary>
///     Matches a method parameter of type <typeparamref name="T" /> against an expectation.
/// </summary>
public interface IParameter<out T>
{
	/// <summary>
	///     Registers a <paramref name="callback" /> to execute for matching parameters.
	/// </summary>
	IParameter<T> Do(Action<T> callback);
}
