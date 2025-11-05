using Mockolate.Exceptions;
using Mockolate.Verify;
using static Mockolate.Tests.MockMethods.SetupMethodTests;

namespace Mockolate.Tests.MockMethods;

public sealed class VerifyInvokedTests
{
	[Theory]
	[InlineData(2)]
	[InlineData(42)]
	public async Task MethodWithReturnValue_ShouldBeRegistered(int numberOfInvocations)
	{
		Mock<MockTests.IMyService> sut = Mock.Create<MockTests.IMyService>();
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
	public async Task VoidMethod_ShouldBeRegistered(int numberOfInvocations)
	{
		Mock<MockTests.IMyService> sut = Mock.Create<MockTests.IMyService>();
		sut.Setup.Method.SetIsValid(With.Any<bool>(), With.Any<Func<bool>?>());

		for (int i = 0; i < numberOfInvocations; i++)
		{
			sut.Subject.SetIsValid(i % 2 == 0, () => true);
		}

		await That(sut.Verify.Invoked.SetIsValid(With.Any<bool>(), With.Any<Func<bool>?>()))
			.Exactly(numberOfInvocations);
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public async Task VoidMethod_ShouldThrowMockNotSetupExceptionWhenBehaviorIsSetToThrow(
		bool throwWhenNotSetup)
	{
		Mock<MockTests.IMyService> sut = Mock.Create<MockTests.IMyService>(MockBehavior.Default with
		{
			ThrowWhenNotSetup = throwWhenNotSetup,
		});

		void Act()
			=> sut.Subject.SetIsValid(true, null);

		await That(Act).Throws<MockNotSetupException>().OnlyIf(throwWhenNotSetup)
			.WithMessage("""
			             The method 'Mockolate.Tests.MockTests.IMyService.SetIsValid(bool, <null>)' was invoked without prior setup.
			             """);
	}

	[Fact]
	public async Task WhenBehaviorIsSetToThrow_ShouldThrowMockNotSetupException()
	{
		Mock<MockTests.IMyService> sut = Mock.Create<MockTests.IMyService>(MockBehavior.Default with
		{
			ThrowWhenNotSetup = true,
		});

		void Act()
			=> sut.Subject.Multiply(3, null);

		await That(Act).Throws<MockNotSetupException>()
			.WithMessage("""
			             The method 'Mockolate.Tests.MockTests.IMyService.Multiply(int, <null>)' was invoked without prior setup.
			             """);
	}

	[Fact]
	public async Task WhenNotSetup_ShouldReturnDefaultValue()
	{
		Mock<MockTests.IMyService> sut = Mock.Create<MockTests.IMyService>();

		int result = sut.Subject.Multiply(3, 4);

		await That(result).IsEqualTo(default(int));
	}

	[Fact]
	public async Task ToString_ShouldWork()
	{
		var expectedResult = Guid.NewGuid().ToString();
		Mock<IMethodService> mock = Mock.Create<IMethodService>();

		var result = mock.Subject.ToString();

		await That(mock.Verify.Invoked.ToString()).Once();
	}

	[Fact]
	public async Task GetHashCode_ShouldWork()
	{
		int expectedResult = Guid.NewGuid().GetHashCode();
		Mock<IMethodService> mock = Mock.Create<IMethodService>();

		var result = mock.Subject.GetHashCode();

		//TODO
		//await That(mock.Verify.Invoked.GetHashCode()).Once();
	}

	[Fact]
	public async Task Equals_ShouldWork()
	{
		var obj = new object();
		Mock<IMethodService> mock = Mock.Create<IMethodService>();

		var result = mock.Subject.Equals(obj);

		//TODO
		//await That(mock.Verify.Invoked.Equals(obj)).Once();
	}
}
