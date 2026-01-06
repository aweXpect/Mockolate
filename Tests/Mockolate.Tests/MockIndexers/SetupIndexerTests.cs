using System.Collections.Generic;
using Mockolate.Exceptions;
using Mockolate.Setup;

namespace Mockolate.Tests.MockIndexers;

public sealed partial class SetupIndexerTests
{
	[Fact]
	public async Task MultipleValues_ShouldAllStoreValues()
	{
		IIndexerService mock = Mock.Create<IIndexerService>();

		mock[1] = "a";
		mock[2] = "b";

		string result1A = mock[1];
		string result2A = mock[2];

		mock[1] = "x";
		mock[2] = "y";

		string result1B = mock[1];
		string result2B = mock[2];

		await That(result1A).IsEqualTo("a");
		await That(result2A).IsEqualTo("b");
		await That(result1B).IsEqualTo("x");
		await That(result2B).IsEqualTo("y");
	}

	[Fact]
	public async Task OverlappingSetups_ShouldUseLatestMatchingSetup()
	{
		IIndexerService mock = Mock.Create<IIndexerService>();
		mock.SetupMock.Indexer(It.IsAny<int>()).InitializeWith("foo");
		mock.SetupMock.Indexer(It.Is(2)).InitializeWith("bar");

		string result1 = mock[1];
		string result2 = mock[2];
		string result3 = mock[3];

		await That(result1).IsEqualTo("foo");
		await That(result2).IsEqualTo("bar");
		await That(result3).IsEqualTo("foo");
	}

	[Fact]
	public async Task OverlappingSetups_WhenGeneralSetupIsLater_ShouldOnlyUseGeneralSetup()
	{
		IIndexerService mock = Mock.Create<IIndexerService>();
		mock.SetupMock.Indexer(It.Is(2)).InitializeWith("bar");
		mock.SetupMock.Indexer(It.IsAny<int>()).InitializeWith("foo");

		string result1 = mock[1];
		string result2 = mock[2];
		string result3 = mock[3];

		await That(result1).IsEqualTo("foo");
		await That(result2).IsEqualTo("foo");
		await That(result3).IsEqualTo("foo");
	}

	[Fact]
	public async Task Parameter_Do_ShouldExecuteCallback()
	{
		List<string> capturedValues = [];
		IIndexerService mock = Mock.Create<IIndexerService>();
		mock.SetupMock.Indexer(It.IsAny<string>().Do(v => capturedValues.Add(v)), It.Is(1), It.Is(2))
			.InitializeWith(42);

		_ = mock["foo", 1, 2];
		_ = mock["bar", 1, 2];

		await That(capturedValues).IsEqualTo(["foo", "bar",]);
	}

	[Fact]
	public async Task Parameter_Do_ShouldOnlyExecuteCallbackWhenAllParametersMatch()
	{
		List<string> capturedValues = [];
		IIndexerService mock = Mock.Create<IIndexerService>();
		mock.SetupMock.Indexer(It.IsAny<string>().Do(v => capturedValues.Add(v)), It.Is(1), It.Is(2))
			.InitializeWith(42);

		_ = mock["foo", 1, 2];
		_ = mock["bar", 2, 2];

		await That(capturedValues).IsEqualTo(["foo",]);
	}

	[Fact]
	public async Task SetOnDifferentLevel_ShouldNotBeUsed()
	{
		IIndexerService mock = Mock.Create<IIndexerService>();

		mock[1] = "foo";
		string result1 = mock[1, 2];
		string result2 = mock[2, 1];

		await That(result1).IsEmpty();
		await That(result2).IsEmpty();
	}

	[Fact]
	public async Task ShouldSupportNullAsParameter()
	{
		IIndexerService mock = Mock.Create<IIndexerService>();

		mock[null, 2, 1] = 42;
		int? result1 = mock[null, 2, 1];
		int? result2 = mock["", 0, 2];

		await That(result1).IsEqualTo(42);
		await That(result2).IsNull();
	}

