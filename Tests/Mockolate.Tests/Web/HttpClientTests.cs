using System.Net;
using System.Net.Http;
using System.Threading;
using Mockolate.Setup;
using Mockolate.Web;

namespace Mockolate.Tests.Web;

public class HttpClientTests
{
	[Fact]
	public async Task HttpClientMock_WithHandler_ShouldUseSharedRegistry()
	{
		HttpMessageHandler handler = HttpMessageHandler.CreateMock();
		HttpClient client1 = HttpClient.CreateMock([handler,]);
		HttpClient client2 = HttpClient.CreateMock([handler,]);
		IMethodSetup setup = client1.Mock.Setup
			.GetAsync(It.IsUri("aweXpect.com"))
			.ReturnsAsync(HttpStatusCode.Accepted);

		HttpResponseMessage response = await client2.GetAsync("https://aweXpect.com", CancellationToken.None);

		await That(response.StatusCode).IsEqualTo(HttpStatusCode.Accepted);
		await That(client1.Mock.VerifySetup(setup)).Once();
		await That(client2.Mock.VerifySetup(setup)).Once();
	}
}
