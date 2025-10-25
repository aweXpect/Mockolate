using Mockolate.Exceptions;
using Mockolate.Tests.TestHelpers;
using static Mockolate.BaseClass;

namespace Mockolate.Tests;

public sealed partial class MockTests
{
	[Fact]
	public async Task Behavior_ShouldBeSet()
	{
		MyMock<MyServiceBase> sut = new(new MyServiceBase(), MockBehavior.Default with
		{
			ThrowWhenNotSetup = true,
		});

		await That(sut.Hidden.Behavior.ThrowWhenNotSetup).IsTrue();
	}

	[Fact]
	public async Task Create_SealedClass_ShouldThrowMockException()
	{
		void Act()
			=> _ = Mock.Create<MySealedClass>();

		await That(Act).Throws<MockException>()
			.WithMessage(
				"The type 'Mockolate.Tests.MockTests+MySealedClass' is sealed and therefore not mockable.");
	}

	[Fact]
	public async Task Create_SealedClass_With1AdditionalInterface_ShouldThrowMockException()
	{
		void Act()
			=> _ = Mock.Create<MySealedClass, IMyService>();

		await That(Act).Throws<MockException>()
			.WithMessage(
				"The type 'Mockolate.Tests.MockTests+MySealedClass' is sealed and therefore not mockable.");
	}

	[Fact]
	public async Task Create_SealedClass_With2AdditionalInterfaces_ShouldThrowMockException()
	{
		void Act()
			=> _ = Mock.Create<MySealedClass, IMyService, IMyService>();

		await That(Act).Throws<MockException>()
			.WithMessage(
				"The type 'Mockolate.Tests.MockTests+MySealedClass' is sealed and therefore not mockable.");
	}

	[Fact]
	public async Task Create_SealedClass_With3AdditionalInterfaces_ShouldThrowMockException()
	{
		void Act()
			=> _ = Mock.Create<MySealedClass, IMyService, IMyService, IMyService>();

		await That(Act).Throws<MockException>()
			.WithMessage(
				"The type 'Mockolate.Tests.MockTests+MySealedClass' is sealed and therefore not mockable.");
	}

	[Fact]
	public async Task Create_SealedClass_With4AdditionalInterfaces_ShouldThrowMockException()
	{
		void Act()
			=> _ = Mock.Create<MySealedClass, IMyService, IMyService, IMyService, IMyService>();

		await That(Act).Throws<MockException>()
			.WithMessage(
				"The type 'Mockolate.Tests.MockTests+MySealedClass' is sealed and therefore not mockable.");
	}

	[Fact]
	public async Task Create_SealedClass_With5AdditionalInterfaces_ShouldThrowMockException()
	{
		void Act()
			=> _ = Mock.Create<MySealedClass, IMyService, IMyService, IMyService, IMyService, IMyService>();

		await That(Act).Throws<MockException>()
			.WithMessage(
				"The type 'Mockolate.Tests.MockTests+MySealedClass' is sealed and therefore not mockable.");
	}

	[Fact]
	public async Task Create_SealedClass_With6AdditionalInterfaces_ShouldThrowMockException()
	{
		void Act()
			=> _ = Mock.Create<MySealedClass, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService>();

		await That(Act).Throws<MockException>()
			.WithMessage(
				"The type 'Mockolate.Tests.MockTests+MySealedClass' is sealed and therefore not mockable.");
	}

	[Fact]
	public async Task Create_SealedClass_With7AdditionalInterfaces_ShouldThrowMockException()
	{
		void Act()
			=> _ = Mock.Create<MySealedClass, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService>();

		await That(Act).Throws<MockException>()
			.WithMessage(
				"The type 'Mockolate.Tests.MockTests+MySealedClass' is sealed and therefore not mockable.");
	}

	[Fact]
	public async Task Create_SealedClass_With8AdditionalInterfaces_ShouldThrowMockException()
	{
		void Act()
			=> _ = Mock.Create<MySealedClass, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService, IMyService>();

		await That(Act).Throws<MockException>()
			.WithMessage(
				"The type 'Mockolate.Tests.MockTests+MySealedClass' is sealed and therefore not mockable.");
	}

	[Fact]
	public async Task Create_WithMockBehavior_SealedClass_ShouldThrowMockException()
	{
		void Act()
			=> _ = Mock.Create<MySealedClass>(MockBehavior.Default);

		await That(Act).Throws<MockException>()
			.WithMessage(
				"The type 'Mockolate.Tests.MockTests+MySealedClass' is sealed and therefore not mockable.");
	}

	[Fact]
	public async Task Create_WithConstructorParameters_SealedClass_ShouldThrowMockException()
	{
		void Act()
			=> _ = Mock.Create<MySealedClass>(BaseClass.WithConstructorParameters());

		await That(Act).Throws<MockException>()
			.WithMessage(
				"The type 'Mockolate.Tests.MockTests+MySealedClass' is sealed and therefore not mockable.");
	}

