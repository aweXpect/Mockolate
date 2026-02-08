using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using Mockolate.Web;

namespace Mockolate.Tests.Web;

public sealed partial class ItExtensionsTests
{
	public sealed partial class IsHttpContentTests
	{
		public sealed class WithStringTests
		{
			[Theory]
			[InlineData("foo", "foo", true)]
			[InlineData("foo", "FOO", true)]
			[InlineData("foo", "bar", false)]
			public async Task IgnoringCase_ShouldCheckForCaseInsensitiveEquality(string body,
				string expected, bool expectSuccess)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithString(expected).IgnoringCase())
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

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
			public async Task Predicate_ShouldValidatePredicate(string content, bool expectSuccess)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.PostAsync(It.IsAny<Uri>(), It.IsHttpContent()
						.WithString(c => c.Equals(c.ToLowerInvariant(), StringComparison.Ordinal)))
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

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
			[InlineData("foo", "f*o", false)]
			public async Task ShouldCheckForEquality(string body, string expected,
				bool expectSuccess)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithString(expected))
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					new StringContent(body),
					CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}

			[Fact]
			public async Task ShouldNotCheckHttpContentType()
			{
				string expectedValue = "foo";
				byte[] bytes = Encoding.UTF8.GetBytes(expectedValue);
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithString("foo"))
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					new ByteArrayContent(bytes),
					CancellationToken.None);

				await That(result.StatusCode).IsEqualTo(HttpStatusCode.OK);
			}

			[Fact]
			public async Task WhenCharsetHeaderIsNotSet_ShouldFallbackToUtf8()
			{
				string original = "äöüß";
				Encoding encoding = Encoding.UTF8;
				byte[] bytes = encoding.GetBytes(original);

				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithString(original))
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
				ByteArrayContent content = new(bytes);

				HttpResponseMessage result = await httpClient.PostAsync(
					"https://www.aweXpect.com", content, CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(HttpStatusCode.OK);
			}

			[Theory]
			[InlineData("", false)]
			[InlineData("iso-8859-1", true)]
			public async Task WhenCharsetHeaderIsSet_ShouldApplyEncodingCorrectly(string contentTypeHeader,
				bool expectSuccess)
			{
				string original = "äöüß";
				Encoding encoding = Encoding.GetEncoding("iso-8859-1");
				byte[] bytes = encoding.GetBytes(original);

				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithString(original))
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
				ByteArrayContent content = new(bytes);
				if (!string.IsNullOrEmpty(contentTypeHeader))
				{
					content.Headers.ContentType = new MediaTypeHeaderValue("text/plain")
					{
						CharSet = contentTypeHeader,
					};
				}

				HttpResponseMessage result = await httpClient.PostAsync(
					"https://www.aweXpect.com", content, CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}
		}
	}
}
