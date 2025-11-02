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
				=> _ = factory.Create<MySealedClass>();

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The type 'Mockolate.Tests.MockTests+MySealedClass' is sealed and therefore not mockable.");
		}

		[Fact]
		public async Task Create_SealedClass_With1AdditionalInterface_ShouldThrowMockException()
		{
			Mock.Factory factory = new(MockBehavior.Default);

			void Act()
				=> _ = factory.Create<MySealedClass, IMyService>();

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The type 'Mockolate.Tests.MockTests+MySealedClass' is sealed and therefore not mockable.");
		}

		[Fact]
		public async Task Create_SealedClass_With2AdditionalInterfaces_ShouldThrowMockException()
		{
			Mock.Factory factory = new(MockBehavior.Default);

			void Act()
				=> _ = factory.Create<MySealedClass, IMyService, IMyService>();

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The type 'Mockolate.Tests.MockTests+MySealedClass' is sealed and therefore not mockable.");
		}

		[Fact]
		public async Task Create_SealedClass_With3AdditionalInterfaces_ShouldThrowMockException()
		{
			Mock.Factory factory = new(MockBehavior.Default);

			void Act()
				=> _ = factory.Create<MySealedClass, IMyService, IMyService, IMyService>();

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The type 'Mockolate.Tests.MockTests+MySealedClass' is sealed and therefore not mockable.");
		}

		[Fact]
		public async Task Create_SealedClass_With4AdditionalInterfaces_ShouldThrowMockException()
		{
			Mock.Factory factory = new(MockBehavior.Default);

			void Act()
				=> _ = factory.Create<MySealedClass, IMyService, IMyService, IMyService, IMyService>();

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The type 'Mockolate.Tests.MockTests+MySealedClass' is sealed and therefore not mockable.");
		}

		[Fact]
		public async Task Create_SealedClass_With5AdditionalInterfaces_ShouldThrowMockException()
		{
			Mock.Factory factory = new(MockBehavior.Default);

			void Act()
				=> _ = factory.Create<MySealedClass, IMyService, IMyService, IMyService, IMyService, IMyService>();

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The type 'Mockolate.Tests.MockTests+MySealedClass' is sealed and therefore not mockable.");
		}

		[Fact]
		public async Task Create_SealedClass_With6AdditionalInterfaces_ShouldThrowMockException()
		{
			Mock.Factory factory = new(MockBehavior.Default);

			void Act()
				=> _ =
					factory
						.Create<MySealedClass, IMyService, IMyService, IMyService, IMyService, IMyService,
							IMyService>();

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The type 'Mockolate.Tests.MockTests+MySealedClass' is sealed and therefore not mockable.");
		}

		[Fact]
		public async Task Create_SealedClass_With7AdditionalInterfaces_ShouldThrowMockException()
		{
			Mock.Factory factory = new(MockBehavior.Default);

			void Act()
				=> _ = factory
					.Create<MySealedClass, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService,
						IMyService>();

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The type 'Mockolate.Tests.MockTests+MySealedClass' is sealed and therefore not mockable.");
		}

		[Fact]
		public async Task Create_SealedClass_With8AdditionalInterfaces_ShouldThrowMockException()
		{
			Mock.Factory factory = new(MockBehavior.Default);

			void Act()
				=> _ = factory
					.Create<MySealedClass, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService,
						IMyService, IMyService>();

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The type 'Mockolate.Tests.MockTests+MySealedClass' is sealed and therefore not mockable.");
		}

		[Fact]
		public async Task Create_WithConstructorParameters_SealedClass_ShouldThrowMockException()
		{
			Mock.Factory factory = new(MockBehavior.Default);

			void Act()
				=> _ = factory.Create<MySealedClass>(BaseClass.WithConstructorParameters());

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The type 'Mockolate.Tests.MockTests+MySealedClass' is sealed and therefore not mockable.");
		}

		[Fact]
		public async Task Create_WithSameNamedInterfaces_ShouldAppendIndex()
		{
			bool isDoSomethingCalled1 = false;
			bool isDoSomethingCalled2 = false;
			Mock.Factory factory = new(MockBehavior.Default);

			Mock<MyServiceBase, IMyService, TestHelpers.IMyService, TestHelpers.Other.IMyService> mock =
				factory.Create<MyServiceBase, IMyService, TestHelpers.IMyService, TestHelpers.Other.IMyService>();

			mock.SetupIMyService.Method.DoSomething(With.Any<int>()).Callback(() => isDoSomethingCalled1 = true);
			mock.SetupIMyService__2.Method.DoSomething(With.Any<int>()).Callback(() => isDoSomethingCalled2 = true);

			mock.SubjectForIMyService.DoSomething(1);

			await That(isDoSomethingCalled1).IsTrue();
			await That(isDoSomethingCalled2).IsFalse();

			mock.SubjectForIMyService__2.DoSomething(1);

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

			Mock<IMyService> mock1 = factory.Create<IMyService>();
			Mock<MyServiceBase, IMyService> mock2 = factory.Create<MyServiceBase, IMyService>();

			await That(((IMock)mock1).Behavior).IsSameAs(behavior);
			await That(((IMock)mock2).Behavior).IsSameAs(behavior);
		}
	}
}
