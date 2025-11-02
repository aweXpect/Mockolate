using Mockolate.Exceptions;
using Mockolate.Setup;

namespace Mockolate.Tests.MockProperties;

public sealed class SetupPropertyTests
{
	[Fact]
	public async Task ToString_ShouldReturnType()
	{
		var setup = new PropertySetup<int>();

		var result = setup.ToString();

		await That(result).IsEqualTo("int");
	}

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
	public async Task MixReturnsAndThrows_ShouldIterateThroughBoth()
	{
		Mock<IPropertyService> sut = Mock.Create<IPropertyService>();

		sut.Setup.Property.MyProperty
			.Returns(4)
			.Throws(new Exception("foo"))
			.Returns(() => 2);

		int result1 = sut.Subject.MyProperty;
		Exception? result2 = Record.Exception(() => _ = sut.Subject.MyProperty);
		int result3 = sut.Subject.MyProperty;

		await That(result1).IsEqualTo(4);
		await That(result2).HasMessage("foo");
		await That(result3).IsEqualTo(2);
	}

	[Fact]
	public async Task MultipleOnGet_ShouldAllGetInvoked()
	{
		int callCount1 = 0;
		int callCount2 = 0;
		Mock<IPropertyService> sut = Mock.Create<IPropertyService>();

		sut.Setup.Property.MyProperty
			.OnGet(() => { callCount1++; })
			.OnGet(v => { callCount2 += v; });

		sut.Subject.MyProperty = 1;
		_ = sut.Subject.MyProperty;
		sut.Subject.MyProperty = 2;
		_ = sut.Subject.MyProperty;

		await That(callCount1).IsEqualTo(2);
		await That(callCount2).IsEqualTo(3);
	}

	[Fact]
	public async Task MultipleOnSet_ShouldAllGetInvoked()
	{
		int callCount1 = 0;
		int callCount2 = 0;
		Mock<IPropertyService> sut = Mock.Create<IPropertyService>();

		sut.Setup.Property.MyProperty
			.InitializeWith(2)
			.OnSet(() => { callCount1++; })
			.OnSet((old, @new) => { callCount2 += old * @new; });

		sut.Subject.MyProperty = 4; // 2 * 4 = 8
		sut.Subject.MyProperty = 6; // 4 * 6 = 24

		await That(callCount1).IsEqualTo(2);
		await That(callCount2).IsEqualTo(8 + 24);
	}

	[Fact]
	public async Task MultipleReturns_ShouldIterateThroughAllRegisteredValues()
	{
		Mock<IPropertyService> sut = Mock.Create<IPropertyService>();

		sut.Setup.Property.MyProperty
			.Returns(4)
			.Returns(() => 3)
			.Returns(v => 10 * v);

		int[] result = new int[10];
		for (int i = 0; i < 10; i++)
		{
			result[i] = sut.Subject.MyProperty;
		}

		await That(result).IsEqualTo([4, 3, 30, 4, 3, 30, 4, 3, 30, 4,]);
	}

	[Fact]
	public async Task OnGet_ShouldExecuteWhenPropertyIsRead()
	{
		int callCount = 0;
		Mock<IPropertyService> sut = Mock.Create<IPropertyService>();

		sut.Setup.Property.MyProperty
			.OnGet(() => { callCount++; });

		_ = sut.Subject.MyProperty;

		await That(callCount).IsEqualTo(1);
	}

	[Fact]
	public async Task OnGet_ShouldNotExecuteWhenPropertyIsWrittenToOrOtherPropertyIsRead()
	{
		int callCount = 0;
		Mock<IPropertyService> sut = Mock.Create<IPropertyService>();

		sut.Setup.Property.MyProperty
			.OnGet(() => { callCount++; });

		_ = sut.Subject.MyOtherProperty;
		sut.Subject.MyProperty = 1;

		await That(callCount).IsEqualTo(0);
	}

	[Fact]
	public async Task OnGet_WithValue_ShouldExecuteWhenPropertyIsRead()
	{
		int callCount = 0;
		int receivedValue = 0;
		Mock<IPropertyService> sut = Mock.Create<IPropertyService>();

		sut.Setup.Property.MyProperty
			.InitializeWith(4)
			.OnGet(v =>
			{
				callCount++;
				receivedValue = v;
			});

		_ = sut.Subject.MyProperty;

		await That(callCount).IsEqualTo(1);
		await That(receivedValue).IsEqualTo(4);
	}

	[Fact]
	public async Task OnGet_WithValue_ShouldNotExecuteWhenPropertyIsWrittenToOrOtherPropertyIsRead()
	{
		int callCount = 0;
		Mock<IPropertyService> sut = Mock.Create<IPropertyService>();

		sut.Setup.Property.MyProperty
			.OnGet(_ => { callCount++; });

		_ = sut.Subject.MyOtherProperty;
		sut.Subject.MyProperty = 1;

		await That(callCount).IsEqualTo(0);
	}

