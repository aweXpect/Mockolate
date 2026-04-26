using System.Linq;
using System.Reflection;
using Mockolate.Exceptions;
using Mockolate.Interactions;
using Mockolate.Internal.Tests.TestHelpers;
using Mockolate.Parameters;
using Mockolate.Setup;
using Mockolate.Verify;

namespace Mockolate.Internal.Tests.Registry;

public sealed class MockRegistryTests
{
	private static MethodInfo GetMethodInfo()
		=> typeof(MockRegistryTests).GetMethod(nameof(GetMethodInfo),
			BindingFlags.Static | BindingFlags.NonPublic)!;

	public sealed class ConstructorSkipFlagAgreementTests
	{
		[Fact]
		public async Task BehaviorAndInteractions_WhenSkipFlagsDisagree_Throws()
		{
			MockBehavior recordingBehavior = MockBehavior.Default;
			FastMockInteractions skippingInteractions = new(0, true);

			void Act()
			{
				_ = new MockRegistry(recordingBehavior, skippingInteractions);
			}

			await That(Act).Throws<ArgumentException>()
				.WithMessage("*SkipInteractionRecording*").AsWildcard();
		}

		[Fact]
		public async Task BehaviorAndInteractions_WhenSkippingBehaviorMeetsRecordingInteractions_Throws()
		{
			MockBehavior skippingBehavior = MockBehavior.Default.SkippingInteractionRecording();
			FastMockInteractions recordingInteractions = new(0);

			void Act()
			{
				_ = new MockRegistry(skippingBehavior, recordingInteractions);
			}

			await That(Act).Throws<ArgumentException>()
				.WithMessage("*SkipInteractionRecording*").AsWildcard();
		}

		[Fact]
		public async Task RegistryAndInteractions_WhenSkipFlagsDisagree_Throws()
		{
			MockRegistry registry = new(MockBehavior.Default);
			FastMockInteractions skippingInteractions = new(0, true);

			void Act()
			{
				_ = new MockRegistry(registry, skippingInteractions);
			}

			await That(Act).Throws<ArgumentException>()
				.WithMessage("*SkipInteractionRecording*").AsWildcard();
		}

		[Fact]
		public async Task RegistryAndInteractions_WhenSkippingRegistryMeetsRecordingInteractions_Throws()
		{
			MockBehavior skippingBehavior = MockBehavior.Default.SkippingInteractionRecording();
			MockRegistry registry = new(skippingBehavior, new FastMockInteractions(0, true));
			FastMockInteractions recordingInteractions = new(0);

			void Act()
			{
				_ = new MockRegistry(registry, recordingInteractions);
			}

			await That(Act).Throws<ArgumentException>()
				.WithMessage("*SkipInteractionRecording*").AsWildcard();
		}
	}

	public sealed class GetIndexerSetupScenarioScopingTests
	{
		[Fact]
		public async Task WithActiveScenario_ShouldReturnScopedSetupOverGlobalSetup()
		{
			MockRegistry registry = new(MockBehavior.Default);
			FakeIndexerSetup globalSetup = new(true);
			FakeIndexerSetup scopedSetup = new(true);
			registry.Setup.Indexers.Add(globalSetup);
			registry.Setup.GetOrCreateScenario("a").Indexers.Add(scopedSetup);

			registry.TransitionTo("a");
			IndexerSetup? result = registry.GetIndexerSetup<IndexerSetup>(_ => true);

			await That(result).IsSameAs(scopedSetup);
		}

		[Fact]
		public async Task WithoutActiveScenario_ShouldFallBackToGlobalSetup()
		{
			MockRegistry registry = new(MockBehavior.Default);
			FakeIndexerSetup globalSetup = new(true);
			FakeIndexerSetup scopedSetup = new(true);
			registry.Setup.Indexers.Add(globalSetup);
			registry.Setup.GetOrCreateScenario("a").Indexers.Add(scopedSetup);

			IndexerSetup? result = registry.GetIndexerSetup<IndexerSetup>(_ => true);

			await That(result).IsSameAs(globalSetup);
		}
	}

