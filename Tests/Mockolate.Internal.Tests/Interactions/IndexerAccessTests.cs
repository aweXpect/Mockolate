using System.Collections.Generic;
using Mockolate.Interactions;
using Mockolate.Setup;

namespace Mockolate.Internal.Tests.Interactions;

public sealed class IndexerAccessTests
{
	[Fact]
	public async Task StoreValue_OnIndexerGetterAccess1_UsesGetOrAddChildDispatch()
	{
		RecordingStorage storage = new();
		IndexerGetterAccess<int> access = new(42)
		{
			Storage = storage,
		};

		access.StoreValue("v");

		await That(storage.Calls).IsEqualTo(["GetOrAdd(42)",]);
	}

	[Fact]
	public async Task StoreValue_OnIndexerGetterAccess2_UsesGetOrAddChildDispatchForEachParameter()
	{
		RecordingStorage storage = new();
		IndexerGetterAccess<int, int> access = new(1, 2)
		{
			Storage = storage,
		};

		access.StoreValue("v");

		await That(storage.Calls).IsEqualTo(["GetOrAdd(1)", "GetOrAdd(2)",]);
	}

	[Fact]
	public async Task StoreValue_OnIndexerGetterAccess3_UsesGetOrAddChildDispatchForEachParameter()
	{
		RecordingStorage storage = new();
		IndexerGetterAccess<int, int, int> access = new(1, 2, 3)
		{
			Storage = storage,
		};

		access.StoreValue("v");

		await That(storage.Calls).IsEqualTo(["GetOrAdd(1)", "GetOrAdd(2)", "GetOrAdd(3)",]);
	}

	[Fact]
	public async Task StoreValue_OnIndexerGetterAccess4_UsesGetOrAddChildDispatchForEachParameter()
	{
		RecordingStorage storage = new();
		IndexerGetterAccess<int, int, int, int> access =
			new(1, 2, 3, 4)
			{
				Storage = storage,
			};

		access.StoreValue("v");

		await That(storage.Calls).IsEqualTo(["GetOrAdd(1)", "GetOrAdd(2)", "GetOrAdd(3)", "GetOrAdd(4)",]);
	}

	[Fact]
	public async Task StoreValue_OnIndexerSetterAccess1_UsesGetOrAddChildDispatch()
	{
		RecordingStorage storage = new();
		IndexerSetterAccess<int, string> access = new(42, "v")
		{
			Storage = storage,
		};

		access.StoreValue("v");

		await That(storage.Calls).IsEqualTo(["GetOrAdd(42)",]);
	}

	[Fact]
	public async Task StoreValue_OnIndexerSetterAccess2_UsesGetOrAddChildDispatchForEachParameter()
	{
		RecordingStorage storage = new();
		IndexerSetterAccess<int, int, string> access =
			new(1, 2, "v")
			{
				Storage = storage,
			};

		access.StoreValue("v");

		await That(storage.Calls).IsEqualTo(["GetOrAdd(1)", "GetOrAdd(2)",]);
	}

	[Fact]
	public async Task StoreValue_OnIndexerSetterAccess3_UsesGetOrAddChildDispatchForEachParameter()
	{
		RecordingStorage storage = new();
		IndexerSetterAccess<int, int, int, string> access =
			new(1, 2, 3, "v")
			{
				Storage = storage,
			};

		access.StoreValue("v");

		await That(storage.Calls).IsEqualTo(["GetOrAdd(1)", "GetOrAdd(2)", "GetOrAdd(3)",]);
	}

	[Fact]
	public async Task StoreValue_OnIndexerSetterAccess4_UsesGetOrAddChildDispatchForEachParameter()
	{
		RecordingStorage storage = new();
		IndexerSetterAccess<int, int, int, int, string> access =
			new(1, 2, 3, 4, "v")
			{
				Storage = storage,
			};

		access.StoreValue("v");

		await That(storage.Calls).IsEqualTo(["GetOrAdd(1)", "GetOrAdd(2)", "GetOrAdd(3)", "GetOrAdd(4)",]);
	}

	[Fact]
	public async Task ToString_OnIndexerGetterAccess4_FormatsEachNullParameterAsNullLiteral()
	{
		IndexerGetterAccess<string?, string?, string?, string?> access =
			new(null, null, null, null);

		await That(access.ToString()).IsEqualTo("get indexer [null, null, null, null]");
	}

