namespace Mockolate.Tests.Setup;

public sealed partial class MockSetupsTests
{
	public sealed class IndexerTests
	{
		[Fact]
		public async Task WithoutSetup_ShouldStoreLastValue()
		{
			var mock = Mock.Create<IIndexerService>();
			IMock sut = mock;

			var result0 = sut.GetIndexer<string>(1);
			sut.SetIndexer("foo", 1);
			var result1 = sut.GetIndexer<string>(1);
			var result2 = sut.GetIndexer<string>(2);

			await That(result0).IsNull();
			await That(result1).IsEqualTo("foo");
			await That(result2).IsNull();
		}

		[Fact]
		public async Task WithoutSetup_ThreeLevels_ShouldStoreLastValue()
		{
			var mock = Mock.Create<IIndexerService>();
			IMock sut = mock;

			var result0 = sut.GetIndexer<int>("foo", 1, 2);
			sut.SetIndexer(42, "foo", 1, 2);
			var result1 = sut.GetIndexer<int>("foo", 1, 2);
			var result2 = sut.GetIndexer<int>("bar", 1, 2);
			var result3 = sut.GetIndexer<int>("foo", 2, 2);
			var result4 = sut.GetIndexer<int>("foo", 1, 3);

			await That(result0).IsEqualTo(default(int));
			await That(result1).IsEqualTo(42);
			await That(result2).IsEqualTo(default(int));
			await That(result3).IsEqualTo(default(int));
			await That(result4).IsEqualTo(default(int));
		}

		[Fact]
		public async Task WithoutSetup_TwoLevels_ShouldStoreLastValue()
		{
			var mock = Mock.Create<IIndexerService>();
			IMock sut = mock;

			var result0 = sut.GetIndexer<string>(1, 2);
			sut.SetIndexer("foo", 1, 2);
			var result1 = sut.GetIndexer<string>(1, 2);
			var result2 = sut.GetIndexer<string>(2, 2);

			await That(result0).IsNull();
			await That(result1).IsEqualTo("foo");
			await That(result2).IsNull();
		}

		[Fact]
		public async Task WithoutSetup_SetOnDifferentLevel_ShouldNotBeUsed()
		{
			var mock = Mock.Create<IIndexerService>();
			IMock sut = mock;

			sut.SetIndexer("foo", 1);
			var result1 = sut.GetIndexer<string>(1, 2);
			var result2 = sut.GetIndexer<string>(2, 1);

			await That(result1).IsNull();
			await That(result2).IsNull();
		}

		public interface IIndexerService
		{
			string this[int index] { get; set; }
			string this[int index1, int index2] { get; set; }
			int this[string index1, int index2, int index3] { get; set; }
		}
	}
}
