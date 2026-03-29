using System.Reflection;
using Mockolate.Setup;

namespace Mockolate.Internal.Tests;

public partial class MockSetupsTests
{
	public sealed class EventSetupsTests
	{
		[Fact]
		public async Task AddAndRemove_ShouldUpdateCount()
		{
			MockSetups.EventSetups setups = new();
			object target = new();
			MethodInfo method = GetMethodInfo();
			string eventName = "evt";
			setups.Add(target, method, eventName);
			await That(setups.Count).IsEqualTo(1);
			setups.Remove(target, method, eventName);
			await That(setups.Count).IsEqualTo(0);
		}

		[Fact]
		public async Task Stress_ShouldMaintainCountAfterManyAddsAndRemoves()
		{
			MockSetups.EventSetups setups = new();
			object target = new();
			MethodInfo method = GetMethodInfo();
			string eventName = "evt";

			for (int r = 0; r < 10; r++)
			{
				Parallel.For(0, 100, i => setups.Add(target, method, eventName + i));
			}

			await That(setups.Count).IsEqualTo(100);

			for (int r = 0; r < 10; r++)
			{
				Parallel.For(0, 100, i => setups.Remove(target, method, eventName + i));
			}

			await That(setups.Count).IsEqualTo(0);
		}

		[Fact]
		public async Task ThreadSafety_DuplicateAdd_ShouldBeIdempotent()
		{
			MockSetups.EventSetups setups = new();
			object target = new();
			MethodInfo method = GetMethodInfo();
			string eventName = "evt";

			Parallel.For(0, 200, _ => setups.Add(target, method, eventName));

			await That(setups.Count).IsEqualTo(1);
		}

		[Fact]
		public async Task ThreadSafety_ShouldHandleParallelAddsAndRemoves()
		{
			MockSetups.EventSetups setups = new();
			object target = new();
			MethodInfo method = GetMethodInfo();
			string eventName = "evt";

			Parallel.For(0, 200, i => setups.Add(target, method, eventName + i));

			await That(setups.Count).IsEqualTo(200);

			Parallel.For(0, 200, i => setups.Remove(target, method, eventName + i));

			await That(setups.Count).IsEqualTo(0);
		}
	}
}
