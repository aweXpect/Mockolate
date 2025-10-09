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

			mock.Object[1] = "foo";
			string result1 = mock.Object[1, 2];
			string result2 = mock.Object[2, 1];

			await That(result1).IsNull();
			await That(result2).IsNull();
		}

		[Fact]
		public async Task ShouldSupportNullAsParameter()
		{
			Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
			IMock sut = mock;

			mock.Object[null, 2] = "foo";
			string result1 = mock.Object[null, 2];
			string result2 = mock.Object[0, 2];

			await That(result1).IsEqualTo("foo");
			await That(result2).IsNull();
		}

		[Fact]
		public async Task ShouldUseInitializedValue()
		{
			Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
			IMock sut = mock;
			mock.SetupIndexer(With.Any<int>()).InitializeWith(2, "foo");

			string result1 = mock.Object[2];
			string result2 = mock.Object[3];

			await That(result1).IsEqualTo("foo");
			await That(result2).IsNull();
		}

		[Fact]
		public async Task ThreeLevels_ShouldUseInitializedValue()
		{
			Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
			IMock sut = mock;
			mock.SetupIndexer(With.Any<string>(), With.Any<int>(), With.Any<int>()).InitializeWith("foo", 1, 2, 42);

			int result1 = mock.Object["foo", 1, 2];
			int result2 = mock.Object["bar", 1, 2];

			await That(result1).IsEqualTo(42);
			await That(result2).IsEqualTo(0);
		}

		[Fact]
		public async Task ThreeLevels_WithoutSetup_ShouldStoreLastValue()
		{
			Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
			IMock sut = mock;

			int result0 = mock.Object["foo", 1, 2];
			sut.SetIndexer(42, "foo", 1, 2);
			int result1 = mock.Object["foo", 1, 2];
			int result2 = mock.Object["bar", 1, 2];
			int result3 = mock.Object["foo", 2, 2];
			int result4 = mock.Object["foo", 1, 3];

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
			mock.SetupIndexer(With.Any<int?>(), With.Any<int>()).InitializeWith(2, 3, "foo");

			string result1 = mock.Object[2, 3];
			string result2 = mock.Object[null, 4];

			await That(result1).IsEqualTo("foo");
			await That(result2).IsNull();
		}

		[Fact]
		public async Task TwoLevels_WithoutSetup_ShouldStoreLastValue()
		{
			Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
			IMock sut = mock;

			string result0 = mock.Object[1, 2];
			mock.Object[1, 2] = "foo";
			string result1 = mock.Object[1, 2];
			string result2 = mock.Object[2, 2];

			await That(result0).IsNull();
			await That(result1).IsEqualTo("foo");
			await That(result2).IsNull();
		}

		[Fact]
		public async Task WithoutSetup_ShouldStoreLastValue()
		{
			Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
			IMock sut = mock;

			string result0 = mock.Object[1];
			mock.Object[1] = "foo";
			string result1 = mock.Object[1];
			string result2 = mock.Object[2];

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
