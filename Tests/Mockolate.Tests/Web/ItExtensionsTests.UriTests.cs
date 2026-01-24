using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using Mockolate.Parameters;
using Mockolate.Web;

namespace Mockolate.Tests.Web;

public sealed partial class ItExtensionsTests
{
	public sealed class IsUriTests
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

			await That(result.StatusCode).IsEqualTo(expectMatch ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
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

			await That(result.StatusCode).IsEqualTo(expectMatch ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
		}

		[Fact]
		public async Task ShouldSupportMonitoring()
		{
			int callbackCount = 0;
			HttpClient httpClient = Mock.Create<HttpClient>();
			httpClient.SetupMock.Method.GetAsync(It.IsUri()
				.Do(_ => callbackCount++)
				.Monitor(out IParameterMonitor<Uri?> monitor));

			await httpClient.GetAsync("https://www.aweXpect.com", CancellationToken.None);
			await httpClient.GetAsync("https://www.aweXpect.com/foo", CancellationToken.None);
			await httpClient.GetAsync("https://www.aweXpect.com/bar", CancellationToken.None);

			await That(monitor.Values.Select(u => u?.ToString()))
				.IsEqualTo([
					"https://www.aweXpect.com/",
					"https://www.aweXpect.com/foo",
					"https://www.aweXpect.com/bar",
				]).IgnoringCase();
			await That(callbackCount).IsEqualTo(3);
		}

		[Theory]
		[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=4", "https://www.aweXpect.com/foo/bar?x=123&y=4", true)]
		[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=4", "http://www.aweXpect.com/foo/bar?x=123&y=4", false)]
		[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=4", "https://www.aweXpect.com/foo/baz?x=123&y=4", false)]
		[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=4", "https://www.aweXpect.com/foo/bar?x=124&y=4", false)]
		[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=4", "https://www.aweXpect.com/foo/bar?x=123", false)]
		[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=4", "*www.aweXpect.com*", true)]
		[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=4", "*/foo/bar*", true)]
		[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=4", "*x=123*", true)]
		[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=4", "*y=4*", true)]
		[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=4", "https*", true)]
		public async Task ShouldVerifyFullUriWithWildcardMatch(string uri, string pattern, bool expectMatch)
		{
			HttpClient httpClient = Mock.Create<HttpClient>();
			httpClient.SetupMock.Method
				.GetAsync(It.IsUri(pattern))
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

			HttpResponseMessage result = await httpClient.GetAsync(uri, CancellationToken.None);

			await That(result.StatusCode).IsEqualTo(expectMatch ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
		}

		[Theory]
		[InlineData("*aweXpect.com")]
		[InlineData("*aweXpect.com/")]
		public async Task TrailingSlash_ShouldBeIgnored(string matchPattern)
		{
			HttpClient httpClient = Mock.Create<HttpClient>();
			httpClient.SetupMock.Method
				.GetAsync(It.IsUri(matchPattern))
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

			HttpResponseMessage result =
				await httpClient.GetAsync("https://www.aweXpect.com", CancellationToken.None);

			await That(result.StatusCode)
				.IsEqualTo(HttpStatusCode.OK);
		}

		[Fact]
		public async Task TrailingSlash_WhenNotPresent_ShouldNotBeAdded()
		{
			HttpClient httpClient = Mock.Create<HttpClient>();
			httpClient.SetupMock.Method
				.GetAsync(It.IsUri("*www.aweXpect.com/foo/"))
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

			HttpResponseMessage result =
				await httpClient.GetAsync("https://www.aweXpect.com/foo", CancellationToken.None);

			await That(result.StatusCode)
				.IsEqualTo(HttpStatusCode.NotImplemented);
		}

		[Theory]
		[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=234", "www.awexpect.com", true)]
		[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=234", "*awexpect*", true)]
		[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=234", "*aweXpect*", true)]
		[InlineData("http://www.aweXpect.com/foo/bar?x=123&y=234", "mockolate.com", false)]
		public async Task WithHost_ShouldVerifyHost(string uri, string hostPattern, bool expectMatch)
		{
			HttpClient httpClient = Mock.Create<HttpClient>();
			httpClient.SetupMock.Method
				.GetAsync(It.IsUri().WithHost(hostPattern))
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

			HttpResponseMessage result = await httpClient.GetAsync(uri, CancellationToken.None);

			await That(result.StatusCode).IsEqualTo(expectMatch ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
		}

		[Theory]
		[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=234", "/foo/bar", true)]
		[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=234", "*foo*", true)]
		[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=234", "*FOO*", true)]
		[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=234", "*bar*", true)]
		[InlineData("http://www.aweXpect.com/foo/bar?x=123&y=234", "*baz*", false)]
		public async Task WithPath_ShouldVerifyPath(string uri, string pathPattern, bool expectMatch)
		{
			HttpClient httpClient = Mock.Create<HttpClient>();
			httpClient.SetupMock.Method
				.GetAsync(It.IsUri().WithPath(pathPattern))
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

			HttpResponseMessage result = await httpClient.GetAsync(uri, CancellationToken.None);

			await That(result.StatusCode).IsEqualTo(expectMatch ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
		}

		[Theory]
		[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=234", 443, true)]
		[InlineData("http://www.aweXpect.com/foo/bar?x=123&y=234", 80, true)]
		[InlineData("https://www.aweXpect.com:8080/foo/bar?x=123&y=234", 8080, true)]
		[InlineData("https://www.aweXpect.com:442/foo/bar?x=123&y=234", 443, false)]
		[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=234", 442, false)]
		public async Task WithPort_ShouldVerifyPort(string uri, int port, bool expectMatch)
		{
			HttpClient httpClient = Mock.Create<HttpClient>();
			httpClient.SetupMock.Method
				.GetAsync(It.IsUri().WithPort(port))
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

			HttpResponseMessage result = await httpClient.GetAsync(uri, CancellationToken.None);

			await That(result.StatusCode).IsEqualTo(expectMatch ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
		}

		[Theory]
		[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=234", "?x=123&y=234", true)]
		[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=234", "*123*", true)]
		[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=234", "*x=*y=*", true)]
		[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=234", "*X=*Y=*", true)]
		[InlineData("http://www.aweXpect.com/foo/bar?x=123&y=234", "*z=*", false)]
		public async Task WithQuery_ShouldVerifyQuery(string uri, string queryPattern, bool expectMatch)
		{
			HttpClient httpClient = Mock.Create<HttpClient>();
			httpClient.SetupMock.Method
				.GetAsync(It.IsUri().WithQuery(queryPattern))
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

			HttpResponseMessage result = await httpClient.GetAsync(uri, CancellationToken.None);

			await That(result.StatusCode).IsEqualTo(expectMatch ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
		}
	}
}
