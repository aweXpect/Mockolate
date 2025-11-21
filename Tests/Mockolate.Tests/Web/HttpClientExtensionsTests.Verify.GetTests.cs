using System.Net.Http;
using System.Threading;
using Mockolate.Exceptions;
using Mockolate.Verify;
using Mockolate.Web;

namespace Mockolate.Tests.Web;

public sealed partial class HttpClientExtensionsTests
{
	public sealed partial class Verify
	{
		public sealed class GetTests
		{
			[Test]
			[Arguments(nameof(HttpMethod.Delete), 0)]
			[Arguments(nameof(HttpMethod.Get), 1)]
			[Arguments(nameof(HttpMethod.Post), 0)]
			[Arguments(nameof(HttpMethod.Put), 0)]
			public async Task StringUri_ShouldVerifyHttpMethod(string method, int expected)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();

				await httpClient.SendAsync(
					new HttpRequestMessage(new HttpMethod(method), "https://www.aweXpect.com"),
					CancellationToken.None);

				await That(httpClient.VerifyMock.Invoked.GetAsync(
						It.IsAny<string?>()))
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

				await httpClient.GetAsync("https://www.aweXpect.com", CancellationToken.None);

				await That(httpClient.VerifyMock.Invoked.GetAsync(
						It.Matches(pattern),
						It.IsAny<CancellationToken>()))
					.Exactly(expected);
			}

			[Test]
			[Arguments(true)]
			[Arguments(false)]
			public async Task StringUri_WithCancellationToken_ShouldVerifyCancellationToken(bool tokenMatches)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();

				await httpClient.GetAsync("https://www.aweXpect.com", CancellationToken.None);

				await That(httpClient.VerifyMock.Invoked.GetAsync(
						It.Matches("*"),
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
						.GetAsync(It.Matches("*aweXpect.com*")).Never();
				}

				await That(Act).Throws<MockException>()
					.WithMessage(
						"Cannot verify HttpClient when it is not mocked with a mockable HttpMessageHandler.");
			}

			[Test]
			[Arguments(nameof(HttpMethod.Delete), 0)]
			[Arguments(nameof(HttpMethod.Get), 1)]
			[Arguments(nameof(HttpMethod.Post), 0)]
			[Arguments(nameof(HttpMethod.Put), 0)]
			public async Task Uri_ShouldVerifyHttpMethod(string method, int expected)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();

				await httpClient.SendAsync(
					new HttpRequestMessage(new HttpMethod(method), "https://www.aweXpect.com"),
					CancellationToken.None);

				await That(httpClient.VerifyMock.Invoked.GetAsync(
						It.IsUri("*aweXpect.com*")))
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

				await httpClient.GetAsync("https://www.aweXpect.com", CancellationToken.None);

				await That(httpClient.VerifyMock.Invoked.GetAsync(
						It.IsUri(pattern)))
					.Exactly(expected);
			}

			[Test]
			[Arguments(true)]
			[Arguments(false)]
			public async Task Uri_WithCancellationToken_ShouldVerifyCancellationToken(bool tokenMatches)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();

				await httpClient.GetAsync("https://www.aweXpect.com", CancellationToken.None);

				await That(httpClient.VerifyMock.Invoked.GetAsync(
						It.IsUri("*aweXpect.com*"),
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
						.GetAsync(It.IsUri("*aweXpect.com*")).Never();
				}

				await That(Act).Throws<MockException>()
					.WithMessage(
						"Cannot verify HttpClient when it is not mocked with a mockable HttpMessageHandler.");
			}
		}
	}
}
