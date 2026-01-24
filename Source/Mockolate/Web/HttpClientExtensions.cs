using System;
using System.Linq;
using System.Net.Http;
using Mockolate.Parameters;
#if NETSTANDARD2_0
using Mockolate.Internals.Polyfills;
#endif

namespace Mockolate.Web;

/// <summary>
///     Extensions for mocking <see cref="HttpClient" />.
/// </summary>
public static partial class HttpClientExtensions
{
	private sealed class HttpRequestMessageParameters(
		HttpMethod method,
		params IHttpRequestMessageParameter[] parameters)
		: IParameter
	{
		public bool Matches(object? value)
		{
			if (value is HttpRequestMessage requestMessage &&
			    requestMessage.Method == method)
			{
				return parameters.All(parameter => parameter.Matches(requestMessage));
			}

			return false;
		}

		public void InvokeCallbacks(object? value)
		{
			if (value is HttpRequestMessage typedValue)
			{
				foreach (IHttpRequestMessageParameter parameter in parameters)
				{
					parameter.InvokeCallbacks(typedValue);
				}
			}
		}
	}


	private interface IHttpRequestMessageParameter
	{
		bool Matches(HttpRequestMessage value);

		void InvokeCallbacks(HttpRequestMessage value);
	}

	private sealed class HttpRequestMessageParameter<T>(
		Func<HttpRequestMessage, T> valueSelector,
		IParameter<T>? parameter)
		: IHttpRequestMessageParameter
	{
		public bool Matches(HttpRequestMessage value)
			=> parameter is null ||
			   ((IParameter)parameter).Matches(valueSelector(value));

		public void InvokeCallbacks(HttpRequestMessage value)
		{
			if (parameter is IParameter invokableParameter)
			{
				invokableParameter.InvokeCallbacks(valueSelector(value));
			}
		}
	}

	private sealed class HttpStringUriParameter(IParameter<string?>? parameter)
		: IHttpRequestMessageParameter
	{
		public bool Matches(HttpRequestMessage value)
		{
			if (parameter is not IParameter invokableParameter)
			{
				return true;
			}

			if (value.RequestUri is null)
			{
				return false;
			}

			string requestUri1 = value.RequestUri.ToString();
			return invokableParameter.Matches(requestUri1) ||
			       (requestUri1.EndsWith('/') && invokableParameter.Matches(requestUri1.TrimEnd('/')));
		}

		public void InvokeCallbacks(HttpRequestMessage value)
		{
			if (parameter is IParameter invokableParameter)
			{
				invokableParameter.InvokeCallbacks(value.RequestUri?.ToString());
			}
		}
	}
}
