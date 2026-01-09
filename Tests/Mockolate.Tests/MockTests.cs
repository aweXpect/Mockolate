using Mockolate.Exceptions;
using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests;

public sealed partial class MockTests
{
	[Fact]
	public async Task Create_BaseClassWithMultipleConstructors()
	{
		void Act()
		{
			_ = Mock.Create<MyServiceBaseWithMultipleConstructors>(BaseClass.WithConstructorParameters(5));
		}

		await That(Act).DoesNotThrow();
	}

	[Fact]
	public async Task Create_BaseClassWithoutConstructor_ShouldThrowMockException()
	{
		void Act()
		{
			_ = Mock.Create<MyBaseClassWithoutConstructor>();
		}

		await That(Act).Throws<MockException>()
			.WithMessage(
				"Could not find any constructor at all for the base type 'Mockolate.Tests.MockTests.MyBaseClassWithoutConstructor'. Therefore mocking is not supported!");
	}

	[Fact]
	public async Task Create_BaseClassWithVirtualCallsInConstructor_AllowExplicitSetup()
	{
		MyServiceBaseWithVirtualCallsInConstructor mock =
			Mock.Create<MyServiceBaseWithVirtualCallsInConstructor>(setup
				=> setup.Method.VirtualMethod().Returns([5, 6,]));

		int value = mock.VirtualProperty;

		await That(mock.VerifyMock.Invoked.VirtualMethod()).Once();
		await That(value).IsEqualTo(5);
	}

	[Fact]
	public async Task
		Create_BaseClassWithVirtualCallsInConstructor_DirectSubjectAccessInCreateSetups_ShouldThrowMockException()
	{
		void Act()
		{
			_ = Mock.Create<MyServiceBaseWithVirtualCallsInConstructor>(setup => setup.Subject.VirtualProperty = 10);
		}

		await That(Act).Throws<MockException>()
			.WithMessage("Subject is not yet available. You can only access the subject in callbacks!");
	}

	[Fact]
	public async Task
		Create_BaseClassWithVirtualCallsInConstructor_WithUseBaseClassAsDefaultValue_ShouldUseBaseClassValuesInConstructor()
	{
		MyServiceBaseWithVirtualCallsInConstructor mock = Mock.Create<MyServiceBaseWithVirtualCallsInConstructor>();

		int value = mock.VirtualProperty;

		await That(mock.VerifyMock.Invoked.VirtualMethod()).Once();
		await That(value).IsEqualTo(1);
	}

	[Fact]
	public async Task Create_SealedClass_ShouldThrowMockException()
	{
		void Act()
		{
#pragma warning disable Mockolate0002
			_ = Mock.Create<MySealedClass>();
#pragma warning restore Mockolate0002
		}

		await That(Act).Throws<MockException>()
			.WithMessage(
				"Unable to mock type 'Mockolate.Tests.MockTests+MySealedClass'. The type is sealed and therefore not mockable.");
	}

	[Fact]
	public async Task Create_SealedClass_With1AdditionalInterface_ShouldThrowMockException()
	{
		void Act()
		{
#pragma warning disable Mockolate0002
			_ = Mock.Create<MySealedClass, IMyService>();
#pragma warning restore Mockolate0002
		}

		await That(Act).Throws<MockException>()
			.WithMessage(
				"Unable to mock type 'Mockolate.Tests.MockTests+MySealedClass'. The type is sealed and therefore not mockable.");
	}

	[Fact]
	public async Task Create_SealedClass_With2AdditionalInterfaces_ShouldThrowMockException()
	{
		void Act()
		{
#pragma warning disable Mockolate0002
			_ = Mock.Create<MySealedClass, IMyService, IMyService>();
#pragma warning restore Mockolate0002
		}

		await That(Act).Throws<MockException>()
			.WithMessage(
				"Unable to mock type 'Mockolate.Tests.MockTests+MySealedClass'. The type is sealed and therefore not mockable.");
	}

	[Fact]
	public async Task Create_SealedClass_With3AdditionalInterfaces_ShouldThrowMockException()
	{
		void Act()
		{
#pragma warning disable Mockolate0002
			_ = Mock.Create<MySealedClass, IMyService, IMyService, IMyService>();
#pragma warning restore Mockolate0002
		}

		await That(Act).Throws<MockException>()
			.WithMessage(
				"Unable to mock type 'Mockolate.Tests.MockTests+MySealedClass'. The type is sealed and therefore not mockable.");
	}

