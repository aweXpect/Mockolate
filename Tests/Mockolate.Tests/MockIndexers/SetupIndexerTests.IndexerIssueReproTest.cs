using Mockolate.Tests.TestHelpers;
using static Mockolate.Match;

namespace Mockolate.Tests.MockIndexers;

public sealed partial class SetupIndexerTests
{
	public sealed class IndexerIssueReproTest
	{
		[Fact]
		public async Task IndexerReturns_WithSpecificParameter_ShouldIterateThroughValues()
		{
			IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
			mock.SetupMock.Indexer(With("Dark"))
				.Returns(1)
				.Returns(2);

			int resultDark1 = mock["Dark"];
			int resultLight1 = mock["Light"];
			int resultDark2 = mock["Dark"];
			int resultDark3 = mock["Dark"];

			await That(resultDark1).IsEqualTo(1);
			await That(resultLight1).IsEqualTo(0);
			await That(resultDark2).IsEqualTo(2);
			await That(resultDark3).IsEqualTo(1);
		}
	}
}
