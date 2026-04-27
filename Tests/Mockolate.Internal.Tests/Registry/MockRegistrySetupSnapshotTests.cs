using Mockolate.Interactions;
using Mockolate.Internal.Tests.TestHelpers;
using Mockolate.Setup;

namespace Mockolate.Internal.Tests.Registry;

public sealed class MockRegistrySetupSnapshotTests
{
	public sealed class MethodTests
	{
		[Fact]
		public async Task SetupMethod_WithIncreasingMemberIds_GrowsTableLazily()
		{
			MockRegistry registry = new(MockBehavior.Default, new FastMockInteractions(0));
			FakeMethodSetup setup0 = new();
			FakeMethodSetup setup7 = new();
			FakeMethodSetup setup3 = new();

			registry.SetupMethod(0, setup0);
			registry.SetupMethod(7, setup7);
			registry.SetupMethod(3, setup3);

			MethodSetup[]? snapshot0 = registry.GetMethodSetupSnapshot(0);
			MethodSetup[]? snapshot7 = registry.GetMethodSetupSnapshot(7);
			MethodSetup[]? snapshot3 = registry.GetMethodSetupSnapshot(3);
			await That(snapshot0).IsNotNull();
			await That(snapshot0![0]).IsSameAs(setup0);
			await That(snapshot7).IsNotNull();
			await That(snapshot7![0]).IsSameAs(setup7);
			await That(snapshot3).IsNotNull();
			await That(snapshot3![0]).IsSameAs(setup3);
		}

		[Fact]
		public async Task SetupMethod_WithMemberId_PublishesToSnapshot()
		{
			MockRegistry registry = new(MockBehavior.Default, new FastMockInteractions(0));
			FakeMethodSetup setup = new();

			registry.SetupMethod(5, setup);

			MethodSetup[]? snapshot = registry.GetMethodSetupSnapshot(5);
			await That(snapshot).IsNotNull();
			await That(snapshot!.Length).IsEqualTo(1);
			await That(snapshot[0]).IsSameAs(setup);
		}

		[Fact]
		public async Task SetupMethod_WithMemberIdAndDefaultScenario_PublishesToSnapshot()
		{
			MockRegistry registry = new(MockBehavior.Default, new FastMockInteractions(0));
			FakeMethodSetup setup = new();

			registry.SetupMethod(5, "", setup);

			MethodSetup[]? snapshot = registry.GetMethodSetupSnapshot(5);
			await That(snapshot).IsNotNull();
			await That(snapshot!.Length).IsEqualTo(1);
			await That(snapshot[0]).IsSameAs(setup);
		}

		[Fact]
		public async Task SetupMethod_WithMemberIdAndNamedScenario_DoesNotPublishToSnapshot()
		{
			MockRegistry registry = new(MockBehavior.Default, new FastMockInteractions(0));
			FakeMethodSetup setup = new();

			registry.SetupMethod(5, "s1", setup);

			await That(registry.GetMethodSetupSnapshot(5)).IsNull();
		}

		[Fact]
		public async Task SetupMethod_WithMemberIdAndNamedScenario_PreservesScenarioBucket()
		{
			MockRegistry registry = new(MockBehavior.Default, new FastMockInteractions(0));
			FakeMethodSetup setup = new();

			registry.SetupMethod(4, "s1", setup);

			await That(registry.GetMethodSetupSnapshot(4)).IsNull();
			await That(registry.Setup.TryGetScenario("s1", out MockScenarioSetup? scoped)).IsTrue();
			await That(scoped!.Methods.Count).IsEqualTo(1);
		}

		[Fact]
		public async Task SetupMethod_WithSameMemberIdTwice_AppendsToBucket()
		{
			MockRegistry registry = new(MockBehavior.Default, new FastMockInteractions(0));
			FakeMethodSetup setupA = new();
			FakeMethodSetup setupB = new();

			registry.SetupMethod(2, setupA);
			registry.SetupMethod(2, setupB);

			MethodSetup[]? snapshot = registry.GetMethodSetupSnapshot(2);
			await That(snapshot).IsNotNull();
			await That(snapshot!.Length).IsEqualTo(2);
			await That(snapshot[0]).IsSameAs(setupA);
			await That(snapshot[1]).IsSameAs(setupB);
		}
	}

