#if NET10_0_OR_GREATER
using System.Net.Http;
using System.Threading.Tasks;
using Mockolate.ExampleTests.GeneratorCoverage;

namespace Mockolate.ExampleTests;

/// <summary>
///     Compile-and-create coverage for every special case in
///     <c>Source/Mockolate.SourceGenerators</c>. The example types in this folder
///     are designed so that this project compiling is itself the primary regression
///     gate — the source generator must successfully process every shape — and each
///     fact below additionally proves that <c>CreateMock</c> doesn't throw at runtime.
/// </summary>
public sealed class MockCreationTests
{
	[Fact]
	public async Task BaseClass_WithMultipleAdditionalInterfaces_CanBeCreated()
	{
		ComprehensiveAbstractClass sut = ComprehensiveAbstractClass.CreateMock()
			.Implementing<ICombinationMockA>()
			.Implementing<ICombinationMockB>();
		await That(sut).IsNotNull();
	}

	[Fact]
	public async Task ComprehensiveAbstractClass_CanBeCreated()
	{
		ComprehensiveAbstractClass sut = ComprehensiveAbstractClass.CreateMock();
		await That(sut).IsNotNull();
	}

	[Fact]
	public async Task ComprehensiveDelegate_CanBeCreated()
	{
		ComprehensiveDelegate sut = ComprehensiveDelegate.CreateMock();
		await That(sut).IsNotNull();
	}

	[Fact]
	public void ComprehensiveInterface_CanBeCreated()
	{
		IComprehensiveInterface sut = IComprehensiveInterface.CreateMock();
		Assert.NotNull(sut);
	}

	[Fact]
	public async Task HttpClient_CanBeCreated()
	{
		HttpClient sut = HttpClient.CreateMock();
		await That(sut).IsNotNull();
	}

	[Fact]
	public async Task KeywordEdgeCases_CanBeCreated()
	{
		IKeywordEdgeCases sut = IKeywordEdgeCases.CreateMock();
		await That(sut).IsNotNull();
	}

	[Fact]
	public async Task RefStructConsumer_CanBeCreated()
	{
		IRefStructConsumer sut = IRefStructConsumer.CreateMock();
		await That(sut).IsNotNull();
	}

	[Fact]
	public void StaticAbstractMembers_CanBeCreated()
	{
		IStaticAbstractMembers sut = IStaticAbstractMembers.CreateMock();
		Assert.NotNull(sut);
	}
}
#endif
