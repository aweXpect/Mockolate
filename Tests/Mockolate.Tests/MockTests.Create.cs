using Mockolate.Checks;
using Mockolate.Exceptions;
using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests;

public sealed partial class MockTests
{
	[Fact]
	public async Task Create_2Arguments_OnlyFirstArgumentIsClass_ShouldForwardBehaviorToBaseClass()
	{
		MockBehavior behavior = new()
		{
			ThrowWhenNotSetup = true,
		};

		Mock<MyBaseClass, IMyService> sut = new MyMock<MyBaseClass, IMyService>(behavior: behavior);

		await That(((IMock)sut).Behavior).IsSameAs(behavior);
		await That(sut.Accessed).IsNotNull();
		await That(sut.Invoked).IsNotNull();
		await That(sut.Event).IsNotNull();
	}

	[Fact]
	public async Task Create_2Arguments_SecondIsClass_ShouldThrow()
	{
		void Act()
			=> _ = new MyMock<IMyService, MyBaseClass>();

		await That(Act).Throws<MockException>()
			.WithMessage("The second generic type argument 'Mockolate.Tests.MockTests+MyBaseClass' is no interface.");
	}

	[Fact]
	public async Task Create_3Arguments_OnlyFirstArgumentIsClass_ShouldForwardBehaviorToBaseClass()
	{
		MockBehavior behavior = new()
		{
			ThrowWhenNotSetup = true,
		};

		Mock<MyBaseClass, IMyService, IMyService> sut =
			new MyMock<MyBaseClass, IMyService, IMyService>(behavior: behavior);

		await That(((IMock)sut).Behavior).IsSameAs(behavior);
		await That(sut.Accessed).IsNotNull();
		await That(sut.Invoked).IsNotNull();
		await That(sut.Event).IsNotNull();
	}

	[Fact]
	public async Task Create_3Arguments_SecondIsClass_ShouldThrow()
	{
		void Act()
			=> _ = new MyMock<IMyService, MyBaseClass, IMyService>();

		await That(Act).Throws<MockException>()
			.WithMessage("The second generic type argument 'Mockolate.Tests.MockTests+MyBaseClass' is no interface.");
	}

	[Fact]
	public async Task Create_3Arguments_ThirdIsClass_ShouldThrow()
	{
		void Act()
			=> _ = new MyMock<IMyService, IMyService, MyBaseClass>();

		await That(Act).Throws<MockException>()
			.WithMessage("The third generic type argument 'Mockolate.Tests.MockTests+MyBaseClass' is no interface.");
	}

	[Fact]
	public async Task Create_4Arguments_OnlyFirstArgumentIsClass_ShouldForwardBehaviorToBaseClass()
	{
		MockBehavior behavior = new()
		{
			ThrowWhenNotSetup = true,
		};

		MyMock<MyBaseClass, IMyService, IMyService, IMyService> sut = new(behavior: behavior);

		await That(((IMock)sut).Behavior).IsSameAs(behavior);
		await That(sut.Accessed).IsNotNull();
		await That(sut.Invoked).IsNotNull();
		await That(sut.Event).IsNotNull();
	}

	[Fact]
	public async Task Create_4Arguments_SecondIsClass_ShouldThrow()
	{
		void Act()
			=> _ = new MyMock<IMyService, MyBaseClass, IMyService, IMyService>();

		await That(Act).Throws<MockException>()
			.WithMessage("The second generic type argument 'Mockolate.Tests.MockTests+MyBaseClass' is no interface.");
	}

	[Fact]
	public async Task Create_4Arguments_ThirdIsClass_ShouldThrow()
	{
		void Act()
			=> _ = new MyMock<IMyService, IMyService, MyBaseClass, IMyService>();

		await That(Act).Throws<MockException>()
			.WithMessage("The third generic type argument 'Mockolate.Tests.MockTests+MyBaseClass' is no interface.");
	}

