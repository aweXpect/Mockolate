using System;

namespace Mockolate.Parameters;

/// <summary>
///     A named parameter value.
/// </summary>
public interface INamedParameterValue
{
	/// <summary>
	///     The name of the parameter.
	/// </summary>
	string Name { get; }

	/// <summary>
	///     Flag indicating if the parameter value is <see langword="null" />.
	/// </summary>
	bool IsNull { get; }

	/// <summary>
	///     Gets the type of the value.
	/// </summary>
	Type GetValueType();

	/// <summary>
	///     Tries to get the value of the parameter as type <typeparamref name="TValue" />.
	///     Returns <see langword="true" /> if the value is of type <typeparamref name="TValue" /> or can be cast to it,
	///     or if the value is <see langword="null" /> and <typeparamref name="TValue" /> is a nullable type.
	///     Returns <see langword="false" /> otherwise.
	/// </summary>
	bool TryGetValue<TValue>(out TValue value);

	/// <summary>
	///     Compares this instance to another <see cref="INamedParameterValue" /> for equality.
	/// </summary>
	bool Equals(INamedParameterValue other);
}
