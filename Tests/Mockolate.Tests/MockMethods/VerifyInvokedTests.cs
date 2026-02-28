using Mockolate.Exceptions;
using Mockolate.Setup;
using static Mockolate.Tests.MockMethods.SetupMethodTests;

namespace Mockolate.Tests.MockMethods;

public sealed partial class VerifyInvokedTests
{
	[Fact]
	public async Task Equals_ShouldWork()
	{
		object obj = new();
		IMethodService mock = Mock.Create<IMethodService>();

		_ = mock.Equals(obj);

		await That(mock.VerifyMock.Invoked.Equals(It.Is(obj))).Once();
	}

	[Fact]
	public async Task Equals_ShouldWorkWithNull()
	{
		object? obj = null;
		IMethodService mock = Mock.Create<IMethodService>();

		_ = mock.Equals(null);

		await That(mock.VerifyMock.Invoked.Equals(It.Is(obj))).Once();
	}

	[Fact]
	public async Task Equals_WithOtherOverload_ShouldWork()
	{
		object obj = new();
		IMethodService mock = Mock.Create<IMethodService>();

		_ = mock.Equals(3);

		await That(mock.VerifyMock.Invoked.Equals(It.Is(obj))).Never();
	}

	[Fact]
	public async Task GetHashCode_ShouldWork()
	{
		IMethodService mock = Mock.Create<IMethodService>();

		_ = mock.GetHashCode();

		await That(mock.VerifyMock.Invoked.GetHashCode()).Once();
	}

	[Theory]
	[InlineData(-1, 0)]
	[InlineData(1, 1)]
	public async Task InvokedSetup_ShouldVerifySameConditionAsSetup(int firstParameter, int expectedCallCount)
	{
		MockTests.IMyService sut = Mock.Create<MockTests.IMyService>();
		IMethodSetup setup = sut.SetupMock.Method.Subtract(
			It.Satisfies<int>(x => x > 0),
			It.IsAny<int?>()).Returns(1);

		sut.Subtract(firstParameter, 4);

		await That(sut.VerifyMock.InvokedSetup(setup)).Exactly(expectedCallCount);
	}

	[Fact]
	public async Task MethodWithDifferentName_ShouldNotMatch()
	{
		MockTests.IMyService sut = Mock.Create<MockTests.IMyService>();

		sut.Multiply(1, 4);
		sut.Multiply(2, 4);

		await That(sut.VerifyMock.Invoked.Subtract(It.IsAny<int>(), It.IsAny<int?>())).Never();
	}

	[Fact]
	public async Task MethodWithDifferentName_WithParameters_ShouldNotMatch()
	{
		MockTests.IMyService sut = Mock.Create<MockTests.IMyService>();

		sut.Subtract(1, 4);
		sut.Subtract(2, 4);

		await That(sut.VerifyMock.Invoked.Multiply(Match.AnyParameters())).Never();
	}

	[Fact]
	public async Task MethodWithDifferentOverload_ShouldNotMatch()
	{
		MockTests.IMyService sut = Mock.Create<MockTests.IMyService>();

		sut.Subtract(1, 4, false);
		sut.Subtract(2, 4, true);

		await That(sut.VerifyMock.Invoked.Subtract(It.IsAny<int>(), It.IsAny<int?>())).Never();
	}

	[Theory]
	[InlineData(2)]
	[InlineData(42)]
	public async Task MethodWithReturnValue_ShouldBeRegistered(int numberOfInvocations)
	{
		MockTests.IMyService sut = Mock.Create<MockTests.IMyService>();
		sut.SetupMock.Method.Multiply(It.IsAny<int>(), It.IsAny<int?>()).Returns(1);

		for (int i = 0; i < numberOfInvocations; i++)
		{
			sut.Multiply(i, 4);
		}

		await That(sut.VerifyMock.Invoked.Multiply(It.IsAny<int>(), It.IsAny<int?>())).Exactly(numberOfInvocations);
	}

	[Fact]
	public async Task ToString_ShouldWork()
	{
		IMethodService mock = Mock.Create<IMethodService>();

		_ = mock.ToString();

		await That(mock.VerifyMock.Invoked.ToString()).Once();
	}

	[Theory]
	[InlineData(2)]
	[InlineData(42)]
	public async Task VoidMethod_ShouldBeRegistered(int numberOfInvocations)
	{
		MockTests.IMyService sut = Mock.Create<MockTests.IMyService>();
		sut.SetupMock.Method.SetIsValid(It.IsAny<bool>(), It.IsAny<Func<bool>?>());

		for (int i = 0; i < numberOfInvocations; i++)
		{
			sut.SetIsValid(i % 2 == 0, () => true);
		}

		await That(sut.VerifyMock.Invoked.SetIsValid(It.IsAny<bool>(), It.IsAny<Func<bool>?>()))
			.Exactly(numberOfInvocations);
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public async Task VoidMethod_ShouldThrowMockNotSetupExceptionWhenBehaviorIsSetToThrow(
		bool throwWhenNotSetup)
	{
		MockTests.IMyService sut = Mock.Create<MockTests.IMyService>(MockBehavior.Default with
		{
			ThrowWhenNotSetup = throwWhenNotSetup,
		});

		void Act()
		{
			sut.SetIsValid(true, null);
		}

		await That(Act).Throws<MockNotSetupException>().OnlyIf(throwWhenNotSetup)
			.WithMessage("""
			             The method 'Mockolate.Tests.MockTests.IMyService.SetIsValid(bool, <null>)' was invoked without prior setup.
			             """);
	}

	[Fact]
	public async Task WhenBehaviorIsSetToThrow_ShouldThrowMockNotSetupException()
	{
		MockTests.IMyService sut = Mock.Create<MockTests.IMyService>(MockBehavior.Default with
		{
			ThrowWhenNotSetup = true,
		});

		void Act()
		{
			sut.Multiply(3, null);
		}

		await That(Act).Throws<MockNotSetupException>()
			.WithMessage("""
			             The method 'Mockolate.Tests.MockTests.IMyService.Multiply(int, <null>)' was invoked without prior setup.
			             """);
	}

	[Fact]
	public async Task WhenNotSetup_ShouldReturnDefaultValue()
	{
		MockTests.IMyService sut = Mock.Create<MockTests.IMyService>();

		int result = sut.Multiply(3, 4);

		await That(result).IsEqualTo(default(int));
	}
}
