using System;

namespace Mockolate.DefaultValues;

/// <summary>
///     Defines a mechanism for generating default values of a specified type.
/// </summary>
public interface IDefaultValueGenerator
{
	/// <summary>
	///     Generates a default value of type <typeparamref name="T" />.
	/// </summary>
	T Generate<T>();

	/// <summary>
	///     Generates a default value of type <typeparamref name="T" />, with
	///     the <paramref name="parameters" /> for context.
	/// </summary>
	T Generate<T>(params object?[] parameters);

	/// <summary>
	///     Generates a default value of the specified <paramref name="type" />, with
	///     the <paramref name="parameters" /> for context.
	/// </summary>
	object? Generate(Type type, params object?[] parameters);
}
