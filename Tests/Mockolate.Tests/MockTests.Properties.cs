using Mockolate.Checks;
using Mockolate.Exceptions;
using Mockolate.Setup;
using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests;

public sealed partial class MockTests
{
	[Fact]
	public async Task Get_ShouldIncreaseInvocationCountOfGetter()
	{
		var sut = Mock.For<IMyService>();

		_ = sut.Object.Counter;

		await That(sut.Accessed.Counter.Getter().Once());
		await That(sut.Accessed.Counter.Setter(With.Any<int>()).Never());
	}

	[Fact]
	public async Task Get_ShouldReturnInitializedValue()
	{
		var sut = Mock.For<IMyService>();
		sut.Setup.Counter.InitializeWith(24);

		var result = sut.Object.Counter;

		await That(result).IsEqualTo(24);
	}

	[Fact]
	public async Task Set_ShouldIncreaseInvocationCountOfGetter()
	{
		var sut = Mock.For<IMyService>();

		sut.Object.Counter = 42;

		await That(sut.Accessed.Counter.Getter().Never());
		await That(sut.Accessed.Counter.Setter(With.Any<int>()).Once());
	}

	[Fact]
	public async Task Set_ShouldUpdateValueForNextGet()
	{
		var sut = Mock.For<IMyService>();

		var result1 = sut.Object.Counter;
		sut.Object.Counter = 5;
		var result2 = sut.Object.Counter;

		await That(result1).IsEqualTo(0);
		await That(result2).IsEqualTo(5);
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public async Task Get_ShouldThrowMockNotSetupExceptionWhenBehaviorIsSetToThrow(bool throwWhenNotSetup)
	{
		var sut = Mock.For<IMyService>(MockBehavior.Default with
		{
			ThrowWhenNotSetup = throwWhenNotSetup
		});

		void Act()
			=> _ = sut.Object.IsValid;

		await That(Act).Throws<MockNotSetupException>().OnlyIf(throwWhenNotSetup)
			.WithMessage("""
			             The property 'Mockolate.Tests.MockTests.IMyService.IsValid' was accessed without prior setup.
			             """);
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public async Task Set_ShouldThrowMockNotSetupExceptionWhenBehaviorIsSetToThrow(bool throwWhenNotSetup)
	{
		var sut = Mock.For<IMyService>(MockBehavior.Default with
		{
			ThrowWhenNotSetup = throwWhenNotSetup
		});

		void Act()
			=> sut.Object.IsValid = true;

		await That(Act).Throws<MockNotSetupException>().OnlyIf(throwWhenNotSetup)
			.WithMessage("""
			             The property 'Mockolate.Tests.MockTests.IMyService.IsValid' was accessed without prior setup.
			             """);
	}

	[Fact]
	public async Task Set_WithNull_ShouldUpdateValueForNextGet()
	{
		var sut = Mock.For<IMyService>();
		sut.Setup.IsValid.InitializeWith(true);

		var result1 = sut.Object.IsValid;
		sut.Object.IsValid = null;
		var result2 = sut.Object.IsValid;

		await That(result1).IsEqualTo(true);
		await That(result2).IsEqualTo(null);
	}
}
