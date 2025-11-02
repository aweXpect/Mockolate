using System.Linq;
using Mockolate.Exceptions;
using Mockolate.Setup;

namespace Mockolate.Tests.MockMethods;

public sealed partial class SetupMethodTests
{
	[Fact]
	public async Task GenericMethod_SetupShouldWork()
	{
		Mock<IMethodService> mock = Mock.Create<IMethodService>();
		mock.Setup.Method.MyGenericMethod<int, string>(0, "foo").Returns(42);

		int result1 = mock.Subject.MyGenericMethod<int, string>(0, "foo");
		int result2 = mock.Subject.MyGenericMethod<long, string>(0, "foo");

		await That(mock.Verify.Invoked.MyGenericMethod<long, string>(With.Any<long>(), With.Any<string>())).Once();
		await That(result1).IsEqualTo(42);
		await That(result2).IsEqualTo(0);
	}

	[Fact]
	public async Task GenericMethods_ShouldConsiderGenericParameter()
	{
		Mock<IMethodService> mock = Mock.Create<IMethodService>();
		mock.Setup.Method.MyGenericMethod<int>().Returns(42);

		int matchingResult = mock.Subject.MyGenericMethod<int>();
		int notMatchingResult = mock.Subject.MyGenericMethod<long>();

		await That(matchingResult).IsEqualTo(42);
		await That(notMatchingResult).IsEqualTo(0);
	}

	[Fact]
	public async Task Register_AfterInvocation_ShouldBeAppliedForFutureUse()
	{
		Mock<IMethodService> mock = Mock.Create<IMethodService>();
		IMockSetup setup = mock.Setup;
		IMock sut = mock;

		MethodSetupResult<int> result0 = sut.Execute<int>("my.method");
		setup.RegisterMethod(new ReturnMethodSetup<int>("my.method").Returns(42));
		MethodSetupResult<int> result1 = sut.Execute<int>("my.method");

		await That(result0.Result).IsEqualTo(0);
		await That(result1.Result).IsEqualTo(42);
	}

	[Fact]
	public async Task ReturnMethod_Callback_ShouldExecuteWhenInvoked()
	{
		int callCount = 0;
		Mock<IReturnMethodSetupWithParametersTest> sut = Mock.Create<IReturnMethodSetupWithParametersTest>();

		sut.Setup.Method.MethodWithoutOtherOverloads(With.AnyParameterCombination())
			.Callback(() => { callCount++; })
			.Returns("foo");

		string result = sut.Subject.MethodWithoutOtherOverloads(1, 2, 3);

		await That(callCount).IsEqualTo(1);
		await That(result).IsEqualTo("foo");
	}

	[Fact]
	public async Task ReturnMethod_Verify_ShouldMatchAnyParameters()
	{
		int callCount = 0;
		Mock<IReturnMethodSetupWithParametersTest> sut = Mock.Create<IReturnMethodSetupWithParametersTest>();

		sut.Setup.Method.MethodWithoutOtherOverloads(With.Any<int>(), With.Any<int>(), With.Any<int>())
			.Callback(() => { callCount++; })
			.Returns("foo");

		string result = sut.Subject.MethodWithoutOtherOverloads(1, 2, 3);

		await That(callCount).IsEqualTo(1);
		await That(result).IsEqualTo("foo");
		await That(sut.Verify.Invoked.MethodWithoutOtherOverloads(With.AnyParameterCombination())).Once();
	}

	[Fact]
	public async Task ReturnMethod_WhenSetupWithNull_ShouldReturnDefaultValue()
	{
		int callCount = 0;
		Mock<IReturnMethodSetupWithParametersTest> sut = Mock.Create<IReturnMethodSetupWithParametersTest>();

		sut.Setup.Method.MethodWithoutOtherOverloads(With.AnyParameterCombination())
			.Callback(() => { callCount++; })
			.Returns((string?)null!);

		string result = sut.Subject.MethodWithoutOtherOverloads(1, 2, 3);

		await That(callCount).IsEqualTo(1);
		await That(result).IsNull();
	}

