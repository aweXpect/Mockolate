using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using Mockolate.Internals;
using Mockolate.Parameters;

namespace Mockolate.Web;

#pragma warning disable S2325 // Methods and properties that don't access instance data should be static
public static partial class ItExtensions
{
	/// <inheritdoc cref="ItExtensions" />
	extension(It _)
	{
		/// <summary>
		///     Expects the <see cref="HttpContent" /> parameter to be a <see cref="StringContent" />.
		/// </summary>
		public static IStringContentParameter IsStringContent()
			=> new StringContentParameter(null);

		/// <summary>
		///     Expects the <see cref="HttpContent" /> parameter to be a <see cref="StringContent" />
		///     with the given <paramref name="mediaType" />.
		/// </summary>
		public static IStringContentParameter IsStringContent(string mediaType)
			=> new StringContentParameter(mediaType);
	}

	/// <summary>
	///     Further expectations on the <see cref="StringContent" />.
	/// </summary>
	public interface IStringContentParameter : IParameter<HttpContent?>
	{
		/// <summary>
		///     Expects the <see cref="StringContent" /> to have the given <paramref name="mediaType" />.
		/// </summary>
		IStringContentParameter WithMediaType(string? mediaType);

		/// <summary>
		///     Expects the <see cref="StringContent" /> to have a body equal to the given <paramref name="value" />.
		/// </summary>
		IStringContentBodyParameter WithBody(string value);

		/// <summary>
		///     Expects the <see cref="StringContent" /> to have a body that matches the given wildcard <paramref name="pattern" />
		///     .
		/// </summary>
		IStringContentBodyMatchingParameter WithBodyMatching(string pattern);
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
	public interface IStringContentBodyParameter : IStringContentParameter
	{
		/// <summary>
		///     Ignores case when matching the body.
		/// </summary>
		IStringContentBodyParameter IgnoringCase();
	}

	private sealed class StringContentParameter(string? mediaType)
		: IStringContentBodyMatchingParameter, IParameter
	{
		private string? _body;
		private BodyMatchType _bodyMatchType = BodyMatchType.Exact;
		private List<Action<HttpContent?>>? _callbacks;
		private bool _ignoringCase;
		private string? _mediaType = mediaType;
		private RegexOptions _regexOptions;
		private TimeSpan? _timeout;

		public bool Matches(object? value)
			=> value is StringContent typedValue && Matches(typedValue);

		public void InvokeCallbacks(object? value)
		{
			if (value is HttpContent httpContent)
			{
				_callbacks?.ForEach(a => a.Invoke(httpContent));
			}
		}

		/// <inheritdoc cref="IStringContentBodyMatchingParameter.AsRegex(RegexOptions, TimeSpan?)" />
		public IStringContentBodyParameter AsRegex(
			RegexOptions options = RegexOptions.None,
			TimeSpan? timeout = null)
		{
			_regexOptions = options;
			_timeout = timeout;
			_bodyMatchType = BodyMatchType.Regex;
			return this;
		}

		public IParameter<HttpContent?> Do(Action<HttpContent?> callback)
		{
			_callbacks ??= [];
			_callbacks.Add(callback);
			return this;
		}

		/// <inheritdoc cref="IStringContentParameter.WithMediaType(string?)" />
		public IStringContentParameter WithMediaType(string? mediaType)
		{
			_mediaType = mediaType;
			return this;
		}

		/// <inheritdoc cref="IStringContentParameter.WithBody(string)" />
		public IStringContentBodyParameter WithBody(string value)
		{
			_body = value;
			_bodyMatchType = BodyMatchType.Exact;
			return this;
		}

		/// <inheritdoc cref="IStringContentParameter.WithBodyMatching(string)" />
		public IStringContentBodyMatchingParameter WithBodyMatching(string pattern)
		{
			_body = pattern;
			_bodyMatchType = BodyMatchType.Wildcard;
			return this;
		}

		/// <inheritdoc cref="IStringContentBodyParameter.IgnoringCase()" />
		public IStringContentBodyParameter IgnoringCase()
		{
			_ignoringCase = true;
			return this;
		}

		private bool Matches(StringContent value)
		{
			if (_mediaType is not null &&
			    value.Headers.ContentType?.MediaType?.Equals(_mediaType, StringComparison.OrdinalIgnoreCase) != true)
			{
				return false;
			}

			if (_body is not null)
			{
				string content = value.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				switch (_bodyMatchType)
				{
					case BodyMatchType.Exact when
						!content.Equals(_body, _ignoringCase
							? StringComparison.OrdinalIgnoreCase
							: StringComparison.Ordinal):
						return false;
					case BodyMatchType.Wildcard when
						!Wildcard.Pattern(_body, _ignoringCase).Matches(content):
						return false;
					case BodyMatchType.Regex:
						{
							Regex regex = new(_body,
								_ignoringCase ? _regexOptions | RegexOptions.IgnoreCase : _regexOptions,
								_timeout ?? Regex.InfiniteMatchTimeout);
							if (!regex.IsMatch(content))
							{
								return false;
							}

							break;
						}
				}
			}

			return true;
		}

		private enum BodyMatchType
		{
			Exact,
			Wildcard,
			Regex,
		}
	}
}
#pragma warning restore S2325 // Methods and properties that don't access instance data should be static
