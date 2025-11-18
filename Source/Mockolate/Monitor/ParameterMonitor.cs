using System.Collections.Generic;

namespace Mockolate.Monitor;

/// <summary>
///     Provides monitoring capabilities for parameters of the specified type from a mocked method or indexer,
///     allowing inspection of actual matched values.
/// </summary>
public sealed class ParameterMonitor<T>
{
	private readonly List<T> _values = new();

	/// <inheritdoc cref="ParameterMonitor{T}" />
	public ParameterMonitor()
	{
		Values = _values.AsReadOnly();
	}
	
	/// <summary>
	///     Verifies the interactions with the mocked subject of <typeparamref name="T" />.
	/// </summary>
	public IReadOnlyList<T> Values { get; }

	internal void AddValue(T value)
	{
		_values.Add(value);
	}
}
