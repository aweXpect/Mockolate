using Mockolate.Internal.Tests.TestHelpers;
using Mockolate.Setup;

namespace Mockolate.Internal.Tests;

public partial class MockSetupsTests
{
	public class IndexerSetupsTests
	{
		[Fact]
		public async Task AddAndGetLatestOrDefault_ShouldReturnLatestMatching()
		{
			MockSetups.IndexerSetups setups = new();
			FakeIndexerSetup setup1 = new(true);
			FakeIndexerSetup setup2 = new(false);
			FakeIndexerAccess access = new();
			setups.Add(setup1);
			setups.Add(setup2);

			FakeIndexerSetup? result = setups.GetMatching<FakeIndexerSetup>(
				s => ((IInteractiveIndexerSetup)s).Matches(access));

			await That(result).IsEqualTo(setup1);
		}

		[Fact]
		public async Task Stress_ShouldMaintainCountAfterManyAdds()
		{
			MockSetups.IndexerSetups setups = new();

			for (int r = 0; r < 10; r++)
			{
				Parallel.For(0, 100, _ => setups.Add(new FakeIndexerSetup(false)));
			}

			await That(setups.Count).IsEqualTo(1000);
		}

		[Fact]
		public async Task ThreadSafety_ConcurrentAddsAndQueries_ShouldReturnConsistentMatches()
		{
			MockSetups.IndexerSetups setups = new();
			FakeIndexerSetup initialMatch = new(true);
			setups.Add(initialMatch);
			Parallel.For(0, 200, i =>
			{
				bool shouldMatch = i % 2 == 0;
				FakeIndexerSetup setup = new(shouldMatch);
				setups.Add(setup);
				FakeIndexerAccess access = new();
				_ = setups.GetMatching<FakeIndexerSetup>(
					s => ((IInteractiveIndexerSetup)s).Matches(access));
			});
			FakeIndexerSetup finalMatch = new(true);
			setups.Add(finalMatch);
			FakeIndexerAccess finalAccess = new();

			FakeIndexerSetup? result = setups.GetMatching<FakeIndexerSetup>(
				s => ((IInteractiveIndexerSetup)s).Matches(finalAccess));

			await That(result).IsEqualTo(finalMatch);
		}

		[Fact]
		public async Task ThreadSafety_ShouldHandleParallelAdds()
		{
			MockSetups.IndexerSetups setups = new();

			Parallel.For(0, 200, _ => setups.Add(new FakeIndexerSetup(false)));

			await That(setups.Count).IsEqualTo(200);
		}
	}
}