	public sealed class IndexerFallbackStoresValueTests
	{
		[Fact]
		public async Task ApplyIndexerGetter_WithNullSetup_ShouldStoreBaseValueForLaterLookup()
		{
			MockRegistry registry = new(MockBehavior.Default);
			IndexerGetterAccess<int> access1 = new(1);
			IndexerGetterAccess<int> access2 = new(1);

			int first = registry.ApplyIndexerGetter(access1, null, 42, 0);
			int second = registry.ApplyIndexerGetter(access2, null, 99, 0);

			await That(first).IsEqualTo(42);
			await That(second).IsEqualTo(42);
		}

		[Fact]
		public async Task GetIndexerFallback_ShouldStoreDefaultForLaterLookup()
		{
			int counter = 0;
			MockBehavior behavior = MockBehavior.Default.WithDefaultValueFor(() => ++counter);
			MockRegistry registry = new(behavior);
			IndexerGetterAccess<int> access1 = new(1);
			IndexerGetterAccess<int> access2 = new(1);

			int first = registry.GetIndexerFallback<int>(access1, 0);
			int second = registry.GetIndexerFallback<int>(access2, 0);

			await That(first).IsEqualTo(1);
			await That(second).IsEqualTo(1);
		}
	}

	public sealed class InitializeStorageTests
	{
		[Fact]
		public async Task WithNegative_ShouldThrowWithDescriptiveMessage()
		{
			MockRegistry registry = new(MockBehavior.Default);

			void Act()
			{
				registry.InitializeStorage(-1);
			}

			await That(Act).Throws<ArgumentOutOfRangeException>()
				.WithMessage("*non-negative*").AsWildcard();
		}

		[Fact]
		public async Task WithZero_ShouldNotThrow()
		{
			MockRegistry registry = new(MockBehavior.Default);

			void Act()
			{
				registry.InitializeStorage(0);
			}

			await That(Act).DoesNotThrow();
		}
	}

	public sealed class WrapConstructorTests
	{
		[Fact]
		public async Task WhenBehaviorSkipsInteractionRecording_WrappedRegistryAlsoSkipsRecording()
		{
			MockBehavior behavior = MockBehavior.Default.SkippingInteractionRecording();
			MockRegistry original = new(behavior);
			object wrapped = new();

			MockRegistry wrappingRegistry = new(original, wrapped);

			MethodInvocation interaction = new("test");
			wrappingRegistry.Interactions.RegisterInteraction(interaction);

			await That(wrappingRegistry.Interactions.Count).IsEqualTo(0);
		}
	}

	public sealed class VerifyPropertyStringKeyedTests
	{
		[Fact]
		public async Task MockGot_WhenNameDoesNotMatch_ShouldReturnNever()
		{
			MockRegistry registry = new(MockBehavior.Default);
			registry.GetProperty("foo.bar", () => 0, null);

			VerificationResult<object> result = registry.VerifyProperty<object>(this, "baz.bar");

			await That(result).Never();
		}

		[Fact]
		public async Task MockGot_WhenNameMatches_ShouldReturnOnce()
		{
			MockRegistry registry = new(MockBehavior.Default);
			registry.GetProperty("foo.bar", () => 0, null);

			VerificationResult<object> result = registry.VerifyProperty<object>(this, "foo.bar");

			await That(result).Once();
		}

		[Fact]
		public async Task MockGot_WithoutInteractions_ShouldReturnNeverResultWithExpectation()
		{
			MockRegistry registry = new(MockBehavior.Default);

			VerificationResult<object> result = registry.VerifyProperty<object>(this, "foo.bar");

			await That(result).Never();
			await That(((IVerificationResult)result).Expectation).IsEqualTo("got property bar");
		}

		[Fact]
		public async Task MockSet_WhenNameAndValueMatches_ShouldReturnOnce()
		{
			MockRegistry registry = new(MockBehavior.Default);
			registry.SetProperty("foo.bar", 4);

			VerificationResult<object> result =
				registry.VerifyProperty<object, int>(this, "foo.bar", (IParameterMatch<int>)It.IsAny<int>());

			await That(result).Once();
		}