	[Fact]
	public async Task Create_4Arguments_FourthIsClass_ShouldThrow()
	{
		void Act()
			=> _ = new MyMock<IMyService, IMyService, IMyService, MyBaseClass>();

		await That(Act).Throws<MockException>()
			.WithMessage("The fourth generic type argument 'Mockolate.Tests.MockTests+MyBaseClass' is no interface.");
	}

	[Fact]
	public async Task Create_5Arguments_OnlyFirstArgumentIsClass_ShouldForwardBehaviorToBaseClass()
	{
		MockBehavior behavior = new()
		{
			ThrowWhenNotSetup = true,
		};

		MyMock<MyBaseClass, IMyService, IMyService, IMyService, IMyService> sut = new(behavior: behavior);

		await That(((IMock)sut).Behavior).IsSameAs(behavior);
		await That(sut.Accessed).IsNotNull();
		await That(sut.Invoked).IsNotNull();
		await That(sut.Event).IsNotNull();
	}

	[Fact]
	public async Task Create_5Arguments_SecondIsClass_ShouldThrow()
	{
		void Act()
			=> _ = new MyMock<IMyService, MyBaseClass, IMyService, IMyService, IMyService>();

		await That(Act).Throws<MockException>()
			.WithMessage("The second generic type argument 'Mockolate.Tests.MockTests+MyBaseClass' is no interface.");
	}

	[Fact]
	public async Task Create_5Arguments_ThirdIsClass_ShouldThrow()
	{
		void Act()
			=> _ = new MyMock<IMyService, IMyService, MyBaseClass, IMyService, IMyService>();

		await That(Act).Throws<MockException>()
			.WithMessage("The third generic type argument 'Mockolate.Tests.MockTests+MyBaseClass' is no interface.");
	}

	[Fact]
	public async Task Create_5Arguments_FourthIsClass_ShouldThrow()
	{
		void Act()
			=> _ = new MyMock<IMyService, IMyService, IMyService, MyBaseClass, IMyService>();

		await That(Act).Throws<MockException>()
			.WithMessage("The fourth generic type argument 'Mockolate.Tests.MockTests+MyBaseClass' is no interface.");
	}

	[Fact]
	public async Task Create_5Arguments_FifthIsClass_ShouldThrow()
	{
		void Act()
			=> _ = new MyMock<IMyService, IMyService, IMyService, IMyService, MyBaseClass>();

		await That(Act).Throws<MockException>()
			.WithMessage("The fifth generic type argument 'Mockolate.Tests.MockTests+MyBaseClass' is no interface.");
	}

	[Fact]
	public async Task Create_6Arguments_OnlyFirstArgumentIsClass_ShouldForwardBehaviorToBaseClass()
	{
		MockBehavior behavior = new()
		{
			ThrowWhenNotSetup = true,
		};

		MyMock<MyBaseClass, IMyService, IMyService, IMyService, IMyService, IMyService> sut = new(behavior: behavior);

		await That(((IMock)sut).Behavior).IsSameAs(behavior);
		await That(sut.Accessed).IsNotNull();
		await That(sut.Invoked).IsNotNull();
		await That(sut.Event).IsNotNull();
	}

	[Fact]
	public async Task Create_6Arguments_SecondIsClass_ShouldThrow()
	{
		void Act()
			=> _ = new MyMock<IMyService, MyBaseClass, IMyService, IMyService, IMyService, IMyService>();

		await That(Act).Throws<MockException>()
			.WithMessage("The second generic type argument 'Mockolate.Tests.MockTests+MyBaseClass' is no interface.");
	}

	[Fact]
	public async Task Create_6Arguments_ThirdIsClass_ShouldThrow()
	{
		void Act()
			=> _ = new MyMock<IMyService, IMyService, MyBaseClass, IMyService, IMyService, IMyService>();

		await That(Act).Throws<MockException>()
			.WithMessage("The third generic type argument 'Mockolate.Tests.MockTests+MyBaseClass' is no interface.");
	}

