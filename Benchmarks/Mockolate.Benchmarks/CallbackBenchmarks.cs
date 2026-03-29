using BenchmarkDotNet.Attributes;
using FakeItEasy;
using Imposter.Abstractions;
using Mockolate.Benchmarks;
using Mockolate.Verify;
using NSubstitute;
using Arg = NSubstitute.Arg;
using Times = Moq.Times;

[assembly: GenerateImposter(typeof(CallbackBenchmarks.IMyCallbackInterface))]

namespace Mockolate.Benchmarks;
#pragma warning disable CA1822 // Mark members as static
/// <summary>
///     In this benchmark we check the case of an interface mock with an event, subscribe to the event and verify
///     the subscription was recorded once.<br />
/// </summary>
public class CallbackBenchmarks : BenchmarksBase
{
	/// <summary>
	///     <see href="https://awexpect.com/Mockolate" />
	/// </summary>
	[Benchmark]
	public int Method_Mockolate()
	{
		int count = 0;
		IMyCallbackInterface sut = IMyCallbackInterface.CreateMock();
		sut.Mock.Setup.MyFunc(It.IsAny<int>()).Do(() => count++);

		sut.MyFunc(42);

		sut.Mock.Verify.MyFunc(It.IsAny<int>()).Once();
		return count;
	}

	/// <summary>
	///     <see href="https://github.com/devlooped/moq" />
	/// </summary>
	[Benchmark]
	public int Method_Moq()
	{
		int count = 0;
		Moq.Mock<IMyCallbackInterface> mock = new();
		mock.Setup(x => x.MyFunc(Moq.It.IsAny<int>())).Callback(() => count++);

		mock.Object.MyFunc(42);

		mock.Verify(x => x.MyFunc(Moq.It.IsAny<int>()), Times.Once());
		return count;
	}

	/// <summary>
	///     <see href="https://nsubstitute.github.io/" />
	/// </summary>
	[Benchmark]
	public int Method_NSubstitute()
	{
		int count = 0;
		IMyCallbackInterface mock = Substitute.For<IMyCallbackInterface>();
		mock.When(x => x.MyFunc(Arg.Any<int>())).Do(_ => count++);

		mock.MyFunc(42);

		mock.Received(1).MyFunc(Arg.Any<int>());
		return count;
	}

	/// <summary>
	///     <see href="https://fakeiteasy.github.io/" />
	/// </summary>
	[Benchmark]
	public int Method_FakeItEasy()
	{
		int count = 0;
		IMyCallbackInterface mock = A.Fake<IMyCallbackInterface>();
		A.CallTo(() => mock.MyFunc(A<int>.Ignored)).Invokes(() => count++);

		mock.MyFunc(42);

		A.CallTo(() => mock.MyFunc(A<int>.Ignored)).MustHaveHappened(1, FakeItEasy.Times.Exactly);
		return count;
	}

	/// <summary>
	///     <see href="https://github.com/themidnightgospel/Imposter" />
	/// </summary>
	[Benchmark]
	public int Method_Imposter()
	{
		int count = 0;
		IMyCallbackInterfaceImposter imposter = IMyCallbackInterface.Imposter();
		imposter.MyFunc(Imposter.Abstractions.Arg<int>.Any()).Callback(_ => count++);

		imposter.Instance().MyFunc(42);

		imposter.MyFunc(Imposter.Abstractions.Arg<int>.Any()).Called(Count.Once());
		return count;
	}

	/// <summary>
	///     <see href="https://github.com/thomhurst/TUnit/" />
	/// </summary>
	[Benchmark]
	public int Callback_TUnitMocks()
	{
		int count = 0;
		Mock<IMyCallbackInterface> mock = TUnit.Mocks.Mock.Of<IMyCallbackInterface>();
		mock.MyFunc(Any<int>())
			.Callback(() => count++);

		IMyCallbackInterface svc = mock.Object;
		svc.MyFunc(1);
		svc.MyFunc(2);
		return count;
	}

	public interface IMyCallbackInterface
	{
		void MyFunc(int value);
	}
}
#pragma warning restore CA1822 // Mark members as static
