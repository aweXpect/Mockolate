using System.Threading;

namespace Mockerade.Tests;

public class MockBehaviorTests
{
	[Fact]
	public async Task DefaultValueGenerator_WithInt_ShouldReturnZero()
	{
		var sut = new MockBehavior();

		var result = sut.DefaultValueGenerator.Generate<int>();

		await That(result).IsEqualTo(0);
	}

	[Fact]
	public async Task DefaultValueGenerator_WithString_ShouldReturnNull()
	{
		var sut = new MockBehavior();

		var result = sut.DefaultValueGenerator.Generate<string>();

		await That(result).IsNull();
	}

	[Fact]
	public async Task DefaultValueGenerator_WithStruct_ShouldReturnDefault()
	{
		var sut = new MockBehavior();

		var result = sut.DefaultValueGenerator.Generate<DateTime>();

		await That(result).IsEqualTo(DateTime.MinValue);
	}

	[Fact]
	public async Task DefaultValueGenerator_WithObject_ShouldReturnNull()
	{
		var sut = new MockBehavior();

		var result = sut.DefaultValueGenerator.Generate<object>();

		await That(result).IsNull();
	}

	[Fact]
	public async Task DefaultValueGenerator_WithNullableInt_ShouldReturnNull()
	{
		var sut = new MockBehavior();

		var result = sut.DefaultValueGenerator.Generate<int?>();

		await That(result).IsNull();
	}

	[Fact]
	public async Task DefaultValueGenerator_WithCancellationToken_ShouldReturnNone()
	{
		var sut = new MockBehavior();

		var result = sut.DefaultValueGenerator.Generate<CancellationToken>();

		await That(result).IsEqualTo(CancellationToken.None);
	}

	[Fact]
	public async Task DefaultValueGenerator_WithTask_ShouldReturnCompletedTask()
	{
		var sut = new MockBehavior();

		var result = sut.DefaultValueGenerator.Generate<Task>();

		await That(result).IsNotNull();
		await That(result.IsCompleted).IsTrue();
		await That(result.IsFaulted).IsFalse();
	}
}
