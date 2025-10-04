using System.Threading;

namespace Mockolate.Tests;

public class MockBehaviorTests
{
	[Fact]
	public async Task DefaultValueGenerator_WithInt_ShouldReturnZero()
	{
		var sut = MockBehavior.Default;

		var result = sut.DefaultValueGenerator.Generate<int>();

		await That(result).IsEqualTo(0);
	}

	[Fact]
	public async Task DefaultValueGenerator_WithString_ShouldReturnNull()
	{
		var sut = MockBehavior.Default;

		var result = sut.DefaultValueGenerator.Generate<string>();

		await That(result).IsNull();
	}

	[Fact]
	public async Task DefaultValueGenerator_WithStruct_ShouldReturnDefault()
	{
		var sut = MockBehavior.Default;

		var result = sut.DefaultValueGenerator.Generate<DateTime>();

		await That(result).IsEqualTo(DateTime.MinValue);
	}

	[Fact]
	public async Task DefaultValueGenerator_WithObject_ShouldReturnNull()
	{
		var sut = MockBehavior.Default;

		var result = sut.DefaultValueGenerator.Generate<object>();

		await That(result).IsNull();
	}

	[Fact]
	public async Task DefaultValueGenerator_WithNullableInt_ShouldReturnNull()
	{
		var sut = MockBehavior.Default;

		var result = sut.DefaultValueGenerator.Generate<int?>();

		await That(result).IsNull();
	}

	[Fact]
	public async Task DefaultValueGenerator_WithCancellationToken_ShouldReturnNone()
	{
		var sut = MockBehavior.Default;

		var result = sut.DefaultValueGenerator.Generate<CancellationToken>();

		await That(result).IsEqualTo(CancellationToken.None);
	}

	[Fact]
	public async Task DefaultValueGenerator_WithTask_ShouldReturnCompletedTask()
	{
		var sut = MockBehavior.Default;

		var result = sut.DefaultValueGenerator.Generate<Task>();

		await That(result).IsNotNull();
		await That(result.IsCompleted).IsTrue();
		await That(result.IsFaulted).IsFalse();
	}

	[Fact]
	public async Task Default_ShouldInitializeCorrectly()
	{
		var sut = MockBehavior.Default;

		await That(sut.ThrowWhenNotSetup).IsFalse();
	}

	[Fact]
	public async Task ShouldSupportWithSyntax()
	{
		var sut = MockBehavior.Default with
		{
			ThrowWhenNotSetup = true,
			DefaultValueGenerator = new MyDefaultValueGenerator()
		};

		await That(sut.ThrowWhenNotSetup).IsTrue();
		await That(sut.DefaultValueGenerator.Generate<string>()).IsEqualTo("foo");
		await That(sut.DefaultValueGenerator.Generate<int>()).IsEqualTo(0);
	}

	private sealed class MyDefaultValueGenerator : MockBehavior.IDefaultValueGenerator
	{
		public T Generate<T>()
		{
			if (typeof(T) == typeof(string))
			{
				return (T)((object)"foo");
			}
			return default!;
		}
	}
}
