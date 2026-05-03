using System.Net;
using System.Net.Http;
using System.Threading;
using Mockolate.Web;

namespace Mockolate.Tests.Web;

public sealed partial class ItExtensionsTests
{
	public sealed partial class IsHttpRequestMessageTests
	{
		public sealed class WhoseUriIsTests
		{
			[Theory]
			[InlineData("https://www.testably.org", true)]
			[InlineData("http://www.testably.org", false)]
			public async Task ShouldSupportPatternWithUriConfiguration(string uri, bool expectMatch)
			{
				HttpClient httpClient = HttpClient.CreateMock();
				httpClient.Mock.Setup
					.SendAsync(It.IsHttpRequestMessage().WhoseUriIs("*awexpect*", u => u.ForHttps()))
					.ReturnsAsync(HttpStatusCode.OK);

				HttpResponseMessage result = await httpClient.GetAsync(uri, CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectMatch ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}

			[Theory]
			[InlineData("https://www.testably.org", true)]
			[InlineData("http://www.testably.org", false)]
			public async Task ShouldSupportUriConfiguration(string uri, bool expectMatch)
			{
				HttpClient httpClient = HttpClient.CreateMock();
				httpClient.Mock.Setup
					.SendAsync(It.IsHttpRequestMessage().WhoseUriIs(u => u.ForHttps()))
					.ReturnsAsync(HttpStatusCode.OK);

				HttpResponseMessage result = await httpClient.GetAsync(uri, CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectMatch ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}

			[Theory]
			[InlineData("https://www.testably.org/foo/bar?x=123&y=4", "https://www.testably.org/foo/bar?x=123&y=4",
				true)]
			[InlineData("https://www.testably.org/foo/bar?x=123&y=4", "http://www.testably.org/foo/bar?x=123&y=4",
				false)]
			[InlineData("https://www.testably.org/foo/bar?x=123&y=4", "https://www.testably.org/foo/baz?x=123&y=4",
				false)]
			[InlineData("https://www.testably.org/foo/bar?x=123&y=4", "https://www.testably.org/foo/bar?x=124&y=4",
				false)]
			[InlineData("https://www.testably.org/foo/bar?x=123&y=4", "https://www.testably.org/foo/bar?x=123", true)]
			[InlineData("https://www.testably.org/foo/bar?x=123&y=4", "*www.testably.org*", true)]
			[InlineData("https://www.testably.org/foo/bar?x=123&y=4", "*/foo/bar*", true)]
			[InlineData("https://www.testably.org/foo/bar?x=123&y=4", "*x=123*", true)]
			[InlineData("https://www.testably.org/foo/bar?x=123&y=4", "*y=4*", true)]
			[InlineData("https://www.testably.org/foo/bar?x=123&y=4", "https*", true)]
			public async Task ShouldVerifyFullUriWithWildcardMatch(string uri, string pattern, bool expectMatch)
			{
				HttpClient httpClient = HttpClient.CreateMock();
				httpClient.Mock.Setup
					.SendAsync(It.IsHttpRequestMessage().WhoseUriIs(pattern))
					.ReturnsAsync(HttpStatusCode.OK);

				HttpResponseMessage result = await httpClient.GetAsync(uri, CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectMatch ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}

			[Theory]
			[InlineData("*testably.org")]
			[InlineData("*testably.org/")]
			public async Task TrailingSlash_ShouldBeIgnored(string matchPattern)
			{
				HttpClient httpClient = HttpClient.CreateMock();
				httpClient.Mock.Setup
					.SendAsync(It.IsHttpRequestMessage()
						.WhoseUriIs(matchPattern))
					.ReturnsAsync(HttpStatusCode.OK);

				HttpResponseMessage result =
					await httpClient.GetAsync("https://www.testably.org", CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(HttpStatusCode.OK);
			}

			[Fact]
			public async Task TrailingSlash_WhenNotPresent_ShouldNotBeAdded()
			{
				HttpClient httpClient = HttpClient.CreateMock();
				httpClient.Mock.Setup
					.SendAsync(It.IsHttpRequestMessage()
						.WhoseUriIs("*www.testably.org/foo/"))
					.ReturnsAsync(HttpStatusCode.OK);

				HttpResponseMessage result =
					await httpClient.GetAsync("https://www.testably.org/foo", CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(HttpStatusCode.NotImplemented);
			}
		}
	}
}
