using Mockolate.DefaultValues;

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
}
