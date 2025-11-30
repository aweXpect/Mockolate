using System;

namespace Mockolate;

/// <summary>
///     Defines a mechanism for generating default values of a specified type.
/// </summary>
public interface IDefaultValueGenerator
{
	/// <summary>
	///     Generates a default value of the specified <paramref name="type" />, with
	///     the <paramref name="parameters" /> for context.
	/// </summary>
	object? Generate(Type type, params object?[] parameters);
}
