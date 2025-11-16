#if NET8_0_OR_GREATER

namespace Mockolate.Tests.MockIndexers;

public sealed partial class VerifySetIndexerTests
{
	public sealed class SpanTests
	{
		[Fact]
		public async Task Memory_WhenPredicateMatches_ShouldApplySetup()
		{
			ISpanMock mock = Mock.Create<ISpanMock>(MockBehavior.Default with
			{
				BaseClassBehavior = BaseClassBehavior.UseBaseClassAsDefaultValue,
			});

			mock[new Memory<int>([1, 2, 3,])] = 3;

			await That(mock.VerifyMock.SetIndexer(With<Memory<int>>(v => v.Length == 2), Any<int>())).Never();
			await That(mock.VerifyMock.SetIndexer(With<Memory<int>>(v => v.Length == 3), Any<int>())).Once();
			await That(mock.VerifyMock.SetIndexer(With<Memory<int>>(v => v.Length == 4), Any<int>())).Never();
		}

		[Fact]
		public async Task Memory_WithoutPredicate_ShouldApplyAllCalls()
		{
			ISpanMock mock = Mock.Create<ISpanMock>(MockBehavior.Default with
			{
				BaseClassBehavior = BaseClassBehavior.UseBaseClassAsDefaultValue,
			});

			mock[new Memory<int>()] = 3;
			mock[new Span<int>([1, 2, 3,])] = 3;
			mock[new Memory<int>([1, 2,])] = 3;

			await That(mock.VerifyMock.SetIndexer(Any<Memory<int>>(), Any<int>())).Twice();
		}

		[Fact]
		public async Task ReadOnlySpan_WhenPredicateMatches_ShouldApplySetup()
		{
			ISpanMock mock = Mock.Create<ISpanMock>(MockBehavior.Default with
			{
				BaseClassBehavior = BaseClassBehavior.UseBaseClassAsDefaultValue,
			});

			mock[new ReadOnlySpan<int>([1, 2, 3,])] = 3;

			await That(mock.VerifyMock.SetIndexer(WithReadOnlySpan<int>(v => v.Length == 2), Any<int>())).Never();
			await That(mock.VerifyMock.SetIndexer(WithReadOnlySpan<int>(v => v.Length == 3), Any<int>())).Once();
			await That(mock.VerifyMock.SetIndexer(WithReadOnlySpan<int>(v => v.Length == 4), Any<int>())).Never();
		}

		[Fact]
		public async Task ReadOnlySpan_WithoutPredicate_ShouldApplyAllCalls()
		{
			ISpanMock mock = Mock.Create<ISpanMock>(MockBehavior.Default with
			{
				BaseClassBehavior = BaseClassBehavior.UseBaseClassAsDefaultValue,
			});

			mock[new ReadOnlySpan<int>()] = 3;
			mock[new Span<int>([1, 2, 3,])] = 3;
			mock[new ReadOnlySpan<int>([1, 2,])] = 3;

			await That(mock.VerifyMock.SetIndexer(AnyReadOnlySpan<int>(), Any<int>())).Twice();
		}

		[Fact]
		public async Task Span_WhenPredicateMatches_ShouldApplySetup()
		{
			ISpanMock mock = Mock.Create<ISpanMock>(MockBehavior.Default with
			{
				BaseClassBehavior = BaseClassBehavior.UseBaseClassAsDefaultValue,
			});

			mock[new Span<int>([1, 2, 3,])] = 3;

			await That(mock.VerifyMock.SetIndexer(WithSpan<int>(v => v.Length == 2), Any<int>())).Never();
			await That(mock.VerifyMock.SetIndexer(WithSpan<int>(v => v.Length == 3), Any<int>())).Once();
			await That(mock.VerifyMock.SetIndexer(WithSpan<int>(v => v.Length == 4), Any<int>())).Never();
		}

		[Fact]
		public async Task Span_WithoutPredicate_ShouldApplyAllCalls()
		{
			ISpanMock mock = Mock.Create<ISpanMock>(MockBehavior.Default with
			{
				BaseClassBehavior = BaseClassBehavior.UseBaseClassAsDefaultValue,
			});

			mock[new Span<int>()] = 3;
			mock[new ReadOnlySpan<int>([1, 2, 3,])] = 3;
			mock[new Span<int>([1, 2,])] = 3;

			await That(mock.VerifyMock.SetIndexer(AnySpan<int>(), Any<int>())).Twice();
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
