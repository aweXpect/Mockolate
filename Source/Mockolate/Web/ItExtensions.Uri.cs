using System;
using System.Collections.Generic;
using System.Net;
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
		///     Expects the <see cref="Uri" /> parameter to match the given <paramref name="pattern" />.
		/// </summary>
		public static IUriParameter IsUri(string? pattern = null)
			=> new UriParameter(pattern);
	}

	/// <summary>
	///     Allows adding additional constraints to a <see cref="Uri" /> parameter matcher.
	/// </summary>
	public interface IUriParameter : IParameter<Uri?>
	{
		/// <summary>
		///     Expects the <see cref="Uri.Scheme" /> to be "http".
		/// </summary>
		IUriParameter ForHttp();

		/// <summary>
		///     Expects the <see cref="Uri.Scheme" /> to be "https".
		/// </summary>
		IUriParameter ForHttps();

		/// <summary>
		///     Expects the <see cref="Uri.Host" /> to match the given <paramref name="hostPattern" /> supporting wildcards.
		/// </summary>
		IUriParameter WithHost(string hostPattern);

		/// <summary>
		///     Expects the <see cref="Uri.Port" /> to be equal to the given <paramref name="port" />.
		/// </summary>
		IUriParameter WithPort(int port);

		/// <summary>
		///     Expects the <see cref="Uri.AbsolutePath" /> to match the given <paramref name="pathPattern" /> supporting
		///     wildcards.
		/// </summary>
		IUriParameter WithPath(string pathPattern);

		/// <summary>
		///     Expects the <see cref="Uri.Query" /> to contain the parameters in the <paramref name="queryString" />.
		/// </summary>
		/// <remarks>
		///     Expect parameters to be separated by '&amp;' and the key-value pairs of the parameters to be separated by '='.
		///     The order of the parameters is ignored.
		/// </remarks>
		IUriParameter WithQuery(string queryString);

		/// <summary>
		///     Expects the <see cref="Uri.Query" /> to contain the given <paramref name="key" />-<paramref name="value" />
		///     pair.
		/// </summary>
		IUriParameter WithQuery(string key, HttpQueryParameterValue value);

		/// <summary>
		///     Expects the <see cref="Uri.Query" /> to contain the given query <paramref name="parameters" />.
		/// </summary>
		IUriParameter WithQuery(params IEnumerable<(string Key, HttpQueryParameterValue Value)> parameters);
	}

	private sealed class UriParameter(string? pattern) : IUriParameter, IParameter
	{
		private List<Action<Uri?>>? _callbacks;
		private string? _hostPattern;
		private string? _pathPattern;
		private Func<int, bool>? _portPredicate;
		private HttpQueryMatcher? _query;
		private string? _scheme;

#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
		public bool Matches(object? value)
		{
			if (value is not Uri uri)
			{
				return false;
			}

			if (pattern is not null)
			{
				string requestUri1 = uri.ToString();
				Wildcard wildcard = Wildcard.Pattern(pattern, true);
				if (!wildcard.Matches(requestUri1) &&
				    (!requestUri1.EndsWith('/') || !wildcard.Matches(requestUri1.TrimEnd('/'))))
				{
					return false;
				}
			}

			if (_scheme is not null &&
			    !string.Equals(_scheme, uri.Scheme, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			if (_portPredicate?.Invoke(uri.Port) == false)
			{
				return false;
			}

			if (_hostPattern is not null &&
			    !Wildcard.Pattern(_hostPattern, true).Matches(uri.Host))
			{
				return false;
			}

			if (_pathPattern is not null &&
			    !Wildcard.Pattern(_pathPattern, true).Matches(WebUtility.UrlDecode(uri.AbsolutePath)))
			{
				return false;
			}

			if (_query is not null && !_query.Matches(uri))
			{
				return false;
			}

			return true;
		}
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high

		public void InvokeCallbacks(object? value)
		{
			if (value is Uri uri)
			{
				_callbacks?.ForEach(a => a.Invoke(uri));
			}
		}

		public IParameter<Uri?> Do(Action<Uri?> callback)
		{
			_callbacks ??= [];
			_callbacks.Add(callback);
			return this;
		}

		public IUriParameter ForHttp()
		{
			_scheme = "http";
			return this;
		}

		public IUriParameter ForHttps()
		{
			_scheme = "https";
			return this;
		}

		public IUriParameter WithHost(string hostPattern)
		{
			_hostPattern = hostPattern;
			return this;
		}

		public IUriParameter WithPort(int port)
		{
			_portPredicate = p => p == port;
			return this;
		}

		public IUriParameter WithPath(string pathPattern)
		{
			_pathPattern = pathPattern;
			return this;
		}

		public IUriParameter WithQuery(string queryString)
		{
			_query ??= new HttpQueryMatcher();
			_query.AddRequiredQueryParameter(queryString);
			return this;
		}

		public IUriParameter WithQuery(string key, HttpQueryParameterValue value)
		{
			_query ??= new HttpQueryMatcher();
			_query.AddRequiredQueryParameter(key, value);
			return this;
		}

		public IUriParameter WithQuery(params IEnumerable<(string Key, HttpQueryParameterValue Value)> parameters)
		{
			_query ??= new HttpQueryMatcher();
			_query.AddRequiredQueryParameter(parameters);
			return this;
		}
	}
}
#pragma warning restore S2325 // Methods and properties that don't access instance data should be static