		[Fact]
		public async Task MockSet_WhenOnlyNameMatches_ShouldReturnNever()
		{
			MockRegistry registry = new(MockBehavior.Default);
			registry.SetProperty("foo.bar", 4);

			VerificationResult<object> result =
				registry.VerifyProperty<object, string>(this, "foo.bar", (IParameterMatch<string>)It.IsAny<string>());

			await That(result).Never();
		}

		[Fact]
		public async Task MockSet_WhenOnlyValueMatches_ShouldReturnNever()
		{
			MockRegistry registry = new(MockBehavior.Default);
			registry.SetProperty("foo.bar", 4);

			VerificationResult<object> result =
				registry.VerifyProperty<object, int>(this, "baz.bar", (IParameterMatch<int>)It.IsAny<int>());

			await That(result).Never();
		}

		[Fact]
		public async Task MockSet_WithoutInteractions_ShouldReturnNeverResultWithExpectation()
		{
			MockRegistry registry = new(MockBehavior.Default);

			VerificationResult<object> result =
				registry.VerifyProperty<object, int>(this, "foo.bar", (IParameterMatch<int>)It.IsAny<int>());

			await That(result).Never();
			await That(((IVerificationResult)result).Expectation).IsEqualTo("set property bar to It.IsAny<int>()");
		}

		[Fact]
		public async Task VerifyProperty_WhenNameContainsNoDot_ShouldIncludeFullNameInExpectation()
		{
			MockRegistry registry = new(MockBehavior.Default);

			VerificationResult<int> result = registry.VerifyProperty(0, "SomeNameWithoutADot");

			result.Never();
			await That(((IVerificationResult)result).Expectation).IsEqualTo("got property SomeNameWithoutADot");
		}

		[Fact]
		public async Task VerifyProperty_WhenNameStartsWithDot_ShouldOmitDotInExpectation()
		{
			MockRegistry registry = new(MockBehavior.Default);

			VerificationResult<int> result = registry.VerifyProperty(0, ".bar");

			result.Never();
			await That(((IVerificationResult)result).Expectation).IsEqualTo("got property bar");
		}
	}

	public sealed class SubscribedToStringKeyedTests
	{
		[Fact]
		public async Task Subscribed_WhenNameDoesNotMatch_ShouldReturnNever()
		{
			MockRegistry registry = new(MockBehavior.Default);
			registry.AddEvent("foo.bar", this, GetMethodInfo());

			VerificationResult<object> result = registry.SubscribedTo<object>(this, "baz.bar");

			await That(result).Never();
		}

		[Fact]
		public async Task Subscribed_WhenNameMatches_ShouldReturnOnce()
		{
			MockRegistry registry = new(MockBehavior.Default);
			registry.AddEvent("foo.bar", this, GetMethodInfo());

			VerificationResult<object> result = registry.SubscribedTo<object>(this, "foo.bar");

			await That(result).Once();
		}

		[Fact]
		public async Task Subscribed_WithoutInteractions_ShouldReturnNeverResultWithExpectation()
		{
			MockRegistry registry = new(MockBehavior.Default);

			VerificationResult<object> result = registry.SubscribedTo<object>(this, "baz.bar");

			await That(result).Never();
			await That(((IVerificationResult)result).Expectation).IsEqualTo("subscribed to event bar");
		}
	}

	public sealed class UnsubscribedFromStringKeyedTests
	{
		[Fact]
		public async Task Unsubscribed_WhenNameDoesNotMatch_ShouldReturnNever()
		{
			MockRegistry registry = new(MockBehavior.Default);
			registry.RemoveEvent("foo.bar", this, GetMethodInfo());

			VerificationResult<object> result = registry.UnsubscribedFrom<object>(this, "baz.bar");

			await That(result).Never();
		}

		[Fact]
		public async Task Unsubscribed_WhenNameMatches_ShouldReturnOnce()
		{
			MockRegistry registry = new(MockBehavior.Default);
			registry.RemoveEvent("foo.bar", this, GetMethodInfo());

			VerificationResult<object> result = registry.UnsubscribedFrom<object>(this, "foo.bar");

			await That(result).Once();
		}

