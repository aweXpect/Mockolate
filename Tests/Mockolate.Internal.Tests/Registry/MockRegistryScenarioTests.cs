using System.Collections.Generic;
using System.Linq;
using Mockolate.Interactions;
using Mockolate.Internal.Tests.TestHelpers;
using Mockolate.Setup;

namespace Mockolate.Internal.Tests.Registry;

public sealed class MockRegistryScenarioTests
{
	[Fact]
	public async Task ApplyIndexerGetter_WithLazyDefault_AndNonNullSetup_DispatchesToSetup()
	{
		MockRegistry registry = new(MockBehavior.Default, new FastMockInteractions(0));
		FakeIndexerSetup setup = new(true);
		IndexerGetterAccess<int> access = new(1);

		int callCount = 0;
		int result = registry.ApplyIndexerGetter(access, setup, () => ++callCount, 0);

		// FakeIndexerSetup.GetResult<TResult>(access, behavior, defaultValueGenerator) returns the generator's value.
		await That(result).IsEqualTo(1);
		await That(callCount).IsEqualTo(1);
	}

	[Fact]
	public async Task GetIndexerSetup_ByAccess_WithActiveScenarioAndScopedMatching_ReturnsScoped()
	{
		MockRegistry registry = new(MockBehavior.Default, new FastMockInteractions(0));
		FakeIndexerSetup global = new(true);
		FakeIndexerSetup scoped = new(true);
		registry.Setup.Indexers.Add(global);
		registry.Setup.GetOrCreateScenario("a").Indexers.Add(scoped);

		registry.TransitionTo("a");

		IndexerSetup? result = registry.GetIndexerSetup<IndexerSetup>(new IndexerGetterAccess<int>(1));

		await That(result).IsSameAs(scoped);
	}

	[Fact]
	public async Task GetIndexerSetup_ByAccess_WithActiveScenarioButScopedDoesNotMatchAccess_FallsBackToGlobal()
	{
		MockRegistry registry = new(MockBehavior.Default, new FastMockInteractions(0));
		FakeIndexerSetup global = new(true);
		FakeIndexerSetup scopedNonMatching = new(false);
		registry.Setup.Indexers.Add(global);
		registry.Setup.GetOrCreateScenario("a").Indexers.Add(scopedNonMatching);

		registry.TransitionTo("a");

		IndexerSetup? result = registry.GetIndexerSetup<IndexerSetup>(new IndexerGetterAccess<int>(1));

		await That(result).IsSameAs(global);
	}

	[Fact]
	public async Task GetIndexerSetup_ByPredicate_WithActiveScenarioButPredicateRejectsScoped_FallsBackToGlobal()
	{
		MockRegistry registry = new(MockBehavior.Default, new FastMockInteractions(0));
		FakeIndexerSetup global = new(true);
		FakeIndexerSetup scoped = new(true);
		registry.Setup.Indexers.Add(global);
		registry.Setup.GetOrCreateScenario("a").Indexers.Add(scoped);

		registry.TransitionTo("a");

		IndexerSetup? result = registry.GetIndexerSetup<IndexerSetup>(s => ReferenceEquals(s, global));

		await That(result).IsSameAs(global);
	}

	[Fact]
	public async Task GetMethodSetups_WithActiveScenarioAndScopedSetup_YieldsScopedThenGlobal()
	{
		MockRegistry registry = new(MockBehavior.Default, new FastMockInteractions(0));
		FakeMethodSetup global = new();
		FakeMethodSetup scoped = new();
		registry.Setup.Methods.Add(global);
		registry.Setup.GetOrCreateScenario("a").Methods.Add(scoped);

		registry.TransitionTo("a");

		List<MethodSetup> setups = registry.GetMethodSetups<MethodSetup>("foo").ToList();

		await That(setups).HasCount(2);
		await That(setups[0]).IsSameAs(scoped);
		await That(setups[1]).IsSameAs(global);
	}

	[Fact]
	public async Task GetMethodSetups_WithActiveScenarioButNoScopedBucket_FallsBackToGlobal()
	{
		MockRegistry registry = new(MockBehavior.Default, new FastMockInteractions(0));
		FakeMethodSetup global = new();
		registry.Setup.Methods.Add(global);

		registry.TransitionTo("never-registered");

		List<MethodSetup> setups = registry.GetMethodSetups<MethodSetup>("foo").ToList();

		await That(setups).HasCount(1);
		await That(setups[0]).IsSameAs(global);
	}
}
