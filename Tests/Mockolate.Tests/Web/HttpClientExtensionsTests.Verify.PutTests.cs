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
		public sealed class PutTests
		{
			[Theory]
			[InlineData("application/json", 1)]
			[InlineData("text/plain", 0)]
			[InlineData("application/txt", 0)]
			public async Task StringUri_ShouldVerifyHttpContent(string mediaType, int expected)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();

				await httpClient.PutAsync("https://www.aweXpect.com",
					new StringContent("{}", Encoding.UTF8, mediaType),
					CancellationToken.None);

				await That(httpClient.VerifyMock.Invoked.PutAsync(
						It.IsAny<string?>(),
						It.IsHttpContent("application/json").WithString("{}")))
					.Exactly(expected);
			}

			[Theory]
			[InlineData(nameof(HttpMethod.Delete), 0)]
			[InlineData(nameof(HttpMethod.Get), 0)]
			[InlineData(nameof(HttpMethod.Post), 0)]
			[InlineData(nameof(HttpMethod.Put), 1)]
			public async Task StringUri_ShouldVerifyHttpMethod(string method, int expected)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();

				await httpClient.SendAsync(
					new HttpRequestMessage(new HttpMethod(method), "https://www.aweXpect.com"),
					CancellationToken.None);

				await That(httpClient.VerifyMock.Invoked.PutAsync(
						It.IsAny<string?>(),
						It.IsAny<HttpContent>()))
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

				await httpClient.PutAsync("https://www.aweXpect.com", new StringContent(""), CancellationToken.None);

				await That(httpClient.VerifyMock.Invoked.PutAsync(
						It.Matches(pattern),
						It.IsAny<HttpContent>(),
						It.IsAny<CancellationToken>()))
					.Exactly(expected);
			}

			[Theory]
			[InlineData(true)]
			[InlineData(false)]
			public async Task StringUri_WithCancellationToken_ShouldVerifyCancellationToken(bool tokenMatches)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();

				await httpClient.PutAsync("https://www.aweXpect.com", new StringContent(""), CancellationToken.None);

				await That(httpClient.VerifyMock.Invoked.PutAsync(
						It.Matches("*"),
						It.IsAny<HttpContent>(),
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
						.PutAsync(It.Matches("*aweXpect.com*")).Never();
				}

				await That(Act).Throws<MockException>()
					.WithMessage(
						"Cannot verify HttpClient when it is not mocked with a mockable HttpMessageHandler.");
			}

			[Theory]
			[InlineData("application/json", 1)]
			[InlineData("text/plain", 0)]
			[InlineData("application/txt", 0)]
			public async Task Uri_ShouldVerifyHttpContent(string mediaType, int expected)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();

				await httpClient.PutAsync("https://www.aweXpect.com",
					new StringContent("{}", Encoding.UTF8, mediaType),
					CancellationToken.None);

				await That(httpClient.VerifyMock.Invoked.PutAsync(
						It.IsUri("*aweXpect.com*"),
						It.IsHttpContent("application/json").WithString("{}")))
					.Exactly(expected);
			}

			[Theory]
			[InlineData(nameof(HttpMethod.Delete), 0)]
			[InlineData(nameof(HttpMethod.Get), 0)]
			[InlineData(nameof(HttpMethod.Post), 0)]
			[InlineData(nameof(HttpMethod.Put), 1)]
			public async Task Uri_ShouldVerifyHttpMethod(string method, int expected)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();

				await httpClient.SendAsync(
					new HttpRequestMessage(new HttpMethod(method), "https://www.aweXpect.com"),
					CancellationToken.None);

				await That(httpClient.VerifyMock.Invoked.PutAsync(
						It.IsUri("*aweXpect.com*"),
						It.IsAny<HttpContent>()))
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

				await httpClient.PutAsync("https://www.aweXpect.com", new StringContent(""), CancellationToken.None);

				await That(httpClient.VerifyMock.Invoked.PutAsync(
						It.IsUri(pattern),
						It.IsAny<HttpContent>()))
					.Exactly(expected);
			}

			[Theory]
			[InlineData(true)]
			[InlineData(false)]
			public async Task Uri_WithCancellationToken_ShouldVerifyCancellationToken(bool tokenMatches)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();

				await httpClient.PutAsync("https://www.aweXpect.com", new StringContent(""), CancellationToken.None);

				await That(httpClient.VerifyMock.Invoked.PutAsync(
						It.IsUri("*aweXpect.com*"),
						It.IsAny<HttpContent>(),
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
						.PutAsync(It.IsUri("*aweXpect.com*")).Never();
				}

				await That(Act).Throws<MockException>()
					.WithMessage(
						"Cannot verify HttpClient when it is not mocked with a mockable HttpMessageHandler.");
			}
		}
	}
}
