using System.Net;
using System.Net.Http;
using System.Threading;
using Mockolate.Web;

namespace Mockolate.Tests.Web;

public sealed partial class ItExtensionsTests
{
	public sealed partial class IsHttpContentTests
	{
		public sealed class WithBytesContainingTests
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
			public async Task ShouldCheckForEquality(byte[] body, byte[] expected, bool expectSuccess)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithBytesContaining(expected))
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					new ByteArrayContent(body),
					CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}
		}
	}
}
