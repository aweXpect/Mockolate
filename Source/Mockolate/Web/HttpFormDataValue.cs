using System;

namespace Mockolate.Web;

/// <summary>
///     The value of an HTTP form data parameter.
/// </summary>
public class HttpFormDataValue
{
	private readonly string _value;

	/// <inheritdoc cref="HttpFormDataValue" />
	public HttpFormDataValue(string value)
	{
		_value = value;
	}

	/// <summary>
	///     Checks whether the given form data parameter value matches this value.
	/// </summary>
	public virtual bool Matches(string parameterValue)
		=> _value.Equals(parameterValue, StringComparison.Ordinal);

	/// <summary>
	///     Implicitly converts a string to an <see cref="HttpFormDataValue" />.
	/// </summary>
	public static implicit operator HttpFormDataValue(string value)
	{
		return new HttpFormDataValue(value);
	}
}