		[Fact]
		public async Task Unsubscribed_WithoutInteractions_ShouldReturnNeverResultWithExpectation()
		{
			MockRegistry registry = new(MockBehavior.Default);

			VerificationResult<object> result = registry.UnsubscribedFrom<object>(this, "baz.bar");

			await That(result).Never();
			await That(((IVerificationResult)result).Expectation).IsEqualTo("unsubscribed from event bar");
		}
	}

	public sealed class GetMethodSetupSnapshotTests
	{
		[Fact]
		public async Task WithEmptyTable_ShouldReturnNull()
		{
			MockRegistry registry = new(MockBehavior.Default);

			MethodSetup[]? result = registry.GetMethodSetupSnapshot(0);

			await That(result).IsNull();
		}

		[Fact]
		public async Task WithMemberIdAtBoundary_ShouldReturnTheBucket()
		{
			MockRegistry registry = new(MockBehavior.Default);
			ReturnMethodSetup<int>.WithParameterCollection setup = new(MockBehavior.Default, "Method");
			registry.SetupMethod(5, setup);

			MethodSetup[]? result = registry.GetMethodSetupSnapshot(5);

			await That(result).IsNotNull();
			await That(result!).Contains(setup);
		}

		[Fact]
		public async Task WithMemberIdBeyondTable_ShouldReturnNull()
		{
			MockRegistry registry = new(MockBehavior.Default);
			ReturnMethodSetup<int>.WithParameterCollection setup = new(MockBehavior.Default, "Method");
			registry.SetupMethod(0, setup);

			MethodSetup[]? result = registry.GetMethodSetupSnapshot(5);

			await That(result).IsNull();
		}
	}

	public sealed class GetPropertySetupSnapshotTests
	{
		[Fact]
		public async Task WithEmptyTable_ShouldReturnNull()
		{
			MockRegistry registry = new(MockBehavior.Default);

			PropertySetup? result = registry.GetPropertySetupSnapshot(0);

			await That(result).IsNull();
		}

		[Fact]
		public async Task WithMemberIdAtBoundary_ShouldReturnTheSetup()
		{
			MockRegistry registry = new(MockBehavior.Default);
			PropertySetup<int> setup = new(registry, "P");
			registry.SetupProperty(5, setup);

			PropertySetup? result = registry.GetPropertySetupSnapshot(5);

			await That(result).IsSameAs(setup);
		}

		[Fact]
		public async Task WithMemberIdBeyondTable_ShouldReturnNull()
		{
			MockRegistry registry = new(MockBehavior.Default);
			PropertySetup<int> setup = new(registry, "P");
			registry.SetupProperty(0, setup);

			PropertySetup? result = registry.GetPropertySetupSnapshot(5);

			await That(result).IsNull();
		}
	}

	public sealed class GetIndexerSetupSnapshotTests
	{
		[Fact]
		public async Task WithEmptyTable_ShouldReturnNull()
		{
			MockRegistry registry = new(MockBehavior.Default);

			IndexerSetup[]? result = registry.GetIndexerSetupSnapshot(0);

			await That(result).IsNull();
		}

		[Fact]
		public async Task WithMemberIdAtBoundary_ShouldReturnTheBucket()
		{
			MockRegistry registry = new(MockBehavior.Default);
			IndexerSetup<string, int> setup = new(registry, (IParameterMatch<int>)It.IsAny<int>());
			registry.SetupIndexer(5, setup);

			IndexerSetup[]? result = registry.GetIndexerSetupSnapshot(5);

			await That(result).IsNotNull();
			await That(result!).Contains(setup);
		}

		[Fact]
		public async Task WithMemberIdBeyondTable_ShouldReturnNull()
		{
			MockRegistry registry = new(MockBehavior.Default);
			IndexerSetup<string, int> setup = new(registry, (IParameterMatch<int>)It.IsAny<int>());
			registry.SetupIndexer(0, setup);

			IndexerSetup[]? result = registry.GetIndexerSetupSnapshot(5);

			await That(result).IsNull();
		}
	}

