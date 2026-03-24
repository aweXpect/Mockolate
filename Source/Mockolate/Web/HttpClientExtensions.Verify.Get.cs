using System;
using System.Net.Http;
using System.Threading;
using Mockolate.Exceptions;
using Mockolate.Parameters;
using Mockolate.Setup;
using Mockolate.Verify;

// ReSharper disable once CheckNamespace
namespace Mockolate;

#pragma warning disable S2325 // Methods and properties that don't access instance data should be static
public static partial class HttpClientExtensions
{
	/// <inheritdoc cref="HttpClientExtensions" />
	extension(IMockVerify<HttpClient> verify)
	{
		/// <summary>
		///     Validates the invocations for the method
		///     <see cref="System.Net.Http.HttpClient.GetAsync(string?)" />
		///     with the given <paramref name="requestUri" />.
		/// </summary>
		public VerificationResult<HttpClient> GetAsync(
			IParameter<string?> requestUri)
			=> verify.GetAsync(requestUri, It.IsAny<CancellationToken>());

		/// <summary>
		///     Validates the invocations for the method
		///     <see cref="System.Net.Http.HttpClient.GetAsync(string?)" />
		///     with the given <paramref name="requestUri" />.
		/// </summary>
		public VerificationResult<HttpClient> GetAsync(
			IParameter<Uri?> requestUri)
			=> verify.GetAsync(requestUri, It.IsAny<CancellationToken>());

		/// <summary>
		///     Validates the invocations for the method
		///     <see cref="System.Net.Http.HttpClient.GetAsync(string?)" />
		///     with the given <paramref name="requestUri" /> and <paramref name="cancellationToken" />.
		/// </summary>
		public VerificationResult<HttpClient> GetAsync(
			IParameter<string?> requestUri,
			IParameter<CancellationToken> cancellationToken)
		{
			if (verify is HttpClient httpClient and IMock { ConstructorParameters.Length: > 0, } httpClientMock &&
			    httpClientMock.ConstructorParameters[0] is IMock httpMessageHandlerMock)
			{
				return httpMessageHandlerMock.MockRegistry.Method(
						httpClientMock.ConstructorParameters[0],
						new MethodParameterMatch("global::System.Net.Http.HttpMessageHandler.SendAsync", [
							new NamedParameter("request", new HttpRequestMessageParameters(HttpMethod.Get, new HttpStringUriParameter(requestUri))),
							new NamedParameter("cancellationToken", (IParameter)cancellationToken),
						]))
					.Map(httpClient);
			}

			throw new MockException("Cannot verify HttpClient when it is not mocked with a mockable HttpMessageHandler.");
		}

		/// <summary>
		///     Validates the invocations for the method
		///     <see cref="System.Net.Http.HttpClient.GetAsync(string?)" />
		///     with the given <paramref name="requestUri" /> and <paramref name="cancellationToken" />.
		/// </summary>
		public VerificationResult<HttpClient> GetAsync(
			IParameter<Uri?> requestUri,
			IParameter<CancellationToken> cancellationToken)
		{
			if (verify is HttpClient httpClient and IMock { ConstructorParameters.Length: > 0, } httpClientMock &&
			    httpClientMock.ConstructorParameters[0] is IMock httpMessageHandlerMock)
			{
				return httpMessageHandlerMock.MockRegistry.Method(
						httpClientMock.ConstructorParameters[0],
						new MethodParameterMatch("global::System.Net.Http.HttpMessageHandler.SendAsync", [
							new NamedParameter("request", new HttpRequestMessageParameters(HttpMethod.Get,
								new HttpRequestMessageParameter<Uri?>(r => r.RequestUri, requestUri))),
							new NamedParameter("cancellationToken", (IParameter)cancellationToken),
						]))
					.Map(httpClient);
			}

			throw new MockException(
				"Cannot verify HttpClient when it is not mocked with a mockable HttpMessageHandler.");
		}
	}
}
#pragma warning restore S2325 // Methods and properties that don't access instance data should be static
