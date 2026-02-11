using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using Mockolate.Parameters;
using Mockolate.Web;

namespace Mockolate.Tests.Web;

public sealed partial class ItExtensionsTests
{
	public sealed partial class IsHttpRequestMessageTests
	{
		[Fact]
		public async Task ShouldSupportMonitoring()
		{
			int callbackCount = 0;
			List<ByteArrayContent> responses =
			[
				new([]),
				new([0x66,]),
				new([0x62, 0x61, 0x72,]),
			];
			HttpClient httpClient = Mock.Create<HttpClient>();
			httpClient.SetupMock.Method.SendAsync(It.IsHttpRequestMessage()
				.Do(_ => callbackCount++)
				.Monitor(out IParameterMonitor<HttpRequestMessage> monitor));

			foreach (ByteArrayContent response in responses)
			{
				await httpClient.PostAsync("https://www.aweXpect.com", response, CancellationToken.None);
			}

#if !NETFRAMEWORK
			await That(
					(await Task.WhenAll(monitor.Values.Select(c => c!.Content!.ReadAsByteArrayAsync())))
					.Select(x => x.Length))
				.IsEqualTo([0, 1, 3,]);
#endif
			await That(callbackCount).IsEqualTo(3);
		}

		[Theory]
		[InlineData(nameof(HttpMethod.Get), true)]
		[InlineData(nameof(HttpMethod.Delete), false)]
		[InlineData(nameof(HttpMethod.Post), false)]
		[InlineData(nameof(HttpMethod.Put), false)]
		public async Task WithMethod_ShouldVerifyMethod(string method, bool expectSuccess)
		{
			HttpClient httpClient = Mock.Create<HttpClient>();
			httpClient.SetupMock.Method
				.SendAsync(It.IsHttpRequestMessage(new HttpMethod(method)))
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

			HttpResponseMessage result = await httpClient.GetAsync("https://www.aweXpect.com",
				CancellationToken.None);

			await That(result.StatusCode).IsEqualTo(expectSuccess ? HttpStatusCode.OK : HttpStatusCode.NotImplemented);
		}
	}
}
