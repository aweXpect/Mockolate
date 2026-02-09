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
	/// <inheritdoc cref="IHttpContentParameter" />
	extension<TParameter>(IHttpHeaderParameter<TParameter> parameter)
	{
		/// <summary>
		///     Expects the <see cref="HttpContent" /> to contain a header matching the <paramref name="name" /> and
		///     <paramref name="value" />.
		/// </summary>
		public TParameter WithHeaders(string name, HttpHeaderValue value)
			=> parameter.WithHeaders((name, value));

		/// <summary>
		///     Expects the <see cref="HttpContent" /> to contain the given <paramref name="headers" />.
		/// </summary>
		public TParameter WithHeaders(string headers)
		{
			List<(string, HttpHeaderValue)> headerList = new();
			using StringReader reader = new(headers);
			string? line = reader.ReadLine();
			while (!string.IsNullOrWhiteSpace(line))
			{
				string[] parts = line.Split(':', 2);

				if (parts.Length != 2)
				{
					// ReSharper disable once LocalizableElement
					throw new ArgumentException("The header contained an invalid line: " + line, nameof(headers));
				}

				headerList.Add((parts[0].Trim(), parts[1].TrimStart(' ')));
				line = reader.ReadLine();
			}

			return parameter.WithHeaders(headerList);
		}
	}

	/// <summary>
	///     Further expectations on the <see cref="HttpContent" />.
	/// </summary>
	public interface IHttpHeaderParameter<out TParameter>
	{
		/// <summary>
		///     Expects the <see cref="HttpContent" /> to contain the given <paramref name="headers" />.
		/// </summary>
		TParameter WithHeaders(params IEnumerable<(string Name, HttpHeaderValue Value)> headers);
	}

	private sealed class HttpHeadersMatcher
	{
		private readonly List<(string Name, HttpHeaderValue Value)> _requiredHeaders = [];

		public bool IncludeRequestHeaders { get; private set; }

		public void AddRequiredHeader(IEnumerable<(string Name, HttpHeaderValue Value)> headers)
			=> _requiredHeaders.AddRange(headers);

		public bool Matches(HttpHeaders messageHeaders, HttpHeaders? alternativeHeaders = null)
			=> alternativeHeaders is null
				? _requiredHeaders.All(header => MatchesHeader(header.Name, header.Value, messageHeaders))
				: _requiredHeaders.All(header => MatchesHeader(header.Name, header.Value, messageHeaders) ||
				                                 MatchesHeader(header.Name, header.Value, alternativeHeaders));

		private static bool MatchesHeader(string name, HttpHeaderValue value, HttpHeaders messageHeaders)
		{
			if (!messageHeaders.TryGetValues(name, out IEnumerable<string>? values))
			{
				return false;
			}

			return values.Any(value.Matches);
		}

		public void IncludingRequestHeaders()
			=> IncludeRequestHeaders = true;
	}
}
