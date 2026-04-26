using System.Net;
using System.Net.Http;
using System.Threading;
using Mockolate.Exceptions;
using Mockolate.Parameters;
using Mockolate.Setup;
using Mockolate.Verify;
using Mockolate.Web;

namespace Mockolate.Tests.Web;

public sealed partial class HttpClientExtensionsTests
{
	[Fact]
	public async Task CustomParameter_WithoutIParameterMatch_ShouldBeInvokedViaAdapter()
	{
		HttpClient httpClient = HttpClient.CreateMock();
		IReturnMethodSetup<Task<HttpResponseMessage>, HttpRequestMessage, CancellationToken> setup = httpClient
			.Mock.Setup
			.GetAsync(new InvalidParameter())
			.ReturnsAsync(HttpStatusCode.OK);
		ReturnMethodSetup<Task<HttpResponseMessage>, HttpRequestMessage, CancellationToken> methodSetup =
			(ReturnMethodSetup<Task<HttpResponseMessage>, HttpRequestMessage, CancellationToken>)setup;

		void Act()
		{
			methodSetup.Matches(
				new HttpRequestMessage(HttpMethod.Get, "http://localhost/"),
				CancellationToken.None);
		}

		await That(Act).Throws<NotSupportedException>();
	}

	[Fact]
	public async Task InvokedSetup_ShouldWorkForHttpClient()
	{
		HttpClient httpClient = HttpClient.CreateMock();
		IMethodSetup setup = httpClient
			.Mock.Setup.GetAsync(It.IsUri("https://www.aweXpect.com"))
			.ReturnsAsync(HttpStatusCode.Accepted);
		HttpResponseMessage result =
			await httpClient.GetAsync("https://www.aweXpect.com", CancellationToken.None);

		await That(result.StatusCode)
			.IsEqualTo(HttpStatusCode.Accepted);
		await That(httpClient.Mock.VerifySetup(setup)).Once();
	}

	[Fact]
	public async Task InvokedSetup_WhenMethodSetupIsNotVerifiable_ShouldThrowMockException()
	{
		HttpClient sut = HttpClient.CreateMock();
		IMethodSetup setup = new MyMethodSetup();

		void Act()
		{
			sut.Mock.VerifySetup(setup).Never();
		}

		await That(Act).Throws<MockException>()
			.WithMessage("The setup is not verifiable.");
	}

	[Fact]
	public async Task NullRequestUri_ShouldInvokeCallbackWithNull()
	{
		// ReSharper disable once VariableCanBeNotNullable
		string? callbackUri = "initialized";
		HttpClient httpClient = HttpClient.CreateMock();
		IMethodSetup setup = httpClient
			.Mock.Setup
			.GetAsync(It.Matches("*").Do(uri => callbackUri = uri))
			.ReturnsAsync(HttpStatusCode.OK);
		ReturnMethodSetup<Task<HttpResponseMessage>, HttpRequestMessage, CancellationToken> interactiveSetup = (ReturnMethodSetup<Task<HttpResponseMessage>, HttpRequestMessage, CancellationToken>)setup;

		interactiveSetup.TriggerCallbacks(new HttpRequestMessage(), CancellationToken.None);
		await That(callbackUri).IsNull();
	}

	[Fact]
	public async Task NullRequestUri_ShouldReturnFalse()
	{
		HttpClient httpClient = HttpClient.CreateMock();
		IMethodSetup setup = httpClient
			.Mock.Setup
			.GetAsync(It.Matches("*"))
			.ReturnsAsync(HttpStatusCode.OK);
		ReturnMethodSetup<Task<HttpResponseMessage>, HttpRequestMessage, CancellationToken> methodSetup =
			(ReturnMethodSetup<Task<HttpResponseMessage>, HttpRequestMessage, CancellationToken>)setup;

		bool result = methodSetup.Matches(new HttpRequestMessage(), CancellationToken.None);

		await That(result).IsFalse();
	}

