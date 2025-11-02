using Mockolate.Exceptions;
using Mockolate.Interactions;
using Mockolate.Setup;

namespace Mockolate.Tests.MockMethods;

public sealed partial class SetupMethodTests
{
	public class ReturnMethodWith0Parameters
	{
		[Fact]
		public async Task Callback_ShouldExecuteWhenInvoked()
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method0()
				.Callback(() => { callCount++; })
				.Returns("a");

			sut.Subject.Method0();

			await That(callCount).IsEqualTo(1);
		}

		[Fact]
		public async Task Callback_ShouldNotExecuteWhenOtherMethodIsInvoked()
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method0().Callback(() => { callCount++; });

			sut.Subject.Method1(1);
			sut.Subject.Method0(false);

			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task GetReturnValue_InvalidType_ShouldThrowMockException()
		{
			int callCount = 0;
			MyReturnMethodSetup setup = new("foo");
			setup.Callback(() => { callCount++; }).Returns(3);
			MethodInvocation invocation = new(0, "foo", Array.Empty<object?>());

			void Act()
				=> setup.GetReturnValue<string>(invocation);

			await That(Act).Throws<MockException>()
				.WithMessage("""
				             The return callback only supports 'int' and not 'string'.
				             """);
			await That(callCount).IsEqualTo(0).Because("The callback should only be executed on success!");
		}

		[Fact]
		public async Task MixReturnsAndThrows_ShouldIterateThroughBoth()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method0()
				.Returns("d")
				.Throws(new Exception("foo"))
				.Returns(() => "b");

			string result1 = sut.Subject.Method0();
			Exception? result2 = Record.Exception(() => sut.Subject.Method0());
			string result3 = sut.Subject.Method0();

