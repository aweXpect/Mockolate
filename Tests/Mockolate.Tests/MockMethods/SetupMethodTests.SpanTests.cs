#if NET8_0_OR_GREATER

namespace Mockolate.Tests.MockMethods;

public sealed partial class SetupMethodTests
{
	public sealed class SpanTests
	{
		[Fact]
		public async Task Memory_WhenPredicateDoesNotMatch_ShouldUseDefaultValue()
		{
			SpanMock mock = Mock.Create<SpanMock>(MockBehavior.Default.CallingBaseClass());
			mock.SetupMock.Method.MyMethod(It.Satisfies<Memory<int>>(v => v.Length == 2)).Returns(4);

			int result = mock.MyMethod(new Memory<int>([1, 2, 3,]));

			await That(result).IsEqualTo(3);
		}

		[Fact]
		public async Task Memory_WhenPredicateMatches_ShouldApplySetup()
		{
			SpanMock mock = Mock.Create<SpanMock>(MockBehavior.Default.CallingBaseClass());
			mock.SetupMock.Method.MyMethod(It.Satisfies<Memory<int>>(v => v.Length == 3)).Returns(42);

			int result = mock.MyMethod(new Memory<int>([1, 2, 3,]));

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task Memory_WithoutPredicate_ShouldMatchAnySpan()
		{
			SpanMock mock = Mock.Create<SpanMock>(MockBehavior.Default.CallingBaseClass());
			mock.SetupMock.Method.MyMethod(It.IsAny<Memory<int>>()).Returns(42);

			int result = mock.MyMethod(new Memory<int>([1, 2, 3,]));

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task ReadOnlySpan_ReturnValueSetup_ShouldReturnExpectedSpan()
		{
			SpanMock mock = Mock.Create<SpanMock>();
			char[] expectedData = ['a', 'b', 'c',];
			mock.SetupMock.Method.GetReadOnlySpan().Returns(new ReadOnlySpan<char>(expectedData));

			char[] result = mock.GetReadOnlySpan().ToArray();

			await That(result).IsEqualTo(expectedData);
		}

		[Fact]
		public async Task ReadOnlySpan_WhenPredicateDoesNotMatch_ShouldUseDefaultValue()
		{
			SpanMock mock = Mock.Create<SpanMock>(MockBehavior.Default.CallingBaseClass());
			mock.SetupMock.Method.MyMethod(It.IsReadOnlySpan<int>(v => v.Length == 2)).Returns(4);

			int result = mock.MyMethod(new ReadOnlySpan<int>([1, 2, 3,]));

			await That(result).IsEqualTo(3);
		}

		[Fact]
		public async Task ReadOnlySpan_WhenPredicateMatches_ShouldApplySetup()
		{
			SpanMock mock = Mock.Create<SpanMock>(MockBehavior.Default.CallingBaseClass());
			mock.SetupMock.Method.MyMethod(It.IsReadOnlySpan<int>(v => v is [1, _, _,])).Returns(42);

			int result = mock.MyMethod(new ReadOnlySpan<int>([1, 2, 3,]));

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task ReadOnlySpan_WithoutPredicate_ShouldMatchAnySpan()
		{
			SpanMock mock = Mock.Create<SpanMock>(MockBehavior.Default.CallingBaseClass());
			mock.SetupMock.Method.MyMethod(It.IsAnyReadOnlySpan<int>()).Returns(42);

			int result = mock.MyMethod(new ReadOnlySpan<int>([1, 2, 3,]));

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task ReadOnlySpan_WithParameter_ShouldMatchAndReturn()
		{
			SpanMock mock = Mock.Create<SpanMock>();
			byte[] inputData = [10, 20, 30,];
			byte[] outputData = [100, 200,];

			mock.SetupMock.Method.ProcessData(It.IsAnySpan<byte>())
				.Returns(new ReadOnlySpan<byte>(outputData));

			byte[] result = mock.ProcessData(new Span<byte>(inputData)).ToArray();

			await That(result).IsEqualTo(outputData);
		}

		[Fact]
		public async Task Span_MultipleReturnValues_ShouldCycleThroughValues()
		{
			SpanMock mock = Mock.Create<SpanMock>();
			int[] firstData = [1, 2,];
			int[] secondData = [3, 4, 5,];
			mock.SetupMock.Method.GetSpan()
				.Returns(new Span<int>(firstData))
				.Returns(new Span<int>(secondData));

			int[] firstResult = mock.GetSpan().ToArray();
			int[] secondResult = mock.GetSpan().ToArray();
			int[] thirdResult = mock.GetSpan().ToArray();

			await That(firstResult).IsEqualTo(firstData);
			await That(secondResult).IsEqualTo(secondData);
			await That(thirdResult).IsEqualTo(firstData);
		}

		[Fact]
		public async Task Span_ReturnValueSetup_ShouldReturnExpectedSpan()
		{
			SpanMock mock = Mock.Create<SpanMock>();
			int[] expectedData = [1, 2, 3,];
			mock.SetupMock.Method.GetSpan().Returns(new Span<int>(expectedData));

			Span<int> result = mock.GetSpan();

			await That(result).IsEqualTo(expectedData);
		}

		[Fact]
		public async Task Span_WhenPredicateDoesNotMatch_ShouldUseDefaultValue()
		{
			SpanMock mock = Mock.Create<SpanMock>(MockBehavior.Default.CallingBaseClass());
			mock.SetupMock.Method.MyMethod(It.IsSpan<int>(v => v.Length == 2)).Returns(4);

			int result = mock.MyMethod(new Span<int>([1, 2, 3,]));

			await That(result).IsEqualTo(3);
		}

		[Fact]
		public async Task Span_WhenPredicateMatches_ShouldApplySetup()
		{
			SpanMock mock = Mock.Create<SpanMock>(MockBehavior.Default.CallingBaseClass());
			mock.SetupMock.Method.MyMethod(It.IsSpan<int>(v => v is [1, _, _,])).Returns(42);

			int result = mock.MyMethod(new Span<int>([1, 2, 3,]));

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task Span_WithCallback_ShouldExecuteCallback()
		{
			SpanMock mock = Mock.Create<SpanMock>();
			bool callbackExecuted = false;
			int[] returnData = [42,];

			mock.SetupMock.Method.GetSpan()
				.Do(() => callbackExecuted = true)
				.Returns(new Span<int>(returnData));

			int[] result = mock.GetSpan().ToArray();

			await That(callbackExecuted).IsTrue();
			await That(result).IsEqualTo(returnData);
		}

		[Fact]
		public async Task Span_WithoutPredicate_ShouldMatchAnySpan()
		{
			SpanMock mock = Mock.Create<SpanMock>(MockBehavior.Default.CallingBaseClass());
			mock.SetupMock.Method.MyMethod(It.IsAnySpan<int>()).Returns(42);

			int result = mock.MyMethod(new Span<int>([1, 2, 3,]));

			await That(result).IsEqualTo(42);
		}

		internal abstract class SpanMock
		{
			public virtual int MyMethod(Memory<int> value)
				=> value.Length;

			public virtual int MyMethod(ReadOnlySpan<int> value)
				=> value.Length;

			public virtual int MyMethod(Span<int> value)
				=> value.Length;

			public abstract Span<int> GetSpan();
			public abstract ReadOnlySpan<char> GetReadOnlySpan();
			public abstract ReadOnlySpan<byte> ProcessData(Span<byte> input);
		}
	}
}
#endif
