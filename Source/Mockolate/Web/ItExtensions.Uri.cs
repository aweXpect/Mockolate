using System;
using System.Collections.Generic;
using Mockolate.Internals;
using Mockolate.Parameters;
#if NETSTANDARD2_0
using Mockolate.Internals.Polyfills;
#endif

namespace Mockolate.Web;

#pragma warning disable S2325 // Methods and properties that don't access instance data should be static
/// <summary>
///     Extensions for parameter matchers for HTTP-related types.
/// </summary>
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
	}

	private sealed class UriParameter(string? pattern) : IUriParameter, IParameter
	{
		private List<Action<Uri?>>? _callbacks;

		public bool Matches(object? value)
		{
			if (value is not Uri uri)
			{
				return false;
			}

			if (pattern is not null)
			{
				string requestUri1 = uri.ToString();
				string requestUri2 = requestUri1.EndsWith('/')
					? requestUri1.Substring(0, requestUri1.Length - 1)
					: requestUri1 + '/';
				Wildcard wildcard = Wildcard.Pattern(pattern, true);
				if (!wildcard.Matches(requestUri1) &&
				    !wildcard.Matches(requestUri2))
				{
					return false;
				}
			}

			return true;
		}

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
	}
}
#pragma warning restore S2325 // Methods and properties that don't access instance data should be static
