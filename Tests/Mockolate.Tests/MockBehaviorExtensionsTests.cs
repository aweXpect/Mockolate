namespace Mockolate.Tests;

public sealed class MockBehaviorExtensionsTests
{
	[Fact]
	public async Task CallingBaseClass_ShouldSetCallBaseClass()
	{
		MockBehavior sut = MockBehavior.Default;

		MockBehavior result = sut.CallingBaseClass();

		await That(result.CallBaseClass).IsTrue();
	}

	[Fact]
	public async Task IgnoringBaseClass_ShouldSetIgnoreBaseClass()
	{
		bool initializedValue = true;
		MockBehavior sut = MockBehavior.Default.CallingBaseClass(initializedValue);

		MockBehavior result = sut.CallingBaseClass(false);

		await That(result.CallBaseClass).IsFalse();
	}
}
