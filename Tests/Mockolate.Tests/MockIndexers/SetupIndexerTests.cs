using Mockolate.Exceptions;

namespace Mockolate.Tests.MockIndexers;

public sealed partial class SetupIndexerTests
{
	[Fact]
	public async Task OverlappingSetups_ShouldUseLatestMatchingSetup()
	{
		Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
		mock.Setup.Indexer(WithAny<int>()).InitializeWith("foo");
		mock.Setup.Indexer(With(2)).InitializeWith("bar");

		string result1 = mock.Subject[1];
		string result2 = mock.Subject[2];
		string result3 = mock.Subject[3];

		await That(result1).IsEqualTo("foo");
		await That(result2).IsEqualTo("bar");
		await That(result3).IsEqualTo("foo");
	}

	[Fact]
	public async Task OverlappingSetups_WhenGeneralSetupIsLater_ShouldOnlyUseGeneralSetup()
	{
		Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
		mock.Setup.Indexer(With(2)).InitializeWith("bar");
		mock.Setup.Indexer(WithAny<int>()).InitializeWith("foo");

		string result1 = mock.Subject[1];
		string result2 = mock.Subject[2];
		string result3 = mock.Subject[3];

		await That(result1).IsEqualTo("foo");
		await That(result2).IsEqualTo("foo");
		await That(result3).IsEqualTo("foo");
	}

	[Fact]
	public async Task SetOnDifferentLevel_ShouldNotBeUsed()
	{
		Mock<IIndexerService> mock = Mock.Create<IIndexerService>();

		mock.Subject[1] = "foo";
		string result1 = mock.Subject[1, 2];
		string result2 = mock.Subject[2, 1];

		await That(result1).IsEmpty();
		await That(result2).IsEmpty();
	}

	[Fact]
	public async Task ShouldSupportNullAsParameter()
	{
		Mock<IIndexerService> mock = Mock.Create<IIndexerService>();

		mock.Subject[null, 2, 1] = 42;
		int? result1 = mock.Subject[null, 2, 1];
		int? result2 = mock.Subject["", 0, 2];

		await That(result1).IsEqualTo(42);
		await That(result2).IsNull();
	}

	[Fact]
	public async Task ShouldUseInitializedValue()
	{
		Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
		mock.Setup.Indexer(With(2)).InitializeWith("foo");

		string result1 = mock.Subject[2];
		string result2 = mock.Subject[3];

		await That(result1).IsEqualTo("foo");
		await That(result2).IsEmpty();
	}

	[Fact]
	public async Task ThreeLevels_ShouldUseInitializedValue()
	{
		Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
		mock.Setup.Indexer(With("foo"), With(1), With(2)).InitializeWith(42);

		int? result1 = mock.Subject["foo", 1, 2];
		int? result2 = mock.Subject["bar", 1, 2];

		await That(result1).IsEqualTo(42);
		await That(result2).IsNull();
	}

	[Fact]
	public async Task ThreeLevels_WithoutSetup_ShouldStoreLastValue()
	{
		Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
		IMock sut = mock;

		int? result0 = mock.Subject["foo", 1, 2];
		sut.SetIndexer(42, "foo", 1, 2);
		int? result1 = mock.Subject["foo", 1, 2];
		int? result2 = mock.Subject["bar", 1, 2];
		int? result3 = mock.Subject["foo", 2, 2];
		int? result4 = mock.Subject["foo", 1, 3];

		await That(result0).IsNull();
		await That(result1).IsEqualTo(42);
		await That(result2).IsNull();
		await That(result3).IsNull();
		await That(result4).IsNull();
	}

	[Fact]
	public async Task TwoLevels_ShouldUseInitializedValue()
	{
		Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
		mock.Setup.Indexer(With(2), With(3)).InitializeWith("foo");

		string result1 = mock.Subject[2, 3];
		string result2 = mock.Subject[1, 4];

		await That(result1).IsEqualTo("foo");
		await That(result2).IsEmpty();
	}

	[Fact]
	public async Task TwoLevels_WithoutSetup_ShouldStoreLastValue()
	{
		Mock<IIndexerService> mock = Mock.Create<IIndexerService>();

		string result0 = mock.Subject[1, 2];
		mock.Subject[1, 2] = "foo";
		string result1 = mock.Subject[1, 2];
		string result2 = mock.Subject[2, 2];

		await That(result0).IsEmpty();
		await That(result1).IsEqualTo("foo");
		await That(result2).IsEmpty();
	}

	[Fact]
	public async Task WhenTypeOfGetIndexerDoesNotMatch_ShouldReturnDefaultValue()
	{
		Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
		mock.Setup.Indexer(WithAny<int>()).Returns("foo");
		IMock hiddenMock = mock;

		string result1 = hiddenMock.GetIndexer<string>(null, 1);
		int result2 = hiddenMock.GetIndexer<int>(null, 1);

		await That(result1).IsEqualTo("foo");
		await That(result2).IsEqualTo(0);
	}

	[Fact]
	public async Task WithoutSetup_ShouldStoreLastValue()
	{
		Mock<IIndexerService> mock = Mock.Create<IIndexerService>();

		string result0 = mock.Subject[1];
		mock.Subject[1] = "foo";
		string result1 = mock.Subject[1];
		string result2 = mock.Subject[2];

		await That(result0).IsEmpty();
		await That(result1).IsEqualTo("foo");
		await That(result2).IsEmpty();
	}

	[Fact]
	public async Task WithoutSetup_ThrowWhenNotSetup_ShouldThrowMockNotSetupException()
	{
		Mock<IIndexerService> mock = Mock.Create<IIndexerService>(MockBehavior.Default with { ThrowWhenNotSetup = true});

		void Act()
			=> _ = mock.Subject[null, 1, 2];

		await That(Act).Throws<MockNotSetupException>()
			.WithMessage("The indexer [null, 1, 2] was accessed without prior setup.");
	}

	public interface IIndexerService
	{
		string this[int index] { get; set; }
		string this[int index1, int index2] { get; set; }
		string this[int index1, int index2, int index3] { get; set; }
		string this[int index1, int index2, int index3, int index4] { get; set; }
		string this[int index1, int index2, int index3, int index4, int index5] { get; set; }

		int? this[string? index1, int index2, int index3] { get; set; }
	}
}