	[Fact]
	public async Task ShouldUseInitializedValue()
	{
		IIndexerService mock = Mock.Create<IIndexerService>();
		mock.SetupMock.Indexer(It.Is(2)).InitializeWith("foo");

		string result1 = mock[2];
		string result2 = mock[3];

		await That(result1).IsEqualTo("foo");
		await That(result2).IsEmpty();
	}

	[Fact]
	public async Task ThreeLevels_ShouldUseInitializedValue()
	{
		IIndexerService mock = Mock.Create<IIndexerService>();
		mock.SetupMock.Indexer(It.Is("foo"), It.Is(1), It.Is(2)).InitializeWith(42);

		int? result1 = mock["foo", 1, 2];
		int? result2 = mock["bar", 1, 2];

		await That(result1).IsEqualTo(42);
		await That(result2).IsNull();
	}

	[Fact]
	public async Task ThreeLevels_WithoutSetup_ShouldStoreLastValue()
	{
		IIndexerService mock = Mock.Create<IIndexerService>();
		MockRegistration registration = ((IHasMockRegistration)mock).Registrations;

		int? result0 = mock["foo", 1, 2];
		registration.SetIndexer(42, "foo", 1, 2);
		int? result1 = mock["foo", 1, 2];
		int? result2 = mock["bar", 1, 2];
		int? result3 = mock["foo", 2, 2];
		int? result4 = mock["foo", 1, 3];

		await That(result0).IsNull();
		await That(result1).IsEqualTo(42);
		await That(result2).IsNull();
		await That(result3).IsNull();
		await That(result4).IsNull();
	}

	[Fact]
	public async Task TwoLevels_ShouldUseInitializedValue()
	{
		IIndexerService mock = Mock.Create<IIndexerService>();
		mock.SetupMock.Indexer(It.Is(2), It.Is(3)).InitializeWith("foo");

		string result1 = mock[2, 3];
		string result2 = mock[1, 4];

		await That(result1).IsEqualTo("foo");
		await That(result2).IsEmpty();
	}

	[Fact]
	public async Task TwoLevels_WithoutSetup_ShouldStoreLastValue()
	{
		IIndexerService mock = Mock.Create<IIndexerService>();

		string result0 = mock[1, 2];
		mock[1, 2] = "foo";
		string result1 = mock[1, 2];
		string result2 = mock[2, 2];

		await That(result0).IsEmpty();
		await That(result1).IsEqualTo("foo");
		await That(result2).IsEmpty();
	}

	[Fact]
	public async Task WhenTypeOfGetIndexerDoesNotMatch_ShouldReturnDefaultValue()
	{
		IIndexerService mock = Mock.Create<IIndexerService>();
		mock.SetupMock.Indexer(It.IsAny<int>()).Returns("foo");
		MockRegistration registration = ((IHasMockRegistration)mock).Registrations;

		IndexerSetupResult<string> result1 = registration.GetIndexer<string>(1);
		IndexerSetupResult<int> result2 = registration.GetIndexer<int>(1);

		await That(result1.GetResult(() => "")).IsEqualTo("foo");
		await That(result2.GetResult(() => 0)).IsEqualTo(0);
	}

	[Fact]
	public async Task WithoutSetup_ShouldStoreLastValue()
	{
		IIndexerService mock = Mock.Create<IIndexerService>();

		string result0 = mock[1];
		mock[1] = "foo";
		string result1 = mock[1];
		string result2 = mock[2];

		await That(result0).IsEmpty();
		await That(result1).IsEqualTo("foo");
		await That(result2).IsEmpty();
	}

	[Fact]
	public async Task WithoutSetup_ThrowWhenNotSetup_ShouldThrowMockNotSetupException()
	{
		IIndexerService mock = Mock.Create<IIndexerService>(MockBehavior.Default with
		{
			ThrowWhenNotSetup = true,
		});

		void Act()
		{
			_ = mock[null, 1, 2];
		}

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
