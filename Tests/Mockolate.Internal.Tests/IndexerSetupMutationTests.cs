using Mockolate.Interactions;
using Mockolate.Internal.Tests.TestHelpers;
using Mockolate.Parameters;
using Mockolate.Setup;

namespace Mockolate.Internal.Tests;

public sealed class IndexerSetupMutationTests
{
	[Fact]
	public async Task TryCast_WhenValueIsNull_ShouldReturnTrue()
	{
		bool success = FakeIndexerSetup.InvokeTryCast<string>(null, out string result, MockBehavior.Default);

		await That(success).IsTrue();
		await That(result).IsNull();
	}

	[Fact]
	public async Task TryCast_WhenValueIsNotOfTargetTypeAndNotNull_ShouldReturnFalse()
	{
		bool success = FakeIndexerSetup.InvokeTryCast<string>(42, out string _, MockBehavior.Default);

		await That(success).IsFalse();
	}

	[Fact]
	public async Task GetResult_WithBaseValue_1Param_StoresComputedValueForLaterLookup()
	{
		IndexerSetup<string, int> setup = new(
			new MockRegistry(MockBehavior.Default),
			(IParameterMatch<int>)It.IsAny<int>());
		IndexerValueStorage<string> storage = new();
		IndexerGetterAccess<int> access1 = new("p", 42) { Storage = storage, };

		string result = setup.GetResult(access1, MockBehavior.Default, "base");

		IndexerGetterAccess<int> access2 = new("p", 42) { Storage = storage, };
		bool found = access2.TryFindStoredValue(out string stored);

		await That(result).IsEqualTo("base");
		await That(found).IsTrue();
		await That(stored).IsEqualTo("base");
	}

	[Fact]
	public async Task GetResult_WithDefaultValueGenerator_1Param_StoresComputedValueForLaterLookup()
	{
		IndexerSetup<string, int> setup = new(
			new MockRegistry(MockBehavior.Default),
			(IParameterMatch<int>)It.IsAny<int>());
		IndexerValueStorage<string> storage = new();
		IndexerGetterAccess<int> access1 = new("p", 42) { Storage = storage, };

		string result = setup.GetResult<string>(access1, MockBehavior.Default, () => "generated");

		IndexerGetterAccess<int> access2 = new("p", 42) { Storage = storage, };
		bool found = access2.TryFindStoredValue(out string stored);

		await That(result).IsEqualTo("generated");
		await That(found).IsTrue();
		await That(stored).IsEqualTo("generated");
	}

	[Fact]
	public async Task GetResult_WithBaseValue_2Param_StoresComputedValueForLaterLookup()
	{
		IndexerSetup<string, int, int> setup = new(
			new MockRegistry(MockBehavior.Default),
			(IParameterMatch<int>)It.IsAny<int>(),
			(IParameterMatch<int>)It.IsAny<int>());
		IndexerValueStorage<string> storage = new();
		IndexerGetterAccess<int, int> access1 = new("p1", 1, "p2", 2) { Storage = storage, };

		string result = setup.GetResult(access1, MockBehavior.Default, "base");

		IndexerGetterAccess<int, int> access2 = new("p1", 1, "p2", 2) { Storage = storage, };
		bool found = access2.TryFindStoredValue(out string stored);

		await That(result).IsEqualTo("base");
		await That(found).IsTrue();
		await That(stored).IsEqualTo("base");
	}

	[Fact]
	public async Task GetResult_WithBaseValue_3Param_StoresComputedValueForLaterLookup()
	{
		IndexerSetup<string, int, int, int> setup = new(
			new MockRegistry(MockBehavior.Default),
			(IParameterMatch<int>)It.IsAny<int>(),
			(IParameterMatch<int>)It.IsAny<int>(),
			(IParameterMatch<int>)It.IsAny<int>());
		IndexerValueStorage<string> storage = new();
		IndexerGetterAccess<int, int, int> access1 =
			new("p1", 1, "p2", 2, "p3", 3) { Storage = storage, };

		string result = setup.GetResult(access1, MockBehavior.Default, "base");

		IndexerGetterAccess<int, int, int> access2 =
			new("p1", 1, "p2", 2, "p3", 3) { Storage = storage, };
		bool found = access2.TryFindStoredValue(out string stored);

		await That(result).IsEqualTo("base");
		await That(found).IsTrue();
		await That(stored).IsEqualTo("base");
	}

	[Fact]
	public async Task GetResult_WithBaseValue_4Param_StoresComputedValueForLaterLookup()
	{
		IndexerSetup<string, int, int, int, int> setup = new(
			new MockRegistry(MockBehavior.Default),
			(IParameterMatch<int>)It.IsAny<int>(),
			(IParameterMatch<int>)It.IsAny<int>(),
			(IParameterMatch<int>)It.IsAny<int>(),
			(IParameterMatch<int>)It.IsAny<int>());
		IndexerValueStorage<string> storage = new();
		IndexerGetterAccess<int, int, int, int> access1 =
			new("p1", 1, "p2", 2, "p3", 3, "p4", 4) { Storage = storage, };

		string result = setup.GetResult(access1, MockBehavior.Default, "base");

		IndexerGetterAccess<int, int, int, int> access2 =
			new("p1", 1, "p2", 2, "p3", 3, "p4", 4) { Storage = storage, };
		bool found = access2.TryFindStoredValue(out string stored);

		await That(result).IsEqualTo("base");
		await That(found).IsTrue();
		await That(stored).IsEqualTo("base");
	}
}
