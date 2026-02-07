using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using Mockolate.Web;

namespace Mockolate.Tests.Web;

public sealed partial class ItExtensionsTests
{
	public sealed partial class IsUriTests
	{
		public sealed class WithQueryTests
		{
			[Theory]
			[InlineData("x", "123", "z", "345", true)]
			[InlineData("z", "345", "x", "123", true)]
			[InlineData("x", "123", "y", "345", false)]
			[InlineData("y", "345", "x", "123", false)]
			public async Task MultipleCalls_ShouldVerifyKeyValueQueryParameters(
				string key1, string value1, string key2, string value2, bool expectSuccess)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.GetAsync(It.IsUri().WithQuery(key1, value1).WithQuery(key2, value2))
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

				HttpResponseMessage result = await httpClient
					.GetAsync("https://www.aweXpect.com/foo/bar?x=123&y=234&z=345", CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}

			[Theory]
			[InlineData("x", "123", "z", "345", true)]
			[InlineData("z", "345", "x", "123", true)]
			[InlineData("x", "123", "y", "345", false)]
			[InlineData("y", "345", "x", "123", false)]
			public async Task MultipleCalls_ShouldVerifyKeyValuePairQueryParameters(
				string key1, string value1, string key2, string value2, bool expectSuccess)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.GetAsync(It.IsUri().WithQuery((key1, value1)).WithQuery((key2, value2)))
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

				HttpResponseMessage result = await httpClient
					.GetAsync("https://www.aweXpect.com/foo/bar?x=123&y=234&z=345", CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}

			[Theory]
			[InlineData("x=123", "z=345", true)]
			[InlineData("z=345", "x=123", true)]
			[InlineData("x=123", "y=345", false)]
			[InlineData("y=345", "x=123", false)]
			public async Task MultipleCalls_ShouldVerifyStringQueryParameters(
				string query1, string query2, bool expectSuccess)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.GetAsync(It.IsUri().WithQuery(query1).WithQuery(query2))
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

				HttpResponseMessage result = await httpClient
					.GetAsync("https://www.aweXpect.com/foo/bar?x=123&y=234&z=345", CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}

			[Theory]
			[InlineData("x<>", "1<2> 3")]
			public async Task ShouldEncodeValues(string key, string value)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.GetAsync(It.IsUri().WithQuery(key, value))
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

				HttpResponseMessage result = await httpClient.GetAsync("https://www.aweXpect.com/?x%3c%3e=1%3c2%3e+3",
					CancellationToken.None);

				await That(result.StatusCode).IsEqualTo(HttpStatusCode.OK);
			}


			[Theory]
			[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=234", "x", "123", true)]
			[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=234", "y", "234", true)]
			[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=234", "x", "", false)]
			[InlineData("http://www.aweXpect.com/foo/bar?x=123&y=234", "y", "", false)]
			[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=234", "x", "234", false)]
			[InlineData("http://www.aweXpect.com/foo/bar?x=123&y=234", "y", "123", false)]
			[InlineData("https://www.aweXpect.com/foo/bar?x=123&foo&y=234", "foo", "", true)]
			public async Task WithQueryParameter_ShouldVerifyQueryParameters(string uri, string key, string value,
				bool expectMatch)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.GetAsync(It.IsUri().WithQuery(key, value))
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

				HttpResponseMessage result = await httpClient.GetAsync(uri, CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectMatch ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}

			[Theory]
			[InlineData("x", "123", "z", "345")]
			[InlineData("z", "345", "x", "123")]
			public async Task WithQueryParameters_ShouldMatchQueryParametersInAnyOrder(params string[] query)
			{
				List<(string, HttpQueryParameterValue)> queryParameters = new();
				for (int i = 0; i < query.Length; i += 2)
				{
					queryParameters.Add((query[i], new HttpQueryParameterValue(query[i + 1])));
				}

				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.GetAsync(It.IsUri().WithQuery(queryParameters))
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

				HttpResponseMessage result = await httpClient.GetAsync("https://www.aweXpect.com?x=123&y=234&z=345",
					CancellationToken.None);

				await That(result.StatusCode).IsEqualTo(HttpStatusCode.OK);
			}

			[Theory]
			[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=234", "?x=123&y=234", true)]
			[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=234", "?y=234&x=123&", true)]
			[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=234", "x=123", true)]
			[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=234", "y=234", true)]
			[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=234", "x", false)]
			[InlineData("http://www.aweXpect.com/foo/bar?x=123&y=234", "y", false)]
			[InlineData("https://www.aweXpect.com/foo/bar?x=123&y=234", "x=234", false)]
			[InlineData("http://www.aweXpect.com/foo/bar?x=123&y=234", "y=123", false)]
			[InlineData("http://www.aweXpect.com/foo/bar?x=123&y=234", "x=123&y=23", false)]
			[InlineData("http://www.aweXpect.com/foo/bar?x=123&foo&y=234", "foo", true)]
			public async Task WithQueryString_ShouldVerifyQueryParameters(string uri, string query, bool expectMatch)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.GetAsync(It.IsUri().WithQuery(query))
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

				HttpResponseMessage result = await httpClient.GetAsync(uri, CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectMatch ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}
		}
	}
}
