using Mockolate.Exceptions;
using Mockolate.Interactions;
using Mockolate.Parameters;
using Mockolate.Setup;
using Mockolate.Verify;

namespace Mockolate.Tests.MockMethods;

public sealed partial class SetupMethodTests
{
	public sealed class OutRefParameterTests
	{
		public class ReturnMethodWith0Parameters
		{
			[Fact]
			public async Task GetReturnValue_InvalidType_ShouldThrowMockException()
			{
				int callCount = 0;
				MyReturnMethodSetup setup = new("foo");
				setup.Do(() => { callCount++; }).Returns(3);
				MethodInvocation invocation = new(0, "foo", Array.Empty<object?>());

				void Act()
				{
					setup.GetReturnValue<string>(invocation);
				}

				await That(Act).Throws<MockException>()
					.WithMessage("""
					             The return callback only supports 'int' and not 'string'.
					             """);
				await That(callCount).IsEqualTo(0).Because("The callback should only be executed on success!");
			}

			[Fact]
			public async Task SetOutParameter_ShouldReturnDefaultValue()
			{
				MyReturnMethodSetup setup = new("foo");

				string result = setup.SetOutParameter<string>("p0");

				await That(result).IsNullOrEmpty();
			}

			[Fact]
			public async Task SetRefParameter_ShouldReturnValue()
			{
				MyReturnMethodSetup setup = new("foo");

				string result = setup.SetRefParameter("p0", "d");

				await That(result).IsEqualTo("d");
			}

			private class MyReturnMethodSetup(string name) : ReturnMethodSetup<int>(name)
			{
				public T SetOutParameter<T>(string parameterName)
					=> base.SetOutParameter<T>(parameterName, () => default!);

				public T SetRefParameter<T>(string parameterName, T value)
					=> base.SetRefParameter(parameterName, value, MockBehavior.Default);

				public T GetReturnValue<T>(MethodInvocation invocation)
					=> base.GetReturnValue<T>(invocation, MockBehavior.Default, () => default!);
			}
		}

		public class ReturnMethodWith1Parameters
		{
			[Fact]
			public async Task GetReturnValue_InvalidInputType_ShouldThrowMockException()
			{
				MyReturnMethodSetup<int> setup = new("foo");
				setup.Returns(x => 3 * x);
				MethodInvocation invocation = new(0, "foo", ["b",]);

				void Act()
				{
					setup.GetReturnValue<int>(invocation);
				}

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
				setup.Do(() => { callCount++; }).Returns(x => 3 * x);
				MethodInvocation invocation = new(0, "foo", [2,]);

				void Act()
				{
					setup.GetReturnValue<string>(invocation);
				}

				await That(Act).Throws<MockException>()
					.WithMessage("""
					             The return callback only supports 'int' and not 'string'.
					             """);
				await That(callCount).IsEqualTo(0).Because("The callback should only be executed on success!");
			}

			[Fact]
			public async Task OutParameter_AnyParameters_ShouldSetToDefaultValue()
			{
				int receivedValue = 0;
				int callCount = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method1WithOutParameter(AnyParameters())
					.Do(v =>
					{
						callCount++;
						receivedValue = v;
					});

				sut.Method1WithOutParameter(out int value);

				await That(callCount).IsEqualTo(1);
				await That(value).IsEqualTo(0);
				await That(receivedValue).IsEqualTo(0);
			}

			[Fact]
			public async Task OutParameter_ShouldSet()
			{
				int receivedValue = 0;
				int callCount = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method1WithOutParameter(Out(() => 3))
					.Do(v =>
					{
						callCount++;
						receivedValue = v;
					});

				sut.Method1WithOutParameter(out int value);

				sut.VerifyMock.Invoked.Method1WithOutParameter(Out<int>()).Once();
				await That(callCount).IsEqualTo(1);
				await That(value).IsEqualTo(3);
				await That(receivedValue).IsEqualTo(0);
			}

			[Fact]
			public async Task RefParameter_AnyParameters_ShouldRemainUnchanged()
			{
				int receivedValue = 0;
				int callCount = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method1WithRefParameter(AnyParameters())
					.Do(v =>
					{
						callCount++;
						receivedValue = v;
					});

				int value = 2;
				sut.Method1WithRefParameter(ref value);

				await That(callCount).IsEqualTo(1);
				await That(value).IsEqualTo(2);
				await That(receivedValue).IsEqualTo(2);
			}

			[Fact]
			public async Task RefParameter_ShouldSet()
			{
				int receivedValue = 0;
				int callCount = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method1WithRefParameter(Ref<int>(v => 3))
					.Do(v =>
					{
						callCount++;
						receivedValue = v;
					});

				int value = 2;
				sut.Method1WithRefParameter(ref value);

				await That(callCount).IsEqualTo(1);
				await That(value).IsEqualTo(3);
				await That(receivedValue).IsEqualTo(2);
			}

			[Fact]
			public async Task SetOutParameter_ShouldReturnDefaultValue()
			{
				MyReturnMethodSetup<int> setup = new("foo");

				string result = setup.SetOutParameter<string>("p1");

				await That(result).IsNullOrEmpty();
			}

			[Fact]
			public async Task SetRefParameter_ShouldReturnValue()
			{
				MyReturnMethodSetup<int> setup = new("foo");

				string result = setup.SetRefParameter("p1", "d");

				await That(result).IsEqualTo("d");
			}

			private class MyReturnMethodSetup<T1>(string name)
				: ReturnMethodSetup<int, T1>(name, new NamedParameter("p1", (IParameter)With<T1>(_ => false)))
			{
				public T SetOutParameter<T>(string parameterName)
					=> base.SetOutParameter<T>(parameterName, () => default!);

				public T SetRefParameter<T>(string parameterName, T value)
					=> base.SetRefParameter(parameterName, value, MockBehavior.Default);

				public T GetReturnValue<T>(MethodInvocation invocation)
					=> base.GetReturnValue<T>(invocation, MockBehavior.Default, () => default!);
			}

			private class MyReturnMethodSetupAnyParameterCombination<T>(string name)
				: ReturnMethodSetup<Task, string>(name, AnyParameters())
			{
				public TValue HiddenSetOutParameter<TValue>(string parameterName, Func<TValue> defaultValueGenerator)
					=> SetOutParameter<TValue>(parameterName, defaultValueGenerator);

				public TValue HiddenSetRefParameter<TValue>(string parameterName, TValue value, MockBehavior behavior)
					=> SetRefParameter(parameterName, value, behavior);
			}
		}

