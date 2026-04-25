using System;
using BenchmarkDotNet.Attributes;
using Mockolate.Benchmarks.Optimized;
using Mockolate.Parameters;
using Mockolate.Setup;
using Mockolate.Verify;

namespace Mockolate.Benchmarks;

#pragma warning disable CA1822 // Mark members as static

/// <summary>
///     Compares today's Mockolate-generated mock (baseline) against the D-optimized hand-written mock
///     across every member kind: method, property getter, indexer getter, event subscribe.
///     Each pair exercises the full lifecycle (setup → N calls → verify).
/// </summary>
public class OptimizedMockComparisonBenchmarks : BenchmarksBase
{
	[GlobalSetup]
	public void Setup()
	{
		// Feature parity checks — if D broke method or all-member behaviour the benchmarks fail at
		// [GlobalSetup] time, before any numbers are measured.
		OptimizedDMockSmokeTest.Run();
		OptimizedDAllMembersSmokeTest.Run();
	}

	[Params(1, 10)]
	public int N { get; set; }

	// ---- Source-generated Mockolate interfaces for the baseline ----
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

	// =============== METHOD (2 params) ===============

	[BenchmarkCategory("Method"), Benchmark(Baseline = true)]
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
	public void Method_HandwrittenOptimized()
	{
		OptimizedMy2ParamMock sut = new();
		((IReturnMethodSetup<bool, int, string>)sut.Mock.Setup.MyFunc(
			It.IsAny<int>(), It.IsAny<string>())).Returns(true);

		for (int i = 0; i < N; i++)
		{
			sut.MyFunc(42, "hello");
		}

		sut.Mock.Verify.MyFunc(It.IsAny<int>(), It.IsAny<string>()).Exactly(N);
	}

	[BenchmarkCategory("Method"), Benchmark]
	public void Method_HandwrittenOptimizedD()
	{
		OptimizedDMy2ParamMock sut = new();
		((IReturnMethodSetup<bool, int, string>)sut.Mock.Setup.MyFunc(
			It.IsAny<int>(), It.IsAny<string>())).Returns(true);

		for (int i = 0; i < N; i++)
		{
			sut.MyFunc(42, "hello");
		}

		sut.Mock.Verify.MyFunc(It.IsAny<int>(), It.IsAny<string>()).Exactly(N);
	}

	// =============== PROPERTY GETTER ===============

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
	public void Property_HandwrittenOptimizedD()
	{
		OptimizedDAllMembersMock sut = new();
		sut.InitializeCounter(42);

		for (int i = 0; i < N; i++)
		{
			_ = sut.Counter;
		}

		sut.Mock.Verify.Counter.Got().Exactly(N);
	}

	// =============== INDEXER GETTER ===============

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
	public void Indexer_HandwrittenOptimizedD()
	{
		OptimizedDAllMembersMock sut = new();
		sut.InitializeIndexer("key", true);

		for (int i = 0; i < N; i++)
		{
			_ = sut["key"];
		}

		FastCountAssert1<string> assert = sut.Mock.Verify[It.IsAny<string>()].Got();
		assert.Exactly(N);
	}

	// =============== EVENT SUBSCRIBE ===============

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
	public void Event_HandwrittenOptimizedD()
	{
		OptimizedDAllMembersMock sut = new();

		for (int i = 0; i < N; i++)
		{
			EventHandler handler = (_, _) => { };
			sut.SomeEvent += handler;
		}

		sut.Mock.Verify.SomeEvent.Subscribed().Exactly(N);
	}
}

#pragma warning restore CA1822
