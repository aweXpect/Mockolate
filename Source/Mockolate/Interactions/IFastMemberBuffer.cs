using System.Collections.Generic;

namespace Mockolate.Interactions;

/// <summary>
///     Per-member typed-storage contract for <see cref="FastMockInteractions" />. Each mocked member
///     (method, property getter/setter, indexer getter/setter, event subscribe/unsubscribe) has its
///     own buffer that records calls without going through the shared <see cref="MockInteractions" />
///     list. The contract is arity-agnostic so the runtime never has to pattern-match on the typed
///     buffer to enumerate or clear it.
/// </summary>
public interface IFastMemberBuffer
{
	/// <summary>
	///     The number of recorded interactions in this buffer.
	/// </summary>
	int Count { get; }

	/// <summary>
	///     Resets the buffer to an empty state.
	/// </summary>
	void Clear();

	/// <summary>
	///     Appends a boxed <see cref="IInteraction" /> per recorded call into <paramref name="dest" />,
	///     each tagged with its monotonic sequence number so callers can interleave records from
	///     several buffers in registration order.
	/// </summary>
	/// <param name="dest">Destination list that boxed records are appended to.</param>
	void AppendBoxed(List<(long Seq, IInteraction Interaction)> dest);
}
