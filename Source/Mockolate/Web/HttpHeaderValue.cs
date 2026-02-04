namespace Mockolate.Web;

/// <summary>
///     The value of an HTTP header.
/// </summary>
public class HttpHeaderValue
{
	private readonly string _value;

	/// <inheritdoc cref="HttpHeaderValue" />
	public HttpHeaderValue(string value)
	{
		_value = value;
	}

	/// <summary>
	///     Checks whether the given header value matches this value.
	/// </summary>
	public virtual bool Matches(string headerValue)
		=> _value.Equals(headerValue);

	/// <summary>
	///     Implicitly converts a string to an <see cref="HttpHeaderValue" />.
	/// </summary>
	public static implicit operator HttpHeaderValue(string value)
	{
		return new HttpHeaderValue(value);
	}
}
