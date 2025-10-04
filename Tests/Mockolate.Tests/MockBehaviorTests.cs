using System.Threading;

namespace Mockolate.Tests;

public class MockBehaviorTests
{
	[Fact]
	public async Task Default_ShouldInitializeCorrectly()
	{
		MockBehavior sut = MockBehavior.Default;

		await That(sut.ThrowWhenNotSetup).IsFalse();
	}

	[Fact]
	public async Task DefaultValueGenerator_WithCancellationToken_ShouldReturnNone()
	{
		MockBehavior sut = MockBehavior.Default;

		CancellationToken result = sut.DefaultValueGenerator.Generate<CancellationToken>();

		await That(result).IsEqualTo(CancellationToken.None);
	}

	[Fact]
	public async Task DefaultValueGenerator_WithInt_ShouldReturnZero()
	{
		MockBehavior sut = MockBehavior.Default;

		int result = sut.DefaultValueGenerator.Generate<int>();

		await That(result).IsEqualTo(0);
	}

	[Fact]
	public async Task DefaultValueGenerator_WithNullableInt_ShouldReturnNull()
	{
		MockBehavior sut = MockBehavior.Default;

		int? result = sut.DefaultValueGenerator.Generate<int?>();

		await That(result).IsNull();
	}

	[Fact]
	public async Task DefaultValueGenerator_WithObject_ShouldReturnNull()
	{
		MockBehavior sut = MockBehavior.Default;

		object result = sut.DefaultValueGenerator.Generate<object>();

		await That(result).IsNull();
	}

	[Fact]
	public async Task DefaultValueGenerator_WithString_ShouldReturnNull()
	{
		MockBehavior sut = MockBehavior.Default;

		string result = sut.DefaultValueGenerator.Generate<string>();

		await That(result).IsNull();
	}

	[Fact]
	public async Task DefaultValueGenerator_WithStruct_ShouldReturnDefault()
	{
		MockBehavior sut = MockBehavior.Default;

		DateTime result = sut.DefaultValueGenerator.Generate<DateTime>();

		await That(result).IsEqualTo(DateTime.MinValue);
	}

	[Fact]
	public async Task DefaultValueGenerator_WithTask_ShouldReturnCompletedTask()
	{
		MockBehavior sut = MockBehavior.Default;

		Task result = sut.DefaultValueGenerator.Generate<Task>();

		await That(result).IsNotNull();
		await That(result.IsCompleted).IsTrue();
		await That(result.IsFaulted).IsFalse();
	}

	[Fact]
	public async Task ShouldSupportWithSyntax()
	{
		MockBehavior sut = MockBehavior.Default with
		{
			ThrowWhenNotSetup = true,
			DefaultValueGenerator = new MyDefaultValueGenerator(),
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
				return (T)(object)"foo";
			}

			return default!;
		}
	}
}
