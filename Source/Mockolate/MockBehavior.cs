using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Mockolate.Setup;

namespace Mockolate;

/// <summary>
///     The behavior of the mock.
/// </summary>
public record MockBehavior : IMockBehaviorAccess
{
	private ConcurrentStack<IConstructorParameters> _constructorParameters = [];
	private ConcurrentStack<IInitializer>? _initializers;

	/// <inheritdoc cref="MockBehavior" />
	public MockBehavior(IDefaultValueGenerator defaultValue)
	{
		DefaultValue = defaultValue;
	}

	/// <summary>
	///     Specifies whether an exception is thrown when an operation is attempted without prior setup.
	/// </summary>
	/// <remarks>
	///     If set to <see langword="false" />, the value from the <see cref="DefaultValue" /> is used for return
	///     values of methods or properties.
	/// </remarks>
	public bool ThrowWhenNotSetup { get; init; }

	/// <summary>
	///     Flag indicating if the base class implementation should be skipped.
	/// </summary>
	/// <remarks>
	///     If set to <see langword="false" /> (default value), the base class implementation gets called and
	///     its return values are used as default values.
	/// </remarks>
	public bool SkipBaseClass { get; init; }

	/// <summary>
	///     The generator for default values when not specified by a setup.
	/// </summary>
	/// <remarks>
	///     If <see cref="ThrowWhenNotSetup" /> is not set to <see langword="false" />, an exception is thrown in such cases.
	/// </remarks>
	public IDefaultValueGenerator DefaultValue { get; init; }

	/// <inheritdoc cref="IMockBehaviorAccess.TryInitialize{T}(out Action{IMockSetup{T}}[])" />
	bool IMockBehaviorAccess.TryInitialize<T>([NotNullWhen(true)] out Action<IMockSetup<T>>[]? setups)
	{
		if (_initializers?.FirstOrDefault(i => i is IInitializer<T>)
		    is not IInitializer<T> initializer)
		{
			setups = null;
			return false;
		}

		setups = initializer.GetSetups();
		return true;
	}

	/// <inheritdoc cref="IMockBehaviorAccess.TryGetConstructorParameters{T}(out object?[])" />
	bool IMockBehaviorAccess.TryGetConstructorParameters<T>([NotNullWhen(true)] out object?[]? parameters)
	{
		if (_constructorParameters.FirstOrDefault(i => i is ConstructorParameters<T>)
		    is not ConstructorParameters<T> constructorParameters)
		{
			parameters = null;
			return false;
		}

		parameters = constructorParameters.GetParameters();
		return true;
	}

	/// <summary>
	///     Initialize all mocks of type <typeparamref name="T" /> with the given <paramref name="setups" />.
	/// </summary>
	public MockBehavior Initialize<T>(params Action<IMockSetup<T>>[] setups)
	{
		MockBehavior behavior = this with
		{
			_initializers = new ConcurrentStack<IInitializer>(_initializers ?? []),
		};
		behavior._initializers.Push(new SimpleInitializer<T>(setups));
		return behavior;
	}

	/// <summary>
	///     Initialize all mocks of type <typeparamref name="T" /> with the given <paramref name="setups" />.
	/// </summary>
	/// <remarks>
	///     Provides a unique counter for each generated mock as first parameter.
	/// </remarks>
	public MockBehavior Initialize<T>(params Action<int, IMockSetup<T>>[] setups)
	{
		MockBehavior behavior = this with
		{
			_initializers = new ConcurrentStack<IInitializer>(_initializers ?? []),
		};
		behavior._initializers.Push(new CounterInitializer<T>(setups));
		return behavior;
	}

	/// <summary>
	///     Initialize all mocks of type <typeparamref name="T" /> to use the given constructor <paramref name="parameters" />.
	/// </summary>
	/// <remarks>
	///     These parameters are only used when no explicit constructor parameters are provided when creating the mock.
	/// </remarks>
	public MockBehavior UseConstructorParametersFor<T>(Func<object?[]> parameters)
	{
		MockBehavior behavior = this with
		{
			_constructorParameters = new ConcurrentStack<IConstructorParameters>(_constructorParameters),
		};
		behavior._constructorParameters.Push(new ConstructorParameters<T>(parameters));
		return behavior;
	}

	/// <summary>
	///     Initialize all mocks of type <typeparamref name="T" /> to use the given constructor <paramref name="parameters" />.
	/// </summary>
	/// <remarks>
	///     These parameters are only used when no explicit constructor parameters are provided when creating the mock.
	/// </remarks>
	public MockBehavior UseConstructorParametersFor<T>(params object?[] parameters)
	{
		MockBehavior behavior = this with
		{
			_constructorParameters = new ConcurrentStack<IConstructorParameters>(_constructorParameters),
		};
		behavior._constructorParameters.Push(new ConstructorParameters<T>(() => parameters));
		return behavior;
	}

	/// <summary>
	///     Uses the given <paramref name="defaultValueFactories" /> to create default values for supported types.
	/// </summary>
	public MockBehavior WithDefaultValueFor(params DefaultValueFactory[] defaultValueFactories)
	{
		MockBehavior behavior = this with
		{
			DefaultValue = new DefaultValueGeneratorWithFactories(DefaultValue, defaultValueFactories),
		};
		return behavior;
	}

	private interface IInitializer;

	private interface IInitializer<in T> : IInitializer
	{
		Action<IMockSetup<T>>[] GetSetups();
	}

	private interface IConstructorParameters;

#pragma warning disable S2326
	// ReSharper disable once UnusedTypeParameter
	private sealed class ConstructorParameters<T>(Func<object?[]> parameters) : IConstructorParameters
	{
		public object?[] GetParameters() => parameters();
	}
#pragma warning restore S2326

	private sealed class SimpleInitializer<T>(Action<IMockSetup<T>>[] setups) : IInitializer<T>
	{
		public Action<IMockSetup<T>>[] GetSetups()
			=> setups;
	}

	private sealed class CounterInitializer<T>(Action<int, IMockSetup<T>>[] setups) : IInitializer<T>
	{
		private int _counter;

		public Action<IMockSetup<T>>[] GetSetups()
		{
			int index = Interlocked.Increment(ref _counter);
			return setups.Select(a => new Action<IMockSetup<T>>(s => a(index, s))).ToArray();
		}
	}

	private sealed class DefaultValueGeneratorWithFactories(
		IDefaultValueGenerator inner,
		DefaultValueFactory[] factories)
		: IDefaultValueGenerator
	{
		public object? GenerateValue(Type type, params object?[] parameters)
		{
			DefaultValueFactory? factory = factories.FirstOrDefault(f => f.CanGenerateValue(type));
			if (factory is not null)
			{
				return factory.GenerateValue(type, parameters);
			}

			return inner.GenerateValue(type, parameters);
		}
	}
}
