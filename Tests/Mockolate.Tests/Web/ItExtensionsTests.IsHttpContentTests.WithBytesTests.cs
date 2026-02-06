using System.Net;
using System.Net.Http;
using System.Threading;
using Mockolate.Web;

namespace Mockolate.Tests.Web;

public sealed partial class ItExtensionsTests
{
	public sealed partial class IsHttpContentTests
	{
		public sealed class WithBytesTests
		{
			[Theory]
			[InlineData(new byte[0], new byte[0], true)]
			[InlineData(new byte[] { 0x66, }, new byte[] { 0x66, }, true)]
			[InlineData(new byte[] { 0x66, }, new byte[] { 0x67, }, false)]
			[InlineData(new byte[] { 0x66, 0x67, }, new byte[] { 0x67, }, false)]
			[InlineData(new byte[] { 0x66, 0x67, }, new byte[] { 0x67, 0x68, 0x69, }, false)]
			public async Task ShouldCheckForEquality(byte[] body, byte[] expected, bool expectSuccess)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithBytes(expected))
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
