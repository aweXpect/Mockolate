using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using Mockolate.Parameters;
using Mockolate.Web;

namespace Mockolate.Tests.Web;

public sealed partial class ItExtensionsTests
{
	public sealed class HttpContentTests
	{
#if !NETFRAMEWORK
		[Fact]
		public async Task HasJsonContent_ShouldSupportMonitoring()
		{
			List<StringContent> responses =
			[
				new("", Encoding.UTF8, "application/json"),
				new("foo", Encoding.UTF8, "application/json"),
				new("bar", Encoding.UTF8, "application/json"),
			];
			HttpClient httpClient = Mock.Create<HttpClient>();
			httpClient.SetupMock.Method.PostAsync(It.IsAny<Uri>(),
				It.HasJsonContent().Monitor(out IParameterMonitor<HttpContent?> monitor));

			foreach (StringContent response in responses)
			{
				await httpClient.PostAsync("https://www.aweXpect.com", response);
			}

			await That(await Task.WhenAll(monitor.Values.Select(c => c!.ReadAsStringAsync())))
				.IsEqualTo(["", "foo", "bar",]);
		}
#endif

		[Theory]
		[InlineData("application/json", true)]
		[InlineData("text/plain", false)]
		[InlineData("application/txt", false)]
		public async Task HasJsonContent_ShouldVerifyMediaType(string mediaType, bool expectSuccess)
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
	}
}
