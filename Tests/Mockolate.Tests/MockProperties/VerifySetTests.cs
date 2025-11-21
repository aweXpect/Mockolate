using Mockolate.Exceptions;

namespace Mockolate.Tests.MockProperties;

public sealed class VerifySetTests
{
	[Test]
	public async Task ShouldIncreaseInvocationCountOfGetter()
	{
		MockTests.IMyService sut = Mock.Create<MockTests.IMyService>();

		sut.Counter = 42;

		await That(sut.VerifyMock.Got.Counter()).Never();
		await That(sut.VerifyMock.Set.Counter(It.IsAny<int>())).Once();
	}

	[Test]
	[Arguments(true)]
	[Arguments(false)]
	public async Task ShouldThrowMockNotSetupExceptionWhenBehaviorIsSetToThrow(bool throwWhenNotSetup)
	{
		MockTests.IMyService sut = Mock.Create<MockTests.IMyService>(MockBehavior.Default with
		{
			ThrowWhenNotSetup = throwWhenNotSetup,
		});

		void Act()
		{
			sut.IsValid = true;
		}

		await That(Act).Throws<MockNotSetupException>().OnlyIf(throwWhenNotSetup)
			.WithMessage("""
			             The property 'Mockolate.Tests.MockTests.IMyService.IsValid' was accessed without prior setup.
			             """);
	}

	[Test]
	public async Task ShouldUpdateValueForNextGet()
	{
		MockTests.IMyService sut = Mock.Create<MockTests.IMyService>();

		int result1 = sut.Counter;
		sut.Counter = 5;
		int result2 = sut.Counter;

		await That(result1).IsEqualTo(0);
		await That(result2).IsEqualTo(5);
	}

	[Test]
	public async Task WithNull_ShouldUpdateValueForNextGet()
	{
		MockTests.IMyService sut = Mock.Create<MockTests.IMyService>();
		sut.SetupMock.Property.IsValid.InitializeWith(true);

		bool? result1 = sut.IsValid;
		sut.IsValid = null;
		bool? result2 = sut.IsValid;

		await That(result1).IsEqualTo(true);
		await That(result2).IsEqualTo(null);
	}
}
