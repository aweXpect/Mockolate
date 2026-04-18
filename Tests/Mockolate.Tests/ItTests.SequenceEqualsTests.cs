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
            var sut = ISequenceCollectionConsumer.CreateMock();
            sut.Mock.Setup.WithArray(It.SequenceEquals(1, 2, 3)).Returns(42);

            var hit = sut.WithArray([1, 2, 3,]);
            var miss = sut.WithArray([1, 2, 4,]);

            await That(hit).IsEqualTo(42);
            await That(miss).IsEqualTo(0);
        }

        [Fact]
        public async Task ShouldMatchEnumerableParameter()
        {
            var sut = ISequenceCollectionConsumer.CreateMock();
            sut.Mock.Setup.WithEnumerable(It.SequenceEquals(1, 2, 3)).Returns(42);

            var hit = sut.WithEnumerable(Enumerable.Range(1, 3));
            var miss = sut.WithEnumerable(Enumerable.Range(2, 3));

            await That(hit).IsEqualTo(42);
            await That(miss).IsEqualTo(0);
        }

        [Fact]
        public async Task ShouldMatchListParameter()
        {
            var sut = ISequenceCollectionConsumer.CreateMock();
            sut.Mock.Setup.WithList(It.SequenceEquals(1, 2, 3)).Returns(42);

            var hit = sut.WithList([1, 2, 3,]);
            var miss = sut.WithList([3, 2, 1,]);

            await That(hit).IsEqualTo(42);
            await That(miss).IsEqualTo(0);
        }

        [Fact]
        public async Task ShouldMatchQueueParameter()
        {
            var sut = ISequenceCollectionConsumer.CreateMock();
            sut.Mock.Setup.WithQueue(It.SequenceEquals(1, 2, 3)).Returns(42);

            var hit = sut.WithQueue(new Queue<int>([1, 2, 3,]));
            var miss = sut.WithQueue(new Queue<int>([1, 2, 4,]));

            await That(hit).IsEqualTo(42);
            await That(miss).IsEqualTo(0);
        }

        [Fact]
        public async Task ShouldMatchReadOnlyListParameter()
        {
            var sut = ISequenceCollectionConsumer.CreateMock();
            sut.Mock.Setup.WithReadOnlyList(It.SequenceEquals(1, 2, 3)).Returns(42);

            var hit = sut.WithReadOnlyList([1, 2, 3,]);
            var miss = sut.WithReadOnlyList([1, 2,]);

            await That(hit).IsEqualTo(42);
            await That(miss).IsEqualTo(0);
        }

        [Fact]
        public async Task ShouldMatchSetParameter()
        {
            var sut = ISequenceCollectionConsumer.CreateMock();
            sut.Mock.Setup.WithSet(It.SequenceEquals(1, 2, 3)).Returns(42);

            var hit = sut.WithSet(new HashSet<int> { 1, 2, 3, });
            var miss = sut.WithSet(new HashSet<int> { 1, 2, 4, });

            await That(hit).IsEqualTo(42);
            await That(miss).IsEqualTo(0);
        }

        [Fact]
        public async Task ShouldMatchStackParameter()
        {
            var sut = ISequenceCollectionConsumer.CreateMock();
            sut.Mock.Setup.WithStack(It.SequenceEquals(3, 2, 1)).Returns(42);

            var hit = sut.WithStack(new Stack<int>([1, 2, 3,]));
            var miss = sut.WithStack(new Stack<int>([1, 2, 4,]));

            await That(hit).IsEqualTo(42);
            await That(miss).IsEqualTo(0);
        }

        [Fact]
        public async Task ShouldMatchWhenSequencesEqual()
        {
            IParameter<IEnumerable<int>> sut = It.SequenceEquals(1, 2, 3);

            var result = ((IParameterMatch<IEnumerable<int>>)sut).Matches([1, 2, 3,]);

            await That(result).IsTrue();
        }

        [Fact]
        public async Task ShouldNotMatchWhenLengthsDiffer()
        {
            IParameter<IEnumerable<int>> sut = It.SequenceEquals(1, 2, 3);

            var result = ((IParameterMatch<IEnumerable<int>>)sut).Matches([1, 2,]);

            await That(result).IsFalse();
        }

        [Fact]
        public async Task ShouldNotMatchWhenSequencesDiffer()
        {
            IParameter<IEnumerable<int>> sut = It.SequenceEquals(1, 2, 3);

            var result = ((IParameterMatch<IEnumerable<int>>)sut).Matches([1, 2, 4,]);

            await That(result).IsFalse();
        }

        [Fact]
        public async Task ShouldNotMatchWhenValueIsNotCollection()
        {
            IParameter<IEnumerable<int>> sut = It.SequenceEquals(1, 2, 3);

            var result = sut.Matches(42);

            await That(result).IsFalse();
        }

        [Fact]
        public async Task ShouldSupportVerify()
        {
            var sut = ISequenceCollectionConsumer.CreateMock();

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
            var expectedValue = "It.SequenceEquals(1, 2, 3)";

            var result = sut.ToString();

            await That(result).IsEqualTo(expectedValue);
        }

        [Fact]
        public async Task ToString_Using_ShouldReturnExpectedValue()
        {
            IParameter<int[]> sut = It.SequenceEquals(1, 2, 3).Using(new AllEqualComparer());
            var expectedValue = "It.SequenceEquals(1, 2, 3).Using(new AllEqualComparer())";

            var result = sut.ToString();

            await That(result).IsEqualTo(expectedValue);
        }

        [Fact]
        public async Task ToString_WithStringValues_ShouldReturnExpectedValue()
        {
            IParameter<string[]> sut = It.SequenceEquals("foo", "bar");
            var expectedValue = "It.SequenceEquals(\"foo\", \"bar\")";

            var result = sut.ToString();

            await That(result).IsEqualTo(expectedValue);
        }

        [Fact]
        public async Task WithComparer_ShouldUseComparer()
        {
            IParameter<int[]> sut = It.SequenceEquals(1, 2, 3).Using(new AllEqualComparer());

            var result = ((IParameterMatch<int[]>)sut).Matches([9, 9, 9,]);

            await That(result).IsTrue();
        }

        public interface ISequenceCollectionConsumer
        {
            int WithArray(int[] items);
            int WithList(List<int> items);
            int WithEnumerable(IEnumerable<int> items);
            int WithQueue(Queue<int> items);
            int WithReadOnlyList(IReadOnlyList<int> items);
            int WithSet(ISet<int> items);
            int WithStack(Stack<int> items);
        }
    }
}
