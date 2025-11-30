using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests;

public sealed partial class MockBehaviorTests
{
	[Fact]
	public async Task Default_ShouldInitializeCorrectly()
	{
		IMyService mock = Mock.Create<IMyService>();
		MockBehavior sut = ((IHasMockRegistration)mock).Registrations.Behavior;

		await That(sut.CallBaseClass).IsFalse();
		await That(sut.ThrowWhenNotSetup).IsFalse();
		await That(sut.DefaultValue).IsNotNull();
	}

	[Fact]
	public async Task ShouldSupportWithSyntax()
	{
		MockBehavior sut = MockBehavior.Default with
		{
			CallBaseClass = true,
			ThrowWhenNotSetup = true,
			DefaultValue = new MyDefaultValueGenerator(),
		};

		await That(sut.CallBaseClass).IsTrue();
		await That(sut.ThrowWhenNotSetup).IsTrue();
		await That(sut.DefaultValue.Generate("")).IsEqualTo("foo");
		await That(sut.DefaultValue.Generate(0)).IsEqualTo(0);
	}

	private sealed class MyDefaultValueGenerator : IDefaultValueGenerator
	{
		/// <inheritdoc cref="IDefaultValueGenerator.Generate(Type, object?[])" />
		public object? Generate(Type type, params object?[] parameters)
		{
			if (type == typeof(string))
			{
				return "foo";
			}

			return null;
		}
	}
}
