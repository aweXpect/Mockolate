using System.Collections.Generic;

namespace Mockolate.Tests.MockIndexers;

public sealed partial class SetupIndexerTests
{
	public sealed class OnGetTests
	{
		[Fact]
		public async Task For_ShouldStopExecutingCallbackAfterTheGivenTimes()
		{
			List<int> invocations = [];
			IIndexerService sut = Mock.Create<IIndexerService>();
			sut.SetupMock.Indexer(It.IsAny<int>())
				.OnGet((i, _) => { invocations.Add(i); })
				.For(4);

			for (int i = 0; i < 20; i++)
			{
				_ = sut[i];
			}

			await That(invocations).IsEqualTo([0, 1, 2, 3,]);
		}

		[Fact]
		public async Task For_WithWhen_ShouldStopExecutingCallbackAfterTheGivenTimes()
		{
			List<int> invocations = [];
			IIndexerService sut = Mock.Create<IIndexerService>();
			sut.SetupMock.Indexer(It.IsAny<int>())
				.OnGet((i, _) => { invocations.Add(i); })
				.When(x => x > 2)
				.For(4);

			for (int i = 0; i < 20; i++)
			{
				_ = sut[i];
			}

			await That(invocations).IsEqualTo([3, 4, 5, 6,]);
		}

		[Fact]
		public async Task InParallel_ShouldInvokeParallelCallbacksAlways()
		{
			int callCount1 = 0;
			int callCount2 = 0;
			int callCount3 = 0;
			IIndexerService sut = Mock.Create<IIndexerService>();

			sut.SetupMock.Indexer(It.IsAny<int>())
				.OnGet(() => { callCount1++; })
				.OnGet(v => { callCount2 += v; }).InParallel()
				.OnGet(v => { callCount3 += v; });

			_ = sut[1];
			_ = sut[2];
			_ = sut[3];
			_ = sut[4];

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
			IIndexerService sut = Mock.Create<IIndexerService>();

			sut.SetupMock.Indexer(It.IsAny<int>())
				.OnGet(v => { sum += v; }).Only(times);

			_ = sut[1];
			_ = sut[2];
			_ = sut[3];
			_ = sut[4];

			await That(sum).IsEqualTo(expectedValue);
		}

		[Fact]
		public async Task OnlyOnce_ShouldDeactivateCallbackAfterFirstExecution()
		{
			int callCount1 = 0;
			int callCount2 = 0;
			IIndexerService sut = Mock.Create<IIndexerService>();

			sut.SetupMock.Indexer(It.IsAny<int>())
				.OnGet(() => { callCount1++; })
				.OnGet(v => { callCount2 += v; }).OnlyOnce();

			_ = sut[1];
			_ = sut[2];
			_ = sut[3];
			_ = sut[4];

			await That(callCount1).IsEqualTo(3);
			await That(callCount2).IsEqualTo(2);
		}

		[Fact]
		public async Task ShouldExecuteGetterCallbacks()
		{
			int callCount = 0;
			IIndexerService mock = Mock.Create<IIndexerService>();
			mock.SetupMock.Indexer(It.Is<int>(i => i < 4))
				.OnGet(() => { callCount++; });

			_ = mock[1];
			_ = mock[2];
			_ = mock[3];
			_ = mock[4];
			_ = mock[5];
			_ = mock[6];

			await That(callCount).IsEqualTo(3);
		}

		[Fact]
		public async Task ShouldExecuteGetterCallbacksWithValue()
		{
			int callCount = 0;
			IIndexerService mock = Mock.Create<IIndexerService>();
			mock.SetupMock.Indexer(It.Is<int>(i => i < 4))
				.OnGet(v => { callCount += v; });

			_ = mock[1];
			_ = mock[2];
			_ = mock[3];
			_ = mock[4];
			_ = mock[5];

			await That(callCount).IsEqualTo(6);
		}

