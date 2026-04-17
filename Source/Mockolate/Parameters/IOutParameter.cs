using System;

namespace Mockolate.Parameters;

/// <summary>
///     Matches an <see langword="out" /> parameter of type <typeparamref name="T" /> against an expectation.
/// </summary>
public interface IOutParameter<T>
{
	/// <summary>
	///     Tries to get the value to which the <see langword="out" /> parameter should be set.
	/// </summary>
	/// <remarks>
	///     When the method returns <see langword="true" />, <paramref name="value" /> is the value the mock should
	///     write back to the <see langword="out" /> parameter. The value may legitimately be <see langword="null" /> if
	///     <typeparamref name="T" /> permits it (e.g., <c>It.IsOut(() =&gt; (string?)null)</c>).
	/// </remarks>
	bool TryGetValue(out T value);

	/// <summary>
	///     Registers a <paramref name="callback" /> to execute for matching parameters.
	/// </summary>
	IOutParameter<T> Do(Action<T> callback);
}
