using System.Collections.Generic;
using System.Linq;
using Mockolate.Parameters;

namespace Mockolate.Tests;

public sealed partial class ItTests
{
	public sealed class ContainsTests
	{
		[Fact]
		public async Task ShouldMatchArrayParameter()
		{
			ICollectionConsumer sut = ICollectionConsumer.CreateMock();
			sut.Mock.Setup.WithArray(It.Contains(5)).Returns(42);

			int hit = sut.WithArray([1, 2, 5,]);
			int miss = sut.WithArray([1, 2, 3,]);

			await That(hit).IsEqualTo(42);
			await That(miss).IsEqualTo(0);
		}

		[Fact]
		public async Task ShouldMatchEnumerableParameter()
		{
			ICollectionConsumer sut = ICollectionConsumer.CreateMock();
			sut.Mock.Setup.WithEnumerable(It.Contains(5)).Returns(42);

			int hit = sut.WithEnumerable(Enumerable.Range(3, 5));
			int miss = sut.WithEnumerable(Enumerable.Range(100, 5));

			await That(hit).IsEqualTo(42);
			await That(miss).IsEqualTo(0);
		}

		[Fact]
		public async Task ShouldMatchHashSetParameter()
		{
			ICollectionConsumer sut = ICollectionConsumer.CreateMock();
			sut.Mock.Setup.WithHashSet(It.Contains(5)).Returns(42);

			int hit = sut.WithHashSet([4, 5, 6,]);
			int miss = sut.WithHashSet([1, 2, 3,]);

			await That(hit).IsEqualTo(42);
			await That(miss).IsEqualTo(0);
		}

		[Fact]
		public async Task ShouldMatchListParameter()
		{
			ICollectionConsumer sut = ICollectionConsumer.CreateMock();
			sut.Mock.Setup.WithList(It.Contains(5)).Returns(42);

			int hit = sut.WithList([1, 2, 5,]);
			int miss = sut.WithList([1, 2, 3,]);

			await That(hit).IsEqualTo(42);
			await That(miss).IsEqualTo(0);
		}

		[Fact]
		public async Task ShouldMatchQueueParameter()
		{
			ICollectionConsumer sut = ICollectionConsumer.CreateMock();
			sut.Mock.Setup.WithQueue(It.Contains(5)).Returns(42);

			int hit = sut.WithQueue(new Queue<int>([1, 2, 5,]));
			int miss = sut.WithQueue(new Queue<int>([1, 2, 3,]));

			await That(hit).IsEqualTo(42);
			await That(miss).IsEqualTo(0);
		}

		[Fact]
		public async Task ShouldMatchReadOnlyListParameter()
		{
			ICollectionConsumer sut = ICollectionConsumer.CreateMock();
			sut.Mock.Setup.WithReadOnlyList(It.Contains(5)).Returns(42);

			int hit = sut.WithReadOnlyList([1, 5, 9,]);
			int miss = sut.WithReadOnlyList([2, 3, 4,]);

			await That(hit).IsEqualTo(42);
			await That(miss).IsEqualTo(0);
		}

		[Fact]
		public async Task ShouldMatchSetParameter()
		{
			ICollectionConsumer sut = ICollectionConsumer.CreateMock();
			sut.Mock.Setup.WithSet(It.Contains(5)).Returns(42);

			int hit = sut.WithSet(new HashSet<int>
			{
				4,
				5,
				6,
			});
			int miss = sut.WithSet(new HashSet<int>
			{
				1,
				2,
				3,
			});

			await That(hit).IsEqualTo(42);
			await That(miss).IsEqualTo(0);
		}

		[Fact]
		public async Task ShouldMatchStackParameter()
		{
			ICollectionConsumer sut = ICollectionConsumer.CreateMock();
			sut.Mock.Setup.WithStack(It.Contains(5)).Returns(42);

			int hit = sut.WithStack(new Stack<int>([1, 2, 5,]));
			int miss = sut.WithStack(new Stack<int>([1, 2, 3,]));

			await That(hit).IsEqualTo(42);
			await That(miss).IsEqualTo(0);
		}

		[Theory]
		[InlineData(1, false)]
		[InlineData(4, false)]
		[InlineData(5, true)]
		[InlineData(6, true)]
		[InlineData(42, false)]
		public async Task ShouldMatchWhenEnumerableContainsItem(int item, bool expectMatch)
		{
			IParameter<IEnumerable<int>> sut = It.Contains(item);

			bool result = ((IParameterMatch<IEnumerable<int>>)sut).Matches([5, 6, 7,]);

			await That(result).IsEqualTo(expectMatch);
		}

		[Fact]
		public async Task ShouldNotMatchWhenValueIsNotCollection()
		{
			IParameter<IEnumerable<int>> sut = It.Contains(5);

			bool result = sut.Matches("not-a-collection");

			await That(result).IsFalse();
		}

		[Fact]
		public async Task ShouldNotMatchWhenValueIsNull()
		{
			IParameter<IEnumerable<int>> sut = It.Contains(5);

			bool result = sut.Matches(null);

			await That(result).IsFalse();
		}

		[Fact]
		public async Task ShouldSupportVerify()
		{
			ICollectionConsumer sut = ICollectionConsumer.CreateMock();

			sut.WithArray([1, 2, 5,]);
			sut.WithArray([1, 2, 3,]);

			await That(sut.Mock.Verify.WithArray(It.Contains(5))).Once();
			await That(sut.Mock.Verify.WithArray(It.Contains(3))).Once();
			await That(sut.Mock.Verify.WithArray(It.Contains(99))).Never();
		}

		[Fact]
		public async Task ToString_ShouldReturnExpectedValue()
		{
			IParameter<int[]> sut = It.Contains(5);
			string expectedValue = "It.Contains(5)";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}

		[Fact]
		public async Task ToString_Using_ShouldReturnExpectedValue()
		{
			IParameter<int[]> sut = It.Contains(5).Using(new AllEqualComparer());
			string expectedValue = "It.Contains(5).Using(new AllEqualComparer())";

			string? result = sut.ToString();

			await That(result).IsEqualTo(expectedValue);
		}

		[Fact]
		public async Task WithComparer_ShouldUseComparer()
		{
			IParameter<int[]> sut = It.Contains(42).Using(new AllEqualComparer());

			bool result = ((IParameterMatch<int[]>)sut).Matches([1, 2, 3,]);

			await That(result).IsTrue();
		}

		public interface ICollectionConsumer
		{
			int WithArray(int[] items);
			int WithList(List<int> items);
			int WithEnumerable(IEnumerable<int> items);
			int WithHashSet(HashSet<int> items);
			int WithQueue(Queue<int> items);
			int WithReadOnlyList(IReadOnlyList<int> items);
			int WithSet(ISet<int> items);
			int WithStack(Stack<int> items);
		}
	}
}
