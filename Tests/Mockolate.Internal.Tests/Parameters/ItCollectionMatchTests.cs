using System.Collections.Generic;
using Mockolate.Parameters;

namespace Mockolate.Internal.Tests.Parameters;

public class ItCollectionMatchTests
{
	public sealed class CollectionMatchDoTests
	{
		[Fact]
		public async Task Do_RegistersCallbackForArray()
		{
			It.IContainsParameter<int> sut = It.Contains(1);
			int[]? captured = null;
			((IParameterWithCallback<int[]>)sut).Do(value => captured = value);

			int[] source = [1, 2, 3,];
			((IParameterMatch<int[]>)sut).InvokeCallbacks(source);

			await That(captured).IsSameAs(source);
		}

		[Fact]
		public async Task Do_RegistersCallbackForHashSet()
		{
			It.IContainsParameter<int> sut = It.Contains(1);
			HashSet<int>? captured = null;
			((IParameterWithCallback<HashSet<int>>)sut).Do(value => captured = value);

			HashSet<int> source = [1, 2, 3,];
			((IParameterMatch<HashSet<int>>)sut).InvokeCallbacks(source);

			await That(captured).IsSameAs(source);
		}

		[Fact]
		public async Task Do_RegistersCallbackForICollection()
		{
			It.IContainsParameter<int> sut = It.Contains(1);
			ICollection<int>? captured = null;
			((IParameterWithCallback<ICollection<int>>)sut).Do(value => captured = value);

			List<int> source = [1, 2, 3,];
			((IParameterMatch<ICollection<int>>)sut).InvokeCallbacks(source);

			await That(captured).IsSameAs(source);
		}

		[Fact]
		public async Task Do_RegistersCallbackForIEnumerable()
		{
			It.IContainsParameter<int> sut = It.Contains(1);
			IEnumerable<int>? captured = null;
			((IParameterWithCallback<IEnumerable<int>>)sut).Do(value => captured = value);

			List<int> source = [1, 2, 3,];
			((IParameterMatch<IEnumerable<int>>)sut).InvokeCallbacks(source);

			await That(captured).IsSameAs(source);
		}

		[Fact]
		public async Task Do_RegistersCallbackForIList()
		{
			It.IContainsParameter<int> sut = It.Contains(1);
			IList<int>? captured = null;
			((IParameterWithCallback<IList<int>>)sut).Do(value => captured = value);

			List<int> source = [1, 2, 3,];
			((IParameterMatch<IList<int>>)sut).InvokeCallbacks(source);

			await That(captured).IsSameAs(source);
		}

		[Fact]
		public async Task Do_RegistersCallbackForIReadOnlyCollection()
		{
			It.IContainsParameter<int> sut = It.Contains(1);
			IReadOnlyCollection<int>? captured = null;
			((IParameterWithCallback<IReadOnlyCollection<int>>)sut).Do(value => captured = value);

			List<int> source = [1, 2, 3,];
			((IParameterMatch<IReadOnlyCollection<int>>)sut).InvokeCallbacks(source);

			await That(captured).IsSameAs(source);
		}

		[Fact]
		public async Task Do_RegistersCallbackForIReadOnlyList()
		{
			It.IContainsParameter<int> sut = It.Contains(1);
			IReadOnlyList<int>? captured = null;
			((IParameterWithCallback<IReadOnlyList<int>>)sut).Do(value => captured = value);

			List<int> source = [1, 2, 3,];
			((IParameterMatch<IReadOnlyList<int>>)sut).InvokeCallbacks(source);

			await That(captured).IsSameAs(source);
		}

		[Fact]
		public async Task Do_RegistersCallbackForISet()
		{
			It.IContainsParameter<int> sut = It.Contains(1);
			ISet<int>? captured = null;
			((IParameterWithCallback<ISet<int>>)sut).Do(value => captured = value);

			HashSet<int> source = [1, 2, 3,];
			((IParameterMatch<ISet<int>>)sut).InvokeCallbacks(source);

			await That(captured).IsSameAs(source);
		}

		[Fact]
		public async Task Do_RegistersCallbackForList()
		{
			It.IContainsParameter<int> sut = It.Contains(1);
			List<int>? captured = null;
			((IParameterWithCallback<List<int>>)sut).Do(value => captured = value);

			List<int> source = [1, 2, 3,];
			((IParameterMatch<List<int>>)sut).InvokeCallbacks(source);

			await That(captured).IsSameAs(source);
		}

		[Fact]
		public async Task Do_RegistersCallbackForQueue()
		{
			It.IContainsParameter<int> sut = It.Contains(1);
			Queue<int>? captured = null;
			((IParameterWithCallback<Queue<int>>)sut).Do(value => captured = value);

			Queue<int> source = new();
			source.Enqueue(1);
			((IParameterMatch<Queue<int>>)sut).InvokeCallbacks(source);

			await That(captured).IsSameAs(source);
		}

		[Fact]
		public async Task Do_RegistersCallbackForStack()
		{
			It.IContainsParameter<int> sut = It.Contains(1);
			Stack<int>? captured = null;
			((IParameterWithCallback<Stack<int>>)sut).Do(value => captured = value);

			Stack<int> source = new();
			source.Push(1);
			((IParameterMatch<Stack<int>>)sut).InvokeCallbacks(source);

			await That(captured).IsSameAs(source);
		}

