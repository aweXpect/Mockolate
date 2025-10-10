namespace Mockolate.Tests.Setup;

public sealed class IndexerSetupTests
{
	[Fact]
	public async Task Callback_WhenLengthDoesNotMatch_ShouldIgnore()
	{
		int callCount = 0;
		Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
		IMock sut = mock;
		mock.SetupIndexer(With.Value(2)).OnGet(() => { callCount++; });

		_ = mock.Object[2, 3];
		_ = mock.Object[2];

		await That(callCount).IsEqualTo(1);
	}

	[Fact]
	public async Task ShouldExecuteAllGetterCallbacks()
	{
		int callCount1 = 0;
		int callCount2 = 0;
		int callCount3 = 0;
		Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
		IMock sut = mock;
		mock.SetupIndexer(With.Value(2)).OnGet(() => { callCount1++; })
				.OnGet(v => { callCount2 += v; })
				.OnGet(() => { callCount3++; });

		_ = mock.Object[2];
		_ = mock.Object[2];

		await That(callCount1).IsEqualTo(2);
		await That(callCount2).IsEqualTo(4);
		await That(callCount3).IsEqualTo(2);
	}

	[Fact]
	public async Task ShouldExecuteAllSetterCallbacks()
	{
		int callCount1 = 0;
		int callCount2 = 0;
		int callCount3 = 0;
		Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
		IMock sut = mock;
		mock.SetupIndexer(With.Any<int>()).OnSet(() => { callCount1++; })
				.OnSet((_, v) => { callCount2 += v; })
				.OnSet(_ => { callCount3++; });

		mock.Object[2] = "foo";
		mock.Object[2] = "bar";

		await That(callCount1).IsEqualTo(2);
		await That(callCount2).IsEqualTo(4);
		await That(callCount3).IsEqualTo(2);
	}

	[Fact]
	public async Task ShouldExecuteGetterCallbacks()
	{
		int callCount = 0;
		Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
		IMock sut = mock;
		mock.SetupIndexer(With.Matching<int>(i => i < 4)).OnGet(() => { callCount++; });

		_ = mock.Object[1];
		_ = mock.Object[2];
		_ = mock.Object[3];
		_ = mock.Object[4];
		_ = mock.Object[5];
		_ = mock.Object[6];

		await That(callCount).IsEqualTo(3);
	}

	[Fact]
	public async Task ShouldExecuteGetterCallbacksWithValue()
	{
		int callCount = 0;
		Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
		IMock sut = mock;
		mock.SetupIndexer(With.Matching<int>(i => i < 4)).OnGet(v => { callCount += v; });

		_ = mock.Object[1];
		_ = mock.Object[2];
		_ = mock.Object[3];
		_ = mock.Object[4];
		_ = mock.Object[5];

		await That(callCount).IsEqualTo(6);
	}

	[Fact]
	public async Task ShouldExecuteSetterCallbacks()
	{
		int callCount = 0;
		Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
		IMock sut = mock;
		mock.SetupIndexer(With.Matching<int>(i => i < 4)).OnSet(_ => { callCount++; });

		mock.Object[1] = "";
		mock.Object[2] = "";
		mock.Object[3] = "";
		mock.Object[4] = "";
		mock.Object[5] = "";
		mock.Object[6] = "";

		await That(callCount).IsEqualTo(3);
	}

	[Fact]
	public async Task ShouldExecuteSetterCallbacksWithoutAnyValue()
	{
		int callCount = 0;
		Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
		IMock sut = mock;
		mock.SetupIndexer(With.Matching<int>(i => i < 4)).OnSet(() => { callCount++; });

		mock.Object[1] = "";
		mock.Object[2] = "";
		mock.Object[3] = "";
		mock.Object[4] = "";
		mock.Object[5] = "";

		await That(callCount).IsEqualTo(3);
	}

	public interface IIndexerService
	{
		string this[int index] { get; set; }
		string this[int? index1, int index2] { get; set; }
		int this[string index1, int index2, int index3] { get; set; }
	}
}
