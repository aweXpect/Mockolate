using System.Collections.Generic;

namespace Mockolate.Parameters;

/// <summary>
///     Provides monitoring capabilities for parameters of the specified type from a mocked method or indexer,
///     allowing inspection of actual matched values.
/// </summary>
public interface IParameterMonitor<out T>
{
	/// <summary>
	///     Verifies the interactions with the mocked subject of <typeparamref name="T" />.
	/// </summary>
	IReadOnlyList<T> Values { get; }
}
