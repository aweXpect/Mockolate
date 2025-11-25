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

		int result0 = registration.GetProperty<int>("my.other.property");
		PropertySetup<int> setup = new();
		registration.SetupProperty("my.property", (IPropertySetup)setup.InitializeWith(42));
		int result1 = registration.GetProperty<int>("my.property");

		await That(result0).IsEqualTo(0);
		await That(result1).IsEqualTo(42);
	}

	[Fact]
	public async Task Register_SamePropertyTwice_ShouldThrowMockException()
	{
		IPropertyService mock = Mock.Create<IPropertyService>();
		MockRegistration registration = ((IHasMockRegistration)mock).Registrations;

		registration.SetupProperty("my.property", new PropertySetup<int>());

		void Act()
		{
			registration.SetupProperty("my.property", new PropertySetup<int>());
		}

		await That(Act).Throws<MockException>()
			.WithMessage("You cannot setup property 'my.property' twice.");

		registration.SetupProperty("my.other.property", new PropertySetup<int>());
	}

	[Fact]
	public async Task ShouldStoreLastValue()
	{
		IPropertyService mock = Mock.Create<IPropertyService>();
		MockRegistration registration = ((IHasMockRegistration)mock).Registrations;

		string result0 = registration.GetProperty<string>("my.property");
		registration.SetProperty("my.property", "foo");
		string result1 = registration.GetProperty<string>("my.property");
		string result2 = registration.GetProperty<string>("my.other.property");

		await That(result0).IsEmpty();
		await That(result1).IsEqualTo("foo");
		await That(result2).IsEmpty();
	}

	[Fact]
	public async Task ToString_ShouldReturnType()
	{
		PropertySetup<int> setup = new();

		string result = setup.ToString();

		await That(result).IsEqualTo("int");
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
		string MyStringProperty { get; set; }
	}

	private sealed class MyPropertySetup<T> : PropertySetup<T>
	{
		public void InvokeSetter(object? value)
			=> InvokeSetter(value, MockBehavior.Default);

		public TResult InvokeGetter<TResult>()
			=> InvokeGetter<TResult>(MockBehavior.Default);
	}
}
