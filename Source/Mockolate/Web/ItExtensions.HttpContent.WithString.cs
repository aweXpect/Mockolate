using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using Mockolate.Internals;

namespace Mockolate.Web;

#pragma warning disable S2325 // Methods and properties that don't access instance data should be static
public static partial class ItExtensions
{
	/// <inheritdoc cref="IHttpContentParameter" />
	extension(IHttpContentParameter parameter)
	{
		/// <summary>
		///     Expects the content to have a string body equal to the <paramref name="expected" /> value.
		/// </summary>
		public IStringContentBodyParameter WithString(string expected)
		{
			StringMatcher data = new(expected, true);
			StringContentParameter contentParameter = new(parameter, data);
			return contentParameter;
		}

		/// <summary>
		///     Expects the content to have a string body that matches the given wildcard <paramref name="pattern" />.
		/// </summary>
		public IStringContentBodyMatchingParameter WithStringMatching(string pattern)
		{
			StringMatcher data = new(pattern, false);
			StringContentParameter contentParameter = new(parameter, data);
			return contentParameter;
		}
	}

	/// <summary>
	///     Further expectations on the matching of a body of the <see cref="StringContent" />.
	/// </summary>
	public interface IStringContentBodyMatchingParameter : IStringContentBodyParameter
	{
		/// <summary>
		///     Expects the body match pattern to be a <see cref="Regex" />.
		/// </summary>
		IStringContentBodyParameter AsRegex(
			RegexOptions options = RegexOptions.None,
			TimeSpan? timeout = null);
	}

	/// <summary>
	///     Further expectations on the body of the <see cref="StringContent" />.
	/// </summary>
	public interface IStringContentBodyParameter : IHttpContentParameter
	{
		/// <summary>
		///     Ignores case when matching the body.
		/// </summary>
		IStringContentBodyParameter IgnoringCase();
	}

	private sealed class StringMatcher(string value, bool isExact)
	{
		private BodyMatchType _bodyMatchType = isExact ? BodyMatchType.Exact : BodyMatchType.Wildcard;
		private bool _ignoringCase;
		private RegexOptions _regexOptions;
		private TimeSpan? _timeout;

		public bool Matches(string stringContent)
		{
			switch (_bodyMatchType)
			{
				case BodyMatchType.Exact when
					!stringContent.Equals(value, _ignoringCase
						? StringComparison.OrdinalIgnoreCase
						: StringComparison.Ordinal):
					return false;
				case BodyMatchType.Wildcard when
					!Wildcard.Pattern(value, _ignoringCase).Matches(stringContent):
					return false;
				case BodyMatchType.Regex:
					{
						Regex regex = new(value,
							_ignoringCase ? _regexOptions | RegexOptions.IgnoreCase : _regexOptions,
							_timeout ?? Regex.InfiniteMatchTimeout);
						if (!regex.IsMatch(stringContent))
						{
							return false;
						}

						break;
					}
			}

			return true;
		}

		public void AsRegex(
			RegexOptions options = RegexOptions.None,
			TimeSpan? timeout = null)
		{
			_regexOptions = options;
			_timeout = timeout;
			_bodyMatchType = BodyMatchType.Regex;
		}

		public void IgnoringCase()
			=> _ignoringCase = true;

		private enum BodyMatchType
		{
			Exact,
			Wildcard,
			Regex,
		}
	}

	private sealed class StringContentParameter : HttpContentParameterWrapper, IStringContentBodyMatchingParameter
	{
		private readonly StringMatcher _data;

		public StringContentParameter(IHttpContentParameter parameter, StringMatcher data) : base(parameter)
		{
			_data = data;
			parameter.WithString(data.Matches);
		}

		public IStringContentBodyParameter IgnoringCase()
		{
			_data.IgnoringCase();
			return this;
		}

		public IStringContentBodyParameter AsRegex(RegexOptions options = RegexOptions.None, TimeSpan? timeout = null)
		{
			_data.AsRegex(options, timeout);
			return this;
		}
	}
}
#pragma warning restore S2325 // Methods and properties that don't access instance data should be static
