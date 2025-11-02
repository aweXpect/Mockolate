using Mockolate.Interactions;
using Mockolate.Tests.TestHelpers;
using Mockolate.Verify;

namespace Mockolate.Tests.MockIndexers;

public sealed partial class MockSetIndexerTests
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
			MockSetIndexer<int, Mock<int>> inner = new(verify);
			IMockSetIndexer<MockVerify<int, Mock<int>>> mockIndexer = inner;
			IMockSetIndexer<MockVerify<int, Mock<int>>> @protected = new ProtectedMockSetIndexer<int, Mock<int>>(inner);
			interactions.RegisterInteraction(new IndexerSetterAccess(0, ["foo.bar",], 4));
			interactions.RegisterInteraction(new IndexerSetterAccess(1, ["foo.bar",], 4));

			VerificationResult<MockVerify<int, Mock<int>>> result1 =
				mockIndexer.Set(With.Any<int>(), With.Any<string>());
			VerificationResult<MockVerify<int, Mock<int>>>
				result2 = @protected.Set(With.Any<int>(), With.Any<string>());

			await That(result1).Exactly(2);
			await That(result2).Exactly(2);
		}
	}
}
