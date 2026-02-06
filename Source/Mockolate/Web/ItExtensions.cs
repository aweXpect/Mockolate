using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
	private sealed class HttpHeadersMatcher
	{
		private readonly List<(string Name, HttpHeaderValue Value)> _requiredHeaders = [];

		public void AddRequiredHeader(string name, HttpHeaderValue value)
			=> _requiredHeaders.Add((name, value));

		public void AddRequiredHeader(IEnumerable<(string Name, HttpHeaderValue Value)> headers)
			=> _requiredHeaders.AddRange(headers);

		public void AddRequiredHeader(string headers)
			=> _requiredHeaders.AddRange(ExtractHeaders(headers));

		public bool Matches(HttpHeaders messageHeaders)
			=> _requiredHeaders.All(header => MatchesHeader(header.Name, header.Value, messageHeaders));

		private static bool MatchesHeader(string name, HttpHeaderValue value, HttpHeaders messageHeaders)
		{
			if (!messageHeaders.TryGetValues(name, out IEnumerable<string>? values))
			{
				return false;
			}

			return values.Any(value.Matches);
		}

		private static List<(string, HttpHeaderValue)> ExtractHeaders(string headers)
		{
			List<(string, HttpHeaderValue)> headerList = new();
			using StringReader reader = new(headers);
			string? line = reader.ReadLine();
			while (!string.IsNullOrWhiteSpace(line))
			{
				string[] parts = line.Split(':', 2);

				if (parts.Length != 2)
				{
					throw new ArgumentException("The header contained an invalid line: " + line, nameof(headers));
				}

				headerList.Add((parts[0].Trim(), parts[1].TrimStart(' ')));
				line = reader.ReadLine();
			}

			return headerList;
		}
	}

	private sealed class HttpQueryMatcher
	{
		private readonly List<(string Name, HttpQueryParameterValue Value)> _requiredQueryParameters = [];

		public void AddRequiredQueryParameter(string name, HttpQueryParameterValue value)
			=> _requiredQueryParameters.Add((name, value));

		public void AddRequiredQueryParameter(IEnumerable<(string Name, HttpQueryParameterValue Value)> queryParameters)
			=> _requiredQueryParameters.AddRange(queryParameters);

		public void AddRequiredQueryParameter(string queryParameters)
			=> _requiredQueryParameters.AddRange(
				ParseQueryParameters(queryParameters)
					.Select(pair => (pair.Key, new HttpQueryParameterValue(pair.Value))));

		public bool Matches(Uri uri)
		{
			List<(string Key, string Value)> queryParameters = ParseQueryParameters(uri.Query).ToList();
			return _requiredQueryParameters.All(requiredParameter
				=> queryParameters.Any(p
					=> p.Key == requiredParameter.Name &&
					   requiredParameter.Value.Matches(p.Value)));
		}

		private static IEnumerable<(string Key, string Value)> ParseQueryParameters(string input)
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
		TParameter WithHeaders(params (string Name, HttpHeaderValue Value)[] headers);

		/// <summary>
		///     Expects the <see cref="HttpContent" /> to contain the given <paramref name="headers" />.
		/// </summary>
		TParameter WithHeaders(string headers);
	}
}
