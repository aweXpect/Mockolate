using Mockolate.Exceptions;
using Mockolate.Tests.TestHelpers;
using static Mockolate.BaseClass;

namespace Mockolate.Tests;

public sealed partial class MockTests
{
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
		var mock = new MyMock<MyBaseClass>(new MyBaseClass());

		void Act()
			=> _ = mock.HiddenCreate<MyBaseClass>(BaseClass.WithConstructorParameters());

		await That(Act).Throws<MockException>()
			.WithMessage("Could not create an instance of 'Mockolate.Tests.MockTests+MyBaseClass' without constructor parameters.");
	}

	[Fact]
	public async Task Create_WithRequiredParameters_WithoutParameters_ShouldThrowMockException()
	{
		var mock = new MyMock<MyBaseClass>(new MyBaseClass());

		void Act()
			=> _ = mock.HiddenCreate<MyBaseClass>();

		await That(Act).Throws<MockException>()
			.WithMessage("Could not create an instance of 'Mockolate.Tests.MockTests+MyBaseClass' without constructor parameters.");
	}

	[Fact]
	public async Task Create_WithTooManyParameters_ShouldThrowMockException()
	{
		var mock = new MyMock<MyBaseClass>(new MyBaseClass());

		void Act()
			=> _ = mock.HiddenCreate<MyBaseClass>(BaseClass.WithConstructorParameters(1, 2, "foo"));

		await That(Act).Throws<MockException>()
			.WithMessage("Could not create an instance of 'Mockolate.Tests.MockTests+MyBaseClass' with 3 parameters (1, 2, foo).");
	}

	[Fact]
	public async Task ShouldSupportImplicitOperator()
	{
		MyMock<string> sut = new("foo");

		string value = sut;

		await That(value).IsEqualTo("foo");
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

		public int Multiply(int value, int multiplier);

		public void SetIsValid(bool isValid, Func<bool> predicate);
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
}
