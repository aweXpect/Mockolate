namespace Mockolate.Tests;

public sealed partial class MockTests
{
	public sealed class ImplementedInterfaceTests
	{
		[Fact]
		public async Task NonVirtualMethod_ShouldBeConfigurableThroughTheInterface()
		{
			Calculator mock = Calculator.CreateMock().Implementing<ICalculator>();
			mock.Mock.As<ICalculator>().Setup.Add(It.IsAny<int>(), It.IsAny<int>()).Returns(99);

			await That(mock.Add(1, 2)).IsEqualTo(3)
				.Because("the class implementation is not virtual and cannot be intercepted");
			await That(((ICalculator)mock).Add(1, 2)).IsEqualTo(99);
			await That(mock.Mock.As<ICalculator>().Verify.Add(1, 2)).Once();
		}

		[Fact]
		public async Task VirtualMethod_ShouldShareSetupBetweenClassAndInterface()
		{
			Calculator mock = Calculator.CreateMock().Implementing<ICalculator>();
			mock.Mock.Setup.Multiply(It.IsAny<int>(), It.IsAny<int>()).Returns(7);

			await That(mock.Multiply(3, 4)).IsEqualTo(7);
			await That(((ICalculator)mock).Multiply(3, 4)).IsEqualTo(7);
			await That(mock.Mock.Verify.Multiply(3, 4)).Twice();
			await That(mock.Mock.As<ICalculator>().Verify.Multiply(3, 4)).Twice();
		}

		[Fact]
		public async Task VirtualMethod_SetupThroughInterface_ShouldApplyToClassCall()
		{
			Calculator mock = Calculator.CreateMock().Implementing<ICalculator>();
			mock.Mock.As<ICalculator>().Setup.Multiply(It.IsAny<int>(), It.IsAny<int>()).Returns(7);

			await That(mock.Multiply(3, 4)).IsEqualTo(7);
		}

		[Fact]
		public async Task VirtualProperty_ShouldShareSetupBetweenClassAndInterface()
		{
			Calculator mock = Calculator.CreateMock().Implementing<ICalculator>();
			mock.Mock.Setup.Precision.InitializeWith(5);

			await That(mock.Precision).IsEqualTo(5);
			await That(((ICalculator)mock).Precision).IsEqualTo(5);
		}

		[Fact]
		public async Task VirtualIndexer_ShouldShareSetupBetweenClassAndInterface()
		{
			Calculator mock = Calculator.CreateMock().Implementing<ICalculator>();
			mock.Mock.Setup[It.IsAny<int>()].InitializeWith(11);

			await That(mock[1]).IsEqualTo(11);
			await That(((ICalculator)mock)[1]).IsEqualTo(11);
		}

		[Fact]
		public async Task VirtualEvent_ShouldShareSubscriptionsBetweenClassAndInterface()
		{
			Calculator mock = Calculator.CreateMock().Implementing<ICalculator>();
			EventHandler handler = (_, _) => { };

			((ICalculator)mock).Calculated += handler;

			await That(mock.Mock.Verify.Calculated.Subscribed()).Once();
			await That(mock.Mock.As<ICalculator>().Verify.Calculated.Subscribed()).Once();
		}

		[Fact]
		public async Task VirtualMethodWithBody_ShouldFallBackToTheBaseImplementationThroughTheInterface()
		{
			Calculator mock = Calculator.CreateMock().Implementing<ICalculator>();

			await That(((ICalculator)mock).Subtract(5, 3)).IsEqualTo(2)
				.Because("no setup matches, so the mock falls back to the base implementation");

			mock.Mock.As<ICalculator>().Setup.Subtract(It.IsAny<int>(), It.IsAny<int>()).Returns(42);

			await That(mock.Subtract(5, 3)).IsEqualTo(42);
			await That(((ICalculator)mock).Subtract(5, 3)).IsEqualTo(42);
		}

		public interface ICalculator
		{
			int Precision { get; set; }
			int this[int index] { get; set; }
			event EventHandler? Calculated;
			int Add(int a, int b);
			int Multiply(int a, int b);
			int Subtract(int a, int b);
		}

		public abstract class Calculator : ICalculator
		{
			public abstract int Precision { get; set; }
			public abstract int this[int index] { get; set; }
			public abstract event EventHandler? Calculated;
			public int Add(int a, int b) => a + b;
			public abstract int Multiply(int a, int b);
			public virtual int Subtract(int a, int b) => a - b;
		}
	}
}