	[Fact]
	public async Task Create_BaseClassWithoutConstructor_ShouldThrowMockException()
	{
		var mock = Mock.Create<MyBaseClassWithoutConstructor>();

		void Act()
			=> _ = mock.Subject;

		await That(Act).Throws<MockException>()
			.WithMessage(
				"Could not find any constructor at all for the base type 'Mockolate.Tests.MockTests.MyBaseClassWithoutConstructor'. Therefore mocking is not supported!");
	}

	[Fact]
	public async Task Create_WithMatchingParameters_ShouldCreateMock()
	{
		Mock<MyBaseClassWithConstructor> Act()
			=> _ = Mock.Create<MyBaseClassWithConstructor>(WithConstructorParameters("foo"));

		await That(Act).DoesNotThrow().AndWhoseResult.IsNotNull();
	}

	[Fact]
	public async Task Create_WithRequiredParameters_WithEmptyParameters_ShouldThrowMockException()
	{
		var mock = Mock.Create<MyBaseClassWithConstructor>(WithConstructorParameters());

		void Act()
			=> _ = mock.Subject;

		await That(Act).Throws<MockException>()
			.WithMessage(
				"No parameterless constructor found for 'Mockolate.Tests.MockTests.MyBaseClassWithConstructor'. Please provide constructor parameters.");
	}

	[Fact]
	public async Task Create_WithRequiredParameters_WithoutParameters_ShouldThrowMockException()
	{
		var mock = Mock.Create<MyBaseClassWithConstructor>();

		void Act()
			=> _ = mock.Subject;

		await That(Act).Throws<MockException>()
			.WithMessage(
				"No parameterless constructor found for 'Mockolate.Tests.MockTests.MyBaseClassWithConstructor'. Please provide constructor parameters.");
	}

	[Fact]
	public async Task Create_WithTooManyParameters_ShouldThrowMockException()
	{
		var mock = Mock.Create<MyBaseClassWithConstructor>(WithConstructorParameters("foo", 1, 2));

		void Act()
			=> _ = mock.Subject;

		await That(Act).Throws<MockException>()
			.WithMessage(
				"Could not find any constructor for 'Mockolate.Tests.MockTests.MyBaseClassWithConstructor' that matches the 3 given parameters (foo, 1, 2).");
	}

	[Fact]
	public async Task DoubleNestedInterfaces_ShouldStillWork()
	{
		Mock<Nested.Nested2.IMyDoubleNestedService> mock = Mock.Create<Nested.Nested2.IMyDoubleNestedService>();
		mock.Setup.Property.IsValid.InitializeWith(true);

		bool result = mock.Subject.IsValid;

		await That(result).IsTrue();
	}

	[Fact]
	public async Task ShouldSupportImplicitOperator()
	{
		MyMock<string> sut = new("foo");

		string value = sut;

		await That(value).IsEqualTo("foo");
	}

	[Fact]
	public async Task TryCast_WhenMatching_ShouldReturnTrue()
	{
		MyMock<MyBaseClass> mock = new(new MyBaseClass());
		object parameter = 42;

		bool result = mock.HiddenTryCast(parameter, out int value);

		await That(result).IsTrue();
		await That(value).IsEqualTo(42);
	}

	[Fact]
	public async Task TryCast_WhenNotMatching_ShouldReturnFalse()
	{
		MyMock<MyBaseClass> mock = new(new MyBaseClass());
		object parameter = "foo";

		bool result = mock.HiddenTryCast(parameter, out int value);

		await That(result).IsFalse();
		await That(value).IsEqualTo(0);
	}

	[Fact]
	public async Task TryCast_WhenNullShouldReturnTrue()
	{
		MyMock<MyBaseClass> mock = new(new MyBaseClass());
		object? parameter = null;

		bool result = mock.HiddenTryCast(parameter, out int value);

		await That(result).IsTrue();
		await That(value).IsEqualTo(0);
	}

	[Fact]
	public async Task WithTwoGenericArguments_WhenSecondIsNoInterface_ShouldThrowMockException()
	{
		void Act()
			=> _ = new MyMock<IMyService, MyBaseClass>();

		await That(Act).Throws<MockException>()
			.WithMessage("""
			             The second generic type argument 'Mockolate.Tests.MockTests+MyBaseClass' is no interface.
			             """);
	}

	public interface IMyService
	{
		public bool? IsValid { get; set; }
		public int Counter { get; set; }

		public int Multiply(int value, int? multiplier);

		public void SetIsValid(bool isValid, Func<bool>? predicate);
	}

	public class MyBaseClass
	{
		public virtual string VirtualMethod() => "Base";
	}

	public class MyBaseClassWithConstructor
	{
		public MyBaseClassWithConstructor(string text)
		{
			Text = text;
		}

		public int Number { get; }
		public string Text { get; }
		public virtual string VirtualMethod() => Text;
	}

	public class MyBaseClassWithoutConstructor
	{
		private MyBaseClassWithoutConstructor()
		{
		}

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

	public sealed class MySealedClass { }
}
