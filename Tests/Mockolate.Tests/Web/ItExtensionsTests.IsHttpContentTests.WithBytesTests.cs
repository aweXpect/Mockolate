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
			[InlineData(new byte[0], 0x1, false)]
			[InlineData(new byte[] { 0x1, }, 0x1, true)]
			[InlineData(new byte[] { 0x1, }, 0x2, false)]
			[InlineData(new byte[] { 0x1, 0x2, 0x3, }, 0x1, true)]
			[InlineData(new byte[] { 0x1, 0x2, 0x3, }, 0x2, false)]
			[InlineData(new byte[] { 0x1, 0x2, 0x3, }, 0x3, false)]
			public async Task Predicate_ShouldValidatePredicate(byte[] body, byte expectedFirstByte, bool expectSuccess)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithBytes(b => b.Length > 0 && b[0] == expectedFirstByte))
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					new ByteArrayContent(body),
					CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}

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
