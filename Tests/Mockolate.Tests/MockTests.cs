using Mockolate.Exceptions;
using Mockolate.Tests.TestHelpers;
using static Mockolate.BaseClass;

namespace Mockolate.Tests;

public sealed partial class MockTests
{
	[Fact]
	public async Task DoubleNestedInterfaces_ShouldStillWork()
	{
		var mock = Mock.Create<Nested.Nested2.IMyDoubleNestedService>();
		mock.Setup.IsValid.InitializeWith(true);

		var result = mock.Object.IsValid;

		await That(result).IsTrue();
	}

	[Fact]
	public async Task Behavior_ShouldBeSet()
	{
		MyMock<string> sut = new("", MockBehavior.Default with
		{
			ThrowWhenNotSetup = true,
		});

		await That(sut.Hidden.Behavior.ThrowWhenNotSetup).IsTrue();
	}

	[Fact]
	public async Task Create_WithRequiredParameters_WithEmptyParameters_ShouldThrowMockException()
	{
		void Act()
			=> _ = Mock.Create<MyBaseClassWithConstructor>(WithConstructorParameters());

		await That(Act).Throws<MockException>()
			.WithMessage(
				"No parameterless constructor found for 'MockTests.MyBaseClassWithConstructor'. Please provide constructor parameters.");
	}

	[Fact]
	public async Task Create_WithRequiredParameters_WithoutParameters_ShouldThrowMockException()
	{
		void Act()
			=> _ = Mock.Create<MyBaseClassWithConstructor>();

		await That(Act).Throws<MockException>()
			.WithMessage(
				"No parameterless constructor found for 'MockTests.MyBaseClassWithConstructor'. Please provide constructor parameters.");
	}

	[Fact]
	public async Task Create_WithTooManyParameters_ShouldThrowMockException()
	{
		void Act()
			=> _ = Mock.Create<MyBaseClassWithConstructor>(WithConstructorParameters("foo", 1, 2));

		await That(Act).Throws<MockException>()
			.WithMessage(
				"Could not find any constructor for 'MockTests.MyBaseClassWithConstructor' that matches the 3 given parameters (foo, 1, 2).");
	}

	[Fact]
	public async Task Create_BaseClassWithoutConstructor_ShouldThrowMockException()
	{
		void Act()
			=> _ = Mock.Create<MyBaseClassWithoutConstructor>();

		await That(Act).Throws<MockException>()
			.WithMessage(
				"Could not find any constructor at all for the base type 'MockTests.MyBaseClassWithoutConstructor'. Therefore mocking is not supported!");
	}

	[Fact]
	public async Task Create_WithMatchingParameters_ShouldCreateMock()
	{
		Mock<MyBaseClassWithConstructor> Act()
			=> _ = Mock.Create<MyBaseClassWithConstructor>(WithConstructorParameters("foo"));

		await That(Act).DoesNotThrow().AndWhoseResult.IsNotNull();
	}

	[Fact]
	public async Task ShouldSupportImplicitOperator()
	{
		MyMock<string> sut = new("foo");

		string value = sut;

		await That(value).IsEqualTo("foo");
	}

	[Fact]
	public async Task TryCast_WhenNotMatching_ShouldReturnFalse()
	{
		var mock = new MyMock<MyBaseClass>(new MyBaseClass());
		object parameter = "foo";

		var result = mock.HiddenTryCast<int>(parameter, out int value);

		await That(result).IsFalse();
		await That(value).IsEqualTo(0);
	}

	[Fact]
	public async Task TryCast_WhenNullShouldReturnTrue()
	{
		var mock = new MyMock<MyBaseClass>(new MyBaseClass());
		object? parameter = null;

		var result = mock.HiddenTryCast<int>(parameter, out int value);

		await That(result).IsTrue();
		await That(value).IsEqualTo(0);
	}

	[Fact]
	public async Task TryCast_WhenMatching_ShouldReturnTrue()
	{
		var mock = new MyMock<MyBaseClass>(new MyBaseClass());
		object parameter = 42;

		var result = mock.HiddenTryCast<int>(parameter, out int value);

		await That(result).IsTrue();
		await That(value).IsEqualTo(42);
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
		{ }

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

}
