using System.Collections.Concurrent;
using System.Threading;
using Mockolate.Interactions;

namespace Mockolate.Tests.Interactions;

public sealed class MockInteractionsTests
{
	[Fact]
	public async Task GetNextIndex_ShouldBeThreadSafe()
	{
		var sut = new MockInteractions();
		var tasks = new Task[50];
		ConcurrentQueue<int> retrievedIds = [];
		for (int i = 0; i < 50; i++)
		{
			tasks[i] = Task.Run(() =>
			{
				for (int i = 0; i < 50; i++)
				{
					retrievedIds.Enqueue(sut.GetNextIndex());
					Task.Yield();
				}
			}, cancellationToken: CancellationToken.None);
		}
		await Task.WhenAll(tasks);

		await That(retrievedIds.Count).IsEqualTo(2500);
		await That(retrievedIds).AreAllUnique();
	}

	[Fact]
	public async Task RegisterInteraction_ShouldBeThreadSafe()
	{
		var sut = new MockInteractions();
		IMockInteractions interactions = sut;
		var tasks = new Task[50];
		for (int i = 0; i < 50; i++)
		{
			tasks[i] = Task.Run(() =>
			{
				for (int i = 0; i < 50; i++)
				{
					var index = sut.GetNextIndex();
					interactions.RegisterInteraction(new PropertySetterAccess(index, "MyTestProperty", index));
					Task.Yield();
				}
			}, cancellationToken: CancellationToken.None);
		}
		await Task.WhenAll(tasks);

		await That(sut.Count).IsEqualTo(2500);
		await That(sut.Interactions).IsInAscendingOrder(x => x.Index);
		await That(sut.Interactions).AreAllUnique();
	}
}
