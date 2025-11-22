namespace Mockolate.DefaultValues;

/// <summary>
///     Defines a mechanism for generating default values of a specified type.
/// </summary>
public interface IDefaultValueGenerator
{
	/// <summary>
	///     Generates a default value of the specified type.
	/// </summary>
	T Generate<T>();

	/// <summary>
	///     Generates a default value of the specified type, with the parameters for context.
	/// </summary>
	T Generate<T>(params object?[] parameters);
}
