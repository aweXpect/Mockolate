using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Mockolate.Exceptions;
using Mockolate.Interactions;
using Mockolate.Parameters;
using Mockolate.Setup;

namespace Mockolate.Tests.MockIndexers;

public sealed partial class SetupIndexerTests
{
	[Fact]
	public async Task Matches_WhenParameterCountDoesNotMatch_ShouldNotInvokeParameterCallbacks()
	{
		IParameter<int> parameter = It.IsAny<int>().Monitor(out IParameterMonitor<int> monitor);

		bool result = MyIndexerSetup.DoesMatch([
			new NamedParameter("foo", (IParameter)parameter),
		], [
			new NamedParameterValue("foo", 4),
			new NamedParameterValue("bar", 5),
		]);

		await That(result).IsFalse();
		await That(monitor.Values).IsEmpty();
	}

	[Fact]
	public async Task Matches_WhenParameterCountMatches_ShouldInvokeParameterCallbacks()
	{
		IParameter<int> parameter = It.IsAny<int>().Monitor(out IParameterMonitor<int> monitor);

		bool result = MyIndexerSetup.DoesMatch([
			new NamedParameter("foo", (IParameter)parameter),
		], [
			new NamedParameterValue("foo", 4),
		]);

		await That(result).IsTrue();
		await That(monitor.Values).IsEqualTo([4,]);
	}

	[Fact]
	public async Task MultipleValues_ShouldAllStoreValues()
	{
		IIndexerService sut = IIndexerService.CreateMock();

		sut[1] = "a";
		sut[2] = "b";

		string result1A = sut[1];
		string result2A = sut[2];

		sut[1] = "x";
		sut[2] = "y";

		string result1B = sut[1];
		string result2B = sut[2];

		await That(result1A).IsEqualTo("a");
		await That(result2A).IsEqualTo("b");
		await That(result1B).IsEqualTo("x");
		await That(result2B).IsEqualTo("y");
	}

	[Fact]
	public async Task OverlappingSetups_ShouldUseLatestMatchingSetup()
	{
		IIndexerService sut = IIndexerService.CreateMock();
		sut.Mock.Setup[It.IsAny<int>()].InitializeWith("foo");
		sut.Mock.Setup[It.Is(2)].InitializeWith("bar");

		string result1 = sut[1];
		string result2 = sut[2];
		string result3 = sut[3];

		await That(result1).IsEqualTo("foo");
		await That(result2).IsEqualTo("bar");
		await That(result3).IsEqualTo("foo");
	}

	[Fact]
	public async Task OverlappingSetups_WhenGeneralSetupIsLater_ShouldOnlyUseGeneralSetup()
	{
		IIndexerService sut = IIndexerService.CreateMock();
		sut.Mock.Setup[It.Is(2)].InitializeWith("bar");
		sut.Mock.Setup[It.IsAny<int>()].InitializeWith("foo");

		string result1 = sut[1];
		string result2 = sut[2];
		string result3 = sut[3];

		await That(result1).IsEqualTo("foo");
		await That(result2).IsEqualTo("foo");
		await That(result3).IsEqualTo("foo");
	}

	[Fact]
	public async Task Parameter_Do_ShouldExecuteCallback()
	{
		List<string> capturedValues = [];
		IIndexerService sut = IIndexerService.CreateMock();
		sut.Mock.Setup[It.IsAny<string>().Do(v => capturedValues.Add(v)), It.Is(1), It.Is(2)]
			.InitializeWith(42);

		_ = sut["foo", 1, 2];
		_ = sut["bar", 1, 2];

		await That(capturedValues).IsEqualTo(["foo", "bar",]);
	}

	[Fact]
	public async Task Parameter_Do_ShouldOnlyExecuteCallbackWhenAllParametersMatch()
	{
		List<string> capturedValues = [];
		IIndexerService sut = IIndexerService.CreateMock();
		sut.Mock.Setup[It.IsAny<string>().Do(v => capturedValues.Add(v)), It.Is(1), It.Is(2)]
			.InitializeWith(42);

		_ = sut["foo", 1, 2];
		_ = sut["bar", 2, 2];

		await That(capturedValues).IsEqualTo(["foo",]);
	}