	public sealed class GetEventSetupSnapshotTests
	{
		[Fact]
		public async Task WithEmptyTable_ShouldReturnNull()
		{
			MockRegistry registry = new(MockBehavior.Default);

			EventSetup[]? result = registry.GetEventSetupSnapshot(0);

			await That(result).IsNull();
		}

		[Fact]
		public async Task WithMemberIdAtBoundary_ShouldReturnTheBucket()
		{
			MockRegistry registry = new(MockBehavior.Default);
			EventSetup setup = new(registry, "OnEvent");
			registry.SetupEvent(5, setup);

			EventSetup[]? result = registry.GetEventSetupSnapshot(5);

			await That(result).IsNotNull();
			await That(result!).Contains(setup);
		}

		[Fact]
		public async Task WithMemberIdBeyondTable_ShouldReturnNull()
		{
			MockRegistry registry = new(MockBehavior.Default);
			EventSetup setup = new(registry, "OnEvent");
			registry.SetupEvent(0, setup);

			EventSetup[]? result = registry.GetEventSetupSnapshot(5);

			await That(result).IsNull();
		}
	}

	public sealed class GetPropertyFastTests
	{
		[Fact]
		public async Task WithActiveScenario_ShouldAlwaysTakeColdPath()
		{
			MockRegistry registry = new(MockBehavior.Default);
			PropertySetup<int> defaultSetup = new(registry, "P");
			defaultSetup.InitializeWith(10);
			registry.SetupProperty(2, defaultSetup);

			PropertySetup<int> scenarioSetup = new(registry, "P");
			scenarioSetup.InitializeWith(99);
			registry.SetupProperty(2, "myScenario", scenarioSetup);

			registry.TransitionTo("myScenario");
			int result = registry.GetPropertyFast(2, "P", _ => 0);

			await That(result).IsEqualTo(99);
		}

		[Fact]
		public async Task WithBaseValueAccessor_ShouldAlwaysTakeColdPath()
		{
			MockRegistry registry = new(MockBehavior.Default);
			PropertySetup<int> snapshot = new(registry, "P");
			registry.SetupProperty(2, snapshot);

			int baseInvocations = 0;

			int Base()
			{
				baseInvocations++;
				return 99;
			}

			int result = registry.GetPropertyFast(2, "P", _ => 7, Base);

			await That(baseInvocations).IsEqualTo(1);
			await That(result).IsEqualTo(99);
		}

		[Fact]
		public async Task WithoutSnapshotSetup_ShouldFallBackToColdPath()
		{
			MockRegistry registry = new(MockBehavior.Default);

			int result = registry.GetPropertyFast(0, "P", _ => 7);

			await That(result).IsEqualTo(7);
		}

		[Fact]
		public async Task WithSnapshotSetup_ShouldBypassSlowResolverAndReturnSetupValue()
		{
			MockRegistry registry = new(MockBehavior.Default);
			PropertySetup<int> snapshot = new(registry, "P");
			snapshot.InitializeWith(42);
			registry.SetupProperty(2, snapshot);

			int generatorInvocations = 0;

			int Generator(MockBehavior _)
			{
				generatorInvocations++;
				return -1;
			}

			int first = registry.GetPropertyFast(2, "P", Generator);
			int second = registry.GetPropertyFast(2, "P", Generator);

			await That(first).IsEqualTo(42);
			await That(second).IsEqualTo(42);
			await That(generatorInvocations).IsEqualTo(0);
		}
	}

	public sealed class SetPropertyFastTests
	{
		[Fact]
		public async Task WithoutSnapshotSetup_ShouldFallBackToResolveSetup()
		{
			MockRegistry registry = new(MockBehavior.Default);
			PropertySetup<int> setup = new(registry, "P");
			setup.InitializeWith(10);
			registry.SetupProperty(setup);

			bool skipBase = registry.SetPropertyFast(2, 3, "P", 42);

			await That(skipBase).IsFalse();
			int after = registry.GetProperty("P", () => -1, null);
			await That(after).IsEqualTo(42);
		}

