using System.Collections.Generic;
using System.Linq;
using Mockolate.Interactions;
using Mockolate.Parameters;
using Mockolate.Setup;
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

		await That(result.Select(x => x.Index)).IsEqualTo([0, 1, 2,]);
	}

	[Theory]
	[InlineData(0, 0, 0, 0, "(none)")]
	[InlineData(1, 0, 0, 0, "1 method")]
	[InlineData(2, 0, 0, 0, "2 methods")]
	[InlineData(0, 1, 0, 0, "1 property")]
	[InlineData(0, 2, 0, 0, "2 properties")]
	[InlineData(0, 0, 1, 0, "1 event")]
	[InlineData(0, 0, 2, 0, "2 events")]
	[InlineData(0, 0, 0, 1, "1 indexer")]
	[InlineData(0, 0, 0, 2, "2 indexers")]
	[InlineData(3, 5, 0, 2, "3 methods, 5 properties, 2 indexers")]
	[InlineData(3, 5, 8, 2, "3 methods, 5 properties, 8 events, 2 indexers")]
	public async Task ToString_Empty_ShouldReturnExpectedValue(
		int methodCount, int propertyCount, int eventCount, int indexerCount, string expected)
	{
		IMyService sut = IMyService.CreateMock();
		IMock mock = (IMock)sut;

		for (int i = 0; i < methodCount; i++)
		{
			mock.MockRegistry.SetupMethod(new ReturnMethodSetup<int>($"my.method{i}"));
		}

		for (int i = 0; i < propertyCount; i++)
		{
			mock.MockRegistry.SetupProperty(new PropertySetup<int>($"my.property{i}"));
		}

		for (int i = 0; i < eventCount; i++)
		{
			mock.MockRegistry.AddEvent($"my.event{i}", this, Helper.GetMethodInfo());
		}

		for (int i = 0; i < indexerCount; i++)
		{
			mock.MockRegistry.SetupIndexer(new IndexerSetup<string, int>(
				new NamedParameter("index1", (IParameter)It.IsAny<int>())));
		}

		string result = mock.MockRegistry.ToString();

		await That(result).IsEqualTo(expected);
	}
}
