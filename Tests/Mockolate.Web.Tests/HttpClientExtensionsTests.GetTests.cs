using System.Net;
using System.Net.Http;
using System.Threading;

namespace Mockolate.Web.Tests;

public sealed partial class HttpClientExtensionsTests
{
	public sealed class GetTests
	{
		[Theory]
		[InlineData(nameof(HttpMethod.Get), true)]
		[InlineData(nameof(HttpMethod.Post), false)]
		[InlineData(nameof(HttpMethod.Put), false)]
		[InlineData(nameof(HttpMethod.Delete), false)]
		public async Task StringUri_ShouldVerifyHttpMethod(string method, bool expectSuccess)
		{
			HttpClient httpClient = Mock.Create<HttpClient>();
			httpClient.SetupMock.Method
				.GetAsync(It.Matches("*aweXpect.com*"))
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

			HttpResponseMessage result = await httpClient.SendAsync(
				new HttpRequestMessage(new HttpMethod(method), "https://www.aweXpect.com"),
				CancellationToken.None);

			await That(result.StatusCode).IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
		}

		[Theory]
		[InlineData("*aweXpect.com*", true)]
		[InlineData("*aweXpect.com", true)]
		[InlineData("aweXpect.com*", false)]
		[InlineData("*foo*", false)]
		public async Task StringUri_ShouldVerifyUriString(string pattern, bool expectSuccess)
		{
			HttpClient httpClient = Mock.Create<HttpClient>();
			httpClient.SetupMock.Method
				.GetAsync(It.Matches(pattern))
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

			HttpResponseMessage result = await httpClient.GetAsync("https://www.aweXpect.com", CancellationToken.None);

			await That(result.StatusCode).IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task StringUri_WithCancellationToken_ShouldVerifyCancellationToken(bool tokenMatches)
		{
			HttpClient httpClient = Mock.Create<HttpClient>();
			httpClient.SetupMock.Method
				.GetAsync(
					It.Matches("*aweXpect.com*"),
					It.Satisfies<CancellationToken>(_ => tokenMatches))
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

			HttpResponseMessage result = await httpClient.GetAsync("https://www.aweXpect.com", CancellationToken.None);

			await That(result.StatusCode).IsEqualTo(tokenMatches ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
		}

		[Theory]
		[InlineData(nameof(HttpMethod.Get), true)]
		[InlineData(nameof(HttpMethod.Post), false)]
		[InlineData(nameof(HttpMethod.Put), false)]
		[InlineData(nameof(HttpMethod.Delete), false)]
		public async Task Uri_ShouldVerifyHttpMethod(string method, bool expectSuccess)
		{
			HttpClient httpClient = Mock.Create<HttpClient>();
			httpClient.SetupMock.Method
				.GetAsync(It.IsUri("*aweXpect.com*"))
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

			HttpResponseMessage result = await httpClient.SendAsync(
				new HttpRequestMessage(new HttpMethod(method), "https://www.aweXpect.com"),
				CancellationToken.None);

			await That(result.StatusCode).IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
		}

		[Theory]
		[InlineData("*aweXpect.com*", true)]
		[InlineData("*aweXpect.com", true)]
		[InlineData("aweXpect.com*", false)]
		[InlineData("*foo*", false)]
		public async Task Uri_ShouldVerifyUri(string pattern, bool expectSuccess)
		{
			HttpClient httpClient = Mock.Create<HttpClient>();
			httpClient.SetupMock.Method
				.GetAsync(It.IsUri(pattern))
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

			HttpResponseMessage result = await httpClient.GetAsync("https://www.aweXpect.com", CancellationToken.None);

			await That(result.StatusCode).IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task Uri_WithCancellationToken_ShouldVerifyCancellationToken(bool tokenMatches)
		{
			HttpClient httpClient = Mock.Create<HttpClient>();
			httpClient.SetupMock.Method
				.GetAsync(
					It.IsUri("*aweXpect.com*"),
					It.Satisfies<CancellationToken>(_ => tokenMatches))
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

			HttpResponseMessage result = await httpClient.GetAsync("https://www.aweXpect.com", CancellationToken.None);

			await That(result.StatusCode).IsEqualTo(tokenMatches ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
		}
	}
}
