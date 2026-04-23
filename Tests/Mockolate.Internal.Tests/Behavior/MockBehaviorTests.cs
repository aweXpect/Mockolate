namespace Mockolate.Internal.Tests.Behavior;

public sealed class MockBehaviorTests
{
	public sealed class ToStringTests
	{
		[Fact]
		public async Task WithMultipleFlags_ShouldKeepAllPartsInOutput()
		{
			MockBehavior behavior = MockBehavior.Default
				.ThrowingWhenNotSetup()
				.SkippingBaseClass()
				.SkippingInteractionRecording();

			string result = behavior.ToString();

			await That(result).Contains("ThrowingWhenNotSetup");
			await That(result).Contains("SkippingBaseClass");
			await That(result).Contains("SkippingInteractionRecording");
		}
	}
}