	public sealed class PropertyTests
	{
		[Fact]
		public async Task PublishPropertyToMemberIdBucket_WithDefaultThenUserSetup_RetainsUserSetup()
		{
			MockRegistry registry = new(MockBehavior.Default, new FastMockInteractions(0));
			PropertySetup defaultSetup = new PropertySetup.Default<int>("P1", 0);
			FakePropertySetup userSetup = new("P1");

			registry.SetupProperty(1, defaultSetup);
			registry.SetupProperty(1, userSetup);

			await That(registry.GetPropertySetupSnapshot(1)).IsSameAs(userSetup);
		}

		[Fact]
		public async Task PublishPropertyToMemberIdBucket_WithUserThenDefaultSetup_RetainsUserSetup()
		{
			MockRegistry registry = new(MockBehavior.Default, new FastMockInteractions(0));
			FakePropertySetup userSetup = new("P1");
			PropertySetup defaultSetup = new PropertySetup.Default<int>("P1", 0);

			registry.SetupProperty(1, userSetup);
			registry.SetupProperty(1, defaultSetup);

			await That(registry.GetPropertySetupSnapshot(1)).IsSameAs(userSetup);
		}

		[Fact]
		public async Task SetupProperty_WithIncreasingMemberIds_GrowsTableLazily()
		{
			MockRegistry registry = new(MockBehavior.Default, new FastMockInteractions(0));
			FakePropertySetup setup0 = new("P0");
			FakePropertySetup setup7 = new("P7");
			FakePropertySetup setup3 = new("P3");

			registry.SetupProperty(0, setup0);
			registry.SetupProperty(7, setup7);
			registry.SetupProperty(3, setup3);

			await That(registry.GetPropertySetupSnapshot(0)).IsSameAs(setup0);
			await That(registry.GetPropertySetupSnapshot(7)).IsSameAs(setup7);
			await That(registry.GetPropertySetupSnapshot(3)).IsSameAs(setup3);
		}

		[Fact]
		public async Task SetupProperty_WithMemberId_PublishesToSnapshot()
		{
			MockRegistry registry = new(MockBehavior.Default, new FastMockInteractions(0));
			FakePropertySetup setup = new("P5");

			registry.SetupProperty(5, setup);

			await That(registry.GetPropertySetupSnapshot(5)).IsSameAs(setup);
		}

		[Fact]
		public async Task SetupProperty_WithMemberIdAndDefaultScenario_PublishesToSnapshot()
		{
			MockRegistry registry = new(MockBehavior.Default, new FastMockInteractions(0));
			FakePropertySetup setup = new("P5");

			registry.SetupProperty(5, "", setup);

			await That(registry.GetPropertySetupSnapshot(5)).IsSameAs(setup);
		}

		[Fact]
		public async Task SetupProperty_WithMemberIdAndNamedScenario_DoesNotPublishToSnapshot()
		{
			MockRegistry registry = new(MockBehavior.Default, new FastMockInteractions(0));
			FakePropertySetup setup = new("P5");

			registry.SetupProperty(5, "s1", setup);

			await That(registry.GetPropertySetupSnapshot(5)).IsNull();
		}

		[Fact]
		public async Task SetupProperty_WithMemberIdAndNamedScenario_PreservesScenarioBucket()
		{
			MockRegistry registry = new(MockBehavior.Default, new FastMockInteractions(0));
			FakePropertySetup setup = new("P4");

			registry.SetupProperty(4, "s1", setup);

			await That(registry.GetPropertySetupSnapshot(4)).IsNull();
			await That(registry.Setup.TryGetScenario("s1", out MockScenarioSetup? scoped)).IsTrue();
			await That(scoped!.Properties.TryGetValue("P4", out PropertySetup? scopedSetup)).IsTrue();
			await That(scopedSetup).IsSameAs(setup);
		}