	[Fact]
	public async Task SetOnDifferentLevel_ShouldNotBeUsed()
	{
		IIndexerService sut = IIndexerService.CreateMock();

		sut[1] = "foo";
		string result1 = sut[1, 2];
		string result2 = sut[2, 1];

		await That(result1).IsEmpty();
		await That(result2).IsEmpty();
	}

	[Fact]
	public async Task ShouldSupportNullAsParameter()
	{
		IIndexerService sut = IIndexerService.CreateMock();

		sut[null, 2, 1] = 42;
		int? result1 = sut[null, 2, 1];
		int? result2 = sut["", 0, 2];

		await That(result1).IsEqualTo(42);
		await That(result2).IsNull();
	}

	[Fact]
	public async Task ShouldUseInitializedValue()
	{
		IIndexerService sut = IIndexerService.CreateMock();
		sut.Mock.Setup[It.Is(2)].InitializeWith("foo");

		string result1 = sut[2];
		string result2 = sut[3];

		await That(result1).IsEqualTo("foo");
		await That(result2).IsEmpty();
	}

	[Fact]
	public async Task ThreeLevels_ShouldUseInitializedValue()
	{
		IIndexerService sut = IIndexerService.CreateMock();
		sut.Mock.Setup[It.Is("foo"), It.Is(1), It.Is(2)].InitializeWith(42);

		int? result1 = sut["foo", 1, 2];
		int? result2 = sut["bar", 1, 2];

		await That(result1).IsEqualTo(42);
		await That(result2).IsNull();
	}

	[Fact]
	public async Task ThreeLevels_WithoutSetup_ShouldStoreLastValue()
	{
		IIndexerService sut = IIndexerService.CreateMock();
		MockRegistry registration = ((IMock)sut).MockRegistry;

		int? result0 = sut["foo", 1, 2];
		registration.SetIndexer(42,
			new NamedParameterValue("index1", "foo"),
			new NamedParameterValue("index2", 1),
			new NamedParameterValue("index3", 2));
		int? result1 = sut["foo", 1, 2];
		int? result2 = sut["bar", 1, 2];
		int? result3 = sut["foo", 2, 2];
		int? result4 = sut["foo", 1, 3];

		await That(result0).IsNull();
		await That(result1).IsEqualTo(42);
		await That(result2).IsNull();
		await That(result3).IsNull();
		await That(result4).IsNull();
	}

	[Fact]
	public async Task TwoLevels_ShouldUseInitializedValue()
	{
		IIndexerService sut = IIndexerService.CreateMock();
		sut.Mock.Setup[It.Is(2), It.Is(3)].InitializeWith("foo");

		string result1 = sut[2, 3];
		string result2 = sut[1, 4];

		await That(result1).IsEqualTo("foo");
		await That(result2).IsEmpty();
	}

	[Fact]
	public async Task TwoLevels_WithoutSetup_ShouldStoreLastValue()
	{
		IIndexerService sut = IIndexerService.CreateMock();

		string result0 = sut[1, 2];
		sut[1, 2] = "foo";
		string result1 = sut[1, 2];
		string result2 = sut[2, 2];

		await That(result0).IsEmpty();
		await That(result1).IsEqualTo("foo");
		await That(result2).IsEmpty();
	}

	[Fact]
	public async Task WhenNameOfGetIndexerDoesNotMatch_ShouldReturnDefaultValue()
	{
		IIndexerService sut = IIndexerService.CreateMock();
		sut.Mock.Setup[It.IsAny<int>()].Returns("foo");
		MockRegistry registration = ((IMock)sut).MockRegistry;

		IndexerSetupResult<string> result1 = registration.GetIndexer<string>(new NamedParameterValue("index", 1));
		IndexerSetupResult<string> result2 = registration.GetIndexer<string>(new NamedParameterValue("other", 1));

		await That(result1.GetResult(() => "")).IsEqualTo("foo");
		await That(result2.GetResult(() => "")).IsEqualTo("");
	}

	[Fact]
	public async Task WhenTypeOfGetIndexerDoesNotMatch_ShouldReturnDefaultValue()
	{
		IIndexerService sut = IIndexerService.CreateMock();
		sut.Mock.Setup[It.IsAny<int>()].Returns("foo");
		MockRegistry registration = ((IMock)sut).MockRegistry;

		IndexerSetupResult<string> result1 = registration.GetIndexer<string>(new NamedParameterValue("index", 1));
		IndexerSetupResult<int> result2 = registration.GetIndexer<int>(new NamedParameterValue("index", 1));

		await That(result1.GetResult(() => "")).IsEqualTo("foo");
		await That(result2.GetResult(() => 0)).IsEqualTo(0);
	}

