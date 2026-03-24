using System.Reflection;
using Mockolate.Parameters;
using Mockolate.Setup;

namespace Mockolate.Internal.Tests;

public class MockSetupsTests
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
	[InlineData(3, 5, 8, 2, "3 methods, 5 properties, 2 indexers, 8 events")]
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
			mock.MockRegistry.AddEvent($"my.event{i}", this, GetMethodInfo());
		}

		for (int i = 0; i < indexerCount; i++)
		{
			mock.MockRegistry.SetupIndexer(new IndexerSetup<string, int>(
				new NamedParameter("index1", (IParameter)It.IsAny<int>())));
		}

		string result = mock.MockRegistry.Setup.ToString();

		await That(result).IsEqualTo(expected);
	}

	internal interface IMyService
	{
	}

	public static MethodInfo GetMethodInfo()
		=> typeof(MockSetupsTests).GetMethod(nameof(GetMethodInfo), BindingFlags.Static | BindingFlags.Public)!;
}
