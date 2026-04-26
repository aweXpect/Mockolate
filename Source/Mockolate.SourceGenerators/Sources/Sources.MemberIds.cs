using System.Text;
using Mockolate.SourceGenerators.Entities;
using Event = Mockolate.SourceGenerators.Entities.Event;

namespace Mockolate.SourceGenerators.Sources;

internal static partial class Sources
{
	/// <summary>
	///     Emits a member-id-keyed lookup of the latest matching <paramref name="methodSetupType" /> setup,
	///     with a slow-path fallback for cases the fast path cannot cover.
	/// </summary>
	/// <remarks>
	///     <para>
	///         Fast path (default scope only): walks the lock-free <c>GetMethodSetupSnapshot</c> array in
	///         reverse and runs <c>Matches({matchArgs})</c> directly. Eliminates the per-call closure
	///         allocation, the string-name comparison, and the lock taken by the legacy
	///         <c>MethodSetups.GetMatching</c> walk.
	///     </para>
	///     <para>
	///         Slow-path fallback: invoked when an active scenario is set or when the fast path finds no
	///         match. The member-id array is only populated by the <c>SetupMethod(int, ...)</c> overloads
	///         emitted by the generator; setups registered through the hand-written
	///         <c>SetupMethod(MethodSetup)</c> overload (e.g. the <c>HttpClientExtensions.SetupMethod</c>
	///         pipeline) live only in the string-keyed list, so the fallback also covers them.
	///     </para>
	/// </remarks>
#pragma warning disable S107 // Methods should not have too many parameters
	internal static void EmitFastMethodSetupLookup(StringBuilder sb, string indent, string mockRegistry,
		string methodSetup, string methodSetupType, string memberIdRef, string uniqueNameString, string matchArgs,
		bool isGeneric)
#pragma warning restore S107
	{
		// All loop-locals carry the unique `methodSetup` suffix so they inherit its
		// parameter-dedup property and stay distinct across nested scopes.
		string snapshotVar = $"snapshot_{methodSetup}";
		string nameVar = $"name_{methodSetup}";
		string indexVar = $"i_{methodSetup}";
		string itemVar = $"s_{methodSetup}";

		sb.Append(indent).Append(methodSetupType).Append("? ").Append(methodSetup).Append(" = null;").AppendLine();
		sb.Append(indent).Append("if (string.IsNullOrEmpty(").Append(mockRegistry).Append(".Scenario))").AppendLine();
		sb.Append(indent).Append('{').AppendLine();
		sb.Append(indent).Append("\tglobal::Mockolate.Setup.MethodSetup[]? ").Append(snapshotVar)
			.Append(" = ").Append(mockRegistry).Append(".GetMethodSetupSnapshot(").Append(memberIdRef).Append(");")
			.AppendLine();
		if (isGeneric)
		{
			// A single member id covers every instantiation of a generic method, so the snapshot
			// can hold setups whose closed-generic Name differs from this call site's instantiation.
			// Pre-compute the runtime name and filter the bucket by setup.Name to keep lookups
			// instantiation-scoped.
			sb.Append(indent).Append("\tstring ").Append(nameVar).Append(" = ")
				.Append(uniqueNameString).Append(';').AppendLine();
		}

		sb.Append(indent).Append("\tif (").Append(snapshotVar).Append(" is not null)").AppendLine();
		sb.Append(indent).Append("\t{").AppendLine();
		sb.Append(indent).Append("\t\tfor (int ").Append(indexVar).Append(" = ").Append(snapshotVar)
			.Append(".Length - 1; ").Append(indexVar).Append(" >= 0; ").Append(indexVar).Append("--)").AppendLine();
		sb.Append(indent).Append("\t\t{").AppendLine();
		sb.Append(indent).Append("\t\t\tif (").Append(snapshotVar).Append('[').Append(indexVar)
			.Append("] is ").Append(methodSetupType).Append(' ').Append(itemVar);
		if (isGeneric)
		{
			sb.Append(" && ").Append(itemVar).Append(".Name == ").Append(nameVar);
		}

		sb.Append(" && ").Append(itemVar).Append(".Matches(").Append(matchArgs).Append("))")
			.AppendLine();
		sb.Append(indent).Append("\t\t\t{").AppendLine();
		sb.Append(indent).Append("\t\t\t\t").Append(methodSetup).Append(" = ").Append(itemVar)
			.Append(';').AppendLine();
		sb.Append(indent).Append("\t\t\t\tbreak;").AppendLine();
		sb.Append(indent).Append("\t\t\t}").AppendLine();
		sb.Append(indent).Append("\t\t}").AppendLine();
		sb.Append(indent).Append("\t}").AppendLine();
		sb.Append(indent).Append('}').AppendLine();
		sb.Append(indent).Append("if (").Append(methodSetup).Append(" is null)").AppendLine();
		sb.Append(indent).Append('{').AppendLine();
		sb.Append(indent).Append("\tforeach (").Append(methodSetupType).Append(' ').Append(itemVar).Append(" in ").Append(mockRegistry)
			.Append(".GetMethodSetups<").Append(methodSetupType).Append(">(").Append(uniqueNameString).Append("))")
			.AppendLine();
		sb.Append(indent).Append("\t{").AppendLine();
		sb.Append(indent).Append("\t\tif (").Append(itemVar).Append(".Matches(").Append(matchArgs).Append("))").AppendLine();
		sb.Append(indent).Append("\t\t{").AppendLine();
		sb.Append(indent).Append("\t\t\t").Append(methodSetup).Append(" = ").Append(itemVar).Append(';').AppendLine();
		sb.Append(indent).Append("\t\t\tbreak;").AppendLine();
		sb.Append(indent).Append("\t\t}").AppendLine();
		sb.Append(indent).Append("\t}").AppendLine();
		sb.Append(indent).Append('}').AppendLine();
	}

