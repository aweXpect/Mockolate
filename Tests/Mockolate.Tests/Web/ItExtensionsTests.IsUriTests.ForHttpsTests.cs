using System.Net;
using System.Net.Http;
using System.Threading;
using Mockolate.Web;

namespace Mockolate.Tests.Web;

public sealed partial class ItExtensionsTests
{
	public sealed partial class IsUriTests
	{
		public sealed class ForHttpsTests
		{
			[Theory]
			[InlineData("http://www.aweXpect.com/foo/bar?x=123&y=234", true)]
			[InlineData("HTTP://www.aweXpect.com/foo/bar?x=123&y=234", true)]
			[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=234", false)]
			public async Task ForHttp_ShouldVerifyHttpScheme(string uri, bool expectMatch)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.GetAsync(It.IsUri().ForHttp())
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

				HttpResponseMessage result = await httpClient.GetAsync(uri, CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectMatch ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}

			[Theory]
			[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=234", true)]
			[InlineData("HTTPS://www.aweXpect.com/foo/bar?x=123&y=234", true)]
			[InlineData("http://www.aweXpect.com/foo/bar?x=123&y=234", false)]
			public async Task ForHttps_ShouldVerifyHttpsScheme(string uri, bool expectMatch)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.GetAsync(It.IsUri().ForHttps())
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

				HttpResponseMessage result = await httpClient.GetAsync(uri, CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectMatch ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}
		}
	}
}