	[Fact]
	public async Task OnSet_ShouldExecuteWhenPropertyIsWrittenTo()
	{
		int callCount = 0;
		Mock<IPropertyService> sut = Mock.Create<IPropertyService>();

		sut.Setup.Property.MyProperty
			.OnSet(() => { callCount++; });

		sut.Subject.MyProperty = 5;

		await That(callCount).IsEqualTo(1);
	}

	[Fact]
	public async Task OnSet_ShouldNotExecuteWhenPropertyIsReadOrOtherPropertyIsWrittenTo()
	{
		int callCount = 0;
		Mock<IPropertyService> sut = Mock.Create<IPropertyService>();

		sut.Setup.Property.MyProperty
			.OnSet(() => { callCount++; });

		sut.Subject.MyOtherProperty = 1;
		_ = sut.Subject.MyProperty;

		await That(callCount).IsEqualTo(0);
	}

	[Fact]
	public async Task OnSet_WithValue_ShouldExecuteWhenPropertyIsWrittenTo()
	{
		int receivedOldValue = 0;
		int receivedNewValue = 0;
		int callCount = 0;
		Mock<IPropertyService> sut = Mock.Create<IPropertyService>();

		sut.Setup.Property.MyProperty
			.InitializeWith(4)
			.OnSet((oldValue, newValue) =>
			{
				callCount++;
				receivedOldValue = oldValue;
				receivedNewValue = newValue;
			});

		sut.Subject.MyProperty = 6;

		await That(callCount).IsEqualTo(1);
		await That(receivedOldValue).IsEqualTo(4);
		await That(receivedNewValue).IsEqualTo(6);
	}

	[Fact]
	public async Task OnSet_WithValue_ShouldNotExecuteWhenPropertyIsReadOrOtherPropertyIsWrittenTo()
	{
		int callCount = 0;
		Mock<IPropertyService> sut = Mock.Create<IPropertyService>();

		sut.Setup.Property.MyProperty
			.OnSet((_, _) => { callCount++; });

		sut.Subject.MyOtherProperty = 1;
		_ = sut.Subject.MyProperty;

		await That(callCount).IsEqualTo(0);
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
	public async Task Returns_Callback_ShouldReturnExpectedValue()
	{
		Mock<IPropertyService> sut = Mock.Create<IPropertyService>();

		sut.Setup.Property.MyProperty
			.Returns(() => 4);

		int result = sut.Subject.MyProperty;

		await That(result).IsEqualTo(4);
	}

	[Fact]
	public async Task Returns_CallbackWithValue_ShouldReturnExpectedValue()
	{
		Mock<IPropertyService> sut = Mock.Create<IPropertyService>();

		sut.Setup.Property.MyProperty
			.InitializeWith(3)
			.Returns(x => 4 * x);

		int result = sut.Subject.MyProperty;

		await That(result).IsEqualTo(12);
	}

	[Fact]
	public async Task Returns_ShouldReturnExpectedValue()
	{
		Mock<IPropertyService> sut = Mock.Create<IPropertyService>();

		sut.Setup.Property.MyProperty
			.Returns(4);

		int result = sut.Subject.MyProperty;

		await That(result).IsEqualTo(4);
	}

	[Fact]
	public async Task Returns_WithoutSetup_ShouldReturnDefault()
	{
		Mock<IPropertyService> sut = Mock.Create<IPropertyService>();

		int result = sut.Subject.MyProperty;

		await That(result).IsEqualTo(0);
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
	public async Task Throws_Callback_ShouldReturnExpectedValue()
	{
		Mock<IPropertyService> sut = Mock.Create<IPropertyService>();

		sut.Setup.Property.MyProperty
			.Throws(() => new Exception("foo"));

		void Act()
			=> _ = sut.Subject.MyProperty;

		await That(Act).ThrowsException().WithMessage("foo");
	}

	[Fact]
	public async Task Throws_CallbackWithValue_ShouldReturnExpectedValue()
	{
		Mock<IPropertyService> sut = Mock.Create<IPropertyService>();

		sut.Setup.Property.MyProperty
			.InitializeWith(42)
			.Throws(v => new Exception($"foo-{v}"));

		void Act()
			=> _ = sut.Subject.MyProperty;

		await That(Act).ThrowsException().WithMessage("foo-42");
	}

	[Fact]
	public async Task Throws_ShouldReturnExpectedValue()
	{
		Mock<IPropertyService> sut = Mock.Create<IPropertyService>();

		sut.Setup.Property.MyProperty
			.Throws(new Exception("foo"));

		void Act()
			=> _ = sut.Subject.MyProperty;

		await That(Act).ThrowsException().WithMessage("foo");
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
