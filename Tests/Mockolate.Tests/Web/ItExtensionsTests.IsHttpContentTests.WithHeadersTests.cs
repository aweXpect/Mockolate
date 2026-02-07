using System.Net;
using System.Net.Http;
using System.Threading;
using Mockolate.Web;

namespace Mockolate.Tests.Web;

public sealed partial class ItExtensionsTests
{
	public sealed partial class IsHttpContentTests
	{
		public sealed class WithHeadersTests
		{
			[Theory]
			[InlineData("x-myHeader1", "foo", "x-myHeader3", "baz", true)]
			[InlineData("x-myHeader3", "baz", "x-myHeader1", "foo", true)]
			[InlineData("x-myHeader2", "baz", "x-myHeader1", "foo", false)]
			[InlineData("x-myHeader1", "foo", "x-myHeader2", "baz", false)]
			public async Task MultipleCalls_ShouldVerifyKeyValueHeaders(
				string key1, string value1, string key2, string value2, bool expectSuccess)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.PostAsync(It.IsAny<Uri>(), It.IsHttpContent()
						.WithHeaders(key1, value1).WithHeaders(key2, value2))
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
				StringContent content = new("");
				content.Headers.Add("x-myHeader1", "foo");
				content.Headers.Add("x-myHeader2", "bar");
				content.Headers.Add("x-myHeader3", "baz");

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					content,
					CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}

			[Theory]
			[InlineData("x-myHeader1", "foo", "x-myHeader3", "baz", true)]
			[InlineData("x-myHeader3", "baz", "x-myHeader1", "foo", true)]
			[InlineData("x-myHeader2", "baz", "x-myHeader1", "foo", false)]
			[InlineData("x-myHeader1", "foo", "x-myHeader2", "baz", false)]
			public async Task MultipleCalls_ShouldVerifyKeyValuePairHeaders(
				string key1, string value1, string key2, string value2, bool expectSuccess)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.PostAsync(It.IsAny<Uri>(), It.IsHttpContent()
						.WithHeaders((key1, value1)).WithHeaders((key2, value2)))
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
				StringContent content = new("");
				content.Headers.Add("x-myHeader1", "foo");
				content.Headers.Add("x-myHeader2", "bar");
				content.Headers.Add("x-myHeader3", "baz");

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					content,
					CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}

			[Theory]
			[InlineData("x-myHeader1: foo", "x-myHeader3: baz", true)]
			[InlineData("x-myHeader3: baz", "x-myHeader1: foo", true)]
			[InlineData("x-myHeader2: baz", "x-myHeader1: foo", false)]
			[InlineData("x-myHeader1: foo", "x-myHeader2: baz", false)]
			public async Task MultipleCalls_ShouldVerifyStringHeaders(
				string headers1, string headers2, bool expectSuccess)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.PostAsync(It.IsAny<Uri>(), It.IsHttpContent()
						.WithHeaders(headers1).WithHeaders(headers2))
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
				StringContent content = new("");
				content.Headers.Add("x-myHeader1", "foo");
				content.Headers.Add("x-myHeader2", "bar");
				content.Headers.Add("x-myHeader3", "baz");

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					content,
					CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}

			[Fact]
			public async Task ShouldOnlyRequireOneMatchingValue()
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithHeaders("x-myHeader", "foo"))
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
				StringContent content = new("");
				content.Headers.Add("x-myHeader", "foo");
				content.Headers.Add("x-myHeader", "bar");
				content.Headers.Add("x-myHeader", "baz");

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					content,
					CancellationToken.None);

				await That(result.StatusCode).IsEqualTo(HttpStatusCode.OK);
			}

			[Theory]
			[InlineData("foo", true)]
			[InlineData("bar", false)]
			[InlineData("FOO", true)]
			public async Task ShouldVerifyHeaderKeyCaseInsensitive(string key, bool expectSuccess)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithHeaders("foo", "my-value"))
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
				StringContent content = new("");
				content.Headers.Add(key, "my-value");

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					content,
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
				httpClient.SetupMock.Method
					.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithHeaders("x-myHeader", "foo"))
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
				StringContent content = new("");
				content.Headers.Add("x-myHeader", value);

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					content,
					CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}

			[Theory]
			[InlineData("foo", true)]
			[InlineData("bar", false)]
			[InlineData("FOO", true)]
			public async Task ShouldVerifyMultipleHeaderKeyCaseInsensitive(string key, bool expectSuccess)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.PostAsync(It.IsAny<Uri>(), It.IsHttpContent()
						.WithHeaders(
							("foo", "my-foo-value"),
							("bar", "my-bar-value")))
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
				StringContent content = new("");
				content.Headers.Add(key, "my-foo-value");
				content.Headers.Add("bar", "my-bar-value");
				content.Headers.Add("baz", "my-baz-value");

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					content,
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
				httpClient.SetupMock.Method
					.PostAsync(It.IsAny<Uri>(), It.IsHttpContent()
						.WithHeaders(
							("x-myHeader1", "foo"),
							("x-myHeader2", "bar")))
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
				StringContent content = new("");
				content.Headers.Add("x-myHeader1", value);
				content.Headers.Add("x-myHeader2", "bar");
				content.Headers.Add("x-myHeader3", "baz");

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					content,
					CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}

			[Theory]
			[InlineData("""
			            x-myHeader1 : foo
			             x-myHeader3: baz
			            """, true)]
			[InlineData("""
			            x-myHeader3: baz
			            x-myHeader1: foo
			            """, true)]
			[InlineData("""
			            x-myHeader2: foo
			            x-myHeader3: baz
			            """, false)]
			public async Task ShouldVerifyStringHeaders(string headers, bool expectSuccess)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.PostAsync(It.IsAny<Uri>(), It.IsHttpContent()
						.WithHeaders(headers))
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
				StringContent content = new("");
				content.Headers.Add("x-myHeader1", "foo");
				content.Headers.Add("x-myHeader2", "bar");
				content.Headers.Add("x-myHeader3", "baz");

				HttpResponseMessage result = await httpClient.PostAsync("https://www.aweXpect.com",
					content,
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
						.PostAsync(It.IsAny<Uri>(), It.IsHttpContent()
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