		public class ReturnMethodWith2Parameters
		{
			[Fact]
			public async Task GetReturnValue_InvalidInputType1_ShouldThrowMockException()
			{
				MyReturnMethodSetup<int, int> setup = new("foo");
				setup.Returns((x, y) => 3 * x);
				MethodInvocation invocation = new(0, "foo", ["a", 2,]);

				void Act()
				{
					setup.GetReturnValue<int>(invocation);
				}

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
				{
					setup.GetReturnValue<int>(invocation);
				}

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
				setup.Do(() => { callCount++; }).Returns(3);
				MethodInvocation invocation = new(0, "foo", [2, 3,]);

				void Act()
				{
					setup.GetReturnValue<string>(invocation);
				}

				await That(Act).Throws<MockException>()
					.WithMessage("""
					             The return callback only supports 'int' and not 'string'.
					             """);
				await That(callCount).IsEqualTo(0).Because("The callback should only be executed on success!");
			}

			[Fact]
			public async Task OutParameter_AnyParameters_ShouldSetToDefaultValue()
			{
				int receivedValue1 = 0;
				int receivedValue2 = 0;
				int callCount = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method2WithOutParameter(AnyParameters())
					.Do((v1, v2) =>
					{
						callCount++;
						receivedValue1 = v1;
						receivedValue2 = v2;
					});

				sut.Method2WithOutParameter(out int value1, out int value2);

				await That(callCount).IsEqualTo(1);
				await That(value1).IsEqualTo(0);
				await That(receivedValue1).IsEqualTo(0);
				await That(value2).IsEqualTo(0);
				await That(receivedValue2).IsEqualTo(0);
			}

			[Fact]
			public async Task OutParameter_ShouldSet()
			{
				int receivedValue1 = 0;
				int receivedValue2 = 0;
				int callCount = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method2WithOutParameter(Out(() => 2), Out(() => 4))
					.Do((v1, v2) =>
					{
						callCount++;
						receivedValue1 = v1;
						receivedValue2 = v2;
					});

				sut.Method2WithOutParameter(out int value1, out int value2);

				await That(callCount).IsEqualTo(1);
				await That(value1).IsEqualTo(2);
				await That(receivedValue1).IsEqualTo(0);
				await That(value2).IsEqualTo(4);
				await That(receivedValue2).IsEqualTo(0);
			}

			[Fact]
			public async Task RefParameter_AnyParameters_ShouldRemainUnchanged()
			{
				int receivedValue1 = 0;
				int receivedValue2 = 0;
				int callCount = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method2WithRefParameter(AnyParameters())
					.Do((v1, v2) =>
					{
						callCount++;
						receivedValue1 = v1;
						receivedValue2 = v2;
					});

				int value1 = 2;
				int value2 = 4;
				sut.Method2WithRefParameter(ref value1, ref value2);

				await That(callCount).IsEqualTo(1);
				await That(value1).IsEqualTo(2);
				await That(receivedValue1).IsEqualTo(2);
				await That(value2).IsEqualTo(4);
				await That(receivedValue2).IsEqualTo(4);
			}

			[Fact]
			public async Task RefParameter_ShouldSet()
			{
				int receivedValue1 = 0;
				int receivedValue2 = 0;
				int callCount = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method2WithRefParameter(Ref<int>(v => v * 10), Ref<int>(v => v * 10))
					.Do((v1, v2) =>
					{
						callCount++;
						receivedValue1 = v1;
						receivedValue2 = v2;
					});

				int value1 = 2;
				int value2 = 4;
				sut.Method2WithRefParameter(ref value1, ref value2);

				await That(callCount).IsEqualTo(1);
				await That(value1).IsEqualTo(20);
				await That(receivedValue1).IsEqualTo(2);
				await That(value2).IsEqualTo(40);
				await That(receivedValue2).IsEqualTo(4);
			}

			[Fact]
			public async Task SetOutParameter_ShouldReturnDefaultValue()
			{
				MyReturnMethodSetup<int, int> setup = new("foo");

				string result = setup.SetOutParameter<string>("p1");

				await That(result).IsNullOrEmpty();
			}

			[Fact]
			public async Task SetRefParameter_ShouldReturnValue()
			{
				MyReturnMethodSetup<int, int> setup = new("foo");

				string result = setup.SetRefParameter("p1", "d");

				await That(result).IsEqualTo("d");
			}

			private class MyReturnMethodSetup<T1, T2>(string name)
				: ReturnMethodSetup<int, T1, T2>(name,
					new NamedParameter("p1", (IParameter)With<T1>(_ => false)),
					new NamedParameter("p2", (IParameter)With<T2>(_ => false)))
			{
				public T SetOutParameter<T>(string parameterName)
					=> base.SetOutParameter<T>(parameterName, () => default!);

				public T SetRefParameter<T>(string parameterName, T value)
					=> base.SetRefParameter(parameterName, value, MockBehavior.Default);

				public T GetReturnValue<T>(MethodInvocation invocation)
					=> base.GetReturnValue<T>(invocation, MockBehavior.Default, () => default!);
			}

			private class MyReturnMethodSetupAnyParameterCombination<T>(string name)
				: ReturnMethodSetup<Task, string, long>(name, AnyParameters())
			{
				public TValue HiddenSetOutParameter<TValue>(string parameterName, Func<TValue> defaultValueGenerator)
					=> SetOutParameter<TValue>(parameterName, defaultValueGenerator);

				public TValue HiddenSetRefParameter<TValue>(string parameterName, TValue value, MockBehavior behavior)
					=> SetRefParameter(parameterName, value, behavior);
			}
		}

		public class ReturnMethodWith3Parameters
		{
			[Fact]
			public async Task GetReturnValue_InvalidInputType1_ShouldThrowMockException()
			{
				MyReturnMethodSetup<int, int, int> setup = new("foo");
				setup.Returns((x, y, z) => 3 * x);
				MethodInvocation invocation = new(0, "foo", ["a", 2, 3,]);

				void Act()
				{
					setup.GetReturnValue<int>(invocation);
				}

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
				{
					setup.GetReturnValue<int>(invocation);
				}

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
				{
					setup.GetReturnValue<int>(invocation);
				}

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
				setup.Do(() => { callCount++; }).Returns(3);
				MethodInvocation invocation = new(0, "foo", [1, 2, 3,]);

				void Act()
				{
					setup.GetReturnValue<string>(invocation);
				}

				await That(Act).Throws<MockException>()
					.WithMessage("""
					             The return callback only supports 'int' and not 'string'.
					             """);
				await That(callCount).IsEqualTo(0).Because("The callback should only be executed on success!");
			}

			[Fact]
			public async Task OutParameter_AnyParameters_ShouldSetToDefaultValue()
			{
				int receivedValue1 = 0;
				int receivedValue2 = 0;
				int receivedValue3 = 0;
				int callCount = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method3WithOutParameter(AnyParameters())
					.Do((v1, v2, v3) =>
					{
						callCount++;
						receivedValue1 = v1;
						receivedValue2 = v2;
						receivedValue3 = v3;
					});

				sut.Method3WithOutParameter(out int value1, out int value2, out int value3);

				await That(callCount).IsEqualTo(1);
				await That(value1).IsEqualTo(0);
				await That(receivedValue1).IsEqualTo(0);
				await That(value2).IsEqualTo(0);
				await That(receivedValue2).IsEqualTo(0);
				await That(value3).IsEqualTo(0);
				await That(receivedValue3).IsEqualTo(0);
			}

