using Mockolate.Parameters;
using Mockolate.Web;

namespace Mockolate.Internal.Tests.Web;

public class ItExtensionsUriTests
{
	[Fact]
	public async Task IsUri_NonGeneric_DispatchesNullThroughCallback()
	{
		ItExtensions.IUriParameter sut = It.IsUri();
		int invocations = 0;
		Uri? captured = new("http://placeholder");
		sut.Do(uri =>
		{
			invocations++;
			captured = uri;
		});

		sut.InvokeCallbacks(null);

		await That(invocations).IsEqualTo(1);
		await That(captured).IsNull();
	}

	[Fact]
	public async Task IsUri_NonGeneric_DispatchesUriThroughCallback()
	{
		ItExtensions.IUriParameter sut = It.IsUri();
		Uri? captured = null;
		sut.Do(uri => captured = uri);

		Uri target = new("http://x/y");
		sut.InvokeCallbacks(target);

		await That(captured).IsSameAs(target);
	}

	[Fact]
	public async Task IsUri_NonGeneric_IgnoresUnrelatedTypes()
	{
		ItExtensions.IUriParameter sut = It.IsUri();
		int invocations = 0;
		sut.Do(_ => invocations++);

		sut.InvokeCallbacks("not a uri");

		await That(invocations).IsEqualTo(0);
	}

	[Fact]
	public async Task IsUri_NonGenericMatches_ReturnsFalseForNonUriValue()
	{
		ItExtensions.IUriParameter sut = It.IsUri();

		bool result = sut.Matches("not a uri");

		await That(result).IsFalse();
	}

	[Fact]
	public async Task IsUri_NonGenericMatches_ReturnsFalseForNullValue()
	{
		ItExtensions.IUriParameter sut = It.IsUri();

		bool result = sut.Matches(null);

		await That(result).IsFalse();
	}

	[Fact]
	public async Task IsUri_WithPattern_AcceptsBothTrailingSlashAndWithoutTrailingSlash()
	{
		ItExtensions.IUriParameter sut = It.IsUri("http://x/foo");
		IParameterMatch<Uri?> matcher = (IParameterMatch<Uri?>)sut;

		bool matchesWithSlash = matcher.Matches(new Uri("http://x/foo/"));
		bool matchesWithoutSlash = matcher.Matches(new Uri("http://x/foo"));

		await That(matchesWithSlash).IsTrue();
		await That(matchesWithoutSlash).IsTrue();
	}
}