	[Fact]
	public async Task Create_6Arguments_FourthIsClass_ShouldThrow()
	{
		void Act()
			=> _ = new MyMock<IMyService, IMyService, IMyService, MyBaseClass, IMyService, IMyService>();

		await That(Act).Throws<MockException>()
			.WithMessage("The fourth generic type argument 'Mockolate.Tests.MockTests+MyBaseClass' is no interface.");
	}

	[Fact]
	public async Task Create_6Arguments_FifthIsClass_ShouldThrow()
	{
		void Act()
			=> _ = new MyMock<IMyService, IMyService, IMyService, IMyService, MyBaseClass, IMyService>();

		await That(Act).Throws<MockException>()
			.WithMessage("The fifth generic type argument 'Mockolate.Tests.MockTests+MyBaseClass' is no interface.");
	}

	[Fact]
	public async Task Create_6Arguments_SixthIsClass_ShouldThrow()
	{
		void Act()
			=> _ = new MyMock<IMyService, IMyService, IMyService, IMyService, IMyService, MyBaseClass>();

		await That(Act).Throws<MockException>()
			.WithMessage("The sixth generic type argument 'Mockolate.Tests.MockTests+MyBaseClass' is no interface.");
	}

	[Fact]
	public async Task Create_7Arguments_OnlyFirstArgumentIsClass_ShouldForwardBehaviorToBaseClass()
	{
		MockBehavior behavior = new()
		{
			ThrowWhenNotSetup = true,
		};

		MyMock<MyBaseClass, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService> sut =
			new(
				behavior: behavior);

		await That(((IMock)sut).Behavior).IsSameAs(behavior);
		await That(sut.Accessed).IsNotNull();
		await That(sut.Invoked).IsNotNull();
		await That(sut.Event).IsNotNull();
	}

	[Fact]
	public async Task Create_7Arguments_SecondIsClass_ShouldThrow()
	{
		void Act()
			=> _ = new MyMock<IMyService, MyBaseClass, IMyService, IMyService, IMyService, IMyService, IMyService>();

		await That(Act).Throws<MockException>()
			.WithMessage("The second generic type argument 'Mockolate.Tests.MockTests+MyBaseClass' is no interface.");
	}

	[Fact]
	public async Task Create_7Arguments_ThirdIsClass_ShouldThrow()
	{
		void Act()
			=> _ = new MyMock<IMyService, IMyService, MyBaseClass, IMyService, IMyService, IMyService, IMyService>();

		await That(Act).Throws<MockException>()
			.WithMessage("The third generic type argument 'Mockolate.Tests.MockTests+MyBaseClass' is no interface.");
	}

	[Fact]
	public async Task Create_7Arguments_FourthIsClass_ShouldThrow()
	{
		void Act()
			=> _ = new MyMock<IMyService, IMyService, IMyService, MyBaseClass, IMyService, IMyService, IMyService>();

		await That(Act).Throws<MockException>()
			.WithMessage("The fourth generic type argument 'Mockolate.Tests.MockTests+MyBaseClass' is no interface.");
	}

	[Fact]
	public async Task Create_7Arguments_FifthIsClass_ShouldThrow()
	{
		void Act()
			=> _ = new MyMock<IMyService, IMyService, IMyService, IMyService, MyBaseClass, IMyService, IMyService>();

		await That(Act).Throws<MockException>()
			.WithMessage("The fifth generic type argument 'Mockolate.Tests.MockTests+MyBaseClass' is no interface.");
	}

	[Fact]
	public async Task Create_7Arguments_SixthIsClass_ShouldThrow()
	{
		void Act()
			=> _ = new MyMock<IMyService, IMyService, IMyService, IMyService, IMyService, MyBaseClass, IMyService>();

		await That(Act).Throws<MockException>()
			.WithMessage("The sixth generic type argument 'Mockolate.Tests.MockTests+MyBaseClass' is no interface.");
	}

