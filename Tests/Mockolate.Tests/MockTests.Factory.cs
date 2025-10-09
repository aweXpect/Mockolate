using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests;

public sealed partial class MockTests
{
	[Fact]
	public async Task Factory_ShouldUseDefinedBehavior()
	{
		var behavior = MockBehavior.Default with { ThrowWhenNotSetup = true };
		var factory = new Mock.Factory(behavior);

		var mock1 = factory.Create<IMyService>();
		var mock2 = factory.Create<MyServiceBase, IMyService>();

		await That(((IMock)mock1).Behavior).IsSameAs(behavior);
		await That(((IMock)mock2).Behavior).IsSameAs(behavior);
	}
}
