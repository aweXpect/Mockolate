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
			[Test]
			[Arguments("https://www.aweXpect.com", true)]
			[Arguments("http://www.aweXpect.com", false)]
			public async Task ShouldSupportPatternWithUriConfiguration(string uri, bool expectMatch)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.SendAsync(It.IsHttpRequestMessage().WhoseUriIs("*awexpect*", u => u.ForHttps()))
					.ReturnsAsync(HttpStatusCode.OK);

				HttpResponseMessage result = await httpClient.GetAsync(uri, CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectMatch ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}

			[Test]
			[Arguments("https://www.aweXpect.com", true)]
			[Arguments("http://www.aweXpect.com", false)]
			public async Task ShouldSupportUriConfiguration(string uri, bool expectMatch)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.SendAsync(It.IsHttpRequestMessage().WhoseUriIs(u => u.ForHttps()))
					.ReturnsAsync(HttpStatusCode.OK);

				HttpResponseMessage result = await httpClient.GetAsync(uri, CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectMatch ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}

			[Test]
			[Arguments("https://www.aweXpect.com/foo/bar?x=123&y=4", "https://www.aweXpect.com/foo/bar?x=123&y=4",
				true)]
			[Arguments("https://www.aweXpect.com/foo/bar?x=123&y=4", "http://www.aweXpect.com/foo/bar?x=123&y=4",
				false)]
			[Arguments("https://www.aweXpect.com/foo/bar?x=123&y=4", "https://www.aweXpect.com/foo/baz?x=123&y=4",
				false)]
			[Arguments("https://www.aweXpect.com/foo/bar?x=123&y=4", "https://www.aweXpect.com/foo/bar?x=124&y=4",
				false)]
			[Arguments("https://www.aweXpect.com/foo/bar?x=123&y=4", "https://www.aweXpect.com/foo/bar?x=123", false)]
			[Arguments("https://www.aweXpect.com/foo/bar?x=123&y=4", "*www.aweXpect.com*", true)]
			[Arguments("https://www.aweXpect.com/foo/bar?x=123&y=4", "*/foo/bar*", true)]
			[Arguments("https://www.aweXpect.com/foo/bar?x=123&y=4", "*x=123*", true)]
			[Arguments("https://www.aweXpect.com/foo/bar?x=123&y=4", "*y=4*", true)]
			[Arguments("https://www.aweXpect.com/foo/bar?x=123&y=4", "https*", true)]
			public async Task ShouldVerifyFullUriWithWildcardMatch(string uri, string pattern, bool expectMatch)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.SendAsync(It.IsHttpRequestMessage().WhoseUriIs(pattern))
					.ReturnsAsync(HttpStatusCode.OK);

				HttpResponseMessage result = await httpClient.GetAsync(uri, CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectMatch ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}

			[Test]
			[Arguments("*aweXpect.com")]
			[Arguments("*aweXpect.com/")]
			public async Task TrailingSlash_ShouldBeIgnored(string matchPattern)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.SendAsync(It.IsHttpRequestMessage()
						.WhoseUriIs(matchPattern))
					.ReturnsAsync(HttpStatusCode.OK);

				HttpResponseMessage result =
					await httpClient.GetAsync("https://www.aweXpect.com", CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(HttpStatusCode.OK);
			}

			[Test]
			public async Task TrailingSlash_WhenNotPresent_ShouldNotBeAdded()
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.SendAsync(It.IsHttpRequestMessage()
						.WhoseUriIs("*www.aweXpect.com/foo/"))
					.ReturnsAsync(HttpStatusCode.OK);

				HttpResponseMessage result =
					await httpClient.GetAsync("https://www.aweXpect.com/foo", CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(HttpStatusCode.NotImplemented);
			}
		}
	}
}