			[Fact]
			public async Task OutParameter_ShouldSet()
			{
				int receivedValue1 = 0;
				int receivedValue2 = 0;
				int receivedValue3 = 0;
				int callCount = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method3WithOutParameter(Out(() => 2), Out(() => 4), Out(() => 6))
					.Do((v1, v2, v3) =>
					{
						callCount++;
						receivedValue1 = v1;
						receivedValue2 = v2;
						receivedValue3 = v3;
					});

				sut.Method3WithOutParameter(out int value1, out int value2, out int value3);

				await That(callCount).IsEqualTo(1);
				await That(value1).IsEqualTo(2);
				await That(receivedValue1).IsEqualTo(0);
				await That(value2).IsEqualTo(4);
				await That(receivedValue2).IsEqualTo(0);
				await That(value3).IsEqualTo(6);
				await That(receivedValue3).IsEqualTo(0);
			}

			[Fact]
			public async Task RefParameter_AnyParameters_ShouldRemainUnchanged()
			{
				int receivedValue1 = 0;
				int receivedValue2 = 0;
				int receivedValue3 = 0;
				int callCount = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method3WithRefParameter(AnyParameters())
					.Do((v1, v2, v3) =>
					{
						callCount++;
						receivedValue1 = v1;
						receivedValue2 = v2;
						receivedValue3 = v3;
					});

				int value1 = 2;
				int value2 = 4;
				int value3 = 6;
				sut.Method3WithRefParameter(ref value1, ref value2, ref value3);

				await That(callCount).IsEqualTo(1);
				await That(value1).IsEqualTo(2);
				await That(receivedValue1).IsEqualTo(2);
				await That(value2).IsEqualTo(4);
				await That(receivedValue2).IsEqualTo(4);
				await That(value3).IsEqualTo(6);
				await That(receivedValue3).IsEqualTo(6);
			}

			[Fact]
			public async Task RefParameter_ShouldSet()
			{
				int receivedValue1 = 0;
				int receivedValue2 = 0;
				int receivedValue3 = 0;
				int callCount = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method3WithRefParameter(Ref<int>(v => v * 10), Ref<int>(v => v * 10),
						Ref<int>(v => v * 10))
					.Do((v1, v2, v3) =>
					{
						callCount++;
						receivedValue1 = v1;
						receivedValue2 = v2;
						receivedValue3 = v3;
					});

				int value1 = 2;
				int value2 = 4;
				int value3 = 6;
				sut.Method3WithRefParameter(ref value1, ref value2, ref value3);

				await That(callCount).IsEqualTo(1);
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
				MyReturnMethodSetup<int, int, int> setup = new("foo");

				string result = setup.SetOutParameter<string>("p1");

				await That(result).IsNullOrEmpty();
			}

			[Fact]
			public async Task SetRefParameter_ShouldReturnValue()
			{
				MyReturnMethodSetup<int, int, int> setup = new("foo");

				string result = setup.SetRefParameter("p1", "d");

				await That(result).IsEqualTo("d");
			}

			private class MyReturnMethodSetup<T1, T2, T3>(string name)
				: ReturnMethodSetup<int, T1, T2, T3>(name,
					new NamedParameter("p1", (IParameter)With<T1>(_ => false)),
					new NamedParameter("p2", (IParameter)With<T2>(_ => false)),
					new NamedParameter("p3", (IParameter)With<T3>(_ => false)))
			{
				public T SetOutParameter<T>(string parameterName)
					=> base.SetOutParameter<T>(parameterName, () => default!);

				public T SetRefParameter<T>(string parameterName, T value)
					=> base.SetRefParameter(parameterName, value, MockBehavior.Default);

				public T GetReturnValue<T>(MethodInvocation invocation)
					=> base.GetReturnValue<T>(invocation, MockBehavior.Default, () => default!);
			}

			private class MyReturnMethodSetupAnyParameterCombination<T>(string name)
				: ReturnMethodSetup<Task, string, long, int>(name, AnyParameters())
			{
				public TValue HiddenSetOutParameter<TValue>(string parameterName, Func<TValue> defaultValueGenerator)
					=> SetOutParameter<TValue>(parameterName, defaultValueGenerator);

				public TValue HiddenSetRefParameter<TValue>(string parameterName, TValue value, MockBehavior behavior)
					=> SetRefParameter(parameterName, value, behavior);
			}
		}

		public class ReturnMethodWith4Parameters
		{
			[Fact]
			public async Task GetReturnValue_InvalidInputType1_ShouldThrowMockException()
			{
				MyReturnMethodSetup<int, int, int, int> setup = new("foo");
				setup.Returns((v1, v2, v3, v4) => 3 * v1 * v2 * v3 * v4);
				MethodInvocation invocation = new(0, "foo", ["a", 2, 3, 4,]);

				void Act()
				{
					setup.GetReturnValue<int>(invocation);
				}

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
				{
					setup.GetReturnValue<int>(invocation);
				}

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
				{
					setup.GetReturnValue<int>(invocation);
				}

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
				{
					setup.GetReturnValue<int>(invocation);
				}

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
				setup.Do(() => { callCount++; }).Returns(3);
				MethodInvocation invocation = new(0, "foo", [1, 2, 3, 4,]);

				void Act()
				{
					setup.GetReturnValue<string>(invocation);
				}

				await That(Act).Throws<MockException>()
					.WithMessage("""
					             The return callback only supports 'int' and not 'string'.
					             """);
				await That(callCount).IsEqualTo(0).Because("The callback should only be executed on success!");
			}

			[Fact]
			public async Task OutParameter_AnyParameters_ShouldSetToDefaultValue()
			{
				int receivedValue1 = 0;
				int receivedValue2 = 0;
				int receivedValue3 = 0;
				int receivedValue4 = 0;
				int callCount = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method4WithOutParameter(AnyParameters())
					.Do((v1, v2, v3, v4) =>
					{
						callCount++;
						receivedValue1 = v1;
						receivedValue2 = v2;
						receivedValue3 = v3;
						receivedValue4 = v4;
					});

				sut.Method4WithOutParameter(out int value1, out int value2, out int value3, out int value4);

				await That(callCount).IsEqualTo(1);
				await That(value1).IsEqualTo(0);
				await That(receivedValue1).IsEqualTo(0);
				await That(value2).IsEqualTo(0);
				await That(receivedValue2).IsEqualTo(0);
				await That(value3).IsEqualTo(0);
				await That(receivedValue3).IsEqualTo(0);
				await That(value4).IsEqualTo(0);
				await That(receivedValue4).IsEqualTo(0);
			}

