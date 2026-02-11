using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using Mockolate.Parameters;
using Mockolate.Web;

namespace Mockolate.Tests.Web;

public sealed partial class ItExtensionsTests
{
	public sealed partial class IsHttpContentTests
	{
		[Fact]
		public async Task ShouldSupportMonitoring()
		{
			int callbackCount = 0;
			List<ByteArrayContent> responses =
			[
				new([]),
				new([0x66,]),
				new([0x62, 0x61, 0x72,]),
			];
			HttpClient httpClient = Mock.Create<HttpClient>();
			httpClient.SetupMock.Method.PostAsync(It.IsAny<Uri>(),
				It.IsHttpContent()
					.Do(_ => callbackCount++)
					.Monitor(out IParameterMonitor<HttpContent?> monitor));

			foreach (ByteArrayContent response in responses)
			{
				await httpClient.PostAsync("https://www.aweXpect.com", response, CancellationToken.None);
			}

#if !NETFRAMEWORK
			await That(
					(await Task.WhenAll(monitor.Values.Select(c => c!.ReadAsByteArrayAsync())))
					.Select(x => x.Length))
				.IsEqualTo([0, 1, 3,]);
#endif
			await That(callbackCount).IsEqualTo(3);
		}

		[Fact]
		public async Task ShouldSupportMultipleCombinations()
		{
			byte[] bytes = "foo"u8.ToArray();
			HttpClient httpClient = Mock.Create<HttpClient>();
			httpClient.SetupMock.Method
				.PostAsync(It.IsAny<Uri>(), It.IsHttpContent()
					.WithStringMatching("*")
					.WithString("foo")
					.WithBytes(b => b.Length == 3))
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
			ByteArrayContent content = new(bytes);
			content.Headers.Add("x-my-header", "my-value");

			HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
				content,
				CancellationToken.None);

			await That(result.StatusCode).IsEqualTo(HttpStatusCode.OK);
		}

		[Fact]
		public async Task ShouldSupportWithHeadersInWrapper()
		{
			byte[] bytes = "foo"u8.ToArray();
			HttpClient httpClient = Mock.Create<HttpClient>();
			httpClient.SetupMock.Method
				.PostAsync(It.IsAny<Uri>(), It.IsHttpContent()
					.WithStringMatching("*")
					.WithHeaders("x-my-header", "my-value"))
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
			ByteArrayContent content = new(bytes);
			content.Headers.Add("x-my-header", "my-value");

			HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
				content,
				CancellationToken.None);

			await That(result.StatusCode).IsEqualTo(HttpStatusCode.OK);
		}

		[Theory]
		[InlineData("image/png", true)]
		[InlineData("text/plain", false)]
		[InlineData("image/gif", false)]
		public async Task ShouldVerifyMediaType(string mediaType, bool expectSuccess)
		{
			HttpClient httpClient = Mock.Create<HttpClient>();
			httpClient.SetupMock.Method
				.PostAsync(It.IsAny<Uri>(), It.IsHttpContent("image/png"))
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
			ByteArrayContent content = new([]);
			content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);

			HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
				content,
				CancellationToken.None);

			await That(result.StatusCode)
				.IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
		}

		[Theory]
		[InlineData("image/png", true)]
		[InlineData("text/plain", false)]
		[InlineData("image/gif", false)]
		public async Task WithMediaType_ShouldVerifyMediaType(string mediaType, bool expectSuccess)
		{
			HttpClient httpClient = Mock.Create<HttpClient>();
			httpClient.SetupMock.Method
				.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithMediaType("image/png"))
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
			ByteArrayContent content = new([]);
			content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);

			HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
				content,
				CancellationToken.None);

			await That(result.StatusCode)
				.IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
		}
	}
}
