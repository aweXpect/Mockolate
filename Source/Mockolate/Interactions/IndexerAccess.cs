using Mockolate.Setup;

namespace Mockolate.Interactions;

/// <summary>
///     An access of an indexer.
/// </summary>
#if !DEBUG
[System.Diagnostics.DebuggerNonUserCode]
#endif
public abstract class IndexerAccess : IInteraction
{
	/// <summary>
	///     Indicates whether this access is a setter access.
	/// </summary>
	public abstract bool IsSetter { get; }

	/// <summary>
	///     Attempts to find a previously stored value for this access inside the given <paramref name="storage" />.
	/// </summary>
	public abstract bool TryFindStoredValue<T>(ValueStorage storage, out T value);

	/// <summary>
	///     Stores the given <paramref name="value" /> for this access inside the given <paramref name="storage" />.
	/// </summary>
	public abstract void StoreValue<T>(ValueStorage storage, T value);
}