	internal static MemberIdTable ComputeMemberIds(params Class[] classes)
	{
		MemberIdTable table = new();
		foreach (Class @class in classes)
		{
			AddMissing(@class.AllProperties().Where(p => !p.IsIndexer), table.PropertyGetIds, table.AddProperty);
			AddMissing(@class.AllEvents(), table.EventSubscribeIds, table.AddEvent);
			AddMissing(@class.AllProperties().Where(p => p.IsIndexer), table.IndexerGetIds, table.AddIndexer);
			AddMissing(@class.AllMethods(), table.MethodIds, table.AddMethod);
		}

		return table;
	}

	private static void AddMissing<T>(IEnumerable<T> items, Dictionary<T, int> existing, Action<T> add)
		where T : notnull
	{
		foreach (T item in items)
		{
			if (!existing.ContainsKey(item))
			{
				add(item);
			}
		}
	}

	/// <summary>
	///     Per-member id assignments for a generated mock class. The ids are compile-time constants
	///     emitted into the proxy class body; setup registrations and proxy method bodies look up
	///     setups by id instead of by string name for fast dispatch.
	/// </summary>
	/// <remarks>
	///     Ids are assigned in a deterministic order: properties (get + set), events (subscribe +
	///     unsubscribe), indexers (get + set per signature), methods (overloaded methods receive
	///     distinct ids). The same member always receives the same id across regenerations of a
	///     given class shape.
	/// </remarks>
	internal sealed class MemberIdTable
	{
		private readonly List<string> _declarations = new();
		private readonly Dictionary<string, int> _usedIdentifiers = new();

		internal Dictionary<Method, int> MethodIds { get; } = new();
		internal Dictionary<Property, int> PropertyGetIds { get; } = new();
		internal Dictionary<Property, int> PropertySetIds { get; } = new();
		internal Dictionary<Property, int> IndexerGetIds { get; } = new();
		internal Dictionary<Property, int> IndexerSetIds { get; } = new();
		internal Dictionary<Event, int> EventSubscribeIds { get; } = new();
		internal Dictionary<Event, int> EventUnsubscribeIds { get; } = new();

		internal int Count => _declarations.Count;

		internal string GetMethodIdentifier(Method method)
			=> _declarations[MethodIds[method]];

		internal string GetPropertyGetIdentifier(Property property)
			=> _declarations[PropertyGetIds[property]];

		internal string GetPropertySetIdentifier(Property property)
			=> _declarations[PropertySetIds[property]];

