namespace Mockolate.Tests;

public sealed class DefaultValueFactoryTests
{
	[Fact]
	public async Task CanGenerateValue_ShouldFallbackToFalse()
	{
		MyDefaultValueFactory defaultValueFactory = new();

		bool result = defaultValueFactory.CanGenerateValue(typeof(DefaultValueFactoryTests));

		await That(result).IsFalse();
	}

	[Fact]
	public async Task GenerateValue_ShouldFallbackToAlwaysReturnNull()
	{
		MyDefaultValueFactory defaultValueFactory = new();

		object? result = defaultValueFactory.GenerateValue(typeof(int));

		await That(result).IsNull();
	}

	private class MyDefaultValueFactory : DefaultValueFactory
	{
	}
}
