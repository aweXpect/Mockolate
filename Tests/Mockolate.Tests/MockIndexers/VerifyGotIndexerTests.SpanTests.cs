#if NET8_0_OR_GREATER

namespace Mockolate.Tests.MockIndexers;

public sealed partial class VerifyGotIndexerTests
{
	public sealed class SpanTests
	{
		[Fact]
		public async Task Memory_WhenPredicateMatches_ShouldApplySetup()
		{
			SpanMock mock = SpanMock.CreateMock();

			_ = mock[new Memory<int>([1, 2, 3,])];

			await That(mock.Mock.Verify[It.Satisfies<Memory<int>>(v => v.Length == 2)].Got()).Never();
			await That(mock.Mock.Verify[It.Satisfies<Memory<int>>(v => v.Length == 3)].Got()).Once();
			await That(mock.Mock.Verify[It.Satisfies<Memory<int>>(v => v.Length == 4)].Got()).Never();
		}

		[Fact]
		public async Task Memory_WithoutPredicate_ShouldApplyAllCalls()
		{
			SpanMock mock = SpanMock.CreateMock();

			_ = mock[new Memory<int>()];
			_ = mock[new Span<int>([1, 2, 3,])];
			_ = mock[new Memory<int>([1, 2,])];

			await That(mock.Mock.Verify[It.IsAny<Memory<int>>()].Got()).Twice();
		}

		[Fact]
		public async Task ReadOnlySpan_WhenPredicateMatches_ShouldApplySetup()
		{
			SpanMock mock = SpanMock.CreateMock();

			_ = mock[new ReadOnlySpan<int>([1, 2, 3,])];

			await That(mock.Mock.Verify[It.IsReadOnlySpan<int>(v => v.Length == 2)].Got()).Never();
			await That(mock.Mock.Verify[It.IsReadOnlySpan<int>(v => v.Length == 3)].Got()).Once();
			await That(mock.Mock.Verify[It.IsReadOnlySpan<int>(v => v.Length == 4)].Got()).Never();
		}

		[Fact]
		public async Task ReadOnlySpan_WithoutPredicate_ShouldApplyAllCalls()
		{
			SpanMock mock = SpanMock.CreateMock();

			_ = mock[new ReadOnlySpan<int>()];
			_ = mock[new Span<int>([1, 2, 3,])];
			_ = mock[new ReadOnlySpan<int>([1, 2,])];

			await That(mock.Mock.Verify[It.IsAnyReadOnlySpan<int>()].Got()).Twice();
		}

		[Fact]
		public async Task Span_WhenPredicateMatches_ShouldApplySetup()
		{
			SpanMock mock = SpanMock.CreateMock();

			_ = mock[new Span<int>([1, 2, 3,])];

			await That(mock.Mock.Verify[It.IsSpan<int>(v => v.Length == 2)].Got()).Never();
			await That(mock.Mock.Verify[It.IsSpan<int>(v => v.Length == 3)].Got()).Once();
			await That(mock.Mock.Verify[It.IsSpan<int>(v => v.Length == 4)].Got()).Never();
		}

		[Fact]
		public async Task Span_WithoutPredicate_ShouldApplyAllCalls()
		{
			SpanMock mock = SpanMock.CreateMock();

			_ = mock[new Span<int>()];
			_ = mock[new ReadOnlySpan<int>([1, 2, 3,])];
			_ = mock[new Span<int>([1, 2,])];

			await That(mock.Mock.Verify[It.IsAnySpan<int>()].Got()).Twice();
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
