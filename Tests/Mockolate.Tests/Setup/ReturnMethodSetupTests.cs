using Mockolate.Checks;
using Mockolate.Exceptions;
using Mockolate.Setup;

namespace Mockolate.Tests.Setup;

public class ReturnMethodSetupTests
{
	public class With0Parameters
	{
		[Fact]
		public async Task Callback_ShouldExecuteWhenInvoked()
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method0()
				.Callback(() => { callCount++; })
				.Returns(1);

			sut.Object.Method0();

			await That(callCount).IsEqualTo(1);
		}

		[Fact]
		public async Task Callback_ShouldNotExecuteWhenOtherMethodIsInvoked()
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method0().Callback(() => { callCount++; });

			sut.Object.Method1(1);
			sut.Object.Method0(false);

			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task GetReturnValue_InvalidType_ShouldThrowMockException()
		{
			int callCount = 0;
			MyReturnMethodSetup setup = new("foo");
			setup.Callback(() => { callCount++; }).Returns(3);
			MethodInvocation invocation = new("foo", Array.Empty<object?>());

			void Act()
				=> setup.GetReturnValue<string>(invocation);

			await That(Act).Throws<MockException>()
				.WithMessage("""
				             The return callback only supports 'System.Int32' and not 'System.String'.
				             """);
			await That(callCount).IsEqualTo(0).Because("The callback should only be executed on success!");
		}

		[Fact]
		public async Task MixReturnsAndThrows_ShouldIterateThroughBoth()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method0()
				.Returns(4)
				.Throws(new Exception("foo"))
				.Returns(() => 2);

			int result1 = sut.Object.Method0();
			Exception? result2 = Record.Exception(() => sut.Object.Method0());
			int result3 = sut.Object.Method0();

