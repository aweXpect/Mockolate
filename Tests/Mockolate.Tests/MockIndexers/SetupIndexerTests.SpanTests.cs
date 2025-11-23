#if NET8_0_OR_GREATER

namespace Mockolate.Tests.MockIndexers;

public sealed partial class SetupIndexerTests
{
	public sealed class SpanTests
	{
		[Fact]
		public async Task Memory_WhenPredicateDoesNotMatch_ShouldUseDefaultValue()
		{
			SpanMock mock = Mock.Create<SpanMock>(MockBehavior.Default.CallingBaseClass());
			mock.SetupMock.Indexer(With<Memory<int>>(v => v.Length == 2)).Returns(4);

			int result = mock[new Memory<int>([1, 2, 3,])];

			await That(result).IsEqualTo(3);
		}

		[Fact]
		public async Task Memory_WhenPredicateMatches_ShouldApplySetup()
		{
			SpanMock mock = Mock.Create<SpanMock>(MockBehavior.Default.CallingBaseClass());
			mock.SetupMock.Indexer(With<Memory<int>>(v => v.Length == 3)).Returns(42);

			int result = mock[new Memory<int>([1, 2, 3,])];

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task Memory_WithoutPredicate_ShouldMatchAnySpan()
		{
			SpanMock mock = Mock.Create<SpanMock>(MockBehavior.Default.CallingBaseClass());
			mock.SetupMock.Indexer(Any<Memory<int>>()).Returns(42);

			int result = mock[new Memory<int>([1, 2, 3,])];

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task ReadOnlySpan_IndexerReturnValueSetup_ShouldReturnExpectedSpan()
		{
			SpanMock mock = Mock.Create<SpanMock>();
			char[] expectedData =
			[
				'x', 'y', 'z',
			];
			mock.SetupMock.Indexer(Any<string>()).Returns(new ReadOnlySpan<char>(expectedData));

			ReadOnlySpan<char> result = mock["test"];

			await That(result).IsEqualTo(expectedData);
		}

		[Fact]
		public async Task ReadOnlySpan_WhenPredicateDoesNotMatch_ShouldUseDefaultValue()
		{
			SpanMock mock = Mock.Create<SpanMock>(MockBehavior.Default.CallingBaseClass());
			mock.SetupMock.Indexer(WithReadOnlySpan<int>(v => v.Length == 2)).Returns(4);

			int result = mock[new ReadOnlySpan<int>([1, 2, 3,])];

			await That(result).IsEqualTo(3);
		}

		[Fact]
		public async Task ReadOnlySpan_WhenPredicateMatches_ShouldApplySetup()
		{
			SpanMock mock = Mock.Create<SpanMock>(MockBehavior.Default.CallingBaseClass());
			mock.SetupMock.Indexer(WithReadOnlySpan<int>(v => v is [1, _, _,])).Returns(42);

			int result = mock[new ReadOnlySpan<int>([1, 2, 3,])];

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task ReadOnlySpan_WithoutPredicate_ShouldMatchAnySpan()
		{
			SpanMock mock = Mock.Create<SpanMock>(MockBehavior.Default.CallingBaseClass());
			mock.SetupMock.Indexer(AnyReadOnlySpan<int>()).Returns(42);

			int result = mock[new ReadOnlySpan<int>([1, 2, 3,])];

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task Span_IndexerReturnValueSetup_ShouldReturnExpectedSpan()
		{
			SpanMock mock = Mock.Create<SpanMock>();
			int[] expectedData =
			[
				1, 2, 3,
			];
			mock.SetupMock.Indexer(Any<int>()).Returns(new Span<int>(expectedData));

			Span<int> result = mock[5];

			await That(result).IsEqualTo(expectedData);
		}

		[Fact]
		public async Task Span_IndexerWithSpecificKey_ShouldMatchAndReturn()
		{
			SpanMock mock = Mock.Create<SpanMock>();
			int[] key1Data =
			[
				10, 20,
			];
			int[] key2Data =
			[
				30, 40, 50,
			];

			mock.SetupMock.Indexer(With<int>(k => k == 1)).Returns(new Span<int>(key1Data));
			mock.SetupMock.Indexer(With<int>(k => k == 2)).Returns(new Span<int>(key2Data));

			int[] result1 = mock[1].ToArray();
			int[] result2 = mock[2].ToArray();

			await That(result1).IsEqualTo(key1Data);
			await That(result2).IsEqualTo(key2Data);
		}

		[Fact]
		public async Task Span_WhenPredicateDoesNotMatch_ShouldUseDefaultValue()
		{
			SpanMock mock = Mock.Create<SpanMock>(MockBehavior.Default.CallingBaseClass());
			mock.SetupMock.Indexer(WithSpan<int>(v => v.Length == 2)).Returns(4);

			int result = mock[new Span<int>([1, 2, 3,])];

			await That(result).IsEqualTo(3);
		}

		[Fact]
		public async Task Span_WhenPredicateMatches_ShouldApplySetup()
		{
			SpanMock mock = Mock.Create<SpanMock>(MockBehavior.Default.CallingBaseClass());
			mock.SetupMock.Indexer(WithSpan<int>(v => v is [1, _, _,])).Returns(42);

			int result = mock[new Span<int>([1, 2, 3,])];

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task Span_WithoutPredicate_ShouldMatchAnySpan()
		{
			SpanMock mock = Mock.Create<SpanMock>(MockBehavior.Default.CallingBaseClass());
			mock.SetupMock.Indexer(AnySpan<int>()).Returns(42);

			int result = mock[new Span<int>([1, 2, 3,])];

			await That(result).IsEqualTo(42);
		}

		internal abstract class SpanMock
		{
			public virtual int this[Memory<int> v]
				=> v.Length;

			public virtual int this[ReadOnlySpan<int> v]
				=> v.Length;

			public virtual int this[Span<int> v]
				=> v.Length;

			public abstract Span<int> this[int key] { get; }

			public abstract ReadOnlySpan<char> this[string key] { get; }
		}
	}
}
#endif
