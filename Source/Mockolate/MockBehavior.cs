using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Mockolate.DefaultValues;
using Mockolate.Setup;

namespace Mockolate;

/// <summary>
///     The behavior of the mock.
/// </summary>
public record MockBehavior
{
	private ConcurrentStack<IInitializer>? _initializers;

	/// <summary>
	///     The default mock behavior settings.
	/// </summary>
	public static MockBehavior Default { get; } = new();

	/// <summary>
	///     Specifies whether an exception is thrown when an operation is attempted without prior setup.
	/// </summary>
	/// <remarks>
	///     If set to <see langword="false" />, the value from the <see cref="DefaultValue" /> is used for return
	///     values of methods or properties.
	/// </remarks>
	public bool ThrowWhenNotSetup { get; init; }

	/// <summary>
	///     Flag indicating if the base class implementation should be called, and its return values used as default values.
	/// </summary>
	/// <remarks>
	///     Defaults to <see langword="false" />.
	/// </remarks>
	public bool CallBaseClass { get; init; }

	/// <summary>
	///     The generator for default values when not specified by a setup.
	/// </summary>
	/// <remarks>
	///     If <see cref="ThrowWhenNotSetup" /> is not set to <see langword="false" />, an exception is thrown in such cases.
	///     <para />
	///     Defaults to an instance of <see cref="DefaultValueGenerator" />.
	/// </remarks>
	public IDefaultValueGenerator DefaultValue { get; init; }
		= new DefaultValueGenerator();

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
	///     Tries to get the initialization setups for a mock of type <typeparamref name="T" />.
	/// </summary>
	/// <remarks>
	///     Returns <see langword="false" />, when no matching initialization is found.
	/// </remarks>
	public bool TryInitialize<T>([NotNullWhen(true)] out Action<IMockSetup<T>>[]? setups)
	{
		if (_initializers?
			    .FirstOrDefault(i => i is IInitializer<T>) is not IInitializer<T> initializer)
		{
			setups = null;
			return false;
		}

		setups = initializer.GetSetups();
		return true;
	}

	/// <summary>
	///     Use reflection-based default value generation for Tasks.
	/// </summary>
#if NET8_0_OR_GREATER
	[RequiresDynamicCode("Uses reflection to create generic methods at runtime.")]
#endif
	public static IDisposable UseReflectionBasedDefaultValues()
	{
		DisposableAction disposable = new();
		disposable.Add(DefaultValueGenerator.Register(new TaskDefaultValueFactory()));
#if NET8_0_OR_GREATER
		disposable.Add(DefaultValueGenerator.Register(new ValueTaskDefaultValueFactory()));
#endif
		return disposable;
	}

	private class DisposableAction : IDisposable
	{
		private readonly List<IDisposable> _disposables = [];

		public void Dispose()
			=> _disposables.ForEach(d => d.Dispose());

		public DisposableAction Add(IDisposable disposable)
		{
			_disposables.Add(disposable);
			return this;
		}
	}

	private interface IInitializer;

	private interface IInitializer<in T> : IInitializer
	{
		Action<IMockSetup<T>>[] GetSetups();
	}

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

#if NET8_0_OR_GREATER
	[RequiresDynamicCode("Uses reflection to create generic Tasks at runtime.")]
#endif
	private class TaskDefaultValueFactory : IDefaultValueFactory
	{
		public bool IsMatch(Type type)
			=> type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>);

		public object? Create(Type type, IDefaultValueGenerator defaultValueGenerator, object?[] parameters)
		{
			Type innerType = type.GetGenericArguments()[0];
			CancellationToken cancellationToken = parameters.OfType<CancellationToken>().FirstOrDefault();
			if (cancellationToken.IsCancellationRequested)
			{
				MethodInfo? method = typeof(Task).GetMethod(nameof(Task.FromCanceled));
				MethodInfo generic = method!.MakeGenericMethod(innerType);
				return generic.Invoke(this, [cancellationToken,]);
			}
			else
			{
				MethodInfo? method = typeof(Task).GetMethod(nameof(Task.FromResult));
				MethodInfo generic = method!.MakeGenericMethod(innerType);
				object? innerValue = defaultValueGenerator.Generate(innerType);
				return generic.Invoke(this, [innerValue,]);
			}
		}
	}

#if NET8_0_OR_GREATER
	[RequiresDynamicCode("Uses reflection to create generic ValueTasks at runtime.")]
	private class ValueTaskDefaultValueFactory : IDefaultValueFactory
	{
		public bool IsMatch(Type type)
			=> type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ValueTask<>);

		public object? Create(Type type, IDefaultValueGenerator defaultValueGenerator, object?[] parameters)
		{
			Type innerType = type.GetGenericArguments()[0];
			CancellationToken cancellationToken = parameters.OfType<CancellationToken>().FirstOrDefault();
			if (cancellationToken.IsCancellationRequested)
			{
				MethodInfo? method = typeof(ValueTask).GetMethod(nameof(ValueTask.FromCanceled));
				MethodInfo generic = method!.MakeGenericMethod(innerType);
				return generic.Invoke(this, [cancellationToken,]);
			}
			else
			{
				MethodInfo? method = typeof(ValueTask).GetMethod(nameof(ValueTask.FromResult));
				MethodInfo generic = method!.MakeGenericMethod(innerType);
				return generic.Invoke(this, [defaultValueGenerator.Generate(innerType),]);
			}
		}
	}
#endif
}