			[Fact]
			public async Task OutParameter_ShouldSet()
			{
				int receivedValue1 = 0;
				int receivedValue2 = 0;
				int receivedValue3 = 0;
				int receivedValue4 = 0;
				int callCount = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method4WithOutParameter(Out(() => 2), Out(() => 4), Out(() => 6),
						Out(() => 8))
					.Do((v1, v2, v3, v4) =>
					{
						callCount++;
						receivedValue1 = v1;
						receivedValue2 = v2;
						receivedValue3 = v3;
						receivedValue4 = v4;
					});

				sut.Method4WithOutParameter(out int value1, out int value2, out int value3, out int value4);

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
			public async Task RefParameter_AnyParameters_ShouldRemainUnchanged()
			{
				int receivedValue1 = 0;
				int receivedValue2 = 0;
				int receivedValue3 = 0;
				int receivedValue4 = 0;
				int callCount = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method4WithRefParameter(AnyParameters())
					.Do((v1, v2, v3, v4) =>
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
				sut.Method4WithRefParameter(ref value1, ref value2, ref value3, ref value4);

				await That(callCount).IsEqualTo(1);
				await That(value1).IsEqualTo(2);
				await That(receivedValue1).IsEqualTo(2);
				await That(value2).IsEqualTo(4);
				await That(receivedValue2).IsEqualTo(4);
				await That(value3).IsEqualTo(6);
				await That(receivedValue3).IsEqualTo(6);
				await That(value4).IsEqualTo(8);
				await That(receivedValue4).IsEqualTo(8);
			}

			[Fact]
			public async Task RefParameter_ShouldSet()
			{
				int receivedValue1 = 0;
				int receivedValue2 = 0;
				int receivedValue3 = 0;
				int receivedValue4 = 0;
				int callCount = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method4WithRefParameter(Ref<int>(v => v * 10), Ref<int>(v => v * 10),
						Ref<int>(v => v * 10), Ref<int>(v => v * 10))
					.Do((v1, v2, v3, v4) =>
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
				sut.Method4WithRefParameter(ref value1, ref value2, ref value3, ref value4);

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
			public async Task SetOutParameter_ShouldReturnDefaultValue()
			{
				MyReturnMethodSetup<int, int, int, int> setup = new("foo");

				string result = setup.SetOutParameter<string>("p1");

				await That(result).IsNullOrEmpty();
			}

			[Fact]
			public async Task SetRefParameter_ShouldReturnValue()
			{
				MyReturnMethodSetup<int, int, int, int> setup = new("foo");

				string result = setup.SetRefParameter("p1", "d");

				await That(result).IsEqualTo("d");
			}

			private class MyReturnMethodSetup<T1, T2, T3, T4>(string name)
				: ReturnMethodSetup<int, T1, T2, T3, T4>(name,
					new NamedParameter("p1", (IParameter)With<T1>(_ => false)),
					new NamedParameter("p2", (IParameter)With<T2>(_ => false)),
					new NamedParameter("p3", (IParameter)With<T3>(_ => false)),
					new NamedParameter("p4", (IParameter)With<T4>(_ => false)))
			{
				public T SetOutParameter<T>(string parameterName)
					=> base.SetOutParameter<T>(parameterName, () => default!);

				public T SetRefParameter<T>(string parameterName, T value)
					=> base.SetRefParameter(parameterName, value, MockBehavior.Default);

				public T GetReturnValue<T>(MethodInvocation invocation)
					=> base.GetReturnValue<T>(invocation, MockBehavior.Default, () => default!);
			}

			private class MyReturnMethodSetupAnyParameterCombination<T>(string name)
				: ReturnMethodSetup<Task, string, long, int, int>(name, AnyParameters())
			{
				public TValue HiddenSetOutParameter<TValue>(string parameterName, Func<TValue> defaultValueGenerator)
					=> SetOutParameter<TValue>(parameterName, defaultValueGenerator);

				public TValue HiddenSetRefParameter<TValue>(string parameterName, TValue value, MockBehavior behavior)
					=> SetRefParameter(parameterName, value, behavior);
			}
		}

		public class ReturnMethodWith5Parameters
		{
			[Fact]
			public async Task GetReturnValue_InvalidInputType1_ShouldThrowMockException()
			{
				MyReturnMethodSetup<int, int, int, int, int> setup = new("foo");
				setup.Returns((v1, v2, v3, v4, v5) => 3 * v1 * v2 * v3 * v4 * v5);
				MethodInvocation invocation = new(0, "foo", ["a", 2, 3, 4, 5,]);

				void Act()
				{
					setup.GetReturnValue<int>(invocation);
				}

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
				{
					setup.GetReturnValue<int>(invocation);
				}

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
				{
					setup.GetReturnValue<int>(invocation);
				}

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
				{
					setup.GetReturnValue<int>(invocation);
				}

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
				{
					setup.GetReturnValue<int>(invocation);
				}

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
				setup.Do(() => { callCount++; }).Returns(3);
				MethodInvocation invocation = new(0, "foo", [1, 2, 3, 4, 5,]);

				void Act()
				{
					setup.GetReturnValue<string>(invocation);
				}

				await That(Act).Throws<MockException>()
					.WithMessage("""
					             The return callback only supports 'int' and not 'string'.
					             """);
				await That(callCount).IsEqualTo(0).Because("The callback should only be executed on success!");
			}

			[Fact]
			public async Task OutParameter_AnyParameters_ShouldSetToDefaultValue()
			{
				int receivedValue1 = 0;
				int receivedValue2 = 0;
				int receivedValue3 = 0;
				int receivedValue4 = 0;
				int receivedValue5 = 0;
				int callCount = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method5WithOutParameter(AnyParameters())
					.Do((v1, v2, v3, v4, v5) =>
					{
						callCount++;
						receivedValue1 = v1;
						receivedValue2 = v2;
						receivedValue3 = v3;
						receivedValue4 = v4;
						receivedValue5 = v5;
					});

				sut.Method5WithOutParameter(out int value1, out int value2, out int value3, out int value4,
					out int value5);

				await That(callCount).IsEqualTo(1);
				await That(value1).IsEqualTo(0);
				await That(receivedValue1).IsEqualTo(0);
				await That(value2).IsEqualTo(0);
				await That(receivedValue2).IsEqualTo(0);
				await That(value3).IsEqualTo(0);
				await That(receivedValue3).IsEqualTo(0);
				await That(value4).IsEqualTo(0);
				await That(receivedValue4).IsEqualTo(0);
				await That(value5).IsEqualTo(0);
				await That(receivedValue5).IsEqualTo(0);
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
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method5WithOutParameter(Out(() => 2), Out(() => 4), Out(() => 6),
						Out(() => 8), Out(() => 10))
					.Do((v1, v2, v3, v4, v5) =>
					{
						callCount++;
						receivedValue1 = v1;
						receivedValue2 = v2;
						receivedValue3 = v3;
						receivedValue4 = v4;
						receivedValue5 = v5;
					});

				sut.Method5WithOutParameter(out int value1, out int value2, out int value3, out int value4,
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
			public async Task RefParameter_AnyParameters_ShouldRemainUnchanged()
			{
				int receivedValue1 = 0;
				int receivedValue2 = 0;
				int receivedValue3 = 0;
				int receivedValue4 = 0;
				int receivedValue5 = 0;
				int callCount = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method5WithRefParameter(AnyParameters())
					.Do((v1, v2, v3, v4, v5) =>
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
				sut.Method5WithRefParameter(ref value1, ref value2, ref value3, ref value4, ref value5);

				await That(callCount).IsEqualTo(1);
				await That(value1).IsEqualTo(2);
				await That(receivedValue1).IsEqualTo(2);
				await That(value2).IsEqualTo(4);
				await That(receivedValue2).IsEqualTo(4);
				await That(value3).IsEqualTo(6);
				await That(receivedValue3).IsEqualTo(6);
				await That(value4).IsEqualTo(8);
				await That(receivedValue4).IsEqualTo(8);
				await That(value5).IsEqualTo(10);
				await That(receivedValue5).IsEqualTo(10);
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
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method5WithRefParameter(Ref<int>(v => v * 10), Ref<int>(v => v * 10),
						Ref<int>(v => v * 10), Ref<int>(v => v * 10), Ref<int>(v => v * 10))
					.Do((v1, v2, v3, v4, v5) =>
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
				sut.Method5WithRefParameter(ref value1, ref value2, ref value3, ref value4, ref value5);

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
			public async Task SetOutParameter_ShouldReturnDefaultValue()
			{
				MyReturnMethodSetup<int, int, int, int, int> setup = new("foo");

				string result = setup.SetOutParameter<string>("p1");

				await That(result).IsNullOrEmpty();
			}

			[Fact]
			public async Task SetRefParameter_ShouldReturnValue()
			{
				MyReturnMethodSetup<int, int, int, int, int> setup = new("foo");

				string result = setup.SetRefParameter("p1", "d");

				await That(result).IsEqualTo("d");
			}

			private class MyReturnMethodSetup<T1, T2, T3, T4, T5>(string name)
				: ReturnMethodSetup<int, T1, T2, T3, T4, T5>(name,
					new NamedParameter("p1", (IParameter)With<T1>(_ => false)),
					new NamedParameter("p2", (IParameter)With<T2>(_ => false)),
					new NamedParameter("p3", (IParameter)With<T3>(_ => false)),
					new NamedParameter("p4", (IParameter)With<T4>(_ => false)),
					new NamedParameter("p5", (IParameter)With<T5>(_ => false)))
			{
				public T SetOutParameter<T>(string parameterName)
					=> base.SetOutParameter<T>(parameterName, () => default!);

				public T SetRefParameter<T>(string parameterName, T value)
					=> base.SetRefParameter(parameterName, value, MockBehavior.Default);

				public T GetReturnValue<T>(MethodInvocation invocation)
					=> base.GetReturnValue<T>(invocation, MockBehavior.Default, () => default!);
			}

			private class MyReturnMethodSetupAnyParameterCombination<T>(string name)
				: ReturnMethodSetup<Task, string, long, int, int, int>(name, AnyParameters())
			{
				public TValue HiddenSetOutParameter<TValue>(string parameterName, Func<TValue> defaultValueGenerator)
					=> SetOutParameter<TValue>(parameterName, defaultValueGenerator);

				public TValue HiddenSetRefParameter<TValue>(string parameterName, TValue value, MockBehavior behavior)
					=> SetRefParameter(parameterName, value, behavior);
			}
		}

		public class VoidMethodWith0Parameters
		{
			[Fact]
			public async Task SetOutParameter_ShouldReturnDefaultValue()
			{
				MyVoidMethodSetup setup = new("foo");

				int result = setup.SetOutParameter<int>("p0");

				await That(result).IsEqualTo(0);
			}

			[Fact]
			public async Task SetRefParameter_ShouldReturnValue()
			{
				MyVoidMethodSetup setup = new("foo");

				int result = setup.SetRefParameter("p0", 4);

				await That(result).IsEqualTo(4);
			}


			private class MyVoidMethodSetup(string name) : VoidMethodSetup(name)
			{
				public T SetOutParameter<T>(string parameterName)
					=> base.SetOutParameter<T>(parameterName, () => default!);

				public T SetRefParameter<T>(string parameterName, T value)
					=> base.SetRefParameter(parameterName, value, MockBehavior.Default);
			}
		}

		public class VoidMethodWith1Parameters
		{
			[Fact]
			public async Task OutParameter_AnyParameters_ShouldSetToDefaultValue()
			{
				int receivedValue = 0;
				int callCount = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method1WithOutParameter(AnyParameters())
					.Do(v =>
					{
						callCount++;
						receivedValue = v;
					});

				sut.Method1WithOutParameter(out int value);

				await That(callCount).IsEqualTo(1);
				await That(value).IsEqualTo(0);
				await That(receivedValue).IsEqualTo(0);
			}

			[Fact]
			public async Task OutParameter_ShouldSet()
			{
				int receivedValue = 0;
				int callCount = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method1WithOutParameter(Out(() => 3))
					.Do(v =>
					{
						callCount++;
						receivedValue = v;
					});

				sut.Method1WithOutParameter(out int value);

				await That(callCount).IsEqualTo(1);
				await That(value).IsEqualTo(3);
				await That(receivedValue).IsEqualTo(0);
			}

			[Fact]
			public async Task RefParameter_AnyParameters_ShouldRemainUnchanged()
			{
				int receivedValue = 0;
				int callCount = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method1WithRefParameter(AnyParameters())
					.Do(v =>
					{
						callCount++;
						receivedValue = v;
					});

				int value = 2;
				sut.Method1WithRefParameter(ref value);

				await That(callCount).IsEqualTo(1);
				await That(value).IsEqualTo(2);
				await That(receivedValue).IsEqualTo(2);
			}

			[Fact]
			public async Task RefParameter_ShouldSet()
			{
				int receivedValue = 0;
				int callCount = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method1WithRefParameter(Ref<int>(v => 3))
					.Do(v =>
					{
						callCount++;
						receivedValue = v;
					});

				int value = 2;
				sut.Method1WithRefParameter(ref value);

				await That(callCount).IsEqualTo(1);
				await That(value).IsEqualTo(3);
				await That(receivedValue).IsEqualTo(2);
			}

			[Fact]
			public async Task SetOutParameter_ShouldReturnDefaultValue()
			{
				MyVoidMethodSetup<int> setup = new("foo");

				int result = setup.SetOutParameter<int>("p1");

				await That(result).IsEqualTo(0);
			}

			[Fact]
			public async Task SetRefParameter_ShouldReturnValue()
			{
				MyVoidMethodSetup<int> setup = new("foo");

				int result = setup.SetRefParameter("p1", 4);

				await That(result).IsEqualTo(4);
			}

			private class MyVoidMethodSetup<T1>(string name)
				: VoidMethodSetup<T1>(name,
					new NamedParameter("p1", (IParameter)With<T1>(_ => false)))
			{
				public T SetOutParameter<T>(string parameterName)
					=> base.SetOutParameter<T>(parameterName, () => default!);

				public T SetRefParameter<T>(string parameterName, T value)
					=> base.SetRefParameter(parameterName, value, MockBehavior.Default);
			}

			private class MyVoidMethodSetupWithParameters(string name)
				: VoidMethodSetup<string>(name, AnyParameters())
			{
				public TValue HiddenSetOutParameter<TValue>(string parameterName, Func<TValue> defaultValueGenerator)
					=> SetOutParameter<TValue>(parameterName, defaultValueGenerator);

				public TValue HiddenSetRefParameter<TValue>(string parameterName, TValue value, MockBehavior behavior)
					=> SetRefParameter(parameterName, value, behavior);
			}
		}

		public class VoidMethodWith2Parameters
		{
			[Fact]
			public async Task OutParameter_AnyParameters_ShouldSetToDefaultValue()
			{
				int receivedValue1 = 0;
				int receivedValue2 = 0;
				int callCount = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method2WithOutParameter(AnyParameters())
					.Do((v1, v2) =>
					{
						callCount++;
						receivedValue1 = v1;
						receivedValue2 = v2;
					});

				sut.Method2WithOutParameter(out int value1, out int value2);

				await That(callCount).IsEqualTo(1);
				await That(value1).IsEqualTo(0);
				await That(receivedValue1).IsEqualTo(0);
				await That(value2).IsEqualTo(0);
				await That(receivedValue2).IsEqualTo(0);
			}

			[Fact]
			public async Task OutParameter_ShouldSet()
			{
				int receivedValue1 = 0;
				int receivedValue2 = 0;
				int callCount = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method2WithOutParameter(Out(() => 2), Out(() => 4))
					.Do((v1, v2) =>
					{
						callCount++;
						receivedValue1 = v1;
						receivedValue2 = v2;
					});

				sut.Method2WithOutParameter(out int value1, out int value2);

				await That(callCount).IsEqualTo(1);
				await That(value1).IsEqualTo(2);
				await That(receivedValue1).IsEqualTo(0);
				await That(value2).IsEqualTo(4);
				await That(receivedValue2).IsEqualTo(0);
			}

			[Fact]
			public async Task RefParameter_AnyParameters_ShouldRemainUnchanged()
			{
				int receivedValue1 = 0;
				int receivedValue2 = 0;
				int callCount = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method2WithRefParameter(AnyParameters())
					.Do((v1, v2) =>
					{
						callCount++;
						receivedValue1 = v1;
						receivedValue2 = v2;
					});

				int value1 = 2;
				int value2 = 4;
				sut.Method2WithRefParameter(ref value1, ref value2);

				await That(callCount).IsEqualTo(1);
				await That(value1).IsEqualTo(2);
				await That(receivedValue1).IsEqualTo(2);
				await That(value2).IsEqualTo(4);
				await That(receivedValue2).IsEqualTo(4);
			}

			[Fact]
			public async Task RefParameter_ShouldSet()
			{
				int receivedValue1 = 0;
				int receivedValue2 = 0;
				int callCount = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method2WithRefParameter(Ref<int>(v => v * 10), Ref<int>(v => v * 10))
					.Do((v1, v2) =>
					{
						callCount++;
						receivedValue1 = v1;
						receivedValue2 = v2;
					});

				int value1 = 2;
				int value2 = 4;
				sut.Method2WithRefParameter(ref value1, ref value2);

				await That(callCount).IsEqualTo(1);
				await That(value1).IsEqualTo(20);
				await That(receivedValue1).IsEqualTo(2);
				await That(value2).IsEqualTo(40);
				await That(receivedValue2).IsEqualTo(4);
			}

			[Fact]
			public async Task SetOutParameter_ShouldReturnDefaultValue()
			{
				MyVoidMethodSetup<int, int> setup = new("foo");

				int result = setup.SetOutParameter<int>("p1");

				await That(result).IsEqualTo(0);
			}

			[Fact]
			public async Task SetRefParameter_ShouldReturnValue()
			{
				MyVoidMethodSetup<int, int> setup = new("foo");

				int result = setup.SetRefParameter("p1", 4);

				await That(result).IsEqualTo(4);
			}

			private class MyVoidMethodSetup<T1, T2>(string name)
				: VoidMethodSetup<T1, T2>(name,
					new NamedParameter("p1", (IParameter)With<T1>(_ => false)),
					new NamedParameter("p2", (IParameter)With<T2>(_ => false)))
			{
				public T SetOutParameter<T>(string parameterName)
					=> base.SetOutParameter<T>(parameterName, () => default!);

				public T SetRefParameter<T>(string parameterName, T value)
					=> base.SetRefParameter(parameterName, value, MockBehavior.Default);
			}

			private class MyVoidMethodSetupWithParameters(string name)
				: VoidMethodSetup<string, long>(name, AnyParameters())
			{
				public TValue HiddenSetOutParameter<TValue>(string parameterName, Func<TValue> defaultValueGenerator)
					=> SetOutParameter<TValue>(parameterName, defaultValueGenerator);

				public TValue HiddenSetRefParameter<TValue>(string parameterName, TValue value, MockBehavior behavior)
					=> SetRefParameter(parameterName, value, behavior);
			}
		}

		public class VoidMethodWith3Parameters
		{
			[Fact]
			public async Task OutParameter_AnyParameters_ShouldSetToDefaultValue()
			{
				int receivedValue1 = 0;
				int receivedValue2 = 0;
				int receivedValue3 = 0;
				int callCount = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method3WithOutParameter(AnyParameters())
					.Do((v1, v2, v3) =>
					{
						callCount++;
						receivedValue1 = v1;
						receivedValue2 = v2;
						receivedValue3 = v3;
					});

				sut.Method3WithOutParameter(out int value1, out int value2, out int value3);

				await That(callCount).IsEqualTo(1);
				await That(value1).IsEqualTo(0);
				await That(receivedValue1).IsEqualTo(0);
				await That(value2).IsEqualTo(0);
				await That(receivedValue2).IsEqualTo(0);
				await That(value3).IsEqualTo(0);
				await That(receivedValue3).IsEqualTo(0);
			}

			[Fact]
			public async Task OutParameter_ShouldSet()
			{
				int receivedValue1 = 0;
				int receivedValue2 = 0;
				int receivedValue3 = 0;
				int callCount = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method3WithOutParameter(Out(() => 2), Out(() => 4), Out(() => 6))
					.Do((v1, v2, v3) =>
					{
						callCount++;
						receivedValue1 = v1;
						receivedValue2 = v2;
						receivedValue3 = v3;
					});

				sut.Method3WithOutParameter(out int value1, out int value2, out int value3);

				await That(callCount).IsEqualTo(1);
				await That(value1).IsEqualTo(2);
				await That(receivedValue1).IsEqualTo(0);
				await That(value2).IsEqualTo(4);
				await That(receivedValue2).IsEqualTo(0);
				await That(value3).IsEqualTo(6);
				await That(receivedValue3).IsEqualTo(0);
			}

			[Fact]
			public async Task RefParameter_AnyParameters_ShouldRemainUnchanged()
			{
				int receivedValue1 = 0;
				int receivedValue2 = 0;
				int receivedValue3 = 0;
				int callCount = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method3WithRefParameter(AnyParameters())
					.Do((v1, v2, v3) =>
					{
						callCount++;
						receivedValue1 = v1;
						receivedValue2 = v2;
						receivedValue3 = v3;
					});

				int value1 = 2;
				int value2 = 4;
				int value3 = 6;
				sut.Method3WithRefParameter(ref value1, ref value2, ref value3);

				await That(callCount).IsEqualTo(1);
				await That(value1).IsEqualTo(2);
				await That(receivedValue1).IsEqualTo(2);
				await That(value2).IsEqualTo(4);
				await That(receivedValue2).IsEqualTo(4);
				await That(value3).IsEqualTo(6);
				await That(receivedValue3).IsEqualTo(6);
			}

			[Fact]
			public async Task RefParameter_ShouldSet()
			{
				int receivedValue1 = 0;
				int receivedValue2 = 0;
				int receivedValue3 = 0;
				int callCount = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method3WithRefParameter(Ref<int>(v => v * 10), Ref<int>(v => v * 10),
						Ref<int>(v => v * 10))
					.Do((v1, v2, v3) =>
					{
						callCount++;
						receivedValue1 = v1;
						receivedValue2 = v2;
						receivedValue3 = v3;
					});

				int value1 = 2;
				int value2 = 4;
				int value3 = 6;
				sut.Method3WithRefParameter(ref value1, ref value2, ref value3);

				await That(callCount).IsEqualTo(1);
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
				MyVoidMethodSetup<int, int, int> setup = new("foo");

				int result = setup.SetOutParameter<int>("p1");

				await That(result).IsEqualTo(0);
			}

			[Fact]
			public async Task SetRefParameter_ShouldReturnValue()
			{
				MyVoidMethodSetup<int, int, int> setup = new("foo");

				int result = setup.SetRefParameter("p1", 4);

				await That(result).IsEqualTo(4);
			}

			private class MyVoidMethodSetup<T1, T2, T3>(string name)
				: VoidMethodSetup<T1, T2, T3>(name,
					new NamedParameter("p1", (IParameter)With<T1>(_ => false)),
					new NamedParameter("p2", (IParameter)With<T2>(_ => false)),
					new NamedParameter("p3", (IParameter)With<T3>(_ => false)))
			{
				public T SetOutParameter<T>(string parameterName)
					=> base.SetOutParameter<T>(parameterName, () => default!);

				public T SetRefParameter<T>(string parameterName, T value)
					=> base.SetRefParameter(parameterName, value, MockBehavior.Default);
			}

			private class MyVoidMethodSetupWithParameters(string name)
				: VoidMethodSetup<string, long, int>(name, AnyParameters())
			{
				public TValue HiddenSetOutParameter<TValue>(string parameterName, Func<TValue> defaultValueGenerator)
					=> SetOutParameter<TValue>(parameterName, defaultValueGenerator);

				public TValue HiddenSetRefParameter<TValue>(string parameterName, TValue value, MockBehavior behavior)
					=> SetRefParameter(parameterName, value, behavior);
			}
		}

		public class VoidMethodWith4Parameters
		{
			[Fact]
			public async Task OutParameter_AnyParameters_ShouldSetToDefaultValue()
			{
				int receivedValue1 = 0;
				int receivedValue2 = 0;
				int receivedValue3 = 0;
				int receivedValue4 = 0;
				int callCount = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method4WithOutParameter(AnyParameters())
					.Do((v1, v2, v3, v4) =>
					{
						callCount++;
						receivedValue1 = v1;
						receivedValue2 = v2;
						receivedValue3 = v3;
						receivedValue4 = v4;
					});

				sut.Method4WithOutParameter(out int value1, out int value2, out int value3, out int value4);

				await That(callCount).IsEqualTo(1);
				await That(value1).IsEqualTo(0);
				await That(receivedValue1).IsEqualTo(0);
				await That(value2).IsEqualTo(0);
				await That(receivedValue2).IsEqualTo(0);
				await That(value3).IsEqualTo(0);
				await That(receivedValue3).IsEqualTo(0);
				await That(value4).IsEqualTo(0);
				await That(receivedValue4).IsEqualTo(0);
			}

			[Fact]
			public async Task OutParameter_ShouldSet()
			{
				int receivedValue1 = 0;
				int receivedValue2 = 0;
				int receivedValue3 = 0;
				int receivedValue4 = 0;
				int callCount = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method4WithOutParameter(Out(() => 2), Out(() => 4), Out(() => 6),
						Out(() => 8))
					.Do((v1, v2, v3, v4) =>
					{
						callCount++;
						receivedValue1 = v1;
						receivedValue2 = v2;
						receivedValue3 = v3;
						receivedValue4 = v4;
					});

				sut.Method4WithOutParameter(out int value1, out int value2, out int value3, out int value4);

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
			public async Task RefParameter_AnyParameters_ShouldRemainUnchanged()
			{
				int receivedValue1 = 0;
				int receivedValue2 = 0;
				int receivedValue3 = 0;
				int receivedValue4 = 0;
				int callCount = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method4WithRefParameter(AnyParameters())
					.Do((v1, v2, v3, v4) =>
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
				sut.Method4WithRefParameter(ref value1, ref value2, ref value3, ref value4);

				await That(callCount).IsEqualTo(1);
				await That(value1).IsEqualTo(2);
				await That(receivedValue1).IsEqualTo(2);
				await That(value2).IsEqualTo(4);
				await That(receivedValue2).IsEqualTo(4);
				await That(value3).IsEqualTo(6);
				await That(receivedValue3).IsEqualTo(6);
				await That(value4).IsEqualTo(8);
				await That(receivedValue4).IsEqualTo(8);
			}

			[Fact]
			public async Task RefParameter_ShouldSet()
			{
				int receivedValue1 = 0;
				int receivedValue2 = 0;
				int receivedValue3 = 0;
				int receivedValue4 = 0;
				int callCount = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method4WithRefParameter(Ref<int>(v => v * 10), Ref<int>(v => v * 10),
						Ref<int>(v => v * 10), Ref<int>(v => v * 10))
					.Do((v1, v2, v3, v4) =>
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
				sut.Method4WithRefParameter(ref value1, ref value2, ref value3, ref value4);

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
			public async Task SetOutParameter_ShouldReturnDefaultValue()
			{
				MyVoidMethodSetup<int, int, int, int> setup = new("foo");

				int result = setup.SetOutParameter<int>("p1");

				await That(result).IsEqualTo(0);
			}

			[Fact]
			public async Task SetRefParameter_ShouldReturnValue()
			{
				MyVoidMethodSetup<int, int, int, int> setup = new("foo");

				int result = setup.SetRefParameter("p1", 4);

				await That(result).IsEqualTo(4);
			}

			private class MyVoidMethodSetup<T1, T2, T3, T4>(string name)
				: VoidMethodSetup<T1, T2, T3, T4>(name,
					new NamedParameter("p1", (IParameter)With<T1>(_ => false)),
					new NamedParameter("p2", (IParameter)With<T2>(_ => false)),
					new NamedParameter("p3", (IParameter)With<T3>(_ => false)),
					new NamedParameter("p4", (IParameter)With<T4>(_ => false)))
			{
				public T SetOutParameter<T>(string parameterName)
					=> base.SetOutParameter<T>(parameterName, () => default!);

				public T SetRefParameter<T>(string parameterName, T value)
					=> base.SetRefParameter(parameterName, value, MockBehavior.Default);
			}

			private class MyVoidMethodSetupWithParameters(string name)
				: VoidMethodSetup<string, long, int, int>(name, AnyParameters())
			{
				public TValue HiddenSetOutParameter<TValue>(string parameterName, Func<TValue> defaultValueGenerator)
					=> SetOutParameter<TValue>(parameterName, defaultValueGenerator);

				public TValue HiddenSetRefParameter<TValue>(string parameterName, TValue value, MockBehavior behavior)
					=> SetRefParameter(parameterName, value, behavior);
			}
		}

		public class VoidMethodWith5Parameters
		{
			[Fact]
			public async Task OutParameter_AnyParameters_ShouldSetToDefaultValue()
			{
				int receivedValue1 = 0;
				int receivedValue2 = 0;
				int receivedValue3 = 0;
				int receivedValue4 = 0;
				int receivedValue5 = 0;
				int callCount = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method5WithOutParameter(AnyParameters())
					.Do((v1, v2, v3, v4, v5) =>
					{
						callCount++;
						receivedValue1 = v1;
						receivedValue2 = v2;
						receivedValue3 = v3;
						receivedValue4 = v4;
						receivedValue5 = v5;
					});

				sut.Method5WithOutParameter(out int value1, out int value2, out int value3, out int value4,
					out int value5);

				await That(callCount).IsEqualTo(1);
				await That(value1).IsEqualTo(0);
				await That(receivedValue1).IsEqualTo(0);
				await That(value2).IsEqualTo(0);
				await That(receivedValue2).IsEqualTo(0);
				await That(value3).IsEqualTo(0);
				await That(receivedValue3).IsEqualTo(0);
				await That(value4).IsEqualTo(0);
				await That(receivedValue4).IsEqualTo(0);
				await That(value5).IsEqualTo(0);
				await That(receivedValue5).IsEqualTo(0);
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
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method5WithOutParameter(Out(() => 2), Out(() => 4), Out(() => 6),
						Out(() => 8), Out(() => 10))
					.Do((v1, v2, v3, v4, v5) =>
					{
						callCount++;
						receivedValue1 = v1;
						receivedValue2 = v2;
						receivedValue3 = v3;
						receivedValue4 = v4;
						receivedValue5 = v5;
					});

				sut.Method5WithOutParameter(out int value1, out int value2, out int value3, out int value4,
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
			public async Task RefParameter_AnyParameters_ShouldRemainUnchanged()
			{
				int receivedValue1 = 0;
				int receivedValue2 = 0;
				int receivedValue3 = 0;
				int receivedValue4 = 0;
				int receivedValue5 = 0;
				int callCount = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method5WithRefParameter(AnyParameters())
					.Do((v1, v2, v3, v4, v5) =>
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
				sut.Method5WithRefParameter(ref value1, ref value2, ref value3, ref value4, ref value5);

				await That(callCount).IsEqualTo(1);
				await That(value1).IsEqualTo(2);
				await That(receivedValue1).IsEqualTo(2);
				await That(value2).IsEqualTo(4);
				await That(receivedValue2).IsEqualTo(4);
				await That(value3).IsEqualTo(6);
				await That(receivedValue3).IsEqualTo(6);
				await That(value4).IsEqualTo(8);
				await That(receivedValue4).IsEqualTo(8);
				await That(value5).IsEqualTo(10);
				await That(receivedValue5).IsEqualTo(10);
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
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method5WithRefParameter(Ref<int>(v => v * 10), Ref<int>(v => v * 10),
						Ref<int>(v => v * 10), Ref<int>(v => v * 10), Ref<int>(v => v * 10))
					.Do((v1, v2, v3, v4, v5) =>
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
				sut.Method5WithRefParameter(ref value1, ref value2, ref value3, ref value4, ref value5);

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
			public async Task SetOutParameter_ShouldReturnDefaultValue()
			{
				MyVoidMethodSetup<int, int, int, int, int> setup = new("foo");

				int result = setup.SetOutParameter<int>("p1");

				await That(result).IsEqualTo(0);
			}

			[Fact]
			public async Task SetRefParameter_ShouldReturnValue()
			{
				MyVoidMethodSetup<int, int, int, int, int> setup = new("foo");

				int result = setup.SetRefParameter("p1", 4);

				await That(result).IsEqualTo(4);
			}

			private class MyVoidMethodSetup<T1, T2, T3, T4, T5>(string name)
				: VoidMethodSetup<T1, T2, T3, T4, T5>(name,
					new NamedParameter("p1", (IParameter)With<T1>(_ => false)),
					new NamedParameter("p2", (IParameter)With<T2>(_ => false)),
					new NamedParameter("p3", (IParameter)With<T3>(_ => false)),
					new NamedParameter("p4", (IParameter)With<T4>(_ => false)),
					new NamedParameter("p5", (IParameter)With<T5>(_ => false)))
			{
				public T SetOutParameter<T>(string parameterName)
					=> base.SetOutParameter<T>(parameterName, () => default!);

				public T SetRefParameter<T>(string parameterName, T value)
					=> base.SetRefParameter(parameterName, value, MockBehavior.Default);
			}

			private class MyVoidMethodSetupWithParameters(string name)
				: VoidMethodSetup<string, long, int, int, int>(name, AnyParameters())
			{
				public TValue HiddenSetOutParameter<TValue>(string parameterName, Func<TValue> defaultValueGenerator)
					=> SetOutParameter<TValue>(parameterName, defaultValueGenerator);

				public TValue HiddenSetRefParameter<TValue>(string parameterName, TValue value, MockBehavior behavior)
					=> SetRefParameter(parameterName, value, behavior);
			}
		}
	}
}
