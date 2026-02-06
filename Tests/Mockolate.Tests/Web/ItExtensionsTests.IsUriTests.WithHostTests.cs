using System.Net;
using System.Net.Http;
using System.Threading;
using Mockolate.Web;

namespace Mockolate.Tests.Web;

public sealed partial class ItExtensionsTests
{
	public sealed partial class IsUriTests
	{
		public sealed class WithHostTests
		{
			[Theory]
			[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=234", "www.awexpect.com", true)]
			[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=234", "*awexpect*", true)]
			[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=234", "*aweXpect*", true)]
			[InlineData("http://www.aweXpect.com/foo/bar?x=123&y=234", "mockolate.com", false)]
			public async Task ShouldVerifyHost(string uri, string hostPattern, bool expectMatch)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.GetAsync(It.IsUri().WithHost(hostPattern))
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

				HttpResponseMessage result = await httpClient.GetAsync(uri, CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectMatch ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}
		}
	}
}