		[Fact]
		public async Task ShouldInvokeCallbacksInSequence()
		{
			int callCount1 = 0;
			int callCount2 = 0;
			IIndexerService sut = Mock.Create<IIndexerService>();

			sut.SetupMock.Indexer(It.IsAny<int>())
				.OnGet(() => { callCount1++; })
				.OnGet(v => { callCount2 += v; });

			_ = sut[1];
			_ = sut[2];
			_ = sut[3];
			_ = sut[4];

			await That(callCount1).IsEqualTo(2);
			await That(callCount2).IsEqualTo(6);
		}

		[Fact]
		public async Task When_ShouldOnlyExecuteCallbackWhenInvocationCountMatches()
		{
			List<int> invocations = [];
			IIndexerService sut = Mock.Create<IIndexerService>();
			sut.SetupMock.Indexer(It.IsAny<int>())
				.OnGet((i, _) => { invocations.Add(i); })
				.When(x => x is > 3 and < 9);

			for (int i = 0; i < 20; i++)
			{
				_ = sut[i];
			}

			await That(invocations).IsEqualTo([4, 5, 6, 7, 8,]);
		}

		[Fact]
		public async Task WhenLengthDoesNotMatch_ShouldIgnore()
		{
			int callCount = 0;
			IIndexerService mock = Mock.Create<IIndexerService>();
			mock.SetupMock.Indexer(It.IsAny<int>())
				.OnGet(() => { callCount++; });

			_ = mock[1];
			_ = mock[2, 2];
			_ = mock[3, 3, 3];

			await That(callCount).IsEqualTo(1);
		}

		public sealed class With2Levels
		{
			[Fact]
			public async Task Callback_WhenLengthDoesNotMatch_ShouldIgnore()
			{
				int callCount = 0;
				IIndexerService mock = Mock.Create<IIndexerService>();
				mock.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>())
					.OnGet(() => { callCount++; });

				_ = mock[1];
				_ = mock[2, 2];
				_ = mock[3, 3, 3];

				await That(callCount).IsEqualTo(1);
			}

			[Fact]
			public async Task For_ShouldStopExecutingCallbackAfterTheGivenTimes()
			{
				List<int> invocations = [];
				IIndexerService sut = Mock.Create<IIndexerService>();
				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>())
					.OnGet((i, _, _) => { invocations.Add(i); })
					.For(4);

				for (int i = 0; i < 20; i++)
				{
					_ = sut[i, 2 * i];
				}

				await That(invocations).IsEqualTo([0, 1, 2, 3,]);
			}

			[Fact]
			public async Task For_WithWhen_ShouldStopExecutingCallbackAfterTheGivenTimes()
			{
				List<int> invocations = [];
				IIndexerService sut = Mock.Create<IIndexerService>();
				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>())
					.OnGet((i, _, _) => { invocations.Add(i); })
					.When(x => x > 2)
					.For(4);

				for (int i = 0; i < 20; i++)
				{
					_ = sut[i, 2 * i];
				}

