using Mockolate.Exceptions;

namespace Mockolate.Tests.MockProperties;

public sealed class VerifySetTests
{
	[Fact]
	public async Task ShouldIncreaseInvocationCountOfGetter()
	{
		Mock<Tests.MockTests.IMyService> sut = Mock.Create<Tests.MockTests.IMyService>();

		sut.Subject.Counter = 42;

		await That(sut.Verify.Got.Counter()).Never();
		await That(sut.Verify.Set.Counter(With.Any<int>())).Once();
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public async Task ShouldThrowMockNotSetupExceptionWhenBehaviorIsSetToThrow(bool throwWhenNotSetup)
	{
		Mock<Tests.MockTests.IMyService> sut = Mock.Create<Tests.MockTests.IMyService>(MockBehavior.Default with
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
	public async Task ShouldUpdateValueForNextGet()
	{
		Mock<Tests.MockTests.IMyService> sut = Mock.Create<Tests.MockTests.IMyService>();

		int result1 = sut.Subject.Counter;
		sut.Subject.Counter = 5;
		int result2 = sut.Subject.Counter;

		await That(result1).IsEqualTo(0);
		await That(result2).IsEqualTo(5);
	}

	[Fact]
	public async Task WithNull_ShouldUpdateValueForNextGet()
	{
		Mock<Tests.MockTests.IMyService> sut = Mock.Create<Tests.MockTests.IMyService>();
		sut.Setup.Property.IsValid.InitializeWith(true);

		bool? result1 = sut.Subject.IsValid;
		sut.Subject.IsValid = null;
		bool? result2 = sut.Subject.IsValid;

		await That(result1).IsEqualTo(true);
		await That(result2).IsEqualTo(null);
	}
}
