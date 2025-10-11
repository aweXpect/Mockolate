namespace Mockolate.Tests.Setup;

public sealed partial class MockSetupsTests
{
	public sealed class IndexerTests
	{
		[Fact]
		public async Task SetOnDifferentLevel_ShouldNotBeUsed()
		{
			Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
			IMock sut = mock;

			mock.Subject[1] = "foo";
			string result1 = mock.Subject[1, 2];
			string result2 = mock.Subject[2, 1];

			await That(result1).IsNull();
			await That(result2).IsNull();
		}

		[Fact]
		public async Task ShouldSupportNullAsParameter()
		{
			Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
			IMock sut = mock;

			mock.Subject[null, 2] = "foo";
			string result1 = mock.Subject[null, 2];
			string result2 = mock.Subject[0, 2];

			await That(result1).IsEqualTo("foo");
			await That(result2).IsNull();
		}

		[Fact]
		public async Task ShouldUseInitializedValue()
		{
			Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
			IMock sut = mock;
			mock.Setup.Indexer(2).InitializeWith("foo");

			string result1 = mock.Subject[2];
			string result2 = mock.Subject[3];

			await That(result1).IsEqualTo("foo");
			await That(result2).IsNull();
		}

		[Fact]
		public async Task ThreeLevels_ShouldUseInitializedValue()
		{
			Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
			IMock sut = mock;
			mock.Setup.Indexer("foo", 1, 2).InitializeWith(42);

			int result1 = mock.Subject["foo", 1, 2];
			int result2 = mock.Subject["bar", 1, 2];

			await That(result1).IsEqualTo(42);
			await That(result2).IsEqualTo(0);
		}

		[Fact]
		public async Task ThreeLevels_WithoutSetup_ShouldStoreLastValue()
		{
			Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
			IMock sut = mock;

			int result0 = mock.Subject["foo", 1, 2];
			sut.SetIndexer(42, "foo", 1, 2);
			int result1 = mock.Subject["foo", 1, 2];
			int result2 = mock.Subject["bar", 1, 2];
			int result3 = mock.Subject["foo", 2, 2];
			int result4 = mock.Subject["foo", 1, 3];

			await That(result0).IsEqualTo(default(int));
			await That(result1).IsEqualTo(42);
			await That(result2).IsEqualTo(default(int));
			await That(result3).IsEqualTo(default(int));
			await That(result4).IsEqualTo(default(int));
		}

		[Fact]
		public async Task TwoLevels_ShouldUseInitializedValue()
		{
			Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
			IMock sut = mock;
			mock.Setup.Indexer(2, 3).InitializeWith("foo");

			string result1 = mock.Subject[2, 3];
			string result2 = mock.Subject[null, 4];

			await That(result1).IsEqualTo("foo");
			await That(result2).IsNull();
		}

		[Fact]
		public async Task TwoLevels_WithoutSetup_ShouldStoreLastValue()
		{
			Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
			IMock sut = mock;

			string result0 = mock.Subject[1, 2];
			mock.Subject[1, 2] = "foo";
			string result1 = mock.Subject[1, 2];
			string result2 = mock.Subject[2, 2];

			await That(result0).IsNull();
			await That(result1).IsEqualTo("foo");
			await That(result2).IsNull();
		}

		[Fact]
		public async Task WithoutSetup_ShouldStoreLastValue()
		{
			Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
			IMock sut = mock;

			string result0 = mock.Subject[1];
			mock.Subject[1] = "foo";
			string result1 = mock.Subject[1];
			string result2 = mock.Subject[2];

			await That(result0).IsNull();
			await That(result1).IsEqualTo("foo");
			await That(result2).IsNull();
		}

		public interface IIndexerService
		{
			string this[int index] { get; set; }
			string this[int? index1, int index2] { get; set; }
			int this[string index1, int index2, int index3] { get; set; }
		}
	}
}
