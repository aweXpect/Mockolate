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
			mock.SetupMock.Indexer(WithReadOnlySpan<int>(v => v.Length == 3 && v[0] == 1)).Returns(42);

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
			mock.SetupMock.Indexer(WithSpan<int>(v => v.Length == 3 && v[0] == 1)).Returns(42);

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

		internal class SpanMock
		{
			public virtual int this[Memory<int> v]
				=> v.Length;
			public virtual int this[ReadOnlySpan<int> v]
				=> v.Length;
			public virtual int this[Span<int> v]
				=> v.Length;
		}
	}
}
#endif
