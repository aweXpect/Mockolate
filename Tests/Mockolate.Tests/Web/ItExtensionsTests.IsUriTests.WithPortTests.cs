using System.Net;
using System.Net.Http;
using System.Threading;
using Mockolate.Web;

namespace Mockolate.Tests.Web;

public sealed partial class ItExtensionsTests
{
	public sealed partial class IsUriTests
	{
		public sealed class WithPortTests
		{
			[Theory]
			[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=234", 443, true)]
			[InlineData("http://www.aweXpect.com/foo/bar?x=123&y=234", 80, true)]
			[InlineData("https://www.aweXpect.com:8080/foo/bar?x=123&y=234", 8080, true)]
			[InlineData("https://www.aweXpect.com:442/foo/bar?x=123&y=234", 443, false)]
			[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=234", 442, false)]
			public async Task ShouldVerifyPort(string uri, int port, bool expectMatch)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.GetAsync(It.IsUri().WithPort(port))
					.ReturnsAsync(HttpStatusCode.OK);

				HttpResponseMessage result = await httpClient.GetAsync(uri, CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectMatch ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}
		}
	}
}
