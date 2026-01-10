using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using Mockolate.Web;

namespace Mockolate.Tests.Web;

public sealed partial class HttpClientExtensionsTests
{
	public sealed class PostTests
	{
		[Theory]
		[InlineData("application/json", true)]
		[InlineData("text/plain", false)]
		[InlineData("application/txt", false)]
		public async Task StringUri_ShouldVerifyHttpContent(string mediaType, bool expectSuccess)
		{
			HttpClient httpClient = Mock.Create<HttpClient>();
			httpClient.SetupMock.Method
				.PostAsync(It.IsAny<string>(), It.HasJsonContent())
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

			HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
				new StringContent("", Encoding.UTF8, mediaType),
				CancellationToken.None);

			await That(result.StatusCode).IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
		}

		[Theory]
		[InlineData(nameof(HttpMethod.Get), false)]
		[InlineData(nameof(HttpMethod.Post), true)]
		[InlineData(nameof(HttpMethod.Put), false)]
		[InlineData(nameof(HttpMethod.Delete), false)]
		public async Task StringUri_ShouldVerifyHttpMethod(string method, bool expectSuccess)
		{
			HttpClient httpClient = Mock.Create<HttpClient>();
			httpClient.SetupMock.Method
				.PostAsync(It.Matches("*aweXpect.com*"), It.IsAny<HttpContent>())
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
				.PostAsync(It.Matches(pattern), It.IsAny<HttpContent>())
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

			HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com", new StringContent(""),
				CancellationToken.None);

			await That(result.StatusCode).IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task StringUri_WithCancellationToken_ShouldVerifyCancellationToken(bool tokenMatches)
		{
			HttpClient httpClient = Mock.Create<HttpClient>();
			httpClient.SetupMock.Method
				.PostAsync(
					It.Matches("*aweXpect.com*"),
					It.IsAny<HttpContent>(),
					It.Satisfies<CancellationToken>(_ => tokenMatches))
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

			HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com", new StringContent(""),
				CancellationToken.None);

			await That(result.StatusCode).IsEqualTo(tokenMatches ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
		}

		[Theory]
		[InlineData("application/json", true)]
		[InlineData("text/plain", false)]
		[InlineData("application/txt", false)]
		public async Task Uri_ShouldVerifyHttpContent(string mediaType, bool expectSuccess)
		{
			HttpClient httpClient = Mock.Create<HttpClient>();
			httpClient.SetupMock.Method
				.PostAsync(It.IsAny<Uri>(), It.HasJsonContent())
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

			HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
				new StringContent("", Encoding.UTF8, mediaType),
				CancellationToken.None);

			await That(result.StatusCode).IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
		}

		[Theory]
		[InlineData(nameof(HttpMethod.Get), false)]
		[InlineData(nameof(HttpMethod.Post), true)]
		[InlineData(nameof(HttpMethod.Put), false)]
		[InlineData(nameof(HttpMethod.Delete), false)]
		public async Task Uri_ShouldVerifyHttpMethod(string method, bool expectSuccess)
		{
			HttpClient httpClient = Mock.Create<HttpClient>();
			httpClient.SetupMock.Method
				.PostAsync(It.IsUri("*aweXpect.com*"), It.IsAny<HttpContent>())
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
				.PostAsync(It.IsUri(pattern), It.IsAny<HttpContent>())
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

			HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com", new StringContent(""),
				CancellationToken.None);

			await That(result.StatusCode).IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task Uri_WithCancellationToken_ShouldVerifyCancellationToken(bool tokenMatches)
		{
			HttpClient httpClient = Mock.Create<HttpClient>();
			httpClient.SetupMock.Method
				.PostAsync(
					It.IsUri("*aweXpect.com*"),
					It.IsAny<HttpContent>(),
					It.Satisfies<CancellationToken>(_ => tokenMatches))
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

			HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com", new StringContent(""),
				CancellationToken.None);

			await That(result.StatusCode).IsEqualTo(tokenMatches ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
		}
	}
}
