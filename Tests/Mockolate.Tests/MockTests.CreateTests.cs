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
		public async Task With3Arguments_OnlyFirstArgumentIsClass_ShouldForwardBehaviorToBaseClass()
		{
			MockBehavior behavior = new()
			{
				ThrowWhenNotSetup = true
			};

			MyServiceBase sut = Mock.Create<MyServiceBase, IMyService, IMyService>(behavior);

			await That(((IHasMockRegistration)sut).Registrations.Behavior).IsSameAs(behavior);
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
			MockBehavior behavior = new()
			{
				ThrowWhenNotSetup = true
			};

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
			MockBehavior behavior = new()
			{
				ThrowWhenNotSetup = true
			};

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
			MockBehavior behavior = new()
			{
				ThrowWhenNotSetup = true
			};

			MyServiceBase sut = Mock.Create<MyServiceBase, IMyService, IMyService, IMyService, IMyService, IMyService>(behavior);

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
					Mock.Create<IMyService, IMyService, IMyService, IMyService, MyServiceBase, IMyService, IMyService>();
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
					Mock.Create<IMyService, IMyService, IMyService, MyServiceBase, IMyService, IMyService, IMyService>();
			}

			await That(Act).Throws<MockException>()
				.WithMessage(
					"The mock declaration has 1 additional implementation that is not an interface: Mockolate.Tests.TestHelpers.MyServiceBase");
		}

		[Fact]
		public async Task With7Arguments_OnlyFirstArgumentIsClass_ShouldForwardBehaviorToBaseClass()
		{
			MockBehavior behavior = new()
			{
				ThrowWhenNotSetup = true
			};

			MyServiceBase sut = Mock.Create<MyServiceBase, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService>(behavior);

			await That(((IHasMockRegistration)sut).Registrations.Behavior).IsSameAs(behavior);
		}

		[Fact]
		public async Task With7Arguments_SecondIsClass_ShouldThrow()
		{
			void Act()
			{
				_ =
					Mock.Create<IMyService, MyServiceBase, IMyService, IMyService, IMyService, IMyService, IMyService>();
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
					Mock.Create<IMyService, IMyService, IMyService, IMyService, IMyService, IMyService, MyServiceBase>();
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
					Mock.Create<IMyService, IMyService, IMyService, IMyService, IMyService, MyServiceBase, IMyService>();
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
					Mock.Create<IMyService, IMyService, MyServiceBase, IMyService, IMyService, IMyService, IMyService>();
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
			MockBehavior behavior = new()
			{
				ThrowWhenNotSetup = true
			};

			MyServiceBase sut = Mock.Create<MyServiceBase, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService>(behavior);

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
			MockBehavior behavior = new()
			{
				ThrowWhenNotSetup = true
			};

			MyServiceBase sut = Mock.Create<MyServiceBase, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService>(behavior);

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
	}
}
