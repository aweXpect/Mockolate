using System;
using System.Net.Http;
using System.Threading;
using Mockolate.Exceptions;
using Mockolate.Parameters;
using Mockolate.Verify;

namespace Mockolate.Web;

#pragma warning disable S2325 // Methods and properties that don't access instance data should be static
public static partial class HttpClientExtensions
{
	/// <inheritdoc cref="HttpClientExtensions" />
	extension(IMockVerifyInvokedWithToStringWithEqualsWithGetHashCode<HttpClient> verifyInvoked)
	{
		/// <summary>
		///     Validates the invocations for the method
		///     <see cref="System.Net.Http.HttpClient.GetAsync(string?)" />
		///     with the given <paramref name="requestUri" />.
		/// </summary>
		public VerificationResult<HttpClient> GetAsync(
			IParameter<string?> requestUri)
			=> verifyInvoked.GetAsync(requestUri, It.IsAny<CancellationToken>());

		/// <summary>
		///     Validates the invocations for the method
		///     <see cref="System.Net.Http.HttpClient.GetAsync(string?)" />
		///     with the given <paramref name="requestUri" />.
		/// </summary>
		public VerificationResult<HttpClient> GetAsync(
			IParameter<Uri?> requestUri)
			=> verifyInvoked.GetAsync(requestUri, It.IsAny<CancellationToken>());

		/// <summary>
		///     Validates the invocations for the method
		///     <see cref="System.Net.Http.HttpClient.GetAsync(string?)" />
		///     with the given <paramref name="requestUri" /> and <paramref name="cancellationToken" />.
		/// </summary>
		public VerificationResult<HttpClient> GetAsync(
			IParameter<string?> requestUri,
			IParameter<CancellationToken> cancellationToken)
		{
			if (verifyInvoked is Mock<HttpClient> { ConstructorParameters.Length: > 0, } httpClientMock &&
			    httpClientMock.ConstructorParameters[0] is IMockSubject<HttpMessageHandler> httpMessageHandlerMock)
			{
				return httpMessageHandlerMock.Mock.Method("System.Net.Http.HttpMessageHandler.SendAsync",
						new NamedParameter("request", new HttpRequestMessageParameters(HttpMethod.Get,
							new HttpStringUriParameter(requestUri))),
						new NamedParameter("cancellationToken", (IParameter)cancellationToken))
					.Map(httpClientMock.Subject);
			}

			throw new MockException(
				"Cannot verify HttpClient when it is not mocked with a mockable HttpMessageHandler.");
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
			if (verifyInvoked is Mock<HttpClient> { ConstructorParameters.Length: > 0, } httpClientMock &&
			    httpClientMock.ConstructorParameters[0] is IMockSubject<HttpMessageHandler> httpMessageHandlerMock)
			{
				return httpMessageHandlerMock.Mock.Method("System.Net.Http.HttpMessageHandler.SendAsync",
						new NamedParameter("request", new HttpRequestMessageParameters(HttpMethod.Get,
							new HttpRequestMessageParameter<Uri?>(r => r.RequestUri, requestUri))),
						new NamedParameter("cancellationToken", (IParameter)cancellationToken))
					.Map(httpClientMock.Subject);
			}

			throw new MockException(
				"Cannot verify HttpClient when it is not mocked with a mockable HttpMessageHandler.");
		}
	}
}
#pragma warning restore S2325 // Methods and properties that don't access instance data should be static
