using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using aweXpect.Chronology;
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
			public async Task AsRegex_IgnoringCase_ShouldCheckForCaseInsensitiveMatchingWildcard(
				string body, string pattern, bool expectSuccess)
			{
				HttpClient httpClient = HttpClient.CreateMock();
				httpClient.Mock.Setup
					.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithStringMatching(pattern).AsRegex().IgnoringCase())
					.ReturnsAsync(HttpStatusCode.OK);

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
			public async Task AsRegex_ShouldCheckForMatchingWildcard(string body, string pattern, bool expectSuccess)
			{
				HttpClient httpClient = HttpClient.CreateMock();
				httpClient.Mock.Setup
					.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithStringMatching(pattern).AsRegex())
					.ReturnsAsync(HttpStatusCode.OK);

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					new StringContent(body),
					CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}

			[Fact]
			public async Task AsRegex_ShouldUseProvidedOptions()
			{
				HttpClient httpClient = HttpClient.CreateMock();
				httpClient.Mock.Setup
					.PostAsync(It.IsAny<Uri>(),
						It.IsHttpContent().WithStringMatching("F[A-Z]*").AsRegex(RegexOptions.IgnoreCase))
					.ReturnsAsync(HttpStatusCode.OK);

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					new StringContent("foo"),
					CancellationToken.None);

				await That(result.StatusCode).IsEqualTo(HttpStatusCode.OK);
			}

			[Fact]
			public async Task AsRegex_ShouldUseTimeout()
			{
				HttpClient httpClient = HttpClient.CreateMock();
				httpClient.Mock.Setup
					.PostAsync(It.IsAny<Uri>(),
						It.IsHttpContent().WithStringMatching("F[A-Z]*").AsRegex(timeout: 0.Seconds()))
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

			[Theory]
			[InlineData("foo", "f?", true)]
			[InlineData("foo", "f??", true)]
			[InlineData("foo", "f*", true)]
			[InlineData("foo", "*", true)]
			[InlineData("foo", "F*", true)]
			[InlineData("foo", "*a*", false)]
			public async Task IgnoringCase_ShouldCheckForCaseInsensitiveMatchingWildcard(
				string body, string pattern, bool expectSuccess)
			{
				HttpClient httpClient = HttpClient.CreateMock();
				httpClient.Mock.Setup
					.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithStringMatching(pattern).IgnoringCase())
					.ReturnsAsync(HttpStatusCode.OK);

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					new StringContent(body),
					CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}

			[Theory]
			[InlineData("foo", "f?", true)]
			[InlineData("foo", "f??", true)]
			[InlineData("foo", "f*", true)]
			[InlineData("foo", "*", true)]
			[InlineData("foo", "o?", true)]
			[InlineData("foo", "?o", true)]
			[InlineData("foo", "o", true)]
			[InlineData("foo", "F*", false)]
			[InlineData("foo", "*a*", false)]
			public async Task ShouldCheckForMatchingWildcard(string body, string pattern, bool expectSuccess)
			{
				HttpClient httpClient = HttpClient.CreateMock();
				httpClient.Mock.Setup
					.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithStringMatching(pattern))
					.ReturnsAsync(HttpStatusCode.OK);

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					new StringContent(body),
					CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}

			[Theory]
			[InlineData(true, "f?o", "foo")]
			[InlineData(true, "f?o", "bar")]
			[InlineData(true, "bar", "f?o")]
			[InlineData(true, "bar", "bar")]
			[InlineData(true, "f?o", "?oo", "b?r", "?ar")]
			[InlineData(false, "f?o", "b?r", "baz")]
			[InlineData(false, "f?o", "baz", "b?r")]
			[InlineData(false, "?az", "f?o", "b?r")]
			[InlineData(false, "?az")]
			public async Task WithMultipleExpectations_ShouldVerifyAll(bool expectSuccess,
				params string[] expectedValues)
			{
				ItExtensions.IHttpContentParameter isHttpContent = It.IsHttpContent();
				foreach (string expectedValue in expectedValues)
				{
					isHttpContent = isHttpContent.WithStringMatching(expectedValue);
				}

				string body = "foo;bar";
				HttpClient httpClient = HttpClient.CreateMock();
				httpClient.Mock.Setup
					.PostAsync(It.IsAny<Uri>(), isHttpContent)
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
