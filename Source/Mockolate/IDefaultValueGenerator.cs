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
	object? GenerateValue(Type type, params object?[] parameters);
}

internal static class DefaultValueGeneratorInternalExtensions
{
	internal static T GenerateTyped<T>(this IDefaultValueGenerator generator)
	{
		if (generator.GenerateValue(typeof(T)) is T value)
		{
			return value;
		}

		return default!;
	}
}
