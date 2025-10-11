using aweXpect.Core.Initialization;
using aweXpect.Customization;

namespace Mockolate.SourceGenerators.Tests.TestHelpers;

public class Initializer : IAweXpectInitializer
{
	public void Initialize() => Customize.aweXpect.Formatting().MaximumNumberOfCollectionItems.Set(40);
}
