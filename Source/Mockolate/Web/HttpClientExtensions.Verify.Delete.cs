using System;
using System.Net.Http;
using System.Threading;
using Mockolate.Exceptions;
using Mockolate.Parameters;
using Mockolate.Verify;

namespace Mockolate.Web;

#pragma warning disable S2325 // Methods and properties that don't access instance data should be static
/// <summary>
///     Extensions for mocking <see cref="HttpClient" />.
/// </summary>
public static partial class HttpClientExtensions
{
	/// <inheritdoc cref="HttpClientExtensions" />
	extension(IMockVerifyInvokedWithToStringWithEqualsWithGetHashCode<HttpClient> verifyInvoked)
	{
		/// <summary>
		///     Validates the invocations for the method
		///     <see cref="System.Net.Http.HttpClient.DeleteAsync(string?)" />
		///     with the given <paramref name="requestUri" />.
		/// </summary>
		public VerificationResult<HttpClient> DeleteAsync(
			IParameter<string?> requestUri)
			=> verifyInvoked.DeleteAsync(requestUri, It.IsAny<CancellationToken>());

		/// <summary>
		///     Validates the invocations for the method
		///     <see cref="System.Net.Http.HttpClient.DeleteAsync(string?)" />
		///     with the given <paramref name="requestUri" />.
		/// </summary>
		public VerificationResult<HttpClient> DeleteAsync(
			IParameter<Uri?> requestUri)
			=> verifyInvoked.DeleteAsync(requestUri, It.IsAny<CancellationToken>());

		/// <summary>
		///     Validates the invocations for the method
		///     <see cref="System.Net.Http.HttpClient.DeleteAsync(string?)" />
		///     with the given <paramref name="requestUri" /> and <paramref name="cancellationToken" />.
		/// </summary>
		public VerificationResult<HttpClient> DeleteAsync(
			IParameter<string?> requestUri,
			IParameter<CancellationToken> cancellationToken)
		{
			if (verifyInvoked is Mock<HttpClient> { ConstructorParameters.Length: > 0, } httpClientMock &&
			    httpClientMock.ConstructorParameters[0] is IMockSubject<HttpMessageHandler> httpMessageHandlerMock)
			{
				return httpMessageHandlerMock.Mock.Method("System.Net.Http.HttpMessageHandler.SendAsync",
						new NamedParameter("request", new HttpRequestMessageParameters(HttpMethod.Delete,
							new HttpStringUriParameter(requestUri))),
						new NamedParameter("cancellationToken", (IParameter)cancellationToken))
					.Map(httpClientMock.Subject);
			}

			throw new MockException(
				"Cannot verify HttpClient when it is not mocked with a mockable HttpMessageHandler.");
		}

		/// <summary>
		///     Validates the invocations for the method
		///     <see cref="System.Net.Http.HttpClient.DeleteAsync(string?)" />
		///     with the given <paramref name="requestUri" /> and <paramref name="cancellationToken" />.
		/// </summary>
		public VerificationResult<HttpClient> DeleteAsync(
			IParameter<Uri?> requestUri,
			IParameter<CancellationToken> cancellationToken)
		{
			if (verifyInvoked is Mock<HttpClient> { ConstructorParameters.Length: > 0, } httpClientMock &&
			    httpClientMock.ConstructorParameters[0] is IMockSubject<HttpMessageHandler> httpMessageHandlerMock)
			{
				return httpMessageHandlerMock.Mock.Method("System.Net.Http.HttpMessageHandler.SendAsync",
						new NamedParameter("request", new HttpRequestMessageParameters(HttpMethod.Delete,
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