	[Fact]
	public async Task ToString_OnIndexerSetterAccess4_FormatsEachNullParameterAsNullLiteral()
	{
		IndexerSetterAccess<string?, string?, string?, string?, string?> access =
			new(null, null, null, null, null);

		await That(access.ToString()).IsEqualTo("set indexer [null, null, null, null] to null");
	}

	[Fact]
	public async Task TryFindStoredValue_OnIndexerGetterAccess1_UsesGetChildDispatch()
	{
		RecordingStorage storage = new();
		IndexerGetterAccess<int> access = new(42)
		{
			Storage = storage,
		};

		access.TryFindStoredValue(out string _);

		await That(storage.Calls).IsEqualTo(["Get(42)",]);
	}

	[Fact]
	public async Task TryFindStoredValue_OnIndexerGetterAccess2_UsesGetChildDispatchForEachParameter()
	{
		RecordingStorage storage = new();
		IndexerGetterAccess<int, int> access = new(1, 2)
		{
			Storage = storage,
		};

		access.TryFindStoredValue(out string _);

		await That(storage.Calls).IsEqualTo(["Get(1)", "Get(2)",]);
	}

	[Fact]
	public async Task TryFindStoredValue_OnIndexerGetterAccess3_UsesGetChildDispatchForEachParameter()
	{
		RecordingStorage storage = new();
		IndexerGetterAccess<int, int, int> access = new(1, 2, 3)
		{
			Storage = storage,
		};

		access.TryFindStoredValue(out string _);

		await That(storage.Calls).IsEqualTo(["Get(1)", "Get(2)", "Get(3)",]);
	}

	[Fact]
	public async Task TryFindStoredValue_OnIndexerGetterAccess4_UsesGetChildDispatchForEachParameter()
	{
		RecordingStorage storage = new();
		IndexerGetterAccess<int, int, int, int> access =
			new(1, 2, 3, 4)
			{
				Storage = storage,
			};

		access.TryFindStoredValue(out string _);

		await That(storage.Calls).IsEqualTo(["Get(1)", "Get(2)", "Get(3)", "Get(4)",]);
	}

	[Fact]
	public async Task TryFindStoredValue_OnIndexerSetterAccess1_UsesGetChildDispatch()
	{
		RecordingStorage storage = new();
		IndexerSetterAccess<int, string> access = new(42, "v")
		{
			Storage = storage,
		};

		access.TryFindStoredValue(out string _);

		await That(storage.Calls).IsEqualTo(["Get(42)",]);
	}

	[Fact]
	public async Task TryFindStoredValue_OnIndexerSetterAccess2_UsesGetChildDispatchForEachParameter()
	{
		RecordingStorage storage = new();
		IndexerSetterAccess<int, int, string> access =
			new(1, 2, "v")
			{
				Storage = storage,
			};

		access.TryFindStoredValue(out string _);

		await That(storage.Calls).IsEqualTo(["Get(1)", "Get(2)",]);
	}

	[Fact]
	public async Task TryFindStoredValue_OnIndexerSetterAccess3_UsesGetChildDispatchForEachParameter()
	{
		RecordingStorage storage = new();
		IndexerSetterAccess<int, int, int, string> access =
			new(1, 2, 3, "v")
			{
				Storage = storage,
			};

		access.TryFindStoredValue(out string _);

		await That(storage.Calls).IsEqualTo(["Get(1)", "Get(2)", "Get(3)",]);
	}

	[Fact]
	public async Task TryFindStoredValue_OnIndexerSetterAccess4_UsesGetChildDispatchForEachParameter()
	{
		RecordingStorage storage = new();
		IndexerSetterAccess<int, int, int, int, string> access =
			new(1, 2, 3, 4, "v")
			{
				Storage = storage,
			};

		access.TryFindStoredValue(out string _);

		await That(storage.Calls).IsEqualTo(["Get(1)", "Get(2)", "Get(3)", "Get(4)",]);
	}

	private sealed class RecordingStorage : IndexerValueStorage
	{
		public List<string> Calls { get; } = [];

		public override IndexerValueStorage? GetChildDispatch<TKey>(TKey key)
		{
			Calls.Add($"Get({key?.ToString() ?? "<null>"})");
			return this;
		}

		public override IndexerValueStorage GetOrAddChildDispatch<TKey>(TKey key)
		{
			Calls.Add($"GetOrAdd({key?.ToString() ?? "<null>"})");
			return this;
		}
	}
}