	[Fact]
	public async Task Create_SealedClass_With4AdditionalInterfaces_ShouldThrowMockException()
	{
		void Act()
		{
#pragma warning disable Mockolate0002
			_ = Mock.Create<MySealedClass, IMyService, IMyService, IMyService, IMyService>();
#pragma warning restore Mockolate0002
		}

		await That(Act).Throws<MockException>()
			.WithMessage(
				"Unable to mock type 'Mockolate.Tests.MockTests+MySealedClass'. The type is sealed and therefore not mockable.");
	}

	[Fact]
	public async Task Create_SealedClass_With5AdditionalInterfaces_ShouldThrowMockException()
	{
		void Act()
		{
#pragma warning disable Mockolate0002
			_ = Mock.Create<MySealedClass, IMyService, IMyService, IMyService, IMyService, IMyService>();
#pragma warning restore Mockolate0002
		}

		await That(Act).Throws<MockException>()
			.WithMessage(
				"Unable to mock type 'Mockolate.Tests.MockTests+MySealedClass'. The type is sealed and therefore not mockable.");
	}

	[Fact]
	public async Task Create_SealedClass_With6AdditionalInterfaces_ShouldThrowMockException()
	{
		void Act()
		{
#pragma warning disable Mockolate0002
			_ = Mock.Create<MySealedClass, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService>();
#pragma warning restore Mockolate0002
		}

		await That(Act).Throws<MockException>()
			.WithMessage(
				"Unable to mock type 'Mockolate.Tests.MockTests+MySealedClass'. The type is sealed and therefore not mockable.");
	}

	[Fact]
	public async Task Create_SealedClass_With7AdditionalInterfaces_ShouldThrowMockException()
	{
		void Act()
		{
#pragma warning disable Mockolate0002
			_ = Mock.Create<MySealedClass,
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
		void Act()
		{
#pragma warning disable Mockolate0002
			_ = Mock.Create<MySealedClass,
				IMyService, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService>();
#pragma warning restore Mockolate0002
		}

		await That(Act).Throws<MockException>()
			.WithMessage(
				"Unable to mock type 'Mockolate.Tests.MockTests+MySealedClass'. The type is sealed and therefore not mockable.");
	}

	[Fact]
	public async Task Create_WithConstructorParameters_SealedClass_ShouldThrowMockException()
	{
		void Act()
		{
#pragma warning disable Mockolate0002
			_ = Mock.Create<MySealedClass>(BaseClass.WithConstructorParameters());
#pragma warning restore Mockolate0002
		}

		await That(Act).Throws<MockException>()
			.WithMessage(
				"Unable to mock type 'Mockolate.Tests.MockTests+MySealedClass'. The type is sealed and therefore not mockable.");
	}

	[Fact]
	public async Task Create_WithConstructorParametersAndSetups_ShouldApplySetups()
	{
		MyBaseClassWithConstructor mock = Mock.Create<MyBaseClassWithConstructor>(
			BaseClass.WithConstructorParameters("foo"),
			setup => setup.Method.VirtualMethod().Returns("bar"));

		string result = mock.VirtualMethod();

		await That(result).IsEqualTo("bar");
	}

	[Fact]
	public async Task Create_WithConstructorParametersMockBehaviorAndSetups_ShouldApplySetups()
	{
		MyBaseClassWithConstructor mock = Mock.Create<MyBaseClassWithConstructor>(
			BaseClass.WithConstructorParameters("foo"), MockBehavior.Default,
			setup => setup.Method.VirtualMethod().Returns("bar"));

		string result = mock.VirtualMethod();

		await That(result).IsEqualTo("bar");
	}

	[Fact]
	public async Task Create_WithMatchingParameters_ShouldCreateMock()
	{
		MyBaseClassWithConstructor Act()
		{
			return _ = Mock.Create<MyBaseClassWithConstructor>(BaseClass.WithConstructorParameters("foo"));
		}

		await That(Act).DoesNotThrow().AndWhoseResult.IsNotNull();
	}

	[Fact]
	public async Task Create_WithMockBehavior_SealedClass_ShouldThrowMockException()
	{
		void Act()
		{
#pragma warning disable Mockolate0002
			_ = Mock.Create<MySealedClass>(MockBehavior.Default);
#pragma warning restore Mockolate0002
		}

		await That(Act).Throws<MockException>()
			.WithMessage(
				"Unable to mock type 'Mockolate.Tests.MockTests+MySealedClass'. The type is sealed and therefore not mockable.");
	}

	[Fact]
	public async Task Create_WithRequiredParameters_WithEmptyParameters_ShouldThrowMockException()
	{
		void Act()
		{
			_ = Mock.Create<MyBaseClassWithConstructor>(BaseClass.WithConstructorParameters());
		}

		await That(Act).Throws<MockException>()
			.WithMessage(
				"No parameterless constructor found for 'Mockolate.Tests.MockTests.MyBaseClassWithConstructor'. Please provide constructor parameters.");
	}

	[Fact]
	public async Task Create_WithRequiredParameters_WithoutParameters_ShouldThrowMockException()
	{
		void Act()
		{
			_ = Mock.Create<MyBaseClassWithConstructor>();
		}

		await That(Act).Throws<MockException>()
			.WithMessage(
				"No parameterless constructor found for 'Mockolate.Tests.MockTests.MyBaseClassWithConstructor'. Please provide constructor parameters.");
	}

	[Fact]
	public async Task Create_WithSetups_ShouldAllowChangingTheSetupSubjectInCallback()
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>(setup => setup.Method
			.Dispense(It.IsAny<string>(), It.IsAny<int>())
			.Do((s, i) => setup.Subject[s] -= i));

		mock["Dark"] = 10;
		mock.Dispense("Dark", 3);
		int remaining = mock["Dark"];

		await That(remaining).IsEqualTo(7);
	}

