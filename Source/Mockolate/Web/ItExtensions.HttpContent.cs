using System;
using System.Collections.Generic;
using System.Net.Http;
using Mockolate.Parameters;

namespace Mockolate.Web;

#pragma warning disable S2325 // Methods and properties that don't access instance data should be static
public static partial class ItExtensions
{
	/// <inheritdoc cref="ItExtensions" />
	extension(It _)
	{
		/// <summary>
		///     Expects the parameter to be a <see cref="HttpContent" />.
		/// </summary>
		public static IHttpContentParameter IsHttpContent()
			=> new HttpContentParameter();
	}

	/// <summary>
	///     Further expectations on the <see cref="HttpContent" />.
	/// </summary>
	public interface IHttpContentParameter : IHttpContentParameter<IHttpContentParameter>;

	/// <summary>
	///     Further expectations on the <see cref="HttpContent" />.
	/// </summary>
	public interface IHttpContentParameter<out TParameter> : IParameter<HttpContent?>, IHttpHeaderParameter<TParameter>
	{
		/// <summary>
		///     Expects the <see cref="HttpContent" /> to have the given <paramref name="mediaType" />.
		/// </summary>
		TParameter WithMediaType(string? mediaType);
	}

	private abstract class HttpContentParameter<TParameter>
		: IHttpContentParameter<TParameter>, IParameter
	{
		private List<Action<HttpContent?>>? _callbacks;
		private HttpHeadersMatcher? _headers;
		private string? _mediaType;

		/// <summary>
		///     Returns <c>this</c> typed as <typeparamref name="TParameter" /> for fluent API.
		/// </summary>
		protected abstract TParameter GetThis { get; }

		/// <inheritdoc cref="IHttpContentParameter{TParameter}.WithMediaType(string?)" />
		public TParameter WithMediaType(string? mediaType)
		{
			_mediaType = mediaType;
			return GetThis;
		}

		public TParameter WithHeaders(string name, HttpHeaderValue value)
		{
			_headers ??= new HttpHeadersMatcher();
			_headers.AddRequiredHeader(name, value);
			return GetThis;
		}

		public TParameter WithHeaders(IEnumerable<KeyValuePair<string, HttpHeaderValue>> headers)
		{
			_headers ??= new HttpHeadersMatcher();
			_headers.AddRequiredHeader(headers);
			return GetThis;
		}

		public TParameter WithHeaders(string headers)
		{
			_headers ??= new HttpHeadersMatcher();
			_headers.AddRequiredHeader(headers);
			return GetThis;
		}

		/// <inheritdoc cref="IParameter{T}.Do(Action{T})" />
		public IParameter<HttpContent?> Do(Action<HttpContent?> callback)
		{
			_callbacks ??= [];
			_callbacks.Add(callback);
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

		/// <summary>
		///     Checks whether the given <see cref="HttpContent" /> <paramref name="value" /> matches the expectations.
		/// </summary>
		protected virtual bool Matches(HttpContent value)
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

			return true;
		}
	}

	private sealed class HttpContentParameter : HttpContentParameter<IHttpContentParameter>, IHttpContentParameter
	{
		/// <inheritdoc cref="HttpContentParameter{TParameter}.GetThis" />
		protected override IHttpContentParameter GetThis => this;
	}
}
#pragma warning restore S2325 // Methods and properties that don't access instance data should be static
