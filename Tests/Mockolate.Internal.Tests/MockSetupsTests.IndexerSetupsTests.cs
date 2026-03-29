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

			IndexerSetup? result = setups.GetLatestOrDefault(access);

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
			FakeIndexerSetup matchSetup = new(true);
			setups.Add(matchSetup);
			Parallel.For(0, 200, i =>
			{
				bool shouldMatch = i % 2 == 0;
				FakeIndexerSetup setup = new(shouldMatch);
				setups.Add(setup);
				FakeIndexerAccess access = new();
				_ = setups.GetLatestOrDefault(access);
			});
			FakeIndexerSetup expected = GetExpectedMatchingSetup(setups);
			FakeIndexerAccess finalAccess = new();
			IndexerSetup? result = setups.GetLatestOrDefault(finalAccess);

			await That(result).IsSameAs(expected);

			static FakeIndexerSetup GetExpectedMatchingSetup(MockSetups.IndexerSetups setups)
			{
				IndexerSetup? latest = setups.GetLatestOrDefault(new FakeIndexerAccess());
				if (latest is FakeIndexerSetup { ShouldMatch: true, } castLatest)
				{
					return castLatest;
				}

				FakeIndexerSetup matching = new(true);
				setups.Add(matching);
				return matching;
			}
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
