using Mockolate.Interactions;
using Mockolate.Tests.TestHelpers;
using Mockolate.Verify;

namespace Mockolate.Tests.Checks;

public sealed partial class MockGotIndexerTests
{
	public sealed class ProtectedTests
	{
		[Fact]
		public async Task ShouldForwardToInner()
		{
			MockInteractions mockInteractions = new();
			IMockInteractions interactions = mockInteractions;
			MyMock<int> mock = new(1);
			MockVerify<int, Mock<int>> verify = new(mockInteractions, mock);
			IMockGotIndexer<MockVerify<int, Mock<int>>> mockIndexer = new MockGotIndexer<int, Mock<int>>(verify);
			IMockGotIndexer<MockVerify<int, Mock<int>>> @protected = new MockGotIndexer<int, Mock<int>>.Protected(verify);
			interactions.RegisterInteraction(new IndexerGetterAccess(0, ["foo.bar"]));
			interactions.RegisterInteraction(new IndexerGetterAccess(1, ["foo.bar"]));

			var result1 = mockIndexer.Got(With.Any<string>());
			var result2 = @protected.Got(With.Any<string>());

			await That(result1).Exactly(2);
			await That(result2).Exactly(2);
		}
	}
}
