#if NET8_0_OR_GREATER

using Mockolate.Setup;

namespace Mockolate.Tests.MockMethods;

public sealed partial class SetupMethodTests
{
	public sealed class SpanReturnTests
	{
		[Fact]
		public async Task Span_ReturnValueSetup_ShouldReturnExpectedSpan()
		{
			SpanReturnMock mock = Mock.Create<SpanReturnMock>();
			var expectedData = new int[] { 1, 2, 3 };
			mock.SetupMock.Method.GetSpan().Returns(new SpanWrapper<int>(expectedData));

			int[] result = mock.GetSpan().ToArray();

			await That(result).IsEqualTo(expectedData);
		}

		[Fact]
		public async Task Span_MultipleReturnValues_ShouldCycleThroughValues()
		{
			SpanReturnMock mock = Mock.Create<SpanReturnMock>();
			var firstData = new int[] { 1, 2 };
			var secondData = new int[] { 3, 4, 5 };
			mock.SetupMock.Method.GetSpan()
				.Returns(new SpanWrapper<int>(firstData))
				.Returns(new SpanWrapper<int>(secondData));

			int[] firstResult = mock.GetSpan().ToArray();
			int[] secondResult = mock.GetSpan().ToArray();
			int[] thirdResult = mock.GetSpan().ToArray(); // Should cycle back to first

			await That(firstResult).IsEqualTo(firstData);
			await That(secondResult).IsEqualTo(secondData);
			await That(thirdResult).IsEqualTo(firstData);
		}

		[Fact]
		public async Task ReadOnlySpan_ReturnValueSetup_ShouldReturnExpectedSpan()
		{
			SpanReturnMock mock = Mock.Create<SpanReturnMock>();
			var expectedData = new char[] { 'a', 'b', 'c' };
			mock.SetupMock.Method.GetReadOnlySpan().Returns(new ReadOnlySpanWrapper<char>(expectedData));

			char[] result = mock.GetReadOnlySpan().ToArray();

			await That(result).IsEqualTo(expectedData);
		}

		[Fact]
		public async Task ReadOnlySpan_WithParameter_ShouldMatchAndReturn()
		{
			SpanReturnMock mock = Mock.Create<SpanReturnMock>();
			var inputData = new byte[] { 10, 20, 30 };
			var outputData = new byte[] { 100, 200 };
			
			mock.SetupMock.Method.ProcessData(AnySpan<byte>())
				.Returns(new ReadOnlySpanWrapper<byte>(outputData));

			byte[] result = mock.ProcessData(new Span<byte>(inputData)).ToArray();

			await That(result).IsEqualTo(outputData);
		}

		[Fact]
		public async Task Span_WithCallback_ShouldExecuteCallback()
		{
			SpanReturnMock mock = Mock.Create<SpanReturnMock>();
			var callbackExecuted = false;
			var returnData = new int[] { 42 };
			
			mock.SetupMock.Method.GetSpan()
				.Do(() => callbackExecuted = true)
				.Returns(new SpanWrapper<int>(returnData));

			int[] result = mock.GetSpan().ToArray();

			await That(callbackExecuted).IsTrue();
			await That(result).IsEqualTo(returnData);
		}

		internal abstract class SpanReturnMock
		{
			public abstract Span<int> GetSpan();
			public abstract ReadOnlySpan<char> GetReadOnlySpan();
			public abstract ReadOnlySpan<byte> ProcessData(Span<byte> input);
		}
	}
}
#endif
