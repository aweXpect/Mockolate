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
			=> setup.InvokeGetter<string>();

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
			=> setup.InvokeSetter("foo");

		await That(Act).Throws<MockException>()
			.WithMessage("""
			             The property value only supports 'int', but is 'string'.
			             """);
	}

	[Fact]
	public async Task Register_AfterInvocation_ShouldBeAppliedForFutureUse()
	{
		Mock<IPropertyService> mock = Mock.Create<IPropertyService>();
		IMockSetup setup = mock.Setup;
		IMock sut = mock;

		int result0 = sut.Get<int>("my.other.property");
		setup.RegisterProperty("my.property", new PropertySetup<int>().InitializeWith(42));
		int result1 = sut.Get<int>("my.property");

		await That(result0).IsEqualTo(0);
		await That(result1).IsEqualTo(42);
	}

	[Fact]
	public async Task Register_SamePropertyTwice_ShouldThrowMockException()
	{
		Mock<IPropertyService> mock = Mock.Create<IPropertyService>();
		IMockSetup setup = mock.Setup;

		setup.RegisterProperty("my.property", new PropertySetup<int>());

		void Act()
			=> setup.RegisterProperty("my.property", new PropertySetup<int>());

		await That(Act).Throws<MockException>()
			.WithMessage("You cannot setup property 'my.property' twice.");

		setup.RegisterProperty("my.other.property", new PropertySetup<int>());
	}

	[Fact]
	public async Task ShouldStoreLastValue()
	{
		Mock<IPropertyService> mock = Mock.Create<IPropertyService>();
		IMock sut = mock;

		string result0 = sut.Get<string>("my.property");
		sut.Set("my.property", "foo");
		string result1 = sut.Get<string>("my.property");
		string result2 = sut.Get<string>("my.other.property");

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
		Mock<IMyPropertyService, IMyPropertyServiceBase1> mock =
			Mock.Create<IMyPropertyService, IMyPropertyServiceBase1>();
		mock.Setup.Property.Value.InitializeWith("Hello");

		string result = mock.Subject.Value;

		await That(mock.Verify.Got.Value()).Once();
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
	}

	private sealed class MyPropertySetup<T> : PropertySetup<T>
	{
		public void InvokeSetter(object? value)
			=> base.InvokeSetter(value, MockBehavior.Default);

		public TResult InvokeGetter<TResult>()
			=> base.InvokeGetter<TResult>(MockBehavior.Default);
	}
}
