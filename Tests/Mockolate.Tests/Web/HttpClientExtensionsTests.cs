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

	private sealed class InvalidParameter : IParameter<string?>
	{
		public IParameter<string?> Do(Action<string?> callback)
			=> throw new NotSupportedException();
	}
}
