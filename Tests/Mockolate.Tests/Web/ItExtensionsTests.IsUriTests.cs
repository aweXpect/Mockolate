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
		public async Task NonGeneric_DispatchesNullThroughCallback()
		{
			ItExtensions.IUriParameter sut = It.IsUri();
			int invocations = 0;
			Uri? captured = new("http://placeholder");
			sut.Do(uri =>
			{
				invocations++;
				captured = uri;
			});

			sut.InvokeCallbacks(null);

			await That(invocations).IsEqualTo(1);
			await That(captured).IsNull();
		}

		[Fact]
		public async Task NonGeneric_DispatchesUriThroughCallback()
		{
			ItExtensions.IUriParameter sut = It.IsUri();
			Uri? captured = null;
			sut.Do(uri => captured = uri);

			Uri target = new("http://x/y");
			sut.InvokeCallbacks(target);

			await That(captured).IsSameAs(target);
		}

		[Fact]
		public async Task NonGeneric_IgnoresUnrelatedTypes()
		{
			ItExtensions.IUriParameter sut = It.IsUri();
			int invocations = 0;
			sut.Do(_ => invocations++);

			sut.InvokeCallbacks("not a uri");

			await That(invocations).IsEqualTo(0);
		}

		[Fact]
		public async Task NonGenericMatches_ReturnsFalseForNonUriValue()
		{
			ItExtensions.IUriParameter sut = It.IsUri();

			bool result = sut.Matches("not a uri");

			await That(result).IsFalse();
		}

		[Fact]
		public async Task NonGenericMatches_ReturnsFalseForNullValue()
		{
			ItExtensions.IUriParameter sut = It.IsUri();

			bool result = sut.Matches(null);

			await That(result).IsFalse();
		}

		[Fact]
		public async Task ShouldSupportMonitoring()
		{
			int callbackCount = 0;
			HttpClient httpClient = HttpClient.CreateMock();
			httpClient.Mock.Setup.GetAsync(It.IsUri()
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
		[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=4", "aweXpect.com*x=123", true)]
		[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=4", "https://www.aweXpect.com/foo/bar?x=123&y=4", true)]
		[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=4", "http://www.aweXpect.com/foo/bar?x=123&y=4", false)]
		[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=4", "https://www.aweXpect.com/foo/baz?x=123&y=4", false)]
		[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=4", "https://www.aweXpect.com/foo/bar?x=124&y=4", false)]
		[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=4", "https://www.aweXpect.com/foo/bar?x=123", true)]
		[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=4", "*www.aweXpect.com*", true)]
		[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=4", "*/foo/bar*", true)]
		[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=4", "*x=123*", true)]
		[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=4", "*y=4*", true)]
		[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=4", "https*", true)]
		public async Task ShouldVerifyFullUriWithWildcardMatch(string uri, string pattern, bool expectMatch)
		{
			HttpClient httpClient = HttpClient.CreateMock();
			httpClient.Mock.Setup
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
			HttpClient httpClient = HttpClient.CreateMock();
			httpClient.Mock.Setup
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
			HttpClient httpClient = HttpClient.CreateMock();
			httpClient.Mock.Setup
				.GetAsync(It.IsUri("*www.aweXpect.com/foo/"))
				.ReturnsAsync(HttpStatusCode.OK);

			HttpResponseMessage result =
				await httpClient.GetAsync("https://www.aweXpect.com/foo", CancellationToken.None);

			await That(result.StatusCode)
				.IsEqualTo(HttpStatusCode.NotImplemented);
		}

		[Fact]
		public async Task WhenTypeDoesNotMatch_Null_ShouldReturnFalse()
		{
			ItExtensions.IUriParameter sut = It.IsUri();
			IParameterMatch<Uri?> parameter = (IParameterMatch<Uri?>)sut;

			bool result = parameter.Matches(null);

			await That(result).IsFalse();
		}

		[Fact]
		public async Task WithPattern_AcceptsBothTrailingSlashAndWithoutTrailingSlash()
		{
			ItExtensions.IUriParameter sut = It.IsUri("http://x/foo");
			IParameterMatch<Uri?> matcher = (IParameterMatch<Uri?>)sut;

			bool matchesWithSlash = matcher.Matches(new Uri("http://x/foo/"));
			bool matchesWithoutSlash = matcher.Matches(new Uri("http://x/foo"));

			await That(matchesWithSlash).IsTrue();
			await That(matchesWithoutSlash).IsTrue();
		}

		[Fact]
		public async Task WithTrailingSlashOnRequestAndNoSuffixWildcardInPattern_ShouldStillMatch()
		{
			HttpClient httpClient = HttpClient.CreateMock();

			await httpClient.GetAsync("https://aweXpect.com/", CancellationToken.None);

			await That(httpClient.Mock.Verify.GetAsync(It.IsUri("https://aweXpect.com")))
				.Once();
		}
	}
}
