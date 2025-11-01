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
	public bool IsMatch(Type type);

	/// <summary>
	///     Creates a new instance of the specified type.
	/// </summary>
	public object? Create(Type type, IDefaultValueGenerator defaultValueGenerator);
}
