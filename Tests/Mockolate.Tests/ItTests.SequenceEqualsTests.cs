using System.Collections.Generic;
using System.Linq;
using Mockolate.Parameters;

namespace Mockolate.Tests;

public sealed partial class ItTests
{
	public sealed class SequenceEqualsTests
	{
		[Fact]
		public async Task ShouldMatchArrayParameter()
		{
			ISequenceCollectionConsumer sut = ISequenceCollectionConsumer.CreateMock();
			sut.Mock.Setup.WithArray(It.SequenceEquals(1, 2, 3)).Returns(42);

			int hit = sut.WithArray([1, 2, 3,]);
			int miss = sut.WithArray([1, 2, 4,]);

			await That(hit).IsEqualTo(42);
			await That(miss).IsEqualTo(0);
		}

		[Fact]
		public async Task ShouldMatchEnumerableParameter()
		{
			ISequenceCollectionConsumer sut = ISequenceCollectionConsumer.CreateMock();
			sut.Mock.Setup.WithEnumerable(It.SequenceEquals(1, 2, 3)).Returns(42);

			int hit = sut.WithEnumerable(Enumerable.Range(1, 3));
			int miss = sut.WithEnumerable(Enumerable.Range(2, 3));

			await That(hit).IsEqualTo(42);
			await That(miss).IsEqualTo(0);
		}

		[Fact]
		public async Task ShouldMatchListParameter()
		{
			ISequenceCollectionConsumer sut = ISequenceCollectionConsumer.CreateMock();
			sut.Mock.Setup.WithList(It.SequenceEquals(1, 2, 3)).Returns(42);

			int hit = sut.WithList([1, 2, 3,]);
			int miss = sut.WithList([3, 2, 1,]);

			await That(hit).IsEqualTo(42);
			await That(miss).IsEqualTo(0);
		}

		[Fact]
		public async Task ShouldMatchQueueParameter()
		{
			ISequenceCollectionConsumer sut = ISequenceCollectionConsumer.CreateMock();
			sut.Mock.Setup.WithQueue(It.SequenceEquals(1, 2, 3)).Returns(42);

			int hit = sut.WithQueue(new Queue<int>([1, 2, 3,]));
			int miss = sut.WithQueue(new Queue<int>([1, 2, 4,]));

			await That(hit).IsEqualTo(42);
			await That(miss).IsEqualTo(0);
		}

		[Fact]
		public async Task ShouldMatchReadOnlyListParameter()
		{
			ISequenceCollectionConsumer sut = ISequenceCollectionConsumer.CreateMock();
			sut.Mock.Setup.WithReadOnlyList(It.SequenceEquals(1, 2, 3)).Returns(42);

			int hit = sut.WithReadOnlyList([1, 2, 3,]);
			int miss = sut.WithReadOnlyList([1, 2,]);

			await That(hit).IsEqualTo(42);
			await That(miss).IsEqualTo(0);
		}

		[Fact]
		public async Task ShouldMatchStackParameter()
		{
			ISequenceCollectionConsumer sut = ISequenceCollectionConsumer.CreateMock();
			sut.Mock.Setup.WithStack(It.SequenceEquals(3, 2, 1)).Returns(42);

			int hit = sut.WithStack(new Stack<int>([1, 2, 3,]));
			int miss = sut.WithStack(new Stack<int>([1, 2, 4,]));

			await That(hit).IsEqualTo(42);
			await That(miss).IsEqualTo(0);
		}

		[Fact]
		public async Task ShouldMatchWhenSequencesEqual()
		{
			IParameter<IEnumerable<int>> sut = It.SequenceEquals(1, 2, 3);

			bool result = ((IParameterMatch<IEnumerable<int>>)sut).Matches([1, 2, 3,]);

			await That(result).IsTrue();
		}

		[Fact]
		public async Task ShouldNotMatchWhenLengthsDiffer()
		{
			IParameter<IEnumerable<int>> sut = It.SequenceEquals(1, 2, 3);

			bool result = ((IParameterMatch<IEnumerable<int>>)sut).Matches([1, 2,]);

			await That(result).IsFalse();
		}

		[Fact]
		public async Task ShouldNotMatchWhenSequencesDiffer()
		{
			IParameter<IEnumerable<int>> sut = It.SequenceEquals(1, 2, 3);

			bool result = ((IParameterMatch<IEnumerable<int>>)sut).Matches([1, 2, 4,]);

			await That(result).IsFalse();
		}

		[Fact]
		public async Task ShouldNotMatchWhenValueIsNotCollection()
		{
			IParameter<IEnumerable<int>> sut = It.SequenceEquals(1, 2, 3);

			bool result = sut.Matches(42);

			await That(result).IsFalse();
		}

		[Fact]
		public async Task ShouldSupportVerify()
		{
			ISequenceCollectionConsumer sut = ISequenceCollectionConsumer.CreateMock();

			sut.WithArray([1, 2, 3,]);
			sut.WithArray([1, 2, 3,]);
			sut.WithArray([9, 9, 9,]);

			await That(sut.Mock.Verify.WithArray(It.SequenceEquals(1, 2, 3))).Exactly(2);
			await That(sut.Mock.Verify.WithArray(It.SequenceEquals(9, 9, 9))).Once();
		}

		[Fact]
		public async Task ToString_ShouldReturnExpectedValue()
		{
			IParameter<int[]> sut = It.SequenceEquals(1, 2, 3);
			string expectedValue = "It.SequenceEquals(1, 2, 3)";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}

		[Fact]
		public async Task ToString_Using_ShouldReturnExpectedValue()
		{
			IParameter<int[]> sut = It.SequenceEquals(1, 2, 3).Using(new AllEqualComparer());
			string expectedValue = "It.SequenceEquals(1, 2, 3).Using(new AllEqualComparer())";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}

		[Fact]
		public async Task ToString_WithStringValues_ShouldReturnExpectedValue()
		{
			IParameter<string[]> sut = It.SequenceEquals("foo", "bar");
			string expectedValue = "It.SequenceEquals(\"foo\", \"bar\")";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}

		[Fact]
		public async Task WithComparer_ShouldUseComparer()
		{
			IParameter<int[]> sut = It.SequenceEquals(1, 2, 3).Using(new AllEqualComparer());

			bool result = ((IParameterMatch<int[]>)sut).Matches([9, 9, 9,]);

			await That(result).IsTrue();
		}

		public interface ISequenceCollectionConsumer
		{
			int WithArray(int[] items);
			int WithList(List<int> items);
			int WithEnumerable(IEnumerable<int> items);
			int WithQueue(Queue<int> items);
			int WithReadOnlyList(IReadOnlyList<int> items);
			int WithStack(Stack<int> items);
		}
	}
}
