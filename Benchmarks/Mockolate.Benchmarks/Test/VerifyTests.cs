using System.Diagnostics;
using BenchmarkDotNet.Attributes;
using Mockolate.Interactions;
using Mockolate.Parameters;
using Mockolate.Setup;
using Mockolate.Internal.Tests.MinimalTest;
using Mockolate.Verify;

namespace Mockolate.Benchmarks.Test;

public class VerifyBenchmarks : BenchmarksBase
{
	[Benchmark(Baseline = true)]
	public void Mockolate()
	{
		var sut = IFoo.CreateMock();
		sut.Mock.Setup.MyMethod(It.Is(6)).Returns("foo");

		_ = sut.MyMethod(6);

		IParameters parameters = null;

		bool result = parameters switch
		{
			IParametersMatch m => m.Matches([]),
			INamedParametersMatch m => m.Matches([]),
			_ => true
		};
		sut.Mock.Verify.MyMethod(It.Is(6)).Never();
	}
	/// <summary>
	///     <see href="https://github.com/thomhurst/TUnit/" />
	/// </summary>
	[Benchmark]
	public void TUnitMocks()
	{
		Mock<IFoo> mock = TUnit.Mocks.Mock.Of<IFoo>();
		mock.MyMethod(Is(6)).Returns("foo");

		IFoo sut = mock.Object;
		_ = sut.MyMethod(6);

		mock.MyMethod(Is(6)).WasCalled(Times.Once);
	}
}
