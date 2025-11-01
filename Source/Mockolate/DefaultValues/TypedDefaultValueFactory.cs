using System;

namespace Mockolate.DefaultValues;

/// <summary>
///     A <see cref="IDefaultValueFactory" /> that returns a specified <paramref name="value" /> for the given type
///     parameter <typeparamref name="T" />.
/// </summary>
internal class TypedDefaultValueFactory<T>(T value) : IDefaultValueFactory
{
	/// <inheritdoc cref="IDefaultValueFactory.IsMatch(Type)" />
	public bool IsMatch(Type type)
		=> type == typeof(T);

	/// <inheritdoc cref="IDefaultValueFactory.Create(Type, IDefaultValueGenerator)" />
	public object? Create(Type type, IDefaultValueGenerator defaultValueGenerator)
		=> value;
}
