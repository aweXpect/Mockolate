using System.Linq;
using System.Threading;

namespace Mockolate.Tests;

public sealed class MockRegistrationTests
{
	[Test]
	public async Task RegisterInteraction_ShouldBeThreadSafe()
	{
		MockRegistration sut = new(MockBehavior.Default, "");
		Task[] tasks = new Task[50];
		for (int i = 0; i < 50; i++)
		{
			tasks[i] = Task.Run(async () =>
			{
				for (int j = 0; j < 20; j++)
				{
					sut.GetProperty<string>("foo", () => "", null);
					await Task.Delay(1);
				}
			}, CancellationToken.None);
		}

		await Task.WhenAll(tasks);

		await That(sut.Interactions.Count).IsEqualTo(1000);
		await That(sut.Interactions.Interactions).IsInAscendingOrder(x => x.Index);
		await That(sut.Interactions.Interactions.Select(i => i.Index)).AreAllUnique();
	}
}
