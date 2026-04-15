using System;
using System.Collections.Generic;

namespace Mockolate.Parameters;

/// <summary>
///     A named parameter value.
/// </summary>
/// <param name="Name">The name of the parameter.</param>
/// <param name="Value">The parameter value.</param>
#if !DEBUG
[System.Diagnostics.DebuggerNonUserCode]
#endif
public record NamedParameterValue<T>(
	string Name,
	T Value) : INamedParameterValue
{
	/// <inheritdoc cref="INamedParameterValue.GetValueType()" />
	public Type GetValueType() => typeof(T);

	/// <inheritdoc cref="INamedParameterValue.TryGetValue{TValue}(out TValue)" />
	public bool TryGetValue<TValue>(out TValue value)
	{
		if (Value is TValue v)
		{
			value = v;
			return true;
		}

		if (Value is null && default(TValue) is null)
		{
			value = default!;
			return true;
		}

		value = default!;
		return false;
	}

	/// <inheritdoc cref="INamedParameterValue.Equals(INamedParameterValue)" />
	public bool Equals(INamedParameterValue other)
		=> string.Equals(Name, other.Name, StringComparison.Ordinal) && other.GetValueType() == GetValueType() &&
		   other.TryGetValue(out T otherValue) && EqualityComparer<T>.Default.Equals(Value, otherValue);

	/// <inheritdoc cref="INamedParameterValue.IsNull" />
	public bool IsNull => Value is null;

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> Value?.ToString() ?? "null";
}
