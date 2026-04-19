#if NET9_0_OR_GREATER
using Mockolate.Parameters;

namespace Mockolate.Setup;

/// <summary>
///     Shared helpers for activating projection-based storage on multi-parameter
///     ref-struct-keyed indexer setups. Centralises the per-slot rules so the arity 2-4
///     hand-written setups and the arity 5+ generator-emitted setups stay consistent.
/// </summary>
internal static class RefStructStorageHelper
{
	/// <summary>
	///     Returns <see langword="true" /> when a slot can contribute to projection-based storage:
	///     either the slot carries a projection matcher, or the slot's type is not a ref struct
	///     (so the caller can supply a boxed raw value as the dispatch key).
	/// </summary>
	internal static bool IsSlotStorageReady<T>(IRefStructProjectionMatch<T>? projection)
		where T : allows ref struct
		=> projection is not null || !typeof(T).IsByRefLike;

	/// <summary>
	///     Resolves the dispatch key for a slot: the projection output if a projection matcher is
	///     present, otherwise the caller-supplied boxed raw value. Returns <see langword="false" />
	///     when neither is available — storage lookup must then bail out.
	/// </summary>
	internal static bool TryResolveKey<T>(IRefStructProjectionMatch<T>? projection, T value, object? rawKey, out object key)
		where T : allows ref struct
	{
		if (projection is not null)
		{
			key = projection.Project(value);
			return true;
		}

		if (rawKey is not null)
		{
			key = rawKey;
			return true;
		}

		key = null!;
		return false;
	}
}
#endif
