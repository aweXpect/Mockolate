using System;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using Mockolate.Internals;

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
			=> new StringContentParameter();

		/// <summary>
		///     Expects the <see cref="HttpContent" /> parameter to be a <see cref="StringContent" />
		///     with the given <paramref name="mediaType" />.
		/// </summary>
		public static IStringContentParameter IsStringContent(string mediaType)
			=> new StringContentParameter().WithMediaType(mediaType);
	}

	/// <summary>
	///     Further expectations on the <see cref="StringContent" />.
	/// </summary>
	public interface IStringContentParameter : IHttpContentParameter<IStringContentParameter>
	{
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

	private sealed class StringContentParameter
		: HttpContentParameter<IStringContentParameter>, IStringContentBodyMatchingParameter
	{
		private string? _body;
		private BodyMatchType _bodyMatchType = BodyMatchType.Exact;
		private bool _ignoringCase;
		private RegexOptions _regexOptions;
		private TimeSpan? _timeout;

		/// <inheritdoc cref="HttpContentParameter{TParameter}.GetThis" />
		protected override IStringContentParameter GetThis => this;

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

		/// <inheritdoc cref="HttpContentParameter{TParameter}.Matches(HttpContent)" />
		protected override bool Matches(HttpContent value)
		{
			if (!base.Matches(value))
			{
				return false;
			}

			if (_body is not null)
			{
#if NET8_0_OR_GREATER
				Stream stream = value.ReadAsStream();
#else
				Stream stream = value.ReadAsStreamAsync().ConfigureAwait(false).GetAwaiter().GetResult();
#endif
				using StreamReader reader = new(stream);
				string content = reader.ReadToEnd();
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
