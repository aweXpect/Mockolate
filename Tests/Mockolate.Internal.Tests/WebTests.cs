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
