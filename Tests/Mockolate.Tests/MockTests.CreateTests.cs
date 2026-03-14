using Mockolate.Exceptions;
using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests;

public sealed partial class MockTests
{
	public sealed class CreateTests
	{
		[Fact]
		public async Task With2Arguments_OnlyFirstArgumentIsClass_ShouldForwardBehaviorToBaseClass()
		{
			MockBehavior behavior = MockBehavior.Default.ThrowingWhenNotSetup();

			MyServiceBase sut = MyServiceBase.CreateMock(behavior).Implementing<IMyService>();

			await That(((IMock)sut).Registrations.Behavior).IsSameAs(behavior);
		}

		[Fact]
		public async Task With2Arguments_WithConstructorParametersAndSetups_ShouldApplySetups()
		{
			MyBaseClassWithConstructor mock = MyBaseClassWithConstructor.CreateMock(
				["foo",],
				setups: setup => setup.VirtualMethod().Returns("bar")).Implementing<IMyService>();

			string result = mock.VirtualMethod();

			await That(result).IsEqualTo("bar");
		}

		[Fact]
		public async Task With2Arguments_WithConstructorParametersMockBehaviorAndSetups_ShouldApplySetups()
		{
			MyBaseClassWithConstructor mock = MyBaseClassWithConstructor.CreateMock(
					["foo",],
					MockBehavior.Default,
					setup => setup.VirtualMethod().Returns("bar"))
				.Implementing<IMyService>();

			string result = mock.VirtualMethod();

			await That(result).IsEqualTo("bar");
		}

		[Fact]
		public async Task With2Arguments_WithSetups_ShouldApplySetups()
		{
			IMyService mock = IMyService.CreateMock(
				setup => setup.Multiply(It.Is(1), It.IsAny<int?>()).Returns(2),
				setup => setup.Multiply(It.Is(2), It.IsAny<int?>()).Returns(4),
				setup => setup.Multiply(It.Is(3), It.IsAny<int?>()).Returns(8));

			int result1 = mock.Multiply(1, null);
			int result2 = mock.Multiply(2, null);
			int result3 = mock.Multiply(3, null);

			await That(result1).IsEqualTo(2);
			await That(result2).IsEqualTo(4);
			await That(result3).IsEqualTo(8);
		}

		[Fact]
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
