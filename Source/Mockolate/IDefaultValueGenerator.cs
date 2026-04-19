using System;

namespace Mockolate;

/// <summary>
///     Defines a mechanism for generating default values of a specified type. Populated on
///     <see cref="MockBehavior.DefaultValue" /> and consulted whenever a mock needs a return value without a
///     matching setup.
/// </summary>
public interface IDefaultValueGenerator
{
	/// <summary>
	///     Generates a default value for <paramref name="type" />.
	/// </summary>
	/// <param name="type">The runtime type to produce a default value for.</param>
	/// <param name="parameters">
	///     Optional context passed through from a caller of a <see cref="DefaultValueFactory" />'s
	///     <c>Generate&lt;T&gt;(T nullValue, params object?[] parameters)</c> helper. The library itself does
	///     not populate this array; treat it as <see langword="null" />/empty unless your factory specifically
	///     relies on user-supplied context.
	/// </param>
	/// <returns>
	///     A default value assignable to <paramref name="type" />, or <see langword="null" /> if no value can be
	///     produced. The caller will fall back to the language default if the returned value is not assignment-compatible.
	/// </returns>
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
