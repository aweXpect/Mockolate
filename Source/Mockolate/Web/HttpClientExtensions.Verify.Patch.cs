#if NET8_0_OR_GREATER
using System;
using System.Net.Http;
using System.Threading;
using Mockolate.Exceptions;
using Mockolate.Interactions;
using Mockolate.Parameters;
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
		///     <see cref="System.Net.Http.HttpClient.PatchAsync(string?, HttpContent?)" />
		///     with the given <paramref name="requestUri" />.
		/// </summary>
		public VerificationResult<HttpClient> PatchAsync(
			IParameter<string?> requestUri,
			IParameter<HttpContent?>? content = null)
			=> verify.PatchAsync(
				requestUri,
				content ?? It.IsAny<HttpContent?>(),
				It.IsAny<CancellationToken>());

		/// <summary>
		///     Validates the invocations for the method
		///     <see cref="System.Net.Http.HttpClient.PatchAsync(string?, HttpContent?)" />
		///     with the given <paramref name="requestUri" />.
		/// </summary>
		public VerificationResult<HttpClient> PatchAsync(
			IParameter<Uri?> requestUri,
			IParameter<HttpContent?>? content = null)
			=> verify.PatchAsync(
				requestUri,
				content ?? It.IsAny<HttpContent?>(),
				It.IsAny<CancellationToken>());

		/// <summary>
		///     Validates the invocations for the method
		///     <see cref="System.Net.Http.HttpClient.PatchAsync(string?, HttpContent?)" />
		///     with the given <paramref name="requestUri" /> and <paramref name="cancellationToken" />.
		/// </summary>
		public VerificationResult<HttpClient> PatchAsync(
			IParameter<string?> requestUri,
			IParameter<HttpContent?> content,
			IParameter<CancellationToken> cancellationToken)
		{
			if (verify is HttpClient httpClient and IMock { MockRegistry.ConstructorParameters.Length: > 0, } httpClientMock &&
			    httpClientMock.MockRegistry.ConstructorParameters[0] is IMock httpMessageHandlerMock)
			{
				HttpRequestMessageParameters requestMatcher = new(HttpMethod.Patch,
					new HttpStringUriParameter(requestUri),
					new HttpRequestMessageParameter<HttpContent?>(r => r.Content, content));
				IParameterMatch<CancellationToken> cancellationTokenMatcher = (IParameterMatch<CancellationToken>)cancellationToken;
				return httpMessageHandlerMock.MockRegistry.VerifyMethod<object, MethodInvocation<HttpRequestMessage, CancellationToken>>(
						httpClientMock.MockRegistry.ConstructorParameters[0]!,
						"global::System.Net.Http.HttpMessageHandler.SendAsync",
						method => requestMatcher.Matches(method.Parameter1) && cancellationTokenMatcher.Matches(method.Parameter2),
						() => $"invoked method SendAsync({requestMatcher}, {cancellationTokenMatcher})")
					.Map(httpClient);
			}

			throw new MockException(
				"Cannot verify HttpClient when it is not mocked with a mockable HttpMessageHandler.");
		}

		/// <summary>
		///     Validates the invocations for the method
		///     <see cref="System.Net.Http.HttpClient.PatchAsync(string?, HttpContent?)" />
		///     with the given <paramref name="requestUri" /> and <paramref name="cancellationToken" />.
		/// </summary>
		public VerificationResult<HttpClient> PatchAsync(
			IParameter<Uri?> requestUri,
			IParameter<HttpContent?> content,
			IParameter<CancellationToken> cancellationToken)
		{
			if (verify is HttpClient httpClient and IMock { MockRegistry.ConstructorParameters.Length: > 0, } httpClientMock &&
			    httpClientMock.MockRegistry.ConstructorParameters[0] is IMock httpMessageHandlerMock)
			{
				HttpRequestMessageParameters requestMatcher = new(HttpMethod.Patch,
					new HttpRequestMessageParameter<Uri?>(r => r.RequestUri, requestUri),
					new HttpRequestMessageParameter<HttpContent?>(r => r.Content, content));
				IParameterMatch<CancellationToken> cancellationTokenMatcher = (IParameterMatch<CancellationToken>)cancellationToken;
				return httpMessageHandlerMock.MockRegistry.VerifyMethod<object, MethodInvocation<HttpRequestMessage, CancellationToken>>(
						httpClientMock.MockRegistry.ConstructorParameters[0]!,
						"global::System.Net.Http.HttpMessageHandler.SendAsync",
						method => requestMatcher.Matches(method.Parameter1) && cancellationTokenMatcher.Matches(method.Parameter2),
						() => $"invoked method SendAsync({requestMatcher}, {cancellationTokenMatcher})")
					.Map(httpClient);
			}

			throw new MockException(
				"Cannot verify HttpClient when it is not mocked with a mockable HttpMessageHandler.");
		}
	}
}
#pragma warning restore S2325 // Methods and properties that don't access instance data should be static
#endif
