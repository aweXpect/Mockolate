using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests;

public sealed partial class MockBehaviorTests
{
	[Test]
	public async Task Default_ShouldInitializeCorrectly()
	{
		IMyService mock = Mock.Create<IMyService>();
		MockBehavior sut = ((IHasMockRegistration)mock).Registrations.Behavior;

		await That(sut.SkipBaseClass).IsFalse();
		await That(sut.ThrowWhenNotSetup).IsFalse();
		await That(sut.DefaultValue).IsNotNull();
	}

	[Test]
	public async Task ShouldSupportCustomDefaultValueGenerator()
	{
		MockBehavior sut = new(new MyDefaultValueGenerator());

		await That(sut.SkipBaseClass).IsFalse();
		await That(sut.ThrowWhenNotSetup).IsFalse();
		await That(sut.DefaultValue.Generate("")).IsEqualTo("foo");
		await That(sut.DefaultValue.Generate(0)).IsEqualTo(0);
	}

	[Test]
	public async Task ShouldSupportWithSyntax()
	{
		MockBehavior sut = MockBehavior.Default with
		{
			SkipBaseClass = true,
			ThrowWhenNotSetup = true,
			DefaultValue = new MyDefaultValueGenerator(),
		};

		await That(sut.SkipBaseClass).IsTrue();
		await That(sut.ThrowWhenNotSetup).IsTrue();
		await That(sut.DefaultValue.Generate("")).IsEqualTo("foo");
		await That(sut.DefaultValue.Generate(0)).IsEqualTo(0);
	}

	private sealed class MyDefaultValueGenerator : IDefaultValueGenerator
	{
		/// <inheritdoc cref="IDefaultValueGenerator.GenerateValue(Type, object?[])" />
		public object? GenerateValue(Type type, params object?[] parameters)
		{
			if (type == typeof(string))
			{
				return "foo";
			}

			return null;
		}
	}
}
