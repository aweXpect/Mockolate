using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Mockolate.Exceptions;
using Mockolate.Parameters;
using Mockolate.Setup;

namespace Mockolate.Web;

#pragma warning disable S2325 // Methods and properties that don't access instance data should be static
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
		///     <see cref="System.Net.Http.HttpClient.PostAsync(string?, HttpContent?)" />
		///     with the given <paramref name="requestUri" />.
		/// </summary>
		public IReturnMethodSetup<Task<HttpResponseMessage>, HttpRequestMessage, CancellationToken> PostAsync(
			IParameter<string?>? requestUri,
			IParameter<HttpContent?>? content)
			=> setup.PostAsync(requestUri, content, It.IsAny<CancellationToken>());

		/// <summary>
		///     Setup for the method
		///     <see cref="System.Net.Http.HttpClient.PostAsync(Uri?, HttpContent?)" />
		///     with the given <paramref name="requestUri" />.
		/// </summary>
		public IReturnMethodSetup<Task<HttpResponseMessage>, HttpRequestMessage, CancellationToken> PostAsync(
			IParameter<Uri?>? requestUri,
			IParameter<HttpContent?>? content)
			=> setup.PostAsync(requestUri, content, It.IsAny<CancellationToken>());

		/// <summary>
		///     Setup for the method
		///     <see cref="System.Net.Http.HttpClient.PostAsync(string?, HttpContent?, System.Threading.CancellationToken)" />
		///     with the given <paramref name="requestUri" /> and <paramref name="cancellationToken" />.
		/// </summary>
		public IReturnMethodSetup<Task<HttpResponseMessage>, HttpRequestMessage, CancellationToken> PostAsync(
			IParameter<string?>? requestUri,
			IParameter<HttpContent?>? content,
			IParameter<CancellationToken> cancellationToken)
		{
			if (setup is Mock<HttpClient> { ConstructorParameters.Length: > 0, } httpClientMock &&
			    httpClientMock.ConstructorParameters[0] is IMockSubject<HttpMessageHandler> httpMessageHandlerMock &&
			    httpMessageHandlerMock.Mock is IMockMethodSetup<HttpMessageHandler> httpMessageHandlerSetup)
			{
				ReturnMethodSetup<Task<HttpResponseMessage>, HttpRequestMessage, CancellationToken> methodSetup =
					new("System.Net.Http.HttpMessageHandler.SendAsync",
						new NamedParameter("request", new HttpRequestMessageParameters(HttpMethod.Post,
							new HttpStringUriParameter(requestUri),
							new HttpRequestMessageParameter<HttpContent?>(r => r.Content, content))),
						new NamedParameter("cancellationToken", (IParameter)cancellationToken));
				CastToMockRegistrationOrThrow(httpMessageHandlerSetup).SetupMethod(methodSetup);
				return methodSetup;
			}

			throw new MockException("Cannot setup HttpClient when it is not mocked with a mockable HttpMessageHandler.");
		}

		/// <summary>
		///     Setup for the method
		///     <see cref="System.Net.Http.HttpClient.PostAsync(Uri?, HttpContent?, System.Threading.CancellationToken)" />
		///     with the given <paramref name="requestUri" /> and <paramref name="cancellationToken" />.
		/// </summary>
		public IReturnMethodSetup<Task<HttpResponseMessage>, HttpRequestMessage, CancellationToken> PostAsync(
			IParameter<Uri?>? requestUri,
			IParameter<HttpContent?>? content,
			IParameter<CancellationToken> cancellationToken)
		{
			if (setup is Mock<HttpClient> { ConstructorParameters.Length: > 0, } httpClientMock &&
			    httpClientMock.ConstructorParameters[0] is IMockSubject<HttpMessageHandler> httpMessageHandlerMock &&
			    httpMessageHandlerMock.Mock is IMockMethodSetup<HttpMessageHandler> httpMessageHandlerSetup)
			{
				ReturnMethodSetup<Task<HttpResponseMessage>, HttpRequestMessage, CancellationToken> methodSetup =
					new("System.Net.Http.HttpMessageHandler.SendAsync",
						new NamedParameter("request", new HttpRequestMessageParameters(HttpMethod.Post,
							new HttpRequestMessageParameter<Uri?>(r => r.RequestUri, requestUri),
							new HttpRequestMessageParameter<HttpContent?>(r => r.Content, content))),
						new NamedParameter("cancellationToken", (IParameter)cancellationToken));
				CastToMockRegistrationOrThrow(httpMessageHandlerSetup).SetupMethod(methodSetup);
				return methodSetup;
			}

			throw new MockException("Cannot setup HttpClient when it is not mocked with a mockable HttpMessageHandler.");
		}
	}
}
#pragma warning restore S2325 // Methods and properties that don't access instance data should be static
