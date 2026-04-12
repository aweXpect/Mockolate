using System.Diagnostics;
using BenchmarkDotNet.Attributes;
using Mockolate.Interactions;
using Mockolate.Parameters;
using Mockolate.Setup;
using Mockolate.Internal.Tests.MinimalTest;

namespace Mockolate.Benchmarks.Test;

public class MinimalBenchmarks : BenchmarksBase
{
	[Benchmark]
	public string Minimal()
	{
		var sut = new Mockolate.Internal.Tests.MinimalTest.Mock.IFoo();
		sut.MockSetup.MyMethod(It.Is(6)).Returns("foo");

		var result1 = sut.MyMethod(6);
		result1 = sut.MyMethod(5);
		return result1;
	}

	[Benchmark(Baseline = true)]
	public string Mockolate()
	{
		var sut = IFoo.CreateMock();
		sut.Mock.Setup.MyMethod(It.Is(6)).Returns("foo");

		var result1 = sut.MyMethod(6);
		result1 = sut.MyMethod(5);
		return result1;
	}
	/// <summary>
	///     <see href="https://github.com/thomhurst/TUnit/" />
	/// </summary>
	[Benchmark]
	public string TUnitMocks()
	{
		Mock<IFoo> mock = TUnit.Mocks.Mock.Of<IFoo>();
		mock.MyMethod(Is(6)).Returns("foo");

		IFoo sut = mock.Object;
		var result1 = sut.MyMethod(6);
		result1 = sut.MyMethod(5);
		return result1;
	}
}
