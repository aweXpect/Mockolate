using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using Mockolate.Internals;
using Mockolate.Parameters;
#if NETSTANDARD2_0
using Mockolate.Internals.Polyfills;
#endif

namespace Mockolate.Web;

#pragma warning disable S2325 // Methods and properties that don't access instance data should be static
public static partial class ItExtensions
{
	/// <inheritdoc cref="ItExtensions" />
	extension(It _)
	{
		/// <summary>
		///     Expects the parameter to be an <see cref="HttpContent" />.
		/// </summary>
		public static IHttpContentParameter IsHttpContent()
			=> new HttpContentParameter();

		/// <summary>
		///     Expects the parameter to be an <see cref="HttpContent" /> with the given <paramref name="mediaType" />.
		/// </summary>
		public static IHttpContentParameter IsHttpContent(string mediaType)
			=> new HttpContentParameter().WithMediaType(mediaType);
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

	/// <summary>
	///     Further expectations on the form-data <see cref="HttpContent" />.
	/// </summary>
	public interface IFormDataContentParameter : IHttpContentParameter
	{
		/// <summary>
		///     Expects the form data content to not contain any additional key-value pairs other than the ones already specified.
		/// </summary>
		IFormDataContentParameter Exactly();
	}

	/// <summary>
	///     Further expectations on the <see cref="HttpContent" />.
	/// </summary>
	public interface IHttpContentParameter : IParameter<HttpContent?>, IHttpHeaderParameter<IHttpContentParameter>
	{
		/// <summary>
		///     Expects the content to have a string body equal to the <paramref name="expected" /> value.
		/// </summary>
		IStringContentBodyParameter WithString(string expected);

		/// <summary>
		///     Expects the content to have a string body that matches the given wildcard <paramref name="pattern" />.
		/// </summary>
		IStringContentBodyMatchingParameter WithStringMatching(string pattern);

		/// <summary>
		///     Expects the binary content to be equal to the given <paramref name="bytes" />.
		/// </summary>
		IHttpContentParameter WithBytes(byte[] bytes);

		/// <summary>
		///     Expects the binary content to contain the given <paramref name="bytes" />.
		/// </summary>
		IHttpContentParameter WithBytesContaining(byte[] bytes);

		/// <summary>
		///     Expects the form data content to contain the given <paramref name="key" />-<paramref name="value" /> pair.
		/// </summary>
		IFormDataContentParameter WithFormData(string key, HttpFormDataValue value);

		/// <summary>
		///     Expects the form data content to contain the given <paramref name="values" />.
		/// </summary>
		IFormDataContentParameter WithFormData(params IEnumerable<(string Key, HttpFormDataValue Value)> values);

		/// <summary>
		///     Expects the form data content to contain the given <paramref name="values" />.
		/// </summary>
		IFormDataContentParameter WithFormData(string values);

		/// <summary>
		///     Expects the <see cref="HttpContent" /> to have the given <paramref name="mediaType" />.
		/// </summary>
		IHttpContentParameter WithMediaType(string? mediaType);
	}

