using Mockolate.Exceptions;
using static Mockolate.Tests.MockMethods.SetupMethodTests;

namespace Mockolate.Tests.MockMethods;

public sealed partial class VerifyInvokedTests
{
	[Test]
	public async Task Equals_ShouldWork()
	{
		object obj = new();
		IMethodService mock = Mock.Create<IMethodService>();

		_ = mock.Equals(obj);

		await That(mock.VerifyMock.Invoked.Equals(It.Is(obj))).Once();
	}

	[Test]
	public async Task GetHashCode_ShouldWork()
	{
		IMethodService mock = Mock.Create<IMethodService>();

		_ = mock.GetHashCode();

		await That(mock.VerifyMock.Invoked.GetHashCode()).Once();
	}

	[Test]
	[Arguments(2)]
	[Arguments(42)]
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

	[Test]
	public async Task ToString_ShouldWork()
	{
		IMethodService mock = Mock.Create<IMethodService>();

		_ = mock.ToString();

		await That(mock.VerifyMock.Invoked.ToString()).Once();
	}

	[Test]
	[Arguments(2)]
	[Arguments(42)]
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

	[Test]
	[Arguments(true)]
	[Arguments(false)]
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

	[Test]
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

	[Test]
	public async Task WhenNotSetup_ShouldReturnDefaultValue()
	{
		MockTests.IMyService sut = Mock.Create<MockTests.IMyService>();

		int result = sut.Multiply(3, 4);

		await That(result).IsEqualTo(default(int));
	}
}