	[Fact]
	public async Task WithoutSetup_ShouldStoreLastValue()
	{
		IIndexerService sut = IIndexerService.CreateMock();

		string result0 = sut[1];
		sut[1] = "foo";
		string result1 = sut[1];
		string result2 = sut[2];

		await That(result0).IsEmpty();
		await That(result1).IsEqualTo("foo");
		await That(result2).IsEmpty();
	}

	[Fact]
	public async Task WithoutSetup_ThrowWhenNotSetup_ShouldThrowMockNotSetupException()
	{
		IIndexerService mock = IIndexerService.CreateMock(MockBehavior.Default with
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

	public class IndexerWith1Parameter
	{
		[Fact]
		public async Task WithExplicitParameter_ShouldWork()
		{
			IIndexerService sut = IIndexerService.CreateMock();
			sut.Mock.Setup[1].InitializeWith("foo");

			string result1 = sut[1];
			string result2 = sut[2];

			await That(result1).IsEqualTo("foo");
			await That(result2).IsEmpty();
		}
	}

	public class IndexerWith2Parameters
	{
		[Fact]
		public async Task WithExplicitParameter1_ShouldWork()
		{
			IIndexerService sut = IIndexerService.CreateMock();
			sut.Mock.Setup[1, It.IsAny<int>()].InitializeWith("foo");

			string result1 = sut[1, 10];
			string result2 = sut[2, 10];

			await That(result1).IsEqualTo("foo");
			await That(result2).IsEmpty();
		}

		[Fact]
		public async Task WithExplicitParameter2_ShouldWork()
		{
			IIndexerService sut = IIndexerService.CreateMock();
			sut.Mock.Setup[It.IsAny<int>(), 1].InitializeWith("foo");

			string result1 = sut[10, 1];
			string result2 = sut[10, 2];

			await That(result1).IsEqualTo("foo");
			await That(result2).IsEmpty();
		}

		[Fact]
		public async Task WithExplicitParameters_ShouldWork()
		{
			IIndexerService sut = IIndexerService.CreateMock();
			sut.Mock.Setup[1, 2].InitializeWith("foo");

			string result1 = sut[1, 2];
			string result2 = sut[1, 10];
			string result3 = sut[10, 2];

			await That(result1).IsEqualTo("foo");
			await That(result2).IsEmpty();
			await That(result3).IsEmpty();
		}
	}

	public class MyIndexerSetup : IndexerSetup
	{
		public static bool DoesMatch(NamedParameter[] namedParameters, NamedParameterValue[] values)
			=> Matches(namedParameters, values);

		protected override T ExecuteGetterCallback<T>(IndexerGetterAccess indexerGetterAccess, T value,
			MockBehavior behavior)
			=> throw new NotSupportedException();

		protected override void ExecuteSetterCallback<T>(IndexerSetterAccess indexerSetterAccess, T value,
			MockBehavior behavior)
			=> throw new NotSupportedException();

		protected override bool IsMatch(NamedParameterValue[] parameters)
			=> throw new NotSupportedException();

		protected override bool? GetSkipBaseClass()
			=> throw new NotSupportedException();

		protected override void GetInitialValue<T>(MockBehavior behavior, Func<T> defaultValueGenerator,
			NamedParameterValue[] parameters,
			[NotNullWhen(true)] out T value)
			=> throw new NotSupportedException();
	}

	public interface IIndexerService
	{
		string this[int index] { get; set; }
		string this[int index1, int index2] { get; set; }
		string this[int index1, int index2, int index3] { get; set; }
		string this[int index1, int index2, int index3, int index4] { get; set; }
		string this[int index1, int index2, int index3, int index4, int index5] { get; set; }

		string this[char index] { get; set; }
		string this[char index1, char index2] { get; set; }
		string this[char index1, char index2, char index3] { get; set; }
		string this[char index1, char index2, char index3, char index4] { get; set; }
		string this[char index1, char index2, char index3, char index4, char index5] { get; set; }

		int? this[string? index1, int index2, int index3] { get; set; }
	}
}
