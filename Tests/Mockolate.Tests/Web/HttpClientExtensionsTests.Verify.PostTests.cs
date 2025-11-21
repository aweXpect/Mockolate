using System.Net.Http;
using System.Text;
using System.Threading;
using Mockolate.Exceptions;
using Mockolate.Verify;
using Mockolate.Web;

namespace Mockolate.Tests.Web;

public sealed partial class HttpClientExtensionsTests
{
	public sealed partial class Verify
	{
		public sealed class PostTests
		{
			[Test]
			[Arguments("application/json", 1)]
			[Arguments("text/plain", 0)]
			[Arguments("application/txt", 0)]
			public async Task StringUri_ShouldVerifyHttpContent(string mediaType, int expected)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();

				await httpClient.PostAsync("https://www.aweXpect.com",
					new StringContent("{}", Encoding.UTF8, mediaType),
					CancellationToken.None);

				await That(httpClient.VerifyMock.Invoked.PostAsync(
						It.IsAny<string?>(),
						It.IsHttpContent("application/json").WithString("{}")))
					.Exactly(expected);
			}

			[Test]
			[Arguments(nameof(HttpMethod.Delete), 0)]
			[Arguments(nameof(HttpMethod.Get), 0)]
			[Arguments(nameof(HttpMethod.Post), 1)]
			[Arguments(nameof(HttpMethod.Put), 0)]
			public async Task StringUri_ShouldVerifyHttpMethod(string method, int expected)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();

				await httpClient.SendAsync(
					new HttpRequestMessage(new HttpMethod(method), "https://www.aweXpect.com"),
					CancellationToken.None);

				await That(httpClient.VerifyMock.Invoked.PostAsync(
						It.IsAny<string?>(),
						It.IsAny<HttpContent>()))
					.Exactly(expected);
			}

			[Test]
			[Arguments("*aweXpect.com*", 1)]
			[Arguments("*aweXpect.com", 1)]
			[Arguments("aweXpect.com*", 0)]
			[Arguments("*foo*", 0)]
			public async Task StringUri_ShouldVerifyUriString(string pattern, int expected)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();

				await httpClient.PostAsync("https://www.aweXpect.com", new StringContent(""), CancellationToken.None);

				await That(httpClient.VerifyMock.Invoked.PostAsync(
						It.Matches(pattern),
						It.IsAny<HttpContent>(),
						It.IsAny<CancellationToken>()))
					.Exactly(expected);
			}

			[Test]
			[Arguments(true)]
			[Arguments(false)]
			public async Task StringUri_WithCancellationToken_ShouldVerifyCancellationToken(bool tokenMatches)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();

				await httpClient.PostAsync("https://www.aweXpect.com", new StringContent(""), CancellationToken.None);

				await That(httpClient.VerifyMock.Invoked.PostAsync(
						It.Matches("*"),
						It.IsAny<HttpContent>(),
						It.Satisfies<CancellationToken>(_ => tokenMatches)))
					.Exactly(tokenMatches ? 1 : 0);
			}

			[Test]
			public async Task StringUri_WithoutMockedHttpMessageHandler_ShouldThrowMockException()
			{
				HttpClient httpClient = Mock.Create<HttpClient>(BaseClass.WithConstructorParameters());

				void Act()
				{
					httpClient.VerifyMock.Invoked
						.PostAsync(It.Matches("*aweXpect.com*")).Never();
				}

				await That(Act).Throws<MockException>()
					.WithMessage(
						"Cannot verify HttpClient when it is not mocked with a mockable HttpMessageHandler.");
			}

			[Test]
			[Arguments("application/json", 1)]
			[Arguments("text/plain", 0)]
			[Arguments("application/txt", 0)]
			public async Task Uri_ShouldVerifyHttpContent(string mediaType, int expected)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();

				await httpClient.PostAsync("https://www.aweXpect.com",
					new StringContent("{}", Encoding.UTF8, mediaType),
					CancellationToken.None);

				await That(httpClient.VerifyMock.Invoked.PostAsync(
						It.IsUri("*aweXpect.com*"),
						It.IsHttpContent("application/json").WithString("{}")))
					.Exactly(expected);
			}

			[Test]
			[Arguments(nameof(HttpMethod.Delete), 0)]
			[Arguments(nameof(HttpMethod.Get), 0)]
			[Arguments(nameof(HttpMethod.Post), 1)]
			[Arguments(nameof(HttpMethod.Put), 0)]
			public async Task Uri_ShouldVerifyHttpMethod(string method, int expected)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();

				await httpClient.SendAsync(
					new HttpRequestMessage(new HttpMethod(method), "https://www.aweXpect.com"),
					CancellationToken.None);

				await That(httpClient.VerifyMock.Invoked.PostAsync(
						It.IsUri("*aweXpect.com*"),
						It.IsAny<HttpContent>()))
					.Exactly(expected);
			}

			[Test]
			[Arguments("*aweXpect.com*", 1)]
			[Arguments("*aweXpect.com", 1)]
			[Arguments("aweXpect.com*", 0)]
			[Arguments("*foo*", 0)]
			public async Task Uri_ShouldVerifyUri(string pattern, int expected)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();

				await httpClient.PostAsync("https://www.aweXpect.com", new StringContent(""), CancellationToken.None);

				await That(httpClient.VerifyMock.Invoked.PostAsync(
						It.IsUri(pattern),
						It.IsAny<HttpContent>()))
					.Exactly(expected);
			}

			[Test]
			[Arguments(true)]
			[Arguments(false)]
			public async Task Uri_WithCancellationToken_ShouldVerifyCancellationToken(bool tokenMatches)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();

				await httpClient.PostAsync("https://www.aweXpect.com", new StringContent(""), CancellationToken.None);

				await That(httpClient.VerifyMock.Invoked.PostAsync(
						It.IsUri("*aweXpect.com*"),
						It.IsAny<HttpContent>(),
						It.Satisfies<CancellationToken>(_ => tokenMatches)))
					.Exactly(tokenMatches ? 1 : 0);
			}

			[Test]
			public async Task Uri_WithoutMockedHttpMessageHandler_ShouldThrowMockException()
			{
				HttpClient httpClient = Mock.Create<HttpClient>(BaseClass.WithConstructorParameters());

				void Act()
				{
					httpClient.VerifyMock.Invoked
						.PostAsync(It.IsUri("*aweXpect.com*")).Never();
				}

				await That(Act).Throws<MockException>()
					.WithMessage(
						"Cannot verify HttpClient when it is not mocked with a mockable HttpMessageHandler.");
			}
		}
	}
}
