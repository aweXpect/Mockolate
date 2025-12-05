using Mockolate.Parameters;
using Mockolate.Setup;
using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests;

public sealed class MockSetupsTests
{
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
		IMyService mock = Mock.Create<IMyService>();
		Mock<IMyService> sut = ((IMockSubject<IMyService>)mock).Mock;

		for (int i = 0; i < methodCount; i++)
		{
			sut.Registrations.SetupMethod(new ReturnMethodSetup<int>($"my.method{i}"));
		}

		for (int i = 0; i < propertyCount; i++)
		{
			sut.Registrations.SetupProperty(new PropertySetup<int>($"my.property{i}"));
		}

		for (int i = 0; i < eventCount; i++)
		{
			sut.Registrations.AddEvent($"my.event{i}", this, Helper.GetMethodInfo());
		}

		for (int i = 0; i < indexerCount; i++)
		{
			sut.Registrations.SetupIndexer(new IndexerSetup<string, int>((IParameter)It.IsAny<int>()));
		}

		string result = sut.Registrations.ToString();

		await That(result).IsEqualTo(expected);
	}
}
