#if NET8_0_OR_GREATER

using Mockolate.Setup;

namespace Mockolate.Tests.MockIndexers;

public sealed partial class SetupIndexerTests
{
	public sealed class SpanReturnTests
	{
		[Fact]
		public async Task Span_IndexerReturnValueSetup_ShouldReturnExpectedSpan()
		{
			SpanReturnIndexerMock mock = Mock.Create<SpanReturnIndexerMock>();
			var expectedData = new int[] { 1, 2, 3 };
			mock.SetupMock.Indexer(Any<int>()).Returns(new SpanWrapper<int>(expectedData));

			int[] result = mock[5].ToArray();

			await That(result).IsEqualTo(expectedData);
		}

		[Fact]
		public async Task Span_IndexerWithSpecificKey_ShouldMatchAndReturn()
		{
			SpanReturnIndexerMock mock = Mock.Create<SpanReturnIndexerMock>();
			var key1Data = new int[] { 10, 20 };
			var key2Data = new int[] { 30, 40, 50 };
			
			mock.SetupMock.Indexer(With<int>(k => k == 1)).Returns(new SpanWrapper<int>(key1Data));
			mock.SetupMock.Indexer(With<int>(k => k == 2)).Returns(new SpanWrapper<int>(key2Data));

			int[] result1 = mock[1].ToArray();
			int[] result2 = mock[2].ToArray();

			await That(result1).IsEqualTo(key1Data);
			await That(result2).IsEqualTo(key2Data);
		}

		[Fact]
		public async Task ReadOnlySpan_IndexerReturnValueSetup_ShouldReturnExpectedSpan()
		{
			ReadOnlySpanReturnIndexerMock mock = Mock.Create<ReadOnlySpanReturnIndexerMock>();
			var expectedData = new char[] { 'x', 'y', 'z' };
			mock.SetupMock.Indexer(Any<string>()).Returns(new ReadOnlySpanWrapper<char>(expectedData));

			char[] result = mock["test"].ToArray();

			await That(result).IsEqualTo(expectedData);
		}

		internal abstract class SpanReturnIndexerMock
		{
			public abstract Span<int> this[int key] { get; }
		}

		internal abstract class ReadOnlySpanReturnIndexerMock
		{
			public abstract ReadOnlySpan<char> this[string key] { get; }
		}
	}
}
#endif
