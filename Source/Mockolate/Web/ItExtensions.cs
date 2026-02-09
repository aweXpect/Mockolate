using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
#if NETSTANDARD2_0
using Mockolate.Internals.Polyfills;
#endif

namespace Mockolate.Web;

/// <summary>
///     Extensions for parameter matchers for HTTP-related types.
/// </summary>
public static partial class ItExtensions
{
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
				.Where(pair => !string.IsNullOrWhiteSpace(pair[0]))
				.Select(pair =>
					(
						WebUtility.UrlDecode(pair[0]),
						pair.Length == 2 ? WebUtility.UrlDecode(pair[1]) : ""
					)
				);
	}
}