		[Fact]
		public async Task SetupProperty_WithSameMemberIdTwice_RetainsLatestUserSetup()
		{
			MockRegistry registry = new(MockBehavior.Default, new FastMockInteractions(0));
			FakePropertySetup setupA = new("P2");
			FakePropertySetup setupB = new("P2");

			registry.SetupProperty(2, setupA);
			registry.SetupProperty(2, setupB);

			await That(registry.GetPropertySetupSnapshot(2)).IsSameAs(setupB);
		}
	}

	public sealed class IndexerTests
	{
		[Fact]
		public async Task SetupIndexer_WithIncreasingMemberIds_GrowsTableLazily()
		{
			MockRegistry registry = new(MockBehavior.Default, new FastMockInteractions(0));
			FakeIndexerSetup setup0 = new(true);
			FakeIndexerSetup setup7 = new(true);
			FakeIndexerSetup setup3 = new(true);

			registry.SetupIndexer(0, setup0);
			registry.SetupIndexer(7, setup7);
			registry.SetupIndexer(3, setup3);

			IndexerSetup[]? snapshot0 = registry.GetIndexerSetupSnapshot(0);
			IndexerSetup[]? snapshot7 = registry.GetIndexerSetupSnapshot(7);
			IndexerSetup[]? snapshot3 = registry.GetIndexerSetupSnapshot(3);
			await That(snapshot0).IsNotNull();
			await That(snapshot0![0]).IsSameAs(setup0);
			await That(snapshot7).IsNotNull();
			await That(snapshot7![0]).IsSameAs(setup7);
			await That(snapshot3).IsNotNull();
			await That(snapshot3![0]).IsSameAs(setup3);
		}

		[Fact]
		public async Task SetupIndexer_WithMemberId_PublishesToSnapshot()
		{
			MockRegistry registry = new(MockBehavior.Default, new FastMockInteractions(0));
			FakeIndexerSetup setup = new(true);

			registry.SetupIndexer(5, setup);

			IndexerSetup[]? snapshot = registry.GetIndexerSetupSnapshot(5);
			await That(snapshot).IsNotNull();
			await That(snapshot!.Length).IsEqualTo(1);
			await That(snapshot[0]).IsSameAs(setup);
		}

		[Fact]
		public async Task SetupIndexer_WithMemberIdAndDefaultScenario_PublishesToSnapshot()
		{
			MockRegistry registry = new(MockBehavior.Default, new FastMockInteractions(0));
			FakeIndexerSetup setup = new(true);

			registry.SetupIndexer(5, "", setup);

			IndexerSetup[]? snapshot = registry.GetIndexerSetupSnapshot(5);
			await That(snapshot).IsNotNull();
			await That(snapshot!.Length).IsEqualTo(1);
			await That(snapshot[0]).IsSameAs(setup);
		}

		[Fact]
		public async Task SetupIndexer_WithMemberIdAndNamedScenario_DoesNotPublishToSnapshot()
		{
			MockRegistry registry = new(MockBehavior.Default, new FastMockInteractions(0));
			FakeIndexerSetup setup = new(true);

			registry.SetupIndexer(5, "s1", setup);

			await That(registry.GetIndexerSetupSnapshot(5)).IsNull();
		}

		[Fact]
		public async Task SetupIndexer_WithMemberIdAndNamedScenario_PreservesScenarioBucket()
		{
			MockRegistry registry = new(MockBehavior.Default, new FastMockInteractions(0));
			FakeIndexerSetup setup = new(true);

			registry.SetupIndexer(4, "s1", setup);

			await That(registry.GetIndexerSetupSnapshot(4)).IsNull();
			await That(registry.Setup.TryGetScenario("s1", out MockScenarioSetup? scoped)).IsTrue();
			await That(scoped!.Indexers.Count).IsEqualTo(1);
		}

		[Fact]
		public async Task SetupIndexer_WithSameMemberIdTwice_AppendsToBucket()
		{
			MockRegistry registry = new(MockBehavior.Default, new FastMockInteractions(0));
			FakeIndexerSetup setupA = new(true);
			FakeIndexerSetup setupB = new(true);

			registry.SetupIndexer(2, setupA);
			registry.SetupIndexer(2, setupB);

			IndexerSetup[]? snapshot = registry.GetIndexerSetupSnapshot(2);
			await That(snapshot).IsNotNull();
			await That(snapshot!.Length).IsEqualTo(2);
			await That(snapshot[0]).IsSameAs(setupA);
			await That(snapshot[1]).IsSameAs(setupB);
		}
	}

