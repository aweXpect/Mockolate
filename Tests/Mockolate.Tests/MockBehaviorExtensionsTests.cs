namespace Mockolate.Tests;

public sealed class MockBehaviorExtensionsTests
{
	[Fact]
	public async Task CallingBaseClass_ShouldSetCallBaseClass()
	{
		MockBehavior sut = MockBehavior.Default;

		MockBehavior result = sut.CallingBaseClass();

		await That(result.BaseClassBehavior).IsEqualTo(BaseClassBehavior.CallBaseClass);
	}

	[Fact]
	public async Task IgnoringBaseClass_ShouldSetIgnoreBaseClass()
	{
		MockBehavior sut = MockBehavior.Default.CallingBaseClass();

		MockBehavior result = sut.IgnoringBaseClass();

		await That(result.BaseClassBehavior).IsEqualTo(BaseClassBehavior.IgnoreBaseClass);
	}
}
