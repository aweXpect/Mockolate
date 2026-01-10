using System.Net;
using System.Net.Http;
using System.Threading;
using Mockolate.Web;

namespace Mockolate.Tests.Web;

public sealed partial class HttpClientExtensionsTests
{
	public sealed class DeleteTests
	{
		[Theory]
		[InlineData(nameof(HttpMethod.Delete), true)]
		[InlineData(nameof(HttpMethod.Post), false)]
		[InlineData(nameof(HttpMethod.Put), false)]
		[InlineData(nameof(HttpMethod.Get), false)]
		public async Task StringUri_ShouldVerifyHttpMethod(string method, bool expectSuccess)
		{
			HttpClient httpClient = Mock.Create<HttpClient>();
			httpClient.SetupMock.Method
				.DeleteAsync(It.Matches("*aweXpect.com*"))
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
				.DeleteAsync(It.Matches(pattern))
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

			HttpResponseMessage result = await httpClient.DeleteAsync("https://www.aweXpect.com", CancellationToken.None);

			await That(result.StatusCode).IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task StringUri_WithCancellationToken_ShouldVerifyCancellationToken(bool tokenMatches)
		{
			HttpClient httpClient = Mock.Create<HttpClient>();
			httpClient.SetupMock.Method
				.DeleteAsync(
					It.Matches("*aweXpect.com*"),
					It.Satisfies<CancellationToken>(_ => tokenMatches))
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

			HttpResponseMessage result = await httpClient.DeleteAsync("https://www.aweXpect.com", CancellationToken.None);

			await That(result.StatusCode).IsEqualTo(tokenMatches ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
		}

		[Theory]
		[InlineData(nameof(HttpMethod.Delete), true)]
		[InlineData(nameof(HttpMethod.Post), false)]
		[InlineData(nameof(HttpMethod.Put), false)]
		[InlineData(nameof(HttpMethod.Get), false)]
		public async Task Uri_ShouldVerifyHttpMethod(string method, bool expectSuccess)
		{
			HttpClient httpClient = Mock.Create<HttpClient>();
			httpClient.SetupMock.Method
				.DeleteAsync(It.IsUri("*aweXpect.com*"))
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
				.DeleteAsync(It.IsUri(pattern))
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

			HttpResponseMessage result = await httpClient.DeleteAsync("https://www.aweXpect.com", CancellationToken.None);

			await That(result.StatusCode).IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task Uri_WithCancellationToken_ShouldVerifyCancellationToken(bool tokenMatches)
		{
			HttpClient httpClient = Mock.Create<HttpClient>();
			httpClient.SetupMock.Method
				.DeleteAsync(
					It.IsUri("*aweXpect.com*"),
					It.Satisfies<CancellationToken>(_ => tokenMatches))
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

			HttpResponseMessage result = await httpClient.DeleteAsync("https://www.aweXpect.com", CancellationToken.None);

			await That(result.StatusCode).IsEqualTo(tokenMatches ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
		}
	}
}