	public sealed class EventTests
	{
		[Fact]
		public async Task SetupEvent_WithIncreasingMemberIds_GrowsTableLazily()
		{
			MockRegistry registry = new(MockBehavior.Default, new FastMockInteractions(0));
			EventSetup setup0 = new(registry, "E0");
			EventSetup setup7 = new(registry, "E7");
			EventSetup setup3 = new(registry, "E3");

			registry.SetupEvent(0, setup0);
			registry.SetupEvent(7, setup7);
			registry.SetupEvent(3, setup3);

			EventSetup[]? snapshot0 = registry.GetEventSetupSnapshot(0);
			EventSetup[]? snapshot7 = registry.GetEventSetupSnapshot(7);
			EventSetup[]? snapshot3 = registry.GetEventSetupSnapshot(3);
			await That(snapshot0).IsNotNull();
			await That(snapshot0![0]).IsSameAs(setup0);
			await That(snapshot7).IsNotNull();
			await That(snapshot7![0]).IsSameAs(setup7);
			await That(snapshot3).IsNotNull();
			await That(snapshot3![0]).IsSameAs(setup3);
		}

		[Fact]
		public async Task SetupEvent_WithMemberId_PublishesToSnapshot()
		{
			MockRegistry registry = new(MockBehavior.Default, new FastMockInteractions(0));
			EventSetup setup = new(registry, "Evt");

			registry.SetupEvent(5, setup);

			EventSetup[]? snapshot = registry.GetEventSetupSnapshot(5);
			await That(snapshot).IsNotNull();
			await That(snapshot!.Length).IsEqualTo(1);
			await That(snapshot[0]).IsSameAs(setup);
		}

		[Fact]
		public async Task SetupEvent_WithMemberIdAndDefaultScenario_PublishesToSnapshot()
		{
			MockRegistry registry = new(MockBehavior.Default, new FastMockInteractions(0));
			EventSetup setup = new(registry, "Evt");

			registry.SetupEvent(5, "", setup);

			EventSetup[]? snapshot = registry.GetEventSetupSnapshot(5);
			await That(snapshot).IsNotNull();
			await That(snapshot!.Length).IsEqualTo(1);
			await That(snapshot[0]).IsSameAs(setup);
		}

		[Fact]
		public async Task SetupEvent_WithMemberIdAndNamedScenario_DoesNotPublishToSnapshot()
		{
			MockRegistry registry = new(MockBehavior.Default, new FastMockInteractions(0));
			EventSetup setup = new(registry, "Evt");

			registry.SetupEvent(5, "s1", setup);

			await That(registry.GetEventSetupSnapshot(5)).IsNull();
		}

		[Fact]
		public async Task SetupEvent_WithMemberIdAndNamedScenario_PreservesScenarioBucket()
		{
			MockRegistry registry = new(MockBehavior.Default, new FastMockInteractions(0));
			EventSetup setup = new(registry, "Evt");

			registry.SetupEvent(4, "s1", setup);

			await That(registry.GetEventSetupSnapshot(4)).IsNull();
			await That(registry.Setup.TryGetScenario("s1", out MockScenarioSetup? scoped)).IsTrue();
			await That(scoped!.Events.Count).IsEqualTo(1);
		}

		[Fact]
		public async Task SetupEvent_WithSameMemberIdTwice_AppendsToBucket()
		{
			MockRegistry registry = new(MockBehavior.Default, new FastMockInteractions(0));
			EventSetup setupA = new(registry, "Evt");
			EventSetup setupB = new(registry, "Evt");

			registry.SetupEvent(2, setupA);
			registry.SetupEvent(2, setupB);

			EventSetup[]? snapshot = registry.GetEventSetupSnapshot(2);
			await That(snapshot).IsNotNull();
			await That(snapshot!.Length).IsEqualTo(2);
			await That(snapshot[0]).IsSameAs(setupA);
			await That(snapshot[1]).IsSameAs(setupB);
		}
	}
}
