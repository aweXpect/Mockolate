using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mockolate.DefaultValues;

/// <summary>
///     Provides default values for common types used in mocking scenarios.
/// </summary>
public class DefaultValueGenerator : IDefaultValueGenerator
{
	private static List<IDefaultValueFactory> _factories = new()
	{
		new TypedDefaultValueFactory<string>(""),
		new TypedDefaultValueFactory<Task>(Task.CompletedTask),
		new TypedDefaultValueFactory<CancellationToken>(CancellationToken.None),
		new TypedDefaultValueFactory<IEnumerable>(Array.Empty<object?>()),
	};

	/// <inheritdoc cref="IDefaultValueGenerator.Generate{T}" />
	public T Generate<T>()
	{
		if (TryGenerate(typeof(T), out object? value) &&
			value is T typedValue)
		{
			return typedValue;
		}

		return default!;
	}

	/// <summary>
	///     Registers a <paramref name="defaultValueFactory"/> to provide default values for a specific type.
	/// </summary>
	public static void Register(IDefaultValueFactory defaultValueFactory)
	{
		_factories.Add(defaultValueFactory);
	}

	/// <summary>
	///     Tries to generate a default value for the specified type.
	/// </summary>
	protected virtual bool TryGenerate(Type type, out object? value)
	{
		foreach (var factory in _factories)
		{
			if (factory.IsMatch(type))
			{
				value = factory.Create(type, this);
				return true;
			}
		}

		value = null;
		return false;
	}
}
