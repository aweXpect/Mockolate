using Mockolate.Interactions;
using Mockolate.Setup;

namespace Mockolate.Internal.Tests.TestHelpers;

internal sealed class FakeIndexerAccess : IndexerAccess
{
	public override bool IsSetter => false;

	public override bool TryFindStoredValue<T>(ValueStorage storage, out T value)
	{
		value = default!;
		return false;
	}

	public override void StoreValue<T>(ValueStorage storage, T value)
	{
	}
}
