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
		new CancellableTaskFactory(),
#if NET8_0_OR_GREATER
		new CancellableValueTaskFactory(),
#endif
		new TypedDefaultValueFactory<CancellationToken>(CancellationToken.None),
		new TypedDefaultValueFactory<IEnumerable>(Array.Empty<object?>()),
	]);

	/// <inheritdoc cref="IDefaultValueGenerator.Generate{T}()" />
	public T Generate<T>()
		=> Generate<T>([]);

	/// <inheritdoc cref="IDefaultValueGenerator.Generate{T}(object?[])" />
	public T Generate<T>(params object?[] parameters)
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
	protected virtual bool TryGenerate(Type type, object?[] parameters, out object? value)
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

	private static bool HasCancellationToken(object?[] parameters, out CancellationToken cancellationToken)
	{
		CancellationToken? parameter = parameters.OfType<CancellationToken>().FirstOrDefault();
		if (parameter != null && parameter.Value.IsCancellationRequested)
		{
			cancellationToken = parameter.Value;
			return true;
		}

		cancellationToken = CancellationToken.None;
		return false;
	}

	private sealed class CancellableTaskFactory : IDefaultValueFactory
	{
		/// <inheritdoc cref="IDefaultValueFactory.IsMatch(Type)" />
		public bool IsMatch(Type type)
			=> type == typeof(Task);

		/// <inheritdoc cref="IDefaultValueFactory.Create(Type, IDefaultValueGenerator, object[])" />
		public object Create(Type type, IDefaultValueGenerator defaultValueGenerator, params object?[] parameters)
		{
			if (HasCancellationToken(parameters, out CancellationToken cancellationToken)
			    && cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled(cancellationToken);
			}

			return Task.CompletedTask;
		}
	}
#if NET8_0_OR_GREATER
	private sealed class CancellableValueTaskFactory : IDefaultValueFactory
	{
		/// <inheritdoc cref="IDefaultValueFactory.IsMatch(Type)" />
		public bool IsMatch(Type type)
			=> type == typeof(ValueTask);

		/// <inheritdoc cref="IDefaultValueFactory.Create(Type, IDefaultValueGenerator, object[])" />
		public object Create(Type type, IDefaultValueGenerator defaultValueGenerator, params object?[] parameters)
		{
			if (HasCancellationToken(parameters, out CancellationToken cancellationToken)
			    && cancellationToken.IsCancellationRequested)
			{
				return ValueTask.FromCanceled(cancellationToken);
			}

			return ValueTask.CompletedTask;
		}
	}
#endif
}
