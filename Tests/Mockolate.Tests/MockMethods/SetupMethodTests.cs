using System.Collections.Generic;
using System.Linq;
using Mockolate.Exceptions;
using Mockolate.Interactions;
using Mockolate.Parameters;
using Mockolate.Setup;
using Mockolate.Tests.TestHelpers;

namespace Mockolate.Tests.MockMethods;

public sealed partial class SetupMethodTests
{
	[Fact]
	public async Task Equals_ShouldWork()
	{
		object obj = new();
		IMethodService mock = Mock.Create<IMethodService>();
		mock.SetupMock.Method.Equals(It.IsAny<object?>()).Returns(true);

		bool result = mock.Equals(obj);

		await That(result).IsEqualTo(true);
	}
	
	[Fact]
	public async Task GenericMethod_SetupShouldWork()
	{
		IMethodService mock = Mock.Create<IMethodService>();
		mock.SetupMock.Method.MyGenericMethod(It.Is(0), It.Is("foo")).Returns(42);

		int result1 = mock.MyGenericMethod(0, "foo");
		int result2 = mock.MyGenericMethod(0L, "foo");

		await That(mock.VerifyMock.Invoked.MyGenericMethod(It.IsAny<long>(), It.IsAny<string>())).Once();
		await That(result1).IsEqualTo(42);
		await That(result2).IsEqualTo(0);
	}

	[Fact]
	public async Task GenericMethods_ShouldConsiderGenericParameter()
	{
		IMethodService mock = Mock.Create<IMethodService>();
		mock.SetupMock.Method.MyGenericMethod<int>().Returns(42);

		int matchingResult = mock.MyGenericMethod<int>();
		int notMatchingResult = mock.MyGenericMethod<long>();

		await That(matchingResult).IsEqualTo(42);
		await That(notMatchingResult).IsEqualTo(0);
	}

	[Fact]
	public async Task GetHashCode_ShouldWork()
	{
		int expectedResult = Guid.NewGuid().GetHashCode();
		IMethodService mock = Mock.Create<IMethodService>();
		mock.SetupMock.Method.GetHashCode().Returns(expectedResult);

		int result = mock.GetHashCode();

		await That(result).IsEqualTo(expectedResult);
	}

	[Fact]
	public async Task HasReturnCalls_ShouldDefaultToFalse()
	{
		MyMethodSetup sut = new();

		bool result = sut.GetHasReturnCalls();

		await That(result).IsFalse();
	}

	[Fact]
	public async Task MultipleOnlyOnceCallbacks_ShouldExecuteInOrder()
	{
		List<int> receivedCalls = [];
		IMethodService sut = Mock.Create<IMethodService>();

		sut.SetupMock.Method.MyVoidMethodWithoutParameters()
			.Do(() => receivedCalls.Add(1)).OnlyOnce()
			.Do(() => receivedCalls.Add(2)).OnlyOnce()
			.Do(() => receivedCalls.Add(3)).OnlyOnce()
			.Do(() => receivedCalls.Add(4)).OnlyOnce()
			.Do(() => receivedCalls.Add(5)).OnlyOnce();

		sut.MyVoidMethodWithoutParameters();
		sut.MyVoidMethodWithoutParameters();
		sut.MyVoidMethodWithoutParameters();
		sut.MyVoidMethodWithoutParameters();
		sut.MyVoidMethodWithoutParameters();

		await That(receivedCalls).IsEqualTo([1, 2, 3, 4, 5,]);
	}

	[Fact]
	public async Task OverlappingSetups_ShouldUseLatestMatchingSetup()
	{
		IMethodService mock = Mock.Create<IMethodService>();
		mock.SetupMock.Method.MyIntMethodWithParameters(It.IsAny<int>(), It.IsAny<string>()).Returns(1);
		mock.SetupMock.Method.MyIntMethodWithParameters(It.Is(0), It.Is("foo")).Returns(2);

		int result1 = mock.MyIntMethodWithParameters(1, "foo");
		int result2 = mock.MyIntMethodWithParameters(0, "foo");
		int result3 = mock.MyIntMethodWithParameters(0, "bar");

		await That(result1).IsEqualTo(1);
		await That(result2).IsEqualTo(2);
		await That(result3).IsEqualTo(1);
	}

	[Fact]
	public async Task OverlappingSetups_WhenGeneralSetupIsLater_ShouldOnlyUseGeneralSetup()
	{
		IMethodService mock = Mock.Create<IMethodService>();
		mock.SetupMock.Method.MyIntMethodWithParameters(It.Is(0), It.Is("foo")).Returns(2);
		mock.SetupMock.Method.MyIntMethodWithParameters(It.IsAny<int>(), It.IsAny<string>()).Returns(1);

		int result1 = mock.MyIntMethodWithParameters(1, "foo");
		int result2 = mock.MyIntMethodWithParameters(0, "foo");
		int result3 = mock.MyIntMethodWithParameters(0, "bar");

		await That(result1).IsEqualTo(1);
		await That(result2).IsEqualTo(1);
		await That(result3).IsEqualTo(1);
	}