	[Fact]
	public async Task Setup_ShouldUseNewestMatchingSetup()
	{
		Mock<IMethodService> mock = Mock.Create<IMethodService>();
		mock.Setup.Method.MyIntMethodWithParameters(With.Any<int>(), With.Any<string>()).Returns(10);

		await That(mock.Subject.MyIntMethodWithParameters(1, "")).IsEqualTo(10);

		mock.Setup.Method.MyIntMethodWithParameters(With.Any<int>(), With.Any<string>()).Returns(20);

		await That(mock.Subject.MyIntMethodWithParameters(1, "")).IsEqualTo(20);
	}

	[Fact]
	public async Task VoidMethod_Callback_ShouldExecuteWhenInvoked()
	{
		int callCount = 0;
		Mock<IVoidMethodSetupWithParametersTest> sut = Mock.Create<IVoidMethodSetupWithParametersTest>();

		sut.Setup.Method.MethodWithoutOtherOverloads(With.AnyParameterCombination())
			.Callback(() => { callCount++; });

		sut.Subject.MethodWithoutOtherOverloads(1, 2, 3);

		await That(callCount).IsEqualTo(1);
	}

	[Theory]
	[InlineData("Method0")]
	[InlineData("Method1", 1)]
	[InlineData("Method2", 1, 2)]
	[InlineData("Method3", 1, 2, 3)]
	[InlineData("Method4", 1, 2, 3, 4)]
	[InlineData("Method5", 1, 2, 3, 4, 5)]
	public async Task VoidMethod_GetReturnValue_ShouldThrowMockException(string methodName, params int[] parameters)
	{
		Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

		sut.Setup.Method.Method0();
		sut.Setup.Method.Method1(With.Any<int>());
		sut.Setup.Method.Method2(With.Any<int>(), With.Any<int>());
		sut.Setup.Method.Method3(With.Any<int>(), With.Any<int>(), With.Any<int>());
		sut.Setup.Method.Method4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>());
		sut.Setup.Method.Method5(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>());

		void Act()
			=> ((IMock)sut).Execute<int>(
				$"Mockolate.Tests.MockMethods.SetupMethodTests.IVoidMethodSetupTest.{methodName}",
				parameters.Select(x => (object?)x).ToArray());

		await That(Act).Throws<MockException>()
			.WithMessage("The method setup does not support return values.");
	}

	[Fact]
	public async Task VoidMethod_Verify_ShouldMatchAnyParameters()
	{
		int callCount = 0;
		Mock<IVoidMethodSetupWithParametersTest> sut = Mock.Create<IVoidMethodSetupWithParametersTest>();

		sut.Setup.Method.MethodWithoutOtherOverloads(With.Any<int>(), With.Any<int>(), With.Any<int>())
			.Callback(() => { callCount++; });

		sut.Subject.MethodWithoutOtherOverloads(1, 2, 3);

		await That(callCount).IsEqualTo(1);
		await That(sut.Verify.Invoked.MethodWithoutOtherOverloads(With.AnyParameterCombination())).Once();
	}

	[Fact]
	public async Task WhenNotSetup_ShouldReturnDefault()
	{
		Mock<IMethodService> mock = Mock.Create<IMethodService>();
		IMock sut = mock;

		MethodSetupResult<string> result0 = sut.Execute<string>("my.method");

		await That(result0.Result).IsEmpty();
	}

	public interface IVoidMethodSetupWithParametersTest
	{
		void MethodWithMultipleOverloads(int p1, int p2);
		void MethodWithMultipleOverloads(int p1, bool p2);
		void MethodWithOutParameter(int p1, out int p2);
		void MethodWithRefParameter(int p1, ref int p2);
		void MethodWithoutOtherOverloads(int p1, int p2, int p3);
		void MethodWithSingleParameter(int p1);
	}

	public interface IReturnMethodSetupWithParametersTest
	{
		string MethodWithMultipleOverloads(int p1, int p2);
		string MethodWithMultipleOverloads(int p1, bool p2);
		string MethodWithOutParameter(int p1, out int p2);
		string MethodWithRefParameter(int p1, ref int p2);
		string MethodWithoutOtherOverloads(int p1, int p2, int p3);
		string MethodWithSingleParameter(int p1);
	}

	public interface IMethodService
	{
		void MyVoidMethodWithoutParameters();
		void MyVoidMethodWithParameters(int x, string y);
		int MyIntMethodWithoutParameters();
		int MyIntMethodWithParameters(int x, string y);
		int MyGenericMethod<T1, T2>(T1 x, T2 y) where T1 : struct where T2 : class;
		int MyGenericMethod<T>();
	}
}
