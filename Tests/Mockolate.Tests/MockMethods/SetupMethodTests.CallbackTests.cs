using System.Collections.Generic;
using Mockolate.Parameters;
using Mockolate.Setup;

namespace Mockolate.Tests.MockMethods;

public sealed partial class SetupMethodTests
{
	public sealed class CallbackTests
	{
		public class ReturnMethodWith0Parameters
		{
			[Fact]
			public async Task Callback_ShouldExecuteWhenInvoked()
			{
				int callCount = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method0()
					.Do(() => { callCount++; })
					.Returns("a");

				sut.Method0();

				await That(callCount).IsEqualTo(1);
			}

			[Fact]
			public async Task Callback_ShouldNotExecuteWhenOtherMethodIsInvoked()
			{
				int callCount = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method0().Do(() => { callCount++; });

				sut.Method1(1);
				sut.Method0(false);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task For_InParallel_ShouldLimitMatches()
			{
				List<int> callIndices = [];
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method0()
					.Do(v => { callIndices.Add(v); }).InParallel().When(v => v > 1).For(2)
					.Returns("a");

				sut.Method0();
				sut.Method0();
				sut.Method0();
				sut.Method0();
				sut.Method0();

				await That(callIndices).IsEqualTo([2, 3,]);
			}

			[Fact]
			public async Task InParallel_ShouldInvokeParallelCallbacksAlways()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				int callCount3 = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method0()
					.Do(() => { callCount1++; })
					.Do(() => { callCount2++; }).InParallel()
					.Do(() => { callCount3++; });

				sut.Method0();
				sut.Method0();
				sut.Method0();
				sut.Method0();
				sut.Method0();

				await That(callCount1).IsEqualTo(3);
				await That(callCount2).IsEqualTo(5);
				await That(callCount3).IsEqualTo(2);
			}

			[Theory]
			[InlineData(1, 1)]
			[InlineData(2, 2)]
			[InlineData(3, 3)]
			public async Task Only_ShouldInvokeCallbacksOnlyTheGivenNumberOfTimes(int times, int expectedValue)
			{
				int callCount = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method0()
					.Do(() => { callCount++; }).Only(times);

				sut.Method0();
				sut.Method0();
				sut.Method0();
				sut.Method0();

				await That(callCount).IsEqualTo(expectedValue);
			}

			[Fact]
			public async Task Only_ShouldLimitMatches()
			{
				List<int> callIndices = [];
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method0()
					.Do(v => { callIndices.Add(v); }).When(v => v > 1).Only(2)
					.Returns("a");

				sut.Method0();
				sut.Method0();
				sut.Method0();
				sut.Method0();
				sut.Method0();

				await That(callIndices).IsEqualTo([2, 3,]);
			}

			[Fact]
			public async Task Only_ShouldStopExecutingCallbackAfterTheGivenTimes()
			{
				List<int> invocations = [];
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();
				sut.SetupMock.Method.Method0()
					.Do(i => { invocations.Add(i); })
					.Only(4);

				for (int i = 0; i < 20; i++)
				{
					sut.Method0();
				}

				await That(invocations).IsEqualTo([0, 1, 2, 3,]);
			}

			[Fact]
			public async Task Only_WithWhen_ShouldStopExecutingCallbackAfterTheGivenTimes()
			{
				List<int> invocations = [];
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();
				sut.SetupMock.Method.Method0()
					.Do(i => { invocations.Add(i); })
					.When(x => x > 2)
					.Only(4);

				for (int i = 0; i < 20; i++)
				{
					sut.Method0();
				}

				await That(invocations).IsEqualTo([3, 4, 5, 6,]);
			}

			[Fact]
			public async Task OnlyOnce_ShouldDeactivateCallbackAfterFirstExecution()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method0()
					.Do(() => { callCount1++; })
					.Do(() => { callCount2++; }).OnlyOnce();

				sut.Method0();
				sut.Method0();
				sut.Method0();
				sut.Method0();

				await That(callCount1).IsEqualTo(3);
				await That(callCount2).IsEqualTo(1);
			}

			[Fact]
			public async Task ShouldInvokeCallbacksInSequence()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method0()
					.Do(() => { callCount1++; })
					.Do(() => { callCount2++; });

				sut.Method0();
				sut.Method0();
				sut.Method0();
				sut.Method0();
				sut.Method0();

				await That(callCount1).IsEqualTo(3);
				await That(callCount2).IsEqualTo(2);
			}

			[Fact]
			public async Task When_ShouldOnlyExecuteCallbackWhenInvocationCountMatches()
			{
				List<int> invocations = [];
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();
				sut.SetupMock.Method.Method0()
					.Do(i => { invocations.Add(i); })
					.When(x => x is > 3 and < 9);

				for (int i = 0; i < 20; i++)
				{
					sut.Method0();
				}

				await That(invocations).IsEqualTo([4, 5, 6, 7, 8,]);
			}

			[Fact]
			public async Task WithoutCallback_IVoidMethodSetupCallbackBuilder_ShouldNotThrow()
			{
				IReturnMethodSetupTest mock = Mock.Create<IReturnMethodSetupTest>();
				IReturnMethodSetupCallbackBuilder<string> setup =
					(IReturnMethodSetupCallbackBuilder<string>)mock.SetupMock.Method.Method0();

				void ActWhen()
				{
					setup.When(_ => true);
				}

				void ActFor()
				{
					setup.For(2);
				}

				void ActInParallel()
				{
					setup.InParallel();
				}

				await That(ActWhen).DoesNotThrow();
				await That(ActFor).DoesNotThrow();
				await That(ActInParallel).DoesNotThrow();
			}