	private sealed class HttpContentParameter
		: IParameter, IStringContentBodyMatchingParameter, IFormDataContentParameter
	{
		private List<Action<HttpContent?>>? _callbacks;
		private IContentMatcher? _contentMatcher;
		private HttpHeadersMatcher? _headers;
		private string? _mediaType;

		public IFormDataContentParameter Exactly()
		{
			if (_contentMatcher is FormDataMatcher formDataMatcher)
			{
				formDataMatcher.Exactly();
			}

			return this;
		}

		/// <inheritdoc cref="IParameter.Matches(object?)" />
		public bool Matches(object? value)
			=> value is HttpContent typedValue && Matches(typedValue);

		/// <inheritdoc cref="IParameter.InvokeCallbacks(object?)" />
		public void InvokeCallbacks(object? value)
		{
			if (value is HttpContent httpContent)
			{
				_callbacks?.ForEach(a => a.Invoke(httpContent));
			}
		}


		public IStringContentBodyParameter WithString(string expected)
		{
			_contentMatcher = new StringMatcher(expected, true);
			return this;
		}

		public IStringContentBodyMatchingParameter WithStringMatching(string pattern)
		{
			_contentMatcher = new StringMatcher(pattern, false);
			return this;
		}

		public IHttpContentParameter WithBytes(byte[] bytes)
		{
			_contentMatcher = new BinaryMatcher(bytes, true);
			return this;
		}

		public IHttpContentParameter WithBytesContaining(byte[] bytes)
		{
			_contentMatcher = new BinaryMatcher(bytes, false);
			return this;
		}

		public IFormDataContentParameter WithFormData(string key, HttpFormDataValue value)
		{
			_contentMatcher = new FormDataMatcher(key, value);
			return this;
		}

		public IFormDataContentParameter WithFormData(params IEnumerable<(string Key, HttpFormDataValue Value)> values)
		{
			_contentMatcher = new FormDataMatcher(values);
			return this;
		}

		public IFormDataContentParameter WithFormData(string values)
		{
			_contentMatcher = new FormDataMatcher(values);
			return this;
		}

		/// <inheritdoc cref="IHttpContentParameter.WithMediaType(string?)" />
		public IHttpContentParameter WithMediaType(string? mediaType)
		{
			_mediaType = mediaType;
			return this;
		}

		public IHttpContentParameter WithHeaders(string name, HttpHeaderValue value)
		{
			_headers ??= new HttpHeadersMatcher();
			_headers.AddRequiredHeader(name, value);
			return this;
		}

		public IHttpContentParameter WithHeaders(params IEnumerable<(string Name, HttpHeaderValue Value)> headers)
		{
			_headers ??= new HttpHeadersMatcher();
			_headers.AddRequiredHeader(headers);
			return this;
		}

		public IHttpContentParameter WithHeaders(string headers)
		{
			_headers ??= new HttpHeadersMatcher();
			_headers.AddRequiredHeader(headers);
			return this;
		}

		/// <inheritdoc cref="IParameter{T}.Do(Action{T})" />
		public IParameter<HttpContent?> Do(Action<HttpContent?> callback)
		{
			_callbacks ??= [];
			_callbacks.Add(callback);
			return this;
		}

		public IStringContentBodyParameter AsRegex(RegexOptions options = RegexOptions.None, TimeSpan? timeout = null)
		{
			if (_contentMatcher is StringMatcher stringMatcher)
			{
				stringMatcher.AsRegex(options, timeout);
			}

			return this;
		}

		public IStringContentBodyParameter IgnoringCase()
		{
			if (_contentMatcher is StringMatcher stringMatcher)
			{
				stringMatcher.IgnoringCase();
			}

			return this;
		}

		/// <summary>
		///     Checks whether the given <see cref="HttpContent" /> <paramref name="value" /> matches the expectations.
		/// </summary>
		private bool Matches(HttpContent value)
		{
			if (_mediaType is not null &&
			    value.Headers.ContentType?.MediaType?.Equals(_mediaType, StringComparison.OrdinalIgnoreCase) != true)
			{
				return false;
			}

			if (_headers is not null &&
			    !_headers.Matches(value.Headers))
			{
				return false;
			}

			if (_contentMatcher is not null &&
			    !_contentMatcher.Matches(value))
			{
				return false;
			}

			return true;
		}

		private interface IContentMatcher
		{
			bool Matches(HttpContent content);
		}

		private sealed class StringMatcher : IContentMatcher
		{
			private readonly string _expected;
			private BodyMatchType _bodyMatchType;
			private bool _ignoringCase;
			private RegexOptions _regexOptions;
			private TimeSpan? _timeout;

			public StringMatcher(string value, bool isExact)
			{
				_expected = value;
				_bodyMatchType = isExact ? BodyMatchType.Exact : BodyMatchType.Wildcard;
			}

			public bool Matches(HttpContent content)
			{
#if NET8_0_OR_GREATER
				Stream stream = content.ReadAsStream();
				using StreamReader reader = new(stream, leaveOpen: true);
#else
				Stream stream = content.ReadAsStreamAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				using StreamReader reader = new(stream);
#endif
				string stringContent = reader.ReadToEnd();
				switch (_bodyMatchType)
				{
					case BodyMatchType.Exact when
						!stringContent.Equals(_expected, _ignoringCase
							? StringComparison.OrdinalIgnoreCase
							: StringComparison.Ordinal):
						return false;
					case BodyMatchType.Wildcard when
						!Wildcard.Pattern(_expected, _ignoringCase).Matches(stringContent):
						return false;
					case BodyMatchType.Regex:
						{
							Regex regex = new(_expected,
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

		private sealed class BinaryMatcher : IContentMatcher
		{
			private readonly BodyMatchType _bodyMatchType;
			private readonly byte[] _expectedBytes;

			public BinaryMatcher(byte[] bytes, bool isExact)
			{
				_expectedBytes = bytes;
				_bodyMatchType = isExact ? BodyMatchType.Exact : BodyMatchType.Contains;
			}

			public bool Matches(HttpContent content)
			{
#if NET8_0_OR_GREATER
				Stream stream = content.ReadAsStream();
#else
				Stream stream = content.ReadAsStreamAsync().ConfigureAwait(false).GetAwaiter().GetResult();
#endif
				using MemoryStream ms = new();
				stream.CopyTo(ms);
				byte[] bytes = ms.ToArray();
				switch (_bodyMatchType)
				{
					case BodyMatchType.Exact when
						!bytes.SequenceEqual(_expectedBytes):
						return false;
					case BodyMatchType.Contains when
						!Contains(bytes, _expectedBytes):
						return false;
				}

				return true;
			}

			private static bool Contains(byte[] data, byte[] otherData)
			{
#if NET8_0_OR_GREATER
				return data.AsSpan().IndexOf(otherData) >= 0;
#else
				int dataLength = data.Length;
				int otherDataLength = otherData.Length;

				if (dataLength < otherDataLength)
				{
					return false;
				}

				for (int i = 0; i <= dataLength - otherDataLength; i++)
				{
					bool isMatch = true;
					for (int j = 0; j < otherDataLength; j++)
					{
						if (data[i + j] != otherData[j])
						{
							isMatch = false;
							break;
						}
					}

					if (isMatch)
					{
						return true;
					}
				}

				return false;
#endif
			}

			private enum BodyMatchType
			{
				Exact,
				Contains,
			}
		}

		private sealed class FormDataMatcher : IContentMatcher
		{
			private readonly List<(string Name, HttpFormDataValue Value)> _requiredFormDataParameters = [];
			private bool _isExactly;

			public FormDataMatcher(string name, HttpFormDataValue value)
			{
				_requiredFormDataParameters.Add((name, value));
			}

			public FormDataMatcher(IEnumerable<(string Name, HttpFormDataValue Value)> formDataParameters)
			{
				_requiredFormDataParameters.AddRange(formDataParameters);
			}

			public FormDataMatcher(string formDataParameters)
			{
				_requiredFormDataParameters.AddRange(
					ParseFormDataParameters(formDataParameters)
						.Select(pair => (pair.Key, new HttpFormDataValue(pair.Value))));
			}

			public bool Matches(HttpContent content)
			{
				List<(string Key, string Value)> formDataParameters = GetFormData(content).ToList();
				return _isExactly
					? _requiredFormDataParameters.All(requiredParameter
						  => formDataParameters.Any(p
							  => p.Key == requiredParameter.Name &&
							     requiredParameter.Value.Matches(p.Value))) &&
					  formDataParameters.All(f => _requiredFormDataParameters.Any(y => f.Key == y.Name))
					: _requiredFormDataParameters.All(requiredParameter
						=> formDataParameters.Any(p
							=> p.Key == requiredParameter.Name &&
							   requiredParameter.Value.Matches(p.Value)));
			}

			public void Exactly()
				=> _isExactly = true;

			private IEnumerable<(string, string)> GetFormData(HttpContent content)
			{
				if (content is MultipartFormDataContent multipartFormDataContent)
				{
					return multipartFormDataContent.SelectMany(GetFormData);
				}

#if NET8_0_OR_GREATER
				Stream stream = content.ReadAsStream();
				using StreamReader reader = new(stream, leaveOpen: true);
#else
				Stream stream = content.ReadAsStreamAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				using StreamReader reader = new(stream);
#endif
				string rawFormData = reader.ReadToEnd();

				return ParseFormDataParameters(rawFormData);
			}

			private static IEnumerable<(string Key, string Value)> ParseFormDataParameters(string input)
				=> input.TrimStart('?')
					.Split('&')
					.Select(pair => pair.Split('=', 2))
					.Where(pair => pair.Length > 0 && !string.IsNullOrWhiteSpace(pair[0]))
					.Select(pair =>
						(
							WebUtility.UrlDecode(pair[0]),
							pair.Length == 2 ? WebUtility.UrlDecode(pair[1]) : ""
						)
					);
		}
	}
}
#pragma warning restore S2325 // Methods and properties that don't access instance data should be static
