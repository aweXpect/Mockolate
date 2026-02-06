using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using Mockolate.Web;

namespace Mockolate.Tests.Web;

public sealed partial class ItExtensionsTests
{
	public sealed partial class IsHttpRequestMessageTests
	{
		public sealed class WhoseContentIsTests
		{
			[Theory]
			[InlineData("application/json", true)]
			[InlineData("text/plain", false)]
			[InlineData("application/txt", false)]
			public async Task ShouldVerifyMediaType(string mediaType, bool expectSuccess)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.SendAsync(It.IsHttpRequestMessage().WhoseContentIs(c => c.WithMediaType("application/json")))
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					new StringContent("", Encoding.UTF8, mediaType),
					CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}
		}
	}
}
