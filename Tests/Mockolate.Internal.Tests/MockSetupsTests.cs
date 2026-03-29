using System.Reflection;
using Mockolate.Interactions;
using Mockolate.Parameters;
using Mockolate.Setup;

namespace Mockolate.Internal.Tests;

public class MockSetupsTests
{
	[Fact]
	public async Task EventSetups_AddAndRemove_ShouldUpdateCount()
	{
		MockSetups.EventSetups setups = new();
		object target = new();
		MethodInfo method = typeof(MockSetupsTests).GetMethod(nameof(EventSetups_AddAndRemove_ShouldUpdateCount))!;
		string eventName = "evt";
		setups.Add(target, method, eventName);
		await That(setups.Count).IsEqualTo(1);
		setups.Remove(target, method, eventName);
		await That(setups.Count).IsEqualTo(0);
	}

	[Fact]
	public async Task EventSetups_ThreadSafety_ShouldHandleParallelAddsAndRemoves()
	{
		MockSetups.EventSetups setups = new();
		object target = new();
		MethodInfo method = typeof(MockSetupsTests).GetMethod(nameof(EventSetups_ThreadSafety_ShouldHandleParallelAddsAndRemoves))!;
		string eventName = "evt";

		Parallel.For(0, 100, i => setups.Add(target, method, eventName + i));
		await That(setups.Count).IsEqualTo(100);

		Parallel.For(0, 100, i => setups.Remove(target, method, eventName + i));
		await That(setups.Count).IsEqualTo(0);
	}

	[Fact]
	public async Task IndexerSetups_AddAndGetLatestOrDefault_ShouldReturnCorrect()
	{
		MockSetups.IndexerSetups setups = new();
		FakeIndexerSetup setup1 = new(true);
		FakeIndexerSetup setup2 = new(false);
		FakeIndexerAccess access = new();
		setups.Add(setup1);
		setups.Add(setup2);

		IndexerSetup? result = setups.GetLatestOrDefault(access);

		await That(result).IsEqualTo(setup1);
	}

	[Fact]
	public async Task IndexerSetups_ThreadSafety_ShouldHandleParallelAdds()
	{
		MockSetups.IndexerSetups setups = new();

		Parallel.For(0, 100, i => setups.Add(new FakeIndexerSetup(false)));

		await That(setups.Count).IsEqualTo(100);
	}

	[Fact]
	public async Task MethodSetups_AddAndRetrieve_ShouldReturnCorrectCount()
	{
		MockSetups.MethodSetups setups = new();
		FakeMethodSetup setup1 = new();
		FakeMethodSetup setup2 = new();

		setups.Add(setup1);
		setups.Add(setup2);

		await That(setups.Count).IsEqualTo(2);
	}

	[Fact]
	public async Task MethodSetups_GetLatestOrDefault_ShouldReturnLatestMatching()
	{
		MockSetups.MethodSetups setups = new();
		FakeMethodSetup setup1 = new();
		FakeMethodSetup setup2 = new();
		setups.Add(setup1);
		setups.Add(setup2);

		MethodSetup? result = setups.GetLatestOrDefault(s => s == setup1);

		await That(result).IsEqualTo(setup1);
	}

	[Fact]
	public async Task MethodSetups_ThreadSafety_ShouldHandleParallelAdds()
	{
		MockSetups.MethodSetups setups = new();

		Parallel.For(0, 100, _ => setups.Add(new FakeMethodSetup()));

		await That(setups.Count).IsEqualTo(100);
	}

	[Fact]
	public async Task PropertySetups_AddDuplicate_ShouldReplaceAndAdjustCount()
	{
		MockSetups.PropertySetups setups = new();
		FakePropertySetup setup1 = new("foo");
		FakePropertySetup setup2 = new("foo");
		setups.Add(setup1);
		setups.Add(setup2);

		await That(setups.Count).IsEqualTo(1);

		setups.TryGetValue("foo", out PropertySetup? found);

		await That(found).IsEqualTo(setup2);
	}

	[Fact]
	public async Task PropertySetups_ThreadSafety_ShouldHandleParallelAdds()
	{
		MockSetups.PropertySetups setups = new();

		Parallel.For(0, 100, i => setups.Add(new FakePropertySetup($"p{i}")));

		await That(setups.Count).IsEqualTo(100);
	}

	[Fact]
	public async Task PropertySetups_TryGetValue_Nonexistent_ShouldReturnFalse()
	{
		MockSetups.PropertySetups setups = new();

		bool result = setups.TryGetValue("bar", out PropertySetup? found);

		await That(result).IsFalse();
		await That(found).IsNull();
	}

	[Theory]
	[InlineData(0, 0, 0, 0, "no setups")]
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
	public async Task ToString_ShouldReturnExpectedValue(
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

	private sealed class FakeMethodSetup : MethodSetup
	{
		public FakeMethodSetup() : base(null!) { }
		protected override bool? GetSkipBaseClass() => null;
		protected override T SetOutParameter<T>(string parameterName, Func<T> defaultValueGenerator) => default!;
		protected override T SetRefParameter<T>(string parameterName, T value, MockBehavior behavior) => value;
		protected override void ExecuteCallback(MethodInvocation invocation, MockBehavior behavior) { }
		protected override TResult GetReturnValue<TResult>(MethodInvocation invocation, MockBehavior behavior, Func<TResult> defaultValueGenerator) => defaultValueGenerator();
		protected override void TriggerParameterCallbacks(object?[] parameters) { }
	}

	private sealed class FakePropertySetup : PropertySetup<int>
	{
		public FakePropertySetup(string name) : base(name) { }
	}

	private sealed class FakeIndexerSetup : IndexerSetup
	{
		private readonly bool _match;
		public FakeIndexerSetup(bool match) { _match = match; }
		protected override bool IsMatch(NamedParameterValue[] parameters) => _match;
		protected override T ExecuteGetterCallback<T>(IndexerGetterAccess indexerGetterAccess, T value, MockBehavior behavior) => value;
		protected override void ExecuteSetterCallback<T>(IndexerSetterAccess indexerSetterAccess, T value, MockBehavior behavior) { }
		protected override bool? GetSkipBaseClass() => null;

		protected override void GetInitialValue<T>(MockBehavior behavior, Func<T> defaultValueGenerator, NamedParameterValue[] parameters, out T value)
			=> value = defaultValueGenerator();
	}

	private sealed class FakeIndexerAccess : IndexerAccess
	{
		public FakeIndexerAccess() : base(Array.Empty<NamedParameterValue>()) { }
	}
}
