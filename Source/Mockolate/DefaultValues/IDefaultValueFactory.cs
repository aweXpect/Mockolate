using System;

namespace Mockolate.DefaultValues;

/// <summary>
///     Defines a factory for creating default values for a specified type.
/// </summary>
public interface IDefaultValueFactory
{
	/// <summary>
	///     Determines whether the specified <paramref name="type" /> can be created by this factory.
	/// </summary>
	bool IsMatch(Type type);

	/// <summary>
	///     Creates a new instance of the specified type.
	/// </summary>
	object? Create(Type type, IDefaultValueGenerator defaultValueGenerator, object?[] parameters);
}
