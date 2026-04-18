using BenchmarkDotNet.Attributes;
using FakeItEasy;
using Imposter.Abstractions;
using Mockolate.Benchmarks;
using Mockolate.Verify;
using NSubstitute;
using Arg = NSubstitute.Arg;
using Times = Moq.Times;

[assembly: GenerateImposter(typeof(CompleteIndexerBenchmarks.IMyIndexerInterface))]

namespace Mockolate.Benchmarks;
#pragma warning disable CA1822 // Mark members as static
/// <summary>
///     In this benchmark we check the case of an interface mock with an indexer, setup the indexer and verify
///     the getter was called exactly <see cref="InvocationCount" /> times.
/// </summary>
public class CompleteIndexerBenchmarks : BenchmarksBase
{
	[Params(1, 10)]
	public int InvocationCount { get; set; }

	/// <summary>
	///     <see href="https://awexpect.com/Mockolate" />
	/// </summary>
	[Benchmark(Baseline = true)]
	public void Indexer_Mockolate()
	{
		IMyIndexerInterface sut = IMyIndexerInterface.CreateMock();
		sut.Mock.Setup[It.IsAny<int>()].Returns("foo");

		for (int i = 0; i < InvocationCount; i++)
		{
			_ = sut[42];
		}

		sut.Mock.Verify[It.IsAny<int>()].Got().Exactly(InvocationCount);
	}

	/// <summary>
	///     <see href="https://github.com/devlooped/moq" />
	/// </summary>
	[Benchmark]
	public void Indexer_Moq()
	{
		Moq.Mock<IMyIndexerInterface> mock = new();
		mock.Setup(x => x[Moq.It.IsAny<int>()]).Returns("foo");

		for (int i = 0; i < InvocationCount; i++)
		{
			_ = mock.Object[42];
		}

		mock.Verify(x => x[Moq.It.IsAny<int>()], Times.Exactly(InvocationCount));
	}

	/// <summary>
	///     <see href="https://nsubstitute.github.io/" />
	/// </summary>
	[Benchmark]
	public void Indexer_NSubstitute()
	{
		IMyIndexerInterface mock = Substitute.For<IMyIndexerInterface>();
		mock[Arg.Any<int>()].Returns("foo");

		for (int i = 0; i < InvocationCount; i++)
		{
			_ = mock[42];
		}

		_ = mock.Received(InvocationCount)[Arg.Any<int>()];
	}

	/// <summary>
	///     <see href="https://fakeiteasy.github.io/" />
	/// </summary>
	[Benchmark]
	public void Indexer_FakeItEasy()
	{
		IMyIndexerInterface mock = A.Fake<IMyIndexerInterface>();
		A.CallTo(() => mock[A<int>.Ignored]).Returns("foo");

		for (int i = 0; i < InvocationCount; i++)
		{
			_ = mock[42];
		}

		A.CallTo(() => mock[A<int>.Ignored]).MustHaveHappened(InvocationCount, FakeItEasy.Times.Exactly);
	}

	/// <summary>
	///     <see href="https://github.com/themidnightgospel/Imposter" />
	/// </summary>
	[Benchmark]
	public void Indexer_Imposter()
	{
		IMyIndexerInterfaceImposter imposter = IMyIndexerInterface.Imposter();
		imposter[Imposter.Abstractions.Arg<int>.Any()].Getter().Returns("foo");

		for (int i = 0; i < InvocationCount; i++)
		{
			_ = imposter.Instance()[42];
		}

		imposter[Imposter.Abstractions.Arg<int>.Any()].Getter().Called(Count.Exactly(InvocationCount));
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

		for (int i = 0; i < InvocationCount; i++)
		{
			_ = mock.Object[42];
		}

		mock[Any<int>()].WasCalled(TUnit.Mocks.Times.Exactly(InvocationCount));
	}
	*/

	public interface IMyIndexerInterface
	{
		string this[int index] { get; set; }
	}
}
#pragma warning restore CA1822 // Mark members as static
