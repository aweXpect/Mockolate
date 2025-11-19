using System.Linq;
using System.Threading;

namespace Mockolate.Tests;

public sealed class MockInteractionsTests
{
	[Fact]
	public async Task RegisterInteraction_ShouldBeThreadSafe()
	{
		MockRegistration registration = new(MockBehavior.Default, "");
		Task[] tasks = new Task[50];
		for (int i = 0; i < 50; i++)
		{
			tasks[i] = Task.Run(async () =>
			{
				for (int j = 0; j < 20; j++)
				{
					registration.GetProperty<string>("foo");
					await Task.Delay(1);
				}
			}, CancellationToken.None);
		}

		await Task.WhenAll(tasks);

		await That(registration.Interactions.Count).IsEqualTo(1000);
		await That(registration.Interactions.Interactions).IsInAscendingOrder(x => x.Index);
		await That(registration.Interactions.Interactions.Select(i => i.Index)).AreAllUnique();
	}
}
