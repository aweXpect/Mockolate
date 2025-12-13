#if NET8_0_OR_GREATER

namespace Mockolate.Tests.MockMethods;

public sealed partial class VerifyInvokedTests
{
	public sealed class SpanTests
	{
		[Fact]
		public async Task Memory_WhenPredicateMatches_ShouldApplySetup()
		{
			SpanMock mock = Mock.Create<SpanMock>(MockBehavior.Default.CallingBaseClass());

			mock.MyMethod(new Memory<int>([1, 2, 3,]));

			await That(mock.VerifyMock.Invoked.MyMethod(It.Satisfies<Memory<int>>(v => v.Length == 2))).Never();
			await That(mock.VerifyMock.Invoked.MyMethod(It.Satisfies<Memory<int>>(v => v.Length == 3))).Once();
			await That(mock.VerifyMock.Invoked.MyMethod(It.Satisfies<Memory<int>>(v => v.Length == 4))).Never();
		}

		[Fact]
		public async Task Memory_WithoutPredicate_ShouldApplyAllCalls()
		{
			SpanMock mock = Mock.Create<SpanMock>(MockBehavior.Default.CallingBaseClass());

			mock.MyMethod(new Memory<int>());
			mock.MyMethod(new Span<int>([1, 2, 3,]));
			mock.MyMethod(new Memory<int>([1, 2,]));

			await That(mock.VerifyMock.Invoked.MyMethod(It.IsAny<Memory<int>>())).Twice();
		}

		[Fact]
		public async Task ReadOnlySpan_WhenPredicateMatches_ShouldApplySetup()
		{
			SpanMock mock = Mock.Create<SpanMock>(MockBehavior.Default.CallingBaseClass());

			mock.MyMethod(new ReadOnlySpan<int>([1, 2, 3,]));

			await That(mock.VerifyMock.Invoked.MyMethod(It.IsReadOnlySpan<int>(v => v.Length == 2))).Never();
			await That(mock.VerifyMock.Invoked.MyMethod(It.IsReadOnlySpan<int>(v => v.Length == 3))).Once();
			await That(mock.VerifyMock.Invoked.MyMethod(It.IsReadOnlySpan<int>(v => v.Length == 4))).Never();
		}

		[Fact]
		public async Task ReadOnlySpan_WithoutPredicate_ShouldApplyAllCalls()
		{
			SpanMock mock = Mock.Create<SpanMock>(MockBehavior.Default.CallingBaseClass());

			mock.MyMethod(new ReadOnlySpan<int>());
			mock.MyMethod(new Span<int>([1, 2, 3,]));
			mock.MyMethod(new ReadOnlySpan<int>([1, 2,]));

			await That(mock.VerifyMock.Invoked.MyMethod(It.IsAnyReadOnlySpan<int>())).Twice();
		}

		[Fact]
		public async Task Span_WhenPredicateMatches_ShouldApplySetup()
		{
			SpanMock mock = Mock.Create<SpanMock>(MockBehavior.Default.CallingBaseClass());

			mock.MyMethod(new Span<int>([1, 2, 3,]));

			await That(mock.VerifyMock.Invoked.MyMethod(It.IsSpan<int>(v => v.Length == 2))).Never();
			await That(mock.VerifyMock.Invoked.MyMethod(It.IsSpan<int>(v => v.Length == 3))).Once();
			await That(mock.VerifyMock.Invoked.MyMethod(It.IsSpan<int>(v => v.Length == 4))).Never();
		}

		[Fact]
		public async Task Span_WithoutPredicate_ShouldApplyAllCalls()
		{
			SpanMock mock = Mock.Create<SpanMock>(MockBehavior.Default.CallingBaseClass());

			mock.MyMethod(new Span<int>());
			mock.MyMethod(new ReadOnlySpan<int>([1, 2, 3,]));
			mock.MyMethod(new Span<int>([1, 2,]));

			await That(mock.VerifyMock.Invoked.MyMethod(It.IsAnySpan<int>())).Twice();
		}

		internal class SpanMock
		{
			public virtual int MyMethod(Memory<int> value)
				=> value.Length;

			public virtual int MyMethod(ReadOnlySpan<int> value)
				=> value.Length;

			public virtual int MyMethod(Span<int> value)
				=> value.Length;
		}
	}
}
#endif
