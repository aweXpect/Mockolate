using System;

namespace Mockolate.Parameters;

/// <summary>
///     Matches an <see langword="out" /> parameter of type <typeparamref name="T" /> against an expectation.
/// </summary>
public interface IOutParameter<T>
{
	/// <summary>
	///     Retrieves the value to which the <see langword="out" /> parameter should be set.
	/// </summary>
	T GetValue(Func<T> defaultValue);

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute for matching parameters.
	/// </summary>
	IOutParameter<T> Do(Action<T> callback);
}
