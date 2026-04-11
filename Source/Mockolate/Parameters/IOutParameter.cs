using System;
using System.Diagnostics.CodeAnalysis;

namespace Mockolate.Parameters;

/// <summary>
///     Matches an <see langword="out" /> parameter of type <typeparamref name="T" /> against an expectation.
/// </summary>
public interface IOutParameter<T>
{
	/// <summary>
	///  Tries to get the value to which the <see langword="out" /> parameter should be set.
	/// </summary>
	bool TryGetValue([NotNullWhen(true)] out T? value);

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute for matching parameters.
	/// </summary>
	IOutParameter<T> Do(Action<T> callback);
}
