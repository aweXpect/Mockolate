using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Mockolate.Exceptions;
using Mockolate.Parameters;
using Mockolate.Setup;

// ReSharper disable once CheckNamespace
namespace Mockolate;

#pragma warning disable S2325 // Methods and properties that don't access instance data should be static
public static partial class HttpClientExtensions
{
	/// <inheritdoc cref="HttpClientExtensions" />
	extension(IMockSetup<HttpClient> setup)
	{
		/// <summary>
		///     Setup for the method
		///     <see cref="System.Net.Http.HttpClient.PutAsync(string?, HttpContent?)" />
		///     with the given <paramref name="requestUri" />.
		/// </summary>
		public IReturnMethodSetup<Task<HttpResponseMessage>, HttpRequestMessage, CancellationToken> PutAsync(
			ParameterMatcher<string?> requestUri,
			ParameterMatcher<HttpContent?>? content = null)
			=> setup.PutAsync(
				requestUri,
				content ?? It.IsAny<HttpContent?>(),
				It.IsAny<CancellationToken>());

		/// <summary>
		///     Setup for the method
		///     <see cref="System.Net.Http.HttpClient.PutAsync(Uri?, HttpContent?)" />
		///     with the given <paramref name="requestUri" />.
		/// </summary>
		public IReturnMethodSetup<Task<HttpResponseMessage>, HttpRequestMessage, CancellationToken> PutAsync(
			ParameterMatcher<Uri?> requestUri,
			ParameterMatcher<HttpContent?>? content = null)
			=> setup.PutAsync(
				requestUri,
				content ?? It.IsAny<HttpContent?>(),
				It.IsAny<CancellationToken>());

		/// <summary>
		///     Setup for the method
		///     <see cref="System.Net.Http.HttpClient.PutAsync(string?, HttpContent?, System.Threading.CancellationToken)" />
		///     with the given <paramref name="requestUri" /> and <paramref name="cancellationToken" />.
		/// </summary>
		public IReturnMethodSetup<Task<HttpResponseMessage>, HttpRequestMessage, CancellationToken> PutAsync(
			ParameterMatcher<string?> requestUri,
			ParameterMatcher<HttpContent?> content,
			ParameterMatcher<CancellationToken> cancellationToken)
		{
			if (setup is IMock { MockRegistry.ConstructorParameters.Length: > 0, } httpClientMock &&
			    httpClientMock.MockRegistry.ConstructorParameters[0] is IMock httpMessageHandlerMock)
			{
				ReturnMethodSetup<Task<HttpResponseMessage>, HttpRequestMessage, CancellationToken> methodSetup =
					new("global::System.Net.Http.HttpMessageHandler.SendAsync",
						new NamedParameter("request", new HttpRequestMessageParameters(HttpMethod.Put,
							new HttpStringUriParameter(requestUri),
							new HttpRequestMessageParameter<HttpContent?>(r => r.Content, content))),
						new NamedParameter("cancellationToken", (IParameter)cancellationToken));
				httpMessageHandlerMock.MockRegistry.SetupMethod(methodSetup);
				return methodSetup;
			}

			throw new MockException(
				"Cannot setup HttpClient when it is not mocked with a mockable HttpMessageHandler.");
		}

		/// <summary>
		///     Setup for the method
		///     <see cref="System.Net.Http.HttpClient.PutAsync(Uri?, HttpContent?, System.Threading.CancellationToken)" />
		///     with the given <paramref name="requestUri" /> and <paramref name="cancellationToken" />.
		/// </summary>
		public IReturnMethodSetup<Task<HttpResponseMessage>, HttpRequestMessage, CancellationToken> PutAsync(
			ParameterMatcher<Uri?> requestUri,
			ParameterMatcher<HttpContent?> content,
			ParameterMatcher<CancellationToken> cancellationToken)
		{
			if (setup is IMock { MockRegistry.ConstructorParameters.Length: > 0, } httpClientMock &&
			    httpClientMock.MockRegistry.ConstructorParameters[0] is IMock httpMessageHandlerMock)
			{
				ReturnMethodSetup<Task<HttpResponseMessage>, HttpRequestMessage, CancellationToken> methodSetup =
					new("global::System.Net.Http.HttpMessageHandler.SendAsync",
						new NamedParameter("request", new HttpRequestMessageParameters(HttpMethod.Put,
							new HttpRequestMessageParameter<Uri?>(r => r.RequestUri, requestUri),
							new HttpRequestMessageParameter<HttpContent?>(r => r.Content, content))),
						new NamedParameter("cancellationToken", (IParameter)cancellationToken));
				httpMessageHandlerMock.MockRegistry.SetupMethod(methodSetup);
				return methodSetup;
			}

			throw new MockException(
				"Cannot setup HttpClient when it is not mocked with a mockable HttpMessageHandler.");
		}
	}
}
#pragma warning restore S2325 // Methods and properties that don't access instance data should be static
