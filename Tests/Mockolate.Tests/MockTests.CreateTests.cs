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
			MockBehavior behavior = new()
			{
				ThrowWhenNotSetup = true
			};

			MyServiceBase sut = Mock.Create<MyServiceBase, IMyService>(behavior);

			await That(((IHasMockRegistration)sut).Registrations.Behavior).IsSameAs(behavior);
		}
/* TODO
		[Fact]
		public async Task With2Arguments_SecondIsClass_ShouldThrow()
		{
			void Act()
			{
				_ = new MyMock<IMyService, MyServiceBase>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The second generic type argument 'Mockolate.Tests.TestHelpers.MyServiceBase' is no interface.");
		}

		[Fact]
		public async Task With3Arguments_OnlyFirstArgumentIsClass_ShouldForwardBehaviorToBaseClass()
		{
			MockBehavior behavior = new()
			{
				ThrowWhenNotSetup = true
			};

			Mock<MyServiceBase, IMyService, IMyService> sut =
				new MyMock<MyServiceBase, IMyService, IMyService>(behavior: behavior);

			await That(((IMock)sut).Behavior).IsSameAs(behavior);
			await That(sut.Verify).IsNotNull();
		}

		[Fact]
		public async Task With3Arguments_SecondIsClass_ShouldThrow()
		{
			void Act()
			{
				_ = new MyMock<IMyService, MyServiceBase, IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The second generic type argument 'Mockolate.Tests.TestHelpers.MyServiceBase' is no interface.");
		}

		[Fact]
		public async Task With3Arguments_ThirdIsClass_ShouldThrow()
		{
			void Act()
			{
				_ = new MyMock<IMyService, IMyService, MyServiceBase>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The third generic type argument 'Mockolate.Tests.TestHelpers.MyServiceBase' is no interface.");
		}

		[Fact]
		public async Task With4Arguments_FourthIsClass_ShouldThrow()
		{
			void Act()
			{
				_ = new MyMock<IMyService, IMyService, IMyService, MyServiceBase>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The fourth generic type argument 'Mockolate.Tests.TestHelpers.MyServiceBase' is no interface.");
		}

		[Fact]
		public async Task With4Arguments_OnlyFirstArgumentIsClass_ShouldForwardBehaviorToBaseClass()
		{
			MockBehavior behavior = new()
			{
				ThrowWhenNotSetup = true
			};

			MyMock<MyServiceBase, IMyService, IMyService, IMyService> sut = new(behavior: behavior);

			await That(((IMock)sut).Behavior).IsSameAs(behavior);
			await That(sut.Verify).IsNotNull();
		}

		[Fact]
		public async Task With4Arguments_SecondIsClass_ShouldThrow()
		{
			void Act()
			{
				_ = new MyMock<IMyService, MyServiceBase, IMyService, IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The second generic type argument 'Mockolate.Tests.TestHelpers.MyServiceBase' is no interface.");
		}

		[Fact]
		public async Task With4Arguments_ThirdIsClass_ShouldThrow()
		{
			void Act()
			{
				_ = new MyMock<IMyService, IMyService, MyServiceBase, IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The third generic type argument 'Mockolate.Tests.TestHelpers.MyServiceBase' is no interface.");
		}

		[Fact]
		public async Task With5Arguments_FifthIsClass_ShouldThrow()
		{
			void Act()
			{
				_ = new MyMock<IMyService, IMyService, IMyService, IMyService, MyServiceBase>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The fifth generic type argument 'Mockolate.Tests.TestHelpers.MyServiceBase' is no interface.");
		}

		[Fact]
		public async Task With5Arguments_FourthIsClass_ShouldThrow()
		{
			void Act()
			{
				_ = new MyMock<IMyService, IMyService, IMyService, MyServiceBase, IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The fourth generic type argument 'Mockolate.Tests.TestHelpers.MyServiceBase' is no interface.");
		}

		[Fact]
		public async Task With5Arguments_OnlyFirstArgumentIsClass_ShouldForwardBehaviorToBaseClass()
		{
			MockBehavior behavior = new()
			{
				ThrowWhenNotSetup = true
			};

			MyMock<MyServiceBase, IMyService, IMyService, IMyService, IMyService> sut = new(behavior: behavior);

			await That(((IMock)sut).Behavior).IsSameAs(behavior);
			await That(sut.Verify).IsNotNull();
		}

		[Fact]
		public async Task With5Arguments_SecondIsClass_ShouldThrow()
		{
			void Act()
			{
				_ = new MyMock<IMyService, MyServiceBase, IMyService, IMyService, IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The second generic type argument 'Mockolate.Tests.TestHelpers.MyServiceBase' is no interface.");
		}

		[Fact]
		public async Task With5Arguments_ThirdIsClass_ShouldThrow()
		{
			void Act()
			{
				_ = new MyMock<IMyService, IMyService, MyServiceBase, IMyService, IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The third generic type argument 'Mockolate.Tests.TestHelpers.MyServiceBase' is no interface.");
		}

		[Fact]
		public async Task With6Arguments_FifthIsClass_ShouldThrow()
		{
			void Act()
			{
				_ = new MyMock<IMyService, IMyService, IMyService, IMyService, MyServiceBase, IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The fifth generic type argument 'Mockolate.Tests.TestHelpers.MyServiceBase' is no interface.");
		}

		[Fact]
		public async Task With6Arguments_FourthIsClass_ShouldThrow()
		{
			void Act()
			{
				_ = new MyMock<IMyService, IMyService, IMyService, MyServiceBase, IMyService, IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The fourth generic type argument 'Mockolate.Tests.TestHelpers.MyServiceBase' is no interface.");
		}

		[Fact]
		public async Task With6Arguments_OnlyFirstArgumentIsClass_ShouldForwardBehaviorToBaseClass()
		{
			MockBehavior behavior = new()
			{
				ThrowWhenNotSetup = true
			};

			MyMock<MyServiceBase, IMyService, IMyService, IMyService, IMyService, IMyService> sut =
				new(behavior: behavior);

			await That(((IMock)sut).Behavior).IsSameAs(behavior);
			await That(sut.Verify).IsNotNull();
		}

		[Fact]
		public async Task With6Arguments_SecondIsClass_ShouldThrow()
		{
			void Act()
			{
				_ = new MyMock<IMyService, MyServiceBase, IMyService, IMyService, IMyService, IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The second generic type argument 'Mockolate.Tests.TestHelpers.MyServiceBase' is no interface.");
		}

		[Fact]
		public async Task With6Arguments_SixthIsClass_ShouldThrow()
		{
			void Act()
			{
				_ = new MyMock<IMyService, IMyService, IMyService, IMyService, IMyService, MyServiceBase>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The sixth generic type argument 'Mockolate.Tests.TestHelpers.MyServiceBase' is no interface.");
		}

		[Fact]
		public async Task With6Arguments_ThirdIsClass_ShouldThrow()
		{
			void Act()
			{
				_ = new MyMock<IMyService, IMyService, MyServiceBase, IMyService, IMyService, IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The third generic type argument 'Mockolate.Tests.TestHelpers.MyServiceBase' is no interface.");
		}

		[Fact]
		public async Task With7Arguments_FifthIsClass_ShouldThrow()
		{
			void Act()
			{
				_ =
					new MyMock<IMyService, IMyService, IMyService, IMyService, MyServiceBase, IMyService, IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The fifth generic type argument 'Mockolate.Tests.TestHelpers.MyServiceBase' is no interface.");
		}

		[Fact]
		public async Task With7Arguments_FourthIsClass_ShouldThrow()
		{
			void Act()
			{
				_ =
					new MyMock<IMyService, IMyService, IMyService, MyServiceBase, IMyService, IMyService, IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The fourth generic type argument 'Mockolate.Tests.TestHelpers.MyServiceBase' is no interface.");
		}

		[Fact]
		public async Task With7Arguments_OnlyFirstArgumentIsClass_ShouldForwardBehaviorToBaseClass()
		{
			MockBehavior behavior = new()
			{
				ThrowWhenNotSetup = true
			};

			MyMock<MyServiceBase, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService> sut =
				new(
					behavior: behavior);

			await That(((IMock)sut).Behavior).IsSameAs(behavior);
			await That(sut.Verify).IsNotNull();
		}

		[Fact]
		public async Task With7Arguments_SecondIsClass_ShouldThrow()
		{
			void Act()
			{
				_ =
					new MyMock<IMyService, MyServiceBase, IMyService, IMyService, IMyService, IMyService, IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The second generic type argument 'Mockolate.Tests.TestHelpers.MyServiceBase' is no interface.");
		}

		[Fact]
		public async Task With7Arguments_SeventhIsClass_ShouldThrow()
		{
			void Act()
			{
				_ =
					new MyMock<IMyService, IMyService, IMyService, IMyService, IMyService, IMyService, MyServiceBase>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The seventh generic type argument 'Mockolate.Tests.TestHelpers.MyServiceBase' is no interface.");
		}

		[Fact]
		public async Task With7Arguments_SixthIsClass_ShouldThrow()
		{
			void Act()
			{
				_ =
					new MyMock<IMyService, IMyService, IMyService, IMyService, IMyService, MyServiceBase, IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The sixth generic type argument 'Mockolate.Tests.TestHelpers.MyServiceBase' is no interface.");
		}

		[Fact]
		public async Task With7Arguments_ThirdIsClass_ShouldThrow()
		{
			void Act()
			{
				_ =
					new MyMock<IMyService, IMyService, MyServiceBase, IMyService, IMyService, IMyService, IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The third generic type argument 'Mockolate.Tests.TestHelpers.MyServiceBase' is no interface.");
		}

		[Fact]
		public async Task With8Arguments_EighthIsClass_ShouldThrow()
		{
			void Act()
			{
				_ =
					new MyMock<IMyService, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService,
						MyServiceBase>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The eighth generic type argument 'Mockolate.Tests.TestHelpers.MyServiceBase' is no interface.");
		}

		[Fact]
		public async Task With8Arguments_FifthIsClass_ShouldThrow()
		{
			void Act()
			{
				_ =
					new MyMock<IMyService, IMyService, IMyService, IMyService, MyServiceBase, IMyService, IMyService,
						IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The fifth generic type argument 'Mockolate.Tests.TestHelpers.MyServiceBase' is no interface.");
		}

		[Fact]
		public async Task With8Arguments_FourthIsClass_ShouldThrow()
		{
			void Act()
			{
				_ =
					new MyMock<IMyService, IMyService, IMyService, MyServiceBase, IMyService, IMyService, IMyService,
						IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The fourth generic type argument 'Mockolate.Tests.TestHelpers.MyServiceBase' is no interface.");
		}

		[Fact]
		public async Task With8Arguments_OnlyFirstArgumentIsClass_ShouldForwardBehaviorToBaseClass()
		{
			MockBehavior behavior = new()
			{
				ThrowWhenNotSetup = true
			};

			MyMock<MyServiceBase, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService>
				sut =
					new(
						behavior: behavior);

			await That(((IMock)sut).Behavior).IsSameAs(behavior);
			await That(sut.Verify).IsNotNull();
		}

		[Fact]
		public async Task With8Arguments_SecondIsClass_ShouldThrow()
		{
			void Act()
			{
				_ =
					new MyMock<IMyService, MyServiceBase, IMyService, IMyService, IMyService, IMyService, IMyService,
						IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The second generic type argument 'Mockolate.Tests.TestHelpers.MyServiceBase' is no interface.");
		}

		[Fact]
		public async Task With8Arguments_SeventhIsClass_ShouldThrow()
		{
			void Act()
			{
				_ =
					new MyMock<IMyService, IMyService, IMyService, IMyService, IMyService, IMyService, MyServiceBase,
						IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The seventh generic type argument 'Mockolate.Tests.TestHelpers.MyServiceBase' is no interface.");
		}

		[Fact]
		public async Task With8Arguments_SixthIsClass_ShouldThrow()
		{
			void Act()
			{
				_ =
					new MyMock<IMyService, IMyService, IMyService, IMyService, IMyService, MyServiceBase, IMyService,
						IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The sixth generic type argument 'Mockolate.Tests.TestHelpers.MyServiceBase' is no interface.");
		}

		[Fact]
		public async Task With8Arguments_ThirdIsClass_ShouldThrow()
		{
			void Act()
			{
				_ =
					new MyMock<IMyService, IMyService, MyServiceBase, IMyService, IMyService, IMyService, IMyService,
						IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The third generic type argument 'Mockolate.Tests.TestHelpers.MyServiceBase' is no interface.");
		}

		[Fact]
		public async Task With9Arguments_EighthIsClass_ShouldThrow()
		{
			void Act()
			{
				_ =
					new MyMock<IMyService, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService,
						MyServiceBase, IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The eighth generic type argument 'Mockolate.Tests.TestHelpers.MyServiceBase' is no interface.");
		}

		[Fact]
		public async Task With9Arguments_FifthIsClass_ShouldThrow()
		{
			void Act()
			{
				_ =
					new MyMock<IMyService, IMyService, IMyService, IMyService, MyServiceBase, IMyService, IMyService,
						IMyService, IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The fifth generic type argument 'Mockolate.Tests.TestHelpers.MyServiceBase' is no interface.");
		}

		[Fact]
		public async Task With9Arguments_FourthIsClass_ShouldThrow()
		{
			void Act()
			{
				_ =
					new MyMock<IMyService, IMyService, IMyService, MyServiceBase, IMyService, IMyService, IMyService,
						IMyService, IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The fourth generic type argument 'Mockolate.Tests.TestHelpers.MyServiceBase' is no interface.");
		}

		[Fact]
		public async Task With9Arguments_NinthIsClass_ShouldThrow()
		{
			void Act()
			{
				_ =
					new MyMock<IMyService, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService,
						IMyService, MyServiceBase>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The ninth generic type argument 'Mockolate.Tests.TestHelpers.MyServiceBase' is no interface.");
		}

		[Fact]
		public async Task With9Arguments_OnlyFirstArgumentIsClass_ShouldForwardBehaviorToBaseClass()
		{
			MockBehavior behavior = new()
			{
				ThrowWhenNotSetup = true
			};

			MyMock<MyServiceBase, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService,
				IMyService> sut = new(behavior: behavior);

			await That(((IMock)sut).Behavior).IsSameAs(behavior);
			await That(sut.Verify).IsNotNull();
		}

		[Fact]
		public async Task With9Arguments_SecondIsClass_ShouldThrow()
		{
			void Act()
			{
				_ =
					new MyMock<IMyService, MyServiceBase, IMyService, IMyService, IMyService, IMyService, IMyService,
						IMyService, IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The second generic type argument 'Mockolate.Tests.TestHelpers.MyServiceBase' is no interface.");
		}

		[Fact]
		public async Task With9Arguments_SeventhIsClass_ShouldThrow()
		{
			void Act()
			{
				_ =
					new MyMock<IMyService, IMyService, IMyService, IMyService, IMyService, IMyService, MyServiceBase,
						IMyService, IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The seventh generic type argument 'Mockolate.Tests.TestHelpers.MyServiceBase' is no interface.");
		}

		[Fact]
		public async Task With9Arguments_SixthIsClass_ShouldThrow()
		{
			void Act()
			{
				_ =
					new MyMock<IMyService, IMyService, IMyService, IMyService, IMyService, MyServiceBase, IMyService,
						IMyService, IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The sixth generic type argument 'Mockolate.Tests.TestHelpers.MyServiceBase' is no interface.");
		}

		[Fact]
		public async Task With9Arguments_ThirdIsClass_ShouldThrow()
		{
			void Act()
			{
				_ =
					new MyMock<IMyService, IMyService, MyServiceBase, IMyService, IMyService, IMyService, IMyService,
						IMyService, IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The third generic type argument 'Mockolate.Tests.TestHelpers.MyServiceBase' is no interface.");
		}
		*/
	}
}
