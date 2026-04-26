using System;
using System.Diagnostics;
using System.Threading;
using Mockolate.Setup;

namespace Mockolate;

public partial class MockRegistry
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly object _setupsByMemberIdLock = new();

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private EventSetup[]?[]? _eventSetupsByMemberId;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private IndexerSetup[]?[]? _indexerSetupsByMemberId;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private PropertySetup?[]? _propertySetupsByMemberId;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private MethodSetup[]?[]? _setupsByMemberId;

	/// <summary>
	///     The registered setups for the mock, including methods, properties, indexers and events.
	/// </summary>
	internal MockSetups Setup { get; }

	/// <summary>
	///     Registers <paramref name="indexerSetup" /> for the default scenario.
	/// </summary>
	/// <param name="indexerSetup">The indexer setup produced by the fluent <c>Setup[...]</c> API.</param>
	public void SetupIndexer(IndexerSetup indexerSetup)
		=> SetupIndexer("", indexerSetup);

	/// <summary>
	///     Registers <paramref name="indexerSetup" /> for the given <paramref name="scenario" />.
	/// </summary>
	/// <param name="scenario">The scenario name the setup applies to. The setup only applies while <see cref="Scenario" /> equals <paramref name="scenario" /> and does not leak into the default bucket.</param>
	/// <param name="indexerSetup">The indexer setup produced by the fluent <c>Setup[...]</c> API.</param>
	public void SetupIndexer(string scenario, IndexerSetup indexerSetup)
		=> Setup.GetOrCreateScenario(scenario).Indexers.Add(indexerSetup);

	/// <summary>
	///     Registers <paramref name="indexerSetup" /> for the default scenario and additionally indexes it by the
	///     generator-emitted <paramref name="memberId" /> for fast dispatch from the proxy indexer body.
	/// </summary>
	/// <param name="memberId">The generator-emitted member id for the setup's target indexer accessor.</param>
	/// <param name="indexerSetup">The indexer setup produced by the fluent <c>Setup[...]</c> API.</param>
	public void SetupIndexer(int memberId, IndexerSetup indexerSetup)
	{
		SetupIndexer(indexerSetup);
		AppendToIndexerMemberIdBucket(memberId, indexerSetup);
	}

	/// <summary>
	///     Registers <paramref name="indexerSetup" /> for the given <paramref name="scenario" />. When
	///     <paramref name="scenario" /> is the default scope, the setup is additionally indexed by
	///     <paramref name="memberId" /> for fast dispatch.
	/// </summary>
	/// <param name="memberId">The generator-emitted member id for the setup's target indexer accessor.</param>
	/// <param name="scenario">The scenario name the setup applies to.</param>
	/// <param name="indexerSetup">The indexer setup produced by the fluent <c>Setup[...]</c> API.</param>
	public void SetupIndexer(int memberId, string scenario, IndexerSetup indexerSetup)
	{
		SetupIndexer(scenario, indexerSetup);
		if (string.IsNullOrEmpty(scenario))
		{
			AppendToIndexerMemberIdBucket(memberId, indexerSetup);
		}
	}

	private void AppendToIndexerMemberIdBucket(int memberId, IndexerSetup indexerSetup)
	{
		lock (_setupsByMemberIdLock)
		{
			IndexerSetup[]?[]? oldTable = _indexerSetupsByMemberId;
			int requiredLen = memberId + 1;
			int newLen = oldTable is null ? requiredLen : Math.Max(oldTable.Length, requiredLen);
			IndexerSetup[]?[] newTable;
			if (oldTable is null || newLen > oldTable.Length)
			{
				newTable = new IndexerSetup[newLen][];
				if (oldTable is not null)
				{
					Array.Copy(oldTable, newTable, oldTable.Length);
				}
			}
			else
			{
				newTable = oldTable;
			}

			IndexerSetup[]? existing = newTable[memberId];
			IndexerSetup[] bucket;
			if (existing is null || existing.Length == 0)
			{
				bucket = new[]
				{
					indexerSetup,
				};
			}
			else
			{
				bucket = new IndexerSetup[existing.Length + 1];
				Array.Copy(existing, bucket, existing.Length);
				bucket[existing.Length] = indexerSetup;
			}

			newTable[memberId] = bucket;
			Volatile.Write(ref _indexerSetupsByMemberId, newTable);
		}
	}

	/// <summary>
	///     Registers <paramref name="methodSetup" /> for the default scenario.
	/// </summary>
	/// <param name="methodSetup">The method setup produced by the fluent <c>Setup.MethodName(...)</c> API.</param>
	public void SetupMethod(MethodSetup methodSetup)
		=> SetupMethod("", methodSetup);

	/// <summary>
	///     Registers <paramref name="methodSetup" /> for the given <paramref name="scenario" />.
	/// </summary>
	/// <param name="scenario">The scenario name the setup applies to. The setup only applies while <see cref="Scenario" /> equals <paramref name="scenario" /> and does not leak into the default bucket.</param>
	/// <param name="methodSetup">The method setup produced by the fluent <c>Setup.MethodName(...)</c> API.</param>
	public void SetupMethod(string scenario, MethodSetup methodSetup)
		=> Setup.GetOrCreateScenario(scenario).Methods.Add(methodSetup);

	/// <summary>
	///     Registers <paramref name="methodSetup" /> for the default scenario and additionally indexes it by the
	///     generator-emitted <paramref name="memberId" /> for fast dispatch from the proxy method body.
	/// </summary>
	/// <remarks>
	///     The <paramref name="memberId" /> is a compile-time constant emitted by the source generator, one per
	///     mocked member. Reads via <see cref="GetMethodSetupSnapshot(int)" /> are lock-free; writes take an
	///     internal lock and publish a new snapshot via <see cref="Volatile.Write{T}(ref T, T)" />.
	/// </remarks>
	/// <param name="memberId">The generator-emitted member id for the setup's target member.</param>
	/// <param name="methodSetup">The method setup produced by the fluent <c>Setup.MethodName(...)</c> API.</param>
	public void SetupMethod(int memberId, MethodSetup methodSetup)
	{
		SetupMethod(methodSetup);
		AppendToMemberIdBucket(memberId, methodSetup);
	}

	/// <summary>
	///     Registers <paramref name="methodSetup" /> for the given <paramref name="scenario" />. When
	///     <paramref name="scenario" /> is the default scope, the setup is additionally indexed by
	///     <paramref name="memberId" /> for fast dispatch.
	/// </summary>
	/// <param name="memberId">The generator-emitted member id for the setup's target member.</param>
	/// <param name="scenario">The scenario name the setup applies to.</param>
	/// <param name="methodSetup">The method setup produced by the fluent <c>Setup.MethodName(...)</c> API.</param>
	public void SetupMethod(int memberId, string scenario, MethodSetup methodSetup)
	{
		SetupMethod(scenario, methodSetup);
		if (string.IsNullOrEmpty(scenario))
		{
			AppendToMemberIdBucket(memberId, methodSetup);
		}
	}

	private void AppendToMemberIdBucket(int memberId, MethodSetup methodSetup)
	{
		lock (_setupsByMemberIdLock)
		{
			MethodSetup[]?[]? oldTable = _setupsByMemberId;
			int requiredLen = memberId + 1;
			int newLen = oldTable is null ? requiredLen : Math.Max(oldTable.Length, requiredLen);
			MethodSetup[]?[] newTable;
			if (oldTable is null || newLen > oldTable.Length)
			{
				newTable = new MethodSetup[newLen][];
				if (oldTable is not null)
				{
					Array.Copy(oldTable, newTable, oldTable.Length);
				}
			}
			else
			{
				newTable = oldTable;
			}

			MethodSetup[]? existing = newTable[memberId];
			MethodSetup[] bucket;
			if (existing is null || existing.Length == 0)
			{
				bucket = new[]
				{
					methodSetup,
				};
			}
			else
			{
				bucket = new MethodSetup[existing.Length + 1];
				Array.Copy(existing, bucket, existing.Length);
				bucket[existing.Length] = methodSetup;
			}

			newTable[memberId] = bucket;
			Volatile.Write(ref _setupsByMemberId, newTable);
		}
	}

	/// <summary>
	///     Registers <paramref name="propertySetup" /> for the default scenario.
	/// </summary>
	/// <param name="propertySetup">The property setup produced by the fluent <c>Setup.PropertyName</c> API.</param>
	public void SetupProperty(PropertySetup propertySetup)
		=> SetupProperty("", propertySetup);

	/// <summary>
	///     Registers <paramref name="propertySetup" /> for the given <paramref name="scenario" />.
	/// </summary>
	/// <param name="scenario">The scenario name the setup applies to. The setup only applies while <see cref="Scenario" /> equals <paramref name="scenario" /> and does not leak into the default bucket.</param>
	/// <param name="propertySetup">The property setup produced by the fluent <c>Setup.PropertyName</c> API.</param>
	public void SetupProperty(string scenario, PropertySetup propertySetup)
		=> Setup.GetOrCreateScenario(scenario).Properties.Add(propertySetup);

	/// <summary>
	///     Registers <paramref name="propertySetup" /> for the default scenario and additionally indexes it by the
	///     generator-emitted <paramref name="memberId" /> for fast dispatch from the proxy property body.
	/// </summary>
	/// <remarks>
	///     The <paramref name="memberId" /> is a compile-time constant emitted by the source generator, one per
	///     mocked property accessor. Reads via <see cref="GetPropertySetupSnapshot(int)" /> are lock-free; writes
	///     take an internal lock and publish via <see cref="Volatile.Write{T}(ref T, T)" />. A non-default setup
	///     never overrides itself with a <see cref="PropertySetup.Default" /> placeholder — mirroring the rules in
	///     <see cref="MockSetups.PropertySetups.Add" />.
	/// </remarks>
	/// <param name="memberId">The generator-emitted member id for the setup's target property.</param>
	/// <param name="propertySetup">The property setup produced by the fluent <c>Setup.PropertyName</c> API.</param>
	public void SetupProperty(int memberId, PropertySetup propertySetup)
	{
		SetupProperty(propertySetup);
		PublishPropertyToMemberIdBucket(memberId, propertySetup);
	}

	/// <summary>
	///     Registers <paramref name="propertySetup" /> for the given <paramref name="scenario" />. When
	///     <paramref name="scenario" /> is the default scope, the setup is additionally indexed by
	///     <paramref name="memberId" /> for fast dispatch.
	/// </summary>
	/// <param name="memberId">The generator-emitted member id for the setup's target property.</param>
	/// <param name="scenario">The scenario name the setup applies to.</param>
	/// <param name="propertySetup">The property setup produced by the fluent <c>Setup.PropertyName</c> API.</param>
	public void SetupProperty(int memberId, string scenario, PropertySetup propertySetup)
	{
		SetupProperty(scenario, propertySetup);
		if (string.IsNullOrEmpty(scenario))
		{
			PublishPropertyToMemberIdBucket(memberId, propertySetup);
		}
	}

	private void PublishPropertyToMemberIdBucket(int memberId, PropertySetup propertySetup)
	{
		lock (_setupsByMemberIdLock)
		{
			PropertySetup?[]? oldTable = _propertySetupsByMemberId;
			int requiredLen = memberId + 1;
			int newLen = oldTable is null ? requiredLen : Math.Max(oldTable.Length, requiredLen);
			PropertySetup?[] newTable;
			if (oldTable is null || newLen > oldTable.Length)
			{
				newTable = new PropertySetup?[newLen];
				if (oldTable is not null)
				{
					Array.Copy(oldTable, newTable, oldTable.Length);
				}
			}
			else
			{
				newTable = oldTable;
			}

			PropertySetup? existing = newTable[memberId];
			// Mirror MockSetups.PropertySetups.Add: never overwrite a user-configured setup with a default placeholder.
			if (existing is not null && existing is not PropertySetup.Default && propertySetup is PropertySetup.Default)
			{
				return;
			}

			newTable[memberId] = propertySetup;
			Volatile.Write(ref _propertySetupsByMemberId, newTable);
		}
	}

	/// <summary>
	///     Registers <paramref name="eventSetup" /> for the default scenario.
	/// </summary>
	/// <param name="eventSetup">The event setup produced by the fluent <c>Setup.EventName</c> API.</param>
	public void SetupEvent(EventSetup eventSetup)
		=> SetupEvent("", eventSetup);

	/// <summary>
	///     Registers <paramref name="eventSetup" /> for the given <paramref name="scenario" />.
	/// </summary>
	/// <param name="scenario">The scenario name the setup applies to. The setup only applies while <see cref="Scenario" /> equals <paramref name="scenario" /> and does not leak into the default bucket.</param>
	/// <param name="eventSetup">The event setup produced by the fluent <c>Setup.EventName</c> API.</param>
	public void SetupEvent(string scenario, EventSetup eventSetup)
		=> Setup.GetOrCreateScenario(scenario).Events.Add(eventSetup);

	/// <summary>
	///     Registers <paramref name="eventSetup" /> for the default scenario and additionally indexes it by the
	///     generator-emitted <paramref name="memberId" /> for fast dispatch from the proxy event subscribe/unsubscribe body.
	/// </summary>
	/// <remarks>
	///     The <paramref name="memberId" /> is the subscribe-side member id emitted by the source generator. A
	///     single <see cref="EventSetup" /> typically wires both subscribe and unsubscribe behavior, so the bucket
	///     is keyed off the subscribe id. Reads via <see cref="GetEventSetupSnapshot(int)" /> are lock-free; writes
	///     take an internal lock and publish a new snapshot via <see cref="Volatile.Write{T}(ref T, T)" />.
	/// </remarks>
	/// <param name="memberId">The generator-emitted subscribe-side member id for the setup's target event.</param>
	/// <param name="eventSetup">The event setup produced by the fluent <c>Setup.EventName</c> API.</param>
	public void SetupEvent(int memberId, EventSetup eventSetup)
	{
		SetupEvent(eventSetup);
		AppendToEventMemberIdBucket(memberId, eventSetup);
	}

	/// <summary>
	///     Registers <paramref name="eventSetup" /> for the given <paramref name="scenario" />. When
	///     <paramref name="scenario" /> is the default scope, the setup is additionally indexed by
	///     <paramref name="memberId" /> for fast dispatch.
	/// </summary>
	/// <param name="memberId">The generator-emitted subscribe-side member id for the setup's target event.</param>
	/// <param name="scenario">The scenario name the setup applies to.</param>
	/// <param name="eventSetup">The event setup produced by the fluent <c>Setup.EventName</c> API.</param>
	public void SetupEvent(int memberId, string scenario, EventSetup eventSetup)
	{
		SetupEvent(scenario, eventSetup);
		if (string.IsNullOrEmpty(scenario))
		{
			AppendToEventMemberIdBucket(memberId, eventSetup);
		}
	}

	private void AppendToEventMemberIdBucket(int memberId, EventSetup eventSetup)
	{
		lock (_setupsByMemberIdLock)
		{
			EventSetup[]?[]? oldTable = _eventSetupsByMemberId;
			int requiredLen = memberId + 1;
			int newLen = oldTable is null ? requiredLen : Math.Max(oldTable.Length, requiredLen);
			EventSetup[]?[] newTable;
			if (oldTable is null || newLen > oldTable.Length)
			{
				newTable = new EventSetup[newLen][];
				if (oldTable is not null)
				{
					Array.Copy(oldTable, newTable, oldTable.Length);
				}
			}
			else
			{
				newTable = oldTable;
			}

			EventSetup[]? existing = newTable[memberId];
			EventSetup[] bucket;
			if (existing is null || existing.Length == 0)
			{
				bucket = new[]
				{
					eventSetup,
				};
			}
			else
			{
				bucket = new EventSetup[existing.Length + 1];
				Array.Copy(existing, bucket, existing.Length);
				bucket[existing.Length] = eventSetup;
			}

			newTable[memberId] = bucket;
			Volatile.Write(ref _eventSetupsByMemberId, newTable);
		}
	}
}
