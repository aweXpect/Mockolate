using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using Mockolate.Parameters;

namespace Mockolate.Web.Tests;

public sealed partial class ItExtensionsTests
{
	public sealed class UriTests
	{
		[Fact]
		public async Task IsUri_ShouldSupportMonitoring()
		{
			HttpClient httpClient = Mock.Create<HttpClient>();
			httpClient.SetupMock.Method.GetAsync(It.IsUri().Monitor(out IParameterMonitor<Uri?> monitor));

			await httpClient.GetAsync("https://www.aweXpect.com", CancellationToken.None);
			await httpClient.GetAsync("https://www.aweXpect.com/foo", CancellationToken.None);
			await httpClient.GetAsync("https://www.aweXpect.com/bar", CancellationToken.None);

			await That(monitor.Values.Select(u => u?.ToString()))
				.IsEqualTo([
					"https://www.aweXpect.com/",
					"https://www.aweXpect.com/foo",
					"https://www.aweXpect.com/bar",
				]).IgnoringCase();
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
		public async Task IsUri_ShouldVerifyFullUriWithWildcardMatch(string uri, string pattern, bool expectMatch)
		{
			HttpClient httpClient = Mock.Create<HttpClient>();
			httpClient.SetupMock.Method
				.GetAsync(It.IsUri(pattern))
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

			HttpResponseMessage result = await httpClient.GetAsync(uri, CancellationToken.None);

			await That(result.StatusCode).IsEqualTo(expectMatch ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
		}
	}
}
