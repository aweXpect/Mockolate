using System;
using System.Collections.Generic;
using System.Net.Http;
using Mockolate.Parameters;

namespace Mockolate.Web;

#pragma warning disable S2325 // Methods and properties that don't access instance data should be static
public static partial class ItExtensions
{
	/// <summary>
	///     Further expectations on the <see cref="HttpContent" />.
	/// </summary>
	public interface IHttpContentParameter<out TParameter> : IParameter<HttpContent?>
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

		public IParameter<HttpContent?> Do(Action<HttpContent?> callback)
		{
			_callbacks ??= [];
			_callbacks.Add(callback);
			return this;
		}

		public bool Matches(object? value)
			=> value is HttpContent typedValue && Matches(typedValue);

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

			return true;
		}
	}
}
#pragma warning restore S2325 // Methods and properties that don't access instance data should be static