				await That(invocations).IsEqualTo([3, 4, 5, 6,]);
			}

			[Fact]
			public async Task InParallel_ShouldInvokeParallelCallbacksAlways()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				int callCount3 = 0;
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>())
					.OnGet(() => { callCount1++; })
					.OnGet((p1, _) => { callCount2 += p1; }).InParallel()
					.OnGet((p1, _) => { callCount3 += p1; });

				_ = sut[1, 2];
				_ = sut[2, 2];
				_ = sut[3, 2];
				_ = sut[4, 2];

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
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>())
					.OnGet((p1, _) => { sum += p1; }).Only(times);

				_ = sut[1, 2];
				_ = sut[2, 2];
				_ = sut[3, 2];
				_ = sut[4, 2];

				await That(sum).IsEqualTo(expectedValue);
			}

			[Fact]
			public async Task OnlyOnce_ShouldDeactivateCallbackAfterFirstExecution()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>())
					.OnGet(() => { callCount1++; })
					.OnGet((p1, _) => { callCount2 += p1; }).OnlyOnce();

				_ = sut[1, 2];
				_ = sut[2, 2];
				_ = sut[3, 2];
				_ = sut[4, 2];

				await That(callCount1).IsEqualTo(3);
				await That(callCount2).IsEqualTo(2);
			}

			[Fact]
			public async Task ShouldExecuteGetterCallbacks()
			{
				int callCount = 0;
				IIndexerService mock = Mock.Create<IIndexerService>();
				mock.SetupMock.Indexer(It.Is<int>(i => i < 4), It.Is<int>(i => i < 4))
					.OnGet(() => { callCount++; });

				_ = mock[5, 1]; // no
				_ = mock[3, 2]; // yes
				_ = mock[2, 3]; // yes
				_ = mock[1, 4]; // no
				_ = mock[1, -4]; // yes
				_ = mock[8, 6]; // no

				await That(callCount).IsEqualTo(3);
			}

			[Fact]
			public async Task ShouldExecuteGetterCallbacksWithValue()
			{
				int callCount = 0;
				IIndexerService mock = Mock.Create<IIndexerService>();
				mock.SetupMock.Indexer(It.Is<int>(i => i < 4), It.Is<int>(i => i < 4))
					.OnGet((v1, v2) => { callCount += v1 * v2; });

				_ = mock[5, 1]; // no
				_ = mock[3, 2]; // yes (6)
				_ = mock[2, 3]; // yes (6)
				_ = mock[1, 4]; // no
				_ = mock[1, -4]; // yes (-4)
				_ = mock[8, 6]; // no

				await That(callCount).IsEqualTo(8);
			}

			[Fact]
			public async Task ShouldInvokeCallbacksInSequence()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>())
					.OnGet(() => { callCount1++; })
					.OnGet((p1, _) => { callCount2 += p1; });

				_ = sut[1, 2];
				_ = sut[2, 2];
				_ = sut[3, 2];
				_ = sut[4, 2];

				await That(callCount1).IsEqualTo(2);
				await That(callCount2).IsEqualTo(6);
			}

			[Fact]
			public async Task When_ShouldOnlyExecuteCallbackWhenInvocationCountMatches()
			{
				List<int> invocations = [];
				IIndexerService sut = Mock.Create<IIndexerService>();
				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>())
					.OnGet((i, _, _) => { invocations.Add(i); })
					.When(x => x is > 3 and < 9);

				for (int i = 0; i < 20; i++)
				{
					_ = sut[i, 2 * i];
				}

				await That(invocations).IsEqualTo([4, 5, 6, 7, 8,]);
			}
		}

		public sealed class With3Levels
		{
			[Fact]
			public async Task Callback_WhenLengthDoesNotMatch_ShouldIgnore()
			{
				int callCount = 0;
				IIndexerService mock = Mock.Create<IIndexerService>();
				mock.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.OnGet(() => { callCount++; });

				_ = mock[1];
				_ = mock[2, 2];
				_ = mock[3, 3, 3];
				_ = mock[4, 4, 4, 4];

				await That(callCount).IsEqualTo(1);
			}

			[Fact]
			public async Task For_ShouldStopExecutingCallbackAfterTheGivenTimes()
			{
				List<int> invocations = [];
				IIndexerService sut = Mock.Create<IIndexerService>();
				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.OnGet((i, _, _, _) => { invocations.Add(i); })
					.For(4);

				for (int i = 0; i < 20; i++)
				{
					_ = sut[i, 2 * i, 3 * i];
				}

				await That(invocations).IsEqualTo([0, 1, 2, 3,]);
			}

			[Fact]
			public async Task For_WithWhen_ShouldStopExecutingCallbackAfterTheGivenTimes()
			{
				List<int> invocations = [];
				IIndexerService sut = Mock.Create<IIndexerService>();
				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.OnGet((i, _, _, _) => { invocations.Add(i); })
					.When(x => x > 2)
					.For(4);

				for (int i = 0; i < 20; i++)
				{
					_ = sut[i, 2 * i, 3 * i];
				}

				await That(invocations).IsEqualTo([3, 4, 5, 6,]);
			}

			[Fact]
			public async Task InParallel_ShouldInvokeParallelCallbacksAlways()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				int callCount3 = 0;
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.OnGet(() => { callCount1++; })
					.OnGet((p1, _, _) => { callCount2 += p1; }).InParallel()
					.OnGet((p1, _, _) => { callCount3 += p1; });

				_ = sut[1, 2, 3];
				_ = sut[2, 2, 3];
				_ = sut[3, 2, 3];
				_ = sut[4, 2, 3];

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
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.OnGet((p1, _, _) => { sum += p1; }).Only(times);

				_ = sut[1, 2, 3];
				_ = sut[2, 2, 3];
				_ = sut[3, 2, 3];
				_ = sut[4, 2, 3];

				await That(sum).IsEqualTo(expectedValue);
			}

			[Fact]
			public async Task OnlyOnce_ShouldDeactivateCallbackAfterFirstExecution()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.OnGet(() => { callCount1++; })
					.OnGet((p1, _, _) => { callCount2 += p1; }).OnlyOnce();

				_ = sut[1, 2, 3];
				_ = sut[2, 2, 3];
				_ = sut[3, 2, 3];
				_ = sut[4, 2, 3];

				await That(callCount1).IsEqualTo(3);
				await That(callCount2).IsEqualTo(2);
			}

			[Fact]
			public async Task ShouldExecuteGetterCallbacks()
			{
				int callCount = 0;
				IIndexerService mock = Mock.Create<IIndexerService>();
				mock.SetupMock.Indexer(It.Is<int>(i => i < 4), It.Is<int>(i => i < 4),
						It.Is<int>(i => i < 4))
					.OnGet(() => { callCount++; });

				_ = mock[1, 5, 1]; // no
				_ = mock[3, 1, 2]; // yes
				_ = mock[2, 2, 3]; // yes
				_ = mock[1, 1, 4]; // no
				_ = mock[1, 1, -4]; // yes
				_ = mock[6, 2, 1]; // no
				_ = mock[6, 7, 8]; // no

				await That(callCount).IsEqualTo(3);
			}

			[Fact]
			public async Task ShouldExecuteGetterCallbacksWithValue()
			{
				int callCount = 0;
				IIndexerService mock = Mock.Create<IIndexerService>();
				mock.SetupMock.Indexer(It.Is<int>(i => i < 4), It.Is<int>(i => i < 4),
						It.Is<int>(i => i < 4))
					.OnGet((v1, v2, v3) => { callCount += v1 * v2 * v3; });

				_ = mock[1, 5, 1]; // no
				_ = mock[3, 1, 2]; // yes (6)
				_ = mock[2, 2, 3]; // yes (12)
				_ = mock[1, 1, 4]; // no
				_ = mock[1, 1, -4]; // yes (-4)
				_ = mock[6, 2, 1]; // no
				_ = mock[6, 7, 8]; // no

				await That(callCount).IsEqualTo(14);
			}

			[Fact]
			public async Task ShouldInvokeCallbacksInSequence()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.OnGet(() => { callCount1++; })
					.OnGet((p1, _, _) => { callCount2 += p1; });

				_ = sut[1, 2, 3];
				_ = sut[2, 2, 3];
				_ = sut[3, 2, 3];
				_ = sut[4, 2, 3];

				await That(callCount1).IsEqualTo(2);
				await That(callCount2).IsEqualTo(6);
			}

			[Fact]
			public async Task When_ShouldOnlyExecuteCallbackWhenInvocationCountMatches()
			{
				List<int> invocations = [];
				IIndexerService sut = Mock.Create<IIndexerService>();
				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.OnGet((i, _, _, _) => { invocations.Add(i); })
					.When(x => x is > 3 and < 9);

				for (int i = 0; i < 20; i++)
				{
					_ = sut[i, 2 * i, 3 * i];
				}

				await That(invocations).IsEqualTo([4, 5, 6, 7, 8,]);
			}
		}

		public sealed class With4Levels
		{
			[Fact]
			public async Task Callback_WhenLengthDoesNotMatch_ShouldIgnore()
			{
				int callCount = 0;
				IIndexerService mock = Mock.Create<IIndexerService>();
				mock.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.OnGet(() => { callCount++; });

				_ = mock[1];
				_ = mock[2, 2];
				_ = mock[3, 3, 3];
				_ = mock[4, 4, 4, 4];

				await That(callCount).IsEqualTo(1);
			}

			[Fact]
			public async Task For_ShouldStopExecutingCallbackAfterTheGivenTimes()
			{
				List<int> invocations = [];
				IIndexerService sut = Mock.Create<IIndexerService>();
				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.OnGet((i, _, _, _, _) => { invocations.Add(i); })
					.For(4);

				for (int i = 0; i < 20; i++)
				{
					_ = sut[i, 2 * i, 3 * i, 4 * i];
				}

				await That(invocations).IsEqualTo([0, 1, 2, 3,]);
			}

			[Fact]
			public async Task For_WithWhen_ShouldStopExecutingCallbackAfterTheGivenTimes()
			{
				List<int> invocations = [];
				IIndexerService sut = Mock.Create<IIndexerService>();
				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.OnGet((i, _, _, _, _) => { invocations.Add(i); })
					.When(x => x > 2)
					.For(4);

				for (int i = 0; i < 20; i++)
				{
					_ = sut[i, 2 * i, 3 * i, 4 * i];
				}

				await That(invocations).IsEqualTo([3, 4, 5, 6,]);
			}

			[Fact]
			public async Task InParallel_ShouldInvokeParallelCallbacksAlways()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				int callCount3 = 0;
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.OnGet(() => { callCount1++; })
					.OnGet((p1, _, _, _) => { callCount2 += p1; }).InParallel()
					.OnGet((p1, _, _, _) => { callCount3 += p1; });

				_ = sut[1, 2, 3, 4];
				_ = sut[2, 2, 3, 4];
				_ = sut[3, 2, 3, 4];
				_ = sut[4, 2, 3, 4];

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
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.OnGet((p1, _, _, _) => { sum += p1; }).Only(times);

				_ = sut[1, 2, 3, 4];
				_ = sut[2, 2, 3, 4];
				_ = sut[3, 2, 3, 4];
				_ = sut[4, 2, 3, 4];

				await That(sum).IsEqualTo(expectedValue);
			}

			[Fact]
			public async Task OnlyOnce_ShouldDeactivateCallbackAfterFirstExecution()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.OnGet(() => { callCount1++; })
					.OnGet((p1, _, _, _) => { callCount2 += p1; }).OnlyOnce();

				_ = sut[1, 2, 3, 4];
				_ = sut[2, 2, 3, 4];
				_ = sut[3, 2, 3, 4];
				_ = sut[4, 2, 3, 4];

				await That(callCount1).IsEqualTo(3);
				await That(callCount2).IsEqualTo(2);
			}

			[Fact]
			public async Task ShouldExecuteGetterCallbacks()
			{
				int callCount = 0;
				IIndexerService mock = Mock.Create<IIndexerService>();
				mock.SetupMock.Indexer(It.Is<int>(i => i < 5), It.Is<int>(i => i < 5),
						It.Is<int>(i => i < 5), It.Is<int>(i => i < 5))
					.OnGet(() => { callCount++; });

				_ = mock[1, 1, 5, 1]; // no
				_ = mock[3, 1, 2, 4]; // yes
				_ = mock[4, 2, 2, 3]; // yes
				_ = mock[1, 1, 1, 5]; // no
				_ = mock[1, 5, 1, 1]; // no
				_ = mock[1, 1, 1, -4]; // yes
				_ = mock[6, 2, 1, 3]; // no
				_ = mock[6, 7, 8, 9]; // no

				await That(callCount).IsEqualTo(3);
			}

			[Fact]
			public async Task ShouldExecuteGetterCallbacksWithValue()
			{
				int callCount = 0;
				IIndexerService mock = Mock.Create<IIndexerService>();
				mock.SetupMock.Indexer(It.Is<int>(i => i < 5), It.Is<int>(i => i < 5),
						It.Is<int>(i => i < 5), It.Is<int>(i => i < 5))
					.OnGet((v1, v2, v3, v4) => { callCount += v1 * v2 * v3 * v4; });

				_ = mock[1, 5, 1, 3]; // no
				_ = mock[3, 1, 2, 4]; // yes (24)
				_ = mock[2, 2, 3, 1]; // yes (12)
				_ = mock[1, 1, 5, 3]; // no
				_ = mock[1, 1, -4, 2]; // yes (-8)
				_ = mock[6, 2, 1, 3]; // no
				_ = mock[6, 7, 8, 9]; // no

				await That(callCount).IsEqualTo(28);
			}

			[Fact]
			public async Task ShouldInvokeCallbacksInSequence()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.OnGet(() => { callCount1++; })
					.OnGet((p1, _, _, _) => { callCount2 += p1; });

				_ = sut[1, 2, 3, 4];
				_ = sut[2, 2, 3, 4];
				_ = sut[3, 2, 3, 4];
				_ = sut[4, 2, 3, 4];

				await That(callCount1).IsEqualTo(2);
				await That(callCount2).IsEqualTo(6);
			}

			[Fact]
			public async Task When_ShouldOnlyExecuteCallbackWhenInvocationCountMatches()
			{
				List<int> invocations = [];
				IIndexerService sut = Mock.Create<IIndexerService>();
				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.OnGet((i, _, _, _, _) => { invocations.Add(i); })
					.When(x => x is > 3 and < 9);

				for (int i = 0; i < 20; i++)
				{
					_ = sut[i, 2 * i, 3 * i, 4 * i];
				}

				await That(invocations).IsEqualTo([4, 5, 6, 7, 8,]);
			}
		}

		public sealed class With5Levels
		{
			[Fact]
			public async Task Callback_WhenLengthDoesNotMatch_ShouldIgnore()
			{
				int callCount = 0;
				IIndexerService mock = Mock.Create<IIndexerService>();
				mock.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.OnGet(() => { callCount++; });

				_ = mock[1];
				_ = mock[2, 2];
				_ = mock[3, 3, 3];
				_ = mock[4, 4, 4, 4];
				_ = mock[5, 5, 5, 5, 5];

				await That(callCount).IsEqualTo(1);
			}

			[Fact]
			public async Task For_ShouldStopExecutingCallbackAfterTheGivenTimes()
			{
				List<int> invocations = [];
				IIndexerService sut = Mock.Create<IIndexerService>();
				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.OnGet((i, _, _, _, _, _) => { invocations.Add(i); })
					.For(4);

				for (int i = 0; i < 20; i++)
				{
					_ = sut[i, 2 * i, 3 * i, 4 * i, 5 * i];
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
					.OnGet((i, _, _, _, _, _) => { invocations.Add(i); })
					.When(x => x > 2)
					.For(4);

				for (int i = 0; i < 20; i++)
				{
					_ = sut[i, 2 * i, 3 * i, 4 * i, 5 * i];
				}

				await That(invocations).IsEqualTo([3, 4, 5, 6,]);
			}

			[Fact]
			public async Task InParallel_ShouldInvokeParallelCallbacksAlways()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				int callCount3 = 0;
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.OnGet(() => { callCount1++; })
					.OnGet((p1, _, _, _, _) => { callCount2 += p1; }).InParallel()
					.OnGet((p1, _, _, _, _) => { callCount3 += p1; });

				_ = sut[1, 2, 3, 4, 5];
				_ = sut[2, 2, 3, 4, 5];
				_ = sut[3, 2, 3, 4, 5];
				_ = sut[4, 2, 3, 4, 5];

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
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.OnGet((p1, _, _, _, _) => { sum += p1; }).Only(times);

				_ = sut[1, 2, 3, 4, 5];
				_ = sut[2, 2, 3, 4, 5];
				_ = sut[3, 2, 3, 4, 5];
				_ = sut[4, 2, 3, 4, 5];

				await That(sum).IsEqualTo(expectedValue);
			}

			[Fact]
			public async Task OnlyOnce_ShouldDeactivateCallbackAfterFirstExecution()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.OnGet(() => { callCount1++; })
					.OnGet((p1, _, _, _, _) => { callCount2 += p1; }).OnlyOnce();

				_ = sut[1, 2, 3, 4, 5];
				_ = sut[2, 2, 3, 4, 5];
				_ = sut[3, 2, 3, 4, 5];
				_ = sut[4, 2, 3, 4, 5];

				await That(callCount1).IsEqualTo(3);
				await That(callCount2).IsEqualTo(2);
			}

			[Fact]
			public async Task ShouldExecuteGetterCallbacks()
			{
				int callCount = 0;
				IIndexerService mock = Mock.Create<IIndexerService>();
				mock.SetupMock.Indexer(It.Is<int>(i => i < 6), It.Is<int>(i => i < 6),
						It.Is<int>(i => i < 6), It.Is<int>(i => i < 6), It.Is<int>(i => i < 6))
					.OnGet(() => { callCount++; });

				_ = mock[1, 1, 1, 6, 1]; // no
				_ = mock[1, 3, 1, 2, 4]; // yes
				_ = mock[2, 4, 2, 1, 3]; // yes
				_ = mock[1, 1, 1, 1, 6]; // no
				_ = mock[1, 1, 6, 1, 1]; // no
				_ = mock[1, 1, 1, 1, -4]; // yes
				_ = mock[1, 6, 2, 1, 3]; // no
				_ = mock[6, 7, 8, 9, 10]; // no

				await That(callCount).IsEqualTo(3);
			}

			[Fact]
			public async Task ShouldExecuteGetterCallbacksWithValue()
			{
				int callCount = 0;
				IIndexerService mock = Mock.Create<IIndexerService>();
				mock.SetupMock.Indexer(It.Is<int>(i => i < 6), It.Is<int>(i => i < 6),
						It.Is<int>(i => i < 6), It.Is<int>(i => i < 6), It.Is<int>(i => i < 6))
					.OnGet((v1, v2, v3, v4, v5) => { callCount += v1 * v2 * v3 * v4 * v5; });

				_ = mock[1, 1, 1, 7, 1]; // no
				_ = mock[1, 3, 1, 2, 4]; // yes (24)
				_ = mock[2, 4, 2, 1, 3]; // yes (48)
				_ = mock[1, 1, 1, 1, 7]; // no
				_ = mock[1, 1, 7, 1, 1]; // no
				_ = mock[1, 3, 3, 1, -4]; // yes (-36)
				_ = mock[1, 7, 2, 1, 3]; // no
				_ = mock[6, 7, 8, 9, 10]; // no

				await That(callCount).IsEqualTo(36);
			}

			[Fact]
			public async Task ShouldInvokeCallbacksInSequence()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.OnGet(() => { callCount1++; })
					.OnGet((p1, _, _, _, _) => { callCount2 += p1; });

				_ = sut[1, 2, 3, 4, 5];
				_ = sut[2, 2, 3, 4, 5];
				_ = sut[3, 2, 3, 4, 5];
				_ = sut[4, 2, 3, 4, 5];

				await That(callCount1).IsEqualTo(2);
				await That(callCount2).IsEqualTo(6);
			}

			[Fact]
			public async Task When_ShouldOnlyExecuteCallbackWhenInvocationCountMatches()
			{
				List<int> invocations = [];
				IIndexerService sut = Mock.Create<IIndexerService>();
				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.OnGet((i, _, _, _, _, _) => { invocations.Add(i); })
					.When(x => x is > 3 and < 9);

				for (int i = 0; i < 20; i++)
				{
					_ = sut[i, 2 * i, 3 * i, 4 * i, 5 * i];
				}

				await That(invocations).IsEqualTo([4, 5, 6, 7, 8,]);
			}
		}
	}
}
