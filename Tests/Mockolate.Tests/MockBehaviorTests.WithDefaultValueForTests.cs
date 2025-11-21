namespace Mockolate.Tests;

public sealed partial class MockBehaviorTests
{
	public sealed class WithDefaultValueForTests
	{
		[Test]
		public async Task MultipleTypedFactories_ShouldSupportAll()
		{
			MockBehavior behavior = MockBehavior.Default
				.WithDefaultValueFor(() => new MySpecialValue(5))
				.WithDefaultValueFor(() => new MyInheritedSpecialValue(6, 7));

			IMyServiceForMySpecialValue mockWithBehavior
				= Mock.Create<IMyServiceForMySpecialValue>(behavior);

			await That(mockWithBehavior.Value.Value).IsEqualTo(5);
			await That(mockWithBehavior.InheritedValue)
				.For(x => x.Value, it => it.IsEqualTo(6)).And
				.For(x => x.OtherValue, it => it.IsEqualTo(7));
		}

		[Test]
		public async Task WithExplicitDefaultValueFactory_ShouldApplyThisFactory()
		{
			DefaultValueFactory defaultValueFactory = new(
				t => typeof(MySpecialValue).IsAssignableFrom(t),
				(_, _) => new MyInheritedSpecialValue(6, 7));
			MockBehavior behavior = MockBehavior.Default
				.WithDefaultValueFor(defaultValueFactory);

			IMyServiceForMySpecialValue mockWithBehavior
				= Mock.Create<IMyServiceForMySpecialValue>(behavior);

			await That(mockWithBehavior.Value.Value).IsEqualTo(6);
			await That(mockWithBehavior.InheritedValue)
				.For(x => x.Value, it => it.IsEqualTo(6)).And
				.For(x => x.OtherValue, it => it.IsEqualTo(7));
		}

		[Test]
		public async Task WithTypedFactory_ShouldRequireExactTypeMatch()
		{
			MockBehavior behavior1 = MockBehavior.Default
				.WithDefaultValueFor(() => new MySpecialValue(5));
			MockBehavior behavior2 = MockBehavior.Default
				.WithDefaultValueFor(() => new MyInheritedSpecialValue(6, 7));

			IMyServiceForMySpecialValue mockWithBehavior1
				= Mock.Create<IMyServiceForMySpecialValue>(behavior1);
			IMyServiceForMySpecialValue mockWithBehavior2
				= Mock.Create<IMyServiceForMySpecialValue>(behavior2);

			await That(mockWithBehavior1.InheritedValue).IsNull();
			await That(mockWithBehavior1.Value.Value).IsEqualTo(5);
			await That(mockWithBehavior2.InheritedValue)
				.For(x => x.Value, it => it.IsEqualTo(6)).And
				.For(x => x.OtherValue, it => it.IsEqualTo(7));
			await That(mockWithBehavior2.Value).IsNull();
		}

		[Test]
		public async Task WithTypedFactory_ShouldUseDefaultValueFactoryFromBehavior()
		{
			MockBehavior behavior = MockBehavior.Default
				.WithDefaultValueFor(() => new MySpecialValue(5));

			IMyServiceForMySpecialValue mockWithDefaultBehavior
				= Mock.Create<IMyServiceForMySpecialValue>();
			IMyServiceForMySpecialValue mockWithCustomBehavior
				= Mock.Create<IMyServiceForMySpecialValue>(behavior);

			await That(mockWithDefaultBehavior.Value).IsNull();
			await That(mockWithCustomBehavior.Value.Value).IsEqualTo(5);
		}

		internal class MySpecialValue(int value)
		{
			public int Value { get; } = value;
		}

		internal class MyInheritedSpecialValue(int value, int otherValue) : MySpecialValue(value)
		{
			public int OtherValue { get; } = otherValue;
		}

		internal interface IMyServiceForMySpecialValue
		{
			MySpecialValue Value { get; }
			MyInheritedSpecialValue InheritedValue { get; }
		}
	}
}