	[Fact]
	public async Task NullUri_ShouldReturnFalse()
	{
		HttpClient httpClient = HttpClient.CreateMock();
		IReturnMethodSetup<Task<HttpResponseMessage>, HttpRequestMessage, CancellationToken> setup = httpClient
			.Mock.Setup
			.GetAsync(It.IsAny<string?>())
			.ReturnsAsync(HttpStatusCode.OK);
		ReturnMethodSetup<Task<HttpResponseMessage>, HttpRequestMessage, CancellationToken> methodSetup =
			(ReturnMethodSetup<Task<HttpResponseMessage>, HttpRequestMessage, CancellationToken>)setup;

		bool result = methodSetup.Matches(new HttpRequestMessage(), CancellationToken.None);

		await That(result).IsFalse();
	}

	[Fact]
	public async Task SendAsync_WithoutMockedHttpMessageHandler_ShouldThrowMockException()
	{
		HttpClient httpClient = HttpClient.CreateMock([]);

		void Act()
		{
			httpClient.Mock.Setup
				.SendAsync(It.IsHttpRequestMessage())
				.ReturnsAsync(HttpStatusCode.OK);
		}

		await That(Act).Throws<MockException>()
			.WithMessage(
				"Cannot setup HttpClient when it is not mocked with a mockable HttpMessageHandler.");
	}

	[Fact]
	public async Task ShouldSupportMonitoring()
	{
		int callbackCount = 0;
		HttpClient httpClient = HttpClient.CreateMock();
		httpClient.Mock.Setup
			.GetAsync(It.Matches("*")
				.Do(_ => callbackCount++)
				.Monitor(out IParameterMonitor<string> monitor))
			.ReturnsAsync(HttpStatusCode.OK);

		await httpClient.GetAsync("https://www.aweXpect.com/foo", CancellationToken.None);
		await httpClient.PostAsync("https://www.aweXpect.com/bar", null, CancellationToken.None);
		await httpClient.GetAsync("https://www.aweXpect.com/baz", CancellationToken.None);

		await That(monitor.Values).IsEqualTo([
			"https://www.awexpect.com/foo",
			"https://www.awexpect.com/baz",
		]);
		await That(callbackCount).IsEqualTo(2);
	}

	[Theory]
	[InlineData("*aweXpect.com")]
	[InlineData("*aweXpect.com/")]
	public async Task TrailingSlash_ShouldBeIgnored(string matchPattern)
	{
		HttpClient httpClient = HttpClient.CreateMock();
		httpClient.Mock.Setup
			.GetAsync(It.Matches(matchPattern))
			.ReturnsAsync(HttpStatusCode.OK);

		HttpResponseMessage result =
			await httpClient.GetAsync("https://www.aweXpect.com", CancellationToken.None);

		await That(result.StatusCode)
			.IsEqualTo(HttpStatusCode.OK);
	}

	[Fact]
	public async Task TrailingSlash_WhenNotPresent_ShouldNotBeAdded()
	{
		HttpClient httpClient = HttpClient.CreateMock();
		httpClient.Mock.Setup
			.GetAsync(It.Matches("*www.aweXpect.com/foo/"))
			.ReturnsAsync(HttpStatusCode.OK);

		HttpResponseMessage result =
			await httpClient.GetAsync("https://www.aweXpect.com/foo", CancellationToken.None);

		await That(result.StatusCode)
			.IsEqualTo(HttpStatusCode.NotImplemented);
	}

	private sealed class InvalidParameter : IParameter<string?>
	{
		public bool Matches(object? value)
			=> throw new NotSupportedException();

		public void InvokeCallbacks(object? value)
			=> throw new NotSupportedException();

		public IParameter<string?> Do(Action<string?> callback)
			=> throw new NotSupportedException();
	}

	private sealed class MyMethodSetup : IMethodSetup
	{
		public string Name { get; } = "Foo";
	}
}