	[Fact]
	public async Task Parameter_Do_ShouldExecuteCallback()
	{
		List<int> capturedValues = [];
		IMethodService mock = Mock.Create<IMethodService>();
		mock.SetupMock.Method.MyIntMethodWithParameters(It.IsAny<int>().Do(v => capturedValues.Add(v)),
			It.IsAny<string>());

		mock.MyIntMethodWithParameters(1, "foo");
		mock.MyIntMethodWithParameters(2, "foobar");
		mock.MyIntMethodWithParameters(3, "bar");

		await That(capturedValues).IsEqualTo([1, 2, 3,]);
	}

	[Fact]
	public async Task Parameter_Do_ShouldOnlyExecuteCallbackWhenAllParametersMatch()
	{
		List<int> capturedValues = [];
		IMethodService mock = Mock.Create<IMethodService>();
		mock.SetupMock.Method.MyIntMethodWithParameters(It.IsAny<int>().Do(v => capturedValues.Add(v)),
			It.Satisfies<string>(s => s.Length == 3));

		mock.MyIntMethodWithParameters(1, "foo");
		mock.MyIntMethodWithParameters(2, "foobar");
		mock.MyIntMethodWithParameters(3, "bar");

		await That(capturedValues).IsEqualTo([1, 3,]);
	}

	[Fact]
	public async Task Register_AfterInvocation_ShouldBeAppliedForFutureUse()
	{
		IChocolateDispenser mock = Mock.Create<IChocolateDispenser>();
		MockRegistration registration = ((IHasMockRegistration)mock).Registrations;

		MethodSetupResult<int> result0 = registration.InvokeMethod("my.method", _ => 0);
		ReturnMethodSetup<int> setup = new("my.method");
		setup.Returns(42);
		registration.SetupMethod(setup);
		MethodSetupResult<int> result1 = registration.InvokeMethod("my.method", _ => 0);

		await That(result0.Result).IsEqualTo(0);
		await That(result1.Result).IsEqualTo(42);
	}

	[Fact]
	public async Task ReturnMethod_Callback_ShouldExecuteWhenInvoked()
	{
		int callCount = 0;
		IReturnMethodSetupWithParametersTest sut = Mock.Create<IReturnMethodSetupWithParametersTest>();

		sut.SetupMock.Method.MethodWithoutOtherOverloads(Match.AnyParameters())
			.Do(() => { callCount++; })
			.Returns("foo");

		string result = sut.MethodWithoutOtherOverloads(1, 2, 3);

		await That(callCount).IsEqualTo(1);
		await That(result).IsEqualTo("foo");
	}

