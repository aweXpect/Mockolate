using Mockolate.Exceptions;
using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests;

public sealed partial class MockTests
{
	public sealed class FactoryTests
	{
		[Fact]
		public async Task Create_SealedClass_ShouldThrowMockException()
		{
			Mock.Factory factory = new(MockBehavior.Default);

			void Act()
			{
#pragma warning disable Mockolate0002
				_ = factory.Create<MySealedClass>();
#pragma warning restore Mockolate0002
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"Unable to mock type 'Mockolate.Tests.MockTests+MySealedClass'. The type is sealed and therefore not mockable.");
		}

		[Fact]
		public async Task Create_SealedClass_With1AdditionalInterface_ShouldThrowMockException()
		{
			Mock.Factory factory = new(MockBehavior.Default);

			void Act()
			{
#pragma warning disable Mockolate0002
				_ = factory.Create<MySealedClass, IMyService>();
#pragma warning restore Mockolate0002
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"Unable to mock type 'Mockolate.Tests.MockTests+MySealedClass'. The type is sealed and therefore not mockable.");
		}

		[Fact]
		public async Task Create_SealedClass_With2AdditionalInterfaces_ShouldThrowMockException()
		{
			Mock.Factory factory = new(MockBehavior.Default);

			void Act()
			{
#pragma warning disable Mockolate0002
				_ = factory.Create<MySealedClass, IMyService, IMyService>();
#pragma warning restore Mockolate0002
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"Unable to mock type 'Mockolate.Tests.MockTests+MySealedClass'. The type is sealed and therefore not mockable.");
		}

		[Fact]
		public async Task Create_SealedClass_With3AdditionalInterfaces_ShouldThrowMockException()
		{
			Mock.Factory factory = new(MockBehavior.Default);

			void Act()
			{
#pragma warning disable Mockolate0002
				_ = factory.Create<MySealedClass, IMyService, IMyService, IMyService>();
#pragma warning restore Mockolate0002
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"Unable to mock type 'Mockolate.Tests.MockTests+MySealedClass'. The type is sealed and therefore not mockable.");
		}

		[Fact]
		public async Task Create_SealedClass_With4AdditionalInterfaces_ShouldThrowMockException()
		{
			Mock.Factory factory = new(MockBehavior.Default);

			void Act()
			{
#pragma warning disable Mockolate0002
				_ = factory.Create<MySealedClass, IMyService, IMyService, IMyService, IMyService>();
#pragma warning restore Mockolate0002
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"Unable to mock type 'Mockolate.Tests.MockTests+MySealedClass'. The type is sealed and therefore not mockable.");
		}

		[Fact]
		public async Task Create_SealedClass_With5AdditionalInterfaces_ShouldThrowMockException()
		{
			Mock.Factory factory = new(MockBehavior.Default);

			void Act()
			{
#pragma warning disable Mockolate0002
				_ = factory.Create<MySealedClass, IMyService, IMyService, IMyService, IMyService, IMyService>();
#pragma warning restore Mockolate0002
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"Unable to mock type 'Mockolate.Tests.MockTests+MySealedClass'. The type is sealed and therefore not mockable.");
		}

		[Fact]
		public async Task Create_SealedClass_With6AdditionalInterfaces_ShouldThrowMockException()
		{
			Mock.Factory factory = new(MockBehavior.Default);

			void Act()
			{
#pragma warning disable Mockolate0002
				_ =
					factory.Create<MySealedClass,
						IMyService, IMyService, IMyService, IMyService, IMyService, IMyService>();
#pragma warning restore Mockolate0002
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"Unable to mock type 'Mockolate.Tests.MockTests+MySealedClass'. The type is sealed and therefore not mockable.");
		}

		[Fact]
		public async Task Create_SealedClass_With7AdditionalInterfaces_ShouldThrowMockException()
		{
			Mock.Factory factory = new(MockBehavior.Default);

			void Act()
			{
#pragma warning disable Mockolate0002
				_ = factory.Create<MySealedClass,
					IMyService, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService>();
#pragma warning restore Mockolate0002
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"Unable to mock type 'Mockolate.Tests.MockTests+MySealedClass'. The type is sealed and therefore not mockable.");
		}

