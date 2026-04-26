using System.IO;
using System.Net;
using System.Net.Http;
using Mockolate.Parameters;
using Mockolate.Web;

namespace Mockolate.Internal.Tests.Web;

public class ItExtensionsHttpContentTests
{
	[Fact]
	public async Task IsHttpContent_NonGeneric_DispatchesContentThroughCallback()
	{
		ItExtensions.IHttpContentParameter sut = It.IsHttpContent();
		HttpContent? captured = null;
		sut.Do(content => captured = content);

		MyHttpContent target = new();
		sut.InvokeCallbacks(target);

		await That(captured).IsSameAs(target);
	}

	[Fact]
	public async Task IsHttpContent_NonGeneric_DispatchesNullThroughCallback()
	{
		ItExtensions.IHttpContentParameter sut = It.IsHttpContent();
		int invocations = 0;
		HttpContent? captured = new MyHttpContent();
		sut.Do(content =>
		{
			invocations++;
			captured = content;
		});

		sut.InvokeCallbacks(null);

		await That(invocations).IsEqualTo(1);
		await That(captured).IsNull();
	}

	[Fact]
	public async Task IsHttpContent_NonGeneric_IgnoresUnrelatedTypes()
	{
		ItExtensions.IHttpContentParameter sut = It.IsHttpContent();
		int invocations = 0;
		sut.Do(_ => invocations++);

		sut.InvokeCallbacks("not http content");

		await That(invocations).IsEqualTo(0);
	}

	[Fact]
	public async Task IsHttpContent_NonGenericMatches_ReturnsFalseForNullValue()
	{
		ItExtensions.IHttpContentParameter sut = It.IsHttpContent();

		bool result = sut.Matches(null);

		await That(result).IsFalse();
	}

	[Fact]
	public async Task IsHttpContent_NonGenericMatches_ReturnsFalseForUnrelatedType()
	{
		ItExtensions.IHttpContentParameter sut = It.IsHttpContent();

		bool result = sut.Matches("not http content");

		await That(result).IsFalse();
	}

	[Fact]
	public async Task IsHttpContent_TypedMatches_ReturnsFalseForNullValue()
	{
		ItExtensions.IHttpContentParameter sut = It.IsHttpContent();

		bool result = ((IParameterMatch<HttpContent?>)sut).Matches(null);

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
}
