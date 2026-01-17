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
		public sealed class DeleteTests
		{
			[Theory]
			[InlineData(nameof(HttpMethod.Delete), 1)]
			[InlineData(nameof(HttpMethod.Get), 0)]
			[InlineData(nameof(HttpMethod.Post), 0)]
			[InlineData(nameof(HttpMethod.Put), 0)]
			public async Task StringUri_ShouldVerifyHttpMethod(string method, int expected)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();

				await httpClient.SendAsync(
					new HttpRequestMessage(new HttpMethod(method), "https://www.aweXpect.com"),
					CancellationToken.None);

				await That(httpClient.VerifyMock.Invoked.DeleteAsync(
						It.IsAny<string?>()))
					.Exactly(expected);
			}

			[Theory]
			[InlineData("*aweXpect.com*", 1)]
			[InlineData("*aweXpect.com", 1)]
			[InlineData("aweXpect.com*", 0)]
			[InlineData("*foo*", 0)]
			public async Task StringUri_ShouldVerifyUriString(string pattern, int expected)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();

				await httpClient.DeleteAsync("https://www.aweXpect.com", CancellationToken.None);

				await That(httpClient.VerifyMock.Invoked.DeleteAsync(
						It.Matches(pattern),
						It.IsAny<CancellationToken>()))
					.Exactly(expected);
			}

			[Theory]
			[InlineData(true)]
			[InlineData(false)]
			public async Task StringUri_WithCancellationToken_ShouldVerifyCancellationToken(bool tokenMatches)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();

				await httpClient.DeleteAsync("https://www.aweXpect.com", CancellationToken.None);

				await That(httpClient.VerifyMock.Invoked.DeleteAsync(
						It.Matches("*"),
						It.Satisfies<CancellationToken>(_ => tokenMatches)))
					.Exactly(tokenMatches ? 1 : 0);
			}

			[Fact]
			public async Task StringUri_WithoutMockedHttpMessageHandler_ShouldThrowMockException()
			{
				HttpClient httpClient = Mock.Create<HttpClient>(BaseClass.WithConstructorParameters());

				void Act()
				{
					httpClient.VerifyMock.Invoked
						.DeleteAsync(It.Matches("*aweXpect.com*")).Never();
				}

				await That(Act).Throws<MockException>()
					.WithMessage(
						"Cannot verify HttpClient when it is not mocked with a mockable HttpMessageHandler.");
			}

			[Theory]
			[InlineData(nameof(HttpMethod.Delete), 1)]
			[InlineData(nameof(HttpMethod.Get), 0)]
			[InlineData(nameof(HttpMethod.Post), 0)]
			[InlineData(nameof(HttpMethod.Put), 0)]
			public async Task Uri_ShouldVerifyHttpMethod(string method, int expected)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();

				await httpClient.SendAsync(
					new HttpRequestMessage(new HttpMethod(method), "https://www.aweXpect.com"),
					CancellationToken.None);

				await That(httpClient.VerifyMock.Invoked.DeleteAsync(
						It.IsUri("*aweXpect.com*")))
					.Exactly(expected);
			}

			[Theory]
			[InlineData("*aweXpect.com*", 1)]
			[InlineData("*aweXpect.com", 1)]
			[InlineData("aweXpect.com*", 0)]
			[InlineData("*foo*", 0)]
			public async Task Uri_ShouldVerifyUri(string pattern, int expected)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();

				await httpClient.DeleteAsync("https://www.aweXpect.com", CancellationToken.None);

				await That(httpClient.VerifyMock.Invoked.DeleteAsync(
						It.IsUri(pattern)))
					.Exactly(expected);
			}

			[Theory]
			[InlineData(true)]
			[InlineData(false)]
			public async Task Uri_WithCancellationToken_ShouldVerifyCancellationToken(bool tokenMatches)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();

				await httpClient.DeleteAsync("https://www.aweXpect.com", CancellationToken.None);

				await That(httpClient.VerifyMock.Invoked.DeleteAsync(
						It.IsUri("*aweXpect.com*"),
						It.Satisfies<CancellationToken>(_ => tokenMatches)))
					.Exactly(tokenMatches ? 1 : 0);
			}

			[Fact]
			public async Task Uri_WithoutMockedHttpMessageHandler_ShouldThrowMockException()
			{
				HttpClient httpClient = Mock.Create<HttpClient>(BaseClass.WithConstructorParameters());

				void Act()
				{
					httpClient.VerifyMock.Invoked
						.DeleteAsync(It.IsUri("*aweXpect.com*")).Never();
				}

				await That(Act).Throws<MockException>()
					.WithMessage(
						"Cannot verify HttpClient when it is not mocked with a mockable HttpMessageHandler.");
			}
		}
	}
}
