using System.Collections.Generic;

namespace Mockolate.Tests.MockIndexers;

public sealed partial class SetupIndexerTests
{
	public sealed class OnSetTests
	{
		[Fact]
		public async Task For_ShouldStopExecutingCallbackAfterTheGivenTimes()
		{
			List<int> invocations = [];
			IIndexerService sut = Mock.Create<IIndexerService>();
			sut.SetupMock.Indexer(It.IsAny<int>())
				.OnSet.Do((i, _, _) => { invocations.Add(i); })
				.For(4);

			for (int i = 0; i < 20; i++)
			{
				sut[i] = $"{i}";
			}

			await That(invocations).IsEqualTo([0, 1, 2, 3,]);
		}

		[Fact]
		public async Task For_WithWhen_ShouldStopExecutingCallbackAfterTheGivenTimes()
		{
			List<int> invocations = [];
			IIndexerService sut = Mock.Create<IIndexerService>();
			sut.SetupMock.Indexer(It.IsAny<int>())
				.OnSet.Do((i, _, _) => { invocations.Add(i); })
				.When(x => x > 2)
				.For(4);

			for (int i = 0; i < 20; i++)
			{
				sut[i] = $"{i}";
			}

			await That(invocations).IsEqualTo([3, 4, 5, 6,]);
		}

		[Fact]
		public async Task InParallel_ShouldInvokeCallbacksInSequence()
		{
			int callCount1 = 0;
			int callCount2 = 0;
			int callCount3 = 0;
			IIndexerService mock = Mock.Create<IIndexerService>();

			mock.SetupMock.Indexer(It.IsAny<int>())
				.OnSet.Do(() => { callCount1++; })
				.OnSet.Do((p1, _) => { callCount2 += p1; }).InParallel()
				.OnSet.Do((p1, _) => { callCount3 += p1; });

			mock[4] = "foo";
			mock[6] = "foo";
			mock[8] = "foo";
			mock[10] = "foo";

			await That(callCount1).IsEqualTo(2);
			await That(callCount2).IsEqualTo(4 + 6 + 8 + 10);
			await That(callCount3).IsEqualTo(6 + 10);
		}

		[Theory]
		[InlineData(1, 1)]
		[InlineData(2, 3)]
		[InlineData(3, 6)]
		public async Task Only_ShouldInvokeCallbacksOnlyTheGivenNumberOfTimes(int times,
			int expectedValue)
		{
			int sum = 0;
			IIndexerService mock = Mock.Create<IIndexerService>();

			mock.SetupMock.Indexer(It.IsAny<int>())
				.OnSet.Do((p1, _) => { sum += p1; }).Only(times);

			mock[1] = "foo";
			mock[2] = "foo";
			mock[3] = "foo";
			mock[4] = "foo";
			mock[5] = "foo";

			await That(sum).IsEqualTo(expectedValue);
		}

		[Fact]
		public async Task OnlyOnce_ShouldDeactivateCallbackAfterFirstExecution()
		{
			int callCount = 0;
			int sum = 0;
			IIndexerService mock = Mock.Create<IIndexerService>();

			mock.SetupMock.Indexer(It.IsAny<int>())
				.OnSet.Do(() => { callCount++; })
				.OnSet.Do((p1, _) => { sum += p1; }).OnlyOnce();

			mock[1] = "foo";
			mock[2] = "foo";
			mock[3] = "foo";
			mock[4] = "foo";

			await That(callCount).IsEqualTo(3);
			await That(sum).IsEqualTo(2);
		}

		[Fact]
		public async Task ShouldExecuteSetterCallbacks()
		{
			int callCount = 0;
			IIndexerService mock = Mock.Create<IIndexerService>();
			mock.SetupMock.Indexer(It.Satisfies<int>(i => i < 4))
				.OnSet.Do(_ => { callCount++; });

			mock[1] = "";
			mock[2] = "";
			mock[3] = "";
			mock[4] = "";
			mock[5] = "";
			mock[6] = "";

			await That(callCount).IsEqualTo(3);
		}

