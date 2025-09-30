using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mockerade;

/// <summary>
/// The behavior of the mock.
/// </summary>
public record MockBehavior
{
	/// <summary>
	///     The default mock behavior settings.
	/// </summary>
	public static MockBehavior Default => _globalDefault;

	/// <summary>
	///     Specifies whether an exception is thrown when an operation is attempted without prior setup.
	/// </summary>
	/// <remarks>
	///     If set to <see langword="false"/>, the value from the <see cref="DefaultValueGenerator"/> is used for return values of methods or properties.
	/// </remarks>
	public bool ThrowWhenNotSetup { get; init; }

	/// <summary>
	///     The generator for default values when not specified by a setup.
	/// </summary>
	/// <remarks>
	///     If <see cref="ThrowWhenNotSetup"/> is not set to <see langword="false" />, an exception is thrown in such cases.<para/>
	///     The default implementation has a fixed set of objects with a not-<see langword="null" /> value:<br />
	///     - <see cref="Task"/><br />
	///     - <see cref="CancellationToken"/>
	/// </remarks>
	public IDefaultValueGenerator DefaultValueGenerator { get; init; }
		= new ReturnDefaultDefaultValueGenerator();

	private static MockBehavior _globalDefault = new MockBehavior();

	/// <summary>
	///     Defines a mechanism for generating default values of a specified type.
	/// </summary>
	public interface IDefaultValueGenerator
	{
		/// <summary>
		///     Generates a default value of the specified type.
		/// </summary>
		T Generate<T>();
	}

	private sealed class ReturnDefaultDefaultValueGenerator : IDefaultValueGenerator
	{
		private static readonly (Type Type, object Value)[] _defaultValues =
			[
				(typeof(Task), Task.CompletedTask),
				(typeof(CancellationToken), CancellationToken.None),
				// When changing this array, please also update the documentation
				// of <see cref="MockBehavior.DefaultValueGenerator"/>!
			];

		/// <inheritdoc cref="IDefaultValueGenerator.Generate{T}" />
		public T Generate<T>()
		{
			foreach (var defaultValue in _defaultValues)
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
