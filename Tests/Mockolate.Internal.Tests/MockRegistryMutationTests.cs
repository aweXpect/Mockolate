using System;
using System.Collections.Generic;
using Mockolate.Interactions;
using Mockolate.Internal.Tests.TestHelpers;
using Mockolate.Setup;

namespace Mockolate.Internal.Tests;

public sealed class MockRegistryMutationTests
{
	public sealed class GetIndexerSetupScenarioScopingTests
	{
		[Fact]
		public async Task WithActiveScenario_ShouldReturnScopedSetupOverGlobalSetup()
		{
			MockRegistry registry = new(MockBehavior.Default);
			FakeIndexerSetup globalSetup = new(match: true);
			FakeIndexerSetup scopedSetup = new(match: true);
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
			FakeIndexerSetup globalSetup = new(match: true);
			FakeIndexerSetup scopedSetup = new(match: true);
			registry.Setup.Indexers.Add(globalSetup);
			registry.Setup.GetOrCreateScenario("a").Indexers.Add(scopedSetup);

			IndexerSetup? result = registry.GetIndexerSetup<IndexerSetup>(_ => true);

			await That(result).IsSameAs(globalSetup);
		}
	}


	public sealed class IndexerFallbackStoresValueTests
	{
		[Fact]
		public async Task GetIndexerFallback_ShouldStoreDefaultForLaterLookup()
		{
			int counter = 0;
			MockBehavior behavior = MockBehavior.Default.WithDefaultValueFor(() => ++counter);
			MockRegistry registry = new(behavior);
			IndexerGetterAccess<int> access1 = new("p", 1);
			IndexerGetterAccess<int> access2 = new("p", 1);

			int first = registry.GetIndexerFallback<int>(access1, 0);
			int second = registry.GetIndexerFallback<int>(access2, 0);

			await That(first).IsEqualTo(1);
			await That(second).IsEqualTo(1);
		}

		[Fact]
		public async Task ApplyIndexerGetter_WithNullSetup_ShouldStoreBaseValueForLaterLookup()
		{
			MockRegistry registry = new(MockBehavior.Default);
			IndexerGetterAccess<int> access1 = new("p", 1);
			IndexerGetterAccess<int> access2 = new("p", 1);

			int first = registry.ApplyIndexerGetter<int>(access1, null, 42, 0);
			int second = registry.ApplyIndexerGetter<int>(access2, null, 99, 0);

			await That(first).IsEqualTo(42);
			await That(second).IsEqualTo(42);
		}
	}

	public sealed class InitializeStorageTests
	{
		[Fact]
		public async Task WithZero_ShouldNotThrow()
		{
			MockRegistry registry = new(MockBehavior.Default);

			void Act()
				=> registry.InitializeStorage(0);

			await That(Act).DoesNotThrow();
		}

		[Fact]
		public async Task WithNegative_ShouldThrowWithDescriptiveMessage()
		{
			MockRegistry registry = new(MockBehavior.Default);

			void Act()
				=> registry.InitializeStorage(-1);

			await That(Act).Throws<ArgumentOutOfRangeException>()
				.WithMessage("*non-negative*").AsWildcard();
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
			((IMockInteractions)wrappingRegistry.Interactions).RegisterInteraction(interaction);

			await That(wrappingRegistry.Interactions.Count).IsEqualTo(0);
		}
	}

	public sealed class MethodInvocationThreeParameterToStringTests
	{
		[Fact]
		public async Task Should_Include_ThirdParameterValue()
		{
			MethodInvocation<int, int, int> sut = new("MyType.MyMethod", "a", 1, "b", 2, "c", 3);

			string result = sut.ToString();

			await That(result).IsEqualTo("invoke method MyMethod(1, 2, 3)");
		}

		[Fact]
		public async Task WithNullThirdParameter_Should_FormatAsNullLiteral()
		{
			MethodInvocation<string?, string?, string?> sut =
				new("MyType.MyMethod", "a", null, "b", null, "c", null);

			string result = sut.ToString();

			await That(result).IsEqualTo("invoke method MyMethod(null, null, null)");
		}
	}

	public sealed class MockBehaviorToStringTests
	{
		[Fact]
		public async Task WithMultipleFlags_ShouldKeepAllPartsInOutput()
		{
			MockBehavior behavior = MockBehavior.Default
				.ThrowingWhenNotSetup()
				.SkippingBaseClass()
				.SkippingInteractionRecording();

			string result = behavior.ToString();

			await That(result).Contains("ThrowingWhenNotSetup");
			await That(result).Contains("SkippingBaseClass");
			await That(result).Contains("SkippingInteractionRecording");
		}
	}
}
