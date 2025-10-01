using System.Linq;
using Mockerade.Exceptions;
using Mockerade.Setup;

namespace Mockerade.Tests.Setup;

public class VoidMethodSetupTests
{
	[Theory]
	[InlineData("Method0")]
	[InlineData("Method1", 1)]
	[InlineData("Method2", 1, 2)]
	[InlineData("Method3", 1, 2, 3)]
	[InlineData("Method4", 1, 2, 3, 4)]
	public async Task GetReturnValue_ShouldThrowMockException(string methodName, params int[] parameters)
	{
		Mock<IVoidMethodSetupTest> sut = Mock.For<IVoidMethodSetupTest>();

		sut.Setup.Method0();
		sut.Setup.Method1(With.Any<int>());
		sut.Setup.Method2(With.Any<int>(), With.Any<int>());
		sut.Setup.Method3(With.Any<int>(), With.Any<int>(), With.Any<int>());
		sut.Setup.Method4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>());

		void Act()
			=> ((IMock)sut).Execute<int>(
				$"Mockerade.Tests.Setup.VoidMethodSetupTests.IVoidMethodSetupTest.{methodName}",
				parameters.Select(x => (object?)x).ToArray());

		await That(Act).Throws<MockException>()
			.WithMessage("The method setup does not support return values.");
	}

	public class With0Parameters
	{
		[Fact]
		public async Task Callback_ShouldExecuteWhenInvoked()
		{
			bool isCalled = false;
			Mock<IVoidMethodSetupTest> sut = Mock.For<IVoidMethodSetupTest>();

			sut.Setup.Method0().Callback(() => { isCalled = true; });

			sut.Object.Method0();

			await That(isCalled).IsTrue();
		}

		[Fact]
		public async Task Callback_ShouldNotExecuteWhenOtherMethodIsInvoked()
		{
			bool isCalled = false;
			Mock<IVoidMethodSetupTest> sut = Mock.For<IVoidMethodSetupTest>();

			sut.Setup.Method0().Callback(() => { isCalled = true; });

			sut.Object.Method1(1);
			sut.Object.Method0(false);

			await That(isCalled).IsFalse();
		}

		[Fact]
		public async Task SetOutParameter_ShouldReturnDefaultValue()
		{
			var setup = new MyVoidMethodSetup("foo");

			var result = setup.SetOutParameter<int>("p0");

			await That(result).IsEqualTo(0);
		}

		[Fact]
		public async Task SetRefParameter_ShouldReturnValue()
		{
			var setup = new MyVoidMethodSetup("foo");

			var result = setup.SetRefParameter<int>("p0", 4);

			await That(result).IsEqualTo(4);
		}

		private class MyVoidMethodSetup(string name) : VoidMethodSetup(name)
		{
			public T SetOutParameter<T>(string parameterName)
				=> base.SetOutParameter<T>(parameterName, MockBehavior.Default);
			public T SetRefParameter<T>(string parameterName, T value)
				=> base.SetRefParameter<T>(parameterName, value, MockBehavior.Default);
		}
	}

	public class With1Parameters
	{
		[Fact]
		public async Task Callback_ShouldExecuteWhenInvoked()
		{
			bool isCalled = false;
			Mock<IVoidMethodSetupTest> sut = Mock.For<IVoidMethodSetupTest>();

			sut.Setup.Method1(With.Any<int>())
				.Callback(() => { isCalled = true; });

			sut.Object.Method1(3);

			await That(isCalled).IsTrue();
		}

		[Fact]
		public async Task Callback_ShouldNotExecuteWhenOtherMethodIsInvoked()
		{
			bool isCalled = false;
			Mock<IVoidMethodSetupTest> sut = Mock.For<IVoidMethodSetupTest>();

			sut.Setup.Method1(With.Any<int>())
				.Callback(() => { isCalled = true; });

			sut.Object.Method0();
			sut.Object.Method1(2, false);

			await That(isCalled).IsFalse();
		}

		[Fact]
		public async Task Callback_ShouldNotExecuteWhenParameterDoesNotMatch()
		{
			bool isCalled = false;
			Mock<IVoidMethodSetupTest> sut = Mock.For<IVoidMethodSetupTest>();

			sut.Setup.Method1(With.Matching<int>(v => v != 1))
				.Callback(() => { isCalled = true; });

			sut.Object.Method1(1);

			await That(isCalled).IsFalse();
		}

		[Fact]
		public async Task CallbackWithValue_ShouldExecuteWhenInvoked()
		{
			bool isCalled = false;
			int receivedValue = 0;
			Mock<IVoidMethodSetupTest> sut = Mock.For<IVoidMethodSetupTest>();

			sut.Setup.Method1(With.Any<int>())
				.Callback(v =>
				{
					isCalled = true;
					receivedValue = v;
				});

			sut.Object.Method1(3);

			await That(isCalled).IsTrue();
			await That(receivedValue).IsEqualTo(3);
		}

		[Fact]
		public async Task CallbackWithValue_ShouldNotExecuteWhenOtherMethodIsInvoked()
		{
			bool isCalled = false;
			Mock<IVoidMethodSetupTest> sut = Mock.For<IVoidMethodSetupTest>();

			sut.Setup.Method1(With.Any<int>())
				.Callback(v => { isCalled = true; });

			sut.Object.Method0();
			sut.Object.Method1(2, false);

			await That(isCalled).IsFalse();
		}

		[Fact]
		public async Task CallbackWithValue_ShouldNotExecuteWhenParameterDoesNotMatch()
		{
			bool isCalled = false;
			Mock<IVoidMethodSetupTest> sut = Mock.For<IVoidMethodSetupTest>();

			sut.Setup.Method1(With.Matching<int>(v => v != 1))
				.Callback(v => { isCalled = true; });

			sut.Object.Method1(1);

			await That(isCalled).IsFalse();
		}

		[Fact]
		public async Task OutParameter_ShouldSet()
		{
			int receivedValue = 0;
			bool isCalled = false;
			Mock<IVoidMethodSetupTest> sut = Mock.For<IVoidMethodSetupTest>();

			sut.Setup.Method1WithOutParameter(With.Out(() => 3))
				.Callback(v =>
				{
					isCalled = true;
					receivedValue = v;
				});

			sut.Object.Method1WithOutParameter(out int value);

			await That(isCalled).IsTrue();
			await That(value).IsEqualTo(3);
			await That(receivedValue).IsEqualTo(0);
		}

		[Fact]
		public async Task RefParameter_ShouldSet()
		{
			int receivedValue = 0;
			bool isCalled = false;
			Mock<IVoidMethodSetupTest> sut = Mock.For<IVoidMethodSetupTest>();

			sut.Setup.Method1WithRefParameter(With.Ref<int>(v => 3))
				.Callback(v =>
				{
					isCalled = true;
					receivedValue = v;
				});

			int value = 2;
			sut.Object.Method1WithRefParameter(ref value);

			await That(isCalled).IsTrue();
			await That(value).IsEqualTo(3);
			await That(receivedValue).IsEqualTo(2);
		}

		[Fact]
		public async Task SetOutParameter_ShouldReturnDefaultValue()
		{
			var setup = new MyVoidMethodSetup<int>("foo");

			var result = setup.SetOutParameter<int>("p1");

			await That(result).IsEqualTo(0);
		}

		[Fact]
		public async Task SetRefParameter_ShouldReturnValue()
		{
			var setup = new MyVoidMethodSetup<int>("foo");

			var result = setup.SetRefParameter<int>("p1", 4);

			await That(result).IsEqualTo(4);
		}

		private class MyVoidMethodSetup<T1>(string name)
			: VoidMethodSetup<T1>(name, new With.NamedParameter("p1", With.Matching<T1>(_ => false)))
		{
			public T SetOutParameter<T>(string parameterName)
				=> base.SetOutParameter<T>(parameterName, MockBehavior.Default);
			public T SetRefParameter<T>(string parameterName, T value)
				=> base.SetRefParameter<T>(parameterName, value, MockBehavior.Default);
		}
	}

	public class With2Parameters
	{
		[Fact]
		public async Task Callback_ShouldExecuteWhenInvoked()
		{
			bool isCalled = false;
			Mock<IVoidMethodSetupTest> sut = Mock.For<IVoidMethodSetupTest>();

			sut.Setup.Method2(With.Any<int>(), With.Any<int>())
				.Callback(() => { isCalled = true; });

			sut.Object.Method2(1, 2);

			await That(isCalled).IsTrue();
		}

		[Fact]
		public async Task Callback_ShouldNotExecuteWhenOtherMethodIsInvoked()
		{
			bool isCalled = false;
			Mock<IVoidMethodSetupTest> sut = Mock.For<IVoidMethodSetupTest>();

			sut.Setup.Method2(With.Any<int>(), With.Any<int>())
				.Callback(() => { isCalled = true; });

			sut.Object.Method1(1);
			sut.Object.Method2(1, 2, false);

			await That(isCalled).IsFalse();
		}

		[Theory]
		[InlineData(false, false)]
		[InlineData(true, false)]
		[InlineData(false, true)]
		public async Task Callback_ShouldNotExecuteWhenAnyParameterDoesNotMatch(bool isMatch1, bool isMatch2)
		{
			bool isCalled = false;
			Mock<IVoidMethodSetupTest> sut = Mock.For<IVoidMethodSetupTest>();

			sut.Setup.Method2(With.Matching<int>(v => isMatch1), With.Matching<int>(v => isMatch2))
				.Callback(() => { isCalled = true; });

			sut.Object.Method2(1, 2);

			await That(isCalled).IsFalse();
		}

		[Fact]
		public async Task CallbackWithValue_ShouldExecuteWhenInvoked()
		{
			bool isCalled = false;
			int receivedValue1 = 0;
			int receivedValue2 = 0;
			Mock<IVoidMethodSetupTest> sut = Mock.For<IVoidMethodSetupTest>();

			sut.Setup.Method2(With.Any<int>(), With.Any<int>())
				.Callback((v1, v2) =>
				{
					isCalled = true;
					receivedValue1 = v1;
					receivedValue2 = v2;
				});

			sut.Object.Method2(2, 4);

			await That(isCalled).IsTrue();
			await That(receivedValue1).IsEqualTo(2);
			await That(receivedValue2).IsEqualTo(4);
		}

		[Fact]
		public async Task CallbackWithValue_ShouldNotExecuteWhenOtherMethodIsInvoked()
		{
			bool isCalled = false;
			Mock<IVoidMethodSetupTest> sut = Mock.For<IVoidMethodSetupTest>();

			sut.Setup.Method2(With.Any<int>(), With.Any<int>())
				.Callback((v1, v2) => { isCalled = true; });

			sut.Object.Method1(1);
			sut.Object.Method2(1, 2, false);

			await That(isCalled).IsFalse();
		}

		[Theory]
		[InlineData(false, false)]
		[InlineData(true, false)]
		[InlineData(false, true)]
		public async Task CallbackWithValue_ShouldNotExecuteWhenAnyParameterDoesNotMatch(bool isMatch1, bool isMatch2)
		{
			bool isCalled = false;
			Mock<IVoidMethodSetupTest> sut = Mock.For<IVoidMethodSetupTest>();

			sut.Setup.Method2(With.Matching<int>(v => isMatch1), With.Matching<int>(v => isMatch2))
				.Callback((v1, v2) => { isCalled = true; });

			sut.Object.Method2(1, 2);

			await That(isCalled).IsFalse();
		}

		[Fact]
		public async Task OutParameter_ShouldSet()
		{
			int receivedValue1 = 0;
			int receivedValue2 = 0;
			bool isCalled = false;
			Mock<IVoidMethodSetupTest> sut = Mock.For<IVoidMethodSetupTest>();

			sut.Setup.Method2WithOutParameter(With.Out(() => 2), With.Out(() => 4))
				.Callback((v1, v2) =>
				{
					isCalled = true;
					receivedValue1 = v1;
					receivedValue2 = v2;
				});

			sut.Object.Method2WithOutParameter(out int value1, out int value2);

			await That(isCalled).IsTrue();
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
			bool isCalled = false;
			Mock<IVoidMethodSetupTest> sut = Mock.For<IVoidMethodSetupTest>();

			sut.Setup.Method2WithRefParameter(With.Ref<int>(v => v * 10), With.Ref<int>(v => v * 10))
				.Callback((v1, v2) =>
				{
					isCalled = true;
					receivedValue1 = v1;
					receivedValue2 = v2;
				});

			int value1 = 2;
			int value2 = 4;
			sut.Object.Method2WithRefParameter(ref value1, ref value2);

			await That(isCalled).IsTrue();
			await That(value1).IsEqualTo(20);
			await That(receivedValue1).IsEqualTo(2);
			await That(value2).IsEqualTo(40);
			await That(receivedValue2).IsEqualTo(4);
		}

		[Fact]
		public async Task SetOutParameter_ShouldReturnDefaultValue()
		{
			var setup = new MyVoidMethodSetup<int, int>("foo");

			var result = setup.SetOutParameter<int>("p1");

			await That(result).IsEqualTo(0);
		}

		[Fact]
		public async Task SetRefParameter_ShouldReturnValue()
		{
			var setup = new MyVoidMethodSetup<int, int>("foo");

			var result = setup.SetRefParameter<int>("p1", 4);

			await That(result).IsEqualTo(4);
		}

		private class MyVoidMethodSetup<T1, T2>(string name)
			: VoidMethodSetup<T1, T2>(name,
				new With.NamedParameter("p1", With.Matching<T1>(_ => false)),
				new With.NamedParameter("p2", With.Matching<T2>(_ => false)))
		{
			public T SetOutParameter<T>(string parameterName)
				=> base.SetOutParameter<T>(parameterName, MockBehavior.Default);
			public T SetRefParameter<T>(string parameterName, T value)
				=> base.SetRefParameter<T>(parameterName, value, MockBehavior.Default);
		}
	}

	public class With3Parameters
	{
		[Fact]
		public async Task Callback_ShouldExecuteWhenInvoked()
		{
			bool isCalled = false;
			Mock<IVoidMethodSetupTest> sut = Mock.For<IVoidMethodSetupTest>();

			sut.Setup.Method3(With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Callback(() => { isCalled = true; });

			sut.Object.Method3(1, 2, 3);

			await That(isCalled).IsTrue();
		}

		[Fact]
		public async Task Callback_ShouldNotExecuteWhenOtherMethodIsInvoked()
		{
			bool isCalled = false;
			Mock<IVoidMethodSetupTest> sut = Mock.For<IVoidMethodSetupTest>();

			sut.Setup.Method3(With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Callback(() => { isCalled = true; });

			sut.Object.Method2(1, 2);
			sut.Object.Method3(1, 2, 3, false);

			await That(isCalled).IsFalse();
		}

		[Theory]
		[InlineData(false, false, false)]
		[InlineData(true, true, false)]
		[InlineData(true, false, true)]
		[InlineData(false, true, true)]
		public async Task Callback_ShouldNotExecuteWhenAnyParameterDoesNotMatch(bool isMatch1, bool isMatch2, bool isMatch3)
		{
			bool isCalled = false;
			Mock<IVoidMethodSetupTest> sut = Mock.For<IVoidMethodSetupTest>();

			sut.Setup.Method3(With.Matching<int>(v => isMatch1), With.Matching<int>(v => isMatch2), With.Matching<int>(v => isMatch3))
				.Callback(() => { isCalled = true; });

			sut.Object.Method3(1, 2, 3);

			await That(isCalled).IsFalse();
		}

		[Fact]
		public async Task CallbackWithValue_ShouldExecuteWhenInvoked()
		{
			bool isCalled = false;
			int receivedValue1 = 0;
			int receivedValue2 = 0;
			int receivedValue3 = 0;
			Mock<IVoidMethodSetupTest> sut = Mock.For<IVoidMethodSetupTest>();

			sut.Setup.Method3(With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Callback((v1, v2, v3) =>
				{
					isCalled = true;
					receivedValue1 = v1;
					receivedValue2 = v2;
					receivedValue3 = v3;
				});

			sut.Object.Method3(2, 4, 6);

			await That(isCalled).IsTrue();
			await That(receivedValue1).IsEqualTo(2);
			await That(receivedValue2).IsEqualTo(4);
			await That(receivedValue3).IsEqualTo(6);
		}

		[Fact]
		public async Task CallbackWithValue_ShouldNotExecuteWhenOtherMethodIsInvoked()
		{
			bool isCalled = false;
			Mock<IVoidMethodSetupTest> sut = Mock.For<IVoidMethodSetupTest>();

			sut.Setup.Method3(With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Callback((v1, v2, v3) => { isCalled = true; });

			sut.Object.Method2(1, 2);
			sut.Object.Method3(1, 2, 3, false);

			await That(isCalled).IsFalse();
		}

		[Theory]
		[InlineData(false, false, false)]
		[InlineData(true, true, false)]
		[InlineData(true, false, true)]
		[InlineData(false, true, true)]
		public async Task CallbackWithValue_ShouldNotExecuteWhenAnyParameterDoesNotMatch(bool isMatch1, bool isMatch2, bool isMatch3)
		{
			bool isCalled = false;
			Mock<IVoidMethodSetupTest> sut = Mock.For<IVoidMethodSetupTest>();

			sut.Setup.Method3(With.Matching<int>(v => isMatch1), With.Matching<int>(v => isMatch2), With.Matching<int>(v => isMatch3))
				.Callback((v1, v2, v3) => { isCalled = true; });

			sut.Object.Method3(1, 2, 3);

			await That(isCalled).IsFalse();
		}

		[Fact]
		public async Task OutParameter_ShouldSet()
		{
			int receivedValue1 = 0;
			int receivedValue2 = 0;
			int receivedValue3 = 0;
			bool isCalled = false;
			Mock<IVoidMethodSetupTest> sut = Mock.For<IVoidMethodSetupTest>();

			sut.Setup.Method3WithOutParameter(With.Out(() => 2), With.Out(() => 4), With.Out(() => 6))
				.Callback((v1, v2, v3) =>
				{
					isCalled = true;
					receivedValue1 = v1;
					receivedValue2 = v2;
					receivedValue3 = v3;
				});

			sut.Object.Method3WithOutParameter(out int value1, out int value2, out int value3);

			await That(isCalled).IsTrue();
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
			bool isCalled = false;
			Mock<IVoidMethodSetupTest> sut = Mock.For<IVoidMethodSetupTest>();

			sut.Setup.Method3WithRefParameter(With.Ref<int>(v => v * 10), With.Ref<int>(v => v * 10), With.Ref<int>(v => v * 10))
				.Callback((v1, v2, v3) =>
				{
					isCalled = true;
					receivedValue1 = v1;
					receivedValue2 = v2;
					receivedValue3 = v3;
				});

			int value1 = 2;
			int value2 = 4;
			int value3 = 6;
			sut.Object.Method3WithRefParameter(ref value1, ref value2, ref value3);

			await That(isCalled).IsTrue();
			await That(value1).IsEqualTo(20);
			await That(receivedValue1).IsEqualTo(2);
			await That(value2).IsEqualTo(40);
			await That(receivedValue2).IsEqualTo(4);
			await That(value3).IsEqualTo(60);
			await That(receivedValue3).IsEqualTo(6);
		}

		[Fact]
		public async Task SetOutParameter_ShouldReturnDefaultValue()
		{
			var setup = new MyVoidMethodSetup<int, int, int>("foo");

			var result = setup.SetOutParameter<int>("p1");

			await That(result).IsEqualTo(0);
		}

		[Fact]
		public async Task SetRefParameter_ShouldReturnValue()
		{
			var setup = new MyVoidMethodSetup<int, int, int>("foo");

			var result = setup.SetRefParameter<int>("p1", 4);

			await That(result).IsEqualTo(4);
		}

		private class MyVoidMethodSetup<T1, T2, T3>(string name)
			: VoidMethodSetup<T1, T2, T3>(name,
				new With.NamedParameter("p1", With.Matching<T1>(_ => false)),
				new With.NamedParameter("p2", With.Matching<T2>(_ => false)),
				new With.NamedParameter("p3", With.Matching<T3>(_ => false)))
		{
			public T SetOutParameter<T>(string parameterName)
				=> base.SetOutParameter<T>(parameterName, MockBehavior.Default);
			public T SetRefParameter<T>(string parameterName, T value)
				=> base.SetRefParameter<T>(parameterName, value, MockBehavior.Default);
		}
	}

	public class With4Parameters
	{
		[Fact]
		public async Task Callback_ShouldExecuteWhenInvoked()
		{
			bool isCalled = false;
			Mock<IVoidMethodSetupTest> sut = Mock.For<IVoidMethodSetupTest>();

			sut.Setup.Method4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Callback(() => { isCalled = true; });

			sut.Object.Method4(1, 2, 3, 4);

			await That(isCalled).IsTrue();
		}

		[Fact]
		public async Task Callback_ShouldNotExecuteWhenOtherMethodIsInvoked()
		{
			bool isCalled = false;
			Mock<IVoidMethodSetupTest> sut = Mock.For<IVoidMethodSetupTest>();

			sut.Setup.Method4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Callback(() => { isCalled = true; });

			sut.Object.Method3(1, 2, 3);
			sut.Object.Method4(1, 2, 3, false);

			await That(isCalled).IsFalse();
		}

		[Theory]
		[InlineData(false, false, false, false)]
		[InlineData(true, true, true, false)]
		[InlineData(true, true, false, true)]
		[InlineData(true, false, true, true)]
		[InlineData(false, true, true, true)]
		public async Task Callback_ShouldNotExecuteWhenAnyParameterDoesNotMatch(bool isMatch1, bool isMatch2, bool isMatch3, bool isMatch4)
		{
			bool isCalled = false;
			Mock<IVoidMethodSetupTest> sut = Mock.For<IVoidMethodSetupTest>();

			sut.Setup.Method4(With.Matching<int>(v => isMatch1), With.Matching<int>(v => isMatch2), With.Matching<int>(v => isMatch3), With.Matching<int>(v => isMatch4))
				.Callback(() => { isCalled = true; });

			sut.Object.Method4(1, 2, 3, 4);

			await That(isCalled).IsFalse();
		}

		[Fact]
		public async Task CallbackWithValue_ShouldExecuteWhenInvoked()
		{
			bool isCalled = false;
			int receivedValue1 = 0;
			int receivedValue2 = 0;
			int receivedValue3 = 0;
			int receivedValue4 = 0;
			Mock<IVoidMethodSetupTest> sut = Mock.For<IVoidMethodSetupTest>();

			sut.Setup.Method4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Callback((v1, v2, v3, v4) =>
				{
					isCalled = true;
					receivedValue1 = v1;
					receivedValue2 = v2;
					receivedValue3 = v3;
					receivedValue4 = v4;
				});

			sut.Object.Method4(2, 4, 6, 8);

			await That(isCalled).IsTrue();
			await That(receivedValue1).IsEqualTo(2);
			await That(receivedValue2).IsEqualTo(4);
			await That(receivedValue3).IsEqualTo(6);
			await That(receivedValue4).IsEqualTo(8);
		}

		[Fact]
		public async Task CallbackWithValue_ShouldNotExecuteWhenOtherMethodIsInvoked()
		{
			bool isCalled = false;
			Mock<IVoidMethodSetupTest> sut = Mock.For<IVoidMethodSetupTest>();

			sut.Setup.Method4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
				.Callback((v1, v2, v3, v4) => { isCalled = true; });

			sut.Object.Method3(1, 2, 3);
			sut.Object.Method4(1, 2, 3, false);

			await That(isCalled).IsFalse();
		}

		[Theory]
		[InlineData(false, false, false, false)]
		[InlineData(true, true, true, false)]
		[InlineData(true, true, false, true)]
		[InlineData(true, false, true, true)]
		[InlineData(false, true, true, true)]
		public async Task CallbackWithValue_ShouldNotExecuteWhenAnyParameterDoesNotMatch(bool isMatch1, bool isMatch2, bool isMatch3, bool isMatch4)
		{
			bool isCalled = false;
			Mock<IVoidMethodSetupTest> sut = Mock.For<IVoidMethodSetupTest>();

			sut.Setup.Method4(With.Matching<int>(v => isMatch1), With.Matching<int>(v => isMatch2), With.Matching<int>(v => isMatch3), With.Matching<int>(v => isMatch4))
				.Callback((v1, v2, v3, v4) => { isCalled = true; });

			sut.Object.Method4(1, 2, 3, 4);

			await That(isCalled).IsFalse();
		}

		[Fact]
		public async Task OutParameter_ShouldSet()
		{
			int receivedValue1 = 0;
			int receivedValue2 = 0;
			int receivedValue3 = 0;
			int receivedValue4 = 0;
			bool isCalled = false;
			Mock<IVoidMethodSetupTest> sut = Mock.For<IVoidMethodSetupTest>();

			sut.Setup.Method4WithOutParameter(With.Out(() => 2), With.Out(() => 4), With.Out(() => 6), With.Out(() => 8))
				.Callback((v1, v2, v3, v4) =>
				{
					isCalled = true;
					receivedValue1 = v1;
					receivedValue2 = v2;
					receivedValue3 = v3;
					receivedValue4 = v4;
				});

			sut.Object.Method4WithOutParameter(out int value1, out int value2, out int value3, out int value4);

			await That(isCalled).IsTrue();
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
			bool isCalled = false;
			Mock<IVoidMethodSetupTest> sut = Mock.For<IVoidMethodSetupTest>();

			sut.Setup.Method4WithRefParameter(With.Ref<int>(v => v * 10), With.Ref<int>(v => v * 10), With.Ref<int>(v => v * 10), With.Ref<int>(v => v * 10))
				.Callback((v1, v2, v3, v4) =>
				{
					isCalled = true;
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

			await That(isCalled).IsTrue();
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
		public async Task SetOutParameter_ShouldReturnDefaultValue()
		{
			var setup = new MyVoidMethodSetup<int, int, int, int>("foo");

			var result = setup.SetOutParameter<int>("p1");

			await That(result).IsEqualTo(0);
		}

		[Fact]
		public async Task SetRefParameter_ShouldReturnValue()
		{
			var setup = new MyVoidMethodSetup<int, int, int, int>("foo");

			var result = setup.SetRefParameter<int>("p1", 4);

			await That(result).IsEqualTo(4);
		}

		private class MyVoidMethodSetup<T1, T2, T3, T4>(string name)
			: VoidMethodSetup<T1, T2, T3, T4>(name,
				new With.NamedParameter("p1", With.Matching<T1>(_ => false)),
				new With.NamedParameter("p2", With.Matching<T2>(_ => false)),
				new With.NamedParameter("p3", With.Matching<T3>(_ => false)),
				new With.NamedParameter("p4", With.Matching<T4>(_ => false)))
		{
			public T SetOutParameter<T>(string parameterName)
				=> base.SetOutParameter<T>(parameterName, MockBehavior.Default);
			public T SetRefParameter<T>(string parameterName, T value)
				=> base.SetRefParameter<T>(parameterName, value, MockBehavior.Default);
		}
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
	}
}
