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
		///     Expects the parameter to be a <see cref="HttpRequestMessage" />.
		/// </summary>
		public static IHttpRequestMessageParameter IsHttpRequestMessage(HttpMethod? method = null)
			=> new HttpRequestMessageParameter(method);
	}

	/// <summary>
	///     Further expectations on the <see cref="HttpRequestMessage" />.
	/// </summary>
	public interface IHttpRequestMessageParameter : IHttpRequestMessageParameter<IHttpRequestMessageParameter>;

	/// <summary>
	///     Further expectations on the <see cref="HttpRequestMessage" />.
	/// </summary>
	public interface IHttpRequestMessageParameter<out TParameter> : IParameter<HttpRequestMessage>,
		IHttpHeaderParameter<TParameter>
	{
		/// <summary>
		///     Add expectations on the <see cref="HttpContent" /> of the <see cref="HttpRequestMessage" />.
		/// </summary>
		TParameter WhoseContentIs(Action<IHttpContentParameter> configureContent);

		/// <summary>
		///     Add expectations on the string <see cref="HttpContent" /> of the <see cref="HttpRequestMessage" />.
		/// </summary>
		TParameter WhoseStringContentIs(Action<IStringContentParameter> configureContent);

		/// <summary>
		///     Add expectations on the binary <see cref="HttpContent" /> of the <see cref="HttpRequestMessage" />.
		/// </summary>
		TParameter WhoseBinaryContentIs(Action<IBinaryContentParameter> configureContent);

		/// <summary>
		///     Add expectations on the URI of the <see cref="HttpRequestMessage" />.
		/// </summary>
		TParameter WhoseUriIs(Action<IUriParameter> configureUri);

		/// <summary>
		///     Add expectations on the URI of the <see cref="HttpRequestMessage" />.
		/// </summary>
		TParameter WhoseUriIs(string uri, Action<IUriParameter>? configureUri = null);
	}

	private abstract class HttpRequestMessageParameter<TParameter>
		: IHttpRequestMessageParameter<TParameter>, IParameter
	{
		private List<Action<HttpRequestMessage>>? _callbacks;
		private HttpHeadersMatcher? _headers;
		private IParameter<HttpContent?>? _contentParameter;
		private IParameter<Uri?>? _uriParameter;

		/// <summary>
		///     Returns <c>this</c> typed as <typeparamref name="TParameter" /> for fluent API.
		/// </summary>
		protected abstract TParameter GetThis { get; }

		public TParameter WithHeaders(string name, HttpHeaderValue value)
		{
			_headers ??= new HttpHeadersMatcher();
			_headers.AddRequiredHeader(name, value);
			return GetThis;
		}

		public TParameter WithHeaders(params IEnumerable<(string Name, HttpHeaderValue Value)> headers)
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
		public IParameter<HttpRequestMessage> Do(Action<HttpRequestMessage> callback)
		{
			_callbacks ??= [];
			_callbacks.Add(callback);
			return this;
		}

		/// <inheritdoc cref="IParameter.Matches(object?)" />
		public bool Matches(object? value)
			=> value is HttpRequestMessage typedValue && Matches(typedValue);

		/// <inheritdoc cref="IParameter.InvokeCallbacks(object?)" />
		public void InvokeCallbacks(object? value)
		{
			if (value is HttpRequestMessage httpRequestMessage)
			{
				_callbacks?.ForEach(a => a.Invoke(httpRequestMessage));
			}
		}

		/// <summary>
		///     Checks whether the given <see cref="HttpRequestMessage" /> <paramref name="value" /> matches the expectations.
		/// </summary>
		protected virtual bool Matches(HttpRequestMessage value)
		{
			if (_headers is not null &&
			    !_headers.Matches(value.Headers, value.Content?.Headers))
			{
				return false;
			}

			if (_uriParameter is not null &&
			    !((IParameter)_uriParameter).Matches(value.RequestUri))
			{
				return false;
			}

			if (_contentParameter is not null &&
			    !((IParameter)_contentParameter).Matches(value.Content))
			{
				return false;
			}

			return true;
		}

		public TParameter WhoseContentIs(Action<IHttpContentParameter> configureContent)
		{
			IHttpContentParameter parameter = new HttpContentParameter();
			configureContent.Invoke(parameter);
			_contentParameter = parameter;
			return GetThis;
		}

		public TParameter WhoseStringContentIs(Action<IStringContentParameter> configureContent)
		{
			IStringContentParameter parameter = new StringContentParameter();
			configureContent.Invoke(parameter);
			_contentParameter = parameter;
			return GetThis;
		}

		public TParameter WhoseBinaryContentIs(Action<IBinaryContentParameter> configureContent)
		{
			IBinaryContentParameter parameter = new BinaryContentParameter();
			configureContent.Invoke(parameter);
			_contentParameter = parameter;
			return GetThis;
		}

		public TParameter WhoseUriIs(Action<IUriParameter> configureUri)
		{
			IUriParameter parameter = new UriParameter(null);
			configureUri.Invoke(parameter);
			_uriParameter = parameter;
			return GetThis;
		}

		public TParameter WhoseUriIs(string uri, Action<IUriParameter>? configureUri = null)
		{
			IUriParameter parameter = new UriParameter(uri);
			configureUri?.Invoke(parameter);
			_uriParameter = parameter;
			return GetThis;
		}
	}

	private sealed class HttpRequestMessageParameter(HttpMethod? method)
		: HttpRequestMessageParameter<IHttpRequestMessageParameter>,
			IHttpRequestMessageParameter
	{
		/// <inheritdoc cref="HttpRequestMessageParameter{TParameter}.GetThis" />
		protected override IHttpRequestMessageParameter GetThis => this;

		protected override bool Matches(HttpRequestMessage value)
		{
			if (method is not null && value.Method != method)
			{
				return false;
			}

			return base.Matches(value);
		}
	}
}
#pragma warning restore S2325 // Methods and properties that don't access instance data should be static
