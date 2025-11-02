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
			public async Task CallbackWithValue_ShouldNotExecuteWhenAnyParameterDoesNotMatch(bool isMatch1,
				bool isMatch2)
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
			public async Task CallbackWithValue_ShouldNotExecuteWhenAnyParameterDoesNotMatch(bool isMatch1,
				bool isMatch2,
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
			public async Task CallbackWithValue_ShouldNotExecuteWhenAnyParameterDoesNotMatch(bool isMatch1,
				bool isMatch2,
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
			public async Task CallbackWithValue_ShouldNotExecuteWhenAnyParameterDoesNotMatch(bool isMatch1,
				bool isMatch2,
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
		}

		public class ReturnMethodWithParameters
		{
			[Fact]
			public async Task Callback_ShouldExecuteWhenInvoked()
			{
				int callCount = 0;
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.UniqueMethodWithParameters(With.AnyParameterCombination())
					.Callback(() => { callCount++; })
					.Returns("a");

				sut.Subject.UniqueMethodWithParameters(1, 2);

				await That(callCount).IsEqualTo(1);
			}

			[Fact]
			public async Task Callback_ShouldNotExecuteWhenOtherMethodIsInvoked()
			{
				int callCount = 0;
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.UniqueMethodWithParameters(With.AnyParameterCombination())
					.Callback(() => { callCount++; });

				sut.Subject.Method1(1);
				sut.Subject.Method2(1, 2, false);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task MultipleCallbacks_ShouldAllGetInvoked()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				Mock<IReturnMethodSetupTest> sut = Mock.Create<IReturnMethodSetupTest>();

				sut.Setup.Method.UniqueMethodWithParameters(With.AnyParameterCombination())
					.Callback(() => { callCount1++; })
					.Callback(() => { callCount2 += 3; })
					.Returns("a");

				sut.Subject.UniqueMethodWithParameters(1, 2);
				sut.Subject.UniqueMethodWithParameters(2, 2);

				await That(callCount1).IsEqualTo(2);
				await That(callCount2).IsEqualTo(6);
			}
		}

		public class VoidMethodWith0Parameters
		{
			[Fact]
			public async Task Callback_ShouldExecuteWhenInvoked()
			{
				int callCount = 0;
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

				sut.Setup.Method.Method0().Callback(() => { callCount++; });

				sut.Subject.Method0();

				await That(callCount).IsEqualTo(1);
			}

			[Fact]
			public async Task Callback_ShouldNotExecuteWhenOtherMethodIsInvoked()
			{
				int callCount = 0;
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

				sut.Setup.Method.Method0().Callback(() => { callCount++; });

				sut.Subject.Method1(1);
				sut.Subject.Method0(false);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task MultipleCallbacks_ShouldAllGetInvoked()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

				sut.Setup.Method.Method0()
					.Callback(() => { callCount1++; })
					.Callback(() => { callCount2++; });

				sut.Subject.Method0();
				sut.Subject.Method0();

				await That(callCount1).IsEqualTo(2);
				await That(callCount2).IsEqualTo(2);
			}
		}

		public class VoidMethodWith1Parameters
		{
			[Fact]
			public async Task Callback_ShouldExecuteWhenInvoked()
			{
				int callCount = 0;
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

				sut.Setup.Method.Method1(With.Any<int>())
					.Callback(() => { callCount++; });

				sut.Subject.Method1(3);

				await That(callCount).IsEqualTo(1);
			}

			[Fact]
			public async Task Callback_ShouldNotExecuteWhenOtherMethodIsInvoked()
			{
				int callCount = 0;
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

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
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

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
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

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
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

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
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

				sut.Setup.Method.Method1(With.Matching<int>(v => v != 1))
					.Callback(v => { callCount++; });

				sut.Subject.Method1(1);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task MultipleCallbacks_ShouldAllGetInvoked()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

				sut.Setup.Method.Method1(With.Any<int>())
					.Callback(() => { callCount1++; })
					.Callback(v => { callCount2 += v; });

				sut.Subject.Method1(1);
				sut.Subject.Method1(2);

				await That(callCount1).IsEqualTo(2);
				await That(callCount2).IsEqualTo(3);
			}
		}

		public class VoidMethodWith2Parameters
		{
			[Fact]
			public async Task Callback_ShouldExecuteWhenInvoked()
			{
				int callCount = 0;
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

				sut.Setup.Method.Method2(With.Any<int>(), With.Any<int>())
					.Callback(() => { callCount++; });

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
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

				sut.Setup.Method.Method2(With.Matching<int>(v => isMatch1), With.Matching<int>(v => isMatch2))
					.Callback(() => { callCount++; });

				sut.Subject.Method2(1, 2);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task Callback_ShouldNotExecuteWhenOtherMethodIsInvoked()
			{
				int callCount = 0;
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

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
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

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
			public async Task CallbackWithValue_ShouldNotExecuteWhenAnyParameterDoesNotMatch(bool isMatch1,
				bool isMatch2)
			{
				int callCount = 0;
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

				sut.Setup.Method.Method2(With.Matching<int>(v => isMatch1), With.Matching<int>(v => isMatch2))
					.Callback((v1, v2) => { callCount++; });

				sut.Subject.Method2(1, 2);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task CallbackWithValue_ShouldNotExecuteWhenOtherMethodIsInvoked()
			{
				int callCount = 0;
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

				sut.Setup.Method.Method2(With.Any<int>(), With.Any<int>())
					.Callback((v1, v2) => { callCount++; });

				sut.Subject.Method1(1);
				sut.Subject.Method2(1, 2, false);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task MultipleCallbacks_ShouldAllGetInvoked()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

				sut.Setup.Method.Method2(With.Any<int>(), With.Any<int>())
					.Callback(() => { callCount1++; })
					.Callback((v1, v2) => { callCount2 += v1 * v2; });

				sut.Subject.Method2(1, 2);
				sut.Subject.Method2(2, 2);

				await That(callCount1).IsEqualTo(2);
				await That(callCount2).IsEqualTo(6);
			}
		}

		public class VoidMethodWith3Parameters
		{
			[Fact]
			public async Task Callback_ShouldExecuteWhenInvoked()
			{
				int callCount = 0;
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

				sut.Setup.Method.Method3(With.Any<int>(), With.Any<int>(), With.Any<int>())
					.Callback(() => { callCount++; });

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
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

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
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

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
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

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
			public async Task CallbackWithValue_ShouldNotExecuteWhenAnyParameterDoesNotMatch(bool isMatch1,
				bool isMatch2,
				bool isMatch3)
			{
				int callCount = 0;
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

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
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

				sut.Setup.Method.Method3(With.Any<int>(), With.Any<int>(), With.Any<int>())
					.Callback((v1, v2, v3) => { callCount++; });

				sut.Subject.Method2(1, 2);
				sut.Subject.Method3(1, 2, 3, false);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task MultipleCallbacks_ShouldAllGetInvoked()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

				sut.Setup.Method.Method3(With.Any<int>(), With.Any<int>(), With.Any<int>())
					.Callback(() => { callCount1++; })
					.Callback((v1, v2, v3) => { callCount2 += v1 * v2 * v3; });

				sut.Subject.Method3(1, 2, 3);
				sut.Subject.Method3(2, 2, 3);

				await That(callCount1).IsEqualTo(2);
				await That(callCount2).IsEqualTo(18);
			}
		}

		public class VoidMethodWith4Parameters
		{
			[Fact]
			public async Task Callback_ShouldExecuteWhenInvoked()
			{
				int callCount = 0;
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

				sut.Setup.Method.Method4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
					.Callback(() => { callCount++; });

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
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

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
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

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
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

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
			public async Task CallbackWithValue_ShouldNotExecuteWhenAnyParameterDoesNotMatch(bool isMatch1,
				bool isMatch2,
				bool isMatch3, bool isMatch4)
			{
				int callCount = 0;
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

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
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

				sut.Setup.Method.Method4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
					.Callback((v1, v2, v3, v4) => { callCount++; });

				sut.Subject.Method3(1, 2, 3);
				sut.Subject.Method4(1, 2, 3, false);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task MultipleCallbacks_ShouldAllGetInvoked()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

				sut.Setup.Method.Method4(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
					.Callback(() => { callCount1++; })
					.Callback((v1, v2, v3, v4) => { callCount2 += v1 * v2 * v3 * v4; });

				sut.Subject.Method4(1, 2, 3, 4);
				sut.Subject.Method4(2, 2, 3, 4);

				await That(callCount1).IsEqualTo(2);
				await That(callCount2).IsEqualTo(72);
			}
		}

		public class VoidMethodWith5Parameters
		{
			[Fact]
			public async Task Callback_ShouldExecuteWhenInvoked()
			{
				int callCount = 0;
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

				sut.Setup.Method.Method5(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>(),
						With.Any<int>())
					.Callback(() => { callCount++; });

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
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

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
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

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
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

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
			public async Task CallbackWithValue_ShouldNotExecuteWhenAnyParameterDoesNotMatch(bool isMatch1,
				bool isMatch2,
				bool isMatch3, bool isMatch4, bool isMatch5)
			{
				int callCount = 0;
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

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
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

				sut.Setup.Method.Method5(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>(),
						With.Any<int>())
					.Callback((v1, v2, v3, v4, v5) => { callCount++; });

				sut.Subject.Method4(1, 2, 3, 4);
				sut.Subject.Method5(1, 2, 3, 4, 5, false);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task MultipleCallbacks_ShouldAllGetInvoked()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

				sut.Setup.Method.Method5(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>(),
						With.Any<int>())
					.Callback(() => { callCount1++; })
					.Callback((v1, v2, v3, v4, v5) => { callCount2 += v1 * v2 * v3 * v4 * v5; });

				sut.Subject.Method5(1, 2, 3, 4, 5);
				sut.Subject.Method5(2, 2, 3, 4, 5);

				await That(callCount1).IsEqualTo(2);
				await That(callCount2).IsEqualTo(360);
			}
		}

		public class VoidMethodWithParameters
		{
			[Fact]
			public async Task Callback_ShouldExecuteWhenInvoked()
			{
				int callCount = 0;
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

				sut.Setup.Method.UniqueMethodWithParameters(With.AnyParameterCombination())
					.Callback(() => { callCount++; });

				sut.Subject.UniqueMethodWithParameters(1, 2);

				await That(callCount).IsEqualTo(1);
			}

			[Fact]
			public async Task Callback_ShouldNotExecuteWhenOtherMethodIsInvoked()
			{
				int callCount = 0;
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

				sut.Setup.Method.UniqueMethodWithParameters(With.AnyParameterCombination())
					.Callback(() => { callCount++; });

				sut.Subject.Method1(1);
				sut.Subject.Method2(1, 2, false);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task MultipleCallbacks_ShouldAllGetInvoked()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				Mock<IVoidMethodSetupTest> sut = Mock.Create<IVoidMethodSetupTest>();

				sut.Setup.Method.UniqueMethodWithParameters(With.AnyParameterCombination())
					.Callback(() => { callCount1++; })
					.Callback(() => { callCount2 += 3; });

				sut.Subject.UniqueMethodWithParameters(1, 2);
				sut.Subject.UniqueMethodWithParameters(2, 2);

				await That(callCount1).IsEqualTo(2);
				await That(callCount2).IsEqualTo(6);
			}
		}
	}
}