	[Fact]
	public async Task ReturnMethod_Verify_ShouldMatchAnyParameters()
	{
		int callCount = 0;
		IReturnMethodSetupWithParametersTest sut = Mock.Create<IReturnMethodSetupWithParametersTest>();

		sut.SetupMock.Method.MethodWithoutOtherOverloads(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
			.Do(() => { callCount++; })
			.Returns("foo");

		string result = sut.MethodWithoutOtherOverloads(1, 2, 3);

		await That(callCount).IsEqualTo(1);
		await That(result).IsEqualTo("foo");
		await That(sut.VerifyMock.Invoked.MethodWithoutOtherOverloads(Match.AnyParameters())).Once();
	}

	[Fact]
	public async Task ReturnMethod_WhenSetupWithNull_ShouldReturnDefaultValue()
	{
		int callCount = 0;
		IReturnMethodSetupWithParametersTest sut = Mock.Create<IReturnMethodSetupWithParametersTest>();

		sut.SetupMock.Method.MethodWithoutOtherOverloads(Match.AnyParameters())
			.Do(() => { callCount++; })
			.Returns((string?)null!);

		string result = sut.MethodWithoutOtherOverloads(1, 2, 3);

		await That(callCount).IsEqualTo(1);
		await That(result).IsNull();
	}

	/* TODO: Re-Enable
	[Fact]
	public async Task ReturnMethodWith17Parameters_ShouldStillAllowCallbackAndReturns()
	{
		int isCalled = 0;
		IMyServiceWithMethodsWithMoreThan16Parameters mock =
			Mock.Create<IMyServiceWithMethodsWithMoreThan16Parameters>();

		mock.SetupMock.Method.ReturnMethod17(
				It.Is(1), It.Is(2), It.Is(3), It.Is(4), It.Is(5), It.Is(6), It.Is(7), It.Is(8),
				It.Is(9), It.Is(10), It.Is(11), It.Is(12), It.Is(13), It.Is(14), It.Is(15), It.Is(16),
				It.Is(17))
			.Do((_, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _)
				=> isCalled++)
			.Returns((p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, p17)
				=> p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9 + p10 + p11 + p12 + p13 + p14 + p15 + p16 + p17);

		int result = mock.ReturnMethod17(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17);

		await That(isCalled).IsEqualTo(1);
		await That(result).IsEqualTo(153);
	}

	[Fact]
	public async Task ReturnMethodWith18Parameters_ShouldStillAllowCallbackAndReturns()
	{
		int isCalled = 0;
		IMyServiceWithMethodsWithMoreThan16Parameters mock =
			Mock.Create<IMyServiceWithMethodsWithMoreThan16Parameters>();

		mock.SetupMock.Method.ReturnMethod18(
				It.Is(1), It.Is(2), It.Is(3), It.Is(4), It.Is(5), It.Is(6), It.Is(7), It.Is(8),
				It.Is(9), It.Is(10), It.Is(11), It.Is(12), It.Is(13), It.Is(14), It.Is(15), It.Is(16),
				It.Is(17), It.Is(18))
			.Do((_, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _)
				=> isCalled++)
			.Returns((p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, p17, p18)
				=> p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9 + p10 + p11 + p12 + p13 + p14 + p15 + p16 + p17 + p18);

		int result = mock.ReturnMethod18(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18);

		await That(isCalled).IsEqualTo(1);
		await That(result).IsEqualTo(171);
	}
	*/

	[Fact]
	public async Task Setup_ShouldUseNewestMatchingSetup()
	{
		IMethodService mock = Mock.Create<IMethodService>();
		mock.SetupMock.Method.MyIntMethodWithParameters(It.IsAny<int>(), It.IsAny<string>()).Returns(10);

		await That(mock.MyIntMethodWithParameters(1, "")).IsEqualTo(10);

		mock.SetupMock.Method.MyIntMethodWithParameters(It.IsAny<int>(), It.IsAny<string>()).Returns(20);

		await That(mock.MyIntMethodWithParameters(1, "")).IsEqualTo(20);
	}
	[Fact]
	public async Task Setup_WithOutParameter_ShouldUseCallbackToSetValue()
	{
		IMethodService mock = Mock.Create<IMethodService>();
		mock.SetupMock.Method.MyMethodWithOutParameter(It.IsOut(() => 4));

		mock.MyMethodWithOutParameter(out int value);

		await That(value).IsEqualTo(4);
	}

	/* TODO: Re-Enable
	[Fact]
	public async Task Setup_WithOutParameterWithoutCallback_ShouldUseDefaultValueSetValue()
	{
		IMethodService mock = Mock.Create<IMethodService>();
		mock.SetupMock.Method.MyMethodWithOutParameter(It.IsAnyOut<int>());

		mock.MyMethodWithOutParameter(out int value);

		await That(value).IsEqualTo(0);
	}

	[Fact]
	public async Task Setup_WithRefParameter_WithCallback_ShouldUseCallbackToSetValue()
	{
		IMethodService mock = Mock.Create<IMethodService>();
		mock.SetupMock.Method.MyMethodWithRefParameter(It.IsRef<int>(_ => 4));
		int value = 2;

		mock.MyMethodWithRefParameter(ref value);

		await That(value).IsEqualTo(4);
	}

	[Fact]
	public async Task Setup_WithRefParameter_WithoutPredicateOrCallback_ShouldNotChangeValue()
	{
		IMethodService mock = Mock.Create<IMethodService>();
		mock.SetupMock.Method.MyMethodWithRefParameter(It.IsAnyRef<int>());
		int value = 2;

		mock.MyMethodWithRefParameter(ref value);

		await That(value).IsEqualTo(2);
	}

	[Fact]
	public async Task Setup_WithRefParameter_WithPredicate_ShouldUseCallbackToSetValue()
	{
		IMethodService mock = Mock.Create<IMethodService>();
		mock.SetupMock.Method.MyMethodWithRefParameter(It.IsRef<int>(v => v > 2));
		int value = 2;

		mock.MyMethodWithRefParameter(ref value);

		await That(value).IsEqualTo(2);
	}

	[Fact]
	public async Task Setup_WithRefParameter_WithPredicateAndCallback_ShouldUseCallbackToSetValueWhenPredicateMatches()
	{
		IMethodService mock = Mock.Create<IMethodService>();
		mock.SetupMock.Method.MyMethodWithRefParameter(It.IsRef<int>(v => v > 2, _ => 4));
		int value1 = 2;
		int value2 = 3;

		mock.MyMethodWithRefParameter(ref value1);
		mock.MyMethodWithRefParameter(ref value2);

		await That(value1).IsEqualTo(2);
		await That(value2).IsEqualTo(4);
	}

	[Fact]
	public async Task ToString_OutParameter_ShouldReturnExpectedValue()
	{
		IVerifyOutParameter<int> sut = It.IsOut<int>();
		string expectedResult = "It.IsOut<int>()";

		string? result = sut.ToString();

		await That(result).IsEqualTo(expectedResult);
	}

	[Fact]
	public async Task ToString_OutParameter_WithCallback_ShouldReturnExpectedValue()
	{
		IOutParameter<int> sut = It.IsOut(() => 4);
		string expectedResult = "It.IsOut<int>(() => 4)";

		string? result = sut.ToString();

		await That(result).IsEqualTo(expectedResult);
	}

	[Fact]
	public async Task ToString_RefParameter_ShouldReturnExpectedValue()
	{
		IVerifyRefParameter<int> sut = It.IsRef<int>();
		string expectedResult = "It.IsRef<int>()";

		string? result = sut.ToString();

		await That(result).IsEqualTo(expectedResult);
	}

	[Fact]
	public async Task ToString_RefParameter_WithCallback_ShouldReturnExpectedValue()
	{
		IRefParameter<int> sut = It.IsRef<int>(_ => 4);
		string expectedResult = "It.IsRef<int>(_ => 4)";

		string? result = sut.ToString();

		await That(result).IsEqualTo(expectedResult);
	}

	[Fact]
	public async Task ToString_RefParameter_WithPredicate_ShouldReturnExpectedValue()
	{
		IRefParameter<int> sut = It.IsRef<int>(v => v > 4);
		string expectedResult = "It.IsRef<int>(v => v > 4)";

		string? result = sut.ToString();

		await That(result).IsEqualTo(expectedResult);
	}

	[Fact]
	public async Task ToString_RefParameter_WithPredicateAndCallback_ShouldReturnExpectedValue()
	{
		IRefParameter<int> sut = It.IsRef<int>(v => v > 4, v => v * 5);
		string expectedResult = "It.IsRef<int>(v => v > 4, v => v * 5)";

		string? result = sut.ToString();

		await That(result).IsEqualTo(expectedResult);
	}

	[Fact]
	public async Task ToString_ShouldWork()
	{
		string expectedResult = Guid.NewGuid().ToString();
		IMethodService mock = Mock.Create<IMethodService>();
		mock.SetupMock.Method.ToString().Returns(expectedResult);

		string result = mock.ToString();

		await That(result).IsEqualTo(expectedResult);
	}

	[Fact]
	public async Task TriggerCallbacks_ArrayLengthDoesNotMatch_ShouldNotThrow()
	{
		void Act()
		{
			MyMethodSetup.DoTriggerCallbacks([null, null,], [null,]);
		}

		await That(Act).DoesNotThrow();
	}

	[Fact]
	public async Task VoidMethod_Callback_ShouldExecuteWhenInvoked()
	{
		int callCount = 0;
		IVoidMethodSetupWithParametersTest sut = Mock.Create<IVoidMethodSetupWithParametersTest>();

		sut.SetupMock.Method.MethodWithoutOtherOverloads(Match.AnyParameters())
			.Do(() => { callCount++; });

		sut.MethodWithoutOtherOverloads(1, 2, 3);

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
		IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();
		MockRegistration registration = ((IHasMockRegistration)sut).Registrations;

		sut.SetupMock.Method.Method0();
		sut.SetupMock.Method.Method1(It.IsAny<int>());
		sut.SetupMock.Method.Method2(It.IsAny<int>(), It.IsAny<int>());
		sut.SetupMock.Method.Method3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>());
		sut.SetupMock.Method.Method4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>());
		sut.SetupMock.Method.Method5(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
			It.IsAny<int>());

		void Act()
		{
			registration.InvokeMethod(
				$"Mockolate.Tests.MockMethods.SetupMethodTests.IVoidMethodSetupTest.{methodName}", _ => 0,
				parameters.Select(x => (object?)x).ToArray());
		}

		await That(Act).Throws<MockException>()
			.WithMessage("The method setup does not support return values.");
	}

	[Fact]
	public async Task VoidMethod_Verify_ShouldMatchAnyParameters()
	{
		int callCount = 0;
		IVoidMethodSetupWithParametersTest sut = Mock.Create<IVoidMethodSetupWithParametersTest>();

		sut.SetupMock.Method.MethodWithoutOtherOverloads(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
			.Do(() => { callCount++; });

		sut.MethodWithoutOtherOverloads(1, 2, 3);

		await That(callCount).IsEqualTo(1);
		await That(sut.VerifyMock.Invoked.MethodWithoutOtherOverloads(Match.AnyParameters())).Once();
	}

	[Fact]
	public async Task VoidMethod_WithParameters_GetReturnValue_ShouldThrowMockException()
	{
		IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();
		MockRegistration registration = ((IHasMockRegistration)sut).Registrations;

		sut.SetupMock.Method.UniqueMethodWithParameters(Match.AnyParameters());

		void Act()
		{
			registration.InvokeMethod(
				"Mockolate.Tests.MockMethods.SetupMethodTests.IVoidMethodSetupTest.UniqueMethodWithParameters", _ => 0,
				1, 2);
		}

		await That(Act).Throws<MockException>()
			.WithMessage("The method setup does not support return values.");
	}

	[Fact]
	public async Task VoidMethodWith17Parameters_ShouldStillAllowCallbackAndReturns()
	{
		int isCalled = 0;
		IMyServiceWithMethodsWithMoreThan16Parameters mock =
			Mock.Create<IMyServiceWithMethodsWithMoreThan16Parameters>();

		mock.SetupMock.Method.VoidMethod17(
				It.Is(1), It.Is(2), It.Is(3), It.Is(4), It.Is(5), It.Is(6), It.Is(7), It.Is(8),
				It.Is(9), It.Is(10), It.Is(11), It.Is(12), It.Is(13), It.Is(14), It.Is(15), It.Is(16),
				It.Is(17))
			.Do((_, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _)
				=> isCalled++);

		mock.VoidMethod17(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17);

		await That(isCalled).IsEqualTo(1);
	}

	[Fact]
	public async Task VoidMethodWith18Parameters_ShouldStillAllowCallbackAndReturns()
	{
		int isCalled = 0;
		IMyServiceWithMethodsWithMoreThan16Parameters mock =
			Mock.Create<IMyServiceWithMethodsWithMoreThan16Parameters>();

		mock.SetupMock.Method.VoidMethod18(
				It.Is(1), It.Is(2), It.Is(3), It.Is(4), It.Is(5), It.Is(6), It.Is(7), It.Is(8),
				It.Is(9), It.Is(10), It.Is(11), It.Is(12), It.Is(13), It.Is(14), It.Is(15), It.Is(16),
				It.Is(17), It.Is(18))
			.Do((_, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _)
				=> isCalled++);

		mock.VoidMethod18(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18);

		await That(isCalled).IsEqualTo(1);
	}

	[Fact]
	public async Task WhenNotSetup_ShouldReturnDefaultValue()
	{
		IMethodService mock = Mock.Create<IMethodService>();

		int result1 = mock.MyIntMethodWithoutParameters();
		int result2 = mock.MyIntMethodWithParameters(0, "foo");

		await That(result1).IsEqualTo(0);
		await That(result2).IsEqualTo(0);
	}

	[Fact]
	public async Task WhenNotSetup_ThrowWhenNotSetup_ShouldThrowMockNotSetupException()
	{
		IMethodService mock = Mock.Create<IMethodService>(MockBehavior.Default with
		{
			ThrowWhenNotSetup = true,
		});

		void Act()
		{
			mock.MyIntMethodWithoutParameters();
		}

		await That(Act).Throws<MockNotSetupException>()
			.WithMessage(
				"The method 'Mockolate.Tests.MockMethods.SetupMethodTests.IMethodService.MyIntMethodWithoutParameters()' was invoked without prior setup.");
	}

	[Fact]
	public async Task WithInParameter_ShouldUseSetup()
	{
		IMethodSetupWithInAndRefReadonlyParameter sut = Mock.Create<IMethodSetupWithInAndRefReadonlyParameter>();
		MyReadonlyStruct value = new(3);
		sut.SetupMock.Method.MethodWithInParameter(It.IsAny<MyReadonlyStruct>())
			.Returns(p1 => p1.Value);

		int result = sut.MethodWithInParameter(in value);

		await That(result).IsEqualTo(3);
	}

	[Fact]
	public async Task WithRefReadonlyParameter_ShouldUseSetup()
	{
		IMethodSetupWithInAndRefReadonlyParameter sut = Mock.Create<IMethodSetupWithInAndRefReadonlyParameter>();
		MyReadonlyStruct value = new(3);
		sut.SetupMock.Method.MethodWithRefReadonlyParameter(It.IsAnyRef<MyReadonlyStruct>())
			.Returns(p1 => p1.Value);

		int result = sut.MethodWithRefReadonlyParameter(in value);

		await That(result).IsEqualTo(3);
	}
*/
	
	public class ReturnMethodWith0Parameters
	{
		[Fact]
		public async Task ToString_ShouldReturnMethodSignature()
		{
			ReturnMethodSetup<int> setup = new("Foo");

			string result = setup.ToString();

			await That(result).IsEqualTo("int Foo()");
		}

		[Fact]
		public async Task WhenSetupWithNull_ShouldReturnDefaultValue()
		{
			int callCount = 0;
			IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

			sut.SetupMock.Method.Method0()
				.Do(() => { callCount++; })
				.Returns((string?)null!);

			string result = sut.Method0();

			await That(callCount).IsEqualTo(1);
			await That(result).IsNull();
		}
	}

	public class ReturnMethodWith1Parameters
	{
		[Fact]
		public async Task ToString_AnyParameters_ShouldReturnMethodSignature()
		{
			ReturnMethodSetup<int, string> setup = new("Foo", Match.AnyParameters());

			string result = setup.ToString();

			await That(result).IsEqualTo("int Foo(Match.AnyParameters())");
		}

		[Fact]
		public async Task ToString_ShouldReturnMethodSignature()
		{
			ReturnMethodSetup<int, string> setup = new("Foo",
				new NamedParameter("bar", (IParameter)It.IsAny<string>()));

			string result = setup.ToString();

			await That(result).IsEqualTo("int Foo(It.IsAny<string>())");
		}

		[Fact]
		public async Task WhenSetupWithNull_ShouldReturnDefaultValue()
		{
			int callCount = 0;
			IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

			sut.SetupMock.Method.Method1(It.IsAny<int>())
				.Do(() => { callCount++; })
				.Returns((string?)null!);

			string result = sut.Method1(1);

			await That(callCount).IsEqualTo(1);
			await That(result).IsNull();
		}
	}

	public class ReturnMethodWith2Parameters
	{
		[Fact]
		public async Task ToString_AnyParameters_ShouldReturnMethodSignature()
		{
			ReturnMethodSetup<int, string, long> setup = new("Foo", Match.AnyParameters());

			string result = setup.ToString();

			await That(result).IsEqualTo("int Foo(Match.AnyParameters())");
		}

		[Fact]
		public async Task ToString_ShouldReturnMethodSignature()
		{
			ReturnMethodSetup<int, string, long> setup = new("Foo",
				new NamedParameter("p1", (IParameter)It.IsAny<string>()),
				new NamedParameter("p2", (IParameter)It.IsAny<long>()));

			string result = setup.ToString();

			await That(result).IsEqualTo("int Foo(It.IsAny<string>(), It.IsAny<long>())");
		}

		[Fact]
		public async Task WhenSetupWithNull_ShouldReturnDefaultValue()
		{
			int callCount = 0;
			IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

			sut.SetupMock.Method.Method2(It.IsAny<int>(), It.IsAny<int>())
				.Do(() => { callCount++; })
				.Returns((string?)null!);

			string result = sut.Method2(1, 2);

			await That(callCount).IsEqualTo(1);
			await That(result).IsNull();
		}
	}

	public class ReturnMethodWith3Parameters
	{
		[Fact]
		public async Task ToString_AnyParameters_ShouldReturnMethodSignature()
		{
			ReturnMethodSetup<int, string, long, int> setup = new("Foo", Match.AnyParameters());

			string result = setup.ToString();

			await That(result).IsEqualTo("int Foo(Match.AnyParameters())");
		}

		[Fact]
		public async Task ToString_ShouldReturnMethodSignature()
		{
			ReturnMethodSetup<int, string, long, int> setup = new("Foo",
				new NamedParameter("p1", (IParameter)It.IsAny<string>()),
				new NamedParameter("p2", (IParameter)It.IsAny<long>()),
				new NamedParameter("p3", (IParameter)It.IsAny<int>()));

			string result = setup.ToString();

			await That(result).IsEqualTo("int Foo(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())");
		}

		[Fact]
		public async Task WhenSetupWithNull_ShouldReturnDefaultValue()
		{
			int callCount = 0;
			IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

			sut.SetupMock.Method.Method3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
				.Do(() => { callCount++; })
				.Returns((string?)null!);

			string result = sut.Method3(1, 2, 3);

			await That(callCount).IsEqualTo(1);
			await That(result).IsNull();
		}
	}

	public class ReturnMethodWith4Parameters
	{
		[Fact]
		public async Task ToString_AnyParameters_ShouldReturnMethodSignature()
		{
			ReturnMethodSetup<int, string, long, int, int> setup = new("Foo", Match.AnyParameters());

			string result = setup.ToString();

			await That(result).IsEqualTo("int Foo(Match.AnyParameters())");
		}

		[Fact]
		public async Task ToString_ShouldReturnMethodSignature()
		{
			ReturnMethodSetup<int, string, long, int, int> setup = new("Foo",
				new NamedParameter("p1", (IParameter)It.IsAny<string>()),
				new NamedParameter("p2", (IParameter)It.IsAny<long>()),
				new NamedParameter("p3", (IParameter)It.IsAny<int>()),
				new NamedParameter("p4", (IParameter)It.IsAny<int>()));

			string result = setup.ToString();

			await That(result)
				.IsEqualTo(
					"int Foo(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<int>())");
		}

		[Fact]
		public async Task WhenSetupWithNull_ShouldReturnDefaultValue()
		{
			int callCount = 0;
			IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

			sut.SetupMock.Method.Method4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
				.Do(() => { callCount++; })
				.Returns((string?)null!);

			string result = sut.Method4(1, 2, 3, 4);

			await That(callCount).IsEqualTo(1);
			await That(result).IsNull();
		}
	}

	public class ReturnMethodWith5Parameters
	{
		[Fact]
		public async Task ToString_AnyParameters_ShouldReturnMethodSignature()
		{
			ReturnMethodSetup<int, string, long, int, int, int> setup = new("Foo", Match.AnyParameters());

			string result = setup.ToString();

			await That(result).IsEqualTo("int Foo(Match.AnyParameters())");
		}

		[Fact]
		public async Task ToString_ShouldReturnMethodSignature()
		{
			ReturnMethodSetup<int, string, long, int, int, int> setup = new("Foo",
				new NamedParameter("p1", (IParameter)It.IsAny<string>()),
				new NamedParameter("p2", (IParameter)It.IsAny<long>()),
				new NamedParameter("p3", (IParameter)It.IsAny<int>()),
				new NamedParameter("p4", (IParameter)It.IsAny<int>()),
				new NamedParameter("p5", (IParameter)It.IsAny<int>()));

			string result = setup.ToString();

			await That(result)
				.IsEqualTo(
					"int Foo(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())");
		}

		[Fact]
		public async Task WhenSetupWithNull_ShouldReturnDefaultValue()
		{
			int callCount = 0;
			IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

			sut.SetupMock.Method.Method5(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
					It.IsAny<int>())
				.Do(() => { callCount++; })
				.Returns((string?)null!);

			string result = sut.Method5(1, 2, 3, 4, 5);

			await That(callCount).IsEqualTo(1);
			await That(result).IsNull();
		}
	}

	public class VoidMethodWith0Parameters
	{
		[Fact]
		public async Task ToString_ShouldReturnMethodSignature()
		{
			VoidMethodSetup setup = new("Foo");

			string result = setup.ToString();

			await That(result).IsEqualTo("void Foo()");
		}
	}

	public class VoidMethodWith1Parameters
	{
		[Fact]
		public async Task ToString_AnyParameters_ShouldReturnMethodSignature()
		{
			VoidMethodSetup<string> setup = new("Foo", Match.AnyParameters());

			string result = setup.ToString();

			await That(result).IsEqualTo("void Foo(Match.AnyParameters())");
		}

		[Fact]
		public async Task ToString_ShouldReturnMethodSignature()
		{
			VoidMethodSetup<string> setup = new("Foo",
				new NamedParameter("bar", (IParameter)It.IsAny<string>()));

			string result = setup.ToString();

			await That(result).IsEqualTo("void Foo(It.IsAny<string>())");
		}
	}

	public class VoidMethodWith2Parameters
	{
		[Fact]
		public async Task ToString_AnyParameters_ShouldReturnMethodSignature()
		{
			VoidMethodSetup<string, long> setup = new("Foo", Match.AnyParameters());

			string result = setup.ToString();

			await That(result).IsEqualTo("void Foo(Match.AnyParameters())");
		}

		[Fact]
		public async Task ToString_ShouldReturnMethodSignature()
		{
			VoidMethodSetup<string, long> setup = new("Foo",
				new NamedParameter("p1", (IParameter)It.IsAny<string>()),
				new NamedParameter("p2", (IParameter)It.IsAny<long>()));

			string result = setup.ToString();

			await That(result).IsEqualTo("void Foo(It.IsAny<string>(), It.IsAny<long>())");
		}
	}

	public class VoidMethodWith3Parameters
	{
		[Fact]
		public async Task ToString_AnyParameters_ShouldReturnMethodSignature()
		{
			VoidMethodSetup<string, long, int> setup = new("Foo", Match.AnyParameters());

			string result = setup.ToString();

			await That(result).IsEqualTo("void Foo(Match.AnyParameters())");
		}

		[Fact]
		public async Task ToString_ShouldReturnMethodSignature()
		{
			VoidMethodSetup<string, long, int> setup = new("Foo",
				new NamedParameter("p1", (IParameter)It.IsAny<string>()),
				new NamedParameter("p2", (IParameter)It.IsAny<long>()),
				new NamedParameter("p3", (IParameter)It.IsAny<int>()));

			string result = setup.ToString();

			await That(result).IsEqualTo("void Foo(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())");
		}
	}

	public class VoidMethodWith4Parameters
	{
		[Fact]
		public async Task ToString_AnyParameters_ShouldReturnMethodSignature()
		{
			VoidMethodSetup<string, long, int, int> setup = new("Foo", Match.AnyParameters());

			string result = setup.ToString();

			await That(result).IsEqualTo("void Foo(Match.AnyParameters())");
		}

		[Fact]
		public async Task ToString_ShouldReturnMethodSignature()
		{
			VoidMethodSetup<string, long, int, int> setup = new("Foo",
				new NamedParameter("p1", (IParameter)It.IsAny<string>()),
				new NamedParameter("p2", (IParameter)It.IsAny<long>()),
				new NamedParameter("p3", (IParameter)It.IsAny<int>()),
				new NamedParameter("p4", (IParameter)It.IsAny<int>()));

			string result = setup.ToString();

			await That(result)
				.IsEqualTo(
					"void Foo(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<int>())");
		}
	}

	public class VoidMethodWith5Parameters
	{
		[Fact]
		public async Task ToString_AnyParameters_ShouldReturnMethodSignature()
		{
			VoidMethodSetup<string, long, int, int, int> setup = new("Foo", Match.AnyParameters());

			string result = setup.ToString();

			await That(result).IsEqualTo("void Foo(Match.AnyParameters())");
		}

		[Fact]
		public async Task ToString_ShouldReturnMethodSignature()
		{
			VoidMethodSetup<string, long, int, int, int> setup = new("Foo",
				new NamedParameter("p1", (IParameter)It.IsAny<string>()),
				new NamedParameter("p2", (IParameter)It.IsAny<long>()),
				new NamedParameter("p3", (IParameter)It.IsAny<int>()),
				new NamedParameter("p4", (IParameter)It.IsAny<int>()),
				new NamedParameter("p5", (IParameter)It.IsAny<int>()));

			string result = setup.ToString();

			await That(result)
				.IsEqualTo(
					"void Foo(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())");
		}
	}

	public class MyMethodSetup : MethodSetup
	{
		public static void DoTriggerCallbacks(NamedParameter?[] namedParameters, object?[] values)
			=> TriggerCallbacks(namedParameters, values);

		public bool GetHasReturnCalls()
			=> base.HasReturnCalls();

		protected override bool? GetSkipBaseClass()
			=> throw new NotSupportedException();

		protected override T SetOutParameter<T>(string parameterName, Func<T> defaultValueGenerator)
			=> throw new NotSupportedException();

		protected override T SetRefParameter<T>(string parameterName, T value, MockBehavior behavior)
			=> throw new NotSupportedException();

		protected override void ExecuteCallback(MethodInvocation invocation, MockBehavior behavior)
			=> throw new NotSupportedException();

		protected override TResult GetReturnValue<TResult>(MethodInvocation invocation, MockBehavior behavior,
			Func<TResult> defaultValueGenerator)
			=> throw new NotSupportedException();

		protected override bool IsMatch(MethodInvocation invocation)
			=> throw new NotSupportedException();

		protected override void TriggerParameterCallbacks(object?[] parameters)
			=> throw new NotSupportedException();
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

	public interface IMethodSetupWithInAndRefReadonlyParameter
	{
		int MethodWithInParameter(in MyReadonlyStruct p1);
		int MethodWithRefReadonlyParameter(ref readonly MyReadonlyStruct p1);
	}

	public readonly struct MyReadonlyStruct(int value)
	{
		public int Value { get; } = value;
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
		void MyMethodWithRefParameter(ref int value);
		void MyMethodWithOutParameter(out int value);
		string ToString();
		bool Equals(object? obj);
		bool Equals(int other);
		int GetHashCode();
	}

	public interface IMyServiceWithMethodsWithMoreThan16Parameters
	{
		int ReturnMethod17(
			int p1, int p2, int p3, int p4, int p5, int p6, int p7, int p8,
			int p9, int p10, int p11, int p12, int p13, int p14, int p15, int p16,
			int p17);

		int ReturnMethod18(
			int p1, int p2, int p3, int p4, int p5, int p6, int p7, int p8,
			int p9, int p10, int p11, int p12, int p13, int p14, int p15, int p16,
			int p17, int p18);

		void VoidMethod17(
			int p1, int p2, int p3, int p4, int p5, int p6, int p7, int p8,
			int p9, int p10, int p11, int p12, int p13, int p14, int p15, int p16,
			int p17);

		void VoidMethod18(
			int p1, int p2, int p3, int p4, int p5, int p6, int p7, int p8,
			int p9, int p10, int p11, int p12, int p13, int p14, int p15, int p16,
			int p17, int p18);
	}

	public interface IReturnMethodSetupTest
	{
		string Method0();
		string Method0(bool withOtherParameter);
		string Method1(int p1);
		string Method1(int p1, bool withOtherParameter);
		string Method1WithOutParameter(out int p1);
		string Method1WithRefParameter(ref int p1);
		string Method2(int p1, int p2);
		string Method2(int p1, int p2, bool withOtherParameter);
		string Method2WithOutParameter(out int p1, out int p2);
		string Method2WithRefParameter(ref int p1, ref int p2);
		string Method3(int p1, int p2, int p3);
		string Method3(int p1, int p2, int p3, bool withOtherParameter);
		string Method3WithOutParameter(out int p1, out int p2, out int p3);
		string Method3WithRefParameter(ref int p1, ref int p2, ref int p3);
		string Method4(int p1, int p2, int p3, int p4);
		string Method4(int p1, int p2, int p3, bool withOtherParameter);
		string Method4WithOutParameter(out int p1, out int p2, out int p3, out int p4);
		string Method4WithRefParameter(ref int p1, ref int p2, ref int p3, ref int p4);
		string Method5(int p1, int p2, int p3, int p4, int p5);
		string Method5(int p1, int p2, int p3, int p4, int p5, bool withOtherParameter);
		string Method5WithOutParameter(out int p1, out int p2, out int p3, out int p4, out int p5);
		string Method5WithRefParameter(ref int p1, ref int p2, ref int p3, ref int p4, ref int p5);
		string UniqueMethodWithParameters(int p1, int p2);
	}

	public interface IVoidMethodSetupTest
	{
		void Method0();
		void Method0(bool withOtherParameter);
		void Method1(int p1);
		void Method1(int p1, bool withOtherParameter);
		void Method1WithOutParameter(out int p1);
		void Method1WithRefParameter(ref int p1);
		void Method2(int p1, int p2);
		void Method2(int p1, int p2, bool withOtherParameter);
		void Method2WithOutParameter(out int p1, out int p2);
		void Method2WithRefParameter(ref int p1, ref int p2);
		void Method3(int p1, int p2, int p3);
		void Method3(int p1, int p2, int p3, bool withOtherParameter);
		void Method3WithOutParameter(out int p1, out int p2, out int p3);
		void Method3WithRefParameter(ref int p1, ref int p2, ref int p3);
		void Method4(int p1, int p2, int p3, int p4);
		void Method4(int p1, int p2, int p3, bool withOtherParameter);
		void Method4WithOutParameter(out int p1, out int p2, out int p3, out int p4);
		void Method4WithRefParameter(ref int p1, ref int p2, ref int p3, ref int p4);
		void Method5(int p1, int p2, int p3, int p4, int p5);
		void Method5(int p1, int p2, int p3, int p4, int p5, bool withOtherParameter);
		void Method5WithOutParameter(out int p1, out int p2, out int p3, out int p4, out int p5);
		void Method5WithRefParameter(ref int p1, ref int p2, ref int p3, ref int p4, ref int p5);
		void UniqueMethodWithParameters(int p1, int p2);
	}
}
