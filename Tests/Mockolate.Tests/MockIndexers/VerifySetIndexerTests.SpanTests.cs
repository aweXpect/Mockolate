#if NET8_0_OR_GREATER

namespace Mockolate.Tests.MockIndexers;

public sealed partial class VerifySetIndexerTests
{
	public sealed class SpanTests
	{
		[Fact]
		public async Task Memory_WhenPredicateMatches_ShouldApplySetup()
		{
			ISpanMock mock = Mock.Create<ISpanMock>(MockBehavior.Default.SkippingBaseClass());

			mock[new Memory<int>([1, 2, 3,])] = 3;

			await That(mock.VerifyMock.SetIndexer(It.Satisfies<Memory<int>>(v => v.Length == 2), It.IsAny<int>())).Never();
			await That(mock.VerifyMock.SetIndexer(It.Satisfies<Memory<int>>(v => v.Length == 3), It.IsAny<int>())).Once();
			await That(mock.VerifyMock.SetIndexer(It.Satisfies<Memory<int>>(v => v.Length == 4), It.IsAny<int>())).Never();
		}

		[Fact]
		public async Task Memory_WithoutPredicate_ShouldApplyAllCalls()
		{
			ISpanMock mock = Mock.Create<ISpanMock>(MockBehavior.Default.SkippingBaseClass());

			mock[new Memory<int>()] = 3;
			mock[new Span<int>([1, 2, 3,])] = 3;
			mock[new Memory<int>([1, 2,])] = 3;

			await That(mock.VerifyMock.SetIndexer(It.IsAny<Memory<int>>(), It.IsAny<int>())).Twice();
		}

		[Fact]
		public async Task ReadOnlySpan_WhenPredicateMatches_ShouldApplySetup()
		{
			ISpanMock mock = Mock.Create<ISpanMock>(MockBehavior.Default.SkippingBaseClass());

			mock[new ReadOnlySpan<int>([1, 2, 3,])] = 3;

			await That(mock.VerifyMock.SetIndexer(It.IsReadOnlySpan<int>(v => v.Length == 2), It.IsAny<int>())).Never();
			await That(mock.VerifyMock.SetIndexer(It.IsReadOnlySpan<int>(v => v.Length == 3), It.IsAny<int>())).Once();
			await That(mock.VerifyMock.SetIndexer(It.IsReadOnlySpan<int>(v => v.Length == 4), It.IsAny<int>())).Never();
		}

		[Fact]
		public async Task ReadOnlySpan_WithoutPredicate_ShouldApplyAllCalls()
		{
			ISpanMock mock = Mock.Create<ISpanMock>(MockBehavior.Default.SkippingBaseClass());

			mock[new ReadOnlySpan<int>()] = 3;
			mock[new Span<int>([1, 2, 3,])] = 3;
			mock[new ReadOnlySpan<int>([1, 2,])] = 3;

			await That(mock.VerifyMock.SetIndexer(It.IsAnyReadOnlySpan<int>(), It.IsAny<int>())).Twice();
		}

		[Fact]
		public async Task Span_WhenPredicateMatches_ShouldApplySetup()
		{
			ISpanMock mock = Mock.Create<ISpanMock>(MockBehavior.Default.SkippingBaseClass());

			mock[new Span<int>([1, 2, 3,])] = 3;

			await That(mock.VerifyMock.SetIndexer(It.IsSpan<int>(v => v.Length == 2), It.IsAny<int>())).Never();
			await That(mock.VerifyMock.SetIndexer(It.IsSpan<int>(v => v.Length == 3), It.IsAny<int>())).Once();
			await That(mock.VerifyMock.SetIndexer(It.IsSpan<int>(v => v.Length == 4), It.IsAny<int>())).Never();
		}

		[Fact]
		public async Task Span_WithoutPredicate_ShouldApplyAllCalls()
		{
			ISpanMock mock = Mock.Create<ISpanMock>(MockBehavior.Default.SkippingBaseClass());

			mock[new Span<int>()] = 3;
			mock[new ReadOnlySpan<int>([1, 2, 3,])] = 3;
			mock[new Span<int>([1, 2,])] = 3;

			await That(mock.VerifyMock.SetIndexer(It.IsAnySpan<int>(), It.IsAny<int>())).Twice();
		}

		internal interface ISpanMock
		{
			int this[Memory<int> v] { get; set; }
			int this[ReadOnlySpan<int> v] { get; set; }
			int this[Span<int> v] { get; set; }
		}
	}
}
#endif
