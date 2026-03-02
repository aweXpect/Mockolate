using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Mockolate.Web;

namespace Mockolate.Tests.Web;

public sealed partial class ItExtensionsTests
{
	public sealed partial class IsHttpRequestMessageTests
	{
		public sealed class WhoseContentIsTests
		{
			[Fact]
			public async Task ShouldNotCheckHttpContentType()
			{
				string expectedValue = "foo";
				byte[] bytes = Encoding.UTF8.GetBytes(expectedValue);
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.SendAsync(It.IsHttpRequestMessage().WhoseContentIs(c => c.WithString("foo")))
					.ReturnsAsync(HttpStatusCode.OK);

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					new ByteArrayContent(bytes),
					CancellationToken.None);

				await That(result.StatusCode).IsEqualTo(HttpStatusCode.OK);
			}

			[Theory]
			[InlineData("image/png", true)]
			[InlineData("text/plain", false)]
			[InlineData("image/gif", false)]
			public async Task ShouldVerifyMediaType(string mediaType, bool expectSuccess)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.SendAsync(It.IsHttpRequestMessage().WhoseContentIs("image/png"))
					.ReturnsAsync(HttpStatusCode.OK);
				ByteArrayContent content = new([]);
				content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					content,
					CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}

			[Theory]
			[InlineData("text/plain", "foo", true)]
			[InlineData("image/png", "foo", false)]
			[InlineData("text/plain", "bar", false)]
			public async Task ShouldVerifyMediaTypeAndContent(string mediaType, string value, bool expectSuccess)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.SendAsync(It.IsHttpRequestMessage().WhoseContentIs(mediaType, c => c.WithString(value)))
					.ReturnsAsync(HttpStatusCode.OK);
				StringContent content = new("foo", Encoding.UTF8, "text/plain");

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					content,
					CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}

			[Theory]
			[InlineData(new byte[0], 0x1, false)]
			[InlineData(new byte[] { 0x1, }, 0x1, true)]
			[InlineData(new byte[] { 0x1, }, 0x2, false)]
			[InlineData(new byte[] { 0x1, 0x2, 0x3, }, 0x1, true)]
			[InlineData(new byte[] { 0x1, 0x2, 0x3, }, 0x2, false)]
			[InlineData(new byte[] { 0x1, 0x2, 0x3, }, 0x3, false)]
			public async Task WithBytes_Predicate_ShouldValidatePredicate(
				byte[] body, byte expectedFirstByte, bool expectSuccess)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.SendAsync(It.IsHttpRequestMessage().WhoseContentIs(c => c.WithBytes(b => b.Length > 0 && b[0] == expectedFirstByte)))
					.ReturnsAsync(HttpStatusCode.OK);

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					new ByteArrayContent(body),
					CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}

			[Theory]
			[InlineData(new byte[0], new byte[0], true)]
			[InlineData(new byte[] { 0x66, }, new byte[] { 0x66, }, true)]
			[InlineData(new byte[] { 0x66, }, new byte[] { 0x67, }, false)]
			[InlineData(new byte[] { 0x66, 0x67, }, new byte[] { 0x67, }, false)]
			[InlineData(new byte[] { 0x66, 0x67, }, new byte[] { 0x67, 0x68, 0x69, }, false)]
			public async Task WithBytes_ShouldCheckForEquality(byte[] body, byte[] expected, bool expectSuccess)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.SendAsync(It.IsHttpRequestMessage().WhoseContentIs(c => c.WithBytes(expected)))
					.ReturnsAsync(HttpStatusCode.OK);

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					new ByteArrayContent(body),
					CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}

			[Theory]
			[InlineData("image/png", true)]
			[InlineData("text/plain", false)]
			[InlineData("image/gif", false)]
			public async Task WithMediaType_ShouldVerifyMediaType(string mediaType, bool expectSuccess)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.SendAsync(It.IsHttpRequestMessage().WhoseContentIs(c => c.WithMediaType("image/png")))
					.ReturnsAsync(HttpStatusCode.OK);
				ByteArrayContent content = new([]);
				content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					content,
					CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}

			[Theory]
			[InlineData("foo", "foo", true)]
			[InlineData("foo", "FOO", true)]
			[InlineData("foo", "bar", false)]
			public async Task WithString_IgnoringCase_ShouldCheckForCaseInsensitiveEquality(string body,
				string expected, bool expectSuccess)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.SendAsync(It.IsHttpRequestMessage().WhoseContentIs(c => c.WithString(expected).IgnoringCase()))
					.ReturnsAsync(HttpStatusCode.OK);

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					new StringContent(body),
					CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}

			[Theory]
			[InlineData("", true)]
			[InlineData("foo", true)]
			[InlineData("FOO", false)]
			[InlineData("bar", true)]
			[InlineData("BAR", false)]
			public async Task WithString_Predicate_ShouldValidatePredicate(string content, bool expectSuccess)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.SendAsync(It.IsHttpRequestMessage().WhoseContentIs(c => c
							.WithString(v => v.Equals(v.ToLowerInvariant(), StringComparison.Ordinal))))
					.ReturnsAsync(HttpStatusCode.OK);

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					new StringContent(content),
					CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}

			[Theory]
			[InlineData("foo", "foo", true)]
			[InlineData("foo", "FOO", false)]
			[InlineData("foo", "bar", false)]
			public async Task WithString_ShouldCheckForEquality(string body, string expected,
				bool expectSuccess)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.SendAsync(It.IsHttpRequestMessage().WhoseContentIs(c => c.WithString(expected)))
					.ReturnsAsync(HttpStatusCode.OK);

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					new StringContent(body),
					CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}

			[Theory]
			[InlineData("foo", "f[aeiou]*", true)]
			[InlineData("foo", "F[aeiou]*", true)]
			[InlineData("foo", ".a.", false)]
			public async Task
				WithStringMatching_AsRegex_IgnoringCase_ShouldCheckForCaseInsensitiveMatchingWildcard(
					string body, string pattern, bool expectSuccess)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.SendAsync(It.IsHttpRequestMessage()
						.WhoseContentIs(c => c.WithStringMatching(pattern).AsRegex().IgnoringCase()))
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
			public async Task WithStringMatching_AsRegex_ShouldCheckForMatchingWildcard(
				string body, string pattern, bool expectSuccess)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.SendAsync(It.IsHttpRequestMessage()
						.WhoseContentIs(c => c.WithStringMatching(pattern).AsRegex()))
					.ReturnsAsync(HttpStatusCode.OK);

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					new StringContent(body),
					CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}

			[Fact]
			public async Task WithStringMatching_AsRegex_ShouldUseProvidedOptions()
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.SendAsync(It.IsHttpRequestMessage().WhoseContentIs(c => c
						.WithStringMatching("F[A-Z]*").AsRegex(RegexOptions.IgnoreCase)))
					.ReturnsAsync(HttpStatusCode.OK);

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					new StringContent("foo"),
					CancellationToken.None);

				await That(result.StatusCode).IsEqualTo(HttpStatusCode.OK);
			}

			[Fact]
			public async Task WithStringMatching_AsRegex_ShouldUseTimeout()
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.SendAsync(It.IsHttpRequestMessage().WhoseContentIs(c => c
						.WithStringMatching("F[A-Z]*").AsRegex(timeout: TimeSpan.FromSeconds(0))))
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
			[InlineData("foo", "f?", false)]
			[InlineData("foo", "f??", true)]
			[InlineData("foo", "f*", true)]
			[InlineData("foo", "*", true)]
			[InlineData("foo", "F*", true)]
			[InlineData("foo", "*a*", false)]
			public async Task WithStringMatching_IgnoringCase_ShouldCheckForCaseInsensitiveMatchingWildcard(
				string body, string pattern, bool expectSuccess)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.SendAsync(It.IsHttpRequestMessage().WhoseContentIs(c => c
						.WithStringMatching(pattern).IgnoringCase()))
					.ReturnsAsync(HttpStatusCode.OK);

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
			public async Task WithStringMatching_ShouldCheckForMatchingWildcard(
				string body, string pattern, bool expectSuccess)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.SendAsync(It.IsHttpRequestMessage().WhoseContentIs(c => c
						.WithStringMatching(pattern)))
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
