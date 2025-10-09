using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests;

public sealed partial class MockTests
{
	[Fact]
	public async Task Factory_ShouldUseDefinedBehavior()
	{
		MockBehavior behavior = MockBehavior.Default with
		{
			ThrowWhenNotSetup = true,
		};
		Mock.Factory factory = new(behavior);

		Mock<IMyService> mock1 = factory.Create<IMyService>();
		Mock<MyServiceBase, IMyService> mock2 = factory.Create<MyServiceBase, IMyService>();

		await That(((IMock)mock1).Behavior).IsSameAs(behavior);
		await That(((IMock)mock2).Behavior).IsSameAs(behavior);
	}
}
