using Mockolate.Internal.Tests.TestHelpers;
using Mockolate.Setup;

namespace Mockolate.Internal.Tests;

public partial class MockSetupsTests
{
	public class PropertySetupsTests
	{
		[Fact]
		public async Task AddDuplicate_ShouldReplaceAndAdjustCount()
		{
			MockSetups.PropertySetups setups = new();
			FakePropertySetup setup1 = new("foo");
			FakePropertySetup setup2 = new("foo");

			setups.Add(setup1);
			setups.Add(setup2);

			await That(setups.Count).IsEqualTo(1);

			setups.TryGetValue("foo", out PropertySetup? found);

			await That(found).IsEqualTo(setup2);
		}

		[Fact]
		public async Task Stress_ShouldMaintainCountAfterManyAddsAndReplacements()
		{
			MockSetups.PropertySetups setups = new();

			for (int r = 0; r < 10; r++)
			{
				Parallel.For(0, 100, i => setups.Add(new FakePropertySetup($"p{i}")));
			}

			await That(setups.Count).IsEqualTo(100);
		}

		[Fact]
		public async Task ThreadSafety_DuplicateKeyReplacements_ShouldPreserveSingleEntry()
		{
			MockSetups.PropertySetups setups = new();
			Parallel.For(0, 200, _ => setups.Add(new FakePropertySetup("same")));

			await That(setups.Count).IsEqualTo(1);

			bool found = setups.TryGetValue("same", out PropertySetup? setup);

			await That(found).IsTrue();
			await That(setup).IsNotNull();
		}

		[Fact]
		public async Task ThreadSafety_ShouldHandleParallelAdds()
		{
			MockSetups.PropertySetups setups = new();

			Parallel.For(0, 200, i => setups.Add(new FakePropertySetup($"p{i}")));

			await That(setups.Count).IsEqualTo(200);
		}

		[Fact]
		public async Task TryGetValue_Nonexistent_ShouldReturnFalse()
		{
			MockSetups.PropertySetups setups = new();

			bool result = setups.TryGetValue("bar", out PropertySetup? found);

			await That(result).IsFalse();
			await That(found).IsNull();
		}
	}
}
