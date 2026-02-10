using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using Mockolate.Parameters;

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
	///     Further expectations on the headers of the <see cref="HttpContent" />.
	/// </summary>
	public interface IHttpContentHeaderParameter : IHttpContentParameter
	{
		/// <summary>
		///     If set, also includes the headers from the <see cref="HttpRequestMessage" /> when matching the headers.
		/// </summary>
		IHttpContentParameter IncludingRequestHeaders();
	}

	/// <summary>
	///     Further expectations on the <see cref="HttpContent" />.
	/// </summary>
	public interface IHttpContentParameter : IParameter<HttpContent?>, IHttpHeaderParameter<IHttpContentHeaderParameter>
	{
		/// <summary>
		///     Expects the content to have a string body that satisfies the <paramref name="predicate" />.
		/// </summary>
		IHttpContentParameter WithString(Func<string, bool> predicate);

		/// <summary>
		///     Expects the binary content to satisfy the <paramref name="predicate" />.
		/// </summary>
		IHttpContentParameter WithBytes(Func<byte[], bool> predicate);

		/// <summary>
		///     Expects the <see cref="HttpContent" /> to have the given <paramref name="mediaType" />.
		/// </summary>
		IHttpContentParameter WithMediaType(string? mediaType);
	}

	private sealed class HttpContentParameter
		: IHttpContentHeaderParameter, IHttpRequestMessagePropertyParameter<HttpContent?>, IParameter
	{
		private List<Action<HttpContent?>>? _callbacks;
		private IContentMatcher? _contentMatcher;
		private HttpHeadersMatcher? _headers;
		private string? _mediaType;

		public IHttpContentParameter IncludingRequestHeaders()
		{
			_headers!.IncludingRequestHeaders();
			return this;
		}

		public IHttpContentParameter WithString(Func<string, bool> predicate)
		{
			_contentMatcher = new PredicateStringMatcher(predicate);
			return this;
		}

		public IHttpContentParameter WithBytes(Func<byte[], bool> predicate)
		{
			_contentMatcher = new BinaryMatcher(predicate);
			return this;
		}

		/// <inheritdoc cref="IHttpContentParameter.WithMediaType(string?)" />
		public IHttpContentParameter WithMediaType(string? mediaType)
		{
			_mediaType = mediaType;
			return this;
		}

		public IHttpContentHeaderParameter WithHeaders(params IEnumerable<(string Name, HttpHeaderValue Value)> headers)
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

		/// <inheritdoc cref="IHttpRequestMessagePropertyParameter{HttpContent}.Matches(HttpContent, HttpRequestMessage)" />
		public bool Matches(HttpContent? value, HttpRequestMessage? requestMessage)
		{
			if (value is null)
			{
				return false;
			}

			if (_mediaType is not null &&
			    value.Headers.ContentType?.MediaType?.Equals(_mediaType, StringComparison.OrdinalIgnoreCase) != true)
			{
				return false;
			}

			if (_headers is not null &&
			    !_headers.Matches(value.Headers, _headers.IncludeRequestHeaders ? requestMessage?.Headers : null))
			{
				return false;
			}

			if (_contentMatcher is not null &&
			    !_contentMatcher.Matches(value, requestMessage))
			{
				return false;
			}

			return true;
		}

		/// <inheritdoc cref="IParameter.Matches(object?)" />
		public bool Matches(object? value)
			=> value is HttpContent typedValue && Matches(typedValue, null);

		/// <inheritdoc cref="IParameter.InvokeCallbacks(object?)" />
		public void InvokeCallbacks(object? value)
		{
			if (value is HttpContent httpContent)
			{
				_callbacks?.ForEach(a => a.Invoke(httpContent));
			}
		}

		private interface IContentMatcher
		{
			bool Matches(HttpContent content, HttpRequestMessage? message);
		}

		private sealed class PredicateStringMatcher(Func<string, bool> predicate) : IContentMatcher
		{
			public bool Matches(HttpContent content, HttpRequestMessage? message)
			{
				static Encoding GetEncodingFromCharset(string? charset)
				{
					if (!string.IsNullOrEmpty(charset))
					{
						try
						{
							return Encoding.GetEncoding(charset);
						}
						catch (ArgumentException)
						{
							// If the charset is invalid or not supported, we fall back to the default encoding (UTF-8).
						}
					}

					return Encoding.UTF8;
				}

				string? charset = content.Headers.ContentType?.CharSet;
				Encoding encoding = GetEncodingFromCharset(charset);
#if NET8_0_OR_GREATER
				Stream stream = content.ReadAsStream();
				long position = stream.Position;
				using StreamReader reader = new(stream, encoding, leaveOpen: true);
				string stringContent = reader.ReadToEnd();
				stream.Position = position;
#else
				string stringContent;
				if (message?.Properties.TryGetValue("Mockolate:HttpContent", out object value) == true
				    && value is byte[] bytes)
				{
					stringContent = encoding.GetString(bytes);
				}
				else
				{
					Stream stream = content.ReadAsStreamAsync().ConfigureAwait(false).GetAwaiter().GetResult();
					long position = stream.Position;
					using StreamReader reader = new(stream, encoding, true, 1024, true);
					stringContent = reader.ReadToEnd();
					stream.Position = position;
				}
#endif
				return predicate.Invoke(stringContent);
			}
		}

		private sealed class BinaryMatcher(Func<byte[], bool> predicate) : IContentMatcher
		{
			public bool Matches(HttpContent content, HttpRequestMessage? message)
			{
#if NET8_0_OR_GREATER
				Stream stream = content.ReadAsStream();
				long position = stream.Position;
				using MemoryStream ms = new();
				stream.CopyTo(ms);
				byte[] bytes = ms.ToArray();
				stream.Position = position;
#else
				byte[] bytes;
				if (message?.Properties.TryGetValue("Mockolate:HttpContent", out object value) == true
				    && value is byte[] b)
				{
					bytes = b;
				}
				else
				{
					Stream stream = content.ReadAsStreamAsync().ConfigureAwait(false).GetAwaiter().GetResult();
					long position = stream.Position;
					using MemoryStream ms = new();
					stream.CopyTo(ms);
					bytes = ms.ToArray();
					stream.Position = position;
				}
#endif
				return predicate.Invoke(bytes);
			}
		}
	}

	/// <summary>
	///     An abstract wrapper base class for <see cref="IHttpContentParameter" />.
	/// </summary>
	public abstract class HttpContentParameterWrapper(IHttpContentParameter parameter) : IHttpContentParameter,
		IHttpRequestMessagePropertyParameter<HttpContent?>, IParameter
	{
		/// <inheritdoc cref="IParameter{T}.Do(Action{T})" />
		public IParameter<HttpContent?> Do(Action<HttpContent?> callback)
			=> parameter.Do(callback);

		/// <inheritdoc cref="IHttpHeaderParameter{T}.WithHeaders" />
		public IHttpContentHeaderParameter WithHeaders(
			params IEnumerable<(string Name, HttpHeaderValue Value)> headers)
			=> parameter.WithHeaders(headers);

		/// <inheritdoc cref="IHttpContentParameter.WithString(Func{string, bool})" />
		public IHttpContentParameter WithString(Func<string, bool> predicate)
			=> parameter.WithString(predicate);

		/// <inheritdoc cref="IHttpContentParameter.WithBytes(Func{byte[], bool})" />
		public IHttpContentParameter WithBytes(Func<byte[], bool> predicate)
			=> parameter.WithBytes(predicate);

		/// <inheritdoc cref="IHttpContentParameter.WithMediaType(string)" />
		public IHttpContentParameter WithMediaType(string? mediaType)
			=> parameter.WithMediaType(mediaType);

		bool IHttpRequestMessagePropertyParameter<HttpContent?>.Matches(HttpContent? value,
			HttpRequestMessage? requestMessage)
		{
			if (parameter is IHttpRequestMessagePropertyParameter<HttpContent?> requestMessagePropertyParameter)
			{
				return requestMessagePropertyParameter.Matches(value, requestMessage);
			}

			return Matches(value);
		}

		/// <inheritdoc cref="IParameter.Matches(object?)" />
		public bool Matches(object? value)
			=> ((IParameter)parameter).Matches(value);

		/// <inheritdoc cref="IParameter.InvokeCallbacks(object?)" />
		public void InvokeCallbacks(object? value)
			=> ((IParameter)parameter).InvokeCallbacks(value);
	}
}
#pragma warning restore S2325 // Methods and properties that don't access instance data should be static
