using BenchmarkDotNet.Attributes;
using FakeItEasy;
using Imposter.Abstractions;
using Mockolate.Benchmarks;
using NSubstitute;

[assembly: GenerateImposter(typeof(MockCreationBenchmarks.ICalculatorService))]

namespace Mockolate.Benchmarks;
#pragma warning disable CA1822 // Mark members as static

/// <summary>
///     Measures the cost of creating an empty mock — no setup, no invocations, no verification.
/// </summary>
public class MockCreationBenchmarks : BenchmarksBase
{
	[Benchmark(Baseline = true, Description = "Mockolate")]
	public object Mockolate_CreateMock()
		=> ICalculatorService.CreateMock();

	[Benchmark(Description = "Imposter")]
	public object Imposter_CreateMock()
	{
		ICalculatorServiceImposter imposter = ICalculatorService.Imposter();
		return imposter.Instance();
	}

	[Benchmark(Description = "TUnit.Mocks")]
	public object TUnitMocks_CreateMock()
	{
		TUnit.Mocks.Mock<ICalculatorService> mock = TUnit.Mocks.Mock.Of<ICalculatorService>();
		return mock.Object;
	}

	[Benchmark(Description = "Moq")]
	public object Moq_CreateMock()
	{
		Moq.Mock<ICalculatorService> mock = new();
		return mock.Object;
	}

	[Benchmark(Description = "NSubstitute")]
	public object NSubstitute_CreateMock()
		=> Substitute.For<ICalculatorService>();

	[Benchmark(Description = "FakeItEasy")]
	public object FakeItEasy_CreateMock()
		=> A.Fake<ICalculatorService>();

	public interface ICalculatorService
	{
		int Zero { get; }
		int Add(int a, int b);
		double Divide(double numerator, double denominator);
		string Format(int value);
	}
}
#pragma warning restore CA1822 // Mark members as static