			await That(result1).IsEqualTo("d");
			await That(result2).HasMessage("foo");
			await That(result3).IsEqualTo("b");
		}

		[Fact]
		public async Task MultipleCallbacks_ShouldAllGetInvoked()
		{
			int callCount1 = 0;
			int callCount2 = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method0()
				.Callback(() => { callCount1++; })
				.Callback(() => { callCount2++; })
				.Returns("a");

			sut.Subject.Method0();
			sut.Subject.Method0();

			await That(callCount1).IsEqualTo(2);
			await That(callCount2).IsEqualTo(2);
		}

		[Fact]
		public async Task MultipleReturns_ShouldIterateThroughAllRegisteredValues()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method0()
				.Returns("d")
				.Returns("c")
				.Returns(() => "b");

			string[] result = new string[10];
			for (int i = 0; i < 10; i++)
			{
				result[i] = sut.Subject.Method0();
			}

			await That(result).IsEqualTo(["d", "c", "b", "d", "c", "b", "d", "c", "b", "d",]);
		}

		[Fact]
		public async Task Returns_Callback_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method0().Returns(() => "d");

			string result = sut.Subject.Method0();

			await That(result).IsEqualTo("d");
		}

		[Fact]
		public async Task Returns_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method0().Returns("d");

			string result = sut.Subject.Method0();

			await That(result).IsEqualTo("d");
		}

		[Fact]
		public async Task Returns_WithoutSetup_ShouldReturnDefault()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			string result = sut.Subject.Method0();

			await That(result).IsEmpty();
		}

		[Fact]
		public async Task SetOutParameter_ShouldReturnDefaultValue()
		{
			MyReturnMethodSetup setup = new("foo");

			string result = setup.SetOutParameter<string>("p0");

			await That(result).IsEmpty();
		}

		[Fact]
		public async Task SetRefParameter_ShouldReturnValue()
		{
			MyReturnMethodSetup setup = new("foo");

			string result = setup.SetRefParameter("p0", "d");

			await That(result).IsEqualTo("d");
		}

		[Fact]
		public async Task Throws_Callback_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method0().Throws(() => new Exception("foo"));

			void Act()
				=> sut.Subject.Method0();

			await That(Act).ThrowsException().WithMessage("foo");
		}

		[Fact]
		public async Task Throws_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method0().Throws(new Exception("foo"));

			void Act()
				=> sut.Subject.Method0();

			await That(Act).ThrowsException().WithMessage("foo");
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

		private class MyReturnMethodSetup(string name) : ReturnMethodSetup<int>(name)
		{
			public T SetOutParameter<T>(string parameterName)
				=> base.SetOutParameter<T>(parameterName, MockBehavior.Default);

			public T SetRefParameter<T>(string parameterName, T value)
				=> base.SetRefParameter(parameterName, value, MockBehavior.Default);

			public T GetReturnValue<T>(MethodInvocation invocation)
				=> base.GetReturnValue<T>(invocation, MockBehavior.Default);
		}
	}

	public class ReturnMethodWith1Parameters
	{
		[Fact]
		public async Task Callback_ShouldExecuteWhenInvoked()
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method1(With.Any<int>())
				.Callback(() => { callCount++; })
				.Returns("a");

			sut.Subject.Method1(3);

			await That(callCount).IsEqualTo(1);
		}

		[Fact]
		public async Task Callback_ShouldNotExecuteWhenOtherMethodIsInvoked()
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method1(With.Any<int>())
				.Callback(() => { callCount++; });

			sut.Subject.Method0();
			sut.Subject.Method1(2, false);

			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task Callback_ShouldNotExecuteWhenParameterDoesNotMatch()
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method1(With.Matching<int>(v => v != 1))
				.Callback(() => { callCount++; });

			sut.Subject.Method1(1);

			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task CallbackWithValue_ShouldExecuteWhenInvoked()
		{
			int callCount = 0;
			int receivedValue = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method1(With.Any<int>())
				.Callback(v =>
				{
					callCount++;
					receivedValue = v;
				});

			sut.Subject.Method1(3);

			await That(callCount).IsEqualTo(1);
			await That(receivedValue).IsEqualTo(3);
		}

		[Fact]
		public async Task CallbackWithValue_ShouldNotExecuteWhenOtherMethodIsInvoked()
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method1(With.Any<int>())
				.Callback(v => { callCount++; });

			sut.Subject.Method0();
			sut.Subject.Method1(2, false);

			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task CallbackWithValue_ShouldNotExecuteWhenParameterDoesNotMatch()
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method1(With.Matching<int>(v => v != 1))
				.Callback(v => { callCount++; });

			sut.Subject.Method1(1);

			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task GetReturnValue_InvalidInputType_ShouldThrowMockException()
		{
			MyReturnMethodSetup<int> setup = new("foo");
			setup.Returns(x => 3 * x);
			MethodInvocation invocation = new(0, "foo", ["b",]);

			void Act()
				=> setup.GetReturnValue<int>(invocation);

			await That(Act).Throws<MockException>()
				.WithMessage("""
				             The input parameter only supports 'int', but is 'string'.
				             """);
		}

		[Fact]
		public async Task GetReturnValue_InvalidType_ShouldThrowMockException()
		{
			int callCount = 0;
			MyReturnMethodSetup<int> setup = new("foo");
			setup.Callback(() => { callCount++; }).Returns(x => 3 * x);
			MethodInvocation invocation = new(0, "foo", [2,]);

			void Act()
				=> setup.GetReturnValue<string>(invocation);

			await That(Act).Throws<MockException>()
				.WithMessage("""
				             The return callback only supports 'int' and not 'string'.
				             """);
			await That(callCount).IsEqualTo(0).Because("The callback should only be executed on success!");
		}

		[Fact]
		public async Task MixReturnsAndThrows_ShouldIterateThroughBoth()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method1(With.Any<int>())
				.Returns("d")
				.Throws(new Exception("foo"))
				.Returns(() => "b");

			string result1 = sut.Subject.Method1(1);
			Exception? result2 = Record.Exception(() => sut.Subject.Method1(2));
			string result3 = sut.Subject.Method1(3);

			await That(result1).IsEqualTo("d");
			await That(result2).HasMessage("foo");
			await That(result3).IsEqualTo("b");
		}

		[Fact]
		public async Task MultipleCallbacks_ShouldAllGetInvoked()
		{
			int callCount1 = 0;
			int callCount2 = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method1(With.Any<int>())
				.Callback(() => { callCount1++; })
				.Callback(v => { callCount2 += v; })
				.Returns("a");

			sut.Subject.Method1(1);
			sut.Subject.Method1(2);

			await That(callCount1).IsEqualTo(2);
			await That(callCount2).IsEqualTo(3);
		}

		[Fact]
		public async Task MultipleReturns_ShouldIterateThroughAllRegisteredValues()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method1(With.Any<int>())
				.Returns("d")
				.Returns(() => "c")
				.Returns(v => $"foo-{v}");

			string[] result = new string[10];
			for (int i = 0; i < 10; i++)
			{
				result[i] = sut.Subject.Method1(i);
			}

			await That(result).IsEqualTo(["d", "c", "foo-2", "d", "c", "foo-5", "d", "c", "foo-8", "d",]);
		}

		[Fact]
		public async Task OutParameter_ShouldSet()
		{
			int receivedValue = 0;
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method1WithOutParameter(With.Out(() => 3))
				.Callback(v =>
				{
					callCount++;
					receivedValue = v;
				});

			sut.Subject.Method1WithOutParameter(out int value);

			await That(callCount).IsEqualTo(1);
			await That(value).IsEqualTo(3);
			await That(receivedValue).IsEqualTo(0);
		}

		[Fact]
		public async Task RefParameter_ShouldSet()
		{
			int receivedValue = 0;
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method1WithRefParameter(With.Ref<int>(v => 3))
				.Callback(v =>
				{
					callCount++;
					receivedValue = v;
				});

			int value = 2;
			sut.Subject.Method1WithRefParameter(ref value);

			await That(callCount).IsEqualTo(1);
			await That(value).IsEqualTo(3);
			await That(receivedValue).IsEqualTo(2);
		}

		[Fact]
		public async Task Returns_Callback_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method1(With.Any<int>()).Returns(() => "d");

			string result = sut.Subject.Method1(3);

			await That(result).IsEqualTo("d");
		}

		[Fact]
		public async Task Returns_CallbackWithValue_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method1(With.Any<int>()).Returns(x => $"foo-{x}");

			string result = sut.Subject.Method1(3);

			await That(result).IsEqualTo("foo-3");
		}

		[Fact]
		public async Task Returns_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method1(With.Any<int>()).Returns("d");

			string result = sut.Subject.Method1(3);

			await That(result).IsEqualTo("d");
		}

		[Fact]
		public async Task Returns_WithoutSetup_ShouldReturnDefault()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			string result = sut.Subject.Method1(3);

			await That(result).IsEmpty();
		}

		[Fact]
		public async Task SetOutParameter_ShouldReturnDefaultValue()
		{
			MyReturnMethodSetup<int> setup = new("foo");

			string result = setup.SetOutParameter<string>("p1");

			await That(result).IsEmpty();
		}

		[Fact]
		public async Task SetRefParameter_ShouldReturnValue()
		{
			MyReturnMethodSetup<int> setup = new("foo");

			string result = setup.SetRefParameter("p1", "d");

			await That(result).IsEqualTo("d");
		}

		[Fact]
		public async Task Throws_Callback_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method1(With.Any<int>())
				.Throws(() => new Exception("foo"));

			void Act()
				=> sut.Subject.Method1(1);

			await That(Act).ThrowsException().WithMessage("foo");
		}

		[Fact]
		public async Task Throws_CallbackWithValue_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method1(With.Any<int>())
				.Throws(v1 => new Exception("foo-" + v1));

			void Act()
				=> sut.Subject.Method1(42);

			await That(Act).ThrowsException().WithMessage("foo-42");
		}

		[Fact]
		public async Task Throws_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method1(With.Any<int>())
				.Throws(new Exception("foo"));

			void Act()
				=> sut.Subject.Method1(1);

			await That(Act).ThrowsException().WithMessage("foo");
		}

		[Fact]
		public async Task WhenSetupWithNull_ShouldReturnDefaultValue()
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method1(With.Any<int>())
				.Callback(() => { callCount++; })
				.Returns((string?)null!);

			string result = sut.Subject.Method1(1);

			await That(callCount).IsEqualTo(1);
			await That(result).IsNull();
		}

		private class MyReturnMethodSetup<T1>(string name)
			: ReturnMethodSetup<int, T1>(name, new With.NamedParameter("p1", With.Matching<T1>(_ => false)))
		{
			public T SetOutParameter<T>(string parameterName)
				=> base.SetOutParameter<T>(parameterName, MockBehavior.Default);

			public T SetRefParameter<T>(string parameterName, T value)
				=> base.SetRefParameter(parameterName, value, MockBehavior.Default);

			public T GetReturnValue<T>(MethodInvocation invocation)
				=> base.GetReturnValue<T>(invocation, MockBehavior.Default);
		}
	}

	public class ReturnMethodWith2Parameters
	{
		[Fact]
		public async Task Callback_ShouldExecuteWhenInvoked()
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method2(With.Any<int>(), With.Any<int>())
				.Callback(() => { callCount++; })
				.Returns("a");

			sut.Subject.Method2(1, 2);

			await That(callCount).IsEqualTo(1);
		}

		[Theory]
		[InlineData(false, false)]
		[InlineData(true, false)]
		[InlineData(false, true)]
		public async Task Callback_ShouldNotExecuteWhenAnyParameterDoesNotMatch(bool isMatch1, bool isMatch2)
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method2(With.Matching<int>(v => isMatch1), With.Matching<int>(v => isMatch2))
				.Callback(() => { callCount++; });

			sut.Subject.Method2(1, 2);

			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task Callback_ShouldNotExecuteWhenOtherMethodIsInvoked()
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method2(With.Any<int>(), With.Any<int>())
				.Callback(() => { callCount++; });

			sut.Subject.Method1(1);
			sut.Subject.Method2(1, 2, false);

			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task CallbackWithValue_ShouldExecuteWhenInvoked()
		{
			int callCount = 0;
			int receivedValue1 = 0;
			int receivedValue2 = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method2(With.Any<int>(), With.Any<int>())
				.Callback((v1, v2) =>
				{
					callCount++;
					receivedValue1 = v1;
					receivedValue2 = v2;
				});

			sut.Subject.Method2(2, 4);

			await That(callCount).IsEqualTo(1);
			await That(receivedValue1).IsEqualTo(2);
			await That(receivedValue2).IsEqualTo(4);
		}

		[Theory]
		[InlineData(false, false)]
		[InlineData(true, false)]
		[InlineData(false, true)]
		public async Task CallbackWithValue_ShouldNotExecuteWhenAnyParameterDoesNotMatch(bool isMatch1, bool isMatch2)
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method2(With.Matching<int>(v => isMatch1), With.Matching<int>(v => isMatch2))
				.Callback((v1, v2) => { callCount++; });

			sut.Subject.Method2(1, 2);

			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task CallbackWithValue_ShouldNotExecuteWhenOtherMethodIsInvoked()
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method2(With.Any<int>(), With.Any<int>())
				.Callback((v1, v2) => { callCount++; });

			sut.Subject.Method1(1);
			sut.Subject.Method2(1, 2, false);

			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task GetReturnValue_InvalidInputType1_ShouldThrowMockException()
		{
			MyReturnMethodSetup<int, int> setup = new("foo");
			setup.Returns((x, y) => 3 * x);
			MethodInvocation invocation = new(0, "foo", ["a", 2,]);

			void Act()
				=> setup.GetReturnValue<int>(invocation);

			await That(Act).Throws<MockException>()
				.WithMessage("""
				             The input parameter 1 only supports 'int', but is 'string'.
				             """);
		}

		[Fact]
		public async Task GetReturnValue_InvalidInputType2_ShouldThrowMockException()
		{
			MyReturnMethodSetup<int, int> setup = new("foo");
			setup.Returns((x, y) => 3 * x);
			MethodInvocation invocation = new(0, "foo", [1, "b",]);

			void Act()
				=> setup.GetReturnValue<int>(invocation);

			await That(Act).Throws<MockException>()
				.WithMessage("""
				             The input parameter 2 only supports 'int', but is 'string'.
				             """);
		}

		[Fact]
		public async Task GetReturnValue_InvalidType_ShouldThrowMockException()
		{
			int callCount = 0;
			MyReturnMethodSetup<int, int> setup = new("foo");
			setup.Callback(() => { callCount++; }).Returns(3);
			MethodInvocation invocation = new(0, "foo", [2, 3,]);

			void Act()
				=> setup.GetReturnValue<string>(invocation);

			await That(Act).Throws<MockException>()
				.WithMessage("""
				             The return callback only supports 'int' and not 'string'.
				             """);
			await That(callCount).IsEqualTo(0).Because("The callback should only be executed on success!");
		}

		[Fact]
		public async Task MixReturnsAndThrows_ShouldIterateThroughBoth()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method2(With.Any<int>(), With.Any<int>())
				.Returns("d")
				.Throws(new Exception("foo"))
				.Returns(() => "b");

			string result1 = sut.Subject.Method2(1, 2);
			Exception? result2 = Record.Exception(() => sut.Subject.Method2(2, 3));
			string result3 = sut.Subject.Method2(3, 4);

			await That(result1).IsEqualTo("d");
			await That(result2).HasMessage("foo");
			await That(result3).IsEqualTo("b");
		}

		[Fact]
		public async Task MultipleCallbacks_ShouldAllGetInvoked()
		{
			int callCount1 = 0;
			int callCount2 = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method2(With.Any<int>(), With.Any<int>())
				.Callback(() => { callCount1++; })
				.Callback((v1, v2) => { callCount2 += v1 * v2; })
				.Returns("a");

			sut.Subject.Method2(1, 2);
			sut.Subject.Method2(2, 2);

			await That(callCount1).IsEqualTo(2);
			await That(callCount2).IsEqualTo(6);
		}

		[Fact]
		public async Task MultipleReturns_ShouldIterateThroughAllRegisteredValues()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method2(With.Any<int>(), With.Any<int>())
				.Returns("d")
				.Returns(() => "c")
				.Returns((v1, v2) => $"foo-{v1}-{v2}");

			string[] result = new string[10];
			for (int i = 0; i < 10; i++)
			{
				result[i] = sut.Subject.Method2(i, 2 * i);
			}

			await That(result).IsEqualTo(["d", "c", "foo-2-4", "d", "c", "foo-5-10", "d", "c", "foo-8-16", "d",]);
		}

		[Fact]
		public async Task OutParameter_ShouldSet()
		{
			int receivedValue1 = 0;
			int receivedValue2 = 0;
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method2WithOutParameter(With.Out(() => 2), With.Out(() => 4))
				.Callback((v1, v2) =>
				{
					callCount++;
					receivedValue1 = v1;
					receivedValue2 = v2;
				});

			sut.Subject.Method2WithOutParameter(out int value1, out int value2);

			await That(callCount).IsEqualTo(1);
			await That(value1).IsEqualTo(2);
			await That(receivedValue1).IsEqualTo(0);
			await That(value2).IsEqualTo(4);
			await That(receivedValue2).IsEqualTo(0);
		}

		[Fact]
		public async Task RefParameter_ShouldSet()
		{
			int receivedValue1 = 0;
			int receivedValue2 = 0;
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method2WithRefParameter(With.Ref<int>(v => v * 10), With.Ref<int>(v => v * 10))
				.Callback((v1, v2) =>
				{
					callCount++;
					receivedValue1 = v1;
					receivedValue2 = v2;
				});

			int value1 = 2;
			int value2 = 4;
			sut.Subject.Method2WithRefParameter(ref value1, ref value2);

			await That(callCount).IsEqualTo(1);
			await That(value1).IsEqualTo(20);
			await That(receivedValue1).IsEqualTo(2);
			await That(value2).IsEqualTo(40);
			await That(receivedValue2).IsEqualTo(4);
		}

		[Fact]
		public async Task Returns_Callback_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method2(With.Any<int>(), With.Any<int>()).Returns(() => "d");

			string result = sut.Subject.Method2(2, 3);

			await That(result).IsEqualTo("d");
		}

		[Fact]
		public async Task Returns_CallbackWithValue_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method2(With.Any<int>(), With.Any<int>()).Returns((x, y) => $"foo-{x}-{y}");

			string result = sut.Subject.Method2(2, 3);

			await That(result).IsEqualTo("foo-2-3");
		}

		[Fact]
		public async Task Returns_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method2(With.Any<int>(), With.Any<int>()).Returns("d");

			string result = sut.Subject.Method2(2, 3);

			await That(result).IsEqualTo("d");
		}

		[Fact]
		public async Task Returns_WithoutSetup_ShouldReturnDefault()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			string result = sut.Subject.Method2(2, 3);

			await That(result).IsEmpty();
		}

		[Fact]
		public async Task SetOutParameter_ShouldReturnDefaultValue()
		{
			MyReturnMethodSetup<int, int> setup = new("foo");

			string result = setup.SetOutParameter<string>("p1");

			await That(result).IsEmpty();
		}

		[Fact]
		public async Task SetRefParameter_ShouldReturnValue()
		{
			MyReturnMethodSetup<int, int> setup = new("foo");

			string result = setup.SetRefParameter("p1", "d");

			await That(result).IsEqualTo("d");
		}

		[Fact]
		public async Task Throws_Callback_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method2(With.Any<int>(), With.Any<int>())
				.Throws(() => new Exception("foo"));

			void Act()
				=> sut.Subject.Method2(1, 2);

			await That(Act).ThrowsException().WithMessage("foo");
		}

		[Fact]
		public async Task Throws_CallbackWithValue_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method2(With.Any<int>(), With.Any<int>())
				.Throws((v1, v2) => new Exception($"foo-{v1}-{v2}"));

			void Act()
				=> sut.Subject.Method2(1, 2);

			await That(Act).ThrowsException().WithMessage("foo-1-2");
		}

		[Fact]
		public async Task Throws_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method2(With.Any<int>(), With.Any<int>())
				.Throws(new Exception("foo"));

			void Act()
				=> sut.Subject.Method2(1, 2);

			await That(Act).ThrowsException().WithMessage("foo");
		}

		[Fact]
		public async Task WhenSetupWithNull_ShouldReturnDefaultValue()
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method2(With.Any<int>(), With.Any<int>())
				.Callback(() => { callCount++; })
				.Returns((string?)null!);

			string result = sut.Subject.Method2(1, 2);

			await That(callCount).IsEqualTo(1);
			await That(result).IsNull();
		}

		private class MyReturnMethodSetup<T1, T2>(string name)
			: ReturnMethodSetup<int, T1, T2>(name,
				new With.NamedParameter("p1", With.Matching<T1>(_ => false)),
				new With.NamedParameter("p2", With.Matching<T2>(_ => false)))
		{
			public T SetOutParameter<T>(string parameterName)
				=> base.SetOutParameter<T>(parameterName, MockBehavior.Default);

			public T SetRefParameter<T>(string parameterName, T value)
				=> base.SetRefParameter(parameterName, value, MockBehavior.Default);

			public T GetReturnValue<T>(MethodInvocation invocation)
				=> base.GetReturnValue<T>(invocation, MockBehavior.Default);
		}
	}

	public class ReturnMethodWith3Parameters
	{
		[Fact]
		public async Task Callback_ShouldExecuteWhenInvoked()
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method3(With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Callback(() => { callCount++; })
				.Returns("a");

			sut.Subject.Method3(1, 2, 3);

			await That(callCount).IsEqualTo(1);
		}

		[Theory]
		[InlineData(false, false, false)]
		[InlineData(true, true, false)]
		[InlineData(true, false, true)]
		[InlineData(false, true, true)]
		public async Task Callback_ShouldNotExecuteWhenAnyParameterDoesNotMatch(bool isMatch1, bool isMatch2,
			bool isMatch3)
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method3(With.Matching<int>(v => isMatch1), With.Matching<int>(v => isMatch2),
					With.Matching<int>(v => isMatch3))
				.Callback(() => { callCount++; });

			sut.Subject.Method3(1, 2, 3);

			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task Callback_ShouldNotExecuteWhenOtherMethodIsInvoked()
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method3(With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Callback(() => { callCount++; });

			sut.Subject.Method2(1, 2);
			sut.Subject.Method3(1, 2, 3, false);

			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task CallbackWithValue_ShouldExecuteWhenInvoked()
		{
			int callCount = 0;
			int receivedValue1 = 0;
			int receivedValue2 = 0;
			int receivedValue3 = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method3(With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Callback((v1, v2, v3) =>
				{
					callCount++;
					receivedValue1 = v1;
					receivedValue2 = v2;
					receivedValue3 = v3;
				});

			sut.Subject.Method3(2, 4, 6);

			await That(callCount).IsEqualTo(1);
			await That(receivedValue1).IsEqualTo(2);
			await That(receivedValue2).IsEqualTo(4);
			await That(receivedValue3).IsEqualTo(6);
		}

		[Theory]
		[InlineData(false, false, false)]
		[InlineData(true, true, false)]
		[InlineData(true, false, true)]
		[InlineData(false, true, true)]
		public async Task CallbackWithValue_ShouldNotExecuteWhenAnyParameterDoesNotMatch(bool isMatch1, bool isMatch2,
			bool isMatch3)
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method3(With.Matching<int>(v => isMatch1), With.Matching<int>(v => isMatch2),
					With.Matching<int>(v => isMatch3))
				.Callback((v1, v2, v3) => { callCount++; });

			sut.Subject.Method3(1, 2, 3);

			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task CallbackWithValue_ShouldNotExecuteWhenOtherMethodIsInvoked()
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method3(With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Callback((v1, v2, v3) => { callCount++; });

			sut.Subject.Method2(1, 2);
			sut.Subject.Method3(1, 2, 3, false);

			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task GetReturnValue_InvalidInputType1_ShouldThrowMockException()
		{
			MyReturnMethodSetup<int, int, int> setup = new("foo");
			setup.Returns((x, y, z) => 3 * x);
			MethodInvocation invocation = new(0, "foo", ["a", 2, 3,]);

			void Act()
				=> setup.GetReturnValue<int>(invocation);

			await That(Act).Throws<MockException>()
				.WithMessage("""
				             The input parameter 1 only supports 'int', but is 'string'.
				             """);
		}

		[Fact]
		public async Task GetReturnValue_InvalidInputType2_ShouldThrowMockException()
		{
			MyReturnMethodSetup<int, int, int> setup = new("foo");
			setup.Returns((x, y, z) => 3 * x);
			MethodInvocation invocation = new(0, "foo", [1, "b", 3,]);

			void Act()
				=> setup.GetReturnValue<int>(invocation);

			await That(Act).Throws<MockException>()
				.WithMessage("""
				             The input parameter 2 only supports 'int', but is 'string'.
				             """);
		}

		[Fact]
		public async Task GetReturnValue_InvalidInputType3_ShouldThrowMockException()
		{
			MyReturnMethodSetup<int, int, int> setup = new("foo");
			setup.Returns((x, y, z) => 3 * x);
			MethodInvocation invocation = new(0, "foo", [1, 2, "c",]);

			void Act()
				=> setup.GetReturnValue<int>(invocation);

			await That(Act).Throws<MockException>()
				.WithMessage("""
				             The input parameter 3 only supports 'int', but is 'string'.
				             """);
		}

		[Fact]
		public async Task GetReturnValue_InvalidType_ShouldThrowMockException()
		{
			int callCount = 0;
			MyReturnMethodSetup<int, int, int> setup = new("foo");
			setup.Callback(() => { callCount++; }).Returns(3);
			MethodInvocation invocation = new(0, "foo", [1, 2, 3,]);

			void Act()
				=> setup.GetReturnValue<string>(invocation);

			await That(Act).Throws<MockException>()
				.WithMessage("""
				             The return callback only supports 'int' and not 'string'.
				             """);
			await That(callCount).IsEqualTo(0).Because("The callback should only be executed on success!");
		}

		[Fact]
		public async Task MixReturnsAndThrows_ShouldIterateThroughBoth()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method3(With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Returns("d")
				.Throws(new Exception("foo"))
				.Returns(() => "b");

			string result1 = sut.Subject.Method3(1, 2, 3);
			Exception? result2 = Record.Exception(() => sut.Subject.Method3(2, 3, 4));
			string result3 = sut.Subject.Method3(3, 4, 5);

			await That(result1).IsEqualTo("d");
			await That(result2).HasMessage("foo");
			await That(result3).IsEqualTo("b");
		}

		[Fact]
		public async Task MultipleCallbacks_ShouldAllGetInvoked()
		{
			int callCount1 = 0;
			int callCount2 = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method3(With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Callback(() => { callCount1++; })
				.Callback((v1, v2, v3) => { callCount2 += v1 * v2 * v3; })
				.Returns("a");

			sut.Subject.Method3(1, 2, 3);
			sut.Subject.Method3(2, 2, 3);

			await That(callCount1).IsEqualTo(2);
			await That(callCount2).IsEqualTo(18);
		}

		[Fact]
		public async Task MultipleReturns_ShouldIterateThroughAllRegisteredValues()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method3(With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Returns("d")
				.Returns(() => "c")
				.Returns((v1, v2, v3) => $"foo-{v1}-{v2}-{v3}");

			string[] result = new string[10];
			for (int i = 0; i < 10; i++)
			{
				result[i] = sut.Subject.Method3(i, 2 * i, 3 * i);
			}

			await That(result)
				.IsEqualTo(["d", "c", "foo-2-4-6", "d", "c", "foo-5-10-15", "d", "c", "foo-8-16-24", "d",]);
		}

		[Fact]
		public async Task OutParameter_ShouldSet()
		{
			int receivedValue1 = 0;
			int receivedValue2 = 0;
			int receivedValue3 = 0;
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method3WithOutParameter(With.Out(() => 2), With.Out(() => 4), With.Out(() => 6))
				.Callback((v1, v2, v3) =>
				{
					callCount++;
					receivedValue1 = v1;
					receivedValue2 = v2;
					receivedValue3 = v3;
				});

			sut.Subject.Method3WithOutParameter(out int value1, out int value2, out int value3);

			await That(callCount).IsEqualTo(1);
			await That(value1).IsEqualTo(2);
			await That(receivedValue1).IsEqualTo(0);
			await That(value2).IsEqualTo(4);
			await That(receivedValue2).IsEqualTo(0);
			await That(value3).IsEqualTo(6);
			await That(receivedValue3).IsEqualTo(0);
		}

		[Fact]
		public async Task RefParameter_ShouldSet()
		{
			int receivedValue1 = 0;
			int receivedValue2 = 0;
			int receivedValue3 = 0;
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method3WithRefParameter(With.Ref<int>(v => v * 10), With.Ref<int>(v => v * 10),
					With.Ref<int>(v => v * 10))
				.Callback((v1, v2, v3) =>
				{
					callCount++;
					receivedValue1 = v1;
					receivedValue2 = v2;
					receivedValue3 = v3;
				});

			int value1 = 2;
			int value2 = 4;
			int value3 = 6;
			sut.Subject.Method3WithRefParameter(ref value1, ref value2, ref value3);

			await That(callCount).IsEqualTo(1);
			await That(value1).IsEqualTo(20);
			await That(receivedValue1).IsEqualTo(2);
			await That(value2).IsEqualTo(40);
			await That(receivedValue2).IsEqualTo(4);
			await That(value3).IsEqualTo(60);
			await That(receivedValue3).IsEqualTo(6);
		}

		[Fact]
		public async Task Returns_Callback_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method3(With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Returns(() => "d");

			string result = sut.Subject.Method3(1, 2, 3);

			await That(result).IsEqualTo("d");
		}

		[Fact]
		public async Task Returns_CallbackWithValue_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method3(With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Returns((x, y, z) => $"foo-{x}-{y}-{z}");

			string result = sut.Subject.Method3(2, 3, 4);

			await That(result).IsEqualTo("foo-2-3-4");
		}

		[Fact]
		public async Task Returns_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method3(With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Returns("d");

			string result = sut.Subject.Method3(1, 2, 3);

			await That(result).IsEqualTo("d");
		}

		[Fact]
		public async Task Returns_WithoutSetup_ShouldReturnDefault()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			string result = sut.Subject.Method3(2, 3, 4);

			await That(result).IsEmpty();
		}

		[Fact]
		public async Task SetOutParameter_ShouldReturnDefaultValue()
		{
			MyReturnMethodSetup<int, int, int> setup = new("foo");

			string result = setup.SetOutParameter<string>("p1");

			await That(result).IsEmpty();
		}

		[Fact]
		public async Task SetRefParameter_ShouldReturnValue()
		{
			MyReturnMethodSetup<int, int, int> setup = new("foo");

			string result = setup.SetRefParameter("p1", "d");

			await That(result).IsEqualTo("d");
		}

		[Fact]
		public async Task Throws_Callback_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method3(With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Throws(() => new Exception("foo"));

			void Act()
				=> sut.Subject.Method3(1, 2, 3);

			await That(Act).ThrowsException().WithMessage("foo");
		}

		[Fact]
		public async Task Throws_CallbackWithValue_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method3(With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Throws((v1, v2, v3) => new Exception($"foo-{v1}-{v2}-{v3}"));

			void Act()
				=> sut.Subject.Method3(1, 2, 3);

			await That(Act).ThrowsException().WithMessage("foo-1-2-3");
		}

		[Fact]
		public async Task Throws_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method3(With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Throws(new Exception("foo"));

			void Act()
				=> sut.Subject.Method3(1, 2, 3);

			await That(Act).ThrowsException().WithMessage("foo");
		}

		[Fact]
		public async Task WhenSetupWithNull_ShouldReturnDefaultValue()
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method3(With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Callback(() => { callCount++; })
				.Returns((string?)null!);

			string result = sut.Subject.Method3(1, 2, 3);

			await That(callCount).IsEqualTo(1);
			await That(result).IsNull();
		}

		private class MyReturnMethodSetup<T1, T2, T3>(string name)
			: ReturnMethodSetup<int, T1, T2, T3>(name,
				new With.NamedParameter("p1", With.Matching<T1>(_ => false)),
				new With.NamedParameter("p2", With.Matching<T2>(_ => false)),
				new With.NamedParameter("p3", With.Matching<T3>(_ => false)))
		{
			public T SetOutParameter<T>(string parameterName)
				=> base.SetOutParameter<T>(parameterName, MockBehavior.Default);

			public T SetRefParameter<T>(string parameterName, T value)
				=> base.SetRefParameter(parameterName, value, MockBehavior.Default);

			public T GetReturnValue<T>(MethodInvocation invocation)
				=> base.GetReturnValue<T>(invocation, MockBehavior.Default);
		}
	}

	public class ReturnMethodWith4Parameters
	{
		[Fact]
		public async Task Callback_ShouldExecuteWhenInvoked()
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Callback(() => { callCount++; })
				.Returns("a");

			sut.Subject.Method4(1, 2, 3, 4);

			await That(callCount).IsEqualTo(1);
		}

		[Theory]
		[InlineData(false, false, false, false)]
		[InlineData(true, true, true, false)]
		[InlineData(true, true, false, true)]
		[InlineData(true, false, true, true)]
		[InlineData(false, true, true, true)]
		public async Task Callback_ShouldNotExecuteWhenAnyParameterDoesNotMatch(bool isMatch1, bool isMatch2,
			bool isMatch3, bool isMatch4)
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method4(With.Matching<int>(v => isMatch1), With.Matching<int>(v => isMatch2),
					With.Matching<int>(v => isMatch3), With.Matching<int>(v => isMatch4))
				.Callback(() => { callCount++; });

			sut.Subject.Method4(1, 2, 3, 4);

			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task Callback_ShouldNotExecuteWhenOtherMethodIsInvoked()
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Callback(() => { callCount++; });

			sut.Subject.Method3(1, 2, 3);
			sut.Subject.Method4(1, 2, 3, false);

			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task CallbackWithValue_ShouldExecuteWhenInvoked()
		{
			int callCount = 0;
			int receivedValue1 = 0;
			int receivedValue2 = 0;
			int receivedValue3 = 0;
			int receivedValue4 = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Callback((v1, v2, v3, v4) =>
				{
					callCount++;
					receivedValue1 = v1;
					receivedValue2 = v2;
					receivedValue3 = v3;
					receivedValue4 = v4;
				});

			sut.Subject.Method4(2, 4, 6, 8);

			await That(callCount).IsEqualTo(1);
			await That(receivedValue1).IsEqualTo(2);
			await That(receivedValue2).IsEqualTo(4);
			await That(receivedValue3).IsEqualTo(6);
			await That(receivedValue4).IsEqualTo(8);
		}

		[Theory]
		[InlineData(false, false, false, false)]
		[InlineData(true, true, true, false)]
		[InlineData(true, true, false, true)]
		[InlineData(true, false, true, true)]
		[InlineData(false, true, true, true)]
		public async Task CallbackWithValue_ShouldNotExecuteWhenAnyParameterDoesNotMatch(bool isMatch1, bool isMatch2,
			bool isMatch3, bool isMatch4)
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method4(With.Matching<int>(v => isMatch1), With.Matching<int>(v => isMatch2),
					With.Matching<int>(v => isMatch3), With.Matching<int>(v => isMatch4))
				.Callback((v1, v2, v3, v4) => { callCount++; });

			sut.Subject.Method4(1, 2, 3, 4);

			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task CallbackWithValue_ShouldNotExecuteWhenOtherMethodIsInvoked()
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Callback((v1, v2, v3, v4) => { callCount++; });

			sut.Subject.Method3(1, 2, 3);
			sut.Subject.Method4(1, 2, 3, false);

			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task GetReturnValue_InvalidInputType1_ShouldThrowMockException()
		{
			MyReturnMethodSetup<int, int, int, int> setup = new("foo");
			setup.Returns((v1, v2, v3, v4) => 3 * v1 * v2 * v3 * v4);
			MethodInvocation invocation = new(0, "foo", ["a", 2, 3, 4,]);

			void Act()
				=> setup.GetReturnValue<int>(invocation);

			await That(Act).Throws<MockException>()
				.WithMessage("""
				             The input parameter 1 only supports 'int', but is 'string'.
				             """);
		}

		[Fact]
		public async Task GetReturnValue_InvalidInputType2_ShouldThrowMockException()
		{
			MyReturnMethodSetup<int, int, int, int> setup = new("foo");
			setup.Returns((v1, v2, v3, v4) => 3 * v1 * v2 * v3 * v4);
			MethodInvocation invocation = new(0, "foo", [1, "b", 3, 4,]);

			void Act()
				=> setup.GetReturnValue<int>(invocation);

			await That(Act).Throws<MockException>()
				.WithMessage("""
				             The input parameter 2 only supports 'int', but is 'string'.
				             """);
		}

		[Fact]
		public async Task GetReturnValue_InvalidInputType3_ShouldThrowMockException()
		{
			MyReturnMethodSetup<int, int, int, int> setup = new("foo");
			setup.Returns((v1, v2, v3, v4) => 3 * v1 * v2 * v3 * v4);
			MethodInvocation invocation = new(0, "foo", [1, 2, "c", 4,]);

			void Act()
				=> setup.GetReturnValue<int>(invocation);

			await That(Act).Throws<MockException>()
				.WithMessage("""
				             The input parameter 3 only supports 'int', but is 'string'.
				             """);
		}

		[Fact]
		public async Task GetReturnValue_InvalidInputType4_ShouldThrowMockException()
		{
			MyReturnMethodSetup<int, int, int, int> setup = new("foo");
			setup.Returns((v1, v2, v3, v4) => 3 * v1 * v2 * v3 * v4);
			MethodInvocation invocation = new(0, "foo", [1, 2, 3, "d",]);

			void Act()
				=> setup.GetReturnValue<int>(invocation);

			await That(Act).Throws<MockException>()
				.WithMessage("""
				             The input parameter 4 only supports 'int', but is 'string'.
				             """);
		}

		[Fact]
		public async Task GetReturnValue_InvalidType_ShouldThrowMockException()
		{
			int callCount = 0;
			MyReturnMethodSetup<int, int, int, int> setup = new("foo");
			setup.Callback(() => { callCount++; }).Returns(3);
			MethodInvocation invocation = new(0, "foo", [1, 2, 3, 4,]);

			void Act()
				=> setup.GetReturnValue<string>(invocation);

			await That(Act).Throws<MockException>()
				.WithMessage("""
				             The return callback only supports 'int' and not 'string'.
				             """);
			await That(callCount).IsEqualTo(0).Because("The callback should only be executed on success!");
		}

		[Fact]
		public async Task MixReturnsAndThrows_ShouldIterateThroughBoth()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Returns("d")
				.Throws(new Exception("foo"))
				.Returns(() => "b");

			string result1 = sut.Subject.Method4(1, 2, 3, 4);
			Exception? result2 = Record.Exception(() => sut.Subject.Method4(2, 3, 4, 5));
			string result3 = sut.Subject.Method4(3, 4, 5, 6);

			await That(result1).IsEqualTo("d");
			await That(result2).HasMessage("foo");
			await That(result3).IsEqualTo("b");
		}

		[Fact]
		public async Task MultipleCallbacks_ShouldAllGetInvoked()
		{
			int callCount1 = 0;
			int callCount2 = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Callback(() => { callCount1++; })
				.Callback((v1, v2, v3, v4) => { callCount2 += v1 * v2 * v3 * v4; })
				.Returns("a");

			sut.Subject.Method4(1, 2, 3, 4);
			sut.Subject.Method4(2, 2, 3, 4);

			await That(callCount1).IsEqualTo(2);
			await That(callCount2).IsEqualTo(72);
		}

		[Fact]
		public async Task MultipleReturns_ShouldIterateThroughAllRegisteredValues()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Returns("d")
				.Returns(() => "c")
				.Returns((v1, v2, v3, v4) => $"foo-{v1}-{v2}-{v3}-{v4}");

			string[] result = new string[10];
			for (int i = 0; i < 10; i++)
			{
				result[i] = sut.Subject.Method4(i, 2 * i, 3 * i, 4 * i);
			}

			await That(result).IsEqualTo([
				"d", "c", "foo-2-4-6-8", "d", "c", "foo-5-10-15-20", "d", "c", "foo-8-16-24-32", "d",
			]);
		}

		[Fact]
		public async Task OutParameter_ShouldSet()
		{
			int receivedValue1 = 0;
			int receivedValue2 = 0;
			int receivedValue3 = 0;
			int receivedValue4 = 0;
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method4WithOutParameter(With.Out(() => 2), With.Out(() => 4), With.Out(() => 6),
					With.Out(() => 8))
				.Callback((v1, v2, v3, v4) =>
				{
					callCount++;
					receivedValue1 = v1;
					receivedValue2 = v2;
					receivedValue3 = v3;
					receivedValue4 = v4;
				});

			sut.Subject.Method4WithOutParameter(out int value1, out int value2, out int value3, out int value4);

			await That(callCount).IsEqualTo(1);
			await That(value1).IsEqualTo(2);
			await That(receivedValue1).IsEqualTo(0);
			await That(value2).IsEqualTo(4);
			await That(receivedValue2).IsEqualTo(0);
			await That(value3).IsEqualTo(6);
			await That(receivedValue3).IsEqualTo(0);
			await That(value4).IsEqualTo(8);
			await That(receivedValue4).IsEqualTo(0);
		}

		[Fact]
		public async Task RefParameter_ShouldSet()
		{
			int receivedValue1 = 0;
			int receivedValue2 = 0;
			int receivedValue3 = 0;
			int receivedValue4 = 0;
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method4WithRefParameter(With.Ref<int>(v => v * 10), With.Ref<int>(v => v * 10),
					With.Ref<int>(v => v * 10), With.Ref<int>(v => v * 10))
				.Callback((v1, v2, v3, v4) =>
				{
					callCount++;
					receivedValue1 = v1;
					receivedValue2 = v2;
					receivedValue3 = v3;
					receivedValue4 = v4;
				});

			int value1 = 2;
			int value2 = 4;
			int value3 = 6;
			int value4 = 8;
			sut.Subject.Method4WithRefParameter(ref value1, ref value2, ref value3, ref value4);

			await That(callCount).IsEqualTo(1);
			await That(value1).IsEqualTo(20);
			await That(receivedValue1).IsEqualTo(2);
			await That(value2).IsEqualTo(40);
			await That(receivedValue2).IsEqualTo(4);
			await That(value3).IsEqualTo(60);
			await That(receivedValue3).IsEqualTo(6);
			await That(value4).IsEqualTo(80);
			await That(receivedValue4).IsEqualTo(8);
		}

		[Fact]
		public async Task Returns_Callback_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Returns(() => "d");

			string result = sut.Subject.Method4(1, 2, 3, 4);

			await That(result).IsEqualTo("d");
		}

		[Fact]
		public async Task Returns_CallbackWithValue_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Returns((x, y, z, a) => $"foo-{x}-{y}-{z}-{a}");

			string result = sut.Subject.Method4(2, 3, 4, 5);

			await That(result).IsEqualTo("foo-2-3-4-5");
		}

		[Fact]
		public async Task Returns_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Returns("d");

			string result = sut.Subject.Method4(1, 2, 3, 4);

			await That(result).IsEqualTo("d");
		}

		[Fact]
		public async Task Returns_WithoutSetup_ShouldReturnDefault()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			string result = sut.Subject.Method4(2, 3, 4, 5);

			await That(result).IsEmpty();
		}

		[Fact]
		public async Task SetOutParameter_ShouldReturnDefaultValue()
		{
			MyReturnMethodSetup<int, int, int, int> setup = new("foo");

			string result = setup.SetOutParameter<string>("p1");

			await That(result).IsEmpty();
		}

		[Fact]
		public async Task SetRefParameter_ShouldReturnValue()
		{
			MyReturnMethodSetup<int, int, int, int> setup = new("foo");

			string result = setup.SetRefParameter("p1", "d");

			await That(result).IsEqualTo("d");
		}

		[Fact]
		public async Task Throws_Callback_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Throws(() => new Exception("foo"));

			void Act()
				=> sut.Subject.Method4(1, 2, 3, 4);

			await That(Act).ThrowsException().WithMessage("foo");
		}

		[Fact]
		public async Task Throws_CallbackWithValue_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Throws((v1, v2, v3, v4) => new Exception($"foo-{v1}-{v2}-{v3}-{v4}"));

			void Act()
				=> sut.Subject.Method4(1, 2, 3, 4);

			await That(Act).ThrowsException().WithMessage("foo-1-2-3-4");
		}

		[Fact]
		public async Task Throws_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Throws(new Exception("foo"));

			void Act()
				=> sut.Subject.Method4(1, 2, 3, 4);

			await That(Act).ThrowsException().WithMessage("foo");
		}

		[Fact]
		public async Task WhenSetupWithNull_ShouldReturnDefaultValue()
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Callback(() => { callCount++; })
				.Returns((string?)null!);

			string result = sut.Subject.Method4(1, 2, 3, 4);

			await That(callCount).IsEqualTo(1);
			await That(result).IsNull();
		}

		private class MyReturnMethodSetup<T1, T2, T3, T4>(string name)
			: ReturnMethodSetup<int, T1, T2, T3, T4>(name,
				new With.NamedParameter("p1", With.Matching<T1>(_ => false)),
				new With.NamedParameter("p2", With.Matching<T2>(_ => false)),
				new With.NamedParameter("p3", With.Matching<T3>(_ => false)),
				new With.NamedParameter("p4", With.Matching<T4>(_ => false)))
		{
			public T SetOutParameter<T>(string parameterName)
				=> base.SetOutParameter<T>(parameterName, MockBehavior.Default);

			public T SetRefParameter<T>(string parameterName, T value)
				=> base.SetRefParameter(parameterName, value, MockBehavior.Default);

			public T GetReturnValue<T>(MethodInvocation invocation)
				=> base.GetReturnValue<T>(invocation, MockBehavior.Default);
		}
	}

	public class ReturnMethodWith5Parameters
	{
		[Fact]
		public async Task Callback_ShouldExecuteWhenInvoked()
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method5(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>(),
					With.Any<int>())
				.Callback(() => { callCount++; })
				.Returns("a");

			sut.Subject.Method5(1, 2, 3, 4, 5);

			await That(callCount).IsEqualTo(1);
		}

		[Theory]
		[InlineData(false, false, false, false, false)]
		[InlineData(true, true, true, true, false)]
		[InlineData(true, true, true, false, true)]
		[InlineData(true, true, false, true, true)]
		[InlineData(true, false, true, true, true)]
		[InlineData(false, true, true, true, true)]
		public async Task Callback_ShouldNotExecuteWhenAnyParameterDoesNotMatch(bool isMatch1, bool isMatch2,
			bool isMatch3, bool isMatch4, bool isMatch5)
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method5(With.Matching<int>(v => isMatch1), With.Matching<int>(v => isMatch2),
					With.Matching<int>(v => isMatch3), With.Matching<int>(v => isMatch4),
					With.Matching<int>(v => isMatch5))
				.Callback(() => { callCount++; });

			sut.Subject.Method5(1, 2, 3, 4, 5);

			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task Callback_ShouldNotExecuteWhenOtherMethodIsInvoked()
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method5(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>(),
					With.Any<int>())
				.Callback(() => { callCount++; });

			sut.Subject.Method4(1, 2, 3, 4);
			sut.Subject.Method5(1, 2, 3, 4, 5, false);

			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task CallbackWithValue_ShouldExecuteWhenInvoked()
		{
			int callCount = 0;
			int receivedValue1 = 0;
			int receivedValue2 = 0;
			int receivedValue3 = 0;
			int receivedValue4 = 0;
			int receivedValue5 = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method5(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>(),
					With.Any<int>())
				.Callback((v1, v2, v3, v4, v5) =>
				{
					callCount++;
					receivedValue1 = v1;
					receivedValue2 = v2;
					receivedValue3 = v3;
					receivedValue4 = v4;
					receivedValue5 = v5;
				});

			sut.Subject.Method5(2, 4, 6, 8, 10);

			await That(callCount).IsEqualTo(1);
			await That(receivedValue1).IsEqualTo(2);
			await That(receivedValue2).IsEqualTo(4);
			await That(receivedValue3).IsEqualTo(6);
			await That(receivedValue4).IsEqualTo(8);
			await That(receivedValue5).IsEqualTo(10);
		}

		[Theory]
		[InlineData(false, false, false, false, false)]
		[InlineData(true, true, true, true, false)]
		[InlineData(true, true, true, false, true)]
		[InlineData(true, true, false, true, true)]
		[InlineData(true, false, true, true, true)]
		[InlineData(false, true, true, true, true)]
		public async Task CallbackWithValue_ShouldNotExecuteWhenAnyParameterDoesNotMatch(bool isMatch1, bool isMatch2,
			bool isMatch3, bool isMatch4, bool isMatch5)
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method5(With.Matching<int>(v => isMatch1), With.Matching<int>(v => isMatch2),
					With.Matching<int>(v => isMatch3), With.Matching<int>(v => isMatch4),
					With.Matching<int>(v => isMatch5))
				.Callback((v1, v2, v3, v4, v5) => { callCount++; });

			sut.Subject.Method5(1, 2, 3, 4, 5);

			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task CallbackWithValue_ShouldNotExecuteWhenOtherMethodIsInvoked()
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method5(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>(),
					With.Any<int>())
				.Callback((v1, v2, v3, v4, v5) => { callCount++; });

			sut.Subject.Method4(1, 2, 3, 4);
			sut.Subject.Method5(1, 2, 3, 4, 5, false);

			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task GetReturnValue_InvalidInputType1_ShouldThrowMockException()
		{
			MyReturnMethodSetup<int, int, int, int, int> setup = new("foo");
			setup.Returns((v1, v2, v3, v4, v5) => 3 * v1 * v2 * v3 * v4 * v5);
			MethodInvocation invocation = new(0, "foo", ["a", 2, 3, 4, 5,]);

			void Act()
				=> setup.GetReturnValue<int>(invocation);

			await That(Act).Throws<MockException>()
				.WithMessage("""
				             The input parameter 1 only supports 'int', but is 'string'.
				             """);
		}

		[Fact]
		public async Task GetReturnValue_InvalidInputType2_ShouldThrowMockException()
		{
			MyReturnMethodSetup<int, int, int, int, int> setup = new("foo");
			setup.Returns((v1, v2, v3, v4, v5) => 3 * v1 * v2 * v3 * v4 * v5);
			MethodInvocation invocation = new(0, "foo", [1, "b", 3, 4, 5,]);

			void Act()
				=> setup.GetReturnValue<int>(invocation);

			await That(Act).Throws<MockException>()
				.WithMessage("""
				             The input parameter 2 only supports 'int', but is 'string'.
				             """);
		}

		[Fact]
		public async Task GetReturnValue_InvalidInputType3_ShouldThrowMockException()
		{
			MyReturnMethodSetup<int, int, int, int, int> setup = new("foo");
			setup.Returns((v1, v2, v3, v4, v5) => 3 * v1 * v2 * v3 * v4 * v5);
			MethodInvocation invocation = new(0, "foo", [1, 2, "c", 4, 5,]);

			void Act()
				=> setup.GetReturnValue<int>(invocation);

			await That(Act).Throws<MockException>()
				.WithMessage("""
				             The input parameter 3 only supports 'int', but is 'string'.
				             """);
		}

		[Fact]
		public async Task GetReturnValue_InvalidInputType4_ShouldThrowMockException()
		{
			MyReturnMethodSetup<int, int, int, int, int> setup = new("foo");
			setup.Returns((v1, v2, v3, v4, v5) => 3 * v1 * v2 * v3 * v4 * v5);
			MethodInvocation invocation = new(0, "foo", [1, 2, 3, "d", 5,]);

			void Act()
				=> setup.GetReturnValue<int>(invocation);

			await That(Act).Throws<MockException>()
				.WithMessage("""
				             The input parameter 4 only supports 'int', but is 'string'.
				             """);
		}

		[Fact]
		public async Task GetReturnValue_InvalidInputType5_ShouldThrowMockException()
		{
			MyReturnMethodSetup<int, int, int, int, int> setup = new("foo");
			setup.Returns((v1, v2, v3, v4, v5) => 3 * v1 * v2 * v3 * v4 * v5);
			MethodInvocation invocation = new(0, "foo", [1, 2, 3, 4, "e",]);

			void Act()
				=> setup.GetReturnValue<int>(invocation);

			await That(Act).Throws<MockException>()
				.WithMessage("""
				             The input parameter 5 only supports 'int', but is 'string'.
				             """);
		}

		[Fact]
		public async Task GetReturnValue_InvalidType_ShouldThrowMockException()
		{
			int callCount = 0;
			MyReturnMethodSetup<int, int, int, int, int> setup = new("foo");
			setup.Callback(() => { callCount++; }).Returns(3);
			MethodInvocation invocation = new(0, "foo", [1, 2, 3, 4, 5,]);

			void Act()
				=> setup.GetReturnValue<string>(invocation);

			await That(Act).Throws<MockException>()
				.WithMessage("""
				             The return callback only supports 'int' and not 'string'.
				             """);
			await That(callCount).IsEqualTo(0).Because("The callback should only be executed on success!");
		}

		[Fact]
		public async Task MixReturnsAndThrows_ShouldIterateThroughBoth()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method5(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>(),
					With.Any<int>())
				.Returns("d")
				.Throws(new Exception("foo"))
				.Returns(() => "b");

			string result1 = sut.Subject.Method5(1, 2, 3, 4, 5);
			Exception? result2 = Record.Exception(() => sut.Subject.Method5(2, 3, 4, 5, 6));
			string result3 = sut.Subject.Method5(3, 4, 5, 6, 7);

			await That(result1).IsEqualTo("d");
			await That(result2).HasMessage("foo");
			await That(result3).IsEqualTo("b");
		}

		[Fact]
		public async Task MultipleCallbacks_ShouldAllGetInvoked()
		{
			int callCount1 = 0;
			int callCount2 = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method5(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>(),
					With.Any<int>())
				.Callback(() => { callCount1++; })
				.Callback((v1, v2, v3, v4, v5) => { callCount2 += v1 * v2 * v3 * v4 * v5; })
				.Returns("a");

			sut.Subject.Method5(1, 2, 3, 4, 5);
			sut.Subject.Method5(2, 2, 3, 4, 5);

			await That(callCount1).IsEqualTo(2);
			await That(callCount2).IsEqualTo(360);
		}

		[Fact]
		public async Task MultipleReturns_ShouldIterateThroughAllRegisteredValues()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method5(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>(),
					With.Any<int>())
				.Returns("d")
				.Returns(() => "c")
				.Returns((v1, v2, v3, v4, v5) => $"foo-{v1}-{v2}-{v3}-{v4}-{v5}");

			string[] result = new string[10];
			for (int i = 0; i < 10; i++)
			{
				result[i] = sut.Subject.Method5(i, 2 * i, 3 * i, 4 * i, 5 * i);
			}

			await That(result).IsEqualTo([
				"d", "c", "foo-2-4-6-8-10", "d", "c", "foo-5-10-15-20-25", "d", "c", "foo-8-16-24-32-40", "d",
			]);
		}

		[Fact]
		public async Task OutParameter_ShouldSet()
		{
			int receivedValue1 = 0;
			int receivedValue2 = 0;
			int receivedValue3 = 0;
			int receivedValue4 = 0;
			int receivedValue5 = 0;
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method5WithOutParameter(With.Out(() => 2), With.Out(() => 4), With.Out(() => 6),
					With.Out(() => 8), With.Out(() => 10))
				.Callback((v1, v2, v3, v4, v5) =>
				{
					callCount++;
					receivedValue1 = v1;
					receivedValue2 = v2;
					receivedValue3 = v3;
					receivedValue4 = v4;
					receivedValue5 = v5;
				});

			sut.Subject.Method5WithOutParameter(out int value1, out int value2, out int value3, out int value4,
				out int value5);

			await That(callCount).IsEqualTo(1);
			await That(value1).IsEqualTo(2);
			await That(receivedValue1).IsEqualTo(0);
			await That(value2).IsEqualTo(4);
			await That(receivedValue2).IsEqualTo(0);
			await That(value3).IsEqualTo(6);
			await That(receivedValue3).IsEqualTo(0);
			await That(value4).IsEqualTo(8);
			await That(receivedValue4).IsEqualTo(0);
			await That(value5).IsEqualTo(10);
			await That(receivedValue5).IsEqualTo(0);
		}

		[Fact]
		public async Task RefParameter_ShouldSet()
		{
			int receivedValue1 = 0;
			int receivedValue2 = 0;
			int receivedValue3 = 0;
			int receivedValue4 = 0;
			int receivedValue5 = 0;
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method5WithRefParameter(With.Ref<int>(v => v * 10), With.Ref<int>(v => v * 10),
					With.Ref<int>(v => v * 10), With.Ref<int>(v => v * 10), With.Ref<int>(v => v * 10))
				.Callback((v1, v2, v3, v4, v5) =>
				{
					callCount++;
					receivedValue1 = v1;
					receivedValue2 = v2;
					receivedValue3 = v3;
					receivedValue4 = v4;
					receivedValue5 = v5;
				});

			int value1 = 2;
			int value2 = 4;
			int value3 = 6;
			int value4 = 8;
			int value5 = 10;
			sut.Subject.Method5WithRefParameter(ref value1, ref value2, ref value3, ref value4, ref value5);

			await That(callCount).IsEqualTo(1);
			await That(value1).IsEqualTo(20);
			await That(receivedValue1).IsEqualTo(2);
			await That(value2).IsEqualTo(40);
			await That(receivedValue2).IsEqualTo(4);
			await That(value3).IsEqualTo(60);
			await That(receivedValue3).IsEqualTo(6);
			await That(value4).IsEqualTo(80);
			await That(receivedValue4).IsEqualTo(8);
			await That(value5).IsEqualTo(100);
			await That(receivedValue5).IsEqualTo(10);
		}

		[Fact]
		public async Task Returns_Callback_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method5(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>(),
					With.Any<int>())
				.Returns(() => "d");

			string result = sut.Subject.Method5(1, 2, 3, 4, 5);

			await That(result).IsEqualTo("d");
		}

		[Fact]
		public async Task Returns_CallbackWithValue_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method5(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>(),
					With.Any<int>())
				.Returns((x, y, z, a, b) => $"foo-{x}-{y}-{z}-{a}-{b}");

			string result = sut.Subject.Method5(2, 3, 4, 5, 6);

			await That(result).IsEqualTo("foo-2-3-4-5-6");
		}

		[Fact]
		public async Task Returns_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method5(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>(),
					With.Any<int>())
				.Returns("d");

			string result = sut.Subject.Method5(1, 2, 3, 4, 5);

			await That(result).IsEqualTo("d");
		}

		[Fact]
		public async Task Returns_WithoutSetup_ShouldReturnDefault()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			string result = sut.Subject.Method5(2, 3, 4, 5, 6);

			await That(result).IsEmpty();
		}

		[Fact]
		public async Task SetOutParameter_ShouldReturnDefaultValue()
		{
			MyReturnMethodSetup<int, int, int, int, int> setup = new("foo");

			string result = setup.SetOutParameter<string>("p1");

			await That(result).IsEmpty();
		}

		[Fact]
		public async Task SetRefParameter_ShouldReturnValue()
		{
			MyReturnMethodSetup<int, int, int, int, int> setup = new("foo");

			string result = setup.SetRefParameter("p1", "d");

			await That(result).IsEqualTo("d");
		}

		[Fact]
		public async Task Throws_Callback_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method5(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>(),
					With.Any<int>())
				.Throws(() => new Exception("foo"));

			void Act()
				=> sut.Subject.Method5(1, 2, 3, 4, 5);

			await That(Act).ThrowsException().WithMessage("foo");
		}

		[Fact]
		public async Task Throws_CallbackWithValue_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method5(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>(),
					With.Any<int>())
				.Throws((v1, v2, v3, v4, v5) => new Exception($"foo-{v1}-{v2}-{v3}-{v4}-{v5}"));

			void Act()
				=> sut.Subject.Method5(1, 2, 3, 4, 5);

			await That(Act).ThrowsException().WithMessage("foo-1-2-3-4-5");
		}

		[Fact]
		public async Task Throws_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method5(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>(),
					With.Any<int>())
				.Throws(new Exception("foo"));

			void Act()
				=> sut.Subject.Method5(1, 2, 3, 4, 5);

			await That(Act).ThrowsException().WithMessage("foo");
		}

		[Fact]
		public async Task WhenSetupWithNull_ShouldReturnDefaultValue()
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

			sut.Setup.Method.Method5(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>(),
					With.Any<int>())
				.Callback(() => { callCount++; })
				.Returns((string?)null!);

			string result = sut.Subject.Method5(1, 2, 3, 4, 5);

			await That(callCount).IsEqualTo(1);
			await That(result).IsNull();
		}

		private class MyReturnMethodSetup<T1, T2, T3, T4, T5>(string name)
			: ReturnMethodSetup<int, T1, T2, T3, T4, T5>(name,
				new With.NamedParameter("p1", With.Matching<T1>(_ => false)),
				new With.NamedParameter("p2", With.Matching<T2>(_ => false)),
				new With.NamedParameter("p3", With.Matching<T3>(_ => false)),
				new With.NamedParameter("p4", With.Matching<T4>(_ => false)),
				new With.NamedParameter("p5", With.Matching<T5>(_ => false)))
		{
			public T SetOutParameter<T>(string parameterName)
				=> base.SetOutParameter<T>(parameterName, MockBehavior.Default);

			public T SetRefParameter<T>(string parameterName, T value)
				=> base.SetRefParameter(parameterName, value, MockBehavior.Default);

			public T GetReturnValue<T>(MethodInvocation invocation)
				=> base.GetReturnValue<T>(invocation, MockBehavior.Default);
		}
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
	}
}
