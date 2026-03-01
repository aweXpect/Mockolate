using System.IO;
using System.Net;
using System.Net.Http;
using Mockolate.Parameters;
using Mockolate.Web;

namespace Mockolate.Internal.Tests;

public class WebTests
{
	[Test]
	public async Task HttpContentParameter_MatchesSomeOtherObject_ShouldReturnFalse()
	{
		ItExtensions.IHttpContentParameter sut = It.IsHttpContent();
		IParameter parameter = (IParameter)sut;

		bool result = parameter.Matches(new SomeOtherObject());

		await That(result).IsFalse();
	}

	[Test]
	public async Task HttpContentParameter_MatchesWithNullContent_ShouldReturnFalse()
	{
		ItExtensions.IHttpContentParameter sut = It.IsHttpContent();
		IHttpRequestMessagePropertyParameter<HttpContent?> parameter =
			(IHttpRequestMessagePropertyParameter<HttpContent?>)sut;

		bool result = parameter.Matches(null, new HttpRequestMessage());

		await That(result).IsFalse();
	}

	[Test]
	public async Task WhenParameterDoesNotImplementIHttpRequestMessagePropertyParameter_ShouldFallbackToParameterMatch()
	{
		ItExtensions.IHttpContentParameter parameter =
			Mock.Create<ItExtensions.IHttpContentParameter, IParameter>();
		parameter.Setup_Mockolate_Parameters_IParameter_Mock.Method.Matches(It.IsAny<object?>()).Returns(true);

		ItExtensions.IStringContentBodyParameter sut = parameter.WithString("foo");

		bool result = ((IHttpRequestMessagePropertyParameter<HttpContent?>)sut).Matches(null, null);

		await That(result).IsTrue();
		await That(parameter.VerifyOn_Mockolate_Parameters_IParameter_Mock.Invoked
				.Matches(It.IsNull<object?>()))
			.Once();
	}

	[Test]
	public async Task WhenParameterImplementsIHttpRequestMessagePropertyParameter_ShouldUseThisMatch()
	{
		ItExtensions.IHttpContentParameter parameter =
			Mock.Create<ItExtensions.IHttpContentParameter, IParameter,
				IHttpRequestMessagePropertyParameter<HttpContent?>>();
		parameter.Setup_Mockolate_Parameters_IParameter_Mock.Method.Matches(It.IsAny<object?>()).Returns(false);
		parameter.Setup_IHttpRequestMessagePropertyParameter_HttpContent__Mock.Method
			.Matches(It.IsAny<HttpContent?>(), It.IsAny<HttpRequestMessage?>()).Returns(true);

		ItExtensions.IStringContentBodyParameter sut = parameter.WithString("foo");

		bool result = ((IHttpRequestMessagePropertyParameter<HttpContent?>)sut).Matches(null, null);

		await That(result).IsTrue();
		await That(parameter.VerifyOn_IHttpRequestMessagePropertyParameter_HttpContent__Mock.Invoked
				.Matches(It.IsNull<HttpContent?>(), It.IsNull<HttpRequestMessage?>()))
			.Once();
		await That(parameter.VerifyOn_Mockolate_Parameters_IParameter_Mock.Invoked
				.Matches(It.IsNull<object?>()))
			.Never();
	}

	[Test]
	public async Task WithoutMediaTypeHeader_WhenNoneIsRequired_ShouldReturnTrue()
	{
		ItExtensions.IHttpContentParameter sut = It.IsHttpContent();
		IParameter parameter = (IParameter)sut;

		bool result = parameter.Matches(new MyHttpContent());

		await That(result).IsTrue();
	}

	[Test]
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
