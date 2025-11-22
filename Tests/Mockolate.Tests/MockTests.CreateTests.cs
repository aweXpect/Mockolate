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
				ThrowWhenNotSetup = true,
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
		public async Task With2Arguments_WithConstructorParametersAndSetups_ShouldApplySetups()
		{
			MyBaseClassWithConstructor mock = Mock.Create<MyBaseClassWithConstructor, IMyService>(
				BaseClass.WithConstructorParameters("foo"),
				setup => setup.Method.VirtualMethod().Do(_ => setup.Method.VirtualMethod().Returns("foo"))
					.Returns("bar"));

			string result1 = mock.VirtualMethod();
			string result2 = mock.VirtualMethod();

			await That(result1).IsEqualTo("bar");
			await That(result2).IsEqualTo("foo");
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
				setup => setup.Method.Multiply(With(1), Any<int?>()).Returns(2),
				setup => setup.Method.Multiply(With(2), Any<int?>()).Returns(4),
				setup => setup.Method.Multiply(With(3), Any<int?>()).Returns(8));

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
			MockBehavior behavior = new()
			{
				ThrowWhenNotSetup = true,
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
				ThrowWhenNotSetup = true,
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
				ThrowWhenNotSetup = true,
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
				ThrowWhenNotSetup = true,
			};

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
			MockBehavior behavior = new()
			{
				ThrowWhenNotSetup = true,
			};

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
			MockBehavior behavior = new()
			{
				ThrowWhenNotSetup = true,
			};

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
			MockBehavior behavior = new()
			{
				ThrowWhenNotSetup = true,
			};

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
	}
}
