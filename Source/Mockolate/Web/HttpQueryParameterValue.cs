using System;

namespace Mockolate.Web;

/// <summary>
///     The value of an HTTP query parameter.
/// </summary>
public class HttpQueryParameterValue
{
	private readonly string _value;

	/// <inheritdoc cref="HttpQueryParameterValue" />
	public HttpQueryParameterValue(string value)
	{
		_value = value;
	}

	/// <summary>
	///     Checks whether the given query parameter value matches this value.
	/// </summary>
	public virtual bool Matches(string parameterValue)
		=> _value.Equals(parameterValue, StringComparison.Ordinal);

	/// <summary>
	///     Implicitly converts a string to an <see cref="HttpQueryParameterValue" />.
	/// </summary>
	public static implicit operator HttpQueryParameterValue(string value)
	{
		return new HttpQueryParameterValue(value);
	}
}
