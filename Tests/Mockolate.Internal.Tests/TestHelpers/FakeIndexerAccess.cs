using Mockolate.Interactions;
using Mockolate.Parameters;

namespace Mockolate.Internal.Tests.TestHelpers;

internal sealed class FakeIndexerAccess : IndexerAccess
{
	internal FakeIndexerAccess() : base(Array.Empty<INamedParameterValue>()) { }
}
