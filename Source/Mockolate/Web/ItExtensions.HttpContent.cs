using System;
using System.Collections.Generic;
using System.Net.Http;
using Mockolate.Parameters;

namespace Mockolate.Web;

/// <summary>
///     Extensions for parameter matchers for HTTP-related types.
/// </summary>
public static partial class ItExtensions
{
	/// <inheritdoc cref="ItExtensions" />
	extension(It _)
	{
		/// <summary>
		///     Expects the <see cref="HttpContent" /> parameter to have a JSON content equivalent to the given
		///     <paramref name="json" />.
		/// </summary>
		public static IParameter<HttpContent?> HasJsonContent(string? json = null)
			=> new ExpressionParameter<HttpContent?>(c =>
				c is { Headers.ContentType.MediaType: "application/json", } &&
				(json is null || c.ReadAsStringAsync().Result == json));
	}

	private sealed class ExpressionParameter<T>(Func<T, bool> predicate) : IParameter<HttpContent?>, IParameter
	{
		private List<Action<HttpContent?>>? _callbacks;

		public bool Matches(object? value)
			=> value is T typedValue && predicate(typedValue);

		public void InvokeCallbacks(object? value)
		{
			if (value is HttpContent httpContent)
			{
				_callbacks?.ForEach(a => a.Invoke(httpContent));
			}
		}

		public IParameter<HttpContent?> Do(Action<HttpContent?> callback)
		{
			_callbacks ??= [];
			_callbacks.Add(callback);
			return this;
		}
	}
}
