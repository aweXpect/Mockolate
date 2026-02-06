using System.Net;
using System.Net.Http;
using System.Threading;
using Mockolate.Web;

namespace Mockolate.Tests.Web;

public sealed partial class ItExtensionsTests
{
	public sealed partial class IsHttpRequestMessageTests
	{
		[Theory]
		[InlineData(nameof(HttpMethod.Get), true)]
		[InlineData(nameof(HttpMethod.Delete), false)]
		[InlineData(nameof(HttpMethod.Post), false)]
		[InlineData(nameof(HttpMethod.Put), false)]
		public async Task WithMethod_ShouldVerifyMethod(string method, bool expectSuccess)
		{
			HttpClient httpClient = Mock.Create<HttpClient>();
			httpClient.SetupMock.Method
				.SendAsync(It.IsHttpRequestMessage(new HttpMethod(method)))
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
			StringContent content = new("");

			HttpResponseMessage result = await httpClient.GetAsync("https://www.aweXpect.com",
				CancellationToken.None);

			await That(result.StatusCode).IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
		}
	}
}
