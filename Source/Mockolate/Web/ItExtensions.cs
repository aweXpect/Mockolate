using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
#if NETSTANDARD2_0
using Mockolate.Internals.Polyfills;
#endif

namespace Mockolate.Web;

/// <summary>
///     Extensions for parameter matchers for HTTP-related types.
/// </summary>
public static partial class ItExtensions
{
	private class HttpHeadersMatcher
	{
		private readonly List<KeyValuePair<string, HttpHeaderValue>> _requiredHeaders = [];

		public void AddRequiredHeader(string name, HttpHeaderValue value)
			=> _requiredHeaders.Add(new KeyValuePair<string, HttpHeaderValue>(name, value));

		public void AddRequiredHeader(IEnumerable<KeyValuePair<string, HttpHeaderValue>> headers)
			=> _requiredHeaders.AddRange(headers);

		public void AddRequiredHeader(string headers)
			=> _requiredHeaders.AddRange(ExtractHeaders(headers));

		public bool Matches(HttpHeaders messageHeaders)
			=> _requiredHeaders.All(header => MatchesHeader(header.Key, header.Value, messageHeaders));

		private static bool MatchesHeader(string name, HttpHeaderValue value, HttpHeaders messageHeaders)
		{
			if (!messageHeaders.TryGetValues(name, out IEnumerable<string>? values))
			{
				return false;
			}

			return values.Any(value.Matches);
		}

		private static IEnumerable<KeyValuePair<string, HttpHeaderValue>> ExtractHeaders(string headerValue)
		{
			List<KeyValuePair<string, HttpHeaderValue>> headers = new();
			using StringReader reader = new(headerValue);
			string? line = reader.ReadLine();
			while (!string.IsNullOrWhiteSpace(line))
			{
				string[] parts = line.Split(':', 2);

				if (parts.Length != 2)
				{
					throw new ArgumentException("The header contained an invalid line: " + line);
				}

				headers.Add(new KeyValuePair<string, HttpHeaderValue>(parts[0], parts[1].TrimStart(' ')));
				line = reader.ReadLine();
			}

			return headers;
		}
	}

	/// <summary>
	///     Further expectations on the <see cref="HttpContent" />.
	/// </summary>
	public interface IHttpHeaderParameter<out TParameter>
	{
		/// <summary>
		///     Expects the <see cref="HttpContent" /> to contain a header matching the <paramref name="name" /> and
		///     <paramref name="value" />.
		/// </summary>
		TParameter WithHeaders(string name, HttpHeaderValue value);

		/// <summary>
		///     Expects the <see cref="HttpContent" /> to contain the given <paramref name="headers" />.
		/// </summary>
		TParameter WithHeaders(IEnumerable<KeyValuePair<string, HttpHeaderValue>> headers);

		/// <summary>
		///     Expects the <see cref="HttpContent" /> to contain the given <paramref name="headers" />.
		/// </summary>
		TParameter WithHeaders(string headers);
	}
}