		[Fact]
		public async Task ShouldExecuteSetterCallbacksWithoutAnyValue()
		{
			int callCount = 0;
			IIndexerService mock = Mock.Create<IIndexerService>();
			mock.SetupMock.Indexer(It.Satisfies<int>(i => i < 4))
				.OnSet.Do(() => { callCount++; });

			mock[1] = "";
			mock[2] = "";
			mock[3] = "";
			mock[4] = "";
			mock[5] = "";

			await That(callCount).IsEqualTo(3);
		}

		[Fact]
		public async Task ShouldInvokeCallbacksInSequence()
		{
			int callCount1 = 0;
			int callCount2 = 0;
			IIndexerService mock = Mock.Create<IIndexerService>();

			mock.SetupMock.Indexer(It.IsAny<int>())
				.OnSet.Do(() => { callCount1++; })
				.OnSet.Do((p1, _) => { callCount2 += p1; });

			mock[4] = "foo";
			mock[6] = "foo";
			mock[8] = "foo";
			mock[10] = "foo";

			await That(callCount1).IsEqualTo(2);
			await That(callCount2).IsEqualTo(6 + 10);
		}

		[Fact]
		public async Task When_ShouldOnlyExecuteCallbackWhenInvocationCountMatches()
		{
			List<int> invocations = [];
			IIndexerService sut = Mock.Create<IIndexerService>();
			sut.SetupMock.Indexer(It.IsAny<int>())
				.OnSet.Do((i, _, _) => { invocations.Add(i); })
				.When(x => x is > 3 and < 9);

			for (int i = 0; i < 20; i++)
			{
				sut[i] = $"{i}";
			}

			await That(invocations).IsEqualTo([4, 5, 6, 7, 8,]);
		}

		public sealed class With2Levels
		{
			[Fact]
			public async Task For_ShouldStopExecutingCallbackAfterTheGivenTimes()
			{
				List<int> invocations = [];
				IIndexerService sut = Mock.Create<IIndexerService>();
				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>())
					.OnSet.Do((i, _, _, _) => { invocations.Add(i); })
					.For(4);

				for (int i = 0; i < 20; i++)
				{
					sut[i, 2 * i] = $"{i}";
				}

				await That(invocations).IsEqualTo([0, 1, 2, 3,]);
			}

			[Fact]
			public async Task For_WithWhen_ShouldStopExecutingCallbackAfterTheGivenTimes()
			{
				List<int> invocations = [];
				IIndexerService sut = Mock.Create<IIndexerService>();
				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>())
					.OnSet.Do((i, _, _, _) => { invocations.Add(i); })
					.When(x => x > 2)
					.For(4);

				for (int i = 0; i < 20; i++)
				{
					sut[i, 2 * i] = $"{i}";
				}

				await That(invocations).IsEqualTo([3, 4, 5, 6,]);
			}

			[Fact]
			public async Task InParallel_ShouldInvokeCallbacksInSequence()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				int callCount3 = 0;
				IIndexerService mock = Mock.Create<IIndexerService>();