	[Fact]
	public async Task Create_7Arguments_SeventhIsClass_ShouldThrow()
	{
		void Act()
			=> _ = new MyMock<IMyService, IMyService, IMyService, IMyService, IMyService, IMyService, MyBaseClass>();

		await That(Act).Throws<MockException>()
			.WithMessage("The seventh generic type argument 'Mockolate.Tests.MockTests+MyBaseClass' is no interface.");
	}

	[Fact]
	public async Task Create_8Arguments_OnlyFirstArgumentIsClass_ShouldForwardBehaviorToBaseClass()
	{
		MockBehavior behavior = new()
		{
			ThrowWhenNotSetup = true,
		};

		MyMock<MyBaseClass, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService> sut =
			new(
				behavior: behavior);

		await That(((IMock)sut).Behavior).IsSameAs(behavior);
		await That(sut.Accessed).IsNotNull();
		await That(sut.Invoked).IsNotNull();
		await That(sut.Event).IsNotNull();
	}

	[Fact]
	public async Task Create_8Arguments_SecondIsClass_ShouldThrow()
	{
		void Act()
			=> _ =
				new MyMock<IMyService, MyBaseClass, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService>();

		await That(Act).Throws<MockException>()
			.WithMessage("The second generic type argument 'Mockolate.Tests.MockTests+MyBaseClass' is no interface.");
	}

	[Fact]
	public async Task Create_8Arguments_ThirdIsClass_ShouldThrow()
	{
		void Act()
			=> _ =
				new MyMock<IMyService, IMyService, MyBaseClass, IMyService, IMyService, IMyService, IMyService, IMyService>();

		await That(Act).Throws<MockException>()
			.WithMessage("The third generic type argument 'Mockolate.Tests.MockTests+MyBaseClass' is no interface.");
	}

	[Fact]
	public async Task Create_8Arguments_FourthIsClass_ShouldThrow()
	{
		void Act()
			=> _ =
				new MyMock<IMyService, IMyService, IMyService, MyBaseClass, IMyService, IMyService, IMyService, IMyService>();

		await That(Act).Throws<MockException>()
			.WithMessage("The fourth generic type argument 'Mockolate.Tests.MockTests+MyBaseClass' is no interface.");
	}

	[Fact]
	public async Task Create_8Arguments_FifthIsClass_ShouldThrow()
	{
		void Act()
			=> _ =
				new MyMock<IMyService, IMyService, IMyService, IMyService, MyBaseClass, IMyService, IMyService, IMyService>();

		await That(Act).Throws<MockException>()
			.WithMessage("The fifth generic type argument 'Mockolate.Tests.MockTests+MyBaseClass' is no interface.");
	}

	[Fact]
	public async Task Create_8Arguments_SixthIsClass_ShouldThrow()
	{
		void Act()
			=> _ =
				new MyMock<IMyService, IMyService, IMyService, IMyService, IMyService, MyBaseClass, IMyService, IMyService>();

		await That(Act).Throws<MockException>()
			.WithMessage("The sixth generic type argument 'Mockolate.Tests.MockTests+MyBaseClass' is no interface.");
	}

	[Fact]
	public async Task Create_8Arguments_SeventhIsClass_ShouldThrow()
	{
		void Act()
			=> _ =
				new MyMock<IMyService, IMyService, IMyService, IMyService, IMyService, IMyService, MyBaseClass, IMyService>();

		await That(Act).Throws<MockException>()
			.WithMessage("The seventh generic type argument 'Mockolate.Tests.MockTests+MyBaseClass' is no interface.");
	}

	[Fact]
	public async Task Create_8Arguments_EighthIsClass_ShouldThrow()
	{
		void Act()
			=> _ =
				new MyMock<IMyService, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService,
					MyBaseClass>();

		await That(Act).Throws<MockException>()
			.WithMessage("The eighth generic type argument 'Mockolate.Tests.MockTests+MyBaseClass' is no interface.");
	}

