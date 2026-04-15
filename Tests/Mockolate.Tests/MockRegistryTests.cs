using System.Collections.Generic;
using System.Threading;
using Mockolate.Interactions;
using Mockolate.Setup;
using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests;

public sealed class MockRegistryTests
{
	[Fact]
	public async Task GetUnusedSetups_IndexerSetup_ShouldHaveCorrectString()
	{
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();
		sut.Mock.Setup["Dark"].Returns(5);
		MockRegistry registry = ((IMock)sut).MockRegistry;

		IReadOnlyCollection<ISetup> result = registry.GetUnusedSetups(new MockInteractions());

		ISetup setup = await That(result).HasSingle();
		await That(setup.ToString()).IsEqualTo("int this[\"Dark\"]");
	}

	[Fact]
	public async Task GetUnusedSetups_MethodSetup_ShouldHaveCorrectString()
	{
		IMyService sut = IMyService.CreateMock();
		sut.Mock.Setup.DoSomething(null, 3.5, It.IsAny<string>()).DoesNotThrow();
		MockRegistry registry = ((IMock)sut).MockRegistry;

		IReadOnlyCollection<ISetup> result = registry.GetUnusedSetups(new MockInteractions());

		ISetup setup = await That(result).HasSingle();
		await That(setup.ToString()).IsEqualTo("void DoSomething(null, 3.5, It.IsAny<string>())");
	}

	[Fact]
	public async Task GetUnusedSetups_PropertySetup_ShouldHaveCorrectString()
	{
		IChocolateDispenser sut = IChocolateDispenser.CreateMock();
		sut.Mock.Setup.TotalDispensed.InitializeWith(4);
		MockRegistry registry = ((IMock)sut).MockRegistry;

		IReadOnlyCollection<ISetup> result = registry.GetUnusedSetups(new MockInteractions());

		ISetup setup = await That(result).HasSingle();
		await That(setup.ToString()).IsEqualTo("int TotalDispensed");
	}

	[Fact]
	public async Task GetUnusedSetups_WithNullableDouble_ShouldHaveCorrectString()
	{
		IMyService sut = IMyService.CreateMock();
		sut.Mock.Setup.DoSomething(new DateTime(2026, 4, 1, 12, 0, 0), 3.5).DoesNotThrow();
		MockRegistry registry = ((IMock)sut).MockRegistry;

		IReadOnlyCollection<ISetup> result = registry.GetUnusedSetups(new MockInteractions());

		ISetup setup = await That(result).HasSingle();
		await That(setup.ToString()).IsEqualTo("void DoSomething(04/01/2026 12:00:00, 3.5)");
	}

	[Fact]
	public async Task ImplicitConversionFromMockBehavior()
	{
		MockBehavior behavior = MockBehavior.Default.ThrowingWhenNotSetup();

		MockRegistry result = behavior;

		await That(result.Behavior).IsSameAs(behavior);
		await That(result.Interactions).IsEmpty();
	}

	[Fact]
	public async Task RegisterInteraction_ShouldBeThreadSafe()
	{
		MockRegistry sut = new(MockBehavior.Default);
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
	}
}
