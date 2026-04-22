using System;

namespace Mockolate.Web;

/// <summary>
///     Expected value of a single query-string parameter, used with <c>It.IsUri().WithQuery(...)</c>.
/// </summary>
/// <remarks>
///     The base class performs ordinal string equality. Subclasses can match by wildcard, regex or a predicate. A
///     plain <see langword="string" /> is implicitly convertible for convenience.
/// </remarks>
public class HttpQueryParameterValue
{
	private readonly string _value;

	/// <inheritdoc cref="HttpQueryParameterValue" />
	/// <param name="value">The exact parameter value to match.</param>
	public HttpQueryParameterValue(string value)
	{
		_value = value;
	}

	/// <summary>
	///     Returns <see langword="true" /> when the observed <paramref name="parameterValue" /> matches this expectation.
	/// </summary>
	/// <param name="parameterValue">The URL-decoded value observed on the request URI.</param>
	/// <returns><see langword="true" /> when it matches; <see langword="false" /> otherwise.</returns>
	public virtual bool Matches(string parameterValue)
		=> _value.Equals(parameterValue, StringComparison.Ordinal);

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> _value;

	/// <summary>
	///     Implicit conversion from <see langword="string" /> to <see cref="HttpQueryParameterValue" /> &#8212; lets
	///     callers write a raw string in place of a wrapped value.
	/// </summary>
	/// <param name="value">The exact parameter value to match.</param>
	public static implicit operator HttpQueryParameterValue(string value)
	{
		return new HttpQueryParameterValue(value);
	}
}
