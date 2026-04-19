using Mockolate.Interactions;
using Mockolate.Setup;

namespace Mockolate.Internal.Tests.TestHelpers;

internal sealed class FakeIndexerAccess : IndexerAccess
{
	public override int ParameterCount => 0;

	public override object? GetParameterValueAt(int index) => null;

	protected override IndexerValueStorage? TraverseStorage(IndexerValueStorage? storage, bool createMissing) => Storage;
}
