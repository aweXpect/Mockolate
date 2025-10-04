namespace Mockolate;

/// <summary>
///     Provides helper methods for specifying constructor parameters when creating mocks of classes with parameterized constructors.
/// </summary>
public static class BaseClass
{
	/// <summary>
	///     Specifies constructor parameters for a mock of a class with parameterized constructors.
	/// </summary>
	public static ConstructorParameters WithConstructorParameters(params object?[]? parameters)
	{
		return new ConstructorParameters(parameters ?? [null]);
	}

	/// <summary>
	///     Represents a collection of parameters to be supplied to a constructor.
	/// </summary>
	/// <param name="Parameters">The parameters to be passed to the constructor.</param>
	public record ConstructorParameters(object?[] Parameters);
}
