using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Mockolate.DefaultValues;

namespace Mockolate.Tests;

public sealed partial class MockBehaviorTests
{
	[Fact]
	public async Task Default_ShouldInitializeCorrectly()
	{
		Mock<IDefaultValueGeneratorProperties> mock = Mock.Create<IDefaultValueGeneratorProperties>();
		MockBehavior sut = ((IMock)mock).Behavior;

		await That(sut.ThrowWhenNotSetup).IsFalse();
	}

	[Fact]
	public async Task DefaultValue_WithArray_ShouldReturnEmptyArray()
	{
		Mock<IDefaultValueGeneratorProperties> mock = Mock.Create<IDefaultValueGeneratorProperties>();

		int[] result = mock.Subject.SimpleArray;

		await That(result).HasCount(0);
	}

	[Fact]
	public async Task DefaultValue_WithCancellationToken_ShouldReturnNone()
	{
		MockBehavior sut = MockBehavior.Default;

		CancellationToken result = sut.DefaultValue.Generate<CancellationToken>();

		await That(result).IsEqualTo(CancellationToken.None);
	}

	[Fact]
	public async Task DefaultValue_WithIEnumerable_ShouldReturnEmptyEnumerable()
	{
		Mock<IDefaultValueGeneratorProperties> mock = Mock.Create<IDefaultValueGeneratorProperties>();

		IEnumerable result = mock.Subject.IEnumerable;

		await That(result).HasCount(0);
	}

	[Fact]
	public async Task DefaultValue_WithIEnumerableOfInt_ShouldReturnEmptyEnumerable()
	{
		Mock<IDefaultValueGeneratorProperties> mock = Mock.Create<IDefaultValueGeneratorProperties>();

		IEnumerable<int> result = mock.Subject.IEnumerableOfInt;

		await That(result).HasCount(0);
	}

	[Fact]
	public async Task DefaultValue_WithInt_ShouldReturnZero()
	{
		MockBehavior sut = MockBehavior.Default;

		int result = sut.DefaultValue.Generate<int>();

		await That(result).IsEqualTo(0);
	}

	[Fact]
	public async Task DefaultValue_WithMultidimensionalArray_ShouldReturnEmptyArray()
	{
		Mock<IDefaultValueGeneratorProperties> mock = Mock.Create<IDefaultValueGeneratorProperties>();

		int[,,][,][] result = mock.Subject.MultiDimensionalArray;

		await That(result).HasCount(0);
	}

	[Fact]
	public async Task DefaultValue_WithNullableInt_ShouldReturnNull()
	{
		MockBehavior sut = MockBehavior.Default;

		int? result = sut.DefaultValue.Generate<int?>();

		await That(result).IsNull();
	}

	[Fact]
	public async Task DefaultValue_WithObject_ShouldReturnNull()
	{
		MockBehavior sut = MockBehavior.Default;

		object result = sut.DefaultValue.Generate<object>();

		await That(result).IsNull();
	}

	[Fact]
	public async Task DefaultValue_WithString_ShouldReturnEmptyString()
	{
		MockBehavior sut = MockBehavior.Default;

		string result = sut.DefaultValue.Generate<string>();

		await That(result).IsEmpty();
	}

	[Fact]
	public async Task DefaultValue_WithStruct_ShouldReturnDefault()
	{
		MockBehavior sut = MockBehavior.Default;

		DateTime result = sut.DefaultValue.Generate<DateTime>();

		await That(result).IsEqualTo(DateTime.MinValue);
	}

	[Fact]
	public async Task DefaultValue_WithTask_ShouldReturnCompletedTask()
	{
		MockBehavior sut = MockBehavior.Default;

		Task result = sut.DefaultValue.Generate<Task>();

		await That(result).IsNotNull();
		await That(result.IsCompleted).IsTrue();
		await That(result.IsFaulted).IsFalse();
	}

