using Mockolate.Setup;

namespace Mockolate.Internal.Tests.TestHelpers;

internal sealed class FakePropertySetup : PropertySetup<int>
{
	internal FakePropertySetup(string name) : base(new MockRegistry(MockBehavior.Default, new FastMockInteractions(0)), name) { }
}
