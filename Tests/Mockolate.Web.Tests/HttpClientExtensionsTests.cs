using System.Net;
using System.Net.Http;
using Mockolate.Parameters;

namespace Mockolate.Web.Tests;

public sealed partial class HttpClientExtensionsTests
{
	[Fact]
	public async Task SupportMonitorOnUriParameters()
	{
		HttpClient httpClient = Mock.Create<HttpClient>();
		httpClient.SetupMock.Method
			.GetAsync(It.Matches("*aweXpect.com*").Monitor(out IParameterMonitor<string> monitor))
			.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

		await httpClient.GetAsync("https://www.aweXpect.com");
		await httpClient.PostAsync("https://www.awexpect.com/aweXpect.Web", null);
		await httpClient.GetAsync("https://www.awexpect.com/Mockolate");

		await That(monitor.Values).IsEqualTo(["https://www.awexpect.com/", "https://www.awexpect.com/Mockolate",]);
	}
}
