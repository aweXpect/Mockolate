using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using Mockolate.Parameters;
using Mockolate.Web;

namespace Mockolate.Tests.Web;

public sealed partial class ItExtensionsTests
{
	public sealed partial class IsUriTests
	{
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
				.ReturnsAsync(HttpStatusCode.OK);

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
				.ReturnsAsync(HttpStatusCode.OK);

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
				.ReturnsAsync(HttpStatusCode.OK);

			HttpResponseMessage result =
				await httpClient.GetAsync("https://www.aweXpect.com/foo", CancellationToken.None);

			await That(result.StatusCode)
				.IsEqualTo(HttpStatusCode.NotImplemented);
		}

		[Theory]
		[InlineData("https://www.aweXpect.com")]
		[InlineData(443)]
		[InlineData(null)]
		public async Task WhenTypeDoesNotMatch_ShouldReturnFalse(object? value)
		{
			ItExtensions.IUriParameter sut = It.IsUri();
			IParameter parameter = (IParameter)sut;

			bool result = parameter.Matches(value);

			await That(result).IsFalse();
		}
	}
}
