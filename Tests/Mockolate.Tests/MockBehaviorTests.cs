using Mockolate.DefaultValues;
using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests;

public sealed partial class MockBehaviorTests
{
	[Fact]
	public async Task Default_ShouldInitializeCorrectly()
	{
		IMyService mock = Mock.Create<IMyService>();
		MockBehavior sut = ((IHasMockRegistration)mock).Registrations.Behavior;

		await That(sut.BaseClassBehavior).IsEqualTo(BaseClassBehavior.DoNotCallBaseClass);
		await That(sut.ThrowWhenNotSetup).IsFalse();
		await That(sut.DefaultValue).Is<DefaultValueGenerator>();
	}

	[Fact]
	public async Task ShouldSupportWithSyntax()
	{
		MockBehavior sut = MockBehavior.Default with
		{
			BaseClassBehavior = BaseClassBehavior.UseBaseClassAsDefaultValue,
			ThrowWhenNotSetup = true,
			DefaultValue = new MyDefaultValueGenerator(),
		};

		await That(sut.BaseClassBehavior).IsEqualTo(BaseClassBehavior.UseBaseClassAsDefaultValue);
		await That(sut.ThrowWhenNotSetup).IsTrue();
		await That(sut.DefaultValue.Generate<string>()).IsEqualTo("foo");
		await That(sut.DefaultValue.Generate<int>()).IsEqualTo(0);
	}

	private sealed class MyDefaultValueGenerator : IDefaultValueGenerator
	{
		public T Generate<T>()
		{
			if (typeof(T) == typeof(string))
			{
				return (T)(object)"foo";
			}

			return default!;
		}
	}
}
