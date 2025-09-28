using Mockerade.Checks;
using Mockerade.Setup;
using Mockerade.Tests.TestHelpers;

namespace Mockerade.Tests;

public class MockTests
{
	[Fact]
	public async Task Execute_MethodWithoutReturnValue_ShouldIncreaseInvocationCountOfRegisteredSetup()
	{
		var sut = new MyMock<string>("foo");
		MethodWithoutReturnValueSetup setup = new("my-method");

		sut.HiddenSetup.RegisterMethod(setup);

		sut.Hidden.Execute("my-method");

		await That(((IMethodSetup)setup).InvocationCount).IsEqualTo(1);
	}

	[Fact]
	public async Task
		Execute_MethodWithReturnValue_ShouldIncreaseInvocationCountOfRegisteredSetupAndReturnRegisteredValue()
	{
		var sut = new MyMock<string>("foo");
		MethodWithReturnValueSetup<int> setup = new("my-method");
		setup.Returns(42);

		sut.HiddenSetup.RegisterMethod(setup);

		int value = sut.Hidden.Execute<int>("my-method").Result;

		await That(((IMethodSetup)setup).InvocationCount).IsEqualTo(1);
		await That(value).IsEqualTo(42);
	}

	[Fact]
	public async Task Get_ShouldIncreaseInvocationCountOfRegisteredSetupAndReturnRegisteredValue()
	{
		var sut = new MyMock<string>("foo");
		PropertySetup<int> setup = new();
		setup.InitializeWith(42);

		sut.HiddenSetup.RegisterProperty("my-property", setup);

		int value = sut.Hidden.Get<int>("my-property");

		await That(setup.GetterInvocationCount).IsEqualTo(1);
		await That(value).IsEqualTo(42);
	}

	[Fact]
	public async Task Invocations_ShouldReturnAllInvocations()
	{
		MyMock<string> sut = new("foo");
		sut.HiddenSetup.RegisterMethod(new MethodWithReturnValueSetup<int>("my-method").Returns(0));

		int value = sut.Hidden.Execute<int>("my-method").Result;
		sut.Hidden.Get<int>("my-get-property");
		sut.Hidden.Set("my-set-property", 42);

		await That(sut.Hidden.Invocations.Invocations).HasCount(3);
		await That(sut.Hidden.Invocations.Invocations).HasItem()
			.Matching<MethodInvocation>(invocation
				=> invocation.Name == "my-method" && invocation.Parameters.Length == 0);
		await That(sut.Hidden.Invocations.Invocations).HasItem()
			.Matching<PropertyGetterInvocation>(invocation => invocation.Name == "my-get-property");
		await That(sut.Hidden.Invocations.Invocations).HasItem()
			.Matching<PropertySetterInvocation>(invocation
				=> invocation.Name == "my-set-property" && Equals(invocation.Value, 42));
	}

	[Fact]
	public async Task RegisterMethod_AfterExecution_ShouldThrowNotSupportedException()
	{
		var sut = new MyMock<string>("foo");
		MethodWithoutReturnValueSetup setup = new("my-method");

		sut.Hidden.Execute("my-method");

		void Act() => sut.HiddenSetup.RegisterMethod(setup);

		await That(Act).Throws<NotSupportedException>()
			.WithMessage("You may not register additional setups after the first usage of the mock");
		await That(((IMethodSetup)setup).InvocationCount).IsEqualTo(0);
	}

	[Fact]
	public async Task RegisterProperty_AfterExecution_ShouldThrowNotSupportedException()
	{
		var sut = new MyMock<string>("foo");
		PropertySetup<int> setup = new();

		sut.Hidden.Execute("my-property");

		void Act() => sut.HiddenSetup.RegisterProperty("my-property", setup);

		await That(Act).Throws<NotSupportedException>()
			.WithMessage("You may not register additional setups after the first usage of the mock");
		await That(setup.GetterInvocationCount).IsEqualTo(0);
		await That(setup.SetterInvocationCount).IsEqualTo(0);
	}

	[Fact]
	public async Task Set_ShouldChangeGetValue()
	{
		var sut = new MyMock<string>("foo");
		PropertySetup<int> setup = new();
		setup.InitializeWith(42);

		sut.HiddenSetup.RegisterProperty("my-property", setup);

		sut.Hidden.Set("my-property", 43);
		int value = sut.Hidden.Get<int>("my-property");

		await That(value).IsEqualTo(43);
	}

	[Fact]
	public async Task Set_ShouldIncreaseInvocationCountOfRegisteredSetupAndReturnRegisteredValue()
	{
		var sut = new MyMock<string>("foo");
		PropertySetup<int> setup = new();

		sut.HiddenSetup.RegisterProperty("my-property", setup);

		sut.Hidden.Set("my-property", 41);

		await That(setup.SetterInvocationCount).IsEqualTo(1);
	}

	[Fact]
	public async Task Set_WithNull_ShouldChangeGetValue()
	{
		var sut = new MyMock<string>("foo");
		PropertySetup<int?> setup = new();
		setup.InitializeWith(42);

		sut.HiddenSetup.RegisterProperty("my-property", setup);

		sut.Hidden.Set("my-property", null);
		int? value = sut.Hidden.Get<int?>("my-property");

		await That(value).IsNull();
	}

	[Fact]
	public async Task Set_WithoutRegisteredSetup_ShouldChangeGetValue()
	{
		var sut = new MyMock<string>("foo");

		sut.Hidden.Set("my-unregistered-property", 43);
		int value = sut.Hidden.Get<int>("my-unregistered-property");

		await That(value).IsEqualTo(43);
	}

	[Fact]
	public async Task ShouldSupportImplicitOperator()
	{
		MyMock<string> sut = new("foo");

		string value = sut;

		await That(value).IsEqualTo("foo");
	}
}
