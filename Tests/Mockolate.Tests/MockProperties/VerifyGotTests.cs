using Mockolate.Exceptions;

namespace Mockolate.Tests.MockProperties;

public sealed class VerifyGotTests
{
	[Fact]
	public async Task ShouldIncreaseInvocationCountOfGetter()
	{
		MockTests.IMyService sut = Mock.Create<MockTests.IMyService>();

		_ = sut.Counter;

		await That(sut.VerifyMock.Got.Counter()).Once();
		await That(sut.VerifyMock.Set.Counter(It.IsAny<int>())).Never();
	}

	[Fact]
	public async Task ShouldReturnInitializedValue()
	{
		MockTests.IMyService sut = Mock.Create<MockTests.IMyService>();
		sut.SetupMock.Property.Counter.InitializeWith(24);

		int result = sut.Counter;

		await That(result).IsEqualTo(24);
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public async Task ShouldThrowMockNotSetupExceptionWhenBehaviorIsSetToThrow(bool throwWhenNotSetup)
	{
		MockTests.IMyService sut = Mock.Create<MockTests.IMyService>(MockBehavior.Default with
		{
			ThrowWhenNotSetup = throwWhenNotSetup,
		});

		void Act()
		{
			_ = sut.IsValid;
		}

		await That(Act).Throws<MockNotSetupException>().OnlyIf(throwWhenNotSetup)
			.WithMessage("""
			             The property 'Mockolate.Tests.MockTests.IMyService.IsValid' was accessed without prior setup.
			             """);
	}
}
