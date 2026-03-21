#if NET8_0_OR_GREATER

namespace Mockolate.Tests.MockIndexers;

public sealed partial class VerifySetIndexerTests
{
	public sealed class SpanTests
	{
		[Fact]
		public async Task Memory_WhenPredicateMatches_ShouldApplySetup()
		{
			ISpanMock mock = ISpanMock.CreateMock();

			mock[new Memory<int>([1, 2, 3,])] = 3;

			await That(mock.Mock.Verify[It.Satisfies<Memory<int>>(v => v.Length == 2)].Set(It.IsAny<int>())).Never();
			await That(mock.Mock.Verify[It.Satisfies<Memory<int>>(v => v.Length == 3)].Set(It.IsAny<int>())).Once();
			await That(mock.Mock.Verify[It.Satisfies<Memory<int>>(v => v.Length == 4)].Set(It.IsAny<int>())).Never();
		}

		[Fact]
		public async Task Memory_WithoutPredicate_ShouldApplyAllCalls()
		{
			ISpanMock mock = ISpanMock.CreateMock();

			mock[new Memory<int>()] = 3;
			mock[new Span<int>([1, 2, 3,])] = 3;
			mock[new Memory<int>([1, 2,])] = 3;

			await That(mock.Mock.Verify[It.IsAny<Memory<int>>()].Set(It.IsAny<int>())).Twice();
		}

		[Fact]
		public async Task ReadOnlySpan_WhenPredicateMatches_ShouldApplySetup()
		{
			ISpanMock mock = ISpanMock.CreateMock();

			mock[new ReadOnlySpan<int>([1, 2, 3,])] = 3;

			await That(mock.Mock.Verify[It.IsReadOnlySpan<int>(v => v.Length == 2)].Set(It.IsAny<int>())).Never();
			await That(mock.Mock.Verify[It.IsReadOnlySpan<int>(v => v.Length == 3)].Set(It.IsAny<int>())).Once();
			await That(mock.Mock.Verify[It.IsReadOnlySpan<int>(v => v.Length == 4)].Set(It.IsAny<int>())).Never();
		}

		[Fact]
		public async Task ReadOnlySpan_WithoutPredicate_ShouldApplyAllCalls()
		{
			ISpanMock mock = ISpanMock.CreateMock();

			mock[new ReadOnlySpan<int>()] = 3;
			mock[new Span<int>([1, 2, 3,])] = 3;
			mock[new ReadOnlySpan<int>([1, 2,])] = 3;

			await That(mock.Mock.Verify[It.IsAnyReadOnlySpan<int>()].Set(It.IsAny<int>())).Twice();
		}

		[Fact]
		public async Task Span_WhenPredicateMatches_ShouldApplySetup()
		{
			ISpanMock mock = ISpanMock.CreateMock();

			mock[new Span<int>([1, 2, 3,])] = 3;

			await That(mock.Mock.Verify[It.IsSpan<int>(v => v.Length == 2)].Set(It.IsAny<int>())).Never();
			await That(mock.Mock.Verify[It.IsSpan<int>(v => v.Length == 3)].Set(It.IsAny<int>())).Once();
			await That(mock.Mock.Verify[It.IsSpan<int>(v => v.Length == 4)].Set(It.IsAny<int>())).Never();
		}

		[Fact]
		public async Task Span_WithoutPredicate_ShouldApplyAllCalls()
		{
			ISpanMock mock = ISpanMock.CreateMock();

			mock[new Span<int>()] = 3;
			mock[new ReadOnlySpan<int>([1, 2, 3,])] = 3;
			mock[new Span<int>([1, 2,])] = 3;

			await That(mock.Mock.Verify[It.IsAnySpan<int>()].Set(It.IsAny<int>())).Twice();
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
