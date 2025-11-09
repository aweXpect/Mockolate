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
		mock.Setup.Method.MyGenericMethod(With(0), With("foo")).Returns(42);

		int result1 = mock.Subject.MyGenericMethod(0, "foo");
		int result2 = mock.Subject.MyGenericMethod(0L, "foo");

		await That(mock.Verify.Invoked.MyGenericMethod(WithAny<long>(), WithAny<string>())).Once();
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
	public async Task OverlappingSetups_ShouldUseLatestMatchingSetup()
	{
		Mock<IMethodService> mock = Mock.Create<IMethodService>();
		mock.Setup.Method.MyIntMethodWithParameters(WithAny<int>(), WithAny<string>()).Returns(1);
		mock.Setup.Method.MyIntMethodWithParameters(With(0), With("foo")).Returns(2);

		int result1 = mock.Subject.MyIntMethodWithParameters(1, "foo");
		int result2 = mock.Subject.MyIntMethodWithParameters(0, "foo");
		int result3 = mock.Subject.MyIntMethodWithParameters(0, "bar");

		await That(result1).IsEqualTo(1);
		await That(result2).IsEqualTo(2);
		await That(result3).IsEqualTo(1);
	}

	[Fact]
	public async Task OverlappingSetups_WhenGeneralSetupIsLater_ShouldOnlyUseGeneralSetup()
	{
		Mock<IMethodService> mock = Mock.Create<IMethodService>();
		mock.Setup.Method.MyIntMethodWithParameters(With(0), With("foo")).Returns(2);
		mock.Setup.Method.MyIntMethodWithParameters(WithAny<int>(), WithAny<string>()).Returns(1);

		int result1 = mock.Subject.MyIntMethodWithParameters(1, "foo");
		int result2 = mock.Subject.MyIntMethodWithParameters(0, "foo");
		int result3 = mock.Subject.MyIntMethodWithParameters(0, "bar");

		await That(result1).IsEqualTo(1);
		await That(result2).IsEqualTo(1);
		await That(result3).IsEqualTo(1);
	}

	[Fact]
	public async Task Register_AfterInvocation_ShouldBeAppliedForFutureUse()
	{
		Mock<IMethodService> mock = Mock.Create<IMethodService>();
		IMockSetup setup = (IMockSetup)mock.Setup;
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

		sut.Setup.Method.MethodWithoutOtherOverloads(WithAnyParameters())
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

		sut.Setup.Method.MethodWithoutOtherOverloads(WithAny<int>(), WithAny<int>(), WithAny<int>())
			.Callback(() => { callCount++; })
			.Returns("foo");

		string result = sut.Subject.MethodWithoutOtherOverloads(1, 2, 3);

		await That(callCount).IsEqualTo(1);
		await That(result).IsEqualTo("foo");
		await That(sut.Verify.Invoked.MethodWithoutOtherOverloads(WithAnyParameters())).Once();
	}

	[Fact]
	public async Task ReturnMethod_WhenSetupWithNull_ShouldReturnDefaultValue()
	{
		int callCount = 0;
		Mock<IReturnMethodSetupWithParametersTest> sut = Mock.Create<IReturnMethodSetupWithParametersTest>();

		sut.Setup.Method.MethodWithoutOtherOverloads(WithAnyParameters())
			.Callback(() => { callCount++; })
			.Returns((string?)null!);

		string result = sut.Subject.MethodWithoutOtherOverloads(1, 2, 3);

		await That(callCount).IsEqualTo(1);
		await That(result).IsNull();
	}

	[Fact]
	public async Task ReturnMethodWith17Parameters_ShouldStillAllowCallbackAndReturns()
	{
		int isCalled = 0;
		Mock<IMyServiceWithMethodsWithMoreThan16Parameters> mock =
			Mock.Create<IMyServiceWithMethodsWithMoreThan16Parameters>();

		mock.Setup.Method.ReturnMethod17(
				With(1), With(2), With(3), With(4), With(5), With(6), With(7), With(8),
				With(9), With(10), With(11), With(12), With(13), With(14), With(15), With(16),
				With(17))
			.Callback((
					int p1, int p2, int p3, int p4, int p5,
					int p6, int p7, int p8, int p9, int p10,
					int p11, int p12, int p13, int p14, int p15,
					int p16, int p17)
				=> isCalled++)
			.Returns((
					int p1, int p2, int p3, int p4, int p5,
					int p6, int p7, int p8, int p9, int p10,
					int p11, int p12, int p13, int p14, int p15,
					int p16, int p17)
				=> p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9 + p10 + p11 + p12 + p13 + p14 + p15 + p16 + p17);

		int result = mock.Subject.ReturnMethod17(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17);

		await That(isCalled).IsEqualTo(1);
		await That(result).IsEqualTo(153);
	}

	[Fact]
	public async Task ReturnMethodWith18Parameters_ShouldStillAllowCallbackAndReturns()
	{
		int isCalled = 0;
		Mock<IMyServiceWithMethodsWithMoreThan16Parameters> mock =
			Mock.Create<IMyServiceWithMethodsWithMoreThan16Parameters>();

		mock.Setup.Method.ReturnMethod18(
				With(1), With(2), With(3), With(4), With(5), With(6), With(7), With(8),
				With(9), With(10), With(11), With(12), With(13), With(14), With(15), With(16),
				With(17), With(18))
			.Callback((
					int p1, int p2, int p3, int p4, int p5,
					int p6, int p7, int p8, int p9, int p10,
					int p11, int p12, int p13, int p14, int p15,
					int p16, int p17, int p18)
				=> isCalled++)
			.Returns((
					int p1, int p2, int p3, int p4, int p5,
					int p6, int p7, int p8, int p9, int p10,
					int p11, int p12, int p13, int p14, int p15,
					int p16, int p17, int p18)
				=> p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9 + p10 + p11 + p12 + p13 + p14 + p15 + p16 + p17 + p18);

		int result = mock.Subject.ReturnMethod18(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18);

		await That(isCalled).IsEqualTo(1);
		await That(result).IsEqualTo(171);
	}

	[Fact]
	public async Task Setup_ShouldUseNewestMatchingSetup()
	{
		Mock<IMethodService> mock = Mock.Create<IMethodService>();
		mock.Setup.Method.MyIntMethodWithParameters(WithAny<int>(), WithAny<string>()).Returns(10);

		await That(mock.Subject.MyIntMethodWithParameters(1, "")).IsEqualTo(10);

		mock.Setup.Method.MyIntMethodWithParameters(WithAny<int>(), WithAny<string>()).Returns(20);

		await That(mock.Subject.MyIntMethodWithParameters(1, "")).IsEqualTo(20);
	}

	[Fact]
	public async Task Setup_WithOutParameter_ShouldUseCallbackToSetValue()
	{
		Mock<IMethodService> mock = Mock.Create<IMethodService>();
		mock.Setup.Method.MyMethodWithOutParameter(Out(() => 4));

		mock.Subject.MyMethodWithOutParameter(out var value);

		await That(value).IsEqualTo(4);
	}

	[Fact]
	public async Task Setup_WithOutParameterWithoutCallback_ShouldUseDefaultValueSetValue()
	{
		Mock<IMethodService> mock = Mock.Create<IMethodService>();
		mock.Setup.Method.MyMethodWithOutParameter(Out<int>());

		mock.Subject.MyMethodWithOutParameter(out var value);

		await That(value).IsEqualTo(0);
	}

	[Fact]
	public async Task Setup_WithRefParameter_WithCallback_ShouldUseCallbackToSetValue()
	{
		Mock<IMethodService> mock = Mock.Create<IMethodService>();
		mock.Setup.Method.MyMethodWithRefParameter(Ref<int>(_ => 4));
		int value = 2;

		mock.Subject.MyMethodWithRefParameter(ref value);

		await That(value).IsEqualTo(4);
	}

	[Fact]
	public async Task Setup_WithRefParameter_WithPredicate_ShouldUseCallbackToSetValue()
	{
		Mock<IMethodService> mock = Mock.Create<IMethodService>();
		mock.Setup.Method.MyMethodWithRefParameter(Ref<int>(v => v > 2));
		int value = 2;

		mock.Subject.MyMethodWithRefParameter(ref value);

		await That(value).IsEqualTo(2);
	}

	[Fact]
	public async Task Setup_WithRefParameter_WithPredicateAndCallback_ShouldUseCallbackToSetValueWhenPredicateMatches()
	{
		Mock<IMethodService> mock = Mock.Create<IMethodService>();
		mock.Setup.Method.MyMethodWithRefParameter(Ref<int>(v => v > 2, _ => 4));
		int value1 = 2;
		int value2 = 3;

		mock.Subject.MyMethodWithRefParameter(ref value1);
		mock.Subject.MyMethodWithRefParameter(ref value2);

		await That(value1).IsEqualTo(2);
		await That(value2).IsEqualTo(4);
	}

	[Fact]
	public async Task Setup_WithRefParameter_WithoutPredicateOrCallback_ShouldNotChangeValue()
	{
		Mock<IMethodService> mock = Mock.Create<IMethodService>();
		mock.Setup.Method.MyMethodWithRefParameter(Ref<int>());
		int value = 2;

		mock.Subject.MyMethodWithRefParameter(ref value);

		await That(value).IsEqualTo(2);
	}

	[Fact]
	public async Task ToString_OutParameter_ShouldReturnExpectedValue()
	{
		var sut = Out<int>();
		string expectedResult = "Out<int>()";

		string? result = sut.ToString();

		await That(result).IsEqualTo(expectedResult);
	}

	[Fact]
	public async Task ToString_OutParameter_WithCallback_ShouldReturnExpectedValue()
	{
		var sut = Out<int>(() => 4);
		string expectedResult = "Out<int>(() => 4)";

		string? result = sut.ToString();

		await That(result).IsEqualTo(expectedResult);
	}

	[Fact]
	public async Task ToString_RefParameter_ShouldReturnExpectedValue()
	{
		var sut = Ref<int>();
		string expectedResult = "Ref<int>()";

		string? result = sut.ToString();

		await That(result).IsEqualTo(expectedResult);
	}

	[Fact]
	public async Task ToString_RefParameter_WithCallback_ShouldReturnExpectedValue()
	{
		var sut = Ref<int>(v => 4);
		string expectedResult = "Ref<int>(v => 4)";

		string? result = sut.ToString();

		await That(result).IsEqualTo(expectedResult);
	}

	[Fact]
	public async Task ToString_RefParameter_WithPredicate_ShouldReturnExpectedValue()
	{
		var sut = Ref<int>(v => v > 4);
		string expectedResult = "Ref<int>(v => v > 4)";

		string? result = sut.ToString();

		await That(result).IsEqualTo(expectedResult);
	}

	[Fact]
	public async Task ToString_RefParameter_WithPredicateAndCallback_ShouldReturnExpectedValue()
	{
		var sut = Ref<int>(v => v > 4, v => v * 5);
		string expectedResult = "Ref<int>(v => v > 4, v => v * 5)";

		string? result = sut.ToString();

		await That(result).IsEqualTo(expectedResult);
	}

	[Fact]
	public async Task VoidMethod_Callback_ShouldExecuteWhenInvoked()
	{
		int callCount = 0;
		Mock<IVoidMethodSetupWithParametersTest> sut = Mock.Create<IVoidMethodSetupWithParametersTest>();

		sut.Setup.Method.MethodWithoutOtherOverloads(WithAnyParameters())
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
		sut.Setup.Method.Method1(WithAny<int>());
		sut.Setup.Method.Method2(WithAny<int>(), WithAny<int>());
		sut.Setup.Method.Method3(WithAny<int>(), WithAny<int>(), WithAny<int>());
		sut.Setup.Method.Method4(WithAny<int>(), WithAny<int>(), WithAny<int>(), WithAny<int>());
		sut.Setup.Method.Method5(WithAny<int>(), WithAny<int>(), WithAny<int>(), WithAny<int>(), WithAny<int>());

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

		sut.Setup.Method.MethodWithoutOtherOverloads(WithAny<int>(), WithAny<int>(), WithAny<int>())
			.Callback(() => { callCount++; });

		sut.Subject.MethodWithoutOtherOverloads(1, 2, 3);

		await That(callCount).IsEqualTo(1);
		await That(sut.Verify.Invoked.MethodWithoutOtherOverloads(WithAnyParameters())).Once();
	}

	[Fact]
	public async Task VoidMethod_WithParameters_GetReturnValue_ShouldThrowMockException()
	{
		Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

		sut.Setup.Method.UniqueMethodWithParameters(WithAnyParameters());

		void Act()
			=> ((IMock)sut).Execute<int>(
				"Mockolate.Tests.MockMethods.SetupMethodTests.IVoidMethodSetupTest.UniqueMethodWithParameters", 1, 2);

		await That(Act).Throws<MockException>()
			.WithMessage("The method setup does not support return values.");
	}

	[Fact]
	public async Task VoidMethodWith17Parameters_ShouldStillAllowCallbackAndReturns()
	{
		int isCalled = 0;
		Mock<IMyServiceWithMethodsWithMoreThan16Parameters> mock =
			Mock.Create<IMyServiceWithMethodsWithMoreThan16Parameters>();

		mock.Setup.Method.VoidMethod17(
				With(1), With(2), With(3), With(4), With(5), With(6), With(7), With(8),
				With(9), With(10), With(11), With(12), With(13), With(14), With(15), With(16),
				With(17))
			.Callback((
					int p1, int p2, int p3, int p4, int p5,
					int p6, int p7, int p8, int p9, int p10,
					int p11, int p12, int p13, int p14, int p15,
					int p16, int p17)
				=> isCalled++);

		mock.Subject.VoidMethod17(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17);

		await That(isCalled).IsEqualTo(1);
	}

	[Fact]
	public async Task VoidMethodWith18Parameters_ShouldStillAllowCallbackAndReturns()
	{
		int isCalled = 0;
		Mock<IMyServiceWithMethodsWithMoreThan16Parameters> mock =
			Mock.Create<IMyServiceWithMethodsWithMoreThan16Parameters>();

		mock.Setup.Method.VoidMethod18(
				With(1), With(2), With(3), With(4), With(5), With(6), With(7), With(8),
				With(9), With(10), With(11), With(12), With(13), With(14), With(15), With(16),
				With(17), With(18))
			.Callback((
					int p1, int p2, int p3, int p4, int p5,
					int p6, int p7, int p8, int p9, int p10,
					int p11, int p12, int p13, int p14, int p15,
					int p16, int p17, int p18)
				=> isCalled++);

		mock.Subject.VoidMethod18(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18);

		await That(isCalled).IsEqualTo(1);
	}

	[Fact]
	public async Task WhenNotSetup_ShouldReturnDefaultValue()
	{
		Mock<IMethodService> mock = Mock.Create<IMethodService>();

		int result1 = mock.Subject.MyIntMethodWithoutParameters();
		int result2 = mock.Subject.MyIntMethodWithParameters(0, "foo");

		await That(result1).IsEqualTo(0);
		await That(result2).IsEqualTo(0);
	}

	[Fact]
	public async Task ToString_ShouldWork()
	{
		var expectedResult = Guid.NewGuid().ToString();
		Mock<IMethodService> mock = Mock.Create<IMethodService>();
		mock.Setup.Method.ToString().Returns(expectedResult);

		var result = mock.Subject.ToString();

		await That(result).IsEqualTo(expectedResult);
	}

	[Fact]
	public async Task GetHashCode_ShouldWork()
	{
		int expectedResult = Guid.NewGuid().GetHashCode();
		Mock<IMethodService> mock = Mock.Create<IMethodService>();
		mock.Setup.Method.GetHashCode().Returns(expectedResult);

		var result = mock.Subject.GetHashCode();

		await That(result).IsEqualTo(expectedResult);
	}

	[Fact]
	public async Task Equals_ShouldWork()
	{
		var obj = new object();
		Mock<IMethodService> mock = Mock.Create<IMethodService>();
		mock.Setup.Method.Equals(WithAny<object?>()).Returns(true);

		var result = mock.Subject.Equals(obj);

		await That(result).IsEqualTo(true);
	}

	[Fact]
	public async Task WhenNotSetup_ThrowWhenNotSetup_ShouldThrowMockNotSetupException()
	{
		Mock<IMethodService> mock = Mock.Create<IMethodService>(MockBehavior.Default with
		{
			ThrowWhenNotSetup = true,
		});

		void Act()
			=> mock.Subject.MyIntMethodWithoutParameters();

		await That(Act).Throws<MockNotSetupException>()
			.WithMessage(
				"The method 'Mockolate.Tests.MockMethods.SetupMethodTests.IMethodService.MyIntMethodWithoutParameters()' was invoked without prior setup.");
	}

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
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method0()
				.Callback(() => { callCount++; })
				.Returns((string?)null!);

			string result = sut.Subject.Method0();

			await That(callCount).IsEqualTo(1);
			await That(result).IsNull();
		}
	}

	public class ReturnMethodWith1Parameters
	{
		[Fact]
		public async Task ToString_ShouldReturnMethodSignature()
		{
			ReturnMethodSetup<int, string> setup = new("Foo", new NamedParameter("bar", WithAny<string>()));

			string result = setup.ToString();

			await That(result).IsEqualTo("int Foo(WithAny<string>() bar)");
		}

		[Fact]
		public async Task WhenSetupWithNull_ShouldReturnDefaultValue()
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method1(WithAny<int>())
				.Callback(() => { callCount++; })
				.Returns((string?)null!);

			string result = sut.Subject.Method1(1);

			await That(callCount).IsEqualTo(1);
			await That(result).IsNull();
		}
	}

	public class ReturnMethodWith2Parameters
	{
		[Fact]
		public async Task ToString_ShouldReturnMethodSignature()
		{
			ReturnMethodSetup<int, string, long> setup = new("Foo", new NamedParameter("p1", WithAny<string>()),
				new NamedParameter("p2", WithAny<long>()));

			string result = setup.ToString();

			await That(result).IsEqualTo("int Foo(WithAny<string>() p1, WithAny<long>() p2)");
		}

		[Fact]
		public async Task ToString_WithAnyParameterCombination_ShouldReturnMethodSignature()
		{
			ReturnMethodSetup<int, string, long> setup = new("Foo", WithAnyParameters());

			string result = setup.ToString();

			await That(result).IsEqualTo("int Foo(WithAnyParameters())");
		}

		[Fact]
		public async Task WhenSetupWithNull_ShouldReturnDefaultValue()
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method2(WithAny<int>(), WithAny<int>())
				.Callback(() => { callCount++; })
				.Returns((string?)null!);

			string result = sut.Subject.Method2(1, 2);

			await That(callCount).IsEqualTo(1);
			await That(result).IsNull();
		}
	}

	public class ReturnMethodWith3Parameters
	{
		[Fact]
		public async Task ToString_ShouldReturnMethodSignature()
		{
			ReturnMethodSetup<int, string, long, int> setup = new("Foo",
				new NamedParameter("p1", WithAny<string>()), new NamedParameter("p2", WithAny<long>()),
				new NamedParameter("p3", WithAny<int>()));

			string result = setup.ToString();

			await That(result).IsEqualTo("int Foo(WithAny<string>() p1, WithAny<long>() p2, WithAny<int>() p3)");
		}

		[Fact]
		public async Task ToString_WithAnyParameterCombination_ShouldReturnMethodSignature()
		{
			ReturnMethodSetup<int, string, long, int> setup = new("Foo", WithAnyParameters());

			string result = setup.ToString();

			await That(result).IsEqualTo("int Foo(WithAnyParameters())");
		}

		[Fact]
		public async Task WhenSetupWithNull_ShouldReturnDefaultValue()
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method3(WithAny<int>(), WithAny<int>(), WithAny<int>())
				.Callback(() => { callCount++; })
				.Returns((string?)null!);

			string result = sut.Subject.Method3(1, 2, 3);

			await That(callCount).IsEqualTo(1);
			await That(result).IsNull();
		}
	}

	public class ReturnMethodWith4Parameters
	{
		[Fact]
		public async Task ToString_ShouldReturnMethodSignature()
		{
			ReturnMethodSetup<int, string, long, int, int> setup = new("Foo",
				new NamedParameter("p1", WithAny<string>()), new NamedParameter("p2", WithAny<long>()),
				new NamedParameter("p3", WithAny<int>()), new NamedParameter("p4", WithAny<int>()));

			string result = setup.ToString();

			await That(result)
				.IsEqualTo(
					"int Foo(WithAny<string>() p1, WithAny<long>() p2, WithAny<int>() p3, WithAny<int>() p4)");
		}

		[Fact]
		public async Task ToString_WithAnyParameterCombination_ShouldReturnMethodSignature()
		{
			ReturnMethodSetup<int, string, long, int, int> setup = new("Foo", WithAnyParameters());

			string result = setup.ToString();

			await That(result).IsEqualTo("int Foo(WithAnyParameters())");
		}

		[Fact]
		public async Task WhenSetupWithNull_ShouldReturnDefaultValue()
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method4(WithAny<int>(), WithAny<int>(), WithAny<int>(), WithAny<int>())
				.Callback(() => { callCount++; })
				.Returns((string?)null!);

			string result = sut.Subject.Method4(1, 2, 3, 4);

			await That(callCount).IsEqualTo(1);
			await That(result).IsNull();
		}
	}

	public class ReturnMethodWith5Parameters
	{
		[Fact]
		public async Task ToString_ShouldReturnMethodSignature()
		{
			ReturnMethodSetup<int, string, long, int, int, int> setup = new("Foo",
				new NamedParameter("p1", WithAny<string>()), new NamedParameter("p2", WithAny<long>()),
				new NamedParameter("p3", WithAny<int>()), new NamedParameter("p4", WithAny<int>()),
				new NamedParameter("p5", WithAny<int>()));

			string result = setup.ToString();

			await That(result)
				.IsEqualTo(
					"int Foo(WithAny<string>() p1, WithAny<long>() p2, WithAny<int>() p3, WithAny<int>() p4, WithAny<int>() p5)");
		}

		[Fact]
		public async Task ToString_WithAnyParameterCombination_ShouldReturnMethodSignature()
		{
			ReturnMethodSetup<int, string, long, int, int, int> setup = new("Foo", WithAnyParameters());

			string result = setup.ToString();

			await That(result).IsEqualTo("int Foo(WithAnyParameters())");
		}

		[Fact]
		public async Task WhenSetupWithNull_ShouldReturnDefaultValue()
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method5(WithAny<int>(), WithAny<int>(), WithAny<int>(), WithAny<int>(),
					WithAny<int>())
				.Callback(() => { callCount++; })
				.Returns((string?)null!);

			string result = sut.Subject.Method5(1, 2, 3, 4, 5);

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
		public async Task ToString_ShouldReturnMethodSignature()
		{
			VoidMethodSetup<string> setup = new("Foo", new NamedParameter("bar", WithAny<string>()));

			string result = setup.ToString();

			await That(result).IsEqualTo("void Foo(WithAny<string>() bar)");
		}
	}

	public class VoidMethodWith2Parameters
	{
		[Fact]
		public async Task ToString_ShouldReturnMethodSignature()
		{
			VoidMethodSetup<string, long> setup = new("Foo", new NamedParameter("p1", WithAny<string>()),
				new NamedParameter("p2", WithAny<long>()));

			string result = setup.ToString();

			await That(result).IsEqualTo("void Foo(WithAny<string>() p1, WithAny<long>() p2)");
		}

		[Fact]
		public async Task ToString_WithAnyParameterCombination_ShouldReturnMethodSignature()
		{
			VoidMethodSetup<string, long> setup = new("Foo", WithAnyParameters());

			string result = setup.ToString();

			await That(result).IsEqualTo("void Foo(WithAnyParameters())");
		}
	}

	public class VoidMethodWith3Parameters
	{
		[Fact]
		public async Task ToString_ShouldReturnMethodSignature()
		{
			VoidMethodSetup<string, long, int> setup = new("Foo", new NamedParameter("p1", WithAny<string>()),
				new NamedParameter("p2", WithAny<long>()), new NamedParameter("p3", WithAny<int>()));

			string result = setup.ToString();

			await That(result).IsEqualTo("void Foo(WithAny<string>() p1, WithAny<long>() p2, WithAny<int>() p3)");
		}

		[Fact]
		public async Task ToString_WithAnyParameterCombination_ShouldReturnMethodSignature()
		{
			VoidMethodSetup<string, long, int> setup = new("Foo", WithAnyParameters());

			string result = setup.ToString();

			await That(result).IsEqualTo("void Foo(WithAnyParameters())");
		}
	}

	public class VoidMethodWith4Parameters
	{
		[Fact]
		public async Task ToString_ShouldReturnMethodSignature()
		{
			VoidMethodSetup<string, long, int, int> setup = new("Foo",
				new NamedParameter("p1", WithAny<string>()), new NamedParameter("p2", WithAny<long>()),
				new NamedParameter("p3", WithAny<int>()), new NamedParameter("p4", WithAny<int>()));

			string result = setup.ToString();

			await That(result)
				.IsEqualTo(
					"void Foo(WithAny<string>() p1, WithAny<long>() p2, WithAny<int>() p3, WithAny<int>() p4)");
		}

		[Fact]
		public async Task ToString_WithAnyParameterCombination_ShouldReturnMethodSignature()
		{
			VoidMethodSetup<string, long, int, int> setup = new("Foo", WithAnyParameters());

			string result = setup.ToString();

			await That(result).IsEqualTo("void Foo(WithAnyParameters())");
		}
	}

	public class VoidMethodWith5Parameters
	{
		[Fact]
		public async Task ToString_ShouldReturnMethodSignature()
		{
			VoidMethodSetup<string, long, int, int, int> setup = new("Foo",
				new NamedParameter("p1", WithAny<string>()), new NamedParameter("p2", WithAny<long>()),
				new NamedParameter("p3", WithAny<int>()), new NamedParameter("p4", WithAny<int>()),
				new NamedParameter("p5", WithAny<int>()));

			string result = setup.ToString();

			await That(result)
				.IsEqualTo(
					"void Foo(WithAny<string>() p1, WithAny<long>() p2, WithAny<int>() p3, WithAny<int>() p4, WithAny<int>() p5)");
		}

		[Fact]
		public async Task ToString_WithAnyParameterCombination_ShouldReturnMethodSignature()
		{
			VoidMethodSetup<string, long, int, int, int> setup = new("Foo", WithAnyParameters());

			string result = setup.ToString();

			await That(result).IsEqualTo("void Foo(WithAnyParameters())");
		}
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
		void MyMethodWithRefParameter(ref int value);
		void MyMethodWithOutParameter(out int value);
		string ToString();
		bool Equals(object? obj);
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