			await That(result1).IsEqualTo(4);
			await That(result2).HasMessage("foo");
			await That(result3).IsEqualTo(2);
		}

		[Fact]
		public async Task MultipleCallbacks_ShouldAllGetInvoked()
		{
			int callCount1 = 0;
			int callCount2 = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method0()
				.Callback(() => { callCount1++; })
				.Callback(() => { callCount2++; })
				.Returns(1);

			sut.Object.Method0();
			sut.Object.Method0();

			await That(callCount1).IsEqualTo(2);
			await That(callCount2).IsEqualTo(2);
		}

		[Fact]
		public async Task MultipleReturns_ShouldIterateThroughAllRegisteredValues()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method0()
				.Returns(4)
				.Returns(3)
				.Returns(() => 2);

			int[] result = new int[10];
			for (int i = 0; i < 10; i++)
			{
				result[i] = sut.Object.Method0();
			}

			await That(result).IsEqualTo([4, 3, 2, 4, 3, 2, 4, 3, 2, 4,]);
		}

		[Fact]
		public async Task Returns_Callback_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method0().Returns(() => 4);

			int result = sut.Object.Method0();

			await That(result).IsEqualTo(4);
		}

		[Fact]
		public async Task Returns_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method0().Returns(4);

			int result = sut.Object.Method0();

			await That(result).IsEqualTo(4);
		}

		[Fact]
		public async Task Returns_WithoutSetup_ShouldReturnDefault()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			int result = sut.Object.Method0();

			await That(result).IsEqualTo(0);
		}

		[Fact]
		public async Task SetOutParameter_ShouldReturnDefaultValue()
		{
			MyReturnMethodSetup setup = new("foo");

			int result = setup.SetOutParameter<int>("p0");

			await That(result).IsEqualTo(0);
		}

		[Fact]
		public async Task SetRefParameter_ShouldReturnValue()
		{
			MyReturnMethodSetup setup = new("foo");

			int result = setup.SetRefParameter("p0", 4);

			await That(result).IsEqualTo(4);
		}

		[Fact]
		public async Task Throws_Callback_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method0().Throws(() => new Exception("foo"));

			void Act()
				=> sut.Object.Method0();

			await That(Act).ThrowsException().WithMessage("foo");
		}

		[Fact]
		public async Task Throws_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method0().Throws(new Exception("foo"));

			void Act()
				=> sut.Object.Method0();

			await That(Act).ThrowsException().WithMessage("foo");
		}

		private class MyReturnMethodSetup(string name) : ReturnMethodSetup<int>(name)
		{
			public T SetOutParameter<T>(string parameterName)
				=> base.SetOutParameter<T>(parameterName, MockBehavior.Default);

			public T SetRefParameter<T>(string parameterName, T value)
				=> base.SetRefParameter<T>(parameterName, value, MockBehavior.Default);

			public T GetReturnValue<T>(MethodInvocation invocation)
				=> base.GetReturnValue<T>(invocation, MockBehavior.Default);
		}
	}

	public class With1Parameters
	{
		[Fact]
		public async Task Callback_ShouldExecuteWhenInvoked()
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method1(With.Any<int>())
				.Callback(() => { callCount++; })
				.Returns(1);

			sut.Object.Method1(3);

			await That(callCount).IsEqualTo(1);
		}

		[Fact]
		public async Task Callback_ShouldNotExecuteWhenOtherMethodIsInvoked()
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method1(With.Any<int>())
				.Callback(() => { callCount++; });

			sut.Object.Method0();
			sut.Object.Method1(2, false);

			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task Callback_ShouldNotExecuteWhenParameterDoesNotMatch()
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method1(With.Matching<int>(v => v != 1))
				.Callback(() => { callCount++; });

			sut.Object.Method1(1);

			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task CallbackWithValue_ShouldExecuteWhenInvoked()
		{
			int callCount = 0;
			int receivedValue = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method1(With.Any<int>())
				.Callback(v =>
				{
					callCount++;
					receivedValue = v;
				});

			sut.Object.Method1(3);

			await That(callCount).IsEqualTo(1);
			await That(receivedValue).IsEqualTo(3);
		}

		[Fact]
		public async Task CallbackWithValue_ShouldNotExecuteWhenOtherMethodIsInvoked()
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method1(With.Any<int>())
				.Callback(v => { callCount++; });

			sut.Object.Method0();
			sut.Object.Method1(2, false);

			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task CallbackWithValue_ShouldNotExecuteWhenParameterDoesNotMatch()
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method1(With.Matching<int>(v => v != 1))
				.Callback(v => { callCount++; });

			sut.Object.Method1(1);

			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task GetReturnValue_InvalidInputType_ShouldThrowMockException()
		{
			MyReturnMethodSetup<int> setup = new("foo");
			setup.Returns(x => 3 * x);
			MethodInvocation invocation = new("foo", ["2",]);

			void Act()
				=> setup.GetReturnValue<int>(invocation);

			await That(Act).Throws<MockException>()
				.WithMessage("""
				             The input parameter only supports 'System.Int32', but is 'System.String'.
				             """);
		}

		[Fact]
		public async Task GetReturnValue_InvalidType_ShouldThrowMockException()
		{
			int callCount = 0;
			MyReturnMethodSetup<int> setup = new("foo");
			setup.Callback(() => { callCount++; }).Returns(x => 3 * x);
			MethodInvocation invocation = new("foo", [2,]);

			void Act()
				=> setup.GetReturnValue<string>(invocation);

			await That(Act).Throws<MockException>()
				.WithMessage("""
				             The return callback only supports 'System.Int32' and not 'System.String'.
				             """);
			await That(callCount).IsEqualTo(0).Because("The callback should only be executed on success!");
		}

		[Fact]
		public async Task MixReturnsAndThrows_ShouldIterateThroughBoth()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method1(With.Any<int>())
				.Returns(4)
				.Throws(new Exception("foo"))
				.Returns(() => 2);

			int result1 = sut.Object.Method1(1);
			Exception? result2 = Record.Exception(() => sut.Object.Method1(2));
			int result3 = sut.Object.Method1(3);

			await That(result1).IsEqualTo(4);
			await That(result2).HasMessage("foo");
			await That(result3).IsEqualTo(2);
		}

		[Fact]
		public async Task MultipleCallbacks_ShouldAllGetInvoked()
		{
			int callCount1 = 0;
			int callCount2 = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method1(With.Any<int>())
				.Callback(() => { callCount1++; })
				.Callback(v => { callCount2 += v; })
				.Returns(1);

			sut.Object.Method1(1);
			sut.Object.Method1(2);

			await That(callCount1).IsEqualTo(2);
			await That(callCount2).IsEqualTo(3);
		}

		[Fact]
		public async Task MultipleReturns_ShouldIterateThroughAllRegisteredValues()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method1(With.Any<int>())
				.Returns(4)
				.Returns(() => 3)
				.Returns(v => 10 * v);

			int[] result = new int[10];
			for (int i = 0; i < 10; i++)
			{
				result[i] = sut.Object.Method1(i);
			}

			await That(result).IsEqualTo([4, 3, 20, 4, 3, 50, 4, 3, 80, 4,]);
		}

		[Fact]
		public async Task OutParameter_ShouldSet()
		{
			int receivedValue = 0;
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method1WithOutParameter(With.Out(() => 3))
				.Callback(v =>
				{
					callCount++;
					receivedValue = v;
				});

			sut.Object.Method1WithOutParameter(out int value);

			await That(callCount).IsEqualTo(1);
			await That(value).IsEqualTo(3);
			await That(receivedValue).IsEqualTo(0);
		}

		[Fact]
		public async Task RefParameter_ShouldSet()
		{
			int receivedValue = 0;
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method1WithRefParameter(With.Ref<int>(v => 3))
				.Callback(v =>
				{
					callCount++;
					receivedValue = v;
				});

			int value = 2;
			sut.Object.Method1WithRefParameter(ref value);

			await That(callCount).IsEqualTo(1);
			await That(value).IsEqualTo(3);
			await That(receivedValue).IsEqualTo(2);
		}

		[Fact]
		public async Task Returns_Callback_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method1(With.Any<int>()).Returns(() => 4);

			int result = sut.Object.Method1(3);

			await That(result).IsEqualTo(4);
		}

		[Fact]
		public async Task Returns_CallbackWithValue_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method1(With.Any<int>()).Returns(x => 4 * x);

			int result = sut.Object.Method1(3);

			await That(result).IsEqualTo(12);
		}

		[Fact]
		public async Task Returns_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method1(With.Any<int>()).Returns(4);

			int result = sut.Object.Method1(3);

			await That(result).IsEqualTo(4);
		}

		[Fact]
		public async Task Returns_WithoutSetup_ShouldReturnDefault()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			int result = sut.Object.Method1(3);

			await That(result).IsEqualTo(0);
		}

		[Fact]
		public async Task SetOutParameter_ShouldReturnDefaultValue()
		{
			MyReturnMethodSetup<int> setup = new("foo");

			int result = setup.SetOutParameter<int>("p1");

			await That(result).IsEqualTo(0);
		}

		[Fact]
		public async Task SetRefParameter_ShouldReturnValue()
		{
			MyReturnMethodSetup<int> setup = new("foo");

			int result = setup.SetRefParameter("p1", 4);

			await That(result).IsEqualTo(4);
		}

		[Fact]
		public async Task Throws_Callback_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method1(With.Any<int>())
				.Throws(() => new Exception("foo"));

			void Act()
				=> sut.Object.Method1(1);

			await That(Act).ThrowsException().WithMessage("foo");
		}

		[Fact]
		public async Task Throws_CallbackWithValue_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method1(With.Any<int>())
				.Throws(v1 => new Exception("foo-" + v1));

			void Act()
				=> sut.Object.Method1(42);

			await That(Act).ThrowsException().WithMessage("foo-42");
		}

		[Fact]
		public async Task Throws_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method1(With.Any<int>())
				.Throws(new Exception("foo"));

			void Act()
				=> sut.Object.Method1(1);

			await That(Act).ThrowsException().WithMessage("foo");
		}

		private class MyReturnMethodSetup<T1>(string name)
			: ReturnMethodSetup<int, T1>(name, new With.NamedParameter("p1", With.Matching<T1>(_ => false)))
		{
			public T SetOutParameter<T>(string parameterName)
				=> base.SetOutParameter<T>(parameterName, MockBehavior.Default);

			public T SetRefParameter<T>(string parameterName, T value)
				=> base.SetRefParameter<T>(parameterName, value, MockBehavior.Default);

			public T GetReturnValue<T>(MethodInvocation invocation)
				=> base.GetReturnValue<T>(invocation, MockBehavior.Default);
		}
	}

	public class With2Parameters
	{
		[Fact]
		public async Task Callback_ShouldExecuteWhenInvoked()
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method2(With.Any<int>(), With.Any<int>())
				.Callback(() => { callCount++; })
				.Returns(1);

			sut.Object.Method2(1, 2);

			await That(callCount).IsEqualTo(1);
		}

		[Theory]
		[InlineData(false, false)]
		[InlineData(true, false)]
		[InlineData(false, true)]
		public async Task Callback_ShouldNotExecuteWhenAnyParameterDoesNotMatch(bool isMatch1, bool isMatch2)
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method2(With.Matching<int>(v => isMatch1), With.Matching<int>(v => isMatch2))
				.Callback(() => { callCount++; });

			sut.Object.Method2(1, 2);

			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task Callback_ShouldNotExecuteWhenOtherMethodIsInvoked()
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method2(With.Any<int>(), With.Any<int>())
				.Callback(() => { callCount++; });

			sut.Object.Method1(1);
			sut.Object.Method2(1, 2, false);

			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task CallbackWithValue_ShouldExecuteWhenInvoked()
		{
			int callCount = 0;
			int receivedValue1 = 0;
			int receivedValue2 = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method2(With.Any<int>(), With.Any<int>())
				.Callback((v1, v2) =>
				{
					callCount++;
					receivedValue1 = v1;
					receivedValue2 = v2;
				});

			sut.Object.Method2(2, 4);

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
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method2(With.Matching<int>(v => isMatch1), With.Matching<int>(v => isMatch2))
				.Callback((v1, v2) => { callCount++; });

			sut.Object.Method2(1, 2);

			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task CallbackWithValue_ShouldNotExecuteWhenOtherMethodIsInvoked()
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method2(With.Any<int>(), With.Any<int>())
				.Callback((v1, v2) => { callCount++; });

			sut.Object.Method1(1);
			sut.Object.Method2(1, 2, false);

			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task GetReturnValue_InvalidInputType1_ShouldThrowMockException()
		{
			MyReturnMethodSetup<int, int> setup = new("foo");
			setup.Returns((x, y) => 3 * x);
			MethodInvocation invocation = new("foo", ["1", 2,]);

			void Act()
				=> setup.GetReturnValue<int>(invocation);

			await That(Act).Throws<MockException>()
				.WithMessage("""
				             The input parameter 1 only supports 'System.Int32', but is 'System.String'.
				             """);
		}

		[Fact]
		public async Task GetReturnValue_InvalidInputType2_ShouldThrowMockException()
		{
			MyReturnMethodSetup<int, int> setup = new("foo");
			setup.Returns((x, y) => 3 * x);
			MethodInvocation invocation = new("foo", [1, "2",]);

			void Act()
				=> setup.GetReturnValue<int>(invocation);

			await That(Act).Throws<MockException>()
				.WithMessage("""
				             The input parameter 2 only supports 'System.Int32', but is 'System.String'.
				             """);
		}

		[Fact]
		public async Task GetReturnValue_InvalidType_ShouldThrowMockException()
		{
			int callCount = 0;
			MyReturnMethodSetup<int, int> setup = new("foo");
			setup.Callback(() => { callCount++; }).Returns(3);
			MethodInvocation invocation = new("foo", [2, 3,]);

			void Act()
				=> setup.GetReturnValue<string>(invocation);

			await That(Act).Throws<MockException>()
				.WithMessage("""
				             The return callback only supports 'System.Int32' and not 'System.String'.
				             """);
			await That(callCount).IsEqualTo(0).Because("The callback should only be executed on success!");
		}

		[Fact]
		public async Task MixReturnsAndThrows_ShouldIterateThroughBoth()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method2(With.Any<int>(), With.Any<int>())
				.Returns(4)
				.Throws(new Exception("foo"))
				.Returns(() => 2);

			int result1 = sut.Object.Method2(1, 2);
			Exception? result2 = Record.Exception(() => sut.Object.Method2(2, 3));
			int result3 = sut.Object.Method2(3, 4);

			await That(result1).IsEqualTo(4);
			await That(result2).HasMessage("foo");
			await That(result3).IsEqualTo(2);
		}

		[Fact]
		public async Task MultipleCallbacks_ShouldAllGetInvoked()
		{
			int callCount1 = 0;
			int callCount2 = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method2(With.Any<int>(), With.Any<int>())
				.Callback(() => { callCount1++; })
				.Callback((v1, v2) => { callCount2 += v1 * v2; })
				.Returns(1);

			sut.Object.Method2(1, 2);
			sut.Object.Method2(2, 2);

			await That(callCount1).IsEqualTo(2);
			await That(callCount2).IsEqualTo(6);
		}

		[Fact]
		public async Task MultipleReturns_ShouldIterateThroughAllRegisteredValues()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method2(With.Any<int>(), With.Any<int>())
				.Returns(4)
				.Returns(() => 3)
				.Returns((v1, v2) => 10 + v1 + v2);

			int[] result = new int[10];
			for (int i = 0; i < 10; i++)
			{
				result[i] = sut.Object.Method2(i, 2 * i);
			}

			await That(result).IsEqualTo([4, 3, 16, 4, 3, 25, 4, 3, 34, 4,]);
		}

		[Fact]
		public async Task OutParameter_ShouldSet()
		{
			int receivedValue1 = 0;
			int receivedValue2 = 0;
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method2WithOutParameter(With.Out(() => 2), With.Out(() => 4))
				.Callback((v1, v2) =>
				{
					callCount++;
					receivedValue1 = v1;
					receivedValue2 = v2;
				});

			sut.Object.Method2WithOutParameter(out int value1, out int value2);

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
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method2WithRefParameter(With.Ref<int>(v => v * 10), With.Ref<int>(v => v * 10))
				.Callback((v1, v2) =>
				{
					callCount++;
					receivedValue1 = v1;
					receivedValue2 = v2;
				});

			int value1 = 2;
			int value2 = 4;
			sut.Object.Method2WithRefParameter(ref value1, ref value2);

			await That(callCount).IsEqualTo(1);
			await That(value1).IsEqualTo(20);
			await That(receivedValue1).IsEqualTo(2);
			await That(value2).IsEqualTo(40);
			await That(receivedValue2).IsEqualTo(4);
		}

		[Fact]
		public async Task Returns_Callback_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method2(With.Any<int>(), With.Any<int>()).Returns(() => 4);

			int result = sut.Object.Method2(2, 3);

			await That(result).IsEqualTo(4);
		}

		[Fact]
		public async Task Returns_CallbackWithValue_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method2(With.Any<int>(), With.Any<int>()).Returns((x, y) => 4 * x * y);

			int result = sut.Object.Method2(2, 3);

			await That(result).IsEqualTo(24);
		}

		[Fact]
		public async Task Returns_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method2(With.Any<int>(), With.Any<int>()).Returns(4);

			int result = sut.Object.Method2(2, 3);

			await That(result).IsEqualTo(4);
		}

		[Fact]
		public async Task Returns_WithoutSetup_ShouldReturnDefault()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			int result = sut.Object.Method2(2, 3);

			await That(result).IsEqualTo(0);
		}

		[Fact]
		public async Task SetOutParameter_ShouldReturnDefaultValue()
		{
			MyReturnMethodSetup<int, int> setup = new("foo");

			int result = setup.SetOutParameter<int>("p1");

			await That(result).IsEqualTo(0);
		}

		[Fact]
		public async Task SetRefParameter_ShouldReturnValue()
		{
			MyReturnMethodSetup<int, int> setup = new("foo");

			int result = setup.SetRefParameter("p1", 4);

			await That(result).IsEqualTo(4);
		}

		[Fact]
		public async Task Throws_Callback_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method2(With.Any<int>(), With.Any<int>())
				.Throws(() => new Exception("foo"));

			void Act()
				=> sut.Object.Method2(1, 2);

			await That(Act).ThrowsException().WithMessage("foo");
		}

		[Fact]
		public async Task Throws_CallbackWithValue_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method2(With.Any<int>(), With.Any<int>())
				.Throws((v1, v2) => new Exception($"foo-{v1}-{v2}"));

			void Act()
				=> sut.Object.Method2(1, 2);

			await That(Act).ThrowsException().WithMessage("foo-1-2");
		}

		[Fact]
		public async Task Throws_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method2(With.Any<int>(), With.Any<int>())
				.Throws(new Exception("foo"));

			void Act()
				=> sut.Object.Method2(1, 2);

			await That(Act).ThrowsException().WithMessage("foo");
		}

		private class MyReturnMethodSetup<T1, T2>(string name)
			: ReturnMethodSetup<int, T1, T2>(name,
				new With.NamedParameter("p1", With.Matching<T1>(_ => false)),
				new With.NamedParameter("p2", With.Matching<T2>(_ => false)))
		{
			public T SetOutParameter<T>(string parameterName)
				=> base.SetOutParameter<T>(parameterName, MockBehavior.Default);

			public T SetRefParameter<T>(string parameterName, T value)
				=> base.SetRefParameter<T>(parameterName, value, MockBehavior.Default);

			public T GetReturnValue<T>(MethodInvocation invocation)
				=> base.GetReturnValue<T>(invocation, MockBehavior.Default);
		}
	}

	public class With3Parameters
	{
		[Fact]
		public async Task Callback_ShouldExecuteWhenInvoked()
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method3(With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Callback(() => { callCount++; })
				.Returns(1);

			sut.Object.Method3(1, 2, 3);

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
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method3(With.Matching<int>(v => isMatch1), With.Matching<int>(v => isMatch2),
					With.Matching<int>(v => isMatch3))
				.Callback(() => { callCount++; });

			sut.Object.Method3(1, 2, 3);

			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task Callback_ShouldNotExecuteWhenOtherMethodIsInvoked()
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method3(With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Callback(() => { callCount++; });

			sut.Object.Method2(1, 2);
			sut.Object.Method3(1, 2, 3, false);

			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task CallbackWithValue_ShouldExecuteWhenInvoked()
		{
			int callCount = 0;
			int receivedValue1 = 0;
			int receivedValue2 = 0;
			int receivedValue3 = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method3(With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Callback((v1, v2, v3) =>
				{
					callCount++;
					receivedValue1 = v1;
					receivedValue2 = v2;
					receivedValue3 = v3;
				});

			sut.Object.Method3(2, 4, 6);

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
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method3(With.Matching<int>(v => isMatch1), With.Matching<int>(v => isMatch2),
					With.Matching<int>(v => isMatch3))
				.Callback((v1, v2, v3) => { callCount++; });

			sut.Object.Method3(1, 2, 3);

			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task CallbackWithValue_ShouldNotExecuteWhenOtherMethodIsInvoked()
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method3(With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Callback((v1, v2, v3) => { callCount++; });

			sut.Object.Method2(1, 2);
			sut.Object.Method3(1, 2, 3, false);

			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task GetReturnValue_InvalidInputType1_ShouldThrowMockException()
		{
			MyReturnMethodSetup<int, int, int> setup = new("foo");
			setup.Returns((x, y, z) => 3 * x);
			MethodInvocation invocation = new("foo", ["1", 2, 3,]);

			void Act()
				=> setup.GetReturnValue<int>(invocation);

			await That(Act).Throws<MockException>()
				.WithMessage("""
				             The input parameter 1 only supports 'System.Int32', but is 'System.String'.
				             """);
		}

		[Fact]
		public async Task GetReturnValue_InvalidInputType2_ShouldThrowMockException()
		{
			MyReturnMethodSetup<int, int, int> setup = new("foo");
			setup.Returns((x, y, z) => 3 * x);
			MethodInvocation invocation = new("foo", [1, "2", 3,]);

			void Act()
				=> setup.GetReturnValue<int>(invocation);

			await That(Act).Throws<MockException>()
				.WithMessage("""
				             The input parameter 2 only supports 'System.Int32', but is 'System.String'.
				             """);
		}

		[Fact]
		public async Task GetReturnValue_InvalidInputType3_ShouldThrowMockException()
		{
			MyReturnMethodSetup<int, int, int> setup = new("foo");
			setup.Returns((x, y, z) => 3 * x);
			MethodInvocation invocation = new("foo", [1, 2, "3",]);

			void Act()
				=> setup.GetReturnValue<int>(invocation);

			await That(Act).Throws<MockException>()
				.WithMessage("""
				             The input parameter 3 only supports 'System.Int32', but is 'System.String'.
				             """);
		}

		[Fact]
		public async Task GetReturnValue_InvalidType_ShouldThrowMockException()
		{
			int callCount = 0;
			MyReturnMethodSetup<int, int, int> setup = new("foo");
			setup.Callback(() => { callCount++; }).Returns(3);
			MethodInvocation invocation = new("foo", [1, 2, 3,]);

			void Act()
				=> setup.GetReturnValue<string>(invocation);

			await That(Act).Throws<MockException>()
				.WithMessage("""
				             The return callback only supports 'System.Int32' and not 'System.String'.
				             """);
			await That(callCount).IsEqualTo(0).Because("The callback should only be executed on success!");
		}

		[Fact]
		public async Task MixReturnsAndThrows_ShouldIterateThroughBoth()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method3(With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Returns(4)
				.Throws(new Exception("foo"))
				.Returns(() => 2);

			int result1 = sut.Object.Method3(1, 2, 3);
			Exception? result2 = Record.Exception(() => sut.Object.Method3(2, 3, 4));
			int result3 = sut.Object.Method3(3, 4, 5);

			await That(result1).IsEqualTo(4);
			await That(result2).HasMessage("foo");
			await That(result3).IsEqualTo(2);
		}

		[Fact]
		public async Task MultipleCallbacks_ShouldAllGetInvoked()
		{
			int callCount1 = 0;
			int callCount2 = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method3(With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Callback(() => { callCount1++; })
				.Callback((v1, v2, v3) => { callCount2 += v1 * v2 * v3; })
				.Returns(1);

			sut.Object.Method3(1, 2, 3);
			sut.Object.Method3(2, 2, 3);

			await That(callCount1).IsEqualTo(2);
			await That(callCount2).IsEqualTo(18);
		}

		[Fact]
		public async Task MultipleReturns_ShouldIterateThroughAllRegisteredValues()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method3(With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Returns(4)
				.Returns(() => 3)
				.Returns((v1, v2, v3) => 10 + v1 + v2 + v3);

			int[] result = new int[10];
			for (int i = 0; i < 10; i++)
			{
				result[i] = sut.Object.Method3(i, 2 * i, 3 * i);
			}

			await That(result).IsEqualTo([4, 3, 22, 4, 3, 40, 4, 3, 58, 4,]);
		}

		[Fact]
		public async Task OutParameter_ShouldSet()
		{
			int receivedValue1 = 0;
			int receivedValue2 = 0;
			int receivedValue3 = 0;
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method3WithOutParameter(With.Out(() => 2), With.Out(() => 4), With.Out(() => 6))
				.Callback((v1, v2, v3) =>
				{
					callCount++;
					receivedValue1 = v1;
					receivedValue2 = v2;
					receivedValue3 = v3;
				});

			sut.Object.Method3WithOutParameter(out int value1, out int value2, out int value3);

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
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method3WithRefParameter(With.Ref<int>(v => v * 10), With.Ref<int>(v => v * 10),
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
			sut.Object.Method3WithRefParameter(ref value1, ref value2, ref value3);

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
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method3(With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Returns(() => 4);

			int result = sut.Object.Method3(1, 2, 3);

			await That(result).IsEqualTo(4);
		}

		[Fact]
		public async Task Returns_CallbackWithValue_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method3(With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Returns((x, y, z) => 4 * x * y * z);

			int result = sut.Object.Method3(2, 3, 4);

			await That(result).IsEqualTo(96);
		}

		[Fact]
		public async Task Returns_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method3(With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Returns(4);

			int result = sut.Object.Method3(1, 2, 3);

			await That(result).IsEqualTo(4);
		}

		[Fact]
		public async Task Returns_WithoutSetup_ShouldReturnDefault()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			int result = sut.Object.Method3(2, 3, 4);

			await That(result).IsEqualTo(0);
		}

		[Fact]
		public async Task SetOutParameter_ShouldReturnDefaultValue()
		{
			MyReturnMethodSetup<int, int, int> setup = new("foo");

			int result = setup.SetOutParameter<int>("p1");

			await That(result).IsEqualTo(0);
		}

		[Fact]
		public async Task SetRefParameter_ShouldReturnValue()
		{
			MyReturnMethodSetup<int, int, int> setup = new("foo");

			int result = setup.SetRefParameter("p1", 4);

			await That(result).IsEqualTo(4);
		}

		[Fact]
		public async Task Throws_Callback_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method3(With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Throws(() => new Exception("foo"));

			void Act()
				=> sut.Object.Method3(1, 2, 3);

			await That(Act).ThrowsException().WithMessage("foo");
		}

		[Fact]
		public async Task Throws_CallbackWithValue_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method3(With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Throws((v1, v2, v3) => new Exception($"foo-{v1}-{v2}-{v3}"));

			void Act()
				=> sut.Object.Method3(1, 2, 3);

			await That(Act).ThrowsException().WithMessage("foo-1-2-3");
		}

		[Fact]
		public async Task Throws_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method3(With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Throws(new Exception("foo"));

			void Act()
				=> sut.Object.Method3(1, 2, 3);

			await That(Act).ThrowsException().WithMessage("foo");
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
				=> base.SetRefParameter<T>(parameterName, value, MockBehavior.Default);

			public T GetReturnValue<T>(MethodInvocation invocation)
				=> base.GetReturnValue<T>(invocation, MockBehavior.Default);
		}
	}

	public class With4Parameters
	{
		[Fact]
		public async Task Callback_ShouldExecuteWhenInvoked()
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Callback(() => { callCount++; })
				.Returns(1);

			sut.Object.Method4(1, 2, 3, 4);

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
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method4(With.Matching<int>(v => isMatch1), With.Matching<int>(v => isMatch2),
					With.Matching<int>(v => isMatch3), With.Matching<int>(v => isMatch4))
				.Callback(() => { callCount++; });

			sut.Object.Method4(1, 2, 3, 4);

			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task Callback_ShouldNotExecuteWhenOtherMethodIsInvoked()
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Callback(() => { callCount++; });

			sut.Object.Method3(1, 2, 3);
			sut.Object.Method4(1, 2, 3, false);

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
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Callback((v1, v2, v3, v4) =>
				{
					callCount++;
					receivedValue1 = v1;
					receivedValue2 = v2;
					receivedValue3 = v3;
					receivedValue4 = v4;
				});

			sut.Object.Method4(2, 4, 6, 8);

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
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method4(With.Matching<int>(v => isMatch1), With.Matching<int>(v => isMatch2),
					With.Matching<int>(v => isMatch3), With.Matching<int>(v => isMatch4))
				.Callback((v1, v2, v3, v4) => { callCount++; });

			sut.Object.Method4(1, 2, 3, 4);

			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task CallbackWithValue_ShouldNotExecuteWhenOtherMethodIsInvoked()
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Callback((v1, v2, v3, v4) => { callCount++; });

			sut.Object.Method3(1, 2, 3);
			sut.Object.Method4(1, 2, 3, false);

			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task GetReturnValue_InvalidInputType1_ShouldThrowMockException()
		{
			MyReturnMethodSetup<int, int, int, int> setup = new("foo");
			setup.Returns((v1, v2, v3, v4) => 3 * v1 * v2 * v3 * v4);
			MethodInvocation invocation = new("foo", ["1", 2, 3, 4,]);

			void Act()
				=> setup.GetReturnValue<int>(invocation);

			await That(Act).Throws<MockException>()
				.WithMessage("""
				             The input parameter 1 only supports 'System.Int32', but is 'System.String'.
				             """);
		}

		[Fact]
		public async Task GetReturnValue_InvalidInputType2_ShouldThrowMockException()
		{
			MyReturnMethodSetup<int, int, int, int> setup = new("foo");
			setup.Returns((v1, v2, v3, v4) => 3 * v1 * v2 * v3 * v4);
			MethodInvocation invocation = new("foo", [1, "2", 3, 4,]);

			void Act()
				=> setup.GetReturnValue<int>(invocation);

			await That(Act).Throws<MockException>()
				.WithMessage("""
				             The input parameter 2 only supports 'System.Int32', but is 'System.String'.
				             """);
		}

		[Fact]
		public async Task GetReturnValue_InvalidInputType3_ShouldThrowMockException()
		{
			MyReturnMethodSetup<int, int, int, int> setup = new("foo");
			setup.Returns((v1, v2, v3, v4) => 3 * v1 * v2 * v3 * v4);
			MethodInvocation invocation = new("foo", [1, 2, "3", 4,]);

			void Act()
				=> setup.GetReturnValue<int>(invocation);

			await That(Act).Throws<MockException>()
				.WithMessage("""
				             The input parameter 3 only supports 'System.Int32', but is 'System.String'.
				             """);
		}

		[Fact]
		public async Task GetReturnValue_InvalidInputType4_ShouldThrowMockException()
		{
			MyReturnMethodSetup<int, int, int, int> setup = new("foo");
			setup.Returns((v1, v2, v3, v4) => 3 * v1 * v2 * v3 * v4);
			MethodInvocation invocation = new("foo", [1, 2, 3, "4",]);

			void Act()
				=> setup.GetReturnValue<int>(invocation);

			await That(Act).Throws<MockException>()
				.WithMessage("""
				             The input parameter 4 only supports 'System.Int32', but is 'System.String'.
				             """);
		}

		[Fact]
		public async Task GetReturnValue_InvalidType_ShouldThrowMockException()
		{
			int callCount = 0;
			MyReturnMethodSetup<int, int, int, int> setup = new("foo");
			setup.Callback(() => { callCount++; }).Returns(3);
			MethodInvocation invocation = new("foo", [1, 2, 3, 4,]);

			void Act()
				=> setup.GetReturnValue<string>(invocation);

			await That(Act).Throws<MockException>()
				.WithMessage("""
				             The return callback only supports 'System.Int32' and not 'System.String'.
				             """);
			await That(callCount).IsEqualTo(0).Because("The callback should only be executed on success!");
		}

		[Fact]
		public async Task MixReturnsAndThrows_ShouldIterateThroughBoth()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Returns(4)
				.Throws(new Exception("foo"))
				.Returns(() => 2);

			int result1 = sut.Object.Method4(1, 2, 3, 4);
			Exception? result2 = Record.Exception(() => sut.Object.Method4(2, 3, 4, 5));
			int result3 = sut.Object.Method4(3, 4, 5, 6);

			await That(result1).IsEqualTo(4);
			await That(result2).HasMessage("foo");
			await That(result3).IsEqualTo(2);
		}

		[Fact]
		public async Task MultipleCallbacks_ShouldAllGetInvoked()
		{
			int callCount1 = 0;
			int callCount2 = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Callback(() => { callCount1++; })
				.Callback((v1, v2, v3, v4) => { callCount2 += v1 * v2 * v3 * v4; })
				.Returns(1);

			sut.Object.Method4(1, 2, 3, 4);
			sut.Object.Method4(2, 2, 3, 4);

			await That(callCount1).IsEqualTo(2);
			await That(callCount2).IsEqualTo(72);
		}

		[Fact]
		public async Task MultipleReturns_ShouldIterateThroughAllRegisteredValues()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Returns(4)
				.Returns(() => 3)
				.Returns((v1, v2, v3, v4) => 10 + v1 + v2 + v3 + v4);

			int[] result = new int[10];
			for (int i = 0; i < 10; i++)
			{
				result[i] = sut.Object.Method4(i, 2 * i, 3 * i, 4 * i);
			}

			await That(result).IsEqualTo([4, 3, 30, 4, 3, 60, 4, 3, 90, 4,]);
		}

		[Fact]
		public async Task OutParameter_ShouldSet()
		{
			int receivedValue1 = 0;
			int receivedValue2 = 0;
			int receivedValue3 = 0;
			int receivedValue4 = 0;
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method4WithOutParameter(With.Out(() => 2), With.Out(() => 4), With.Out(() => 6),
					With.Out(() => 8))
				.Callback((v1, v2, v3, v4) =>
				{
					callCount++;
					receivedValue1 = v1;
					receivedValue2 = v2;
					receivedValue3 = v3;
					receivedValue4 = v4;
				});

			sut.Object.Method4WithOutParameter(out int value1, out int value2, out int value3, out int value4);

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
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method4WithRefParameter(With.Ref<int>(v => v * 10), With.Ref<int>(v => v * 10),
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
			sut.Object.Method4WithRefParameter(ref value1, ref value2, ref value3, ref value4);

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
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Returns(() => 4);

			int result = sut.Object.Method4(1, 2, 3, 4);

			await That(result).IsEqualTo(4);
		}

		[Fact]
		public async Task Returns_CallbackWithValue_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Returns((x, y, z, a) => 4 * x * y * z * a);

			int result = sut.Object.Method4(2, 3, 4, 5);

			await That(result).IsEqualTo(480);
		}

		[Fact]
		public async Task Returns_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Returns(4);

			int result = sut.Object.Method4(1, 2, 3, 4);

			await That(result).IsEqualTo(4);
		}

		[Fact]
		public async Task Returns_WithoutSetup_ShouldReturnDefault()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			int result = sut.Object.Method4(2, 3, 4, 5);

			await That(result).IsEqualTo(0);
		}

		[Fact]
		public async Task SetOutParameter_ShouldReturnDefaultValue()
		{
			MyReturnMethodSetup<int, int, int, int> setup = new("foo");

			int result = setup.SetOutParameter<int>("p1");

			await That(result).IsEqualTo(0);
		}

		[Fact]
		public async Task SetRefParameter_ShouldReturnValue()
		{
			MyReturnMethodSetup<int, int, int, int> setup = new("foo");

			int result = setup.SetRefParameter("p1", 4);

			await That(result).IsEqualTo(4);
		}

		[Fact]
		public async Task Throws_Callback_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Throws(() => new Exception("foo"));

			void Act()
				=> sut.Object.Method4(1, 2, 3, 4);

			await That(Act).ThrowsException().WithMessage("foo");
		}

		[Fact]
		public async Task Throws_CallbackWithValue_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Throws((v1, v2, v3, v4) => new Exception($"foo-{v1}-{v2}-{v3}-{v4}"));

			void Act()
				=> sut.Object.Method4(1, 2, 3, 4);

			await That(Act).ThrowsException().WithMessage("foo-1-2-3-4");
		}

		[Fact]
		public async Task Throws_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Throws(new Exception("foo"));

			void Act()
				=> sut.Object.Method4(1, 2, 3, 4);

			await That(Act).ThrowsException().WithMessage("foo");
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
				=> base.SetRefParameter<T>(parameterName, value, MockBehavior.Default);

			public T GetReturnValue<T>(MethodInvocation invocation)
				=> base.GetReturnValue<T>(invocation, MockBehavior.Default);
		}
	}

	public class With5Parameters
	{
		[Fact]
		public async Task Callback_ShouldExecuteWhenInvoked()
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method5(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Callback(() => { callCount++; })
				.Returns(1);

			sut.Object.Method5(1, 2, 3, 4, 5);

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
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method5(With.Matching<int>(v => isMatch1), With.Matching<int>(v => isMatch2),
					With.Matching<int>(v => isMatch3), With.Matching<int>(v => isMatch4),
					With.Matching<int>(v => isMatch5))
				.Callback(() => { callCount++; });

			sut.Object.Method5(1, 2, 3, 4, 5);

			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task Callback_ShouldNotExecuteWhenOtherMethodIsInvoked()
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method5(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Callback(() => { callCount++; });

			sut.Object.Method4(1, 2, 3, 4);
			sut.Object.Method5(1, 2, 3, 4, 5, false);

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
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method5(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Callback((v1, v2, v3, v4, v5) =>
				{
					callCount++;
					receivedValue1 = v1;
					receivedValue2 = v2;
					receivedValue3 = v3;
					receivedValue4 = v4;
					receivedValue5 = v5;
				});

			sut.Object.Method5(2, 4, 6, 8, 10);

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
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method5(With.Matching<int>(v => isMatch1), With.Matching<int>(v => isMatch2),
					With.Matching<int>(v => isMatch3), With.Matching<int>(v => isMatch4),
					With.Matching<int>(v => isMatch5))
				.Callback((v1, v2, v3, v4, v5) => { callCount++; });

			sut.Object.Method5(1, 2, 3, 4, 5);

			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task CallbackWithValue_ShouldNotExecuteWhenOtherMethodIsInvoked()
		{
			int callCount = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method5(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Callback((v1, v2, v3, v4, v5) => { callCount++; });

			sut.Object.Method4(1, 2, 3, 4);
			sut.Object.Method5(1, 2, 3, 4, 5, false);

			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task GetReturnValue_InvalidInputType1_ShouldThrowMockException()
		{
			MyReturnMethodSetup<int, int, int, int, int> setup = new("foo");
			setup.Returns((v1, v2, v3, v4, v5) => 3 * v1 * v2 * v3 * v4 * v5);
			MethodInvocation invocation = new("foo", ["1", 2, 3, 4, 5,]);

			void Act()
				=> setup.GetReturnValue<int>(invocation);

			await That(Act).Throws<MockException>()
				.WithMessage("""
				             The input parameter 1 only supports 'System.Int32', but is 'System.String'.
				             """);
		}

		[Fact]
		public async Task GetReturnValue_InvalidInputType2_ShouldThrowMockException()
		{
			MyReturnMethodSetup<int, int, int, int, int> setup = new("foo");
			setup.Returns((v1, v2, v3, v4, v5) => 3 * v1 * v2 * v3 * v4 * v5);
			MethodInvocation invocation = new("foo", [1, "2", 3, 4, 5,]);

			void Act()
				=> setup.GetReturnValue<int>(invocation);

			await That(Act).Throws<MockException>()
				.WithMessage("""
				             The input parameter 2 only supports 'System.Int32', but is 'System.String'.
				             """);
		}

		[Fact]
		public async Task GetReturnValue_InvalidInputType3_ShouldThrowMockException()
		{
			MyReturnMethodSetup<int, int, int, int, int> setup = new("foo");
			setup.Returns((v1, v2, v3, v4, v5) => 3 * v1 * v2 * v3 * v4 * v5);
			MethodInvocation invocation = new("foo", [1, 2, "3", 4, 5,]);

			void Act()
				=> setup.GetReturnValue<int>(invocation);

			await That(Act).Throws<MockException>()
				.WithMessage("""
				             The input parameter 3 only supports 'System.Int32', but is 'System.String'.
				             """);
		}

		[Fact]
		public async Task GetReturnValue_InvalidInputType4_ShouldThrowMockException()
		{
			MyReturnMethodSetup<int, int, int, int, int> setup = new("foo");
			setup.Returns((v1, v2, v3, v4, v5) => 3 * v1 * v2 * v3 * v4 * v5);
			MethodInvocation invocation = new("foo", [1, 2, 3, "4", 5,]);

			void Act()
				=> setup.GetReturnValue<int>(invocation);

			await That(Act).Throws<MockException>()
				.WithMessage("""
				             The input parameter 4 only supports 'System.Int32', but is 'System.String'.
				             """);
		}

		[Fact]
		public async Task GetReturnValue_InvalidInputType5_ShouldThrowMockException()
		{
			MyReturnMethodSetup<int, int, int, int, int> setup = new("foo");
			setup.Returns((v1, v2, v3, v4, v5) => 3 * v1 * v2 * v3 * v4 * v5);
			MethodInvocation invocation = new("foo", [1, 2, 3, 4, "5",]);

			void Act()
				=> setup.GetReturnValue<int>(invocation);

			await That(Act).Throws<MockException>()
				.WithMessage("""
				             The input parameter 5 only supports 'System.Int32', but is 'System.String'.
				             """);
		}

		[Fact]
		public async Task GetReturnValue_InvalidType_ShouldThrowMockException()
		{
			int callCount = 0;
			MyReturnMethodSetup<int, int, int, int, int> setup = new("foo");
			setup.Callback(() => { callCount++; }).Returns(3);
			MethodInvocation invocation = new("foo", [1, 2, 3, 4, 5,]);

			void Act()
				=> setup.GetReturnValue<string>(invocation);

			await That(Act).Throws<MockException>()
				.WithMessage("""
				             The return callback only supports 'System.Int32' and not 'System.String'.
				             """);
			await That(callCount).IsEqualTo(0).Because("The callback should only be executed on success!");
		}

		[Fact]
		public async Task MixReturnsAndThrows_ShouldIterateThroughBoth()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method5(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Returns(4)
				.Throws(new Exception("foo"))
				.Returns(() => 2);

			int result1 = sut.Object.Method5(1, 2, 3, 4, 5);
			Exception? result2 = Record.Exception(() => sut.Object.Method5(2, 3, 4, 5, 6));
			int result3 = sut.Object.Method5(3, 4, 5, 6, 7);

			await That(result1).IsEqualTo(4);
			await That(result2).HasMessage("foo");
			await That(result3).IsEqualTo(2);
		}

		[Fact]
		public async Task MultipleCallbacks_ShouldAllGetInvoked()
		{
			int callCount1 = 0;
			int callCount2 = 0;
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method5(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Callback(() => { callCount1++; })
				.Callback((v1, v2, v3, v4, v5) => { callCount2 += v1 * v2 * v3 * v4 * v5; })
				.Returns(1);

			sut.Object.Method5(1, 2, 3, 4, 5);
			sut.Object.Method5(2, 2, 3, 4, 5);

			await That(callCount1).IsEqualTo(2);
			await That(callCount2).IsEqualTo(360);
		}

		[Fact]
		public async Task MultipleReturns_ShouldIterateThroughAllRegisteredValues()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method5(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Returns(4)
				.Returns(() => 3)
				.Returns((v1, v2, v3, v4, v5) => 10 + v1 + v2 + v3 + v4 + v5);

			int[] result = new int[10];
			for (int i = 0; i < 10; i++)
			{
				result[i] = sut.Object.Method5(i, 2 * i, 3 * i, 4 * i, 5 * i);
			}

			await That(result).IsEqualTo([4, 3, 40, 4, 3, 85, 4, 3, 130, 4,]);
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
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method5WithOutParameter(With.Out(() => 2), With.Out(() => 4), With.Out(() => 6),
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

			sut.Object.Method5WithOutParameter(out int value1, out int value2, out int value3, out int value4,
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
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method5WithRefParameter(With.Ref<int>(v => v * 10), With.Ref<int>(v => v * 10),
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
			sut.Object.Method5WithRefParameter(ref value1, ref value2, ref value3, ref value4, ref value5);

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
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method5(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Returns(() => 4);

			int result = sut.Object.Method5(1, 2, 3, 4, 5);

			await That(result).IsEqualTo(4);
		}

		[Fact]
		public async Task Returns_CallbackWithValue_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method5(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Returns((x, y, z, a, b) => 4 * x * y * z * a * b);

			int result = sut.Object.Method5(2, 3, 4, 5, 6);

			await That(result).IsEqualTo(2880);
		}

		[Fact]
		public async Task Returns_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method5(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Returns(4);

			int result = sut.Object.Method5(1, 2, 3, 4, 5);

			await That(result).IsEqualTo(4);
		}

		[Fact]
		public async Task Returns_WithoutSetup_ShouldReturnDefault()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			int result = sut.Object.Method5(2, 3, 4, 5, 6);

			await That(result).IsEqualTo(0);
		}

		[Fact]
		public async Task SetOutParameter_ShouldReturnDefaultValue()
		{
			MyReturnMethodSetup<int, int, int, int, int> setup = new("foo");

			int result = setup.SetOutParameter<int>("p1");

			await That(result).IsEqualTo(0);
		}

		[Fact]
		public async Task SetRefParameter_ShouldReturnValue()
		{
			MyReturnMethodSetup<int, int, int, int, int> setup = new("foo");

			int result = setup.SetRefParameter("p1", 4);

			await That(result).IsEqualTo(4);
		}

		[Fact]
		public async Task Throws_Callback_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method5(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Throws(() => new Exception("foo"));

			void Act()
				=> sut.Object.Method5(1, 2, 3, 4, 5);

			await That(Act).ThrowsException().WithMessage("foo");
		}

		[Fact]
		public async Task Throws_CallbackWithValue_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method5(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Throws((v1, v2, v3, v4, v5) => new Exception($"foo-{v1}-{v2}-{v3}-{v4}-{v5}"));

			void Act()
				=> sut.Object.Method5(1, 2, 3, 4, 5);

			await That(Act).ThrowsException().WithMessage("foo-1-2-3-4-5");
		}

		[Fact]
		public async Task Throws_ShouldReturnExpectedValue()
		{
			Mock<IReturnMethodSetupTest> sut = Mock.For<IReturnMethodSetupTest>();

			sut.Setup.Method5(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Throws(new Exception("foo"));

			void Act()
				=> sut.Object.Method5(1, 2, 3, 4, 5);

			await That(Act).ThrowsException().WithMessage("foo");
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
				=> base.SetRefParameter<T>(parameterName, value, MockBehavior.Default);

			public T GetReturnValue<T>(MethodInvocation invocation)
				=> base.GetReturnValue<T>(invocation, MockBehavior.Default);
		}
	}

	public interface IReturnMethodSetupTest
	{
		int Method0();
		int Method0(bool withOtherParameter);
		int Method1(int p1);
		int Method1(int p1, bool withOtherParameter);
		int Method1WithOutParameter(out int p1);
		int Method1WithRefParameter(ref int p1);
		int Method2(int p1, int p2);
		int Method2(int p1, int p2, bool withOtherParameter);
		int Method2WithOutParameter(out int p1, out int p2);
		int Method2WithRefParameter(ref int p1, ref int p2);
		int Method3(int p1, int p2, int p3);
		int Method3(int p1, int p2, int p3, bool withOtherParameter);
		int Method3WithOutParameter(out int p1, out int p2, out int p3);
		int Method3WithRefParameter(ref int p1, ref int p2, ref int p3);
		int Method4(int p1, int p2, int p3, int p4);
		int Method4(int p1, int p2, int p3, bool withOtherParameter);
		int Method4WithOutParameter(out int p1, out int p2, out int p3, out int p4);
		int Method4WithRefParameter(ref int p1, ref int p2, ref int p3, ref int p4);
		int Method5(int p1, int p2, int p3, int p4, int p5);
		int Method5(int p1, int p2, int p3, int p4, int p5, bool withOtherParameter);
		int Method5WithOutParameter(out int p1, out int p2, out int p3, out int p4, out int p5);
		int Method5WithRefParameter(ref int p1, ref int p2, ref int p3, ref int p4, ref int p5);
	}
}
