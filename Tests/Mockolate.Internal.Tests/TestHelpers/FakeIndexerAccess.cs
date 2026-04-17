using Mockolate.Interactions;

namespace Mockolate.Internal.Tests.TestHelpers;

internal sealed class FakeIndexerAccess : IndexerAccess
{
	public override bool IsSetter => false;

	public override int ParameterCount => 0;

	public override object? GetParameterValueAt(int index) => null;
}
