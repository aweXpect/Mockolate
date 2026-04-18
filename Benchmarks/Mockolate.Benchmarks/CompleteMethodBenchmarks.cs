using BenchmarkDotNet.Attributes;
using FakeItEasy;
using Imposter.Abstractions;
using Mockolate.Benchmarks;
using Mockolate.Verify;
using NSubstitute;
using Arg = NSubstitute.Arg;
using Times = Moq.Times;

[assembly: GenerateImposter(typeof(CompleteMethodBenchmarks.IMyMethodInterface))]

namespace Mockolate.Benchmarks;
#pragma warning disable CA1822 // Mark members as static
/// <summary>
///     In this benchmark we check the simple case of an interface mock, setup a single method that gets called and
///     verified to be called exactly <see cref="InvocationCount" /> times.
/// </summary>
public class CompleteMethodBenchmarks : BenchmarksBase
{
	[Params(1, 10)]
	public int InvocationCount { get; set; }

	/// <summary>
	///     <see href="https://awexpect.com/Mockolate" />
	/// </summary>
	[Benchmark(Baseline = true)]
	public void Method_Mockolate()
	{
		IMyMethodInterface sut = IMyMethodInterface.CreateMock();
		sut.Mock.Setup.MyFunc(It.IsAny<int>()).Returns(true);

		for (int i = 0; i < InvocationCount; i++)
		{
			sut.MyFunc(42);
		}

		sut.Mock.Verify.MyFunc(It.IsAny<int>()).Exactly(InvocationCount);
	}

	/// <summary>
	///     <see href="https://github.com/devlooped/moq" />
	/// </summary>
	[Benchmark]
	public void Method_Moq()
	{
		Moq.Mock<IMyMethodInterface> mock = new();
		mock.Setup(x => x.MyFunc(Moq.It.IsAny<int>())).Returns(true);

		for (int i = 0; i < InvocationCount; i++)
		{
			mock.Object.MyFunc(42);
		}

		mock.Verify(x => x.MyFunc(Moq.It.IsAny<int>()), Times.Exactly(InvocationCount));
	}

	/// <summary>
	///     <see href="https://nsubstitute.github.io/" />
	/// </summary>
	[Benchmark]
	public void Method_NSubstitute()
	{
		IMyMethodInterface mock = Substitute.For<IMyMethodInterface>();
		mock.MyFunc(Arg.Any<int>()).Returns(true);

		for (int i = 0; i < InvocationCount; i++)
		{
			mock.MyFunc(42);
		}

		mock.Received(InvocationCount).MyFunc(Arg.Any<int>());
	}

	/// <summary>
	///     <see href="https://fakeiteasy.github.io/" />
	/// </summary>
	[Benchmark]
	public void Method_FakeItEasy()
	{
		IMyMethodInterface mock = A.Fake<IMyMethodInterface>();
		A.CallTo(() => mock.MyFunc(A<int>.Ignored)).Returns(true);

		for (int i = 0; i < InvocationCount; i++)
		{
			mock.MyFunc(42);
		}

		A.CallTo(() => mock.MyFunc(A<int>.Ignored)).MustHaveHappened(InvocationCount, FakeItEasy.Times.Exactly);
	}

	/// <summary>
	///     <see href="https://github.com/themidnightgospel/Imposter" />
	/// </summary>
	[Benchmark]
	public void Method_Imposter()
	{
		IMyMethodInterfaceImposter imposter = IMyMethodInterface.Imposter();
		imposter.MyFunc(Imposter.Abstractions.Arg<int>.Any()).Returns(true);

		for (int i = 0; i < InvocationCount; i++)
		{
			imposter.Instance().MyFunc(42);
		}

		imposter.MyFunc(Imposter.Abstractions.Arg<int>.Any()).Called(Count.Exactly(InvocationCount));
	}

	/// <summary>
	///     <see href="https://github.com/thomhurst/TUnit/" />
	/// </summary>
	[Benchmark]
	public void Method_TUnitMocks()
	{
		Mock<IMyMethodInterface> mock = TUnit.Mocks.Mock.Of<IMyMethodInterface>();
		mock.MyFunc(Any<int>()).Returns(true);

		for (int i = 0; i < InvocationCount; i++)
		{
			mock.Object.MyFunc(42);
		}

		mock.MyFunc(Any<int>()).WasCalled(TUnit.Mocks.Times.Exactly(InvocationCount));
	}

	public interface IMyMethodInterface
	{
		bool MyFunc(int value);
	}
}
#pragma warning restore CA1822 // Mark members as static
