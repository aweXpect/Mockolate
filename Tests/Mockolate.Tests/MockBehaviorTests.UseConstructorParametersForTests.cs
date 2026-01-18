namespace Mockolate.Tests;

public sealed partial class MockBehaviorTests
{
	public sealed class UseConstructorParametersForTests
	{
		[Fact]
		public async Task WithExplicitConstructorParameters_ShouldIgnoreConstructorParametersFromBehavior()
		{
			MockBehavior behavior = MockBehavior.Default
				.UseConstructorParametersFor<MyServiceWithMultipleConstructors>(() => [5,]);

			MyServiceWithMultipleConstructors mock
				= Mock.Create<MyServiceWithMultipleConstructors>(BaseClass.WithConstructorParameters(7), behavior);

			int value = mock.Value;

			await That(value).IsEqualTo(7);
		}

		[Fact]
		public async Task
			WithExplicitParameters_ShouldUseConstructorParametersFromBehavior()
		{
			MockBehavior behavior = MockBehavior.Default
				.UseConstructorParametersFor<MyServiceWithMultipleConstructors>(5);

			MyServiceWithMultipleConstructors mockWithDefaultBehavior
				= Mock.Create<MyServiceWithMultipleConstructors>();
			MyServiceWithMultipleConstructors mockWithCustomBehavior
				= Mock.Create<MyServiceWithMultipleConstructors>(behavior);

			int valueWithDefaultBehavior = mockWithDefaultBehavior.Value;
			int valueWithCustomBehavior = mockWithCustomBehavior.Value;

			await That(valueWithDefaultBehavior).IsEqualTo(0);
			await That(valueWithCustomBehavior).IsEqualTo(5);
		}

		[Fact]
		public async Task WithPredicate_ShouldUseConstructorParametersFromBehavior()
		{
			MockBehavior behavior = MockBehavior.Default
				.UseConstructorParametersFor<MyServiceWithMultipleConstructors>(() => [5,]);

			MyServiceWithMultipleConstructors mockWithDefaultBehavior
				= Mock.Create<MyServiceWithMultipleConstructors>();
			MyServiceWithMultipleConstructors mockWithCustomBehavior
				= Mock.Create<MyServiceWithMultipleConstructors>(behavior);

			int valueWithDefaultBehavior = mockWithDefaultBehavior.Value;
			int valueWithCustomBehavior = mockWithCustomBehavior.Value;

			await That(valueWithDefaultBehavior).IsEqualTo(0);
			await That(valueWithCustomBehavior).IsEqualTo(5);
		}

		internal class MyServiceWithMultipleConstructors
		{
			public MyServiceWithMultipleConstructors()
			{
				Value = 0;
			}

			public MyServiceWithMultipleConstructors(int value)
			{
				Value = value;
			}

			public int Value { get; }
		}
	}
}
