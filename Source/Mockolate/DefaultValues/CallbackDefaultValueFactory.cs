
using System;
using System.IO;

namespace Mockolate.DefaultValues;

/// <summary>
///     A <see cref="IDefaultValueFactory"/> that uses the <paramref name="callback"/> to create a value for the given type parameter <typeparamref name="T"/>.
/// </summary>
public class CallbackDefaultValueFactory<T>(
	Func<IDefaultValueGenerator, T> callback,
	Func<Type, bool>? isMatch = null) : IDefaultValueFactory
{
	/// <inheritdoc cref="IDefaultValueFactory.IsMatch(Type)" />
	public bool IsMatch(Type type)
		=> isMatch?.Invoke(type) ?? type == typeof(T);

	/// <inheritdoc cref="IDefaultValueFactory.Create(Type, IDefaultValueGenerator)" />
	public object? Create(Type type, IDefaultValueGenerator defaultValueGenerator)
		=> callback(defaultValueGenerator);
}
