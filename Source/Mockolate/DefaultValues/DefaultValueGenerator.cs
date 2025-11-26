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
	private static readonly ConcurrentQueue<DefaultValueFactoryContainer> _factories = new([
		new DefaultValueFactoryContainer(new TypedDefaultValueFactory<string>("")),
		new DefaultValueFactoryContainer(new CancellableTaskFactory()),
#if NET8_0_OR_GREATER
		new DefaultValueFactoryContainer(new CancellableValueTaskFactory()),
#endif
		new DefaultValueFactoryContainer(new TypedDefaultValueFactory<CancellationToken>(CancellationToken.None)),
		new DefaultValueFactoryContainer(new TypedDefaultValueFactory<IEnumerable>(Array.Empty<object?>())),
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

	/// <inheritdoc cref="IDefaultValueGenerator.Generate(Type, object?[])" />
	public object? Generate(Type type, params object?[] parameters)
	{
		if (TryGenerate(type, parameters, out object? value))
		{
			return value;
		}

		return null;
	}

	/// <summary>
	///     Registers a <paramref name="defaultValueFactory" /> to provide default values for a specific type.
	/// </summary>
	public static IDisposable Register(IDefaultValueFactory defaultValueFactory)
	{
		DefaultValueFactoryContainer factory = new(defaultValueFactory);
		_factories.Enqueue(factory);
		return new DisposableAction(() =>
		{
			factory.IsActive = false;
		});
	}

	/// <summary>
	///     Tries to generate a default value for the specified type.
	/// </summary>
	protected virtual bool TryGenerate(Type type, object?[] parameters, out object? value)
	{
		IDefaultValueFactory? matchingFactory = _factories
			.Where(f => f.IsActive && f.Factory.IsMatch(type))
			.Select(f => f.Factory)
			.FirstOrDefault();
		if (matchingFactory is not null)
		{
			value = matchingFactory.Create(type, this, parameters);
			return true;
		}

		value = null;
		return false;
	}

	private static bool HasCanceledCancellationToken(object?[] parameters, out CancellationToken cancellationToken)
	{
		CancellationToken parameter = parameters.OfType<CancellationToken>().FirstOrDefault();
		if (parameter.IsCancellationRequested)
		{
			cancellationToken = parameter;
			return true;
		}

		cancellationToken = CancellationToken.None;
		return false;
	}

	private class DefaultValueFactoryContainer(IDefaultValueFactory factory)
	{
		public IDefaultValueFactory Factory { get; } = factory;
		public bool IsActive { get; set; } = true;
	}

	private struct DisposableAction(Action action) : IDisposable
	{
		public void Dispose()
			=> action();
	}

	private sealed class CancellableTaskFactory : IDefaultValueFactory
	{
		/// <inheritdoc cref="IDefaultValueFactory.IsMatch(Type)" />
		public bool IsMatch(Type type)
			=> type == typeof(Task);

		/// <inheritdoc cref="IDefaultValueFactory.Create(Type, IDefaultValueGenerator, object[])" />
		public object Create(Type type, IDefaultValueGenerator defaultValueGenerator, params object?[] parameters)
		{
			if (HasCanceledCancellationToken(parameters, out CancellationToken cancellationToken))
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
			if (HasCanceledCancellationToken(parameters, out CancellationToken cancellationToken))
			{
				return ValueTask.FromCanceled(cancellationToken);
			}

			return ValueTask.CompletedTask;
		}
	}
#endif
}
