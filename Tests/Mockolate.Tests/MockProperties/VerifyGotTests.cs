using Mockolate.Exceptions;

namespace Mockolate.Tests.MockProperties;

public sealed class VerifyGotTests
{
	[Fact]
	public async Task ShouldIncreaseInvocationCountOfGetter()
	{
		Mock<MockTests.IMyService> sut = Mock.Create<MockTests.IMyService>();

		_ = sut.Subject.Counter;

		await That(sut.Verify.Got.Counter()).Once();
		await That(sut.Verify.Set.Counter(WithAny<int>())).Never();
	}

	[Fact]
	public async Task ShouldReturnInitializedValue()
	{
		Mock<MockTests.IMyService> sut = Mock.Create<MockTests.IMyService>();
		sut.Setup.Property.Counter.InitializeWith(24);

		int result = sut.Subject.Counter;

		await That(result).IsEqualTo(24);
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public async Task ShouldThrowMockNotSetupExceptionWhenBehaviorIsSetToThrow(bool throwWhenNotSetup)
	{
		Mock<MockTests.IMyService> sut = Mock.Create<MockTests.IMyService>(MockBehavior.Default with
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
}
