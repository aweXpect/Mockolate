using Mockolate.Exceptions;
using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests;

public sealed partial class MockTests
{
	[Fact]
	public async Task Factory_ShouldUseDefinedBehavior()
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

	[Fact]
	public async Task Factory_Create_SealedClass_ShouldThrowMockException()
	{
		Mock.Factory factory = new(MockBehavior.Default);

		void Act()
			=> _ = factory.Create<MySealedClass>();

		await That(Act).Throws<MockException>()
			.WithMessage(
				"The type 'Mockolate.Tests.MockTests+MySealedClass' is sealed and therefore not mockable.");
	}

	[Fact]
	public async Task Factory_Create_WithConstructorParameters_SealedClass_ShouldThrowMockException()
	{
		Mock.Factory factory = new(MockBehavior.Default);

		void Act()
			=> _ = factory.Create<MySealedClass>(BaseClass.WithConstructorParameters());

		await That(Act).Throws<MockException>()
			.WithMessage(
				"The type 'Mockolate.Tests.MockTests+MySealedClass' is sealed and therefore not mockable.");
	}

	[Fact]
	public async Task Factory_Create_WithSameNamedInterfaces_ShouldAppendIndex()
	{
		bool isDoSomethingCalled1 = false;
		bool isDoSomethingCalled2 = false;
		Mock.Factory factory = new(MockBehavior.Default);

		var mock = factory.Create<MyServiceBase, IMyService, TestHelpers.IMyService, TestHelpers.Other.IMyService>();

		mock.SetupIMyService.Method.DoSomething(With.Any<int>()).Callback(() => isDoSomethingCalled1 = true);
		mock.SetupIMyService__2.Method.DoSomething(With.Any<int>()).Callback(() => isDoSomethingCalled2 = true);

		mock.SubjectForIMyService.DoSomething(1);

		await That(isDoSomethingCalled1).IsTrue();
		await That(isDoSomethingCalled2).IsFalse();

		mock.SubjectForIMyService__2.DoSomething(1);

		await That(isDoSomethingCalled1).IsTrue();
		await That(isDoSomethingCalled2).IsTrue();
	}
}
