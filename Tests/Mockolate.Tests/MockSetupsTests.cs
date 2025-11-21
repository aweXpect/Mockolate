using Mockolate.Setup;
using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests;

public sealed class MockSetupsTests
{
	[Test]
	[Arguments(0, 0, 0, 0, "(none)")]
	[Arguments(1, 0, 0, 0, "1 method")]
	[Arguments(2, 0, 0, 0, "2 methods")]
	[Arguments(0, 1, 0, 0, "1 property")]
	[Arguments(0, 2, 0, 0, "2 properties")]
	[Arguments(0, 0, 1, 0, "1 event")]
	[Arguments(0, 0, 2, 0, "2 events")]
	[Arguments(0, 0, 0, 1, "1 indexer")]
	[Arguments(0, 0, 0, 2, "2 indexers")]
	[Arguments(3, 5, 0, 2, "3 methods, 5 properties, 2 indexers")]
	[Arguments(3, 5, 8, 2, "3 methods, 5 properties, 8 events, 2 indexers")]
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
			sut.Registrations.SetupProperty($"my.property{i}", new PropertySetup<int>());
		}

		for (int i = 0; i < eventCount; i++)
		{
			sut.Registrations.AddEvent($"my.event{i}", this, Helper.GetMethodInfo());
		}

		for (int i = 0; i < indexerCount; i++)
		{
			sut.Registrations.SetupIndexer(new IndexerSetup<string, int>((IParameter)Any<int>()));
		}

		string result = sut.Registrations.ToString();

		await That(result).IsEqualTo(expected);
	}
}
