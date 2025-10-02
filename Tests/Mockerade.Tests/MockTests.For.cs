using Mockerade.Exceptions;
using Mockerade.Tests.TestHelpers;

namespace Mockerade.Tests;

public sealed partial class MockTests
{
	[Fact]
	public async Task For_2Arguments_SecondIsClass_ShouldThrow()
	{
		void Act()
			=> _ = new MyMock<IMyService, MyBaseClass>();

		await That(Act).Throws<MockException>()
			.WithMessage("The second generic type argument 'Mockerade.Tests.MockTests+MyBaseClass' is no interface.");
	}

	[Fact]
	public async Task For_2Arguments_OnlyFirstArgumentIsClass_ShouldForwardBehaviorToBaseClass()
	{
		var behavior = new MockBehavior
		{
			ThrowWhenNotSetup = true
		};

		Mock<MyBaseClass> sut = new MyMock<MyBaseClass, IMyService>(behavior: behavior);

		await That(((IMock)sut).Behavior).IsSameAs(behavior);
	}

	[Fact]
	public async Task For_3Arguments_SecondIsClass_ShouldThrow()
	{
		void Act()
			=> _ = new MyMock<IMyService, IMyService, MyBaseClass>();

		await That(Act).Throws<MockException>()
			.WithMessage("The third generic type argument 'Mockerade.Tests.MockTests+MyBaseClass' is no interface.");
	}

	[Fact]
	public async Task For_3Arguments_OnlyFirstArgumentIsClass_ShouldForwardBehaviorToBaseClass()
	{
		var behavior = new MockBehavior
		{
			ThrowWhenNotSetup = true
		};

		Mock<MyBaseClass> sut = new MyMock<MyBaseClass, IMyService, IMyService>(behavior: behavior);

		await That(((IMock)sut).Behavior).IsSameAs(behavior);
	}

	[Fact]
	public async Task For_4Arguments_SecondIsClass_ShouldThrow()
	{
		void Act()
			=> _ = new MyMock<IMyService, IMyService, IMyService, MyBaseClass>();

		await That(Act).Throws<MockException>()
			.WithMessage("The fourth generic type argument 'Mockerade.Tests.MockTests+MyBaseClass' is no interface.");
	}

	[Fact]
	public async Task For_4Arguments_OnlyFirstArgumentIsClass_ShouldForwardBehaviorToBaseClass()
	{
		var behavior = new MockBehavior
		{
			ThrowWhenNotSetup = true
		};

		Mock<MyBaseClass> sut = new MyMock<MyBaseClass, IMyService, IMyService, IMyService>(behavior: behavior);

		await That(((IMock)sut).Behavior).IsSameAs(behavior);
	}

	[Fact]
	public async Task For_5Arguments_SecondIsClass_ShouldThrow()
	{
		void Act()
			=> _ = new MyMock<IMyService, IMyService, IMyService, IMyService, MyBaseClass>();

		await That(Act).Throws<MockException>()
			.WithMessage("The fifth generic type argument 'Mockerade.Tests.MockTests+MyBaseClass' is no interface.");
	}

	[Fact]
	public async Task For_5Arguments_OnlyFirstArgumentIsClass_ShouldForwardBehaviorToBaseClass()
	{
		var behavior = new MockBehavior
		{
			ThrowWhenNotSetup = true
		};

		Mock<MyBaseClass> sut = new MyMock<MyBaseClass, IMyService, IMyService, IMyService, IMyService>(behavior: behavior);

		await That(((IMock)sut).Behavior).IsSameAs(behavior);
	}

	[Fact]
	public async Task For_6Arguments_SecondIsClass_ShouldThrow()
	{
		void Act()
			=> _ = new MyMock<IMyService, IMyService, IMyService, IMyService, IMyService, MyBaseClass>();

		await That(Act).Throws<MockException>()
			.WithMessage("The sixth generic type argument 'Mockerade.Tests.MockTests+MyBaseClass' is no interface.");
	}

	[Fact]
	public async Task For_6Arguments_OnlyFirstArgumentIsClass_ShouldForwardBehaviorToBaseClass()
	{
		var behavior = new MockBehavior
		{
			ThrowWhenNotSetup = true
		};

		Mock<MyBaseClass> sut = new MyMock<MyBaseClass, IMyService, IMyService, IMyService, IMyService, IMyService>(behavior: behavior);

		await That(((IMock)sut).Behavior).IsSameAs(behavior);
	}

	[Fact]
	public async Task For_7Arguments_SecondIsClass_ShouldThrow()
	{
		void Act()
			=> _ = new MyMock<IMyService, IMyService, IMyService, IMyService, IMyService, IMyService, MyBaseClass>();

		await That(Act).Throws<MockException>()
			.WithMessage("The seventh generic type argument 'Mockerade.Tests.MockTests+MyBaseClass' is no interface.");
	}

	[Fact]
	public async Task For_7Arguments_OnlyFirstArgumentIsClass_ShouldForwardBehaviorToBaseClass()
	{
		var behavior = new MockBehavior
		{
			ThrowWhenNotSetup = true
		};

		Mock<MyBaseClass> sut = new MyMock<MyBaseClass, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService>(behavior: behavior);

		await That(((IMock)sut).Behavior).IsSameAs(behavior);
	}

	[Fact]
	public async Task For_8Arguments_SecondIsClass_ShouldThrow()
	{
		void Act()
			=> _ = new MyMock<IMyService, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService, MyBaseClass>();

		await That(Act).Throws<MockException>()
			.WithMessage("The eighth generic type argument 'Mockerade.Tests.MockTests+MyBaseClass' is no interface.");
	}

	[Fact]
	public async Task For_8Arguments_OnlyFirstArgumentIsClass_ShouldForwardBehaviorToBaseClass()
	{
		var behavior = new MockBehavior
		{
			ThrowWhenNotSetup = true
		};

		Mock<MyBaseClass> sut = new MyMock<MyBaseClass, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService>(behavior: behavior);

		await That(((IMock)sut).Behavior).IsSameAs(behavior);
	}

	[Fact]
	public async Task For_9Arguments_SecondIsClass_ShouldThrow()
	{
		void Act()
			=> _ = new MyMock<IMyService, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService, MyBaseClass>();

		await That(Act).Throws<MockException>()
			.WithMessage("The ninth generic type argument 'Mockerade.Tests.MockTests+MyBaseClass' is no interface.");
	}

	[Fact]
	public async Task For_9Arguments_OnlyFirstArgumentIsClass_ShouldForwardBehaviorToBaseClass()
	{
		var behavior = new MockBehavior
		{
			ThrowWhenNotSetup = true
		};

		Mock<MyBaseClass> sut = new MyMock<MyBaseClass, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService>(behavior: behavior);

		await That(((IMock)sut).Behavior).IsSameAs(behavior);
	}
}
