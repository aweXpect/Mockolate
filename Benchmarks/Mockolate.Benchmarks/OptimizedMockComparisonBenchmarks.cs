using System;
using BenchmarkDotNet.Attributes;
using Mockolate.Benchmarks.RealisticSuggestedFix;
using Mockolate.Benchmarks.Suggested;
using Mockolate.Parameters;
using Mockolate.Verify;

namespace Mockolate.Benchmarks;

#pragma warning disable CA1822 // Mark members as static

/// <summary>
///     Per-member-kind harness exercising the full lifecycle (setup → N calls → verify) on a generated
///     Mockolate mock. Originally a side-by-side comparison against the hand-written D-preview mocks
///     (deleted in v3.0 once the real runtime caught up); kept as a focused per-kind reference next to
///     the broader cross-library suites in <see cref="CompleteMethodBenchmarks" /> &amp; co.
/// </summary>
public class OptimizedMockComparisonBenchmarks : BenchmarksBase
{
	[Params(1, 10)]
	public int N { get; set; }

	public interface IMockolateMy2ParamInterface
	{
		bool MyFunc(int value, string name);
	}

	public interface IMockolateCounterInterface
	{
		int Counter { get; set; }
	}

	public interface IMockolateKeyIndexerInterface
	{
		bool this[string key] { get; set; }
	}

	public interface IMockolateSomeEventInterface
	{
		event EventHandler SomeEvent;
	}

	[BenchmarkCategory("Method"), Benchmark]
	public void Method_Mockolate()
	{
		IMockolateMy2ParamInterface sut = IMockolateMy2ParamInterface.CreateMock();
		sut.Mock.Setup.MyFunc(It.IsAny<int>(), It.IsAny<string>()).Returns(true);

		for (int i = 0; i < N; i++)
		{
			sut.MyFunc(42, "hello");
		}

		sut.Mock.Verify.MyFunc(It.IsAny<int>(), It.IsAny<string>()).Exactly(N);
	}

	[BenchmarkCategory("Method"), Benchmark]
	public void Method_Suggested()
	{
		SuggestedMy2ParamMock sut = new();
		sut.Mock.Setup.MyFunc(It.IsAny<int>(), It.IsAny<string>()).Returns(true);

		for (int i = 0; i < N; i++)
		{
			sut.MyFunc(42, "hello");
		}

		sut.Mock.Verify.MyFunc(It.IsAny<int>(), It.IsAny<string>()).Exactly(N);
	}

	[BenchmarkCategory("Method"), Benchmark]
	public void Method_RealisticSuggested()
	{
		RealisticSuggestedMy2ParamMock sut = new();
		sut.Mock.Setup.MyFunc(It.IsAny<int>(), It.IsAny<string>()).Returns(true);

		for (int i = 0; i < N; i++)
		{
			sut.MyFunc(42, "hello");
		}

		sut.Mock.Verify.MyFunc(It.IsAny<int>(), It.IsAny<string>()).Exactly(N);
	}

	[BenchmarkCategory("Property"), Benchmark]
	public void Property_Mockolate()
	{
		IMockolateCounterInterface sut = IMockolateCounterInterface.CreateMock();
		sut.Mock.Setup.Counter.InitializeWith(42);

		for (int i = 0; i < N; i++)
		{
			_ = sut.Counter;
		}

		sut.Mock.Verify.Counter.Got().Exactly(N);
	}

	[BenchmarkCategory("Property"), Benchmark]
	public void Property_Suggested()
	{
		SuggestedCounterMock sut = new();
		sut.Mock.Setup.Counter.InitializeWith(42);

		for (int i = 0; i < N; i++)
		{
			_ = sut.Counter;
		}

		sut.Mock.Verify.Counter.Got().Exactly(N);
	}

	[BenchmarkCategory("Indexer"), Benchmark]
	public void Indexer_Mockolate()
	{
		IMockolateKeyIndexerInterface sut = IMockolateKeyIndexerInterface.CreateMock();
		sut.Mock.Setup[It.IsAny<string>()].Returns(true);

		for (int i = 0; i < N; i++)
		{
			_ = sut["key"];
		}

		sut.Mock.Verify[It.IsAny<string>()].Got().Exactly(N);
	}

	[BenchmarkCategory("Indexer"), Benchmark]
	public void Indexer_Suggested()
	{
		SuggestedKeyIndexerMock sut = new();
		sut.Mock.Setup[It.IsAny<string>()].Returns(true);

		for (int i = 0; i < N; i++)
		{
			_ = sut["key"];
		}

		sut.Mock.Verify[It.IsAny<string>()].Got().Exactly(N);
	}

	[BenchmarkCategory("Event"), Benchmark]
	public void Event_Mockolate()
	{
		IMockolateSomeEventInterface sut = IMockolateSomeEventInterface.CreateMock();

		for (int i = 0; i < N; i++)
		{
			EventHandler handler = (_, _) => { };
			sut.SomeEvent += handler;
		}

		sut.Mock.Verify.SomeEvent.Subscribed().Exactly(N);
	}

	[BenchmarkCategory("Event"), Benchmark]
	public void Event_Suggested()
	{
		SuggestedSomeEventMock sut = new();

		for (int i = 0; i < N; i++)
		{
			EventHandler handler = (_, _) => { };
			sut.SomeEvent += handler;
		}

		sut.Mock.Verify.SomeEvent.Subscribed().Exactly(N);
	}
}

#pragma warning restore CA1822
