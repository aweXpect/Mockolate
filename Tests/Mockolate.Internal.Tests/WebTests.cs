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

		bool result = parameter.Matches(new SomeOtherObject());

		await That(result).IsFalse();
	}

	[Fact]
	public async Task HttpContentParameter_MatchesWithNullContent_ShouldReturnFalse()
	{
		ItExtensions.IHttpContentParameter sut = It.IsHttpContent();
		IHttpRequestMessagePropertyParameter<HttpContent?> parameter =
			(IHttpRequestMessagePropertyParameter<HttpContent?>)sut;

		bool result = parameter.Matches(null, new HttpRequestMessage());

		await That(result).IsFalse();
	}

	[Fact]
	public async Task WhenParameterImplementsIHttpRequestMessagePropertyParameter_ShouldUseThisMatch()
	{
		ItExtensions.IHttpContentParameter parameter =
			Mock.Create<ItExtensions.IHttpContentParameter, IParameter,
				IHttpRequestMessagePropertyParameter<HttpContent?>>();
		parameter.SetupIParameterMock.Method.Matches(It.IsAny<object?>()).Returns(true);
		parameter.SetupIHttpRequestMessagePropertyParameter_HttpContent_Mock.Method
			.Matches(It.IsAny<HttpContent?>(), It.IsAny<HttpRequestMessage?>()).Returns(true);

		ItExtensions.IStringContentBodyParameter sut = parameter.WithString("foo");

		bool result = ((IHttpRequestMessagePropertyParameter<HttpContent?>)sut).Matches(null, null);

		await That(result).IsTrue();
		await That(parameter.VerifyOnIHttpRequestMessagePropertyParameter_HttpContent_Mock.Invoked
				.Matches(It.IsNull<HttpContent?>(), It.IsNull<HttpRequestMessage?>()))
			.Once();
	}

	[Fact]
	public async Task WithoutMediaTypeHeader_WhenNoneIsRequired_ShouldReturnTrue()
	{
		ItExtensions.IHttpContentParameter sut = It.IsHttpContent();
		IParameter parameter = (IParameter)sut;

		bool result = parameter.Matches(new MyHttpContent());

		await That(result).IsTrue();
	}

	[Fact]
	public async Task WithoutMediaTypeHeader_WhenOneIsRequired_ShouldReturnFalse()
	{
		ItExtensions.IHttpContentParameter sut = It.IsHttpContent("*");
		IParameter parameter = (IParameter)sut;

		bool result = parameter.Matches(new MyHttpContent());

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
