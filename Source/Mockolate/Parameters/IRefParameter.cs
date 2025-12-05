using System;

namespace Mockolate.Parameters;

/// <summary>
///     Matches an <see langword="ref" /> parameter of type <typeparamref name="T" /> against an expectation.
/// </summary>
public interface IRefParameter<T>
{
	/// <summary>
	///     Retrieves the value to which the <see langword="out" /> parameter should be set.
	/// </summary>
	T GetValue(T value);

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute for matching parameters.
	/// </summary>
	IRefParameter<T> Do(Action<T> callback);
}