			[Fact]
			public async Task WithoutCallback_IVoidMethodSetupCallbackWhenBuilder_ShouldNotThrow()
			{
				IReturnMethodSetupTest mock = Mock.Create<IReturnMethodSetupTest>();
				IReturnMethodSetupCallbackWhenBuilder<string> setup =
					(IReturnMethodSetupCallbackWhenBuilder<string>)mock.SetupMock.Method.Method0();

				void ActFor()
				{
					setup.For(2);
				}

				void ActOnly()
				{
					setup.Only(2);
				}

				await That(ActFor).DoesNotThrow();
				await That(ActOnly).DoesNotThrow();
			}
		}

		public class ReturnMethodWith1Parameters
		{
			[Fact]
			public async Task Callback_ShouldExecuteWhenInvoked()
			{
				int callCount = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method1(It.IsAny<int>())
					.Do(() => { callCount++; })
					.Returns("a");

				sut.Method1(3);

				await That(callCount).IsEqualTo(1);
			}

			[Fact]
			public async Task Callback_ShouldNotExecuteWhenOtherMethodIsInvoked()
			{
				int callCount = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method1(It.IsAny<int>())
					.Do(() => { callCount++; });

				sut.Method0();
				sut.Method1(2, false);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task Callback_ShouldNotExecuteWhenParameterDoesNotMatch()
			{
				int callCount = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method1(It.Satisfies<int>(v => v != 1))
					.Do(() => { callCount++; });

				sut.Method1(1);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task CallbackWithValue_ShouldExecuteWhenInvoked()
			{
				int callCount = 0;
				int receivedValue = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method1(It.IsAny<int>())
					.Do(v =>
					{
						callCount++;
						receivedValue = v;
					});

				sut.Method1(3);

				await That(callCount).IsEqualTo(1);
				await That(receivedValue).IsEqualTo(3);
			}

			[Fact]
			public async Task CallbackWithValue_ShouldNotExecuteWhenOtherMethodIsInvoked()
			{
				int callCount = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method1(It.IsAny<int>())
					.Do(_ => { callCount++; });

				sut.Method0();
				sut.Method1(2, false);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task CallbackWithValue_ShouldNotExecuteWhenParameterDoesNotMatch()
			{
				int callCount = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method1(It.Satisfies<int>(v => v != 1))
					.Do(_ => { callCount++; });

				sut.Method1(1);

				await That(callCount).IsEqualTo(0);
			}

			[Theory]
			[InlineData(-2)]
			[InlineData(0)]
			public async Task For_LessThanOne_ShouldThrowArgumentOutOfRangeException(int times)
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				void Act()
				{
					sut.SetupMock.Method.Method1(It.IsAny<int>())
						.Do(() => { }).For(times);
				}

				await That(Act).Throws<ArgumentOutOfRangeException>()
					.WithMessage("Times must be greater than zero.").AsPrefix();
			}

			[Fact]
			public async Task InParallel_ShouldInvokeParallelCallbacksAlways()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				int callCount3 = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method1(It.IsAny<int>())
					.Do(() => { callCount1++; })
					.Do(p1 => { callCount2 += p1; }).InParallel()
					.Do(p1 => { callCount3 += p1; });

				sut.Method1(1);
				sut.Method1(2);
				sut.Method1(3);
				sut.Method1(4);

				await That(callCount1).IsEqualTo(2);
				await That(callCount2).IsEqualTo(10);
				await That(callCount3).IsEqualTo(6);
			}

			[Theory]
			[InlineData(-2)]
			[InlineData(0)]
			public async Task Only_LessThanOne_ShouldThrowArgumentOutOfRangeException(int times)
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				void Act()
				{
					sut.SetupMock.Method.Method1(It.IsAny<int>())
						.Do(() => { }).Only(times);
				}

				await That(Act).Throws<ArgumentOutOfRangeException>()
					.WithMessage("Times must be greater than zero.").AsPrefix();
			}

			[Theory]
			[InlineData(1, 1)]
			[InlineData(2, 3)]
			[InlineData(3, 6)]
			public async Task Only_ShouldInvokeCallbacksOnlyTheGivenNumberOfTimes(int times, int expectedValue)
			{
				int sum = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method1(It.IsAny<int>())
					.Do(p1 => { sum += p1; }).Only(times);

				sut.Method1(1);
				sut.Method1(2);
				sut.Method1(3);
				sut.Method1(4);

				await That(sum).IsEqualTo(expectedValue);
			}

			[Fact]
			public async Task Only_ShouldStopExecutingCallbackAfterTheGivenTimes()
			{
				List<int> invocations = [];
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();
				sut.SetupMock.Method.Method1(It.IsAny<int>())
					.Do((i, _) => { invocations.Add(i); })
					.Only(4);

				for (int i = 0; i < 20; i++)
				{
					sut.Method1(i);
				}

				await That(invocations).IsEqualTo([0, 1, 2, 3,]);
			}

			[Fact]
			public async Task Only_WithWhen_ShouldStopExecutingCallbackAfterTheGivenTimes()
			{
				List<int> invocations = [];
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();
				sut.SetupMock.Method.Method1(It.IsAny<int>())
					.Do((i, _) => { invocations.Add(i); })
					.When(x => x > 2)
					.Only(4);

				for (int i = 0; i < 20; i++)
				{
					sut.Method1(i);
				}

				await That(invocations).IsEqualTo([3, 4, 5, 6,]);
			}

			[Fact]
			public async Task OnlyOnce_ShouldDeactivateCallbackAfterFirstExecution()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method1(It.IsAny<int>())
					.Do(() => { callCount1++; })
					.Do(p1 => { callCount2 += p1; }).OnlyOnce();

				sut.Method1(1);
				sut.Method1(2);
				sut.Method1(3);
				sut.Method1(4);

				await That(callCount1).IsEqualTo(3);
				await That(callCount2).IsEqualTo(2);
			}

			[Fact]
			public async Task ParameterCallbacks_ShouldOnlyBeInvokedWhenAllMatch()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method1(
					It.Satisfies<int>(x => x > 0).Monitor(out IParameterMonitor<int> values1));

				sut.Method1(-1);
				sut.Method1(1);
				sut.Method1(0);
				sut.Method1(3);

				await That(values1.Values).IsEqualTo([1, 3,]);
			}

			[Fact]
			public async Task ShouldInvokeCallbacksInSequence()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method1(It.IsAny<int>())
					.Do(() => { callCount1++; })
					.Do(p1 => { callCount2 += p1; });

				sut.Method1(1);
				sut.Method1(2);
				sut.Method1(3);
				sut.Method1(4);

				await That(callCount1).IsEqualTo(2);
				await That(callCount2).IsEqualTo(6);
			}

			[Fact]
			public async Task When_ShouldOnlyExecuteCallbackWhenInvocationCountMatches()
			{
				List<int> invocations = [];
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();
				sut.SetupMock.Method.Method1(It.IsAny<int>())
					.Do((i, _) => { invocations.Add(i); })
					.When(x => x is > 3 and < 9);

				for (int i = 0; i < 20; i++)
				{
					sut.Method1(i);
				}

				await That(invocations).IsEqualTo([4, 5, 6, 7, 8,]);
			}

			[Fact]
			public async Task WithoutCallback_IVoidMethodSetupCallbackBuilder_ShouldNotThrow()
			{
				IReturnMethodSetupTest mock = Mock.Create<IReturnMethodSetupTest>();
				IReturnMethodSetupCallbackBuilder<string, int> setup =
					(IReturnMethodSetupCallbackBuilder<string, int>)mock.SetupMock.Method.Method1(
						It.IsAny<int>());

				void ActWhen()
				{
					setup.When(_ => true);
				}

				void ActFor()
				{
					setup.For(2);
				}

				void ActInParallel()
				{
					setup.InParallel();
				}

				await That(ActWhen).DoesNotThrow();
				await That(ActFor).DoesNotThrow();
				await That(ActInParallel).DoesNotThrow();
			}

			[Fact]
			public async Task WithoutCallback_IVoidMethodSetupCallbackWhenBuilder_ShouldNotThrow()
			{
				IReturnMethodSetupTest mock = Mock.Create<IReturnMethodSetupTest>();
				IReturnMethodSetupCallbackWhenBuilder<string, int> setup =
					(IReturnMethodSetupCallbackWhenBuilder<string, int>)mock.SetupMock.Method.Method1(
						It.IsAny<int>());

				void ActFor()
				{
					setup.For(2);
				}

				void ActOnly()
				{
					setup.Only(2);
				}

				await That(ActFor).DoesNotThrow();
				await That(ActOnly).DoesNotThrow();
			}
		}

		public class ReturnMethodWith2Parameters
		{
			[Fact]
			public async Task Callback_ShouldExecuteWhenInvoked()
			{
				int callCount = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method2(It.IsAny<int>(), It.IsAny<int>())
					.Do(() => { callCount++; })
					.Returns("a");

				sut.Method2(1, 2);

				await That(callCount).IsEqualTo(1);
			}

			[Theory]
			[InlineData(false, false)]
			[InlineData(true, false)]
			[InlineData(false, true)]
			public async Task Callback_ShouldNotExecuteWhenAnyParameterDoesNotMatch(bool isMatch1, bool isMatch2)
			{
				int callCount = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method2(It.Satisfies<int>(_ => isMatch1), It.Satisfies<int>(_ => isMatch2))
					.Do(() => { callCount++; });

				sut.Method2(1, 2);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task Callback_ShouldNotExecuteWhenOtherMethodIsInvoked()
			{
				int callCount = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method2(It.IsAny<int>(), It.IsAny<int>())
					.Do(() => { callCount++; });

				sut.Method1(1);
				sut.Method2(1, 2, false);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task CallbackWithValue_ShouldExecuteWhenInvoked()
			{
				int callCount = 0;
				int receivedValue1 = 0;
				int receivedValue2 = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method2(It.IsAny<int>(), It.IsAny<int>())
					.Do((v1, v2) =>
					{
						callCount++;
						receivedValue1 = v1;
						receivedValue2 = v2;
					});

				sut.Method2(2, 4);

				await That(callCount).IsEqualTo(1);
				await That(receivedValue1).IsEqualTo(2);
				await That(receivedValue2).IsEqualTo(4);
			}

			[Theory]
			[InlineData(false, false)]
			[InlineData(true, false)]
			[InlineData(false, true)]
			public async Task CallbackWithValue_ShouldNotExecuteWhenAnyParameterDoesNotMatch(bool isMatch1,
				bool isMatch2)
			{
				int callCount = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method2(It.Satisfies<int>(_ => isMatch1), It.Satisfies<int>(_ => isMatch2))
					.Do((_, _) => { callCount++; });

				sut.Method2(1, 2);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task CallbackWithValue_ShouldNotExecuteWhenOtherMethodIsInvoked()
			{
				int callCount = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method2(It.IsAny<int>(), It.IsAny<int>())
					.Do((_, _) => { callCount++; });

				sut.Method1(1);
				sut.Method2(1, 2, false);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task InParallel_ShouldInvokeParallelCallbacksAlways()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				int callCount3 = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method2(It.IsAny<int>(), It.IsAny<int>())
					.Do(() => { callCount1++; })
					.Do((p1, _) => { callCount2 += p1; }).InParallel()
					.Do((p1, _) => { callCount3 += p1; });

				sut.Method2(1, 2);
				sut.Method2(2, 2);
				sut.Method2(3, 2);
				sut.Method2(4, 2);

				await That(callCount1).IsEqualTo(2);
				await That(callCount2).IsEqualTo(10);
				await That(callCount3).IsEqualTo(6);
			}

			[Theory]
			[InlineData(1, 1)]
			[InlineData(2, 3)]
			[InlineData(3, 6)]
			public async Task Only_ShouldInvokeCallbacksOnlyTheGivenNumberOfTimes(int times, int expectedValue)
			{
				int sum = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method2(It.IsAny<int>(), It.IsAny<int>())
					.Do((p1, _) => { sum += p1; }).Only(times);

				sut.Method2(1, 2);
				sut.Method2(2, 2);
				sut.Method2(3, 2);
				sut.Method2(4, 2);

				await That(sum).IsEqualTo(expectedValue);
			}

			[Fact]
			public async Task Only_ShouldStopExecutingCallbackAfterTheGivenTimes()
			{
				List<int> invocations = [];
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();
				sut.SetupMock.Method.Method2(It.IsAny<int>(), It.IsAny<int>())
					.Do((i, _, _) => { invocations.Add(i); })
					.Only(4);

				for (int i = 0; i < 20; i++)
				{
					sut.Method2(i, 2 * i);
				}

				await That(invocations).IsEqualTo([0, 1, 2, 3,]);
			}

			[Fact]
			public async Task Only_WithWhen_ShouldStopExecutingCallbackAfterTheGivenTimes()
			{
				List<int> invocations = [];
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();
				sut.SetupMock.Method.Method2(It.IsAny<int>(), It.IsAny<int>())
					.Do((i, _, _) => { invocations.Add(i); })
					.When(x => x > 2)
					.Only(4);

				for (int i = 0; i < 20; i++)
				{
					sut.Method2(i, 2 * i);
				}

				await That(invocations).IsEqualTo([3, 4, 5, 6,]);
			}

			[Fact]
			public async Task OnlyOnce_ShouldDeactivateCallbackAfterFirstExecution()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method2(It.IsAny<int>(), It.IsAny<int>())
					.Do(() => { callCount1++; })
					.Do((p1, _) => { callCount2 += p1; }).OnlyOnce();

				sut.Method2(1, 2);
				sut.Method2(2, 2);
				sut.Method2(3, 2);
				sut.Method2(4, 2);

				await That(callCount1).IsEqualTo(3);
				await That(callCount2).IsEqualTo(2);
			}

			[Fact]
			public async Task ParameterCallbacks_ShouldOnlyBeInvokedWhenAllMatch()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method2(
					It.Satisfies<int>(x => x > 0).Monitor(out IParameterMonitor<int> values1),
					It.Satisfies<int>(x => x > 0).Monitor(out IParameterMonitor<int> values2));

				sut.Method2(-1, 4);
				sut.Method2(1, 4);
				sut.Method2(2, 3);
				sut.Method2(1, -1);

				await That(values1.Values).IsEqualTo([1, 2,]);
				await That(values2.Values).IsEqualTo([4, 3,]);
			}

			[Fact]
			public async Task ShouldInvokeCallbacksInSequence()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method2(It.IsAny<int>(), It.IsAny<int>())
					.Do(() => { callCount1++; })
					.Do((p1, _) => { callCount2 += p1; });

				sut.Method2(1, 2);
				sut.Method2(2, 2);
				sut.Method2(3, 2);
				sut.Method2(4, 2);

				await That(callCount1).IsEqualTo(2);
				await That(callCount2).IsEqualTo(6);
			}

			[Fact]
			public async Task When_ShouldOnlyExecuteCallbackWhenInvocationCountMatches()
			{
				List<int> invocations = [];
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();
				sut.SetupMock.Method.Method2(It.IsAny<int>(), It.IsAny<int>())
					.Do((i, _, _) => { invocations.Add(i); })
					.When(x => x is > 3 and < 9);

				for (int i = 0; i < 20; i++)
				{
					sut.Method2(i, 2 * i);
				}

				await That(invocations).IsEqualTo([4, 5, 6, 7, 8,]);
			}

			[Fact]
			public async Task WithoutCallback_IVoidMethodSetupCallbackBuilder_ShouldNotThrow()
			{
				IReturnMethodSetupTest mock = Mock.Create<IReturnMethodSetupTest>();
				IReturnMethodSetupCallbackBuilder<string, int, int> setup =
					(IReturnMethodSetupCallbackBuilder<string, int, int>)mock.SetupMock.Method.Method2(
						It.IsAny<int>(), It.IsAny<int>());

				void ActWhen()
				{
					setup.When(_ => true);
				}

				void ActFor()
				{
					setup.For(2);
				}

				void ActInParallel()
				{
					setup.InParallel();
				}

				await That(ActWhen).DoesNotThrow();
				await That(ActFor).DoesNotThrow();
				await That(ActInParallel).DoesNotThrow();
			}

			[Fact]
			public async Task WithoutCallback_IVoidMethodSetupCallbackWhenBuilder_ShouldNotThrow()
			{
				IReturnMethodSetupTest mock = Mock.Create<IReturnMethodSetupTest>();
				IReturnMethodSetupCallbackWhenBuilder<string, int, int> setup =
					(IReturnMethodSetupCallbackWhenBuilder<string, int, int>)mock.SetupMock.Method.Method2(
						It.IsAny<int>(), It.IsAny<int>());

				void ActFor()
				{
					setup.For(2);
				}

				void ActOnly()
				{
					setup.Only(2);
				}

				await That(ActFor).DoesNotThrow();
				await That(ActOnly).DoesNotThrow();
			}
		}

		public class ReturnMethodWith3Parameters
		{
			[Fact]
			public async Task Callback_ShouldExecuteWhenInvoked()
			{
				int callCount = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Do(() => { callCount++; })
					.Returns("a");

				sut.Method3(1, 2, 3);

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
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method3(It.Satisfies<int>(_ => isMatch1), It.Satisfies<int>(_ => isMatch2),
						It.Satisfies<int>(_ => isMatch3))
					.Do(() => { callCount++; });

				sut.Method3(1, 2, 3);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task Callback_ShouldNotExecuteWhenOtherMethodIsInvoked()
			{
				int callCount = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Do(() => { callCount++; });

				sut.Method2(1, 2);
				sut.Method3(1, 2, 3, false);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task CallbackWithValue_ShouldExecuteWhenInvoked()
			{
				int callCount = 0;
				int receivedValue1 = 0;
				int receivedValue2 = 0;
				int receivedValue3 = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Do((v1, v2, v3) =>
					{
						callCount++;
						receivedValue1 = v1;
						receivedValue2 = v2;
						receivedValue3 = v3;
					});

				sut.Method3(2, 4, 6);

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
			public async Task CallbackWithValue_ShouldNotExecuteWhenAnyParameterDoesNotMatch(bool isMatch1,
				bool isMatch2,
				bool isMatch3)
			{
				int callCount = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method3(It.Satisfies<int>(_ => isMatch1), It.Satisfies<int>(_ => isMatch2),
						It.Satisfies<int>(_ => isMatch3))
					.Do((_, _, _) => { callCount++; });

				sut.Method3(1, 2, 3);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task CallbackWithValue_ShouldNotExecuteWhenOtherMethodIsInvoked()
			{
				int callCount = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Do((_, _, _) => { callCount++; });

				sut.Method2(1, 2);
				sut.Method3(1, 2, 3, false);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task InParallel_ShouldInvokeParallelCallbacksAlways()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				int callCount3 = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Do(() => { callCount1++; })
					.Do((p1, _, _) => { callCount2 += p1; }).InParallel()
					.Do((p1, _, _) => { callCount3 += p1; });

				sut.Method3(1, 2, 3);
				sut.Method3(2, 2, 3);
				sut.Method3(3, 2, 3);
				sut.Method3(4, 2, 3);

				await That(callCount1).IsEqualTo(2);
				await That(callCount2).IsEqualTo(10);
				await That(callCount3).IsEqualTo(6);
			}

			[Theory]
			[InlineData(1, 1)]
			[InlineData(2, 3)]
			[InlineData(3, 6)]
			public async Task Only_ShouldInvokeCallbacksOnlyTheGivenNumberOfTimes(int times, int expectedValue)
			{
				int sum = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Do((p1, _, _) => { sum += p1; }).Only(times);

				sut.Method3(1, 2, 3);
				sut.Method3(2, 2, 3);
				sut.Method3(3, 2, 3);
				sut.Method3(4, 2, 3);

				await That(sum).IsEqualTo(expectedValue);
			}

			[Fact]
			public async Task Only_ShouldStopExecutingCallbackAfterTheGivenTimes()
			{
				List<int> invocations = [];
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();
				sut.SetupMock.Method.Method3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Do((i, _, _, _) => { invocations.Add(i); })
					.Only(4);

				for (int i = 0; i < 20; i++)
				{
					sut.Method3(i, 2 * i, 3 * i);
				}

				await That(invocations).IsEqualTo([0, 1, 2, 3,]);
			}

			[Fact]
			public async Task Only_WithWhen_ShouldStopExecutingCallbackAfterTheGivenTimes()
			{
				List<int> invocations = [];
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();
				sut.SetupMock.Method.Method3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Do((i, _, _, _) => { invocations.Add(i); })
					.When(x => x > 2)
					.Only(4);

				for (int i = 0; i < 20; i++)
				{
					sut.Method3(i, 2 * i, 3 * i);
				}

				await That(invocations).IsEqualTo([3, 4, 5, 6,]);
			}

			[Fact]
			public async Task OnlyOnce_ShouldDeactivateCallbackAfterFirstExecution()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Do(() => { callCount1++; })
					.Do((p1, _, _) => { callCount2 += p1; }).OnlyOnce();

				sut.Method3(1, 2, 3);
				sut.Method3(2, 2, 3);
				sut.Method3(3, 2, 3);
				sut.Method3(4, 2, 3);

				await That(callCount1).IsEqualTo(3);
				await That(callCount2).IsEqualTo(2);
			}

			[Fact]
			public async Task ParameterCallbacks_ShouldOnlyBeInvokedWhenAllMatch()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method3(
					It.Satisfies<int>(x => x > 0).Monitor(out IParameterMonitor<int> values1),
					It.Satisfies<int>(x => x > 0).Monitor(out IParameterMonitor<int> values2),
					It.Satisfies<int>(x => x > 0).Monitor(out IParameterMonitor<int> values3));

				sut.Method3(-1, 2, 3);
				sut.Method3(1, 2, 3);
				sut.Method3(1, -2, 3);
				sut.Method3(2, 3, 4);
				sut.Method3(1, 2, -3);

				await That(values1.Values).IsEqualTo([1, 2,]);
				await That(values2.Values).IsEqualTo([2, 3,]);
				await That(values3.Values).IsEqualTo([3, 4,]);
			}

			[Fact]
			public async Task ShouldInvokeCallbacksInSequence()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Do(() => { callCount1++; })
					.Do((p1, _, _) => { callCount2 += p1; });

				sut.Method3(1, 2, 3);
				sut.Method3(2, 2, 3);
				sut.Method3(3, 2, 3);
				sut.Method3(4, 2, 3);

				await That(callCount1).IsEqualTo(2);
				await That(callCount2).IsEqualTo(6);
			}

			[Fact]
			public async Task When_ShouldOnlyExecuteCallbackWhenInvocationCountMatches()
			{
				List<int> invocations = [];
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();
				sut.SetupMock.Method.Method3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Do((i, _, _, _) => { invocations.Add(i); })
					.When(x => x is > 3 and < 9);

				for (int i = 0; i < 20; i++)
				{
					sut.Method3(i, 2 * i, 3 * i);
				}

				await That(invocations).IsEqualTo([4, 5, 6, 7, 8,]);
			}

			[Fact]
			public async Task WithoutCallback_IVoidMethodSetupCallbackBuilder_ShouldNotThrow()
			{
				IReturnMethodSetupTest mock = Mock.Create<IReturnMethodSetupTest>();
				IReturnMethodSetupCallbackBuilder<string, int, int, int> setup =
					(IReturnMethodSetupCallbackBuilder<string, int, int, int>)mock.SetupMock.Method.Method3(
						It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>());

				void ActWhen()
				{
					setup.When(_ => true);
				}

				void ActFor()
				{
					setup.For(2);
				}

				void ActInParallel()
				{
					setup.InParallel();
				}

				await That(ActWhen).DoesNotThrow();
				await That(ActFor).DoesNotThrow();
				await That(ActInParallel).DoesNotThrow();
			}

			[Fact]
			public async Task WithoutCallback_IVoidMethodSetupCallbackWhenBuilder_ShouldNotThrow()
			{
				IReturnMethodSetupTest mock = Mock.Create<IReturnMethodSetupTest>();
				IReturnMethodSetupCallbackWhenBuilder<string, int, int, int> setup =
					(IReturnMethodSetupCallbackWhenBuilder<string, int, int, int>)mock.SetupMock.Method.Method3(
						It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>());

				void ActFor()
				{
					setup.For(2);
				}

				void ActOnly()
				{
					setup.Only(2);
				}

				await That(ActFor).DoesNotThrow();
				await That(ActOnly).DoesNotThrow();
			}
		}

		public class ReturnMethodWith4Parameters
		{
			[Fact]
			public async Task Callback_ShouldExecuteWhenInvoked()
			{
				int callCount = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Do(() => { callCount++; })
					.Returns("a");

				sut.Method4(1, 2, 3, 4);

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
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method4(It.Satisfies<int>(_ => isMatch1), It.Satisfies<int>(_ => isMatch2),
						It.Satisfies<int>(_ => isMatch3), It.Satisfies<int>(_ => isMatch4))
					.Do(() => { callCount++; });

				sut.Method4(1, 2, 3, 4);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task Callback_ShouldNotExecuteWhenOtherMethodIsInvoked()
			{
				int callCount = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Do(() => { callCount++; });

				sut.Method3(1, 2, 3);
				sut.Method4(1, 2, 3, false);

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
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Do((v1, v2, v3, v4) =>
					{
						callCount++;
						receivedValue1 = v1;
						receivedValue2 = v2;
						receivedValue3 = v3;
						receivedValue4 = v4;
					});

				sut.Method4(2, 4, 6, 8);

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
			public async Task CallbackWithValue_ShouldNotExecuteWhenAnyParameterDoesNotMatch(bool isMatch1,
				bool isMatch2,
				bool isMatch3, bool isMatch4)
			{
				int callCount = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method4(It.Satisfies<int>(_ => isMatch1), It.Satisfies<int>(_ => isMatch2),
						It.Satisfies<int>(_ => isMatch3), It.Satisfies<int>(_ => isMatch4))
					.Do((_, _, _, _) => { callCount++; });

				sut.Method4(1, 2, 3, 4);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task CallbackWithValue_ShouldNotExecuteWhenOtherMethodIsInvoked()
			{
				int callCount = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Do((_, _, _, _) => { callCount++; });

				sut.Method3(1, 2, 3);
				sut.Method4(1, 2, 3, false);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task InParallel_ShouldInvokeParallelCallbacksAlways()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				int callCount3 = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Do(() => { callCount1++; })
					.Do((p1, _, _, _) => { callCount2 += p1; }).InParallel()
					.Do((p1, _, _, _) => { callCount3 += p1; });

				sut.Method4(1, 2, 3, 4);
				sut.Method4(2, 2, 3, 4);
				sut.Method4(3, 2, 3, 4);
				sut.Method4(4, 2, 3, 4);

				await That(callCount1).IsEqualTo(2);
				await That(callCount2).IsEqualTo(10);
				await That(callCount3).IsEqualTo(6);
			}

			[Theory]
			[InlineData(1, 1)]
			[InlineData(2, 3)]
			[InlineData(3, 6)]
			public async Task Only_ShouldInvokeCallbacksOnlyTheGivenNumberOfTimes(int times, int expectedValue)
			{
				int sum = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Do((p1, _, _, _) => { sum += p1; }).Only(times);

				sut.Method4(1, 2, 3, 4);
				sut.Method4(2, 2, 3, 4);
				sut.Method4(3, 2, 3, 4);
				sut.Method4(4, 2, 3, 4);

				await That(sum).IsEqualTo(expectedValue);
			}

			[Fact]
			public async Task Only_ShouldStopExecutingCallbackAfterTheGivenTimes()
			{
				List<int> invocations = [];
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();
				sut.SetupMock.Method.Method4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Do((i, _, _, _, _) => { invocations.Add(i); })
					.Only(4);

				for (int i = 0; i < 20; i++)
				{
					sut.Method4(i, 2 * i, 3 * i, 4 * i);
				}

				await That(invocations).IsEqualTo([0, 1, 2, 3,]);
			}

			[Fact]
			public async Task Only_WithWhen_ShouldStopExecutingCallbackAfterTheGivenTimes()
			{
				List<int> invocations = [];
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();
				sut.SetupMock.Method.Method4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Do((i, _, _, _, _) => { invocations.Add(i); })
					.When(x => x > 2)
					.Only(4);

				for (int i = 0; i < 20; i++)
				{
					sut.Method4(i, 2 * i, 3 * i, 4 * i);
				}

				await That(invocations).IsEqualTo([3, 4, 5, 6,]);
			}

			[Fact]
			public async Task OnlyOnce_ShouldDeactivateCallbackAfterFirstExecution()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Do(() => { callCount1++; })
					.Do((p1, _, _, _) => { callCount2 += p1; }).OnlyOnce();

				sut.Method4(1, 2, 3, 4);
				sut.Method4(2, 2, 3, 4);
				sut.Method4(3, 2, 3, 4);
				sut.Method4(4, 2, 3, 4);

				await That(callCount1).IsEqualTo(3);
				await That(callCount2).IsEqualTo(2);
			}

			[Fact]
			public async Task ParameterCallbacks_ShouldOnlyBeInvokedWhenAllMatch()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method4(
					It.Satisfies<int>(x => x > 0).Monitor(out IParameterMonitor<int> values1),
					It.Satisfies<int>(x => x > 0).Monitor(out IParameterMonitor<int> values2),
					It.Satisfies<int>(x => x > 0).Monitor(out IParameterMonitor<int> values3),
					It.Satisfies<int>(x => x > 0).Monitor(out IParameterMonitor<int> values4));

				sut.Method4(-1, 2, 3, 4);
				sut.Method4(1, 2, 3, 4);
				sut.Method4(1, -2, 3, 4);
				sut.Method4(2, 3, 4, 5);
				sut.Method4(1, 2, -3, 4);
				sut.Method4(3, 4, 5, 6);
				sut.Method4(1, 2, 3, -4);

				await That(values1.Values).IsEqualTo([1, 2, 3,]);
				await That(values2.Values).IsEqualTo([2, 3, 4,]);
				await That(values3.Values).IsEqualTo([3, 4, 5,]);
				await That(values4.Values).IsEqualTo([4, 5, 6,]);
			}

			[Fact]
			public async Task ShouldInvokeCallbacksInSequence()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Do(() => { callCount1++; })
					.Do((p1, _, _, _) => { callCount2 += p1; });

				sut.Method4(1, 2, 3, 4);
				sut.Method4(2, 2, 3, 4);
				sut.Method4(3, 2, 3, 4);
				sut.Method4(4, 2, 3, 4);

				await That(callCount1).IsEqualTo(2);
				await That(callCount2).IsEqualTo(6);
			}

			[Fact]
			public async Task When_ShouldOnlyExecuteCallbackWhenInvocationCountMatches()
			{
				List<int> invocations = [];
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();
				sut.SetupMock.Method.Method4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Do((i, _, _, _, _) => { invocations.Add(i); })
					.When(x => x is > 3 and < 9);

				for (int i = 0; i < 20; i++)
				{
					sut.Method4(i, 2 * i, 3 * i, 4 * i);
				}

				await That(invocations).IsEqualTo([4, 5, 6, 7, 8,]);
			}

			[Fact]
			public async Task WithoutCallback_IVoidMethodSetupCallbackBuilder_ShouldNotThrow()
			{
				IReturnMethodSetupTest mock = Mock.Create<IReturnMethodSetupTest>();
				IReturnMethodSetupCallbackBuilder<string, int, int, int, int> setup =
					(IReturnMethodSetupCallbackBuilder<string, int, int, int, int>)mock.SetupMock.Method.Method4(
						It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>());

				void ActWhen()
				{
					setup.When(_ => true);
				}

				void ActFor()
				{
					setup.For(2);
				}

				void ActInParallel()
				{
					setup.InParallel();
				}

				await That(ActWhen).DoesNotThrow();
				await That(ActFor).DoesNotThrow();
				await That(ActInParallel).DoesNotThrow();
			}

			[Fact]
			public async Task WithoutCallback_IVoidMethodSetupCallbackWhenBuilder_ShouldNotThrow()
			{
				IReturnMethodSetupTest mock = Mock.Create<IReturnMethodSetupTest>();
				IReturnMethodSetupCallbackWhenBuilder<string, int, int, int, int> setup =
					(IReturnMethodSetupCallbackWhenBuilder<string, int, int, int, int>)mock.SetupMock.Method.Method4(
						It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>());

				void ActFor()
				{
					setup.For(2);
				}

				void ActOnly()
				{
					setup.Only(2);
				}

				await That(ActFor).DoesNotThrow();
				await That(ActOnly).DoesNotThrow();
			}
		}

		public class ReturnMethodWith5Parameters
		{
			[Fact]
			public async Task Callback_ShouldExecuteWhenInvoked()
			{
				int callCount = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method5(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.Do(() => { callCount++; })
					.Returns("a");

				sut.Method5(1, 2, 3, 4, 5);

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
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method5(It.Satisfies<int>(_ => isMatch1), It.Satisfies<int>(_ => isMatch2),
						It.Satisfies<int>(_ => isMatch3), It.Satisfies<int>(_ => isMatch4),
						It.Satisfies<int>(_ => isMatch5))
					.Do(() => { callCount++; });

				sut.Method5(1, 2, 3, 4, 5);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task Callback_ShouldNotExecuteWhenOtherMethodIsInvoked()
			{
				int callCount = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method5(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.Do(() => { callCount++; });

				sut.Method4(1, 2, 3, 4);
				sut.Method5(1, 2, 3, 4, 5, false);

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
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method5(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.Do((v1, v2, v3, v4, v5) =>
					{
						callCount++;
						receivedValue1 = v1;
						receivedValue2 = v2;
						receivedValue3 = v3;
						receivedValue4 = v4;
						receivedValue5 = v5;
					});

				sut.Method5(2, 4, 6, 8, 10);

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
			public async Task CallbackWithValue_ShouldNotExecuteWhenAnyParameterDoesNotMatch(bool isMatch1,
				bool isMatch2,
				bool isMatch3, bool isMatch4, bool isMatch5)
			{
				int callCount = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method5(It.Satisfies<int>(_ => isMatch1), It.Satisfies<int>(_ => isMatch2),
						It.Satisfies<int>(_ => isMatch3), It.Satisfies<int>(_ => isMatch4),
						It.Satisfies<int>(_ => isMatch5))
					.Do((_, _, _, _, _) => { callCount++; });

				sut.Method5(1, 2, 3, 4, 5);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task CallbackWithValue_ShouldNotExecuteWhenOtherMethodIsInvoked()
			{
				int callCount = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method5(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.Do((_, _, _, _, _) => { callCount++; });

				sut.Method4(1, 2, 3, 4);
				sut.Method5(1, 2, 3, 4, 5, false);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task InParallel_ShouldInvokeParallelCallbacksAlways()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				int callCount3 = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method5(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.Do(() => { callCount1++; })
					.Do((p1, _, _, _, _) => { callCount2 += p1; }).InParallel()
					.Do((p1, _, _, _, _) => { callCount3 += p1; });

				sut.Method5(1, 2, 3, 4, 5);
				sut.Method5(2, 2, 3, 4, 5);
				sut.Method5(3, 2, 3, 4, 5);
				sut.Method5(4, 2, 3, 4, 5);

				await That(callCount1).IsEqualTo(2);
				await That(callCount2).IsEqualTo(10);
				await That(callCount3).IsEqualTo(6);
			}

			[Theory]
			[InlineData(1, 1)]
			[InlineData(2, 3)]
			[InlineData(3, 6)]
			public async Task Only_ShouldInvokeCallbacksOnlyTheGivenNumberOfTimes(int times, int expectedValue)
			{
				int sum = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method5(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.Do((p1, _, _, _, _) => { sum += p1; }).Only(times);

				sut.Method5(1, 2, 3, 4, 5);
				sut.Method5(2, 2, 3, 4, 5);
				sut.Method5(3, 2, 3, 4, 5);
				sut.Method5(4, 2, 3, 4, 5);

				await That(sum).IsEqualTo(expectedValue);
			}

			[Fact]
			public async Task Only_ShouldStopExecutingCallbackAfterTheGivenTimes()
			{
				List<int> invocations = [];
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();
				sut.SetupMock.Method.Method5(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.Do((i, _, _, _, _, _) => { invocations.Add(i); })
					.Only(4);

				for (int i = 0; i < 20; i++)
				{
					sut.Method5(i, 2 * i, 3 * i, 4 * i, 5 * i);
				}

				await That(invocations).IsEqualTo([0, 1, 2, 3,]);
			}

			[Fact]
			public async Task Only_WithWhen_ShouldStopExecutingCallbackAfterTheGivenTimes()
			{
				List<int> invocations = [];
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();
				sut.SetupMock.Method.Method5(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.Do((i, _, _, _, _, _) => { invocations.Add(i); })
					.When(x => x > 2)
					.Only(4);

				for (int i = 0; i < 20; i++)
				{
					sut.Method5(i, 2 * i, 3 * i, 4 * i, 5 * i);
				}

				await That(invocations).IsEqualTo([3, 4, 5, 6,]);
			}

			[Fact]
			public async Task OnlyOnce_ShouldDeactivateCallbackAfterFirstExecution()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method5(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.Do(() => { callCount1++; })
					.Do((p1, _, _, _, _) => { callCount2 += p1; }).OnlyOnce();

				sut.Method5(1, 2, 3, 4, 5);
				sut.Method5(2, 2, 3, 4, 5);
				sut.Method5(3, 2, 3, 4, 5);
				sut.Method5(4, 2, 3, 4, 5);

				await That(callCount1).IsEqualTo(3);
				await That(callCount2).IsEqualTo(2);
			}

			[Fact]
			public async Task ParameterCallbacks_ShouldOnlyBeInvokedWhenAllMatch()
			{
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method5(
					It.Satisfies<int>(x => x > 0).Monitor(out IParameterMonitor<int> values1),
					It.Satisfies<int>(x => x > 0).Monitor(out IParameterMonitor<int> values2),
					It.Satisfies<int>(x => x > 0).Monitor(out IParameterMonitor<int> values3),
					It.Satisfies<int>(x => x > 0).Monitor(out IParameterMonitor<int> values4),
					It.Satisfies<int>(x => x > 0).Monitor(out IParameterMonitor<int> values5));

				sut.Method5(-1, 2, 3, 4, 5);
				sut.Method5(1, 2, 3, 4, 5);
				sut.Method5(1, -2, 3, 4, 5);
				sut.Method5(2, 3, 4, 5, 6);
				sut.Method5(1, 2, -3, 4, 5);
				sut.Method5(3, 4, 5, 6, 7);
				sut.Method5(1, 2, 3, -4, 5);
				sut.Method5(4, 5, 6, 7, 8);
				sut.Method5(1, 2, 3, -4, 5);

				await That(values1.Values).IsEqualTo([1, 2, 3, 4,]);
				await That(values2.Values).IsEqualTo([2, 3, 4, 5,]);
				await That(values3.Values).IsEqualTo([3, 4, 5, 6,]);
				await That(values4.Values).IsEqualTo([4, 5, 6, 7,]);
				await That(values5.Values).IsEqualTo([5, 6, 7, 8,]);
			}

			[Fact]
			public async Task ShouldInvokeCallbacksInSequence()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();

				sut.SetupMock.Method.Method5(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.Do(() => { callCount1++; })
					.Do((p1, _, _, _, _) => { callCount2 += p1; });

				sut.Method5(1, 2, 3, 4, 5);
				sut.Method5(2, 2, 3, 4, 5);
				sut.Method5(3, 2, 3, 4, 5);
				sut.Method5(4, 2, 3, 4, 5);

				await That(callCount1).IsEqualTo(2);
				await That(callCount2).IsEqualTo(6);
			}

			[Fact]
			public async Task When_ShouldOnlyExecuteCallbackWhenInvocationCountMatches()
			{
				List<int> invocations = [];
				IReturnMethodSetupTest sut = Mock.Create<IReturnMethodSetupTest>();
				sut.SetupMock.Method.Method5(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.Do((i, _, _, _, _, _) => { invocations.Add(i); })
					.When(x => x is > 3 and < 9);

				for (int i = 0; i < 20; i++)
				{
					sut.Method5(i, 2 * i, 3 * i, 4 * i, 5 * i);
				}

				await That(invocations).IsEqualTo([4, 5, 6, 7, 8,]);
			}

			[Fact]
			public async Task WithoutCallback_IVoidMethodSetupCallbackBuilder_ShouldNotThrow()
			{
				IReturnMethodSetupTest mock = Mock.Create<IReturnMethodSetupTest>();
				IReturnMethodSetupCallbackBuilder<string, int, int, int, int, int> setup =
					(IReturnMethodSetupCallbackBuilder<string, int, int, int, int, int>)mock.SetupMock.Method.Method5(
						It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>());

				void ActWhen()
				{
					setup.When(_ => true);
				}

				void ActFor()
				{
					setup.For(2);
				}

				void ActInParallel()
				{
					setup.InParallel();
				}

				await That(ActWhen).DoesNotThrow();
				await That(ActFor).DoesNotThrow();
				await That(ActInParallel).DoesNotThrow();
			}

			[Fact]
			public async Task WithoutCallback_IVoidMethodSetupCallbackWhenBuilder_ShouldNotThrow()
			{
				IReturnMethodSetupTest mock = Mock.Create<IReturnMethodSetupTest>();
				IReturnMethodSetupCallbackWhenBuilder<string, int, int, int, int, int> setup =
					(IReturnMethodSetupCallbackWhenBuilder<string, int, int, int, int, int>)mock.SetupMock.Method
						.Method5(
							It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>());

				void ActFor()
				{
					setup.For(2);
				}

				void ActOnly()
				{
					setup.Only(2);
				}

				await That(ActFor).DoesNotThrow();
				await That(ActOnly).DoesNotThrow();
			}
		}

		public class VoidMethodWith0Parameters
		{
			[Fact]
			public async Task Callback_ShouldExecuteWhenInvoked()
			{
				int callCount = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method0().Do(() => { callCount++; });

				sut.Method0();

				await That(callCount).IsEqualTo(1);
			}

			[Fact]
			public async Task Callback_ShouldNotExecuteWhenOtherMethodIsInvoked()
			{
				int callCount = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method0().Do(() => { callCount++; });

				sut.Method1(1);
				sut.Method0(false);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task For_InParallel_ShouldLimitMatches()
			{
				List<int> callIndices = [];
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method0()
					.Do(v => { callIndices.Add(v); }).InParallel().When(v => v > 1).For(2);

				sut.Method0();
				sut.Method0();
				sut.Method0();
				sut.Method0();
				sut.Method0();

				await That(callIndices).IsEqualTo([2, 3,]);
			}

			[Fact]
			public async Task InParallel_ShouldInvokeParallelCallbacksAlways()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				int callCount3 = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method0()
					.Do(() => { callCount1++; })
					.Do(() => { callCount2++; }).InParallel()
					.Do(() => { callCount3++; });

				sut.Method0();
				sut.Method0();
				sut.Method0();
				sut.Method0();
				sut.Method0();

				await That(callCount1).IsEqualTo(3);
				await That(callCount2).IsEqualTo(5);
				await That(callCount3).IsEqualTo(2);
			}

			[Theory]
			[InlineData(1, 1)]
			[InlineData(2, 2)]
			[InlineData(3, 3)]
			public async Task Only_ShouldInvokeCallbacksOnlyTheGivenNumberOfTimes(int times, int expectedValue)
			{
				int callCount = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method0()
					.Do(() => { callCount++; }).Only(times);

				sut.Method0();
				sut.Method0();
				sut.Method0();
				sut.Method0();

				await That(callCount).IsEqualTo(expectedValue);
			}

			[Fact]
			public async Task Only_ShouldLimitMatches()
			{
				List<int> callIndices = [];
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method0()
					.Do(v => { callIndices.Add(v); }).When(v => v > 1).Only(2);

				sut.Method0();
				sut.Method0();
				sut.Method0();
				sut.Method0();
				sut.Method0();

				await That(callIndices).IsEqualTo([2, 3,]);
			}

			[Fact]
			public async Task Only_ShouldStopExecutingCallbackAfterTheGivenTimes()
			{
				List<int> invocations = [];
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();
				sut.SetupMock.Method.Method0()
					.Do(i => { invocations.Add(i); })
					.Only(4);

				for (int i = 0; i < 20; i++)
				{
					sut.Method0();
				}

				await That(invocations).IsEqualTo([0, 1, 2, 3,]);
			}

			[Fact]
			public async Task Only_WithWhen_ShouldStopExecutingCallbackAfterTheGivenTimes()
			{
				List<int> invocations = [];
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();
				sut.SetupMock.Method.Method0()
					.Do(i => { invocations.Add(i); })
					.When(x => x > 2)
					.Only(4);

				for (int i = 0; i < 20; i++)
				{
					sut.Method0();
				}

				await That(invocations).IsEqualTo([3, 4, 5, 6,]);
			}

			[Fact]
			public async Task OnlyOnce_ShouldDeactivateCallbackAfterFirstExecution()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method0()
					.Do(() => { callCount1++; })
					.Do(() => { callCount2++; }).OnlyOnce();

				sut.Method0();
				sut.Method0();
				sut.Method0();
				sut.Method0();

				await That(callCount1).IsEqualTo(3);
				await That(callCount2).IsEqualTo(1);
			}

			[Fact]
			public async Task ShouldInvokeCallbacksInSequence()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method0()
					.Do(() => { callCount1++; })
					.Do(() => { callCount2++; });

				sut.Method0();
				sut.Method0();
				sut.Method0();
				sut.Method0();
				sut.Method0();

				await That(callCount1).IsEqualTo(3);
				await That(callCount2).IsEqualTo(2);
			}

			[Fact]
			public async Task When_ShouldOnlyExecuteCallbackWhenInvocationCountMatches()
			{
				List<int> invocations = [];
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();
				sut.SetupMock.Method.Method0()
					.Do(i => { invocations.Add(i); })
					.When(x => x is > 3 and < 9);

				for (int i = 0; i < 20; i++)
				{
					sut.Method0();
				}

				await That(invocations).IsEqualTo([4, 5, 6, 7, 8,]);
			}

			[Fact]
			public async Task WithoutCallback_IVoidMethodSetupCallbackBuilder_ShouldNotThrow()
			{
				IVoidMethodSetupTest mock = Mock.Create<IVoidMethodSetupTest>();
				IVoidMethodSetupCallbackBuilder setup =
					(IVoidMethodSetupCallbackBuilder)mock.SetupMock.Method.Method0();

				void ActWhen()
				{
					setup.When(_ => true);
				}

				void ActFor()
				{
					setup.For(2);
				}

				void ActInParallel()
				{
					setup.InParallel();
				}

				await That(ActWhen).DoesNotThrow();
				await That(ActFor).DoesNotThrow();
				await That(ActInParallel).DoesNotThrow();
			}

			[Fact]
			public async Task WithoutCallback_IVoidMethodSetupCallbackWhenBuilder_ShouldNotThrow()
			{
				IVoidMethodSetupTest mock = Mock.Create<IVoidMethodSetupTest>();
				IVoidMethodSetupCallbackWhenBuilder setup =
					(IVoidMethodSetupCallbackWhenBuilder)mock.SetupMock.Method.Method0();

				void ActFor()
				{
					setup.For(2);
				}

				void ActOnly()
				{
					setup.Only(2);
				}

				await That(ActFor).DoesNotThrow();
				await That(ActOnly).DoesNotThrow();
			}
		}

		public class VoidMethodWith1Parameters
		{
			[Fact]
			public async Task Callback_ShouldExecuteWhenInvoked()
			{
				int callCount = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method1(It.IsAny<int>())
					.Do(() => { callCount++; });

				sut.Method1(3);

				await That(callCount).IsEqualTo(1);
			}

			[Fact]
			public async Task Callback_ShouldNotExecuteWhenOtherMethodIsInvoked()
			{
				int callCount = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method1(It.IsAny<int>())
					.Do(() => { callCount++; });

				sut.Method0();
				sut.Method1(2, false);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task Callback_ShouldNotExecuteWhenParameterDoesNotMatch()
			{
				int callCount = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method1(It.Satisfies<int>(v => v != 1))
					.Do(() => { callCount++; });

				sut.Method1(1);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task CallbackWithValue_ShouldExecuteWhenInvoked()
			{
				int callCount = 0;
				int receivedValue = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method1(It.IsAny<int>())
					.Do(v =>
					{
						callCount++;
						receivedValue = v;
					});

				sut.Method1(3);

				await That(callCount).IsEqualTo(1);
				await That(receivedValue).IsEqualTo(3);
			}

			[Fact]
			public async Task CallbackWithValue_ShouldNotExecuteWhenOtherMethodIsInvoked()
			{
				int callCount = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method1(It.IsAny<int>())
					.Do(_ => { callCount++; });

				sut.Method0();
				sut.Method1(2, false);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task CallbackWithValue_ShouldNotExecuteWhenParameterDoesNotMatch()
			{
				int callCount = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method1(It.Satisfies<int>(v => v != 1))
					.Do(_ => { callCount++; });

				sut.Method1(1);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task InParallel_ShouldInvokeParallelCallbacksAlways()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				int callCount3 = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method1(It.IsAny<int>())
					.Do(() => { callCount1++; })
					.Do(p1 => { callCount2 += p1; }).InParallel()
					.Do(p1 => { callCount3 += p1; });

				sut.Method1(1);
				sut.Method1(2);
				sut.Method1(3);
				sut.Method1(4);

				await That(callCount1).IsEqualTo(2);
				await That(callCount2).IsEqualTo(10);
				await That(callCount3).IsEqualTo(6);
			}

			[Theory]
			[InlineData(1, 1)]
			[InlineData(2, 3)]
			[InlineData(3, 6)]
			public async Task Only_ShouldInvokeCallbacksOnlyTheGivenNumberOfTimes(int times, int expectedValue)
			{
				int sum = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method1(It.IsAny<int>())
					.Do(p1 => { sum += p1; }).Only(times);

				sut.Method1(1);
				sut.Method1(2);
				sut.Method1(3);
				sut.Method1(4);

				await That(sum).IsEqualTo(expectedValue);
			}

			[Fact]
			public async Task Only_ShouldStopExecutingCallbackAfterTheGivenTimes()
			{
				List<int> invocations = [];
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();
				sut.SetupMock.Method.Method1(It.IsAny<int>())
					.Do((i, _) => { invocations.Add(i); })
					.Only(4);

				for (int i = 0; i < 20; i++)
				{
					sut.Method1(i);
				}

				await That(invocations).IsEqualTo([0, 1, 2, 3,]);
			}

			[Fact]
			public async Task Only_WithWhen_ShouldStopExecutingCallbackAfterTheGivenTimes()
			{
				List<int> invocations = [];
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();
				sut.SetupMock.Method.Method1(It.IsAny<int>())
					.Do((i, _) => { invocations.Add(i); })
					.When(x => x > 2)
					.Only(4);

				for (int i = 0; i < 20; i++)
				{
					sut.Method1(i);
				}

				await That(invocations).IsEqualTo([3, 4, 5, 6,]);
			}

			[Fact]
			public async Task OnlyOnce_ShouldDeactivateCallbackAfterFirstExecution()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method1(It.IsAny<int>())
					.Do(() => { callCount1++; })
					.Do(p1 => { callCount2 += p1; }).OnlyOnce();

				sut.Method1(1);
				sut.Method1(2);
				sut.Method1(3);
				sut.Method1(4);

				await That(callCount1).IsEqualTo(3);
				await That(callCount2).IsEqualTo(2);
			}

			[Fact]
			public async Task ParameterCallbacks_ShouldOnlyBeInvokedWhenAllMatch()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method1(
					It.Satisfies<int>(x => x > 0).Monitor(out IParameterMonitor<int> values1));

				sut.Method1(-1);
				sut.Method1(1);
				sut.Method1(0);
				sut.Method1(3);

				await That(values1.Values).IsEqualTo([1, 3,]);
			}

			[Fact]
			public async Task ShouldInvokeCallbacksInSequence()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method1(It.IsAny<int>())
					.Do(() => { callCount1++; })
					.Do(p1 => { callCount2 += p1; });

				sut.Method1(1);
				sut.Method1(2);
				sut.Method1(3);
				sut.Method1(4);

				await That(callCount1).IsEqualTo(2);
				await That(callCount2).IsEqualTo(6);
			}

			[Fact]
			public async Task When_ShouldOnlyExecuteCallbackWhenInvocationCountMatches()
			{
				List<int> invocations = [];
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();
				sut.SetupMock.Method.Method1(It.IsAny<int>())
					.Do((i, _) => { invocations.Add(i); })
					.When(x => x is > 3 and < 9);

				for (int i = 0; i < 20; i++)
				{
					sut.Method1(i);
				}

				await That(invocations).IsEqualTo([4, 5, 6, 7, 8,]);
			}

			[Fact]
			public async Task WithoutCallback_IVoidMethodSetupCallbackBuilder_ShouldNotThrow()
			{
				IVoidMethodSetupTest mock = Mock.Create<IVoidMethodSetupTest>();
				IVoidMethodSetupCallbackBuilder<int> setup =
					(IVoidMethodSetupCallbackBuilder<int>)mock.SetupMock.Method.Method1(
						It.IsAny<int>());

				void ActWhen()
				{
					setup.When(_ => true);
				}

				void ActFor()
				{
					setup.For(2);
				}

				void ActInParallel()
				{
					setup.InParallel();
				}

				await That(ActWhen).DoesNotThrow();
				await That(ActFor).DoesNotThrow();
				await That(ActInParallel).DoesNotThrow();
			}

			[Fact]
			public async Task WithoutCallback_IVoidMethodSetupCallbackWhenBuilder_ShouldNotThrow()
			{
				IVoidMethodSetupTest mock = Mock.Create<IVoidMethodSetupTest>();
				IVoidMethodSetupCallbackWhenBuilder<int> setup =
					(IVoidMethodSetupCallbackWhenBuilder<int>)mock.SetupMock.Method.Method1(
						It.IsAny<int>());

				void ActFor()
				{
					setup.For(2);
				}

				void ActOnly()
				{
					setup.Only(2);
				}

				await That(ActFor).DoesNotThrow();
				await That(ActOnly).DoesNotThrow();
			}
		}

		public class VoidMethodWith2Parameters
		{
			[Fact]
			public async Task Callback_ShouldExecuteWhenInvoked()
			{
				int callCount = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method2(It.IsAny<int>(), It.IsAny<int>())
					.Do(() => { callCount++; });

				sut.Method2(1, 2);

				await That(callCount).IsEqualTo(1);
			}

			[Theory]
			[InlineData(false, false)]
			[InlineData(true, false)]
			[InlineData(false, true)]
			public async Task Callback_ShouldNotExecuteWhenAnyParameterDoesNotMatch(bool isMatch1, bool isMatch2)
			{
				int callCount = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method2(It.Satisfies<int>(_ => isMatch1), It.Satisfies<int>(_ => isMatch2))
					.Do(() => { callCount++; });

				sut.Method2(1, 2);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task Callback_ShouldNotExecuteWhenOtherMethodIsInvoked()
			{
				int callCount = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method2(It.IsAny<int>(), It.IsAny<int>())
					.Do(() => { callCount++; });

				sut.Method1(1);
				sut.Method2(1, 2, false);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task CallbackWithValue_ShouldExecuteWhenInvoked()
			{
				int callCount = 0;
				int receivedValue1 = 0;
				int receivedValue2 = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method2(It.IsAny<int>(), It.IsAny<int>())
					.Do((v1, v2) =>
					{
						callCount++;
						receivedValue1 = v1;
						receivedValue2 = v2;
					});

				sut.Method2(2, 4);

				await That(callCount).IsEqualTo(1);
				await That(receivedValue1).IsEqualTo(2);
				await That(receivedValue2).IsEqualTo(4);
			}

			[Theory]
			[InlineData(false, false)]
			[InlineData(true, false)]
			[InlineData(false, true)]
			public async Task CallbackWithValue_ShouldNotExecuteWhenAnyParameterDoesNotMatch(bool isMatch1,
				bool isMatch2)
			{
				int callCount = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method2(It.Satisfies<int>(_ => isMatch1), It.Satisfies<int>(_ => isMatch2))
					.Do((_, _) => { callCount++; });

				sut.Method2(1, 2);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task CallbackWithValue_ShouldNotExecuteWhenOtherMethodIsInvoked()
			{
				int callCount = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method2(It.IsAny<int>(), It.IsAny<int>())
					.Do((_, _) => { callCount++; });

				sut.Method1(1);
				sut.Method2(1, 2, false);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task InParallel_ShouldInvokeParallelCallbacksAlways()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				int callCount3 = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method2(It.IsAny<int>(), It.IsAny<int>())
					.Do(() => { callCount1++; })
					.Do((p1, _) => { callCount2 += p1; }).InParallel()
					.Do((p1, _) => { callCount3 += p1; });

				sut.Method2(1, 2);
				sut.Method2(2, 2);
				sut.Method2(3, 2);
				sut.Method2(4, 2);

				await That(callCount1).IsEqualTo(2);
				await That(callCount2).IsEqualTo(10);
				await That(callCount3).IsEqualTo(6);
			}

			[Theory]
			[InlineData(1, 1)]
			[InlineData(2, 3)]
			[InlineData(3, 6)]
			public async Task Only_ShouldInvokeCallbacksOnlyTheGivenNumberOfTimes(int times, int expectedValue)
			{
				int sum = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method2(It.IsAny<int>(), It.IsAny<int>())
					.Do((p1, _) => { sum += p1; }).Only(times);

				sut.Method2(1, 2);
				sut.Method2(2, 2);
				sut.Method2(3, 2);
				sut.Method2(4, 2);

				await That(sum).IsEqualTo(expectedValue);
			}

			[Fact]
			public async Task Only_ShouldStopExecutingCallbackAfterTheGivenTimes()
			{
				List<int> invocations = [];
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();
				sut.SetupMock.Method.Method2(It.IsAny<int>(), It.IsAny<int>())
					.Do((i, _, _) => { invocations.Add(i); })
					.Only(4);

				for (int i = 0; i < 20; i++)
				{
					sut.Method2(i, 2 * i);
				}

				await That(invocations).IsEqualTo([0, 1, 2, 3,]);
			}

			[Fact]
			public async Task Only_WithWhen_ShouldStopExecutingCallbackAfterTheGivenTimes()
			{
				List<int> invocations = [];
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();
				sut.SetupMock.Method.Method2(It.IsAny<int>(), It.IsAny<int>())
					.Do((i, _, _) => { invocations.Add(i); })
					.When(x => x > 2)
					.Only(4);

				for (int i = 0; i < 20; i++)
				{
					sut.Method2(i, 2 * i);
				}

				await That(invocations).IsEqualTo([3, 4, 5, 6,]);
			}

			[Fact]
			public async Task OnlyOnce_ShouldDeactivateCallbackAfterFirstExecution()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method2(It.IsAny<int>(), It.IsAny<int>())
					.Do(() => { callCount1++; })
					.Do((p1, _) => { callCount2 += p1; }).OnlyOnce();

				sut.Method2(1, 2);
				sut.Method2(2, 2);
				sut.Method2(3, 2);
				sut.Method2(4, 2);

				await That(callCount1).IsEqualTo(3);
				await That(callCount2).IsEqualTo(2);
			}

			[Fact]
			public async Task ParameterCallbacks_ShouldOnlyBeInvokedWhenAllMatch()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method2(
					It.Satisfies<int>(x => x > 0).Monitor(out IParameterMonitor<int> values1),
					It.Satisfies<int>(x => x > 0).Monitor(out IParameterMonitor<int> values2));

				sut.Method2(-1, 4);
				sut.Method2(1, 4);
				sut.Method2(2, 3);
				sut.Method2(1, -1);

				await That(values1.Values).IsEqualTo([1, 2,]);
				await That(values2.Values).IsEqualTo([4, 3,]);
			}

			[Fact]
			public async Task ShouldInvokeCallbacksInSequence()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method2(It.IsAny<int>(), It.IsAny<int>())
					.Do(() => { callCount1++; })
					.Do((p1, _) => { callCount2 += p1; });

				sut.Method2(1, 2);
				sut.Method2(2, 2);
				sut.Method2(3, 2);
				sut.Method2(4, 2);

				await That(callCount1).IsEqualTo(2);
				await That(callCount2).IsEqualTo(6);
			}

			[Fact]
			public async Task When_ShouldOnlyExecuteCallbackWhenInvocationCountMatches()
			{
				List<int> invocations = [];
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();
				sut.SetupMock.Method.Method2(It.IsAny<int>(), It.IsAny<int>())
					.Do((i, _, _) => { invocations.Add(i); })
					.When(x => x is > 3 and < 9);

				for (int i = 0; i < 20; i++)
				{
					sut.Method2(i, 2 * i);
				}

				await That(invocations).IsEqualTo([4, 5, 6, 7, 8,]);
			}

			[Fact]
			public async Task WithoutCallback_IVoidMethodSetupCallbackBuilder_ShouldNotThrow()
			{
				IVoidMethodSetupTest mock = Mock.Create<IVoidMethodSetupTest>();
				IVoidMethodSetupCallbackBuilder<int, int> setup =
					(IVoidMethodSetupCallbackBuilder<int, int>)mock.SetupMock.Method.Method2(
						It.IsAny<int>(), It.IsAny<int>());

				void ActWhen()
				{
					setup.When(_ => true);
				}

				void ActFor()
				{
					setup.For(2);
				}

				void ActInParallel()
				{
					setup.InParallel();
				}

				await That(ActWhen).DoesNotThrow();
				await That(ActFor).DoesNotThrow();
				await That(ActInParallel).DoesNotThrow();
			}

			[Fact]
			public async Task WithoutCallback_IVoidMethodSetupCallbackWhenBuilder_ShouldNotThrow()
			{
				IVoidMethodSetupTest mock = Mock.Create<IVoidMethodSetupTest>();
				IVoidMethodSetupCallbackWhenBuilder<int, int> setup =
					(IVoidMethodSetupCallbackWhenBuilder<int, int>)mock.SetupMock.Method.Method2(
						It.IsAny<int>(), It.IsAny<int>());

				void ActFor()
				{
					setup.For(2);
				}

				void ActOnly()
				{
					setup.Only(2);
				}

				await That(ActFor).DoesNotThrow();
				await That(ActOnly).DoesNotThrow();
			}
		}

		public class VoidMethodWith3Parameters
		{
			[Fact]
			public async Task Callback_ShouldExecuteWhenInvoked()
			{
				int callCount = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Do(() => { callCount++; });

				sut.Method3(1, 2, 3);

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
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method3(It.Satisfies<int>(_ => isMatch1), It.Satisfies<int>(_ => isMatch2),
						It.Satisfies<int>(_ => isMatch3))
					.Do(() => { callCount++; });

				sut.Method3(1, 2, 3);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task Callback_ShouldNotExecuteWhenOtherMethodIsInvoked()
			{
				int callCount = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Do(() => { callCount++; });

				sut.Method2(1, 2);
				sut.Method3(1, 2, 3, false);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task CallbackWithValue_ShouldExecuteWhenInvoked()
			{
				int callCount = 0;
				int receivedValue1 = 0;
				int receivedValue2 = 0;
				int receivedValue3 = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Do((v1, v2, v3) =>
					{
						callCount++;
						receivedValue1 = v1;
						receivedValue2 = v2;
						receivedValue3 = v3;
					});

				sut.Method3(2, 4, 6);

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
			public async Task CallbackWithValue_ShouldNotExecuteWhenAnyParameterDoesNotMatch(bool isMatch1,
				bool isMatch2,
				bool isMatch3)
			{
				int callCount = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method3(It.Satisfies<int>(_ => isMatch1), It.Satisfies<int>(_ => isMatch2),
						It.Satisfies<int>(_ => isMatch3))
					.Do((_, _, _) => { callCount++; });

				sut.Method3(1, 2, 3);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task CallbackWithValue_ShouldNotExecuteWhenOtherMethodIsInvoked()
			{
				int callCount = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Do((_, _, _) => { callCount++; });

				sut.Method2(1, 2);
				sut.Method3(1, 2, 3, false);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task InParallel_ShouldInvokeParallelCallbacksAlways()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				int callCount3 = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Do(() => { callCount1++; })
					.Do((p1, _, _) => { callCount2 += p1; }).InParallel()
					.Do((p1, _, _) => { callCount3 += p1; });

				sut.Method3(1, 2, 3);
				sut.Method3(2, 2, 3);
				sut.Method3(3, 2, 3);
				sut.Method3(4, 2, 3);

				await That(callCount1).IsEqualTo(2);
				await That(callCount2).IsEqualTo(10);
				await That(callCount3).IsEqualTo(6);
			}

			[Theory]
			[InlineData(1, 1)]
			[InlineData(2, 3)]
			[InlineData(3, 6)]
			public async Task Only_ShouldInvokeCallbacksOnlyTheGivenNumberOfTimes(int times, int expectedValue)
			{
				int sum = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Do((p1, _, _) => { sum += p1; }).Only(times);

				sut.Method3(1, 2, 3);
				sut.Method3(2, 2, 3);
				sut.Method3(3, 2, 3);
				sut.Method3(4, 2, 3);

				await That(sum).IsEqualTo(expectedValue);
			}

			[Fact]
			public async Task Only_ShouldStopExecutingCallbackAfterTheGivenTimes()
			{
				List<int> invocations = [];
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();
				sut.SetupMock.Method.Method3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Do((i, _, _, _) => { invocations.Add(i); })
					.Only(4);

				for (int i = 0; i < 20; i++)
				{
					sut.Method3(i, 2 * i, 3 * i);
				}

				await That(invocations).IsEqualTo([0, 1, 2, 3,]);
			}

			[Fact]
			public async Task Only_WithWhen_ShouldStopExecutingCallbackAfterTheGivenTimes()
			{
				List<int> invocations = [];
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();
				sut.SetupMock.Method.Method3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Do((i, _, _, _) => { invocations.Add(i); })
					.When(x => x > 2)
					.Only(4);

				for (int i = 0; i < 20; i++)
				{
					sut.Method3(i, 2 * i, 3 * i);
				}

				await That(invocations).IsEqualTo([3, 4, 5, 6,]);
			}

			[Fact]
			public async Task OnlyOnce_ShouldDeactivateCallbackAfterFirstExecution()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Do(() => { callCount1++; })
					.Do((p1, _, _) => { callCount2 += p1; }).OnlyOnce();

				sut.Method3(1, 2, 3);
				sut.Method3(2, 2, 3);
				sut.Method3(3, 2, 3);
				sut.Method3(4, 2, 3);

				await That(callCount1).IsEqualTo(3);
				await That(callCount2).IsEqualTo(2);
			}

			[Fact]
			public async Task ParameterCallbacks_ShouldOnlyBeInvokedWhenAllMatch()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method3(
					It.Satisfies<int>(x => x > 0).Monitor(out IParameterMonitor<int> values1),
					It.Satisfies<int>(x => x > 0).Monitor(out IParameterMonitor<int> values2),
					It.Satisfies<int>(x => x > 0).Monitor(out IParameterMonitor<int> values3));

				sut.Method3(-1, 2, 3);
				sut.Method3(1, 2, 3);
				sut.Method3(1, -2, 3);
				sut.Method3(2, 3, 4);
				sut.Method3(1, 2, -3);

				await That(values1.Values).IsEqualTo([1, 2,]);
				await That(values2.Values).IsEqualTo([2, 3,]);
				await That(values3.Values).IsEqualTo([3, 4,]);
			}

			[Fact]
			public async Task ShouldInvokeCallbacksInSequence()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Do(() => { callCount1++; })
					.Do((p1, _, _) => { callCount2 += p1; });

				sut.Method3(1, 2, 3);
				sut.Method3(2, 2, 3);
				sut.Method3(3, 2, 3);
				sut.Method3(4, 2, 3);

				await That(callCount1).IsEqualTo(2);
				await That(callCount2).IsEqualTo(6);
			}

			[Fact]
			public async Task When_ShouldOnlyExecuteCallbackWhenInvocationCountMatches()
			{
				List<int> invocations = [];
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();
				sut.SetupMock.Method.Method3(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Do((i, _, _, _) => { invocations.Add(i); })
					.When(x => x is > 3 and < 9);

				for (int i = 0; i < 20; i++)
				{
					sut.Method3(i, 2 * i, 3 * i);
				}

				await That(invocations).IsEqualTo([4, 5, 6, 7, 8,]);
			}

			[Fact]
			public async Task WithoutCallback_IVoidMethodSetupCallbackBuilder_ShouldNotThrow()
			{
				IVoidMethodSetupTest mock = Mock.Create<IVoidMethodSetupTest>();
				IVoidMethodSetupCallbackBuilder<int, int, int> setup =
					(IVoidMethodSetupCallbackBuilder<int, int, int>)mock.SetupMock.Method.Method3(
						It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>());

				void ActWhen()
				{
					setup.When(_ => true);
				}

				void ActFor()
				{
					setup.For(2);
				}

				void ActInParallel()
				{
					setup.InParallel();
				}

				await That(ActWhen).DoesNotThrow();
				await That(ActFor).DoesNotThrow();
				await That(ActInParallel).DoesNotThrow();
			}

			[Fact]
			public async Task WithoutCallback_IVoidMethodSetupCallbackWhenBuilder_ShouldNotThrow()
			{
				IVoidMethodSetupTest mock = Mock.Create<IVoidMethodSetupTest>();
				IVoidMethodSetupCallbackWhenBuilder<int, int, int> setup =
					(IVoidMethodSetupCallbackWhenBuilder<int, int, int>)mock.SetupMock.Method.Method3(
						It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>());

				void ActFor()
				{
					setup.For(2);
				}

				void ActOnly()
				{
					setup.Only(2);
				}

				await That(ActFor).DoesNotThrow();
				await That(ActOnly).DoesNotThrow();
			}
		}

		public class VoidMethodWith4Parameters
		{
			[Fact]
			public async Task Callback_ShouldExecuteWhenInvoked()
			{
				int callCount = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Do(() => { callCount++; });

				sut.Method4(1, 2, 3, 4);

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
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method4(It.Satisfies<int>(_ => isMatch1), It.Satisfies<int>(_ => isMatch2),
						It.Satisfies<int>(_ => isMatch3), It.Satisfies<int>(_ => isMatch4))
					.Do(() => { callCount++; });

				sut.Method4(1, 2, 3, 4);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task Callback_ShouldNotExecuteWhenOtherMethodIsInvoked()
			{
				int callCount = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Do(() => { callCount++; });

				sut.Method3(1, 2, 3);
				sut.Method4(1, 2, 3, false);

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
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Do((v1, v2, v3, v4) =>
					{
						callCount++;
						receivedValue1 = v1;
						receivedValue2 = v2;
						receivedValue3 = v3;
						receivedValue4 = v4;
					});

				sut.Method4(2, 4, 6, 8);

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
			public async Task CallbackWithValue_ShouldNotExecuteWhenAnyParameterDoesNotMatch(bool isMatch1,
				bool isMatch2,
				bool isMatch3, bool isMatch4)
			{
				int callCount = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method4(It.Satisfies<int>(_ => isMatch1), It.Satisfies<int>(_ => isMatch2),
						It.Satisfies<int>(_ => isMatch3), It.Satisfies<int>(_ => isMatch4))
					.Do((_, _, _, _) => { callCount++; });

				sut.Method4(1, 2, 3, 4);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task CallbackWithValue_ShouldNotExecuteWhenOtherMethodIsInvoked()
			{
				int callCount = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Do((_, _, _, _) => { callCount++; });

				sut.Method3(1, 2, 3);
				sut.Method4(1, 2, 3, false);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task InParallel_ShouldInvokeParallelCallbacksAlways()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				int callCount3 = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Do(() => { callCount1++; })
					.Do((p1, _, _, _) => { callCount2 += p1; }).InParallel()
					.Do((p1, _, _, _) => { callCount3 += p1; });

				sut.Method4(1, 2, 3, 4);
				sut.Method4(2, 2, 3, 4);
				sut.Method4(3, 2, 3, 4);
				sut.Method4(4, 2, 3, 4);

				await That(callCount1).IsEqualTo(2);
				await That(callCount2).IsEqualTo(10);
				await That(callCount3).IsEqualTo(6);
			}

			[Theory]
			[InlineData(1, 1)]
			[InlineData(2, 3)]
			[InlineData(3, 6)]
			public async Task Only_ShouldInvokeCallbacksOnlyTheGivenNumberOfTimes(int times, int expectedValue)
			{
				int sum = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Do((p1, _, _, _) => { sum += p1; }).Only(times);

				sut.Method4(1, 2, 3, 4);
				sut.Method4(2, 2, 3, 4);
				sut.Method4(3, 2, 3, 4);
				sut.Method4(4, 2, 3, 4);

				await That(sum).IsEqualTo(expectedValue);
			}

			[Fact]
			public async Task Only_ShouldStopExecutingCallbackAfterTheGivenTimes()
			{
				List<int> invocations = [];
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();
				sut.SetupMock.Method.Method4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Do((i, _, _, _, _) => { invocations.Add(i); })
					.Only(4);

				for (int i = 0; i < 20; i++)
				{
					sut.Method4(i, 2 * i, 3 * i, 4 * i);
				}

				await That(invocations).IsEqualTo([0, 1, 2, 3,]);
			}

			[Fact]
			public async Task Only_WithWhen_ShouldStopExecutingCallbackAfterTheGivenTimes()
			{
				List<int> invocations = [];
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();
				sut.SetupMock.Method.Method4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Do((i, _, _, _, _) => { invocations.Add(i); })
					.When(x => x > 2)
					.Only(4);

				for (int i = 0; i < 20; i++)
				{
					sut.Method4(i, 2 * i, 3 * i, 4 * i);
				}

				await That(invocations).IsEqualTo([3, 4, 5, 6,]);
			}

			[Fact]
			public async Task OnlyOnce_ShouldDeactivateCallbackAfterFirstExecution()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Do(() => { callCount1++; })
					.Do((p1, _, _, _) => { callCount2 += p1; }).OnlyOnce();

				sut.Method4(1, 2, 3, 4);
				sut.Method4(2, 2, 3, 4);
				sut.Method4(3, 2, 3, 4);
				sut.Method4(4, 2, 3, 4);

				await That(callCount1).IsEqualTo(3);
				await That(callCount2).IsEqualTo(2);
			}

			[Fact]
			public async Task ParameterCallbacks_ShouldOnlyBeInvokedWhenAllMatch()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method4(
					It.Satisfies<int>(x => x > 0).Monitor(out IParameterMonitor<int> values1),
					It.Satisfies<int>(x => x > 0).Monitor(out IParameterMonitor<int> values2),
					It.Satisfies<int>(x => x > 0).Monitor(out IParameterMonitor<int> values3),
					It.Satisfies<int>(x => x > 0).Monitor(out IParameterMonitor<int> values4));

				sut.Method4(-1, 2, 3, 4);
				sut.Method4(1, 2, 3, 4);
				sut.Method4(1, -2, 3, 4);
				sut.Method4(2, 3, 4, 5);
				sut.Method4(1, 2, -3, 4);
				sut.Method4(3, 4, 5, 6);
				sut.Method4(1, 2, 3, -4);

				await That(values1.Values).IsEqualTo([1, 2, 3,]);
				await That(values2.Values).IsEqualTo([2, 3, 4,]);
				await That(values3.Values).IsEqualTo([3, 4, 5,]);
				await That(values4.Values).IsEqualTo([4, 5, 6,]);
			}

			[Fact]
			public async Task ShouldInvokeCallbacksInSequence()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Do(() => { callCount1++; })
					.Do((p1, _, _, _) => { callCount2 += p1; });

				sut.Method4(1, 2, 3, 4);
				sut.Method4(2, 2, 3, 4);
				sut.Method4(3, 2, 3, 4);
				sut.Method4(4, 2, 3, 4);

				await That(callCount1).IsEqualTo(2);
				await That(callCount2).IsEqualTo(6);
			}

			[Fact]
			public async Task When_ShouldOnlyExecuteCallbackWhenInvocationCountMatches()
			{
				List<int> invocations = [];
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();
				sut.SetupMock.Method.Method4(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.Do((i, _, _, _, _) => { invocations.Add(i); })
					.When(x => x is > 3 and < 9);

				for (int i = 0; i < 20; i++)
				{
					sut.Method4(i, 2 * i, 3 * i, 4 * i);
				}

				await That(invocations).IsEqualTo([4, 5, 6, 7, 8,]);
			}

			[Fact]
			public async Task WithoutCallback_IVoidMethodSetupCallbackBuilder_ShouldNotThrow()
			{
				IVoidMethodSetupTest mock = Mock.Create<IVoidMethodSetupTest>();
				IVoidMethodSetupCallbackBuilder<int, int, int, int> setup =
					(IVoidMethodSetupCallbackBuilder<int, int, int, int>)mock.SetupMock.Method.Method4(
						It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>());

				void ActWhen()
				{
					setup.When(_ => true);
				}

				void ActFor()
				{
					setup.For(2);
				}

				void ActInParallel()
				{
					setup.InParallel();
				}

				await That(ActWhen).DoesNotThrow();
				await That(ActFor).DoesNotThrow();
				await That(ActInParallel).DoesNotThrow();
			}

			[Fact]
			public async Task WithoutCallback_IVoidMethodSetupCallbackWhenBuilder_ShouldNotThrow()
			{
				IVoidMethodSetupTest mock = Mock.Create<IVoidMethodSetupTest>();
				IVoidMethodSetupCallbackWhenBuilder<int, int, int, int> setup =
					(IVoidMethodSetupCallbackWhenBuilder<int, int, int, int>)mock.SetupMock.Method.Method4(
						It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>());

				void ActFor()
				{
					setup.For(2);
				}

				void ActOnly()
				{
					setup.Only(2);
				}

				await That(ActFor).DoesNotThrow();
				await That(ActOnly).DoesNotThrow();
			}
		}

		public class VoidMethodWith5Parameters
		{
			[Fact]
			public async Task Callback_ShouldExecuteWhenInvoked()
			{
				int callCount = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method5(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.Do(() => { callCount++; });

				sut.Method5(1, 2, 3, 4, 5);

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
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method5(It.Satisfies<int>(_ => isMatch1), It.Satisfies<int>(_ => isMatch2),
						It.Satisfies<int>(_ => isMatch3), It.Satisfies<int>(_ => isMatch4),
						It.Satisfies<int>(_ => isMatch5))
					.Do(() => { callCount++; });

				sut.Method5(1, 2, 3, 4, 5);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task Callback_ShouldNotExecuteWhenOtherMethodIsInvoked()
			{
				int callCount = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method5(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.Do(() => { callCount++; });

				sut.Method4(1, 2, 3, 4);
				sut.Method5(1, 2, 3, 4, 5, false);

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
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method5(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.Do((v1, v2, v3, v4, v5) =>
					{
						callCount++;
						receivedValue1 = v1;
						receivedValue2 = v2;
						receivedValue3 = v3;
						receivedValue4 = v4;
						receivedValue5 = v5;
					});

				sut.Method5(2, 4, 6, 8, 10);

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
			public async Task CallbackWithValue_ShouldNotExecuteWhenAnyParameterDoesNotMatch(bool isMatch1,
				bool isMatch2,
				bool isMatch3, bool isMatch4, bool isMatch5)
			{
				int callCount = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method5(It.Satisfies<int>(_ => isMatch1), It.Satisfies<int>(_ => isMatch2),
						It.Satisfies<int>(_ => isMatch3), It.Satisfies<int>(_ => isMatch4),
						It.Satisfies<int>(_ => isMatch5))
					.Do((_, _, _, _, _) => { callCount++; });

				sut.Method5(1, 2, 3, 4, 5);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task CallbackWithValue_ShouldNotExecuteWhenOtherMethodIsInvoked()
			{
				int callCount = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method5(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.Do((_, _, _, _, _) => { callCount++; });

				sut.Method4(1, 2, 3, 4);
				sut.Method5(1, 2, 3, 4, 5, false);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task InParallel_ShouldInvokeParallelCallbacksAlways()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				int callCount3 = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method5(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.Do(() => { callCount1++; })
					.Do((p1, _, _, _, _) => { callCount2 += p1; }).InParallel()
					.Do((p1, _, _, _, _) => { callCount3 += p1; });

				sut.Method5(1, 2, 3, 4, 5);
				sut.Method5(2, 2, 3, 4, 5);
				sut.Method5(3, 2, 3, 4, 5);
				sut.Method5(4, 2, 3, 4, 5);

				await That(callCount1).IsEqualTo(2);
				await That(callCount2).IsEqualTo(10);
				await That(callCount3).IsEqualTo(6);
			}

			[Theory]
			[InlineData(1, 1)]
			[InlineData(2, 3)]
			[InlineData(3, 6)]
			public async Task Only_ShouldInvokeCallbacksOnlyTheGivenNumberOfTimes(int times, int expectedValue)
			{
				int sum = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method5(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.Do((p1, _, _, _, _) => { sum += p1; }).Only(times);

				sut.Method5(1, 2, 3, 4, 5);
				sut.Method5(2, 2, 3, 4, 5);
				sut.Method5(3, 2, 3, 4, 5);
				sut.Method5(4, 2, 3, 4, 5);

				await That(sum).IsEqualTo(expectedValue);
			}

			[Fact]
			public async Task Only_ShouldStopExecutingCallbackAfterTheGivenTimes()
			{
				List<int> invocations = [];
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();
				sut.SetupMock.Method.Method5(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.Do((i, _, _, _, _, _) => { invocations.Add(i); })
					.Only(4);

				for (int i = 0; i < 20; i++)
				{
					sut.Method5(i, 2 * i, 3 * i, 4 * i, 5 * i);
				}

				await That(invocations).IsEqualTo([0, 1, 2, 3,]);
			}

			[Fact]
			public async Task Only_WithWhen_ShouldStopExecutingCallbackAfterTheGivenTimes()
			{
				List<int> invocations = [];
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();
				sut.SetupMock.Method.Method5(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.Do((i, _, _, _, _, _) => { invocations.Add(i); })
					.When(x => x > 2)
					.Only(4);

				for (int i = 0; i < 20; i++)
				{
					sut.Method5(i, 2 * i, 3 * i, 4 * i, 5 * i);
				}

				await That(invocations).IsEqualTo([3, 4, 5, 6,]);
			}

			[Fact]
			public async Task OnlyOnce_ShouldDeactivateCallbackAfterFirstExecution()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method5(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.Do(() => { callCount1++; })
					.Do((p1, _, _, _, _) => { callCount2 += p1; }).OnlyOnce();

				sut.Method5(1, 2, 3, 4, 5);
				sut.Method5(2, 2, 3, 4, 5);
				sut.Method5(3, 2, 3, 4, 5);
				sut.Method5(4, 2, 3, 4, 5);

				await That(callCount1).IsEqualTo(3);
				await That(callCount2).IsEqualTo(2);
			}

			[Fact]
			public async Task ParameterCallbacks_ShouldOnlyBeInvokedWhenAllMatch()
			{
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method5(
					It.Satisfies<int>(x => x > 0).Monitor(out IParameterMonitor<int> values1),
					It.Satisfies<int>(x => x > 0).Monitor(out IParameterMonitor<int> values2),
					It.Satisfies<int>(x => x > 0).Monitor(out IParameterMonitor<int> values3),
					It.Satisfies<int>(x => x > 0).Monitor(out IParameterMonitor<int> values4),
					It.Satisfies<int>(x => x > 0).Monitor(out IParameterMonitor<int> values5));

				sut.Method5(-1, 2, 3, 4, 5);
				sut.Method5(1, 2, 3, 4, 5);
				sut.Method5(1, -2, 3, 4, 5);
				sut.Method5(2, 3, 4, 5, 6);
				sut.Method5(1, 2, -3, 4, 5);
				sut.Method5(3, 4, 5, 6, 7);
				sut.Method5(1, 2, 3, -4, 5);
				sut.Method5(4, 5, 6, 7, 8);
				sut.Method5(1, 2, 3, -4, 5);

				await That(values1.Values).IsEqualTo([1, 2, 3, 4,]);
				await That(values2.Values).IsEqualTo([2, 3, 4, 5,]);
				await That(values3.Values).IsEqualTo([3, 4, 5, 6,]);
				await That(values4.Values).IsEqualTo([4, 5, 6, 7,]);
				await That(values5.Values).IsEqualTo([5, 6, 7, 8,]);
			}

			[Fact]
			public async Task ShouldInvokeCallbacksInSequence()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();

				sut.SetupMock.Method.Method5(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.Do(() => { callCount1++; })
					.Do((p1, _, _, _, _) => { callCount2 += p1; });

				sut.Method5(1, 2, 3, 4, 5);
				sut.Method5(2, 2, 3, 4, 5);
				sut.Method5(3, 2, 3, 4, 5);
				sut.Method5(4, 2, 3, 4, 5);

				await That(callCount1).IsEqualTo(2);
				await That(callCount2).IsEqualTo(6);
			}

			[Fact]
			public async Task When_ShouldOnlyExecuteCallbackWhenInvocationCountMatches()
			{
				List<int> invocations = [];
				IVoidMethodSetupTest sut = Mock.Create<IVoidMethodSetupTest>();
				sut.SetupMock.Method.Method5(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.Do((i, _, _, _, _, _) => { invocations.Add(i); })
					.When(x => x is > 3 and < 9);

				for (int i = 0; i < 20; i++)
				{
					sut.Method5(i, 2 * i, 3 * i, 4 * i, 5 * i);
				}

				await That(invocations).IsEqualTo([4, 5, 6, 7, 8,]);
			}

			[Fact]
			public async Task WithoutCallback_IVoidMethodSetupCallbackBuilder_ShouldNotThrow()
			{
				IVoidMethodSetupTest mock = Mock.Create<IVoidMethodSetupTest>();
				IVoidMethodSetupCallbackBuilder<int, int, int, int, int> setup =
					(IVoidMethodSetupCallbackBuilder<int, int, int, int, int>)mock.SetupMock.Method.Method5(
						It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>());

				void ActWhen()
				{
					setup.When(_ => true);
				}

				void ActFor()
				{
					setup.For(2);
				}

				void ActInParallel()
				{
					setup.InParallel();
				}

				await That(ActWhen).DoesNotThrow();
				await That(ActFor).DoesNotThrow();
				await That(ActInParallel).DoesNotThrow();
			}

			[Fact]
			public async Task WithoutCallback_IVoidMethodSetupCallbackWhenBuilder_ShouldNotThrow()
			{
				IVoidMethodSetupTest mock = Mock.Create<IVoidMethodSetupTest>();
				IVoidMethodSetupCallbackWhenBuilder<int, int, int, int, int> setup =
					(IVoidMethodSetupCallbackWhenBuilder<int, int, int, int, int>)mock.SetupMock.Method.Method5(
						It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>());

				void ActFor()
				{
					setup.For(2);
				}

				void ActOnly()
				{
					setup.Only(2);
				}

				await That(ActFor).DoesNotThrow();
				await That(ActOnly).DoesNotThrow();
			}
		}
	}
}
