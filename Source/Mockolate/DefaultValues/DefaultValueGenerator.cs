using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mockolate.DefaultValues;

/// <summary>
///     Provides default values for common types used in mocking scenarios.
/// </summary>
public class DefaultValueGenerator : IDefaultValueGenerator
{
	private static readonly ConcurrentQueue<IDefaultValueFactory> _factories = new([
		new TypedDefaultValueFactory<string>(""),
		CancellableTaskFactory.ForTask(),
#if !NETSTANDARD2_0
		CancellableTaskFactory.ForValueTask(),
#endif
		new TypedDefaultValueFactory<CancellationToken>(CancellationToken.None),
		new TypedDefaultValueFactory<IEnumerable>(Array.Empty<object?>()),
	]);

	/// <inheritdoc cref="IDefaultValueGenerator.Generate{T}()" />
	public T Generate<T>()
	{
		return Generate<T>(null);
	}

	/// <summary>
	///     Generates a default value of the specified type, with optional parameters for context.
	/// </summary>
	public T Generate<T>(params object?[]? parameters)
	{
		if (TryGenerate(typeof(T), parameters, out object? value) &&
		    value is T typedValue)
		{
			return typedValue;
		}

		return default!;
	}

	/// <summary>
	///     Registers a <paramref name="defaultValueFactory" /> to provide default values for a specific type.
	/// </summary>
	public static void Register(IDefaultValueFactory defaultValueFactory)
		=> _factories.Enqueue(defaultValueFactory);

	/// <summary>
	///     Tries to generate a default value for the specified type.
	/// </summary>
	protected virtual bool TryGenerate(Type type, object?[]? parameters, out object? value)
	{
		IDefaultValueFactory? matchingFactory = _factories.FirstOrDefault(f => f.IsMatch(type));
		if (matchingFactory is not null)
		{
			value = matchingFactory.Create(type, this, parameters);
			return true;
		}

		value = null;
		return false;
	}
}
