using System.Net;
using System.Net.Http;
using System.Threading;
using Mockolate.Interactions;
using Mockolate.Parameters;
using Mockolate.Setup;
using Mockolate.Web;

namespace Mockolate.Tests.Web;

public sealed partial class HttpClientExtensionsTests
{
	[Fact]
	public async Task Callback_ShouldBeInvoked()
	{
		HttpClient httpClient = Mock.Create<HttpClient>();
		httpClient.SetupMock.Method
			.GetAsync(It.Matches("*").Monitor(out IParameterMonitor<string> monitor))
			.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

		await httpClient.GetAsync("https://www.aweXpect.com/foo", CancellationToken.None);
		await httpClient.PostAsync("https://www.aweXpect.com/bar", null, CancellationToken.None);
		await httpClient.GetAsync("https://www.aweXpect.com/baz", CancellationToken.None);

		await That(monitor.Values).IsEqualTo([
			"https://www.awexpect.com/foo",
			"https://www.awexpect.com/baz",
		]);
	}

	[Fact]
	public async Task InvalidParameter_ShouldReturnTrue()
	{
		HttpClient httpClient = Mock.Create<HttpClient>();
		IReturnMethodSetup<Task<HttpResponseMessage>, HttpRequestMessage, CancellationToken> setup = httpClient
			.SetupMock.Method
			.GetAsync(new InvalidParameter())
			.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
		IInteractiveMethodSetup interactiveSetup = (IInteractiveMethodSetup)setup;

		bool result = interactiveSetup.Matches(new MethodInvocation(0, "System.Net.Http.HttpMessageHandler.SendAsync",
		[
			new NamedParameterValue("request", new HttpRequestMessage()),
			new NamedParameterValue("cancellationToken", CancellationToken.None),
		]));

		await That(result).IsTrue();
	}

	[Fact]
	public async Task NullUri_ShouldReturnFalse()
	{
		HttpClient httpClient = Mock.Create<HttpClient>();
		IReturnMethodSetup<Task<HttpResponseMessage>, HttpRequestMessage, CancellationToken> setup = httpClient
			.SetupMock.Method
			.GetAsync(It.IsAny<string?>())
			.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
		IInteractiveMethodSetup interactiveSetup = (IInteractiveMethodSetup)setup;

		bool result = interactiveSetup.Matches(new MethodInvocation(0, "System.Net.Http.HttpMessageHandler.SendAsync",
		[
			new NamedParameterValue("request", new HttpRequestMessage()),
			new NamedParameterValue("cancellationToken", CancellationToken.None),
		]));

		await That(result).IsFalse();
	}

	[Theory]
	[InlineData("*aweXpect.com")]
	[InlineData("*aweXpect.com/")]
	public async Task TrailingSlash_ShouldBeIgnored(string matchPattern)
	{
		HttpClient httpClient = Mock.Create<HttpClient>();
		httpClient.SetupMock.Method
			.GetAsync(It.Matches(matchPattern))
			.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

		HttpResponseMessage result =
			await httpClient.GetAsync("https://www.aweXpect.com", CancellationToken.None);

		await That(result.StatusCode)
			.IsEqualTo(HttpStatusCode.OK);
	}

	[Fact]
	public async Task TrailingSlash_WhenNotPresent_ShouldNotBeAdded()
	{
		HttpClient httpClient = Mock.Create<HttpClient>();
		httpClient.SetupMock.Method
			.GetAsync(It.Matches("*www.aweXpect.com/foo/"))
			.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

		HttpResponseMessage result =
			await httpClient.GetAsync("https://www.aweXpect.com/foo", CancellationToken.None);

		await That(result.StatusCode)
			.IsEqualTo(HttpStatusCode.NotImplemented);
	}

	private sealed class InvalidParameter : IParameter<string?>
	{
		public IParameter<string?> Do(Action<string?> callback)
			=> throw new NotSupportedException();
	}
}
