using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Mockolate;

/// <summary>
///     The behavior of the mock.
/// </summary>
#if !DEBUG
[System.Diagnostics.DebuggerNonUserCode]
#endif
public record MockBehavior : IMockBehaviorAccess
{
	private ConcurrentStack<IConstructorParameters>? _constructorParameters;
	private ConcurrentStack<object?>? _values;

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
	///     Flag indicating whether interaction recording is skipped for performance.
	/// </summary>
	/// <remarks>
	///     If set to <see langword="false" /> (default value), every interaction with the mock is recorded and
	///     can be verified later.
	///     <para />
	///     When set to <see langword="true" />, the mock skips allocating interaction records, avoids the
	///     interaction-list lock, and does not raise the
	///     <see cref="Mockolate.Interactions.MockInteractions" /> added-event. Setups, returns, callbacks, and
	///     base-class delegation continue to work normally - only verification is disabled. Any attempt to
	///     verify throws a <see cref="Mockolate.Exceptions.MockException" />.
	/// </remarks>
	public bool SkipInteractionRecording { get; init; }

	/// <summary>
	///     The generator for default values when not specified by a setup.
	/// </summary>
	/// <remarks>
	///     If <see cref="ThrowWhenNotSetup" /> is not set to <see langword="false" />, an exception is thrown in such cases.
	/// </remarks>
	public IDefaultValueGenerator DefaultValue { get; init; }

	MockBehavior IMockBehaviorAccess.Set<T>(T value)
	{
		MockBehavior behavior = this with
		{
			_values = new ConcurrentStack<object?>(_values ?? []),
		};
		behavior._values.Push(value);
		return behavior;
	}

	bool IMockBehaviorAccess.TryGet<T>([NotNullWhen(true)] out T value)
	{
		if (_values?.FirstOrDefault(i => i is T)
		    is not T typedValue)
		{
			value = default!;
			return false;
		}

		value = typedValue;
		return true;
	}

	/// <inheritdoc cref="IMockBehaviorAccess.TryGetConstructorParameters{T}(out object?[])" />
	bool IMockBehaviorAccess.TryGetConstructorParameters<T>([NotNullWhen(true)] out object?[]? parameters)
	{
		if (_constructorParameters?.FirstOrDefault(i => i is ConstructorParameters<T>)
		    is not ConstructorParameters<T> constructorParameters)
		{
			parameters = null;
			return false;
		}

		parameters = constructorParameters.GetParameters();
		return true;
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
			_constructorParameters = new ConcurrentStack<IConstructorParameters>(_constructorParameters ?? []),
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
			_constructorParameters = new ConcurrentStack<IConstructorParameters>(_constructorParameters ?? []),
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

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
	{
		List<string>? parts = null;
		if (ThrowWhenNotSetup)
		{
			(parts ??= []).Add(nameof(MockBehaviorExtensions.ThrowingWhenNotSetup));
		}

		if (SkipBaseClass)
		{
			(parts ??= []).Add(nameof(MockBehaviorExtensions.SkippingBaseClass));
		}

		if (SkipInteractionRecording)
		{
			(parts ??= []).Add(nameof(MockBehaviorExtensions.SkippingInteractionRecording));
		}

		string baseString = parts is null
			? "Default"
			: string.Join(" and ", parts);

		if (_constructorParameters is not null)
		{
			baseString += $" with {_constructorParameters.Count} constructor parameter registrations";
		}

		if (_values is not null)
		{
			baseString += $" with {_values.Count} setup registrations";
		}

		return baseString;
	}

	private interface IConstructorParameters;

#pragma warning disable S2326
	// ReSharper disable once UnusedTypeParameter
	private sealed class ConstructorParameters<T>(Func<object?[]> parameters) : IConstructorParameters
	{
		public object?[] GetParameters() => parameters();
	}
#pragma warning restore S2326

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