		internal string GetIndexerGetIdentifier(Property indexer)
			=> _declarations[IndexerGetIds[indexer]];

		internal string GetIndexerSetIdentifier(Property indexer)
			=> _declarations[IndexerSetIds[indexer]];

		internal string GetEventSubscribeIdentifier(Event @event)
			=> _declarations[EventSubscribeIds[@event]];

		internal string GetEventUnsubscribeIdentifier(Event @event)
			=> _declarations[EventUnsubscribeIds[@event]];

		private int AllocateId(string identifier)
		{
			int id = _declarations.Count;
			_declarations.Add(identifier);
			return id;
		}

		private string UniqueIdentifier(string baseIdentifier)
		{
			string sanitized = Sanitize(baseIdentifier);
			if (!_usedIdentifiers.TryGetValue(sanitized, out int count))
			{
				_usedIdentifiers[sanitized] = 1;
				return sanitized;
			}

			_usedIdentifiers[sanitized] = count + 1;
			return sanitized + "_" + (count + 1);
		}

		internal void AddProperty(Property property)
		{
			string identifierGet = UniqueIdentifier(property.Name + "_Get");
			int getId = AllocateId("MemberId_" + identifierGet);
			PropertyGetIds[property] = getId;

			string identifierSet = UniqueIdentifier(property.Name + "_Set");
			int setId = AllocateId("MemberId_" + identifierSet);
			PropertySetIds[property] = setId;
		}

		internal void AddIndexer(Property indexer)
		{
			string keySignature = BuildIndexerSignatureSuffix(indexer);
			string identifierGet = UniqueIdentifier("Indexer" + keySignature + "_Get");
			int getId = AllocateId("MemberId_" + identifierGet);
			IndexerGetIds[indexer] = getId;

			string identifierSet = UniqueIdentifier("Indexer" + keySignature + "_Set");
			int setId = AllocateId("MemberId_" + identifierSet);
			IndexerSetIds[indexer] = setId;
		}

		internal void AddEvent(Event @event)
		{
			string identifierSub = UniqueIdentifier(@event.Name + "_Subscribe");
			int subId = AllocateId("MemberId_" + identifierSub);
			EventSubscribeIds[@event] = subId;

			string identifierUnsub = UniqueIdentifier(@event.Name + "_Unsubscribe");
			int unsubId = AllocateId("MemberId_" + identifierUnsub);
			EventUnsubscribeIds[@event] = unsubId;
		}

		internal void AddMethod(Method method)
		{
			string identifier = UniqueIdentifier(method.Name);
			int id = AllocateId("MemberId_" + identifier);
			MethodIds[method] = id;
		}

		internal void Emit(StringBuilder sb, string indent)
		{
			if (_declarations.Count == 0)
			{
				sb.Append(indent).Append("internal const int MemberCount = 0;").AppendLine();
				return;
			}

			for (int i = 0; i < _declarations.Count; i++)
			{
				sb.Append(indent).Append("internal const int ").Append(_declarations[i])
					.Append(" = ").Append(i).Append(';').AppendLine();
			}

			sb.Append(indent).Append("internal const int MemberCount = ").Append(_declarations.Count)
				.Append(';').AppendLine();
		}

		private static string BuildIndexerSignatureSuffix(Property indexer)
		{
			if (indexer.IndexerParameters is null || indexer.IndexerParameters.Value.Count == 0)
			{
				return string.Empty;
			}

			StringBuilder sb = new();
			foreach (MethodParameter parameter in indexer.IndexerParameters.Value)
			{
				sb.Append('_').Append(Sanitize(parameter.Type.Fullname));
			}

			return sb.ToString();
		}

		private static string Sanitize(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return "_";
			}

			StringBuilder sb = new(value.Length);
			foreach (char c in value)
			{
				if (char.IsLetterOrDigit(c))
				{
					sb.Append(c);
				}
				else if (c == '_')
				{
					sb.Append('_');
				}
				else
				{
					sb.Append('_');
				}
			}

			if (sb.Length == 0 || char.IsDigit(sb[0]))
			{
				sb.Insert(0, '_');
			}

			return sb.ToString();
		}
	}
}
