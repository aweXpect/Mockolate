using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using Mockolate.Web;

namespace Mockolate.Tests.Web;

public sealed partial class ItExtensionsTests
{
	public sealed partial class IsHttpContentTests
	{
		public sealed class WithStringMatchingTests
		{
			[Test]
			[Arguments("foo", "f[aeiou]*", true)]
			[Arguments("foo", "F[aeiou]*", true)]
			[Arguments("foo", ".a.", false)]
			public async Task AsRegex_IgnoringCase_ShouldCheckForCaseInsensitiveMatchingWildcard(
				string body, string pattern, bool expectSuccess)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithStringMatching(pattern).AsRegex().IgnoringCase())
					.ReturnsAsync(HttpStatusCode.OK);

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					new StringContent(body),
					CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}

			[Test]
			[Arguments("foo", "f[aeiou]*", true)]
			[Arguments("foo", "F[aeiou]*", false)]
			[Arguments("foo", ".a.", false)]
			public async Task AsRegex_ShouldCheckForMatchingWildcard(string body, string pattern, bool expectSuccess)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithStringMatching(pattern).AsRegex())
					.ReturnsAsync(HttpStatusCode.OK);

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					new StringContent(body),
					CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}

			[Test]
			public async Task AsRegex_ShouldUseProvidedOptions()
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.PostAsync(It.IsAny<Uri>(),
						It.IsHttpContent().WithStringMatching("F[A-Z]*").AsRegex(RegexOptions.IgnoreCase))
					.ReturnsAsync(HttpStatusCode.OK);

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					new StringContent("foo"),
					CancellationToken.None);

				await That(result.StatusCode).IsEqualTo(HttpStatusCode.OK);
			}

			[Test]
			public async Task AsRegex_ShouldUseTimeout()
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.PostAsync(It.IsAny<Uri>(),
						It.IsHttpContent().WithStringMatching("F[A-Z]*").AsRegex(timeout: TimeSpan.FromSeconds(0)))
					.ReturnsAsync(HttpStatusCode.OK);

				Task Act()
				{
					return httpClient.PostAsync("https://www.aweXpect.com",
						new StringContent("foo"),
						CancellationToken.None);
				}

				await That(Act)
					.Throws<ArgumentOutOfRangeException>()
					.WithParamName("matchTimeout");
			}

			[Test]
			[Arguments("foo", "f?", false)]
			[Arguments("foo", "f??", true)]
			[Arguments("foo", "f*", true)]
			[Arguments("foo", "*", true)]
			[Arguments("foo", "F*", true)]
			[Arguments("foo", "*a*", false)]
			public async Task IgnoringCase_ShouldCheckForCaseInsensitiveMatchingWildcard(
				string body, string pattern, bool expectSuccess)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithStringMatching(pattern).IgnoringCase())
					.ReturnsAsync(HttpStatusCode.OK);

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					new StringContent(body),
					CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}

			[Test]
			[Arguments("foo", "f?", false)]
			[Arguments("foo", "f??", true)]
			[Arguments("foo", "f*", true)]
			[Arguments("foo", "*", true)]
			[Arguments("foo", "F*", false)]
			[Arguments("foo", "*a*", false)]
			public async Task ShouldCheckForMatchingWildcard(string body, string pattern, bool expectSuccess)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithStringMatching(pattern))
					.ReturnsAsync(HttpStatusCode.OK);

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					new StringContent(body),
					CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}
		}
	}
}
