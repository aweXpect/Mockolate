using System.Reflection;
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
}
