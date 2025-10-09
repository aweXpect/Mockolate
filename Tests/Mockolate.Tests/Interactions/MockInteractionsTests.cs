﻿using System.Collections.Concurrent;
using System.Threading;
using Mockolate.Interactions;

namespace Mockolate.Tests.Interactions;

public sealed class MockInteractionsTests
{
	[Fact]
	public async Task GetNextIndex_ShouldBeThreadSafe()
	{
		MockInteractions sut = new();
		Task[] tasks = new Task[50];
		ConcurrentQueue<int> retrievedIds = [];
		for (int i = 0; i < 50; i++)
		{
			tasks[i] = Task.Run(async () =>
			{
				for (int i = 0; i < 20; i++)
				{
					retrievedIds.Enqueue(sut.GetNextIndex());
					await Task.Delay(1);
				}
			}, CancellationToken.None);
		}

		await Task.WhenAll(tasks);

		await That(retrievedIds.Count).IsEqualTo(1000);
		await That(retrievedIds).AreAllUnique();
	}

	[Fact]
	public async Task RegisterInteraction_ShouldBeThreadSafe()
	{
		MockInteractions sut = new();
		IMockInteractions interactions = sut;
		Task[] tasks = new Task[50];
		for (int i = 0; i < 50; i++)
		{
			tasks[i] = Task.Run(async () =>
			{
				for (int i = 0; i < 20; i++)
				{
					int index = sut.GetNextIndex();
					interactions.RegisterInteraction(new PropertySetterAccess(index, "MyTestProperty", index));
					await Task.Delay(1);
				}
			}, CancellationToken.None);
		}

		await Task.WhenAll(tasks);

		await That(sut.Count).IsEqualTo(1000);
		await That(sut.Interactions).IsInAscendingOrder(x => x.Index);
		await That(sut.Interactions).AreAllUnique();
	}
}
