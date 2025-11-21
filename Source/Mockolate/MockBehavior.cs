using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
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
			_initializers = _initializers ?? [],
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
			_initializers = _initializers ?? [],
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

	private interface IInitializer;

	private interface IInitializer<T> : IInitializer
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
}
