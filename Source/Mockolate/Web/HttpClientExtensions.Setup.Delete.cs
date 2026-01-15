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
		///     <see cref="System.Net.Http.HttpClient.DeleteAsync(string?)" />
		///     with the given <paramref name="requestUri" />.
		/// </summary>
		public IReturnMethodSetup<Task<HttpResponseMessage>, HttpRequestMessage, CancellationToken> DeleteAsync(
			IParameter<string?>? requestUri)
			=> setup.DeleteAsync(requestUri, It.IsAny<CancellationToken>());

		/// <summary>
		///     Setup for the method
		///     <see cref="System.Net.Http.HttpClient.DeleteAsync(Uri?)" />
		///     with the given <paramref name="requestUri" />.
		/// </summary>
		public IReturnMethodSetup<Task<HttpResponseMessage>, HttpRequestMessage, CancellationToken> DeleteAsync(
			IParameter<Uri?>? requestUri)
			=> setup.DeleteAsync(requestUri, It.IsAny<CancellationToken>());

		/// <summary>
		///     Setup for the method
		///     <see cref="System.Net.Http.HttpClient.DeleteAsync(string?, System.Threading.CancellationToken)" />
		///     with the given <paramref name="requestUri" /> and <paramref name="cancellationToken" />.
		/// </summary>
		public IReturnMethodSetup<Task<HttpResponseMessage>, HttpRequestMessage, CancellationToken> DeleteAsync(
			IParameter<string?>? requestUri,
			IParameter<CancellationToken> cancellationToken)
		{
			if (setup is Mock<HttpClient> { ConstructorParameters.Length: > 0, } httpClientMock &&
			    httpClientMock.ConstructorParameters[0] is IMockSubject<HttpMessageHandler> httpMessageHandlerMock &&
			    httpMessageHandlerMock.Mock is IMockMethodSetup<HttpMessageHandler> httpMessageHandlerSetup)
			{
				ReturnMethodSetup<Task<HttpResponseMessage>, HttpRequestMessage, CancellationToken> methodSetup =
					new("System.Net.Http.HttpMessageHandler.SendAsync",
						new NamedParameter("request", new HttpRequestMessageParameters(HttpMethod.Delete,
							new HttpStringUriParameter(requestUri))),
						new NamedParameter("cancellationToken", (IParameter)cancellationToken));
				CastToMockRegistrationOrThrow(httpMessageHandlerSetup).SetupMethod(methodSetup);
				return methodSetup;
			}

			throw new MockException("Cannot setup HttpClient when it is not mocked with a mockable HttpMessageHandler.");
		}

		/// <summary>
		///     Setup for the method
		///     <see cref="System.Net.Http.HttpClient.DeleteAsync(Uri?, System.Threading.CancellationToken)" />
		///     with the given <paramref name="requestUri" /> and <paramref name="cancellationToken" />.
		/// </summary>
		public IReturnMethodSetup<Task<HttpResponseMessage>, HttpRequestMessage, CancellationToken> DeleteAsync(
			IParameter<Uri?>? requestUri,
			IParameter<CancellationToken> cancellationToken)
		{
			if (setup is Mock<HttpClient> { ConstructorParameters.Length: > 0, } httpClientMock &&
			    httpClientMock.ConstructorParameters[0] is IMockSubject<HttpMessageHandler> httpMessageHandlerMock &&
			    httpMessageHandlerMock.Mock is IMockMethodSetup<HttpMessageHandler> httpMessageHandlerSetup)
			{
				ReturnMethodSetup<Task<HttpResponseMessage>, HttpRequestMessage, CancellationToken> methodSetup =
					new("System.Net.Http.HttpMessageHandler.SendAsync",
						new NamedParameter("request", new HttpRequestMessageParameters(HttpMethod.Delete,
							new HttpRequestMessageParameter<Uri?>(r => r.RequestUri, requestUri))),
						new NamedParameter("cancellationToken", (IParameter)cancellationToken));
				CastToMockRegistrationOrThrow(httpMessageHandlerSetup).SetupMethod(methodSetup);
				return methodSetup;
			}

			throw new MockException("Cannot setup HttpClient when it is not mocked with a mockable HttpMessageHandler.");
		}
	}
}
#pragma warning restore S2325 // Methods and properties that don't access instance data should be static