				mock.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>())
					.OnSet.Do(() => { callCount1++; })
					.OnSet.Do((p1, _, _) => { callCount2 += p1; }).InParallel()
					.OnSet.Do((p1, _, _) => { callCount3 += p1; });

				mock[4, 2] = "foo";
				mock[6, 2] = "foo";
				mock[8, 2] = "foo";
				mock[10, 2] = "foo";

				await That(callCount1).IsEqualTo(2);
				await That(callCount2).IsEqualTo(4 + 6 + 8 + 10);
				await That(callCount3).IsEqualTo(6 + 10);
			}

			[Theory]
			[InlineData(1, 1)]
			[InlineData(2, 3)]
			[InlineData(3, 6)]
			public async Task Only_ShouldInvokeCallbacksOnlyTheGivenNumberOfTimes(int times,
				int expectedValue)
			{
				int sum = 0;
				IIndexerService mock = Mock.Create<IIndexerService>();

				mock.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>())
					.OnSet.Do((p1, _, _) => { sum += p1; }).Only(times);

				mock[1, 2] = "foo";
				mock[2, 2] = "foo";
				mock[3, 2] = "foo";
				mock[4, 2] = "foo";
				mock[5, 2] = "foo";

				await That(sum).IsEqualTo(expectedValue);
			}

			[Fact]
			public async Task OnlyOnce_ShouldDeactivateCallbackAfterFirstExecution()
			{
				int callCount = 0;
				int sum = 0;
				IIndexerService mock = Mock.Create<IIndexerService>();

				mock.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>())
					.OnSet.Do(() => { callCount++; })
					.OnSet.Do((p1, _, _) => { sum += p1; }).OnlyOnce();

				mock[1, 2] = "foo";
				mock[2, 2] = "foo";
				mock[3, 2] = "foo";
				mock[4, 2] = "foo";

				await That(callCount).IsEqualTo(3);
				await That(sum).IsEqualTo(2);
			}

			[Fact]
			public async Task ShouldExecuteSetterCallbacks()
			{
				int callCount = 0;
				IIndexerService mock = Mock.Create<IIndexerService>();
				mock.SetupMock.Indexer(It.Satisfies<int>(i => i < 4), It.Satisfies<int>(i => i < 4))
					.OnSet.Do(v => { callCount += v.Length; });

				mock[1, 1] = "a"; // yes (1)
				mock[1, 2] = "bb"; // yes (2)
				mock[1, 3] = "ccc"; // yes (3)
				mock[1, 4] = "dddd"; // no
				mock[1, 5] = "eeeee"; // no
				mock[6, 1] = "ffffff"; // no
				mock[6, 7] = "ggggggg"; // no
				mock[8, -9] = "hhhhhhhh"; // no
				mock[3, 3] = "iiiiiiiii"; // yes (9)

				await That(callCount).IsEqualTo(15);
			}

			[Fact]
			public async Task ShouldExecuteSetterCallbacksWithoutAnyValue()
			{
				int callCount = 0;
				IIndexerService mock = Mock.Create<IIndexerService>();
				mock.SetupMock.Indexer(It.Satisfies<int>(i => i < 4), It.Satisfies<int>(i => i < 4))
					.OnSet.Do(() => { callCount++; });

				mock[1, 1] = ""; // yes
				mock[1, 2] = ""; // yes
				mock[1, 3] = ""; // yes
				mock[1, 4] = ""; // no
				mock[5, 1] = ""; // no
				mock[2, 1] = ""; // yes

				await That(callCount).IsEqualTo(4);
			}

			[Fact]
			public async Task ShouldInvokeCallbacksInSequence()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				IIndexerService mock = Mock.Create<IIndexerService>();

				mock.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>())
					.OnSet.Do(() => { callCount1++; })
					.OnSet.Do((p1, _, _) => { callCount2 += p1; });

				mock[4, 2] = "foo";
				mock[6, 2] = "foo";
				mock[8, 2] = "foo";
				mock[10, 2] = "foo";

				await That(callCount1).IsEqualTo(2);
				await That(callCount2).IsEqualTo(6 + 10);
			}

			[Fact]
			public async Task When_ShouldOnlyExecuteCallbackWhenInvocationCountMatches()
			{
				List<int> invocations = [];
				IIndexerService sut = Mock.Create<IIndexerService>();
				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>())
					.OnSet.Do((i, _, _, _) => { invocations.Add(i); })
					.When(x => x is > 3 and < 9);

				for (int i = 0; i < 20; i++)
				{
					sut[i, 2 * i] = $"{i}";
				}

				await That(invocations).IsEqualTo([4, 5, 6, 7, 8,]);
			}
		}

		public sealed class With3Levels
		{
			[Fact]
			public async Task For_ShouldStopExecutingCallbackAfterTheGivenTimes()
			{
				List<int> invocations = [];
				IIndexerService sut = Mock.Create<IIndexerService>();
				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.OnSet.Do((i, _, _, _, _) => { invocations.Add(i); })
					.For(4);

				for (int i = 0; i < 20; i++)
				{
					sut[i, 2 * i, 3 * i] = $"{i}";
				}

				await That(invocations).IsEqualTo([0, 1, 2, 3,]);
			}

			[Fact]
			public async Task For_WithWhen_ShouldStopExecutingCallbackAfterTheGivenTimes()
			{
				List<int> invocations = [];
				IIndexerService sut = Mock.Create<IIndexerService>();
				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.OnSet.Do((i, _, _, _, _) => { invocations.Add(i); })
					.When(x => x > 2)
					.For(4);

				for (int i = 0; i < 20; i++)
				{
					sut[i, 2 * i, 3 * i] = $"{i}";
				}

				await That(invocations).IsEqualTo([3, 4, 5, 6,]);
			}

			[Fact]
			public async Task InParallel_ShouldInvokeCallbacksInSequence()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				int callCount3 = 0;
				IIndexerService mock = Mock.Create<IIndexerService>();

				mock.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.OnSet.Do(() => { callCount1++; })
					.OnSet.Do((p1, _, _, _) => { callCount2 += p1; }).InParallel()
					.OnSet.Do((p1, _, _, _) => { callCount3 += p1; });

				mock[4, 2, 3] = "foo";
				mock[6, 2, 3] = "foo";
				mock[8, 2, 3] = "foo";
				mock[10, 2, 3] = "foo";

				await That(callCount1).IsEqualTo(2);
				await That(callCount2).IsEqualTo(4 + 6 + 8 + 10);
				await That(callCount3).IsEqualTo(6 + 10);
			}

			[Theory]
			[InlineData(1, 1)]
			[InlineData(2, 3)]
			[InlineData(3, 6)]
			public async Task Only_ShouldInvokeCallbacksOnlyTheGivenNumberOfTimes(int times,
				int expectedValue)
			{
				int sum = 0;
				IIndexerService mock = Mock.Create<IIndexerService>();

				mock.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.OnSet.Do((p1, _, _, _) => { sum += p1; }).Only(times);

				mock[1, 2, 3] = "foo";
				mock[2, 2, 3] = "foo";
				mock[3, 2, 3] = "foo";
				mock[4, 2, 3] = "foo";
				mock[5, 2, 3] = "foo";

				await That(sum).IsEqualTo(expectedValue);
			}

			[Fact]
			public async Task OnlyOnce_ShouldDeactivateCallbackAfterFirstExecution()
			{
				int callCount = 0;
				int sum = 0;
				IIndexerService mock = Mock.Create<IIndexerService>();

				mock.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.OnSet.Do(() => { callCount++; })
					.OnSet.Do((p1, _, _, _) => { sum += p1; }).OnlyOnce();

				mock[1, 2, 3] = "foo";
				mock[2, 2, 3] = "foo";
				mock[3, 2, 3] = "foo";
				mock[4, 2, 3] = "foo";

				await That(callCount).IsEqualTo(3);
				await That(sum).IsEqualTo(2);
			}

			[Fact]
			public async Task ShouldExecuteSetterCallbacks()
			{
				int callCount = 0;
				IIndexerService mock = Mock.Create<IIndexerService>();
				mock.SetupMock.Indexer(It.Satisfies<int>(i => i < 4), It.Satisfies<int>(i => i < 4),
						It.Satisfies<int>(i => i < 4))
					.OnSet.Do(v => { callCount += v.Length; });

				mock[1, 1, 1] = "a"; // yes (1)
				mock[1, 2, 1] = "bb"; // yes (2)
				mock[3, 1, 2] = "ccc"; // yes (3)
				mock[1, 1, 4] = "dddd"; // no
				mock[1, 5, 1] = "eeeee"; // no
				mock[6, 1, 1] = "ffffff"; // no
				mock[6, 7, 8] = "ggggggg"; // no
				mock[8, -9, 1] = "hhhhhhhh"; // no
				mock[3, 3, 3] = "iiiiiiiii"; // yes (9)

				await That(callCount).IsEqualTo(15);
			}

			[Fact]
			public async Task ShouldExecuteSetterCallbacksWithoutAnyValue()
			{
				int callCount = 0;
				IIndexerService mock = Mock.Create<IIndexerService>();
				mock.SetupMock.Indexer(It.Satisfies<int>(i => i < 4), It.Satisfies<int>(i => i < 4),
						It.Satisfies<int>(i => i < 4))
					.OnSet.Do(() => { callCount++; });

				mock[1, 1, 1] = ""; // yes
				mock[1, 1, 2] = ""; // yes
				mock[1, 3, 1] = ""; // yes
				mock[1, 1, 4] = ""; // no
				mock[1, 5, 1] = ""; // no
				mock[6, 1, 1] = ""; // no
				mock[2, 1, 1] = ""; // yes

				await That(callCount).IsEqualTo(4);
			}

			[Fact]
			public async Task ShouldInvokeCallbacksInSequence()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				IIndexerService mock = Mock.Create<IIndexerService>();

				mock.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.OnSet.Do(() => { callCount1++; })
					.OnSet.Do((p1, _, _, _) => { callCount2 += p1; });

				mock[4, 2, 3] = "foo";
				mock[6, 2, 3] = "foo";
				mock[8, 2, 3] = "foo";
				mock[10, 2, 3] = "foo";

				await That(callCount1).IsEqualTo(2);
				await That(callCount2).IsEqualTo(6 + 10);
			}

			[Fact]
			public async Task When_ShouldOnlyExecuteCallbackWhenInvocationCountMatches()
			{
				List<int> invocations = [];
				IIndexerService sut = Mock.Create<IIndexerService>();
				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.OnSet.Do((i, _, _, _, _) => { invocations.Add(i); })
					.When(x => x is > 3 and < 9);

				for (int i = 0; i < 20; i++)
				{
					sut[i, 2 * i, 3 * i] = $"{i}";
				}

				await That(invocations).IsEqualTo([4, 5, 6, 7, 8,]);
			}
		}

		public sealed class With4Levels
		{
			[Fact]
			public async Task For_ShouldStopExecutingCallbackAfterTheGivenTimes()
			{
				List<int> invocations = [];
				IIndexerService sut = Mock.Create<IIndexerService>();
				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.OnSet.Do((i, _, _, _, _, _) => { invocations.Add(i); })
					.For(4);

				for (int i = 0; i < 20; i++)
				{
					sut[i, 2 * i, 3 * i, 4 * i] = $"{i}";
				}

				await That(invocations).IsEqualTo([0, 1, 2, 3,]);
			}

			[Fact]
			public async Task For_WithWhen_ShouldStopExecutingCallbackAfterTheGivenTimes()
			{
				List<int> invocations = [];
				IIndexerService sut = Mock.Create<IIndexerService>();
				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.OnSet.Do((i, _, _, _, _, _) => { invocations.Add(i); })
					.When(x => x > 2)
					.For(4);

				for (int i = 0; i < 20; i++)
				{
					sut[i, 2 * i, 3 * i, 4 * i] = $"{i}";
				}

				await That(invocations).IsEqualTo([3, 4, 5, 6,]);
			}

			[Fact]
			public async Task InParallel_ShouldInvokeCallbacksInSequence()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				int callCount3 = 0;
				IIndexerService mock = Mock.Create<IIndexerService>();

				mock.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.OnSet.Do(() => { callCount1++; })
					.OnSet.Do((p1, _, _, _, _) => { callCount2 += p1; }).InParallel()
					.OnSet.Do((p1, _, _, _, _) => { callCount3 += p1; });

				mock[4, 2, 3, 4] = "foo";
				mock[6, 2, 3, 4] = "foo";
				mock[8, 2, 3, 4] = "foo";
				mock[10, 2, 3, 4] = "foo";

				await That(callCount1).IsEqualTo(2);
				await That(callCount2).IsEqualTo(4 + 6 + 8 + 10);
				await That(callCount3).IsEqualTo(6 + 10);
			}

			[Theory]
			[InlineData(1, 1)]
			[InlineData(2, 3)]
			[InlineData(3, 6)]
			public async Task Only_ShouldInvokeCallbacksOnlyTheGivenNumberOfTimes(int times,
				int expectedValue)
			{
				int sum = 0;
				IIndexerService mock = Mock.Create<IIndexerService>();

				mock.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.OnSet.Do((p1, _, _, _, _) => { sum += p1; }).Only(times);

				mock[1, 2, 3, 4] = "foo";
				mock[2, 2, 3, 4] = "foo";
				mock[3, 2, 3, 4] = "foo";
				mock[4, 2, 3, 4] = "foo";
				mock[5, 2, 3, 4] = "foo";

				await That(sum).IsEqualTo(expectedValue);
			}

			[Fact]
			public async Task OnlyOnce_ShouldDeactivateCallbackAfterFirstExecution()
			{
				int callCount = 0;
				int sum = 0;
				IIndexerService mock = Mock.Create<IIndexerService>();

				mock.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.OnSet.Do(() => { callCount++; })
					.OnSet.Do((p1, _, _, _, _) => { sum += p1; }).OnlyOnce();

				mock[1, 2, 3, 4] = "foo";
				mock[2, 2, 3, 4] = "foo";
				mock[3, 2, 3, 4] = "foo";
				mock[4, 2, 3, 4] = "foo";

				await That(callCount).IsEqualTo(3);
				await That(sum).IsEqualTo(2);
			}

			[Fact]
			public async Task ShouldExecuteSetterCallbacks()
			{
				int callCount = 0;
				IIndexerService mock = Mock.Create<IIndexerService>();
				mock.SetupMock.Indexer(It.Satisfies<int>(i => i < 5), It.Satisfies<int>(i => i < 5),
						It.Satisfies<int>(i => i < 5), It.Satisfies<int>(i => i < 5))
					.OnSet.Do(v => { callCount += v.Length; });

				mock[1, 1, 1, 1] = "a"; // yes (1)
				mock[1, 2, 1, 3] = "bb"; // yes (2)
				mock[3, 1, 2, 4] = "ccc"; // yes (3)
				mock[1, 1, 4, 3] = "dddd"; // yes (4)
				mock[1, 5, 1, 1] = "eeeee"; // no
				mock[6, 1, 1, 1] = "ffffff"; // no
				mock[6, 7, 8, 9] = "ggggggg"; // no
				mock[8, -9, 1, 3] = "hhhhhhhh"; // no
				mock[4, 4, 4, 4] = "iiiiiiiii"; // yes (9)

				await That(callCount).IsEqualTo(19);
			}

			[Fact]
			public async Task ShouldExecuteSetterCallbacksWithoutAnyValue()
			{
				int callCount = 0;
				IIndexerService mock = Mock.Create<IIndexerService>();
				mock.SetupMock.Indexer(It.Satisfies<int>(i => i < 5), It.Satisfies<int>(i => i < 5),
						It.Satisfies<int>(i => i < 5), It.Satisfies<int>(i => i < 5))
					.OnSet.Do(() => { callCount++; });

				mock[1, 1, 1, 1] = ""; // yes
				mock[1, 1, 2, 2] = ""; // yes
				mock[1, 3, 1, 3] = ""; // yes
				mock[1, 1, 4, 4] = ""; // yes
				mock[1, 5, 1, 1] = ""; // no
				mock[6, 1, 1, 1] = ""; // no
				mock[2, 1, 1, 3] = ""; // yes

				await That(callCount).IsEqualTo(5);
			}

			[Fact]
			public async Task ShouldInvokeCallbacksInSequence()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				IIndexerService mock = Mock.Create<IIndexerService>();

				mock.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.OnSet.Do(() => { callCount1++; })
					.OnSet.Do((p1, _, _, _, _) => { callCount2 += p1; });

				mock[4, 2, 3, 4] = "foo";
				mock[6, 2, 3, 4] = "foo";
				mock[8, 2, 3, 4] = "foo";
				mock[10, 2, 3, 4] = "foo";

				await That(callCount1).IsEqualTo(2);
				await That(callCount2).IsEqualTo(6 + 10);
			}

			[Fact]
			public async Task When_ShouldOnlyExecuteCallbackWhenInvocationCountMatches()
			{
				List<int> invocations = [];
				IIndexerService sut = Mock.Create<IIndexerService>();
				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.OnSet.Do((i, _, _, _, _, _) => { invocations.Add(i); })
					.When(x => x is > 3 and < 9);

				for (int i = 0; i < 20; i++)
				{
					sut[i, 2 * i, 3 * i, 4 * i] = $"{i}";
				}

				await That(invocations).IsEqualTo([4, 5, 6, 7, 8,]);
			}
		}

		public sealed class With5Levels
		{
			[Fact]
			public async Task For_ShouldStopExecutingCallbackAfterTheGivenTimes()
			{
				List<int> invocations = [];
				IIndexerService sut = Mock.Create<IIndexerService>();
				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.OnSet.Do((i, _, _, _, _, _, _) => { invocations.Add(i); })
					.For(4);

				for (int i = 0; i < 20; i++)
				{
					sut[i, 2 * i, 3 * i, 4 * i, 5 * i] = $"{i}";
				}

				await That(invocations).IsEqualTo([0, 1, 2, 3,]);
			}

			[Fact]
			public async Task For_WithWhen_ShouldStopExecutingCallbackAfterTheGivenTimes()
			{
				List<int> invocations = [];
				IIndexerService sut = Mock.Create<IIndexerService>();
				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.OnSet.Do((i, _, _, _, _, _, _) => { invocations.Add(i); })
					.When(x => x > 2)
					.For(4);

				for (int i = 0; i < 20; i++)
				{
					sut[i, 2 * i, 3 * i, 4 * i, 5 * i] = $"{i}";
				}

				await That(invocations).IsEqualTo([3, 4, 5, 6,]);
			}

			[Fact]
			public async Task InParallel_ShouldInvokeCallbacksInSequence()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				int callCount3 = 0;
				IIndexerService mock = Mock.Create<IIndexerService>();

				mock.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.OnSet.Do(() => { callCount1++; })
					.OnSet.Do((p1, _, _, _, _, _) => { callCount2 += p1; }).InParallel()
					.OnSet.Do((p1, _, _, _, _, _) => { callCount3 += p1; });

				mock[4, 2, 3, 4, 5] = "foo";
				mock[6, 2, 3, 4, 5] = "foo";
				mock[8, 2, 3, 4, 5] = "foo";
				mock[10, 2, 3, 4, 5] = "foo";

				await That(callCount1).IsEqualTo(2);
				await That(callCount2).IsEqualTo(4 + 6 + 8 + 10);
				await That(callCount3).IsEqualTo(6 + 10);
			}

			[Theory]
			[InlineData(1, 1)]
			[InlineData(2, 3)]
			[InlineData(3, 6)]
			public async Task Only_ShouldInvokeCallbacksOnlyTheGivenNumberOfTimes(int times,
				int expectedValue)
			{
				int sum = 0;
				IIndexerService mock = Mock.Create<IIndexerService>();

				mock.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.OnSet.Do((p1, _, _, _, _, _) => { sum += p1; }).Only(times);

				mock[1, 2, 3, 4, 5] = "foo";
				mock[2, 2, 3, 4, 5] = "foo";
				mock[3, 2, 3, 4, 5] = "foo";
				mock[4, 2, 3, 4, 5] = "foo";
				mock[5, 2, 3, 4, 5] = "foo";

				await That(sum).IsEqualTo(expectedValue);
			}

			[Fact]
			public async Task OnlyOnce_ShouldDeactivateCallbackAfterFirstExecution()
			{
				int callCount = 0;
				int sum = 0;
				IIndexerService mock = Mock.Create<IIndexerService>();

				mock.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.OnSet.Do(() => { callCount++; })
					.OnSet.Do((p1, _, _, _, _, _) => { sum += p1; }).OnlyOnce();

				mock[1, 2, 3, 4, 5] = "foo";
				mock[2, 2, 3, 4, 5] = "foo";
				mock[3, 2, 3, 4, 5] = "foo";
				mock[4, 2, 3, 4, 5] = "foo";

				await That(callCount).IsEqualTo(3);
				await That(sum).IsEqualTo(2);
			}

			[Fact]
			public async Task ShouldExecuteSetterCallbacks()
			{
				int callCount = 0;
				IIndexerService mock = Mock.Create<IIndexerService>();
				mock.SetupMock.Indexer(It.Satisfies<int>(i => i < 6), It.Satisfies<int>(i => i < 6),
						It.Satisfies<int>(i => i < 6), It.Satisfies<int>(i => i < 6), It.Satisfies<int>(i => i < 6))
					.OnSet.Do(v => { callCount += v.Length; });

				mock[1, 1, 1, 1, 1] = "a"; // yes (1)
				mock[4, 1, 2, 1, 3] = "bb"; // yes (2)
				mock[1, 3, 1, 2, 4] = "ccc"; // yes (3)
				mock[2, 1, 1, 4, 3] = "dddd"; // yes (4)
				mock[1, 1, 5, 1, 1] = "eeeee"; // yes (5)
				mock[1, 6, 1, 1, 1] = "ffffff"; // no
				mock[5, 6, 7, 8, 9] = "ggggggg"; // no
				mock[8, 8, -9, 1, 3] = "hhhhhhhh"; // no
				mock[5, 5, 5, 5, 5] = "iiiiiiiii"; // yes (9)

				await That(callCount).IsEqualTo(24);
			}

			[Fact]
			public async Task ShouldExecuteSetterCallbacksWithoutAnyValue()
			{
				int callCount = 0;
				IIndexerService mock = Mock.Create<IIndexerService>();
				mock.SetupMock.Indexer(It.Satisfies<int>(i => i < 6), It.Satisfies<int>(i => i < 6),
						It.Satisfies<int>(i => i < 6), It.Satisfies<int>(i => i < 6), It.Satisfies<int>(i => i < 6))
					.OnSet.Do(() => { callCount++; });

				mock[1, 1, 1, 1, 1] = ""; // yes
				mock[1, 1, 1, 2, 2] = ""; // yes
				mock[1, 1, 3, 1, 3] = ""; // yes
				mock[1, 1, 1, 4, 4] = ""; // yes
				mock[1, 1, 6, 1, 1] = ""; // no
				mock[1, 6, 1, 1, 1] = ""; // no
				mock[1, 2, 1, 1, 3] = ""; // yes

				await That(callCount).IsEqualTo(5);
			}

			[Fact]
			public async Task ShouldInvokeCallbacksInSequence()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				IIndexerService mock = Mock.Create<IIndexerService>();

				mock.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.OnSet.Do(() => { callCount1++; })
					.OnSet.Do((p1, _, _, _, _, _) => { callCount2 += p1; });

				mock[4, 2, 3, 4, 5] = "foo";
				mock[6, 2, 3, 4, 5] = "foo";
				mock[8, 2, 3, 4, 5] = "foo";
				mock[10, 2, 3, 4, 5] = "foo";

				await That(callCount1).IsEqualTo(2);
				await That(callCount2).IsEqualTo(6 + 10);
			}

			[Fact]
			public async Task When_ShouldOnlyExecuteCallbackWhenInvocationCountMatches()
			{
				List<int> invocations = [];
				IIndexerService sut = Mock.Create<IIndexerService>();
				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.OnSet.Do((i, _, _, _, _, _, _) => { invocations.Add(i); })
					.When(x => x is > 3 and < 9);

				for (int i = 0; i < 20; i++)
				{
					sut[i, 2 * i, 3 * i, 4 * i, 5 * i] = $"{i}";
				}

				await That(invocations).IsEqualTo([4, 5, 6, 7, 8,]);
			}
		}
	}
}
