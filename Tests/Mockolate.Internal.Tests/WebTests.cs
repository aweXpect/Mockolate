using System.IO;
using System.Net;
using System.Net.Http;
using Mockolate.Parameters;
using Mockolate.Web;

namespace Mockolate.Internal.Tests;

public class WebTests
{
	[Fact]
	public async Task HttpContentParameter_MatchesSomeOtherObject_ShouldReturnFalse()
	{
		ItExtensions.IHttpContentParameter sut = It.IsHttpContent();
		IParameter parameter = (IParameter)sut;

		bool result = parameter.Matches(new NamedParameterValue<SomeOtherObject>(string.Empty, new SomeOtherObject()));

		await That(result).IsFalse();
	}

	// ...existing code...

	[Fact]
	public async Task WhenParameterDoesNotImplementIHttpRequestMessagePropertyParameter_ShouldFallbackToParameterMatch()
	{
		ItExtensions.IHttpContentParameter parameter =
			ItExtensions.IHttpContentParameter.CreateMock().Implementing<IParameter>();
		parameter.Mock.As<IParameter>().Setup.Matches(It.IsAny<INamedParameterValue>()).Returns(true);

		ItExtensions.IStringContentBodyParameter sut = parameter.WithString("foo");

		bool result = ((IHttpRequestMessagePropertyParameter<HttpContent?>)sut).Matches(null, null);

		await That(result).IsTrue();
		await That(parameter.Mock.As<IParameter>().Verify
				.Matches(It.IsAny<INamedParameterValue>()))
			.Once();
	}

	[Fact]
	public async Task WhenParameterImplementsIHttpRequestMessagePropertyParameter_ShouldUseThisMatch()
	{
		ItExtensions.IHttpContentParameter parameter = ItExtensions.IHttpContentParameter.CreateMock()
			.Implementing<IParameter>()
			.Implementing<IHttpRequestMessagePropertyParameter<HttpContent?>>();
		parameter.Mock.As<IParameter>().Setup.Matches(It.IsAny<INamedParameterValue>()).Returns(false);
		parameter.Mock.As<IHttpRequestMessagePropertyParameter<HttpContent?>>().Setup
			.Matches(It.IsAny<HttpContent?>(), It.IsAny<HttpRequestMessage?>()).Returns(true);

		ItExtensions.IStringContentBodyParameter sut = parameter.WithString("foo");

		bool result = ((IHttpRequestMessagePropertyParameter<HttpContent?>)sut).Matches(null, null);

		await That(result).IsTrue();
		await That(parameter.Mock.As<IHttpRequestMessagePropertyParameter<HttpContent?>>().Verify
				.Matches(It.IsNull<HttpContent?>(), It.IsNull<HttpRequestMessage?>()))
			.Once();
		await That(parameter.Mock.As<IParameter>().Verify
				.Matches(It.IsAny<INamedParameterValue>()))
			.Never();
	}

	[Fact]
	public async Task WithoutMediaTypeHeader_WhenNoneIsRequired_ShouldReturnTrue()
	{
		ItExtensions.IHttpContentParameter sut = It.IsHttpContent();
		IParameter parameter = (IParameter)sut;

		bool result = parameter.Matches(new NamedParameterValue<MyHttpContent>(string.Empty, new MyHttpContent()));

		await That(result).IsTrue();
	}

	[Fact]
	public async Task WithoutMediaTypeHeader_WhenOneIsRequired_ShouldReturnFalse()
	{
		ItExtensions.IHttpContentParameter sut = It.IsHttpContent("*");
		IParameter parameter = (IParameter)sut;

		bool result = parameter.Matches(new NamedParameterValue<MyHttpContent>(string.Empty, new MyHttpContent()));

		await That(result).IsFalse();
	}

	private sealed class MyHttpContent : HttpContent
	{
		protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
			=> Task.CompletedTask;

		protected override bool TryComputeLength(out long length)
		{
			length = 0;
			return false;
		}
	}

	private sealed class SomeOtherObject;
}
