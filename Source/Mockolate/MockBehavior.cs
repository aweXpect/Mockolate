using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Mockolate;

/// <summary>
///     The behavior of the mock.
/// </summary>
public record MockBehavior
{
	/// <summary>
	///     The default mock behavior settings.
	/// </summary>
	public static MockBehavior Default { get; } = new();

	/// <summary>
	///     Specifies whether an exception is thrown when an operation is attempted without prior setup.
	/// </summary>
	/// <remarks>
	///     If set to <see langword="false" />, the value from the <see cref="DefaultValueGenerator" /> is used for return
	///     values of methods or properties.
	/// </remarks>
	public bool ThrowWhenNotSetup { get; init; }

	/// <summary>
	///     The generator for default values when not specified by a setup.
	/// </summary>
	/// <remarks>
	///     If <see cref="ThrowWhenNotSetup" /> is not set to <see langword="false" />, an exception is thrown in such cases.
	///     <para />
	///     The default implementation has a fixed set of objects with a not-<see langword="null" /> value:<br />
	///     - <see cref="Task" /><br />
	///     - <see cref="CancellationToken" />
	/// </remarks>
	public IDefaultValueGenerator DefaultValueGenerator { get; init; }
		= new ReturnDefaultValueGenerator();

	/// <summary>
	///     Defines a mechanism for generating default values of a specified type.
	/// </summary>
	public interface IDefaultValueGenerator
	{
		/// <summary>
		///     Generates a default value of the specified type.
		/// </summary>
		T Generate<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>();
	}

	private sealed class ReturnDefaultValueGenerator : IDefaultValueGenerator
	{
		private static readonly (Type Type, object Value)[] _defaultValues =
		[
			(typeof(Task), Task.CompletedTask),
			(typeof(CancellationToken), CancellationToken.None),
			// When changing this array, please also update the documentation
			// of <see cref="MockBehavior.DefaultValueGenerator"/>!
		];

		object CreateValueTupleOf([System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicConstructors)] Type type)
		{
			var itemTypes = type.GetGenericArguments();
			var items = new object[itemTypes.Length];
			for (int i = 0, n = itemTypes.Length; i < n; ++i)
			{
				items[i] = this.Generate(itemTypes[i]);
			}
			// Fix CS8603 by using null-forgiving operator and IL2067 by adding annotation above
			return Activator.CreateInstance(type, items)!;
		}
		object Generate(Type type)
		{
			foreach ((Type Type, object Value) defaultValue in _defaultValues)
			{
				if (defaultValue.Type == type)
				{
					return defaultValue.Value;
				}
			}

			return default!;
		}

		/// <inheritdoc cref="IDefaultValueGenerator.Generate{T}" />
		public T Generate<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>()
		{
			if (typeof(T) == typeof(ValueTuple<,>) &&
				CreateValueTupleOf(typeof(T)) is T tupleValue)
			{
				return tupleValue;
			}

			foreach ((Type Type, object Value) defaultValue in _defaultValues)
			{
				if (defaultValue.Value is T value &&
				    defaultValue.Type == typeof(T))
				{
					return value;
				}
			}

			return default!;
		}
	}
}
