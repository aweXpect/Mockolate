using System.Collections.Generic;
using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests;

public sealed partial class MockTests
{
	public sealed class WrapTests
	{
		[Fact]
		public async Task Wrap_Method_ShouldDelegateToWrappedInstance()
		{
			MyChocolateDispenser myDispenser = new();
			IChocolateDispenser wrappedDispenser = Mock.Wrap<IChocolateDispenser>(myDispenser);

			bool result = wrappedDispenser.Dispense("Dark", 4);

			await That(result).IsTrue();
			await That(wrappedDispenser["Dark"]).IsEqualTo(1);
			await That(myDispenser["Dark"]).IsEqualTo(1);
		}

		[Fact]
		public async Task Wrap_Property_ShouldDelegateToWrappedInstance()
		{
			MyChocolateDispenser myDispenser = new();
			IChocolateDispenser wrappedDispenser = Mock.Wrap<IChocolateDispenser>(myDispenser);

			wrappedDispenser.TotalDispensed = 12;

			await That(wrappedDispenser.TotalDispensed).IsEqualTo(12);
			await That(myDispenser.TotalDispensed).IsEqualTo(12);
		}

		[Fact]
		public async Task Wrap_Indexer_ShouldDelegateToWrappedInstance()
		{
			MyChocolateDispenser myDispenser = new();
			IChocolateDispenser wrappedDispenser = Mock.Wrap<IChocolateDispenser>(myDispenser);

			wrappedDispenser["Dark"] = 12;

			await That(wrappedDispenser["Dark"]).IsEqualTo(12);
			await That(myDispenser["Dark"]).IsEqualTo(12);
			await That(wrappedDispenser["White"]).IsEqualTo(8);
			await That(myDispenser["White"]).IsEqualTo(8);
		}

		[Fact]
		public async Task Wrap_WithSetup_ShouldOverrideMethod()
		{
			MyChocolateDispenser myDispenser = new();
			IChocolateDispenser wrappedDispenser = Mock.Wrap<IChocolateDispenser>(myDispenser);
			wrappedDispenser.SetupMock.Method.Dispense(It.IsAny<string>(), It.IsAny<int>()).Returns(false);

			bool result = wrappedDispenser.Dispense("Dark", 4);

			await That(result).IsFalse();
			await That(wrappedDispenser["Dark"]).IsEqualTo(1);
			await That(myDispenser.TotalDispensed).IsEqualTo(4);
		}

		private class MyChocolateDispenser : IChocolateDispenser
		{
			private readonly Dictionary<string, int> _inventory = new()
			{
				{
					"Milk", 10
				},
				{
					"Dark", 5
				},
				{
					"White", 8
				},
			};

			public int this[string type]
			{
				get => _inventory[type];
				set => _inventory[type] = value;
			}

			public int TotalDispensed { get; set; }

			public bool Dispense(string type, int amount)
			{
				if (_inventory[type] >= amount)
				{
					TotalDispensed += amount;
					_inventory[type] -= amount;
					ChocolateDispensed?.Invoke(type, amount);
					return true;
				}

				return false;
			}

			public event ChocolateDispensedDelegate? ChocolateDispensed;
		}
	}
}
