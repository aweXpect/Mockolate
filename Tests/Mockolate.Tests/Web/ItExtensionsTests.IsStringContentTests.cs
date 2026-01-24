using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Mockolate.Parameters;
using Mockolate.Web;

namespace Mockolate.Tests.Web;

public sealed partial class ItExtensionsTests
{
	public sealed class IsStringContentTests
	{
#if !NETFRAMEWORK
		[Fact]
		public async Task ShouldSupportMonitoring()
		{
			int callbackCount = 0;
			List<StringContent> responses =
			[
				new("", Encoding.UTF8, "application/json"),
				new("foo", Encoding.UTF8, "application/json"),
				new("bar", Encoding.UTF8, "application/json"),
			];
			HttpClient httpClient = Mock.Create<HttpClient>();
			httpClient.SetupMock.Method.PostAsync(It.IsAny<Uri>(),
				It.IsStringContent("application/json")
					.Do(_ => callbackCount++)
					.Monitor(out IParameterMonitor<HttpContent?> monitor));

			foreach (StringContent response in responses)
			{
				await httpClient.PostAsync("https://www.aweXpect.com", response);
			}

			await That(await Task.WhenAll(monitor.Values.Select(c => c!.ReadAsStringAsync())))
				.IsEqualTo(["", "foo", "bar",]);
			await That(callbackCount).IsEqualTo(3);
		}
#endif

		[Theory]
		[InlineData("application/json", true)]
		[InlineData("text/plain", false)]
		[InlineData("application/txt", false)]
		public async Task ShouldVerifyMediaType(string mediaType, bool expectSuccess)
		{
			HttpClient httpClient = Mock.Create<HttpClient>();
			httpClient.SetupMock.Method
				.PostAsync(It.IsAny<Uri>(), It.IsStringContent("application/json"))
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

			HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
				new StringContent("", Encoding.UTF8, mediaType),
				CancellationToken.None);

			await That(result.StatusCode).IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
		}

		[Theory]
		[InlineData("foo", "foo", true)]
		[InlineData("foo", "FOO", true)]
		[InlineData("foo", "bar", false)]
		public async Task WithBody_IgnoringCase_ShouldCheckForCaseInsensitiveEquality(string body,
			string expected, bool expectSuccess)
		{
			HttpClient httpClient = Mock.Create<HttpClient>();
			httpClient.SetupMock.Method
				.PostAsync(It.IsAny<Uri>(), It.IsStringContent().WithBody(expected).IgnoringCase())
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

			HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
				new StringContent(body),
				CancellationToken.None);

			await That(result.StatusCode).IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
		}

		[Theory]
		[InlineData("foo", "foo", true)]
		[InlineData("foo", "FOO", false)]
		[InlineData("foo", "bar", false)]
		public async Task WithBody_ShouldCheckForEquality(string body, string expected,
			bool expectSuccess)
		{
			HttpClient httpClient = Mock.Create<HttpClient>();
			httpClient.SetupMock.Method
				.PostAsync(It.IsAny<Uri>(), It.IsStringContent().WithBody(expected))
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

			HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
				new StringContent(body),
				CancellationToken.None);

			await That(result.StatusCode).IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
		}

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
				.PostAsync(It.IsAny<Uri>(), It.IsStringContent().WithBodyMatching(pattern).AsRegex().IgnoringCase())
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

			HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
				new StringContent(body),
				CancellationToken.None);

			await That(result.StatusCode).IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
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
				.PostAsync(It.IsAny<Uri>(), It.IsStringContent().WithBodyMatching(pattern).AsRegex())
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

			HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
				new StringContent(body),
				CancellationToken.None);

			await That(result.StatusCode).IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
		}

		[Fact]
		public async Task WithBodyMatching_AsRegex_ShouldUseProvidedOptions()
		{
			HttpClient httpClient = Mock.Create<HttpClient>();
			httpClient.SetupMock.Method
				.PostAsync(It.IsAny<Uri>(),
					It.IsStringContent().WithBodyMatching("F[A-Z]*").AsRegex(RegexOptions.IgnoreCase))
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
					It.IsStringContent().WithBodyMatching("F[A-Z]*").AsRegex(timeout: TimeSpan.FromSeconds(0)))
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
				.PostAsync(It.IsAny<Uri>(), It.IsStringContent().WithBodyMatching(pattern).IgnoringCase())
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

			HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
				new StringContent(body),
				CancellationToken.None);

			await That(result.StatusCode).IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
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
				.PostAsync(It.IsAny<Uri>(), It.IsStringContent().WithBodyMatching(pattern))
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

			HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
				new StringContent(body),
				CancellationToken.None);

			await That(result.StatusCode).IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
		}

		[Theory]
		[InlineData("application/json", true)]
		[InlineData("text/plain", false)]
		[InlineData("application/txt", false)]
		public async Task WithMediaType_ShouldVerifyMediaType(string mediaType, bool expectSuccess)
		{
			HttpClient httpClient = Mock.Create<HttpClient>();
			httpClient.SetupMock.Method
				.PostAsync(It.IsAny<Uri>(), It.IsStringContent().WithMediaType("application/json"))
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

			HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
				new StringContent("", Encoding.UTF8, mediaType),
				CancellationToken.None);

			await That(result.StatusCode).IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
		}
	}
}