		[Fact]
		public async Task InvokeCallbacks_OnNonEnumerableValue_ShortCircuits()
		{
			It.IContainsParameter<int> sut = It.Contains(1);
			int invocations = 0;
			((IParameterWithCallback<List<int>>)sut).Do(_ => invocations++);

			sut.InvokeCallbacks(new object());

			await That(invocations).IsEqualTo(0);
		}

		[Fact]
		public async Task InvokeCallbacks_WithoutAnyRegistration_DoesNotThrow()
		{
			It.IContainsParameter<int> sut = It.Contains(1);

			void Act()
			{
				sut.InvokeCallbacks(new List<int>
				{
					1,
				});
			}

			await That(Act).DoesNotThrow();
		}
	}

	public sealed class OrderedCollectionMatchDoTests
	{
		[Fact]
		public async Task Do_RegistersCallbackForArray()
		{
			It.ISequenceEqualsParameter<int> sut = It.SequenceEquals(1, 2, 3);
			int[]? captured = null;
			((IParameterWithCallback<int[]>)sut).Do(value => captured = value);

			int[] source = [1, 2, 3,];
			((IParameterMatch<int[]>)sut).InvokeCallbacks(source);

			await That(captured).IsSameAs(source);
		}

		[Fact]
		public async Task Do_RegistersCallbackForICollection()
		{
			It.ISequenceEqualsParameter<int> sut = It.SequenceEquals(1, 2, 3);
			ICollection<int>? captured = null;
			((IParameterWithCallback<ICollection<int>>)sut).Do(value => captured = value);

			List<int> source = [1, 2, 3,];
			((IParameterMatch<ICollection<int>>)sut).InvokeCallbacks(source);

			await That(captured).IsSameAs(source);
		}

		[Fact]
		public async Task Do_RegistersCallbackForIEnumerable()
		{
			It.ISequenceEqualsParameter<int> sut = It.SequenceEquals(1, 2, 3);
			IEnumerable<int>? captured = null;
			((IParameterWithCallback<IEnumerable<int>>)sut).Do(value => captured = value);

			List<int> source = [1, 2, 3,];
			((IParameterMatch<IEnumerable<int>>)sut).InvokeCallbacks(source);

			await That(captured).IsSameAs(source);
		}

		[Fact]
		public async Task Do_RegistersCallbackForIList()
		{
			It.ISequenceEqualsParameter<int> sut = It.SequenceEquals(1, 2, 3);
			IList<int>? captured = null;
			((IParameterWithCallback<IList<int>>)sut).Do(value => captured = value);

			List<int> source = [1, 2, 3,];
			((IParameterMatch<IList<int>>)sut).InvokeCallbacks(source);

			await That(captured).IsSameAs(source);
		}

		[Fact]
		public async Task Do_RegistersCallbackForIReadOnlyCollection()
		{
			It.ISequenceEqualsParameter<int> sut = It.SequenceEquals(1, 2, 3);
			IReadOnlyCollection<int>? captured = null;
			((IParameterWithCallback<IReadOnlyCollection<int>>)sut).Do(value => captured = value);

			List<int> source = [1, 2, 3,];
			((IParameterMatch<IReadOnlyCollection<int>>)sut).InvokeCallbacks(source);

			await That(captured).IsSameAs(source);
		}

		[Fact]
		public async Task Do_RegistersCallbackForIReadOnlyList()
		{
			It.ISequenceEqualsParameter<int> sut = It.SequenceEquals(1, 2, 3);
			IReadOnlyList<int>? captured = null;
			((IParameterWithCallback<IReadOnlyList<int>>)sut).Do(value => captured = value);

			List<int> source = [1, 2, 3,];
			((IParameterMatch<IReadOnlyList<int>>)sut).InvokeCallbacks(source);

			await That(captured).IsSameAs(source);
		}

		[Fact]
		public async Task Do_RegistersCallbackForList()
		{
			It.ISequenceEqualsParameter<int> sut = It.SequenceEquals(1, 2, 3);
			List<int>? captured = null;
			((IParameterWithCallback<List<int>>)sut).Do(value => captured = value);

			List<int> source = [1, 2, 3,];
			((IParameterMatch<List<int>>)sut).InvokeCallbacks(source);

			await That(captured).IsSameAs(source);
		}

		[Fact]
		public async Task Do_RegistersCallbackForQueue()
		{
			It.ISequenceEqualsParameter<int> sut = It.SequenceEquals(1, 2, 3);
			Queue<int>? captured = null;
			((IParameterWithCallback<Queue<int>>)sut).Do(value => captured = value);

			Queue<int> source = new();
			source.Enqueue(1);
			((IParameterMatch<Queue<int>>)sut).InvokeCallbacks(source);

			await That(captured).IsSameAs(source);
		}

		[Fact]
		public async Task Do_RegistersCallbackForStack()
		{
			It.ISequenceEqualsParameter<int> sut = It.SequenceEquals(1, 2, 3);
			Stack<int>? captured = null;
			((IParameterWithCallback<Stack<int>>)sut).Do(value => captured = value);

			Stack<int> source = new();
			source.Push(1);
			((IParameterMatch<Stack<int>>)sut).InvokeCallbacks(source);

			await That(captured).IsSameAs(source);
		}
	}
}