	[Fact]
	public async Task Create_9Arguments_OnlyFirstArgumentIsClass_ShouldForwardBehaviorToBaseClass()
	{
		MockBehavior behavior = new()
		{
			ThrowWhenNotSetup = true,
		};

		MyMock<MyBaseClass, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService,
			IMyService> sut = new(behavior: behavior);

		await That(((IMock)sut).Behavior).IsSameAs(behavior);
		await That(sut.Accessed).IsNotNull();
		await That(sut.Invoked).IsNotNull();
		await That(sut.Event).IsNotNull();
	}

	[Fact]
	public async Task Create_9Arguments_SecondIsClass_ShouldThrow()
	{
		void Act()
			=> _ =
				new MyMock<IMyService, MyBaseClass, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService>();

		await That(Act).Throws<MockException>()
			.WithMessage("The second generic type argument 'Mockolate.Tests.MockTests+MyBaseClass' is no interface.");
	}

	[Fact]
	public async Task Create_9Arguments_ThirdIsClass_ShouldThrow()
	{
		void Act()
			=> _ =
				new MyMock<IMyService, IMyService, MyBaseClass, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService>();

		await That(Act).Throws<MockException>()
			.WithMessage("The third generic type argument 'Mockolate.Tests.MockTests+MyBaseClass' is no interface.");
	}

	[Fact]
	public async Task Create_9Arguments_FourthIsClass_ShouldThrow()
	{
		void Act()
			=> _ =
				new MyMock<IMyService, IMyService, IMyService, MyBaseClass, IMyService, IMyService, IMyService, IMyService, IMyService>();

		await That(Act).Throws<MockException>()
			.WithMessage("The fourth generic type argument 'Mockolate.Tests.MockTests+MyBaseClass' is no interface.");
	}

	[Fact]
	public async Task Create_9Arguments_FifthIsClass_ShouldThrow()
	{
		void Act()
			=> _ =
				new MyMock<IMyService, IMyService, IMyService, IMyService, MyBaseClass, IMyService, IMyService, IMyService, IMyService>();

		await That(Act).Throws<MockException>()
			.WithMessage("The fifth generic type argument 'Mockolate.Tests.MockTests+MyBaseClass' is no interface.");
	}

	[Fact]
	public async Task Create_9Arguments_SixthIsClass_ShouldThrow()
	{
		void Act()
			=> _ =
				new MyMock<IMyService, IMyService, IMyService, IMyService, IMyService, MyBaseClass, IMyService, IMyService, IMyService>();

		await That(Act).Throws<MockException>()
			.WithMessage("The sixth generic type argument 'Mockolate.Tests.MockTests+MyBaseClass' is no interface.");
	}

	[Fact]
	public async Task Create_9Arguments_SeventhIsClass_ShouldThrow()
	{
		void Act()
			=> _ =
				new MyMock<IMyService, IMyService, IMyService, IMyService, IMyService, IMyService, MyBaseClass, IMyService, IMyService>();

		await That(Act).Throws<MockException>()
			.WithMessage("The seventh generic type argument 'Mockolate.Tests.MockTests+MyBaseClass' is no interface.");
	}

	[Fact]
	public async Task Create_9Arguments_EighthIsClass_ShouldThrow()
	{
		void Act()
			=> _ =
				new MyMock<IMyService, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService, MyBaseClass, IMyService>();

		await That(Act).Throws<MockException>()
			.WithMessage("The eighth generic type argument 'Mockolate.Tests.MockTests+MyBaseClass' is no interface.");
	}

	[Fact]
	public async Task Create_9Arguments_NinthIsClass_ShouldThrow()
	{
		void Act()
			=> _ =
				new MyMock<IMyService, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService, MyBaseClass>();

		await That(Act).Throws<MockException>()
			.WithMessage("The ninth generic type argument 'Mockolate.Tests.MockTests+MyBaseClass' is no interface.");
	}
}