		[Fact]
		public async Task Create_SealedClass_With8AdditionalInterfaces_ShouldThrowMockException()
		{
			Mock.Factory factory = new(MockBehavior.Default);

			void Act()
			{
#pragma warning disable Mockolate0002
				_ = factory.Create<MySealedClass,
					IMyService, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService>();
#pragma warning restore Mockolate0002
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"Unable to mock type 'Mockolate.Tests.MockTests+MySealedClass'. The type is sealed and therefore not mockable.");
		}

		[Fact]
		public async Task Create_SealedClass_WithConstructorParameters_ShouldThrowMockException()
		{
			Mock.Factory factory = new(MockBehavior.Default);

			void Act()
			{
#pragma warning disable Mockolate0002
				_ = factory.Create<MySealedClass>(BaseClass.WithConstructorParameters());
#pragma warning restore Mockolate0002
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"Unable to mock type 'Mockolate.Tests.MockTests+MySealedClass'. The type is sealed and therefore not mockable.");
		}

		[Fact]
		public async Task Create_WithSameNamedInterfaces_ShouldAppendIndex()
		{
			bool isDoSomethingCalled1 = false;
			bool isDoSomethingCalled2 = false;
			Mock.Factory factory = new(MockBehavior.Default);

			MyServiceBase mock =
				factory.Create<MyServiceBase, IMyService, TestHelpers.IMyService, TestHelpers.Other.IMyService>();

			mock.SetupIMyServiceMock.Method.DoSomething(It.IsAny<int>()).Do(() => isDoSomethingCalled1 = true);
			mock.SetupIMyService__2Mock.Method.DoSomething(It.IsAny<int>()).Do(() => isDoSomethingCalled2 = true);

			((TestHelpers.IMyService)mock).DoSomething(1);

			await That(isDoSomethingCalled1).IsTrue();
			await That(isDoSomethingCalled2).IsFalse();

			((TestHelpers.Other.IMyService)mock).DoSomething(1);

			await That(isDoSomethingCalled1).IsTrue();
			await That(isDoSomethingCalled2).IsTrue();
		}

		[Fact]
		public async Task ShouldUseDefinedBehavior()
		{
			MockBehavior behavior = MockBehavior.Default with
			{
				ThrowWhenNotSetup = true,
			};
			Mock.Factory factory = new(behavior);

			IMyService mock1 = factory.Create<IMyService>();
			MyServiceBase mock2 = factory.Create<MyServiceBase, IMyService>();

			await That(((IHasMockRegistration)mock1).Registrations.Behavior).IsSameAs(behavior);
			await That(((IHasMockRegistration)mock2).Registrations.Behavior).IsSameAs(behavior);
		}

		[Fact]
		public async Task WithSetups_ShouldApplySetups()
		{
			MockBehavior behavior = MockBehavior.Default.SkippingBaseClass();
			Mock.Factory factory = new(behavior);

			IMyService mock1 =
				factory.Create<IMyService>(setup => setup.Method.Multiply(It.IsAny<int>(), It.IsAny<int?>()).Returns(42));
			MyServiceBase mock2 = factory.Create<MyServiceBase, IMyService>(BaseClass.WithConstructorParameters(),
				setup => setup.Method.DoSomething(It.IsAny<int>(), It.Is(true)).Returns(5),
				setup => setup.Method.DoSomething(It.IsAny<int>(), It.Is(false)).Returns(6));

			int result = mock1.Multiply(2, null);
			int result21 = mock2.DoSomething(1, true);
			int result22 = mock2.DoSomething(1, false);

			await That(result).IsEqualTo(42);
			await That(result21).IsEqualTo(5);
			await That(result22).IsEqualTo(6);
		}
	}
}
