#if NET8_0_OR_GREATER

namespace Mockolate.Tests.MockMethods;

public sealed partial class SetupMethodTests
{
	public sealed class SpanTests
	{
		[Fact]
		public async Task Memory_WhenPredicateDoesNotMatch_ShouldUseDefaultValue()
		{
			SpanMock mock = Mock.Create<SpanMock>(MockBehavior.Default with
			{
				BaseClassBehavior = BaseClassBehavior.UseBaseClassAsDefaultValue,
			});
			mock.SetupMock.Method.MyMethod(With<Memory<int>>(v => v.Length == 2)).Returns(4);

			int result = mock.MyMethod(new Memory<int>([1, 2, 3,]));

			await That(result).IsEqualTo(3);
		}

		[Fact]
		public async Task Memory_WhenPredicateMatches_ShouldApplySetup()
		{
			SpanMock mock = Mock.Create<SpanMock>(MockBehavior.Default with
			{
				BaseClassBehavior = BaseClassBehavior.UseBaseClassAsDefaultValue,
			});
			mock.SetupMock.Method.MyMethod(With<Memory<int>>(v => v.Length == 3)).Returns(42);

			int result = mock.MyMethod(new Memory<int>([1, 2, 3,]));

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task Memory_WithoutPredicate_ShouldMatchAnySpan()
		{
			SpanMock mock = Mock.Create<SpanMock>(MockBehavior.Default with
			{
				BaseClassBehavior = BaseClassBehavior.UseBaseClassAsDefaultValue,
			});
			mock.SetupMock.Method.MyMethod(Any<Memory<int>>()).Returns(42);

			int result = mock.MyMethod(new Memory<int>([1, 2, 3,]));

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task ReadOnlySpan_WhenPredicateDoesNotMatch_ShouldUseDefaultValue()
		{
			SpanMock mock = Mock.Create<SpanMock>(MockBehavior.Default with
			{
				BaseClassBehavior = BaseClassBehavior.UseBaseClassAsDefaultValue,
			});
			mock.SetupMock.Method.MyMethod(WithReadOnlySpan<int>(v => v.Length == 2)).Returns(4);

			int result = mock.MyMethod(new ReadOnlySpan<int>([1, 2, 3,]));

			await That(result).IsEqualTo(3);
		}

		[Fact]
		public async Task ReadOnlySpan_WhenPredicateMatches_ShouldApplySetup()
		{
			SpanMock mock = Mock.Create<SpanMock>(MockBehavior.Default with
			{
				BaseClassBehavior = BaseClassBehavior.UseBaseClassAsDefaultValue,
			});
			mock.SetupMock.Method.MyMethod(WithReadOnlySpan<int>(v => v.Length == 3 && v[0] == 1)).Returns(42);

			int result = mock.MyMethod(new ReadOnlySpan<int>([1, 2, 3,]));

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task ReadOnlySpan_WithoutPredicate_ShouldMatchAnySpan()
		{
			SpanMock mock = Mock.Create<SpanMock>(MockBehavior.Default with
			{
				BaseClassBehavior = BaseClassBehavior.UseBaseClassAsDefaultValue,
			});
			mock.SetupMock.Method.MyMethod(AnyReadOnlySpan<int>()).Returns(42);

			int result = mock.MyMethod(new ReadOnlySpan<int>([1, 2, 3,]));

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task Span_WhenPredicateDoesNotMatch_ShouldUseDefaultValue()
		{
			SpanMock mock = Mock.Create<SpanMock>(MockBehavior.Default with
			{
				BaseClassBehavior = BaseClassBehavior.UseBaseClassAsDefaultValue,
			});
			mock.SetupMock.Method.MyMethod(WithSpan<int>(v => v.Length == 2)).Returns(4);

			int result = mock.MyMethod(new Span<int>([1, 2, 3,]));

			await That(result).IsEqualTo(3);
		}

		[Fact]
		public async Task Span_WhenPredicateMatches_ShouldApplySetup()
		{
			SpanMock mock = Mock.Create<SpanMock>(MockBehavior.Default with
			{
				BaseClassBehavior = BaseClassBehavior.UseBaseClassAsDefaultValue,
			});
			mock.SetupMock.Method.MyMethod(WithSpan<int>(v => v.Length == 3 && v[0] == 1)).Returns(42);

			int result = mock.MyMethod(new Span<int>([1, 2, 3,]));

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task Span_WithoutPredicate_ShouldMatchAnySpan()
		{
			SpanMock mock = Mock.Create<SpanMock>(MockBehavior.Default with
			{
				BaseClassBehavior = BaseClassBehavior.UseBaseClassAsDefaultValue,
			});
			mock.SetupMock.Method.MyMethod(AnySpan<int>()).Returns(42);

			int result = mock.MyMethod(new Span<int>([1, 2, 3,]));

			await That(result).IsEqualTo(42);
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