		[Fact]
		public async Task WithSnapshotSetup_ShouldInvokeSetterAndStoreValue()
		{
			MockRegistry registry = new(MockBehavior.Default);
			PropertySetup<int> snapshot = new(registry, "P");
			snapshot.InitializeWith(0);
			registry.SetupProperty(2, snapshot);

			bool skipBase = registry.SetPropertyFast(2, 3, "P", 42);

			await That(skipBase).IsFalse();
			int after = registry.GetPropertyFast(2, "P", _ => -1);
			await That(after).IsEqualTo(42);
		}
	}

	public sealed class AddEventMemberIdTests
	{
		[Fact]
		public async Task AddEvent_WithMemberId_ShouldRecordAsEventSubscription()
		{
			MockRegistry registry = new(MockBehavior.Default);

			registry.AddEvent(0, "OnFoo", this, GetMethodInfo());

			IInteraction recorded = registry.Interactions.Single();
			await That(recorded).IsExactly<EventSubscription>();
		}

		[Fact]
		public async Task AddEvent_WithMemberIdAndMatchingSnapshot_ShouldInvokeSubscribedCallback()
		{
			MockRegistry registry = new(MockBehavior.Default);
			int subscribedCount = 0;
			EventSetup setup = new(registry, "OnFoo");
			setup.OnSubscribed.Do(() => subscribedCount++);
			registry.SetupEvent(7, setup);

			registry.AddEvent(7, "OnFoo", this, GetMethodInfo());

			await That(subscribedCount).IsEqualTo(1);
		}

		[Fact]
		public async Task AddEvent_WithNullMethod_ShouldThrowMockException()
		{
			MockRegistry registry = new(MockBehavior.Default);

			void Act()
			{
				registry.AddEvent(0, "OnFoo", this, null);
			}

			await That(Act).Throws<MockException>();
		}
	}

	public sealed class RemoveEventMemberIdTests
	{
		[Fact]
		public async Task RemoveEvent_WithMemberId_ShouldRecordAsEventUnsubscription()
		{
			MockRegistry registry = new(MockBehavior.Default);

			registry.RemoveEvent(0, "OnFoo", this, GetMethodInfo());

			IInteraction recorded = registry.Interactions.Single();
			await That(recorded).IsExactly<EventUnsubscription>();
		}

		[Fact]
		public async Task RemoveEvent_WithMemberIdAndMatchingSnapshot_ShouldInvokeUnsubscribedCallback()
		{
			MockRegistry registry = new(MockBehavior.Default);
			int unsubscribedCount = 0;
			EventSetup setup = new(registry, "OnFoo");
			setup.OnUnsubscribed.Do(() => unsubscribedCount++);
			registry.SetupEvent(7, setup);

			registry.RemoveEvent(7, "OnFoo", this, GetMethodInfo());

			await That(unsubscribedCount).IsEqualTo(1);
		}

		[Fact]
		public async Task RemoveEvent_WithNullMethod_ShouldThrowMockException()
		{
			MockRegistry registry = new(MockBehavior.Default);

			void Act()
			{
				registry.RemoveEvent(0, "OnFoo", this, null);
			}

			await That(Act).Throws<MockException>();
		}
	}

	public sealed class SetPropertyFallbackBehaviorTests
	{
		[Fact]
		public async Task SetProperty_WithoutSnapshot_ShouldReturnBehaviorSkipBaseClassWhenSetupAllowsBase()
		{
			MockBehavior behavior = MockBehavior.Default.SkippingBaseClass();
			MockRegistry registry = new(behavior);

			bool result = registry.SetProperty("P", 42);

			await That(result).IsTrue();
		}

		[Fact]
		public async Task SetProperty_WithoutSnapshot_ShouldReturnFalseWhenBehaviorDoesNotSkip()
		{
			MockRegistry registry = new(MockBehavior.Default);

			bool result = registry.SetProperty("P", 42);

			await That(result).IsFalse();
		}
	}
}
