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
		///     <see cref="System.Net.Http.HttpClient.GetAsync(string?)" />
		///     with the given <paramref name="requestUri" />.
		/// </summary>
		public IReturnMethodSetup<Task<HttpResponseMessage>, HttpRequestMessage, CancellationToken> GetAsync(
			IParameter<string?> requestUri)
			=> setup.GetAsync(requestUri, It.IsAny<CancellationToken>());

		/// <summary>
		///     Setup for the method
		///     <see cref="System.Net.Http.HttpClient.GetAsync(Uri?)" />
		///     with the given <paramref name="requestUri" />.
		/// </summary>
		public IReturnMethodSetup<Task<HttpResponseMessage>, HttpRequestMessage, CancellationToken> GetAsync(
			IParameter<Uri?> requestUri)
			=> setup.GetAsync(requestUri, It.IsAny<CancellationToken>());

		/// <summary>
		///     Setup for the method
		///     <see cref="System.Net.Http.HttpClient.GetAsync(string?, System.Threading.CancellationToken)" />
		///     with the given <paramref name="requestUri" /> and <paramref name="cancellationToken" />.
		/// </summary>
		public IReturnMethodSetup<Task<HttpResponseMessage>, HttpRequestMessage, CancellationToken> GetAsync(
			IParameter<string?> requestUri,
			IParameter<CancellationToken> cancellationToken)
		{
			if (setup is IMock { MockRegistry.ConstructorParameters.Length: > 0, } httpClientMock &&
			    httpClientMock.MockRegistry.ConstructorParameters[0] is IMock httpMessageHandlerMock)
			{
				ReturnMethodSetup<Task<HttpResponseMessage>, HttpRequestMessage, CancellationToken>.WithParameterCollection methodSetup =
					new(httpMessageHandlerMock.MockRegistry,
						"global::System.Net.Http.HttpMessageHandler.SendAsync",
						new HttpRequestMessageParameters(HttpMethod.Get,
							new HttpStringUriParameter(requestUri)),
						cancellationToken.AsParameterMatch());
				httpMessageHandlerMock.MockRegistry.SetupMethod(methodSetup);
				return methodSetup;
			}

			throw new MockException(
				"Cannot setup HttpClient when it is not mocked with a mockable HttpMessageHandler.");
		}

		/// <summary>
		///     Setup for the method
		///     <see cref="System.Net.Http.HttpClient.GetAsync(Uri?, System.Threading.CancellationToken)" />
		///     with the given <paramref name="requestUri" /> and <paramref name="cancellationToken" />.
		/// </summary>
		public IReturnMethodSetup<Task<HttpResponseMessage>, HttpRequestMessage, CancellationToken> GetAsync(
			IParameter<Uri?> requestUri,
			IParameter<CancellationToken> cancellationToken)
		{
			if (setup is IMock { MockRegistry.ConstructorParameters.Length: > 0, } httpClientMock &&
			    httpClientMock.MockRegistry.ConstructorParameters[0] is IMock httpMessageHandlerMock)
			{
				ReturnMethodSetup<Task<HttpResponseMessage>, HttpRequestMessage, CancellationToken>.WithParameterCollection methodSetup =
					new(httpMessageHandlerMock.MockRegistry,
						"global::System.Net.Http.HttpMessageHandler.SendAsync",
						new HttpRequestMessageParameters(HttpMethod.Get,
							new HttpRequestMessageParameter<Uri?>(r => r.RequestUri, requestUri)),
						cancellationToken.AsParameterMatch());
				httpMessageHandlerMock.MockRegistry.SetupMethod(methodSetup);
				return methodSetup;
			}

			throw new MockException(
				"Cannot setup HttpClient when it is not mocked with a mockable HttpMessageHandler.");
		}
	}
}
#pragma warning restore S2325 // Methods and properties that don't access instance data should be static
