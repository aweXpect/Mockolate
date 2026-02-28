using System.Net;
using System.Net.Http;
using System.Threading;
using Mockolate.Web;

namespace Mockolate.Tests.Web;

public sealed partial class ItExtensionsTests
{
	public sealed partial class IsUriTests
	{
		public sealed class WithPathTests
		{
			[Theory]
			[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=234", "/foo/bar", true)]
			[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=234", "*foo*", true)]
			[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=234", "*FOO*", true)]
			[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=234", "*bar*", true)]
			[InlineData("http://www.aweXpect.com/foo/bar?x=123&y=234", "*baz*", false)]
			public async Task ShouldVerifyPath(string uri, string pathPattern, bool expectMatch)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.GetAsync(It.IsUri().WithPath(pathPattern))
					.ReturnsAsync(HttpStatusCode.OK);

				HttpResponseMessage result = await httpClient.GetAsync(uri, CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectMatch ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}
		}
	}
}
