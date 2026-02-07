using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using Mockolate.Web;

namespace Mockolate.Tests.Web;

public sealed partial class ItExtensionsTests
{
	public sealed partial class IsHttpRequestMessageTests
	{
		public sealed class WithHeadersTests
		{
			[Theory]
			[InlineData("x-myHeader", "foo", "Authorization", "Basic abcdef", true)]
			[InlineData("Authorization", "Basic abcdef", "x-myHeader", "foo", true)]
			[InlineData("Authorization", "Basic xyz", "x-myHeader", "foo", false)]
			[InlineData("x-myHeader", "foo", "Authorization", "Basic xyz", false)]
			public async Task MultipleCalls_ShouldVerifyKeyValueHeaders(
				string key1, string value1, string key2, string value2, bool expectSuccess)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "abcdef");
				httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				httpClient.SetupMock.Method
					.SendAsync(It.IsHttpRequestMessage()
						.WithHeaders(key1, value1).WithHeaders(key2, value2))
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
				StringContent content = new("");
				content.Headers.Add("x-myHeader", "foo");

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					content,
					CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}

			[Theory]
			[InlineData("x-myHeader", "foo", "Authorization", "Basic abcdef", true)]
			[InlineData("Authorization", "Basic abcdef", "x-myHeader", "foo", true)]
			[InlineData("Authorization", "Basic xyz", "x-myHeader", "foo", false)]
			[InlineData("x-myHeader", "foo", "Authorization", "Basic xyz", false)]
			public async Task MultipleCalls_ShouldVerifyKeyValuePairHeaders(
				string key1, string value1, string key2, string value2, bool expectSuccess)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "abcdef");
				httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				httpClient.SetupMock.Method
					.SendAsync(It.IsHttpRequestMessage()
						.WithHeaders((key1, value1)).WithHeaders((key2, value2)))
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
				StringContent content = new("");
				content.Headers.Add("x-myHeader", "foo");

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					content,
					CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}

			[Theory]
			[InlineData("x-myHeader: foo", "Authorization: Basic abcdef", true)]
			[InlineData("Authorization: Basic abcdef", "x-myHeader: foo", true)]
			[InlineData("Authorization: Basic xyz", "x-myHeader: foo", false)]
			[InlineData("x-myHeader: foo", "Authorization: Basic xyz", false)]
			public async Task MultipleCalls_ShouldVerifyStringHeaders(
				string headers1, string headers2, bool expectSuccess)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "abcdef");
				httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				httpClient.SetupMock.Method
					.SendAsync(It.IsHttpRequestMessage()
						.WithHeaders(headers1).WithHeaders(headers2))
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
				StringContent content = new("");
				content.Headers.Add("x-myHeader", "foo");

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					content,
					CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}

			[Fact]
			public async Task ShouldAlsoMatchContentHeaders()
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "abcdef");
				httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				httpClient.SetupMock.Method
					.SendAsync(It.IsHttpRequestMessage()
						.WithHeaders("""
						             x-myHeader1 : foo
						              Authorization : Basic abcdef
						              x-myHeader3: baz
						             """))
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
				StringContent content = new("");
				content.Headers.Add("x-myHeader1", "foo");
				content.Headers.Add("x-myHeader2", "bar");
				content.Headers.Add("x-myHeader3", "baz");

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					content,
					CancellationToken.None);

				await That(result.StatusCode).IsEqualTo(HttpStatusCode.OK);
			}

			[Fact]
			public async Task ShouldMatchAgainstDefaultRequestHeaders()
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "abcdef");
				httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				httpClient.SetupMock.Method
					.SendAsync(It.IsHttpRequestMessage()
						.WithHeaders("""
						             Authorization: Basic abcdef
						             Accept: application/json
						             """))
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					new StringContent(""),
					CancellationToken.None);

				await That(result.StatusCode).IsEqualTo(HttpStatusCode.OK);
			}

			[Theory]
			[InlineData("Authorization", true)]
			[InlineData("AUTHORIZATION", true)]
			[InlineData("Authentication", false)]
			public async Task ShouldVerifyHeaderKeyCaseInsensitive(string key, bool expectSuccess)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "abcdef");
				httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				httpClient.SetupMock.Method
					.SendAsync(It.IsHttpRequestMessage().WithHeaders(key, "Basic abcdef"))
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					new StringContent(""),
					CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}

			[Theory]
			[InlineData("foo", true)]
			[InlineData("bar", false)]
			[InlineData("FOO", false)]
			public async Task ShouldVerifyHeaderValueCaseSensitive(string value, bool expectSuccess)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "foo");
				httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				httpClient.SetupMock.Method
					.SendAsync(It.IsHttpRequestMessage().WithHeaders("Authorization", $"Basic {value}"))
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					new StringContent(""),
					CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}

			[Theory]
			[InlineData("Authorization", true)]
			[InlineData("AUTHORIZATION", true)]
			[InlineData("Authentication", false)]
			public async Task ShouldVerifyMultipleHeaderKeyCaseInsensitive(string key, bool expectSuccess)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "foo");
				httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				httpClient.SetupMock.Method
					.SendAsync(It.IsHttpRequestMessage()
						.WithHeaders(
							(key, "Basic foo"),
							("Accept", "application/json")))
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					new StringContent(""),
					CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}

			[Theory]
			[InlineData("foo", true)]
			[InlineData("bar", false)]
			[InlineData("FOO", false)]
			public async Task ShouldVerifyMultipleHeaderValueCaseSensitive(string value, bool expectSuccess)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", value);
				httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				httpClient.SetupMock.Method
					.SendAsync(It.IsHttpRequestMessage()
						.WithHeaders(
							("Authorization", "Basic foo"),
							("Accept", "application/json")))
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					new StringContent(""),
					CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}

			[Fact]
			public async Task WithInvalidStringHeader_ShouldThrowArgumentException()
			{
				HttpClient httpClient = Mock.Create<HttpClient>();

				void Act()
				{
					httpClient.SetupMock.Method
						.SendAsync(It.IsHttpRequestMessage()
							.WithHeaders("""
							             x-myHeader1: foo
							             x-myHeader2
							             x-myHeader3: baz
							             """))
						.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
				}

				await That(Act).Throws<ArgumentException>()
					.WithParamName("headers").And
					.WithMessage("The header contained an invalid line: x-myHeader2").AsPrefix();
			}
		}
	}
}
