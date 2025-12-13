namespace Mockolate.Tests;

public sealed class MockBehaviorExtensionsTests
{
	[Test]
	public async Task CallingBaseClass_ShouldSetCallBaseClass()
	{
		MockBehavior sut = MockBehavior.Default;

		MockBehavior result = sut.CallingBaseClass();

		await That(result.CallBaseClass).IsTrue();
	}

	[Test]
	public async Task CallingBaseClass_WithFalse_ShouldUpdateCallBaseClass()
	{
		bool initializedValue = true;
		MockBehavior sut = MockBehavior.Default.CallingBaseClass(initializedValue);

		MockBehavior result = sut.CallingBaseClass(false);

		await That(result.CallBaseClass).IsFalse();
	}

	[Test]
	public async Task ThrowingWhenNotSetup_ShouldSetThrowWhenNotSetup()
	{
		MockBehavior sut = MockBehavior.Default;

		MockBehavior result = sut.ThrowingWhenNotSetup();

		await That(result.ThrowWhenNotSetup).IsTrue();
	}

	[Test]
	public async Task ThrowingWhenNotSetup_WithFalse_ShouldUpdateThrowWhenNotSetup()
	{
		bool initializedValue = true;
		MockBehavior sut = MockBehavior.Default.ThrowingWhenNotSetup(initializedValue);

		MockBehavior result = sut.ThrowingWhenNotSetup(false);

		await That(result.ThrowWhenNotSetup).IsFalse();
	}
}
