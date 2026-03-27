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
		///     <see cref="System.Net.Http.HttpClient.PutAsync(string?, HttpContent?)" />
		///     with the given <paramref name="requestUri" />.
		/// </summary>
		public VerificationResult<HttpClient> PutAsync(
			ParameterMatcher<string?> requestUri,
			ParameterMatcher<HttpContent?>? content = null)
			=> verify.PutAsync(
				requestUri,
				content ?? It.IsAny<HttpContent?>(),
				It.IsAny<CancellationToken>());

		/// <summary>
		///     Validates the invocations for the method
		///     <see cref="System.Net.Http.HttpClient.PutAsync(string?, HttpContent?)" />
		///     with the given <paramref name="requestUri" />.
		/// </summary>
		public VerificationResult<HttpClient> PutAsync(
			ParameterMatcher<Uri?> requestUri,
			ParameterMatcher<HttpContent?>? content = null)
			=> verify.PutAsync(
				requestUri,
				content ?? It.IsAny<HttpContent?>(),
				It.IsAny<CancellationToken>());

		/// <summary>
		///     Validates the invocations for the method
		///     <see cref="System.Net.Http.HttpClient.PutAsync(string?, HttpContent?)" />
		///     with the given <paramref name="requestUri" /> and <paramref name="cancellationToken" />.
		/// </summary>
		public VerificationResult<HttpClient> PutAsync(
			ParameterMatcher<string?> requestUri,
			ParameterMatcher<HttpContent?> content,
			ParameterMatcher<CancellationToken> cancellationToken)
		{
			if (verify is HttpClient httpClient and IMock { MockRegistry.ConstructorParameters.Length: > 0, } httpClientMock &&
			    httpClientMock.MockRegistry.ConstructorParameters[0] is IMock httpMessageHandlerMock)
			{
				return httpMessageHandlerMock.MockRegistry.Method(
						httpClientMock.MockRegistry.ConstructorParameters[0],
						new MethodParameterMatch("global::System.Net.Http.HttpMessageHandler.SendAsync", [
							new NamedParameter("request", new HttpRequestMessageParameters(HttpMethod.Put,
								new HttpStringUriParameter(requestUri),
								new HttpRequestMessageParameter<HttpContent?>(r => r.Content, content))),
							new NamedParameter("cancellationToken", (IParameter)cancellationToken),
						]))
					.Map(httpClient);
			}

			throw new MockException(
				"Cannot verify HttpClient when it is not mocked with a mockable HttpMessageHandler.");
		}

		/// <summary>
		///     Validates the invocations for the method
		///     <see cref="System.Net.Http.HttpClient.PutAsync(string?, HttpContent?)" />
		///     with the given <paramref name="requestUri" /> and <paramref name="cancellationToken" />.
		/// </summary>
		public VerificationResult<HttpClient> PutAsync(
			ParameterMatcher<Uri?> requestUri,
			ParameterMatcher<HttpContent?> content,
			ParameterMatcher<CancellationToken> cancellationToken)
		{
			if (verify is HttpClient httpClient and IMock { MockRegistry.ConstructorParameters.Length: > 0, } httpClientMock &&
			    httpClientMock.MockRegistry.ConstructorParameters[0] is IMock httpMessageHandlerMock)
			{
				return httpMessageHandlerMock.MockRegistry.Method(
						httpClientMock.MockRegistry.ConstructorParameters[0],
						new MethodParameterMatch("global::System.Net.Http.HttpMessageHandler.SendAsync", [
							new NamedParameter("request", new HttpRequestMessageParameters(HttpMethod.Put,
								new HttpRequestMessageParameter<Uri?>(r => r.RequestUri, requestUri),
								new HttpRequestMessageParameter<HttpContent?>(r => r.Content, content))),
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
