using System.Collections.Generic;
using System.Linq;
using Mockolate.Interactions;
using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests;

public sealed class MockSetupsTests
{
	[Fact]
	public async Task ClearAllInteractions_ShouldResetIndex()
	{
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();
		MockInteractions interactions = ((IMock)sut).MockRegistry.Interactions;

		sut.Dispense("Dark", 1);
		sut.Dispense("Light", 2);
		sut.Mock.ClearAllInteractions();
		sut.Dispense("Dark", 3);
		sut.Dispense("Light", 4);
		sut.Dispense("Milk", 5);
		IReadOnlyCollection<IInteraction> result = interactions.GetUnverifiedInteractions();

		await That(result.Select(x => x.Index!.Value)).IsEqualTo([0, 1, 2,]);
	}
}
