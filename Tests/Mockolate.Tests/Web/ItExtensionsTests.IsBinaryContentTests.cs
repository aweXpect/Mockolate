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
	public sealed class IsBinaryContentTests
	{
		[Theory]
		[InlineData(new byte[0], new byte[0], true)]
		[InlineData(new byte[] { 0x1, }, new byte[] { 0x1, }, true)]
		[InlineData(new byte[] { 0x1, }, new byte[] { 0x2, }, false)]
		[InlineData(new byte[] { 0x1, 0x2, 0x3, }, new byte[] { 0x1, }, true)]
		[InlineData(new byte[] { 0x1, 0x2, 0x3, }, new byte[] { 0x2, }, true)]
		[InlineData(new byte[] { 0x1, 0x2, 0x3, }, new byte[] { 0x3, }, true)]
		[InlineData(new byte[] { 0x1, 0x2, 0x3, }, new byte[] { 0x1, 0x2, }, true)]
		[InlineData(new byte[] { 0x1, 0x2, 0x3, }, new byte[] { 0x2, 0x3, }, true)]
		[InlineData(new byte[] { 0x1, 0x2, 0x3, }, new byte[] { 0x1, 0x3, }, false)]
		[InlineData(new byte[] { 0x1, 0x2, 0x3, }, new byte[] { 0x1, 0x2, 0x3, }, true)]
		[InlineData(new byte[] { 0x1, 0x2, 0x3, }, new byte[] { 0x1, 0x2, 0x3, 0x4, }, false)]
		public async Task Containing_ShouldCheckForEquality(byte[] body, byte[] expected, bool expectSuccess)
		{
			HttpClient httpClient = Mock.Create<HttpClient>();
			httpClient.SetupMock.Method
				.PostAsync(It.IsAny<Uri>(), It.IsBinaryContent().Containing(expected))
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

			HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
				new ByteArrayContent(body),
				CancellationToken.None);

			await That(result.StatusCode).IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
		}

		[Theory]
		[InlineData(new byte[0], new byte[0], true)]
		[InlineData(new byte[] { 0x66, }, new byte[] { 0x66, }, true)]
		[InlineData(new byte[] { 0x66, }, new byte[] { 0x67, }, false)]
		[InlineData(new byte[] { 0x66, 0x67, }, new byte[] { 0x67, }, false)]
		[InlineData(new byte[] { 0x66, 0x67, }, new byte[] { 0x67, 0x68, 0x69, }, false)]
		public async Task EqualTo_ShouldCheckForEquality(byte[] body, byte[] expected, bool expectSuccess)
		{
			HttpClient httpClient = Mock.Create<HttpClient>();
			httpClient.SetupMock.Method
				.PostAsync(It.IsAny<Uri>(), It.IsBinaryContent().EqualTo(expected))
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

			HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
				new ByteArrayContent(body),
				CancellationToken.None);

			await That(result.StatusCode).IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
		}
		
#if !NETFRAMEWORK
		[Fact]
		public async Task ShouldSupportMonitoring()
		{
			List<ByteArrayContent> responses =
			[
				new([]),
				new([0x66,]),
				new([0x62, 0x61, 0x72,]),
			];
			HttpClient httpClient = Mock.Create<HttpClient>();
			httpClient.SetupMock.Method.PostAsync(It.IsAny<Uri>(),
				It.IsBinaryContent().Monitor(out IParameterMonitor<HttpContent?> monitor));

			foreach (ByteArrayContent response in responses)
			{
				await httpClient.PostAsync("https://www.aweXpect.com", response);
			}

			await That(
					(await Task.WhenAll(monitor.Values.Select(c => c!.ReadAsByteArrayAsync()))).Select(x => x.Length))
				.IsEqualTo([0, 1, 3,]);
		}
#endif

		[Theory]
		[InlineData("image/png", true)]
		[InlineData("text/plain", false)]
		[InlineData("image/gif", false)]
		public async Task ShouldVerifyMediaType(string mediaType, bool expectSuccess)
		{
			HttpClient httpClient = Mock.Create<HttpClient>();
			httpClient.SetupMock.Method
				.PostAsync(It.IsAny<Uri>(), It.IsBinaryContent("image/png"))
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
			ByteArrayContent content = new([]);
			content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);

			HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
				content,
				CancellationToken.None);

			await That(result.StatusCode).IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
		}

		[Theory]
		[InlineData("image/png", true)]
		[InlineData("text/plain", false)]
		[InlineData("image/gif", false)]
		public async Task WithMediaType_ShouldVerifyMediaType(string mediaType, bool expectSuccess)
		{
			HttpClient httpClient = Mock.Create<HttpClient>();
			httpClient.SetupMock.Method
				.PostAsync(It.IsAny<Uri>(), It.IsBinaryContent().WithMediaType("image/png"))
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
			ByteArrayContent content = new([]);
			content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);

			HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
				content,
				CancellationToken.None);

			await That(result.StatusCode).IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
		}
	}
}
