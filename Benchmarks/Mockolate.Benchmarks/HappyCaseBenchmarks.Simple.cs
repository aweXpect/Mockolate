using BenchmarkDotNet.Attributes;
using FakeItEasy;
using Imposter.Abstractions;
using Mockolate.Verify;
using NSubstitute;
using Arg = NSubstitute.Arg;
using Times = Moq.Times;

[assembly: GenerateImposter(typeof(Mockolate.Benchmarks.HappyCaseBenchmarks.IMyInterface))]

namespace Mockolate.Benchmarks;
#pragma warning disable CA1822 // Mark members as static
/// <summary>
///     In this benchmark we check the simple case of an interface mock, setup a single method that gets called and
///     verified to be called once.<br />
/// </summary>
public partial class HappyCaseBenchmarks
{
	/// <summary>
	///     <see href="https://awexpect.com/Mockolate" />
	/// </summary>
	[Benchmark]
	public void Simple_Mockolate()
	{
		IMyInterface mock = IMyInterface.CreateMock();
		mock.Mock.Setup.MyFunc(It.IsAny<int>()).Returns(true);

		mock.MyFunc(42);

		mock.Mock.Verify.MyFunc(It.IsAny<int>()).Once();
	}

	/// <summary>
	///     <see href="https://github.com/devlooped/moq" />
	/// </summary>
	[Benchmark]
	public void Simple_Moq()
	{
		Moq.Mock<IMyInterface> mock = new();
		mock.Setup(x => x.MyFunc(Moq.It.IsAny<int>())).Returns(true);

		mock.Object.MyFunc(42);

		mock.Verify(x => x.MyFunc(Moq.It.IsAny<int>()), Times.Once());
	}

	/// <summary>
	///     <see href="https://nsubstitute.github.io/" />
	/// </summary>
	[Benchmark]
	public void Simple_NSubstitute()
	{
		IMyInterface mock = Substitute.For<IMyInterface>();
		mock.MyFunc(Arg.Any<int>()).Returns(true);

		mock.MyFunc(42);

		mock.Received(1).MyFunc(Arg.Any<int>());
	}

	/// <summary>
	///     <see href="https://fakeiteasy.github.io/" />
	/// </summary>
	[Benchmark]
	public void Simple_FakeItEasy()
	{
		IMyInterface mock = A.Fake<IMyInterface>();
		A.CallTo(() => mock.MyFunc(A<int>.Ignored)).Returns(true);

		mock.MyFunc(42);

		A.CallTo(() => mock.MyFunc(A<int>.Ignored)).MustHaveHappened(1, FakeItEasy.Times.Exactly);
	}

	/// <summary>
	///     <see href="https://github.com/themidnightgospel/Imposter" />
	/// </summary>
	[Benchmark]
	public void Simple_Imposter()
	{
		IMyInterfaceImposter imposter = IMyInterface.Imposter();
		imposter.MyFunc(Imposter.Abstractions.Arg<int>.Any()).Returns(true);

		imposter.Instance().MyFunc(42);

		imposter.MyFunc(Imposter.Abstractions.Arg<int>.Any()).Called(Count.Once());
	}

	/// <summary>
	///     <see href="https://github.com/thomhurst/TUnit/" />
	/// </summary>
	[Benchmark]
	public void Simple_TUnitMocks()
	{
		TUnit.Mocks.Mock<IMyInterface> mock = TUnit.Mocks.Mock.Of<IMyInterface>();
		mock.MyFunc(Any<int>()).Returns(true);

		mock.Object.MyFunc(42);

		mock.MyFunc(Any<int>()).WasCalled();
	}

	public interface IMyInterface
	{
		bool MyFunc(int value);
	}
}
#pragma warning restore CA1822 // Mark members as static
