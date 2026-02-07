using System.Net;
using System.Net.Http;
using System.Text;
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
			[Theory]
			[InlineData("foo", "f[aeiou]*", true)]
			[InlineData("foo", "F[aeiou]*", true)]
			[InlineData("foo", ".a.", false)]
			public async Task
				WithBodyMatching_AsRegex_IgnoringCase_ShouldCheckForCaseInsensitiveMatchingWildcard(
					string body, string pattern, bool expectSuccess)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithStringMatching(pattern).AsRegex().IgnoringCase())
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					new StringContent(body),
					CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}

			[Theory]
			[InlineData("foo", "f[aeiou]*", true)]
			[InlineData("foo", "F[aeiou]*", false)]
			[InlineData("foo", ".a.", false)]
			public async Task WithBodyMatching_AsRegex_ShouldCheckForMatchingWildcard(
				string body, string pattern, bool expectSuccess)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithStringMatching(pattern).AsRegex())
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					new StringContent(body),
					CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}

			[Fact]
			public async Task WithBodyMatching_AsRegex_ShouldUseProvidedOptions()
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.PostAsync(It.IsAny<Uri>(),
						It.IsHttpContent().WithStringMatching("F[A-Z]*").AsRegex(RegexOptions.IgnoreCase))
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					new StringContent("foo"),
					CancellationToken.None);

				await That(result.StatusCode).IsEqualTo(HttpStatusCode.OK);
			}

			[Fact]
			public async Task WithBodyMatching_AsRegex_ShouldUseTimeout()
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.PostAsync(It.IsAny<Uri>(),
						It.IsHttpContent().WithStringMatching("F[A-Z]*").AsRegex(timeout: TimeSpan.FromSeconds(0)))
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

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

			[Theory]
			[InlineData("foo", "f?", false)]
			[InlineData("foo", "f??", true)]
			[InlineData("foo", "f*", true)]
			[InlineData("foo", "*", true)]
			[InlineData("foo", "F*", true)]
			[InlineData("foo", "*a*", false)]
			public async Task WithBodyMatching_IgnoringCase_ShouldCheckForCaseInsensitiveMatchingWildcard(
				string body, string pattern, bool expectSuccess)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithStringMatching(pattern).IgnoringCase())
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					new StringContent(body),
					CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}

			[Theory]
			[InlineData("foo", "f?", false)]
			[InlineData("foo", "f??", true)]
			[InlineData("foo", "f*", true)]
			[InlineData("foo", "*", true)]
			[InlineData("foo", "F*", false)]
			[InlineData("foo", "*a*", false)]
			public async Task WithBodyMatching_ShouldCheckForMatchingWildcard(
				string body, string pattern, bool expectSuccess)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithStringMatching(pattern))
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					new StringContent(body),
					CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}
		}
	}
}
