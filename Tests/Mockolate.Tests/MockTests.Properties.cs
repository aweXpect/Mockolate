using Mockolate.Exceptions;

namespace Mockolate.Tests;

public sealed partial class MockTests
{
	[Fact]
	public async Task Get_ShouldIncreaseInvocationCountOfGetter()
	{
		Mock<IMyService> sut = Mock.Create<IMyService>();

		_ = sut.Subject.Counter;

		await That(sut.Accessed.Counter.Getter()).Once();
		await That(sut.Accessed.Counter.Setter(With.Any<int>())).Never();
	}

	[Fact]
	public async Task Get_ShouldReturnInitializedValue()
	{
		Mock<IMyService> sut = Mock.Create<IMyService>();
		sut.Setup.Property.Counter.InitializeWith(24);

		int result = sut.Subject.Counter;

		await That(result).IsEqualTo(24);
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public async Task Get_ShouldThrowMockNotSetupExceptionWhenBehaviorIsSetToThrow(bool throwWhenNotSetup)
	{
		Mock<IMyService> sut = Mock.Create<IMyService>(MockBehavior.Default with
		{
			ThrowWhenNotSetup = throwWhenNotSetup,
		});

		void Act()
			=> _ = sut.Subject.IsValid;

		await That(Act).Throws<MockNotSetupException>().OnlyIf(throwWhenNotSetup)
			.WithMessage("""
			             The property 'Mockolate.Tests.MockTests.IMyService.IsValid' was accessed without prior setup.
			             """);
	}

	[Fact]
	public async Task Set_ShouldIncreaseInvocationCountOfGetter()
	{
		Mock<IMyService> sut = Mock.Create<IMyService>();

		sut.Subject.Counter = 42;

		await That(sut.Accessed.Counter.Getter()).Never();
		await That(sut.Accessed.Counter.Setter(With.Any<int>())).Once();
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public async Task Set_ShouldThrowMockNotSetupExceptionWhenBehaviorIsSetToThrow(bool throwWhenNotSetup)
	{
		Mock<IMyService> sut = Mock.Create<IMyService>(MockBehavior.Default with
		{
			ThrowWhenNotSetup = throwWhenNotSetup,
		});

		void Act()
			=> sut.Subject.IsValid = true;

		await That(Act).Throws<MockNotSetupException>().OnlyIf(throwWhenNotSetup)
			.WithMessage("""
			             The property 'Mockolate.Tests.MockTests.IMyService.IsValid' was accessed without prior setup.
			             """);
	}

	[Fact]
	public async Task Set_ShouldUpdateValueForNextGet()
	{
		Mock<IMyService> sut = Mock.Create<IMyService>();

		int result1 = sut.Subject.Counter;
		sut.Subject.Counter = 5;
		int result2 = sut.Subject.Counter;

		await That(result1).IsEqualTo(0);
		await That(result2).IsEqualTo(5);
	}

	[Fact]
	public async Task Set_WithNull_ShouldUpdateValueForNextGet()
	{
		Mock<IMyService> sut = Mock.Create<IMyService>();
		sut.Setup.Property.IsValid.InitializeWith(true);

		bool? result1 = sut.Subject.IsValid;
		sut.Subject.IsValid = null;
		bool? result2 = sut.Subject.IsValid;

		await That(result1).IsEqualTo(true);
		await That(result2).IsEqualTo(null);
	}
}
