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

			MyServiceBase sut = Mock.Create<MyServiceBase, IMyService>(behavior);

			await That(((IHasMockRegistration)sut).Registrations.Behavior).IsSameAs(behavior);
		}

		[Fact]
		public async Task With2Arguments_SecondIsClass_ShouldThrow()
		{
			void Act()
			{
				_ = Mock.Create<IMyService, MyServiceBase>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The mock declaration has 1 additional implementation that is not an interface: Mockolate.Tests.TestHelpers.MyServiceBase");
		}

		[Fact]
		public async Task With2Arguments_WithConstructorParametersAndSetups_ShouldApplySetups()
		{
			MyBaseClassWithConstructor mock = Mock.Create<MyBaseClassWithConstructor, IMyService>(
				BaseClass.WithConstructorParameters("foo"),
				setup => setup.Method.VirtualMethod().Returns("bar"));

			string result = mock.VirtualMethod();

			await That(result).IsEqualTo("bar");
		}

		[Fact]
		public async Task With2Arguments_WithConstructorParametersMockBehaviorAndSetups_ShouldApplySetups()
		{
			MyBaseClassWithConstructor mock = Mock.Create<MyBaseClassWithConstructor, IMyService>(
				BaseClass.WithConstructorParameters("foo"), MockBehavior.Default,
				setup => setup.Method.VirtualMethod().Returns("bar"));

			string result = mock.VirtualMethod();

			await That(result).IsEqualTo("bar");
		}

		[Fact]
		public async Task With2Arguments_WithSetups_ShouldApplySetups()
		{
			IMyService mock = Mock.Create<IMyService, IMyService>(
				setup => setup.Method.Multiply(It.Is(1), It.IsAny<int?>()).Returns(2),
				setup => setup.Method.Multiply(It.Is(2), It.IsAny<int?>()).Returns(4),
				setup => setup.Method.Multiply(It.Is(3), It.IsAny<int?>()).Returns(8));

			int result1 = mock.Multiply(1, null);
			int result2 = mock.Multiply(2, null);
			int result3 = mock.Multiply(3, null);

			await That(result1).IsEqualTo(2);
			await That(result2).IsEqualTo(4);
			await That(result3).IsEqualTo(8);
		}

		[Fact]
		public async Task With3Arguments_OnlyFirstArgumentIsClass_ShouldForwardBehaviorToBaseClass()
		{
			MockBehavior behavior = MockBehavior.Default.ThrowingWhenNotSetup();

			MyServiceBase sut = Mock.Create<MyServiceBase, IMyService, IMyService>(behavior);

			await That(((IHasMockRegistration)sut).Registrations.Behavior).IsSameAs(behavior);
		}

		[Fact]
		public async Task With3Arguments_SecondAndThirdAreClasses_ShouldThrow()
		{
			void Act()
			{
				_ = Mock.Create<IMyService, MyServiceBase, MyServiceBase>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The mock declaration has 2 additional implementations that are not interfaces: Mockolate.Tests.TestHelpers.MyServiceBase, Mockolate.Tests.TestHelpers.MyServiceBase");
		}

		[Fact]
		public async Task With3Arguments_SecondIsClass_ShouldThrow()
		{
			void Act()
			{
				_ = Mock.Create<IMyService, MyServiceBase, IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The mock declaration has 1 additional implementation that is not an interface: Mockolate.Tests.TestHelpers.MyServiceBase");
		}

		[Fact]
		public async Task With3Arguments_ThirdIsClass_ShouldThrow()
		{
			void Act()
			{
				_ = Mock.Create<IMyService, IMyService, MyServiceBase>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The mock declaration has 1 additional implementation that is not an interface: Mockolate.Tests.TestHelpers.MyServiceBase");
		}

		[Fact]
		public async Task With4Arguments_FourthIsClass_ShouldThrow()
		{
			void Act()
			{
				_ = Mock.Create<IMyService, IMyService, IMyService, MyServiceBase>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The mock declaration has 1 additional implementation that is not an interface: Mockolate.Tests.TestHelpers.MyServiceBase");
		}

		[Fact]
		public async Task With4Arguments_OnlyFirstArgumentIsClass_ShouldForwardBehaviorToBaseClass()
		{
			MockBehavior behavior = MockBehavior.Default.ThrowingWhenNotSetup();

			MyServiceBase sut = Mock.Create<MyServiceBase, IMyService, IMyService, IMyService>(behavior);

			await That(((IHasMockRegistration)sut).Registrations.Behavior).IsSameAs(behavior);
		}

		[Fact]
		public async Task With4Arguments_SecondIsClass_ShouldThrow()
		{
			void Act()
			{
				_ = Mock.Create<IMyService, MyServiceBase, IMyService, IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The mock declaration has 1 additional implementation that is not an interface: Mockolate.Tests.TestHelpers.MyServiceBase");
		}

		[Fact]
		public async Task With4Arguments_ThirdIsClass_ShouldThrow()
		{
			void Act()
			{
				_ = Mock.Create<IMyService, IMyService, MyServiceBase, IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The mock declaration has 1 additional implementation that is not an interface: Mockolate.Tests.TestHelpers.MyServiceBase");
		}

		[Fact]
		public async Task With5Arguments_FifthIsClass_ShouldThrow()
		{
			void Act()
			{
				_ = Mock.Create<IMyService, IMyService, IMyService, IMyService, MyServiceBase>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The mock declaration has 1 additional implementation that is not an interface: Mockolate.Tests.TestHelpers.MyServiceBase");
		}

		[Fact]
		public async Task With5Arguments_FourthIsClass_ShouldThrow()
		{
			void Act()
			{
				_ = Mock.Create<IMyService, IMyService, IMyService, MyServiceBase, IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The mock declaration has 1 additional implementation that is not an interface: Mockolate.Tests.TestHelpers.MyServiceBase");
		}

		[Fact]
		public async Task With5Arguments_OnlyFirstArgumentIsClass_ShouldForwardBehaviorToBaseClass()
		{
			MockBehavior behavior = MockBehavior.Default.ThrowingWhenNotSetup();

			MyServiceBase sut = Mock.Create<MyServiceBase, IMyService, IMyService, IMyService, IMyService>(behavior);

			await That(((IHasMockRegistration)sut).Registrations.Behavior).IsSameAs(behavior);
		}

		[Fact]
		public async Task With5Arguments_SecondIsClass_ShouldThrow()
		{
			void Act()
			{
				_ = Mock.Create<IMyService, MyServiceBase, IMyService, IMyService, IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The mock declaration has 1 additional implementation that is not an interface: Mockolate.Tests.TestHelpers.MyServiceBase");
		}

		[Fact]
		public async Task With5Arguments_ThirdIsClass_ShouldThrow()
		{
			void Act()
			{
				_ = Mock.Create<IMyService, IMyService, MyServiceBase, IMyService, IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The mock declaration has 1 additional implementation that is not an interface: Mockolate.Tests.TestHelpers.MyServiceBase");
		}

		[Fact]
		public async Task With6Arguments_FifthIsClass_ShouldThrow()
		{
			void Act()
			{
				_ = Mock.Create<IMyService, IMyService, IMyService, IMyService, MyServiceBase, IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The mock declaration has 1 additional implementation that is not an interface: Mockolate.Tests.TestHelpers.MyServiceBase");
		}

		[Fact]
		public async Task With6Arguments_FourthIsClass_ShouldThrow()
		{
			void Act()
			{
				_ = Mock.Create<IMyService, IMyService, IMyService, MyServiceBase, IMyService, IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The mock declaration has 1 additional implementation that is not an interface: Mockolate.Tests.TestHelpers.MyServiceBase");
		}

		[Fact]
		public async Task With6Arguments_OnlyFirstArgumentIsClass_ShouldForwardBehaviorToBaseClass()
		{
			MockBehavior behavior = MockBehavior.Default.ThrowingWhenNotSetup();

			MyServiceBase sut =
				Mock.Create<MyServiceBase, IMyService, IMyService, IMyService, IMyService, IMyService>(behavior);

			await That(((IHasMockRegistration)sut).Registrations.Behavior).IsSameAs(behavior);
		}

		[Fact]
		public async Task With6Arguments_SecondIsClass_ShouldThrow()
		{
			void Act()
			{
				_ = Mock.Create<IMyService, MyServiceBase, IMyService, IMyService, IMyService, IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The mock declaration has 1 additional implementation that is not an interface: Mockolate.Tests.TestHelpers.MyServiceBase");
		}

		[Fact]
		public async Task With6Arguments_SixthIsClass_ShouldThrow()
		{
			void Act()
			{
				_ = Mock.Create<IMyService, IMyService, IMyService, IMyService, IMyService, MyServiceBase>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The mock declaration has 1 additional implementation that is not an interface: Mockolate.Tests.TestHelpers.MyServiceBase");
		}

		[Fact]
		public async Task With6Arguments_ThirdIsClass_ShouldThrow()
		{
			void Act()
			{
				_ = Mock.Create<IMyService, IMyService, MyServiceBase, IMyService, IMyService, IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The mock declaration has 1 additional implementation that is not an interface: Mockolate.Tests.TestHelpers.MyServiceBase");
		}

		[Fact]
		public async Task With7Arguments_FifthIsClass_ShouldThrow()
		{
			void Act()
			{
				_ =
					Mock.Create<IMyService, IMyService, IMyService, IMyService, MyServiceBase, IMyService,
						IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The mock declaration has 1 additional implementation that is not an interface: Mockolate.Tests.TestHelpers.MyServiceBase");
		}

		[Fact]
		public async Task With7Arguments_FourthIsClass_ShouldThrow()
		{
			void Act()
			{
				_ =
					Mock.Create<IMyService, IMyService, IMyService, MyServiceBase, IMyService, IMyService,
						IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The mock declaration has 1 additional implementation that is not an interface: Mockolate.Tests.TestHelpers.MyServiceBase");
		}

		[Fact]
		public async Task With7Arguments_OnlyFirstArgumentIsClass_ShouldForwardBehaviorToBaseClass()
		{
			MockBehavior behavior = MockBehavior.Default.ThrowingWhenNotSetup();

			MyServiceBase sut =
				Mock.Create<MyServiceBase, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService>(
					behavior);

			await That(((IHasMockRegistration)sut).Registrations.Behavior).IsSameAs(behavior);
		}

		[Fact]
		public async Task With7Arguments_SecondIsClass_ShouldThrow()
		{
			void Act()
			{
				_ =
					Mock.Create<IMyService, MyServiceBase, IMyService, IMyService, IMyService, IMyService,
						IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The mock declaration has 1 additional implementation that is not an interface: Mockolate.Tests.TestHelpers.MyServiceBase");
		}

		[Fact]
		public async Task With7Arguments_SeventhIsClass_ShouldThrow()
		{
			void Act()
			{
				_ =
					Mock.Create<IMyService, IMyService, IMyService, IMyService, IMyService, IMyService,
						MyServiceBase>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The mock declaration has 1 additional implementation that is not an interface: Mockolate.Tests.TestHelpers.MyServiceBase");
		}

		[Fact]
		public async Task With7Arguments_SixthIsClass_ShouldThrow()
		{
			void Act()
			{
				_ =
					Mock.Create<IMyService, IMyService, IMyService, IMyService, IMyService, MyServiceBase,
						IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The mock declaration has 1 additional implementation that is not an interface: Mockolate.Tests.TestHelpers.MyServiceBase");
		}

		[Fact]
		public async Task With7Arguments_ThirdIsClass_ShouldThrow()
		{
			void Act()
			{
				_ =
					Mock.Create<IMyService, IMyService, MyServiceBase, IMyService, IMyService, IMyService,
						IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The mock declaration has 1 additional implementation that is not an interface: Mockolate.Tests.TestHelpers.MyServiceBase");
		}

		[Fact]
		public async Task With8Arguments_EighthIsClass_ShouldThrow()
		{
			void Act()
			{
				_ =
					Mock.Create<IMyService, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService,
						MyServiceBase>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The mock declaration has 1 additional implementation that is not an interface: Mockolate.Tests.TestHelpers.MyServiceBase");
		}

		[Fact]
		public async Task With8Arguments_FifthIsClass_ShouldThrow()
		{
			void Act()
			{
				_ =
					Mock.Create<IMyService, IMyService, IMyService, IMyService, MyServiceBase, IMyService, IMyService,
						IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The mock declaration has 1 additional implementation that is not an interface: Mockolate.Tests.TestHelpers.MyServiceBase");
		}

		[Fact]
		public async Task With8Arguments_FourthIsClass_ShouldThrow()
		{
			void Act()
			{
				_ =
					Mock.Create<IMyService, IMyService, IMyService, MyServiceBase, IMyService, IMyService, IMyService,
						IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The mock declaration has 1 additional implementation that is not an interface: Mockolate.Tests.TestHelpers.MyServiceBase");
		}

		[Fact]
		public async Task With8Arguments_OnlyFirstArgumentIsClass_ShouldForwardBehaviorToBaseClass()
		{
			MockBehavior behavior = MockBehavior.Default.ThrowingWhenNotSetup();

			MyServiceBase sut =
				Mock.Create<MyServiceBase, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService,
					IMyService>(behavior);

			await That(((IHasMockRegistration)sut).Registrations.Behavior).IsSameAs(behavior);
		}

		[Fact]
		public async Task With8Arguments_SecondIsClass_ShouldThrow()
		{
			void Act()
			{
				_ =
					Mock.Create<IMyService, MyServiceBase, IMyService, IMyService, IMyService, IMyService, IMyService,
						IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The mock declaration has 1 additional implementation that is not an interface: Mockolate.Tests.TestHelpers.MyServiceBase");
		}

		[Fact]
		public async Task With8Arguments_SeventhIsClass_ShouldThrow()
		{
			void Act()
			{
				_ =
					Mock.Create<IMyService, IMyService, IMyService, IMyService, IMyService, IMyService, MyServiceBase,
						IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The mock declaration has 1 additional implementation that is not an interface: Mockolate.Tests.TestHelpers.MyServiceBase");
		}

		[Fact]
		public async Task With8Arguments_SixthIsClass_ShouldThrow()
		{
			void Act()
			{
				_ =
					Mock.Create<IMyService, IMyService, IMyService, IMyService, IMyService, MyServiceBase, IMyService,
						IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The mock declaration has 1 additional implementation that is not an interface: Mockolate.Tests.TestHelpers.MyServiceBase");
		}

		[Fact]
		public async Task With8Arguments_ThirdIsClass_ShouldThrow()
		{
			void Act()
			{
				_ =
					Mock.Create<IMyService, IMyService, MyServiceBase, IMyService, IMyService, IMyService, IMyService,
						IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The mock declaration has 1 additional implementation that is not an interface: Mockolate.Tests.TestHelpers.MyServiceBase");
		}

		[Fact]
		public async Task With9Arguments_EighthIsClass_ShouldThrow()
		{
			void Act()
			{
				_ =
					Mock.Create<IMyService, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService,
						MyServiceBase, IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The mock declaration has 1 additional implementation that is not an interface: Mockolate.Tests.TestHelpers.MyServiceBase");
		}

		[Fact]
		public async Task With9Arguments_FifthIsClass_ShouldThrow()
		{
			void Act()
			{
				_ =
					Mock.Create<IMyService, IMyService, IMyService, IMyService, MyServiceBase, IMyService, IMyService,
						IMyService, IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The mock declaration has 1 additional implementation that is not an interface: Mockolate.Tests.TestHelpers.MyServiceBase");
		}

		[Fact]
		public async Task With9Arguments_FourthIsClass_ShouldThrow()
		{
			void Act()
			{
				_ =
					Mock.Create<IMyService, IMyService, IMyService, MyServiceBase, IMyService, IMyService, IMyService,
						IMyService, IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The mock declaration has 1 additional implementation that is not an interface: Mockolate.Tests.TestHelpers.MyServiceBase");
		}

		[Fact]
		public async Task With9Arguments_NinthIsClass_ShouldThrow()
		{
			void Act()
			{
				_ =
					Mock.Create<IMyService, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService,
						IMyService, MyServiceBase>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The mock declaration has 1 additional implementation that is not an interface: Mockolate.Tests.TestHelpers.MyServiceBase");
		}

		[Fact]
		public async Task With9Arguments_OnlyFirstArgumentIsClass_ShouldForwardBehaviorToBaseClass()
		{
			MockBehavior behavior = MockBehavior.Default.ThrowingWhenNotSetup();

			MyServiceBase sut =
				Mock.Create<MyServiceBase, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService,
					IMyService, IMyService>(behavior);

			await That(((IHasMockRegistration)sut).Registrations.Behavior).IsSameAs(behavior);
		}

		[Fact]
		public async Task With9Arguments_SecondIsClass_ShouldThrow()
		{
			void Act()
			{
				_ =
					Mock.Create<IMyService, MyServiceBase, IMyService, IMyService, IMyService, IMyService, IMyService,
						IMyService, IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The mock declaration has 1 additional implementation that is not an interface: Mockolate.Tests.TestHelpers.MyServiceBase");
		}

		[Fact]
		public async Task With9Arguments_SeventhIsClass_ShouldThrow()
		{
			void Act()
			{
				_ =
					Mock.Create<IMyService, IMyService, IMyService, IMyService, IMyService, IMyService, MyServiceBase,
						IMyService, IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The mock declaration has 1 additional implementation that is not an interface: Mockolate.Tests.TestHelpers.MyServiceBase");
		}

		[Fact]
		public async Task With9Arguments_SixthIsClass_ShouldThrow()
		{
			void Act()
			{
				_ =
					Mock.Create<IMyService, IMyService, IMyService, IMyService, IMyService, MyServiceBase, IMyService,
						IMyService, IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The mock declaration has 1 additional implementation that is not an interface: Mockolate.Tests.TestHelpers.MyServiceBase");
		}

		[Fact]
		public async Task With9Arguments_ThirdIsClass_ShouldThrow()
		{
			void Act()
			{
				_ =
					Mock.Create<IMyService, IMyService, MyServiceBase, IMyService, IMyService, IMyService, IMyService,
						IMyService, IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The mock declaration has 1 additional implementation that is not an interface: Mockolate.Tests.TestHelpers.MyServiceBase");
		}

		[Fact]
		public async Task WithAdditionalInterfacesFromDifferentNamespaces_ShouldHaveUniqueName()
		{
			int invocationCount1 = 0;
			int invocationCount2 = 0;
			IChocolateDispenser sut1 = Mock.Create<IChocolateDispenser, TestHelpers.IMyService>();
			IChocolateDispenser sut2 = Mock.Create<IChocolateDispenser, TestHelpers.Other.IMyService>();

			sut1.SetupIMyServiceMock.Method
				.DoSomething(It.IsAny<int>())
				.Do(() => invocationCount1++);
			sut2.SetupIMyService__2Mock.Method
				.DoSomething(It.IsAny<int>())
				.Do(() => invocationCount2++);

			((TestHelpers.IMyService)sut1).DoSomething(1);
			((TestHelpers.IMyService)sut1).DoSomething(2);
			((TestHelpers.Other.IMyService)sut2).DoSomething(1);
			((TestHelpers.Other.IMyService)sut2).DoSomething(2);
			((TestHelpers.Other.IMyService)sut2).DoSomething(3);

			await That(invocationCount1).IsEqualTo(2);
			await That(invocationCount2).IsEqualTo(3);
			await That(sut1.VerifyOnIMyServiceMock.Invoked
				.DoSomething(It.IsAny<int>())).Exactly(2);
			await That(() => sut1.VerifyOnIMyService__2Mock)
				.Throws<InvalidCastException>();
			await That(() => sut2.VerifyOnIMyServiceMock)
				.Throws<InvalidCastException>();
			await That(sut2.VerifyOnIMyService__2Mock.Invoked
				.DoSomething(It.IsAny<int>())).Exactly(3);
		}
	}
}
