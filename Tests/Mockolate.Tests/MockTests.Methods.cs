using Mockolate.Exceptions;

namespace Mockolate.Tests;

public sealed partial class MockTests
{
	[Theory]
	[InlineData(2)]
	[InlineData(42)]
	public async Task Execute_MethodWithReturnValue_ShouldBeRegistered(int numberOfInvocations)
	{
		Mock<IMyService> sut = Mock.Create<IMyService>();
		sut.Setup.Method.Multiply(With.Any<int>(), With.Any<int?>()).Returns(1);

		for (int i = 0; i < numberOfInvocations; i++)
		{
			sut.Subject.Multiply(i, 4);
		}

		await That(sut.Verify.Invoked.Multiply(With.Any<int>(), With.Any<int?>())).Exactly(numberOfInvocations);
	}

	[Theory]
	[InlineData(2)]
	[InlineData(42)]
	public async Task Execute_VoidMethod_ShouldBeRegistered(int numberOfInvocations)
	{
		Mock<IMyService> sut = Mock.Create<IMyService>();
		sut.Setup.Method.SetIsValid(With.Any<bool>(), With.Any<Func<bool>?>());

		for (int i = 0; i < numberOfInvocations; i++)
		{
			sut.Subject.SetIsValid(i % 2 == 0, () => true);
		}

		await That(sut.Verify.Invoked.SetIsValid(With.Any<bool>(), With.Any<Func<bool>?>())).Exactly(numberOfInvocations);
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public async Task Execute_VoidMethod_ShouldThrowMockNotSetupExceptionWhenBehaviorIsSetToThrow(
		bool throwWhenNotSetup)
	{
		Mock<IMyService> sut = Mock.Create<IMyService>(MockBehavior.Default with
		{
			ThrowWhenNotSetup = throwWhenNotSetup,
		});

		void Act()
			=> sut.Subject.SetIsValid(true, null);

		await That(Act).Throws<MockNotSetupException>().OnlyIf(throwWhenNotSetup)
			.WithMessage("""
			             The method 'Mockolate.Tests.MockTests.IMyService.SetIsValid(System.Boolean, <null>)' was invoked without prior setup.
			             """);
	}

	[Fact]
	public async Task Execute_WhenBehaviorIsSetToThrow_ShouldThrowMockNotSetupException()
	{
		Mock<IMyService> sut = Mock.Create<IMyService>(MockBehavior.Default with
		{
			ThrowWhenNotSetup = true,
		});

		void Act()
			=> sut.Subject.Multiply(3, null);

		await That(Act).Throws<MockNotSetupException>()
			.WithMessage("""
			             The method 'Mockolate.Tests.MockTests.IMyService.Multiply(System.Int32, <null>)' was invoked without prior setup.
			             """);
	}

	[Fact]
	public async Task Execute_WhenNotSetup_ShouldReturnDefaultValue()
	{
		Mock<IMyService> sut = Mock.Create<IMyService>();

		int result = sut.Subject.Multiply(3, 4);

		await That(result).IsEqualTo(default(int));
	}
}
