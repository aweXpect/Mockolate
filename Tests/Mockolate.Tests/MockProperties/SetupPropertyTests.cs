using Mockolate.Exceptions;
using Mockolate.Setup;

namespace Mockolate.Tests.MockProperties;

public sealed partial class SetupPropertyTests
{
	[Fact]
	public async Task InvokeGetter_InvalidType_ShouldThrowMockException()
	{
		MyPropertySetup<int> setup = new();

		void Act()
		{
			setup.InvokeGetter<string>();
		}

		await That(Act).Throws<MockException>()
			.WithMessage("""
			             The property only supports 'int' and not 'string'.
			             """);
	}

	[Fact]
	public async Task InvokeSetter_InvalidType_ShouldThrowMockException()
	{
		MyPropertySetup<int> setup = new();

		void Act()
		{
			setup.InvokeSetter("foo");
		}

		await That(Act).Throws<MockException>()
			.WithMessage("""
			             The property value only supports 'int', but is 'string'.
			             """);
	}

	[Fact]
	public async Task Register_AfterInvocation_ShouldBeAppliedForFutureUse()
	{
		IPropertyService mock = Mock.Create<IPropertyService>();
		MockRegistration registration = ((IHasMockRegistration)mock).Registrations;

		int result0 = registration.GetProperty("my.other.property", () => 0, null);
		PropertySetup<int> setup = new("my.property");
		((IInteractivePropertySetup)setup).InitializeWith(42);
		registration.SetupProperty(setup);
		int result1 = registration.GetProperty("my.property", () => 0, null);

		await That(result0).IsEqualTo(0);
		await That(result1).IsEqualTo(42);
	}

	[Fact]
	public async Task Register_MultipleProperties_ShouldAllStoreValues()
	{
		IPropertyService mock = Mock.Create<IPropertyService>();

		mock.MyProperty = 1;
		mock.MyOtherProperty = 2;

		int myResult1 = mock.MyProperty;
		int myOtherResult1 = mock.MyOtherProperty;

		mock.MyProperty = 10;
		mock.MyOtherProperty = 20;

		int myResult2 = mock.MyProperty;
		int myOtherResult2 = mock.MyOtherProperty;

		await That(myResult1).IsEqualTo(1);
		await That(myOtherResult1).IsEqualTo(2);
		await That(myResult2).IsEqualTo(10);
		await That(myOtherResult2).IsEqualTo(20);
	}

	[Fact]
	public async Task Register_SamePropertyTwice_ShouldOverwritePreviousSetup()
	{
		IPropertyService mock = Mock.Create<IPropertyService>();
		mock.SetupMock.Property.MyProperty.InitializeWith(4);

		int result1 = mock.MyProperty;
		mock.MyProperty = 5;
		int result2 = mock.MyProperty;

		mock.SetupMock.Property.MyProperty.Returns(6);

		int result3 = mock.MyProperty;
		mock.MyProperty = 7;
		int result4 = mock.MyProperty;

		await That(result1).IsEqualTo(4);
		await That(result2).IsEqualTo(5);
		await That(result3).IsEqualTo(6);
		await That(result4).IsEqualTo(6);
	}

	[Fact]
	public async Task ShouldStoreLastValue()
	{
		IPropertyService mock = Mock.Create<IPropertyService>();
		MockRegistration registration = ((IHasMockRegistration)mock).Registrations;

		string result0 = registration.GetProperty<string>("my.property", () => "", null);
		registration.SetProperty("my.property", "foo");
		string result1 = registration.GetProperty<string>("my.property", () => "", null);
		string result2 = registration.GetProperty<string>("my.other.property", () => "", null);

		await That(result0).IsEmpty();
		await That(result1).IsEqualTo("foo");
		await That(result2).IsEmpty();
	}

	[Fact]
	public async Task ToString_ShouldReturnType()
	{
		PropertySetup<int> setup = new("Foo");

		string result = setup.ToString();

		await That(result).IsEqualTo("int Foo");
	}

	[Fact]
	public async Task WhenMockInheritsPropertyMultipleTimes()
	{
		IMyPropertyService mock =
			Mock.Create<IMyPropertyService, IMyPropertyServiceBase1>();
		mock.SetupMock.Property.Value.InitializeWith("Hello");

		string result = mock.Value;

		await That(mock.VerifyMock.Got.Value()).Once();
		await That(result).IsEqualTo("Hello");
	}

	public interface IMyPropertyService : IMyPropertyServiceBase1
	{
		new string Value { get; set; }
	}

	public interface IMyPropertyServiceBase1
	{
		int Value { get; set; }
	}

	public interface IPropertyService
	{
		int MyProperty { get; set; }
		int MyOtherProperty { get; set; }
		string? MyStringProperty { get; set; }
	}

	private sealed class MyPropertySetup<T>() : PropertySetup<T>("My.Property")
	{
		public void InvokeSetter(object? value)
			=> InvokeSetter(value, MockBehavior.Default);

		public TResult InvokeGetter<TResult>()
			=> InvokeGetter<TResult>(MockBehavior.Default, () => default!);
	}
}
