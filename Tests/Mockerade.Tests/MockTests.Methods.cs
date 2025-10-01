using Mockerade.Exceptions;

namespace Mockerade.Tests;

public sealed partial class MockTests
{
	[Theory]
	[InlineData(2)]
	[InlineData(42)]
	public async Task Execute_MethodWithReturnValue_ShouldBeRegistered(int numberOfInvocations)
	{
		var sut = Mock.For<IMyService>();
		sut.Setup.Double(With.Any<int>()).Returns(1);

		for (int i = 0; i < numberOfInvocations; i++)
		{
			sut.Object.Double(i);
		}

		await That(sut.Invoked.Double(With.Any<int>()).Exactly(numberOfInvocations));
	}

	[Fact]
	public async Task Execute_WhenNotSetup_ShouldReturnDefaultValue()
	{
		var sut = Mock.For<IMyService>();

		var result = sut.Object.Double(3);

		await That(result).IsEqualTo(default(int));
	}

	[Fact]
	public async Task Execute_WhenBehaviorIsSetToThrow_ShouldThrowMockNotSetupException()
	{
		var sut = Mock.For<IMyService>(MockBehavior.Default with
		{
			ThrowWhenNotSetup = true
		});

		void Act()
			=> sut.Object.Double(3);

		await That(Act).Throws<MockNotSetupException>()
			.WithMessage("""
			             The method 'Mockerade.Tests.MockTests.IMyService.Double(System.Int32)' was invoked without prior setup.
			             """);
	}

	[Theory]
	[InlineData(2)]
	[InlineData(42)]
	public async Task Execute_VoidMethod_ShouldBeRegistered(int numberOfInvocations)
	{
		var sut = Mock.For<IMyService>();
		sut.Setup.SetIsValid(With.Any<bool>());

		for (int i = 0; i < numberOfInvocations; i++)
		{
			sut.Object.SetIsValid(i % 2 == 0);
		}

		await That(sut.Invoked.SetIsValid(With.Any<bool>()).Exactly(numberOfInvocations));
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public async Task Execute_VoidMethod_ShouldThrowMockNotSetupExceptionWhenBehaviorIsSetToThrow(bool throwWhenNotSetup)
	{
		var sut = Mock.For<IMyService>(MockBehavior.Default with
		{
			ThrowWhenNotSetup = throwWhenNotSetup
		});

		void Act()
			=> sut.Object.SetIsValid(true);

		await That(Act).Throws<MockNotSetupException>().OnlyIf(throwWhenNotSetup)
			.WithMessage("""
			             The method 'Mockerade.Tests.MockTests.IMyService.SetIsValid(System.Boolean)' was invoked without prior setup.
			             """);
	}
}
