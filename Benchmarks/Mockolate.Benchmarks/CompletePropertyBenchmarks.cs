using BenchmarkDotNet.Attributes;
using FakeItEasy;
using Imposter.Abstractions;
using Mockolate.Benchmarks;
using Mockolate.Verify;
using NSubstitute;
using Times = Moq.Times;

[assembly: GenerateImposter(typeof(CompletePropertyBenchmarks.IMyPropertyInterface))]

namespace Mockolate.Benchmarks;
#pragma warning disable CA1822 // Mark members as static
/// <summary>
///     In this benchmark we check the case of an interface mock with a property, setup the property and verify
///     the getter was called exactly <see cref="N" /> times.
/// </summary>
public class CompletePropertyBenchmarks : BenchmarksBase
{
	[Params(1, 10)]
	public int N { get; set; }

	/// <summary>
	///     <see href="https://awexpect.com/Mockolate" />
	/// </summary>
	[Benchmark(Baseline = true)]
	public void Property_Mockolate()
	{
		IMyPropertyInterface sut = IMyPropertyInterface.CreateMock();
		sut.Mock.Setup.Counter.InitializeWith(42);

		for (int i = 0; i < N; i++)
		{
			_ = sut.Counter;
		}

		sut.Mock.Verify.Counter.Got().Exactly(N);
	}

	/// <summary>
	///     <see href="https://github.com/devlooped/moq" />
	/// </summary>
	[Benchmark]
	public void Property_Moq()
	{
		Moq.Mock<IMyPropertyInterface> mock = new();
		mock.SetupGet(x => x.Counter).Returns(42);
		IMyPropertyInterface sut = mock.Object;

		for (int i = 0; i < N; i++)
		{
			_ = sut.Counter;
		}

		mock.VerifyGet(x => x.Counter, Times.Exactly(N));
	}

	/// <summary>
	///     <see href="https://nsubstitute.github.io/" />
	/// </summary>
	[Benchmark]
	public void Property_NSubstitute()
	{
		IMyPropertyInterface mock = Substitute.For<IMyPropertyInterface>();
		mock.Counter.Returns(42);

		for (int i = 0; i < N; i++)
		{
			_ = mock.Counter;
		}

		_ = mock.Received(N).Counter;
	}

	/// <summary>
	///     <see href="https://fakeiteasy.github.io/" />
	/// </summary>
	[Benchmark]
	public void Property_FakeItEasy()
	{
		IMyPropertyInterface mock = A.Fake<IMyPropertyInterface>();
		A.CallTo(() => mock.Counter).Returns(42);

		for (int i = 0; i < N; i++)
		{
			_ = mock.Counter;
		}

		A.CallTo(() => mock.Counter).MustHaveHappened(N, FakeItEasy.Times.Exactly);
	}

	/// <summary>
	///     <see href="https://github.com/themidnightgospel/Imposter" />
	/// </summary>
	[Benchmark]
	public void Property_Imposter()
	{
		IMyPropertyInterfaceImposter imposter = IMyPropertyInterface.Imposter();
		imposter.Counter.Getter().Returns(42);
		IMyPropertyInterface sut = imposter.Instance();

		for (int i = 0; i < N; i++)
		{
			_ = sut.Counter;
		}

		imposter.Counter.Getter().Called(Count.Exactly(N));
	}

	/// <summary>
	///     <see href="https://github.com/thomhurst/TUnit/" />
	/// </summary>
	[Benchmark]
	public void Property_TUnitMocks()
	{
		Mock<IMyPropertyInterface> mock = TUnit.Mocks.Mock.Of<IMyPropertyInterface>();
		mock.Counter.Returns(42);
		IMyPropertyInterface sut = mock.Object;

		for (int i = 0; i < N; i++)
		{
			_ = sut.Counter;
		}

		mock.Counter.WasCalled(TUnit.Mocks.Times.Exactly(N));
	}

	public interface IMyPropertyInterface
	{
		int Counter { get; set; }
	}
}
#pragma warning restore CA1822 // Mark members as static
