using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using Mockolate.Web;

namespace Mockolate.Tests.Web;

public sealed partial class ItExtensionsTests
{
	public sealed partial class IsHttpContentTests
	{
		public sealed class WithFormDataTests
		{
			[Theory]
			[InlineData(true, "x", "123", "y", "234", "z", "345")]
			[InlineData(true, "x", "123", "z", "345", "y", "234")]
			[InlineData(false, "y", "234", "z", "345")]
			[InlineData(false, "x", "123", "z", "345")]
			[InlineData(false, "x", "123", "y", "234")]
			public async Task Exactly_ShouldOnlyMatchWhenAllParametersAreChecked(
				bool expectSuccess, params string[] rawValues)
			{
				List<(string, HttpFormDataValue)> values = new();
				for (int i = 0; i < rawValues.Length; i += 2)
				{
					values.Add((rawValues[i], new HttpFormDataValue(rawValues[i + 1])));
				}

				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithFormData(values).Exactly())
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
				MultipartFormDataContent content = new()
				{
					new FormUrlEncodedContent(new Dictionary<string, string>
					{
						{
							"x", "123"
						},
						{
							"y", "234"
						},
						{
							"z", "345"
						},
					}),
				};

				HttpResponseMessage result =
					await httpClient.PostAsync("https://www.aweXpect.com", content, CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}

			[Theory]
			[InlineData("x", "123", "z", "345")]
			[InlineData("z", "345", "x", "123")]
			public async Task WithFormData_ShouldMatchParametersInAnyOrder(params string[] rawValues)
			{
				List<(string, HttpFormDataValue)> values = new();
				for (int i = 0; i < rawValues.Length; i += 2)
				{
					values.Add((rawValues[i], new HttpFormDataValue(rawValues[i + 1])));
				}

				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithFormData(values))
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
				MultipartFormDataContent content = new()
				{
					new FormUrlEncodedContent(new Dictionary<string, string>
					{
						{
							"x", "123"
						},
						{
							"y", "234"
						},
						{
							"z", "345"
						},
					}),
				};

				HttpResponseMessage result =
					await httpClient.PostAsync("https://www.aweXpect.com", content, CancellationToken.None);

				await That(result.StatusCode).IsEqualTo(HttpStatusCode.OK);
			}

			[Theory]
			[InlineData("x", "123", true)]
			[InlineData("y", "234", true)]
			[InlineData("x", "", false)]
			[InlineData("y", "", false)]
			[InlineData("x", "234", false)]
			[InlineData("y", "123", false)]
			[InlineData("foo", "", true)]
			public async Task WithFormDataParameter_ShouldVerifyParameters(string key, string value, bool expectMatch)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithFormData(key, value))
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
				MultipartFormDataContent content = new()
				{
					new FormUrlEncodedContent(new Dictionary<string, string>
					{
						{
							"x", "123"
						},
						{
							"foo", ""
						},
						{
							"y", "234"
						},
					}),
				};

				HttpResponseMessage result =
					await httpClient.PostAsync("https://www.aweXpect.com", content, CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectMatch ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}

			[Theory]
			[InlineData("?x=123&y=234", true)]
			[InlineData("?y=234&x=123&", true)]
			[InlineData("x=123", true)]
			[InlineData("y=234", true)]
			[InlineData("x", false)]
			[InlineData("y", false)]
			[InlineData("x=234", false)]
			[InlineData("y=123", false)]
			[InlineData("foo", true)]
			public async Task WithFormDataString_ShouldVerifyParameters(string values, bool expectMatch)
			{
				HttpClient httpClient = Mock.Create<HttpClient>();
				httpClient.SetupMock.Method
					.PostAsync(It.IsAny<Uri>(), It.IsHttpContent().WithFormData(values))
					.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
				MultipartFormDataContent content = new()
				{
					new FormUrlEncodedContent(new Dictionary<string, string>
					{
						{
							"x", "123"
						},
						{
							"foo", ""
						},
						{
							"y", "234"
						},
					}),
				};

				HttpResponseMessage result =
					await httpClient.PostAsync("https://www.aweXpect.com", content, CancellationToken.None);

				await That(result.StatusCode)
					.IsEqualTo(expectMatch ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
			}
		}
	}
}