	[Fact]
	public async Task DefaultValue_WithTaskInt_ShouldReturnZero()
	{
		Mock<IDefaultValueGeneratorProperties> mock = Mock.Create<IDefaultValueGeneratorProperties>();
		MockBehavior sut = ((IMock)mock).Behavior;

		Task<int> result = mock.Subject.IntTask;

		await That(result.IsCompleted).IsTrue();
		await That(result).IsEqualTo(0);
	}

	[Fact]
	public async Task DefaultValue_WithTaskIntArray_ShouldReturnZero()
	{
		Mock<IDefaultValueGeneratorProperties> mock = Mock.Create<IDefaultValueGeneratorProperties>();
		MockBehavior sut = ((IMock)mock).Behavior;

		Task<int[]> result = mock.Subject.IntArrayTask;

		await That(result.IsCompleted).IsTrue();
		await That(result).IsEmpty();
	}

	[Fact]
	public async Task DefaultValue_WithValueTaskInt_ShouldReturnZero()
	{
		Mock<IDefaultValueGeneratorProperties> mock = Mock.Create<IDefaultValueGeneratorProperties>();
		MockBehavior sut = ((IMock)mock).Behavior;

		ValueTask<int> result = mock.Subject.IntValueTask;

		await That(result.IsCompleted).IsTrue();
		await That(await result).IsEqualTo(0);
	}

	[Fact]
	public async Task DefaultValue_WithValueTaskIntArray_ShouldReturnZero()
	{
		Mock<IDefaultValueGeneratorProperties> mock = Mock.Create<IDefaultValueGeneratorProperties>();
		MockBehavior sut = ((IMock)mock).Behavior;

		ValueTask<int[]> result = mock.Subject.IntArrayValueTask;

		await That(result.IsCompleted).IsTrue();
		await That(await result).IsEmpty();
	}

	[Fact]
	public async Task DefaultValue_WithValueTuple_ShouldReturnValueTupleWithDefaultValues()
	{
		Mock<IDefaultValueGeneratorProperties> mock = Mock.Create<IDefaultValueGeneratorProperties>();

		(int V1, string V2) result = mock.Subject.NamedValueTuple;

		await That(result.V1).IsEqualTo(0);
		await That(result.V2).IsEqualTo("");
	}

	[Fact]
	public async Task DefaultValue_WithValueTuple8_ShouldReturnValueTupleWithDefaultValues()
	{
		Mock<IDefaultValueGeneratorProperties> mock = Mock.Create<IDefaultValueGeneratorProperties>();
		MockBehavior sut = ((IMock)mock).Behavior;

		(int V1, string V2, int V3, string V4, int V5, string V6, int V7, string V8) result = mock.Subject.ValueTuple8;

		await That(result.V1).IsEqualTo(0);
		await That(result.V2).IsEqualTo("");
		await That(result.V3).IsEqualTo(0);
		await That(result.V4).IsEqualTo("");
		await That(result.V5).IsEqualTo(0);
		await That(result.V6).IsEqualTo("");
		await That(result.V7).IsEqualTo(0);
		await That(result.V8).IsEqualTo("");
	}

	[Fact]
	public async Task ShouldSupportWithSyntax()
	{
		MockBehavior sut = MockBehavior.Default with
		{
			ThrowWhenNotSetup = true,
			DefaultValue = new MyDefaultValueGenerator(),
		};

		await That(sut.ThrowWhenNotSetup).IsTrue();
		await That(sut.DefaultValue.Generate<string>()).IsEqualTo("foo");
		await That(sut.DefaultValue.Generate<int>()).IsEqualTo(0);
	}

	public interface IDefaultValueGeneratorProperties
	{
		int[] SimpleArray { get; }
		int[,,][,][] MultiDimensionalArray { get; }
		IEnumerable IEnumerable { get; }
		IEnumerable<int> IEnumerableOfInt { get; }
		(int V1, string V2) NamedValueTuple { get; }
		(int, string, int, string, int, string, int, string) ValueTuple8 { get; }
		Task<int> IntTask { get; }
		Task<int[]> IntArrayTask { get; }
		ValueTask<int> IntValueTask { get; }
		ValueTask<int[]> IntArrayValueTask { get; }
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
