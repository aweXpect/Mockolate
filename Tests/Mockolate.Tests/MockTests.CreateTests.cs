using Mockolate.Exceptions;
using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests;

public sealed partial class MockTests
{
	public sealed class CreateTests
	{
		[Test]
		public async Task TypedOverload_ConstructorWithDecimalDefault_ShouldForwardExplicitValue()
		{
			// This test exercises the generator's default-value emission end-to-end: if the
			// generator dropped the 'm' suffix ("decimal price = 19.95"), the generated source
			// would fail to compile, taking the whole test project down with it.
			MyBaseClassWithDecimalDefault sut = MyBaseClassWithDecimalDefault.CreateMock(42.50m);

			await That(sut.Price).IsEqualTo(42.50m);
		}

		[Test]
		public async Task TypedOverload_ConstructorWithDefaultValue_ShouldApplyDefault()
		{
			MyBaseClassWithMultipleConstructors sut = MyBaseClassWithMultipleConstructors.CreateMock(42);

			await That(sut.Text).IsEqualTo("default");
			await That(sut.Number).IsEqualTo(42);
		}

		[Test]
		public async Task TypedOverload_MultipleConstructors_ShouldDispatchToMatchingConstructor()
		{
			MyBaseClassWithMultipleConstructors sutFromString =
				MyBaseClassWithMultipleConstructors.CreateMock("foo");
			MyBaseClassWithMultipleConstructors sutFromIntAndString =
				MyBaseClassWithMultipleConstructors.CreateMock(42, "bar");

			await That(sutFromString.Text).IsEqualTo("foo");
			await That(sutFromString.Number).IsEqualTo(0);
			await That(sutFromIntAndString.Text).IsEqualTo("bar");
			await That(sutFromIntAndString.Number).IsEqualTo(42);
		}

		[Test]
		public async Task TypedOverload_SingleConstructor_ShouldForwardToBaseClass()
		{
			MyBaseClassWithConstructor sut = MyBaseClassWithConstructor.CreateMock("foo");

			await That(sut.Text).IsEqualTo("foo");
		}

		[Test]
		public async Task TypedOverload_WithMockBehavior_ShouldUseBehavior()
		{
			MockBehavior behavior = MockBehavior.Default.ThrowingWhenNotSetup();

			MyBaseClassWithConstructor sut = MyBaseClassWithConstructor.CreateMock(behavior, "foo");

			await That(((IMock)sut).MockRegistry.Behavior).IsSameAs(behavior);
			await That(sut.Text).IsEqualTo("foo");
		}

		[Test]
		public async Task TypedOverload_WithMockBehaviorAndSetup_ShouldApplyBoth()
		{
			MockBehavior behavior = MockBehavior.Default;

			MyBaseClassWithConstructor sut = MyBaseClassWithConstructor.CreateMock(
				behavior,
				setup => setup.VirtualMethod().Returns("bar"),
				"foo");

			await That(((IMock)sut).MockRegistry.Behavior).IsSameAs(behavior);
			await That(sut.VirtualMethod()).IsEqualTo("bar");
		}

		[Test]
		public async Task TypedOverload_WithSetup_ShouldApplySetup()
		{
			MyBaseClassWithConstructor sut = MyBaseClassWithConstructor.CreateMock(
				setup => setup.VirtualMethod().Returns("bar"),
				"foo");

			await That(sut.VirtualMethod()).IsEqualTo("bar");
		}

		[Test]
		public async Task With2Arguments_OnlyFirstArgumentIsClass_ShouldForwardBehaviorToBaseClass()
		{
			MockBehavior behavior = MockBehavior.Default.ThrowingWhenNotSetup();

			MyServiceBase sut = MyServiceBase.CreateMock(behavior).Implementing<IMyService>();

			await That(((IMock)sut).MockRegistry.Behavior).IsSameAs(behavior);
		}

		[Test]
		public async Task With2Arguments_WithConstructorParametersAndSetups_ShouldApplySetups()
		{
			MyBaseClassWithConstructor mock = MyBaseClassWithConstructor.CreateMock(
				setup => setup.VirtualMethod().Returns("bar"),
				["foo",]).Implementing<IMyService>();

			string result = mock.VirtualMethod();

			await That(result).IsEqualTo("bar");
		}

		[Test]
		public async Task With2Arguments_WithConstructorParametersMockBehaviorAndSetups_ShouldApplySetups()
		{
			MyBaseClassWithConstructor sut = MyBaseClassWithConstructor.CreateMock(
					MockBehavior.Default,
					setup => setup.VirtualMethod().Returns("bar"),
					["foo",])
				.Implementing<IMyService>();

			string result = sut.VirtualMethod();

			await That(result).IsEqualTo("bar");
		}

		[Test]
		public async Task With2Arguments_WithSetups_ShouldApplySetups()
		{
			IMyService sut = IMyService.CreateMock(setup =>
			{
				setup.Multiply(It.Is(1), It.IsAny<int?>()).Returns(2);
				setup.Multiply(It.Is(2), It.IsAny<int?>()).Returns(4);
				setup.Multiply(It.Is(3), It.IsAny<int?>()).Returns(8);
			});

			int result1 = sut.Multiply(1, null);
			int result2 = sut.Multiply(2, null);
			int result3 = sut.Multiply(3, null);

			await That(result1).IsEqualTo(2);
			await That(result2).IsEqualTo(4);
			await That(result3).IsEqualTo(8);
		}

		[Test]
		public async Task WithAdditionalInterfacesFromDifferentNamespaces_ShouldHaveUniqueName()
		{
			int invocationCount1 = 0;
			int invocationCount2 = 0;
			IChocolateDispenser sut1 = IChocolateDispenser.CreateMock().Implementing<TestHelpers.IMyService>();
			IChocolateDispenser sut2 = IChocolateDispenser.CreateMock().Implementing<TestHelpers.Other.IMyService>();

			sut1.Mock.As<TestHelpers.IMyService>().Setup
				.DoSomething(It.IsAny<int>())
				.Do(() => invocationCount1++);
			sut2.Mock.As<TestHelpers.Other.IMyService>().Setup
				.DoSomething(It.IsAny<int>())
				.Do(() => invocationCount2++);

			((TestHelpers.IMyService)sut1).DoSomething(1);
			((TestHelpers.IMyService)sut1).DoSomething(2);
			((TestHelpers.Other.IMyService)sut2).DoSomething(1);
			((TestHelpers.Other.IMyService)sut2).DoSomething(2);
			((TestHelpers.Other.IMyService)sut2).DoSomething(3);

			await That(invocationCount1).IsEqualTo(2);
			await That(invocationCount2).IsEqualTo(3);
			await That(sut1.Mock.As<TestHelpers.IMyService>().Verify
				.DoSomething(It.IsAny<int>())).Exactly(2);
			await That(() => sut1.Mock.As<TestHelpers.Other.IMyService>())
				.Throws<MockException>().WithMessage("The subject does not support type Mockolate.Tests.TestHelpers.Other.IMyService.");
			await That(() => sut2.Mock.As<TestHelpers.IMyService>())
				.Throws<MockException>().WithMessage("The subject does not support type Mockolate.Tests.TestHelpers.IMyService.");
			await That(sut2.Mock.As<TestHelpers.Other.IMyService>().Verify
				.DoSomething(It.IsAny<int>())).Exactly(3);
		}
	}
}
