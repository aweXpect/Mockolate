using aweXpect;
using BenchmarkDotNet.Attributes;
using FakeItEasy;
using NSubstitute;

namespace Mockolate.Benchmarks;

/// <summary>
///     In this benchmark we check the simple case of an interface mock, setup a single method that gets called and verified to be called once.<br />
/// </summary>
public partial class HappyCaseBenchmarks
{
	/// <summary>
	///     <see href="https://github.com/Mockolate/Mockolate"/>
	/// </summary>
	[Benchmark]
	public async Task Simple_Mockolate()
	{
		var mock = Mock.For<IMyInterface>();
		mock.Setup.MyFunc(With.Any<int>()).Returns(true);

		mock.Object.MyFunc(42);

		await Expect.That(mock.Invoked.MyFunc(With.Any<int>()).Once());
	}

	/// <summary>
	///     <see href="https://github.com/devlooped/moq"/>
	/// </summary>
	[Benchmark]
	public void Simple_Moq()
	{
		var mock = new Moq.Mock<IMyInterface>();
		mock.Setup(x => x.MyFunc(Moq.It.IsAny<int>())).Returns(true);

		mock.Object.MyFunc(42);

		mock.Verify(x => x.MyFunc(Moq.It.IsAny<int>()), Moq.Times.Once());
	}

	/// <summary>
	///     <see href="https://nsubstitute.github.io/"/>
	/// </summary>
	[Benchmark]
	public void Simple_NSubstitute()
	{
		var mock = Substitute.For<IMyInterface>();
		mock.MyFunc(Arg.Any<int>()).Returns(true);

		mock.MyFunc(42);

		mock.Received(1).MyFunc(Arg.Any<int>());
	}

	/// <summary>
	///     <see href="https://fakeiteasy.github.io/"/>
	/// </summary>
	[Benchmark]
	public void Simple_FakeItEasy()
	{
		var mock = A.Fake<IMyInterface>();
		A.CallTo(() => mock.MyFunc(A<int>.Ignored)).Returns(true);

		mock.MyFunc(42);

		A.CallTo(() => mock.MyFunc(A<int>.Ignored)).MustHaveHappened(1, Times.Exactly);
	}

	public interface IMyInterface
	{
		bool MyFunc(int value);
	}
}
