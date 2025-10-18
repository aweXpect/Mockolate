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
}