	[Fact]
	public async Task Create_WithSetups_ShouldApplySetups()
	{
		IMyService mock = Mock.Create<IMyService>(
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
	public async Task Create_WithTooManyParameters_ShouldThrowMockException()
	{
		void Act()
		{
			_ = Mock.Create<MyBaseClassWithConstructor>(BaseClass.WithConstructorParameters("foo", 1, 2));
		}

		await That(Act).Throws<MockException>()
			.WithMessage(
				"Could not find any constructor for 'Mockolate.Tests.MockTests.MyBaseClassWithConstructor' that matches the 3 given parameters (foo, 1, 2).");
	}

	[Fact]
	public async Task DoubleNestedInterfaces_ShouldStillWork()
	{
		Nested.Nested2.IMyDoubleNestedService mock = Mock.Create<Nested.Nested2.IMyDoubleNestedService>();
		mock.SetupMock.Property.IsValid.InitializeWith(true);

		bool result = mock.IsValid;

		await That(result).IsTrue();
	}

	[Fact]
	public async Task WithConstructorParameters_ShouldBeAccessibleViaMock()
	{
		MyBaseClassWithConstructor sut = Mock.Create<MyBaseClassWithConstructor>(
			BaseClass.WithConstructorParameters("foo"));

		Mock<MyBaseClassWithConstructor> mock
			= ((IMockSubject<MyBaseClassWithConstructor>)sut).Mock;

		await That(mock.ConstructorParameters).HasCount(1).And.Contains("foo");
	}

	[Fact]
	public async Task WithoutConstructorParameters_MockConstructorParametersShouldBeEmpty()
	{
		IChocolateDispenser sut = Mock.Create<IChocolateDispenser>();

		Mock<IChocolateDispenser> mock = ((IMockSubject<IChocolateDispenser>)sut).Mock;

		await That(mock.ConstructorParameters).IsEmpty();
	}

	public class MyServiceBaseWithVirtualCallsInConstructor
	{
		// ReSharper disable VirtualMemberCallInConstructor
		public MyServiceBaseWithVirtualCallsInConstructor()
		{
			int[] values = VirtualMethod();
			VirtualProperty = values[0];
		}
		// ReSharper restore VirtualMemberCallInConstructor

		public virtual int VirtualProperty { get; set; }

		public virtual int[] VirtualMethod() => [1, 2,];
	}

	public class MyServiceBaseWithMultipleConstructors
	{
		// ReSharper disable once UnusedParameter.Local
		public MyServiceBaseWithMultipleConstructors(int initialValue)
		{
		}

		// ReSharper disable once UnusedParameter.Local
		public MyServiceBaseWithMultipleConstructors(DateTime initialValue)
		{
		}
	}

	public interface IMyService
	{
		bool? IsValid { get; set; }
		int Counter { get; set; }

		int Multiply(int value, int? multiplier);

		int Subtract(int minuend, int? subtrahend);

		int Subtract(int minuend, int? subtrahend, bool flag);

		void SetIsValid(bool isValid, Func<bool>? predicate);
	}

	public class MyBaseClassWithConstructor
	{
		public MyBaseClassWithConstructor(string text)
		{
			Text = text;
		}

		// ReSharper disable once UnassignedGetOnlyAutoProperty
		public int Number { get; }
		public string Text { get; }
		public virtual string VirtualMethod() => Text;
	}

	public class MyBaseClassWithoutConstructor
	{
		private MyBaseClassWithoutConstructor()
		{
		}

		// ReSharper disable once UnassignedGetOnlyAutoProperty
		public int Number { get; }
	}

	public sealed class Nested
	{
		public sealed class Nested2
		{
			public interface IMyDoubleNestedService
			{
				bool IsValid { get; }
			}
		}
	}

	public sealed class MySealedClass
	{
	}
}
