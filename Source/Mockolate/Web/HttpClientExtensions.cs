using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Mockolate.Exceptions;
using Mockolate.Parameters;
using Mockolate.Setup;
#if NETSTANDARD2_0
using Mockolate.Internals.Polyfills;
#endif

namespace Mockolate.Web;

/// <summary>
///     Extensions for mocking <see cref="HttpClient" />.
/// </summary>
public static partial class HttpClientExtensions
{
	/// <inheritdoc cref="HttpClientExtensions" />
	extension(IMockMethodSetup<HttpClient> setup)
	{
		/// <summary>
		///     Setup for the method
		///     <see
		///         cref="System.Net.Http.HttpClient.SendAsync(System.Net.Http.HttpRequestMessage, System.Threading.CancellationToken)" />
		///     with the given <paramref name="request" />.
		/// </summary>
		public IReturnMethodSetup<Task<HttpResponseMessage>, HttpRequestMessage, CancellationToken> SendAsync(
			IParameter<HttpRequestMessage> request)
		{
			if (setup is Mock<HttpClient> { ConstructorParameters.Length: > 0, } httpClientMock &&
			    httpClientMock.ConstructorParameters[0] is IMockSubject<HttpMessageHandler> httpMessageHandlerMock)
			{
				ReturnMethodSetup<Task<HttpResponseMessage>, HttpRequestMessage, CancellationToken> methodSetup =
					new("System.Net.Http.HttpMessageHandler.SendAsync",
						new NamedParameter("request", (IParameter)request),
						new NamedParameter("cancellationToken", (IParameter)It.IsAny<CancellationToken>()));
				httpMessageHandlerMock.Mock.Registrations.SetupMethod(methodSetup);
				return methodSetup;
			}

			throw new MockException(
				"Cannot setup HttpClient when it is not mocked with a mockable HttpMessageHandler.");
		}
	}

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
		IParameter<T> parameter)
		: IHttpRequestMessageParameter
	{
		public bool Matches(HttpRequestMessage value)
			=> ((IParameter)parameter).Matches(valueSelector(value));

		public void InvokeCallbacks(HttpRequestMessage value)
		{
			if (parameter is IParameter invokableParameter)
			{
				invokableParameter.InvokeCallbacks(valueSelector(value));
			}
		}
	}

	private sealed class HttpStringUriParameter(IParameter<string?> parameter)
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
