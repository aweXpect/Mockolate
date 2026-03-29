using BenchmarkDotNet.Attributes;
using FakeItEasy;
using Imposter.Abstractions;
using Mockolate.Verify;
using NSubstitute;
using Arg = NSubstitute.Arg;
using Times = Moq.Times;

[assembly: GenerateImposter(typeof(Mockolate.Benchmarks.CompleteIndexerBenchmarks.IMyIndexerInterface))]

namespace Mockolate.Benchmarks;
#pragma warning disable CA1822 // Mark members as static
/// <summary>
///     In this benchmark we check the case of an interface mock with an indexer, setup the indexer and verify
///     the getter was called once.<br />
/// </summary>
public class CompleteIndexerBenchmarks : BenchmarksBase
{
	/// <summary>
	///     <see href="https://awexpect.com/Mockolate" />
	/// </summary>
	[Benchmark(Baseline = true)]
	public void Indexer_Mockolate()
	{
		IMyIndexerInterface sut = IMyIndexerInterface.CreateMock();
		sut.Mock.Setup[It.IsAny<int>()].Returns("foo");

		_ = sut[42];

		sut.Mock.Verify[It.IsAny<int>()].Got().Once();
	}

	/// <summary>
	///     <see href="https://github.com/devlooped/moq" />
	/// </summary>
	[Benchmark]
	public void Indexer_Moq()
	{
		Moq.Mock<IMyIndexerInterface> mock = new();
		mock.Setup(x => x[Moq.It.IsAny<int>()]).Returns("foo");

		_ = mock.Object[42];

		mock.Verify(x => x[Moq.It.IsAny<int>()], Times.Once());
	}

	/// <summary>
	///     <see href="https://nsubstitute.github.io/" />
	/// </summary>
	[Benchmark]
	public void Indexer_NSubstitute()
	{
		IMyIndexerInterface mock = Substitute.For<IMyIndexerInterface>();
		mock[Arg.Any<int>()].Returns("foo");

		_ = mock[42];

		_ = mock.Received(1)[Arg.Any<int>()];
	}

	/// <summary>
	///     <see href="https://fakeiteasy.github.io/" />
	/// </summary>
	[Benchmark]
	public void Indexer_FakeItEasy()
	{
		IMyIndexerInterface mock = A.Fake<IMyIndexerInterface>();
		A.CallTo(() => mock[A<int>.Ignored]).Returns("foo");

		_ = mock[42];

		A.CallTo(() => mock[A<int>.Ignored]).MustHaveHappened(1, FakeItEasy.Times.Exactly);
	}

	/// <summary>
	///     <see href="https://github.com/themidnightgospel/Imposter" />
	/// </summary>
	[Benchmark]
	public void Indexer_Imposter()
	{
		IMyIndexerInterfaceImposter imposter = IMyIndexerInterface.Imposter();
		imposter[Imposter.Abstractions.Arg<int>.Any()].Getter().Returns("foo");

		_ = imposter.Instance()[42];

		imposter[Imposter.Abstractions.Arg<int>.Any()].Getter().Called(Count.Once());
	}

	/* Indexers not supported on TUnit.Mocks
	/// <summary>
	///     <see href="https://github.com/thomhurst/TUnit/" />
	/// </summary>
	[Benchmark]
	public void Indexer_TUnitMocks()
	{
		TUnit.Mocks.Mock<IMyIndexerInterface> mock = TUnit.Mocks.Mock.Of<IMyIndexerInterface>();
		mock[Any<int>()].Returns("foo");

		_ = mock.Object[42];

		mock[Any<int>()].WasCalled();
	}
	*/

	public interface IMyIndexerInterface
	{
		string this[int index] { get; set; }
	}
}
#pragma warning restore CA1822 // Mark members as static
