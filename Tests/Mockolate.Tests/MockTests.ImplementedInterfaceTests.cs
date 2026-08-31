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
		public async Task VirtualIndexer_ShouldShareStoredValuesBetweenClassAndInterface()
		{
			Calculator mock = Calculator.CreateMock().Implementing<ICalculator>();

			mock[1] = 5;

			await That(((ICalculator)mock)[1]).IsEqualTo(5)
				.Because("both surfaces address one indexer, so they share its storage");

			((ICalculator)mock)[2] = 7;

			await That(mock[2]).IsEqualTo(7);
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

		[Fact]
		public async Task RenamedGenericTypeParameter_ShouldShareSetupBetweenClassAndInterface()
		{
			Calculator mock = Calculator.CreateMock().Implementing<ICalculator>();
			mock.Mock.Setup.Convert<int>(It.IsAny<int>()).Returns(13);

			await That(mock.Convert(1)).IsEqualTo(13);
			await That(((ICalculator)mock).Convert(1)).IsEqualTo(13)
				.Because("the name of a generic type parameter is not part of the member's identity");
		}

		[Fact]
		public async Task ObliviousNullabilityOnTheReturnType_ShouldShareSetupBetweenClassAndInterface()
		{
			LegacyCalculator mock = LegacyCalculator.CreateMock().Implementing<ILegacyCalculator>();
			mock.Mock.Setup.Describe().Returns("chocolate");

			await That(((ILegacyCalculator)mock).Describe()).IsEqualTo("chocolate")
				.Because("the nullability annotation on the return type is not part of the member's identity");
		}

		[Fact]
		public async Task InterfaceImplementedByABaseClass_ShouldShareSetupWithTheBaseClassMember()
		{
			DerivedCalculator mock = DerivedCalculator.CreateMock().Implementing<ISquarer>();
			mock.Mock.Setup.Square(It.IsAny<int>()).Returns(6);

			await That(mock.Square(2)).IsEqualTo(6);
			await That(((ISquarer)mock).Square(2)).IsEqualTo(6)
				.Because("the base class member implements the interface, and the mock overrides it");
		}

		[Fact]
		public async Task SealedOverrideImplementation_ShouldStayConfigurableThroughTheInterface()
		{
			SealingCalculator mock = SealingCalculator.CreateMock().Implementing<ISquarer>();
			mock.Mock.As<ISquarer>().Setup.Square(It.IsAny<int>()).Returns(9);

			await That(mock.Square(2)).IsEqualTo(4)
				.Because("a sealed override cannot be overridden, so the class call cannot be intercepted");
			await That(((ISquarer)mock).Square(2)).IsEqualTo(9)
				.Because("the interface slot is re-implemented, which is the only way to reach it");
		}

		public interface ISquarer
		{
			int Square(int value);
		}

		public abstract class SquarerBase : ISquarer
		{
			public abstract int Square(int value);
		}

		public abstract class DerivedCalculator : SquarerBase;

		public abstract class SealingCalculator : SquarerBase
		{
			public sealed override int Square(int value) => value * value;
		}

		public interface ICalculator
		{
			int Precision { get; set; }
			int this[int index] { get; set; }
			event EventHandler? Calculated;
			int Add(int a, int b);
			int Multiply(int a, int b);
			int Subtract(int a, int b);
			T Convert<T>(T value);
		}

		public abstract class Calculator : ICalculator
		{
			public abstract int Precision { get; set; }
			public abstract int this[int index] { get; set; }
			public abstract event EventHandler? Calculated;
			public int Add(int a, int b) => a + b;
			public abstract int Multiply(int a, int b);
			public virtual int Subtract(int a, int b) => a - b;
			public abstract TValue Convert<TValue>(TValue value);
		}

		public interface ILegacyCalculator
		{
			string? Describe();
		}

		// An oblivious (pre-nullable) class: `string` here is neither annotated nor not-annotated, so it
		// implements `string?` without a nullability warning while still differing from it as a type name.
#nullable disable
		public abstract class LegacyCalculator : ILegacyCalculator
		{
			public abstract string Describe();
		}
#nullable restore
	}
}
