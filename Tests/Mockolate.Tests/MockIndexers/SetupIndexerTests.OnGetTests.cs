using System.Collections.Generic;
using Mockolate.Interactions;
using Mockolate.Parameters;
using Mockolate.Setup;

namespace Mockolate.Tests.MockIndexers;

public sealed partial class SetupIndexerTests
{
	public sealed class OnGetTests
	{
		[Fact]
		public async Task ExecuteGetterCallback_WhenGenericTypeDoesNotMatch_ShouldNotExecute()
		{
			int callCount = 0;
			MyIndexerSetup<int> indexerSetup = new();
			indexerSetup.OnGet.Do(() => { callCount++; });
			IndexerGetterAccess<string> access = new("p1", "mismatch");

			long result = indexerSetup.DoGetResult(access, 2L);

			await That(result).IsEqualTo(2L);
			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task ExecuteGetterCallback_WhenNumberOfParametersDoesNotMatch_ShouldNotExecute()
		{
			int callCount = 0;
			MyIndexerSetup<int> indexerSetup = new();
			indexerSetup.OnGet.Do(() => { callCount++; });
			IndexerGetterAccess<int, int> access = new("p1", 1, "p2", 1);

			string result = indexerSetup.DoGetResult(access, "foo");

			await That(result).IsEqualTo("foo");
			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task ExecuteGetterCallback_WhenParameterTypeDoesNotMatch_ShouldNotExecute()
		{
			int callCount = 0;
			MyIndexerSetup<int> indexerSetup = new();
			indexerSetup.OnGet.Do(() => { callCount++; });
			IndexerGetterAccess<string> access = new("p1", "expect-int");

			string result = indexerSetup.DoGetResult(access, "foo");

			await That(result).IsEqualTo("foo");
			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task ExecuteGetterCallback_WhenTypesAndNumberMatch_ShouldExecute()
		{
			int callCount = 0;
			MyIndexerSetup<int> indexerSetup = new();
			indexerSetup.OnGet.Do(() => { callCount++; });
			IndexerGetterAccess<int> access = new("p1", 1);

			string result = indexerSetup.DoGetResult(access, "foo");

			await That(callCount).IsEqualTo(1);
		}

		[Fact]
		public async Task ExecuteSetterCallback_WhenGenericTypeDoesNotMatch_ShouldNotExecute()
		{
			int callCount = 0;
			MyIndexerSetup<int> indexerSetup = new();
			indexerSetup.OnSet.Do(() => { callCount++; });
			IndexerSetterAccess<string, string> access = new("p1", "mismatch", "bar");

			indexerSetup.DoSetResult(access, 2L);

			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task ExecuteSetterCallback_WhenNumberOfParametersDoesNotMatch_ShouldNotExecute()
		{
			int callCount = 0;
			MyIndexerSetup<int> indexerSetup = new();
			indexerSetup.OnSet.Do(() => { callCount++; });
			IndexerSetterAccess<int, int, string> access = new("p1", 1, "p2", 1, "bar");

			indexerSetup.DoSetResult(access, "foo");

			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task ExecuteSetterCallback_WhenParameterTypeDoesNotMatch_ShouldNotExecute()
		{
			int callCount = 0;
			MyIndexerSetup<int> indexerSetup = new();
			indexerSetup.OnSet.Do(() => { callCount++; });
			IndexerSetterAccess<string, string> access = new("p1", "expect-int", "bar");

			indexerSetup.DoSetResult(access, "foo");

			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task ExecuteSetterCallback_WhenTypesAndNumberMatch_ShouldExecute()
		{
			int callCount = 0;
			MyIndexerSetup<int> indexerSetup = new();
			indexerSetup.OnSet.Do(() => { callCount++; });
			IndexerSetterAccess<int, string> access = new("p1", 1, "bar");

			indexerSetup.DoSetResult(access, "foo");

			await That(callCount).IsEqualTo(1);
		}

		[Fact]
		public async Task InParallel_ShouldInvokeParallelCallbacksAlways()
		{
			int callCount1 = 0;
			int callCount2 = 0;
			int callCount3 = 0;
			IIndexerService sut = IIndexerService.CreateMock();

			sut.Mock.Setup[It.IsAny<int>()]
				.OnGet.Do(() => { callCount1++; })
				.OnGet.Do(v => { callCount2 += v; }).InParallel()
				.OnGet.Do(v => { callCount3 += v; });

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
			IIndexerService sut = IIndexerService.CreateMock();

			sut.Mock.Setup[It.IsAny<int>()]
				.OnGet.Do(v => { sum += v; }).Only(times);

			_ = sut[1];
			_ = sut[2];
			_ = sut[3];
			_ = sut[4];

			await That(sum).IsEqualTo(expectedValue);
		}

		[Fact]
		public async Task Only_ShouldStopExecutingCallbackAfterTheGivenTimes()
		{
			List<int> invocations = [];
			IIndexerService sut = IIndexerService.CreateMock();
			sut.Mock.Setup[It.IsAny<int>()]
				.OnGet.Do((i, _) => { invocations.Add(i); })
				.Only(4);

			for (int i = 0; i < 20; i++)
			{
				_ = sut[i];
			}

			await That(invocations).IsEqualTo([0, 1, 2, 3,]);
		}

		[Fact]
		public async Task Only_WithWhen_ShouldStopExecutingCallbackAfterTheGivenTimes()
		{
			List<int> invocations = [];
			IIndexerService sut = IIndexerService.CreateMock();
			sut.Mock.Setup[It.IsAny<int>()]
				.OnGet.Do((i, _) => { invocations.Add(i); })
				.When(x => x > 2)
				.Only(4);

			for (int i = 0; i < 20; i++)
			{
				_ = sut[i];
			}

			await That(invocations).IsEqualTo([3, 4, 5, 6,]);
		}

		[Fact]
		public async Task OnlyOnce_ShouldDeactivateCallbackAfterFirstExecution()
		{
			int callCount1 = 0;
			int callCount2 = 0;
			IIndexerService sut = IIndexerService.CreateMock();

			sut.Mock.Setup[It.IsAny<int>()]
				.OnGet.Do(() => { callCount1++; })
				.OnGet.Do(v => { callCount2 += v; }).OnlyOnce();

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
			IIndexerService sut = IIndexerService.CreateMock();
			sut.Mock.Setup[It.Satisfies<int>(i => i < 4)]
				.OnGet.Do(() => { callCount++; });

			_ = sut[1];
			_ = sut[2];
			_ = sut[3];
			_ = sut[4];
			_ = sut[5];
			_ = sut[6];

			await That(callCount).IsEqualTo(3);
		}

		[Fact]
		public async Task ShouldExecuteGetterCallbacksWithValue()
		{
			int callCount = 0;
			IIndexerService sut = IIndexerService.CreateMock();
			sut.Mock.Setup[It.Satisfies<int>(i => i < 4)]
				.OnGet.Do(v => { callCount += v; });

			_ = sut[1];
			_ = sut[2];
			_ = sut[3];
			_ = sut[4];
			_ = sut[5];

			await That(callCount).IsEqualTo(6);
		}

		[Fact]
		public async Task ShouldIncludeIncrementingAccessCounter()
		{
			List<int> invocations = [];
			IIndexerService sut = IIndexerService.CreateMock();
			sut.Mock.Setup[It.IsAny<int>()]
				.OnGet.Do((i, _, _) => { invocations.Add(i); });

			for (int i = 0; i < 10; i++)
			{
				_ = sut[10 * i];
			}

			await That(invocations).IsEqualTo([0, 1, 2, 3, 4, 5, 6, 7, 8, 9,]);
		}

		[Fact]
		public async Task ShouldInvokeCallbacksInSequence()
		{
			int callCount1 = 0;
			int callCount2 = 0;
			IIndexerService sut = IIndexerService.CreateMock();

			sut.Mock.Setup[It.IsAny<int>()]
				.OnGet.Do(() => { callCount1++; })
				.OnGet.Do(v => { callCount2 += v; });

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
			IIndexerService sut = IIndexerService.CreateMock();
			sut.Mock.Setup[It.IsAny<int>()]
				.OnGet.Do((i, _) => { invocations.Add(i); })
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
			IIndexerService sut = IIndexerService.CreateMock();
			sut.Mock.Setup[It.IsAny<int>()]
				.OnGet.Do(() => { callCount++; });

			_ = sut[1];
			_ = sut[2, 2];
			_ = sut[3, 3, 3];

			await That(callCount).IsEqualTo(1);
		}

		[Fact]
		public async Task WithoutCallback_IIndexerSetupCallbackBuilder_ShouldNotThrow()
		{
			IIndexerService sut = IIndexerService.CreateMock();
			IIndexerSetupCallbackBuilder<string, int> setup =
				sut.Mock.Setup[It.IsAny<int>()];

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
		public async Task WithoutCallback_IIndexerSetupCallbackWhenBuilder_ShouldNotThrow()
		{
			IIndexerService sut = IIndexerService.CreateMock();
			IIndexerSetupCallbackWhenBuilder<string, int> setup =
				sut.Mock.Setup[It.IsAny<int>()];

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

		private sealed class MyIndexerSetup<T1>()
			: IndexerSetup<string, T1>(
				(IParameterMatch<T1>)It.IsAny<T1>())
		{
			private readonly ValueStorage _storage = new();

			public T DoGetResult<T>(
				IndexerAccess indexerAccess, T value, MockBehavior? behavior = null)
				=> GetResult(indexerAccess, behavior ?? MockBehavior.Default, _storage, value);

			public void DoSetResult<T>(
				IndexerAccess indexerAccess, T value, MockBehavior? behavior = null)
				=> SetResult(indexerAccess, behavior ?? MockBehavior.Default, _storage, value);
		}

		public sealed class With2Levels
		{
			[Fact]
			public async Task Callback_WhenLengthDoesNotMatch_ShouldIgnore()
			{
				int callCount = 0;
				IIndexerService sut = IIndexerService.CreateMock();
				sut.Mock.Setup[It.IsAny<int>(), It.IsAny<int>()]
					.OnGet.Do(() => { callCount++; });

				_ = sut[1];
				_ = sut[2, 2];
				_ = sut[3, 3, 3];

				await That(callCount).IsEqualTo(1);
			}

			[Fact]
			public async Task ExecuteGetterCallback_WhenGenericTypeDoesNotMatch_ShouldNotExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int> indexerSetup = new();
				indexerSetup.OnGet.Do(() => { callCount++; });
				IndexerGetterAccess<string, string> access = new("p1", "a", "p2", "b");

				long result = indexerSetup.DoGetResult(access, 2L);

				await That(result).IsEqualTo(2L);
				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task ExecuteGetterCallback_WhenNumberOfParametersDoesNotMatch_ShouldNotExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int> indexerSetup = new();
				indexerSetup.OnGet.Do(() => { callCount++; });
				IndexerGetterAccess<int, int, int> access = new("p1", 1, "p2", 2, "p3", 3);

				string result = indexerSetup.DoGetResult(access, "foo");

				await That(result).IsEqualTo("foo");
				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task ExecuteGetterCallback_WhenParameterTypeDoesNotMatch_ShouldNotExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int> indexerSetup = new();
				indexerSetup.OnGet.Do(() => { callCount++; });
				IndexerGetterAccess<int, string> access = new("p1", 1, "p2", "expect-int");

				string result = indexerSetup.DoGetResult(access, "foo");

				await That(result).IsEqualTo("foo");
				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task ExecuteGetterCallback_WhenTypesAndNumberMatch_ShouldExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int> indexerSetup = new();
				indexerSetup.OnGet.Do(() => { callCount++; });
				IndexerGetterAccess<int, int> access = new("p1", 1, "p2", 2);

				string result = indexerSetup.DoGetResult(access, "foo");

				await That(callCount).IsEqualTo(1);
			}

			[Fact]
			public async Task ExecuteSetterCallback_WhenGenericTypeDoesNotMatch_ShouldNotExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int> indexerSetup = new();
				indexerSetup.OnSet.Do(() => { callCount++; });
				IndexerSetterAccess<int, int, int> access = new("p1", 1, "p2", 2, 99);

				indexerSetup.DoSetResult(access, 2L);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task ExecuteSetterCallback_WhenNumberOfParametersDoesNotMatch_ShouldNotExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int> indexerSetup = new();
				indexerSetup.OnSet.Do(() => { callCount++; });
				IndexerSetterAccess<int, int, int, string> access = new("p1", 1, "p2", 2, "p3", 3, "bar");

				indexerSetup.DoSetResult(access, "foo");

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task ExecuteSetterCallback_WhenParameterTypeDoesNotMatch_ShouldNotExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int> indexerSetup = new();
				indexerSetup.OnSet.Do(() => { callCount++; });
				IndexerSetterAccess<int, string, string> access = new("p1", 1, "p2", "expect-int", "bar");

				indexerSetup.DoSetResult(access, "foo");

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task ExecuteSetterCallback_WhenTypesAndNumberMatch_ShouldExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int> indexerSetup = new();
				indexerSetup.OnSet.Do(() => { callCount++; });
				IndexerSetterAccess<int, int, string> access = new("p1", 1, "p2", 2, "bar");

				indexerSetup.DoSetResult(access, "foo");

				await That(callCount).IsEqualTo(1);
			}

			[Fact]
			public async Task InParallel_ShouldInvokeParallelCallbacksAlways()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				int callCount3 = 0;
				IIndexerService sut = IIndexerService.CreateMock();

				sut.Mock.Setup[It.IsAny<int>(), It.IsAny<int>()]
					.OnGet.Do(() => { callCount1++; })
					.OnGet.Do((p1, _) => { callCount2 += p1; }).InParallel()
					.OnGet.Do((p1, _) => { callCount3 += p1; });

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
				IIndexerService sut = IIndexerService.CreateMock();

				sut.Mock.Setup[It.IsAny<int>(), It.IsAny<int>()]
					.OnGet.Do((p1, _) => { sum += p1; }).Only(times);

				_ = sut[1, 2];
				_ = sut[2, 2];
				_ = sut[3, 2];
				_ = sut[4, 2];

				await That(sum).IsEqualTo(expectedValue);
			}

			[Fact]
			public async Task Only_ShouldStopExecutingCallbackAfterTheGivenTimes()
			{
				List<int> invocations = [];
				IIndexerService sut = IIndexerService.CreateMock();
				sut.Mock.Setup[It.IsAny<int>(), It.IsAny<int>()]
					.OnGet.Do((i, _, _) => { invocations.Add(i); })
					.Only(4);

				for (int i = 0; i < 20; i++)
				{
					_ = sut[i, 2 * i];
				}

				await That(invocations).IsEqualTo([0, 1, 2, 3,]);
			}

			[Fact]
			public async Task Only_WithWhen_ShouldStopExecutingCallbackAfterTheGivenTimes()
			{
				List<int> invocations = [];
				IIndexerService sut = IIndexerService.CreateMock();
				sut.Mock.Setup[It.IsAny<int>(), It.IsAny<int>()]
					.OnGet.Do((i, _, _) => { invocations.Add(i); })
					.When(x => x > 2)
					.Only(4);

				for (int i = 0; i < 20; i++)
				{
					_ = sut[i, 2 * i];
				}

				await That(invocations).IsEqualTo([3, 4, 5, 6,]);
			}

			[Fact]
			public async Task OnlyOnce_ShouldDeactivateCallbackAfterFirstExecution()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				IIndexerService sut = IIndexerService.CreateMock();

				sut.Mock.Setup[It.IsAny<int>(), It.IsAny<int>()]
					.OnGet.Do(() => { callCount1++; })
					.OnGet.Do((p1, _) => { callCount2 += p1; }).OnlyOnce();

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
				IIndexerService sut = IIndexerService.CreateMock();
				sut.Mock.Setup[It.Satisfies<int>(i => i < 4), It.Satisfies<int>(i => i < 4)]
					.OnGet.Do(() => { callCount++; });

				_ = sut[5, 1]; // no
				_ = sut[3, 2]; // yes
				_ = sut[2, 3]; // yes
				_ = sut[1, 4]; // no
				_ = sut[1, -4]; // yes
				_ = sut[8, 6]; // no

				await That(callCount).IsEqualTo(3);
			}

			[Fact]
			public async Task ShouldExecuteGetterCallbacksWithValue()
			{
				int callCount = 0;
				IIndexerService sut = IIndexerService.CreateMock();
				sut.Mock.Setup[It.Satisfies<int>(i => i < 4), It.Satisfies<int>(i => i < 4)]
					.OnGet.Do((v1, v2) => { callCount += v1 * v2; });

				_ = sut[5, 1]; // no
				_ = sut[3, 2]; // yes (6)
				_ = sut[2, 3]; // yes (6)
				_ = sut[1, 4]; // no
				_ = sut[1, -4]; // yes (-4)
				_ = sut[8, 6]; // no

				await That(callCount).IsEqualTo(8);
			}

			[Fact]
			public async Task ShouldIncludeIncrementingAccessCounter()
			{
				List<int> invocations = [];
				IIndexerService sut = IIndexerService.CreateMock();
				sut.Mock.Setup[It.IsAny<int>(), It.IsAny<int>()]
					.OnGet.Do((i, _, _, _) => { invocations.Add(i); });

				for (int i = 0; i < 10; i++)
				{
					_ = sut[10 * i, 20 * i];
				}

				await That(invocations).IsEqualTo([0, 1, 2, 3, 4, 5, 6, 7, 8, 9,]);
			}

			[Fact]
			public async Task ShouldInvokeCallbacksInSequence()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				IIndexerService sut = IIndexerService.CreateMock();

				sut.Mock.Setup[It.IsAny<int>(), It.IsAny<int>()]
					.OnGet.Do(() => { callCount1++; })
					.OnGet.Do((p1, _) => { callCount2 += p1; });

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
				IIndexerService sut = IIndexerService.CreateMock();
				sut.Mock.Setup[It.IsAny<int>(), It.IsAny<int>()]
					.OnGet.Do((i, _, _) => { invocations.Add(i); })
					.When(x => x is > 3 and < 9);

				for (int i = 0; i < 20; i++)
				{
					_ = sut[i, 2 * i];
				}

				await That(invocations).IsEqualTo([4, 5, 6, 7, 8,]);
			}

			[Fact]
			public async Task WithoutCallback_IIndexerSetupCallbackBuilder_ShouldNotThrow()
			{
				IIndexerService sut = IIndexerService.CreateMock();
				IIndexerSetupCallbackBuilder<string, int, int> setup =
					sut.Mock.Setup[It.IsAny<int>(),
						It.IsAny<int>()];

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
			public async Task WithoutCallback_IIndexerSetupCallbackWhenBuilder_ShouldNotThrow()
			{
				IIndexerService sut = IIndexerService.CreateMock();
				IIndexerSetupCallbackWhenBuilder<string, int, int> setup =
					sut.Mock.Setup[It.IsAny<int>(), It.IsAny<int>()];

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

			private sealed class MyIndexerSetup<T1, T2>()
				: IndexerSetup<string, T1, T2>(
					(IParameterMatch<T1>)It.IsAny<T1>(),
					(IParameterMatch<T2>)It.IsAny<T2>())
			{
				private readonly ValueStorage _storage = new();

				public T DoGetResult<T>(
					IndexerAccess indexerAccess, T value, MockBehavior? behavior = null)
					=> GetResult(indexerAccess, behavior ?? MockBehavior.Default, _storage, value);

				public void DoSetResult<T>(
					IndexerAccess indexerAccess, T value, MockBehavior? behavior = null)
					=> SetResult(indexerAccess, behavior ?? MockBehavior.Default, _storage, value);
			}
		}

		public sealed class With3Levels
		{
			[Fact]
			public async Task Callback_WhenLengthDoesNotMatch_ShouldIgnore()
			{
				int callCount = 0;
				IIndexerService sut = IIndexerService.CreateMock();
				sut.Mock.Setup[It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()]
					.OnGet.Do(() => { callCount++; });

				_ = sut[1];
				_ = sut[2, 2];
				_ = sut[3, 3, 3];
				_ = sut[4, 4, 4, 4];

				await That(callCount).IsEqualTo(1);
			}

			[Fact]
			public async Task ExecuteGetterCallback_WhenGenericTypeDoesNotMatch_ShouldNotExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int, int> indexerSetup = new();
				indexerSetup.OnGet.Do(() => { callCount++; });
				IndexerGetterAccess<string, string, string> access = new("p1", "a", "p2", "b", "p3", "c");

				long result = indexerSetup.DoGetResult(access, 2L);

				await That(result).IsEqualTo(2L);
				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task ExecuteGetterCallback_WhenNumberOfParametersDoesNotMatch_ShouldNotExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int, int> indexerSetup = new();
				indexerSetup.OnGet.Do(() => { callCount++; });
				IndexerGetterAccess<int, int, int, int> access = new("p1", 1, "p2", 2, "p3", 3, "p4", 4);

				string result = indexerSetup.DoGetResult(access, "foo");

				await That(result).IsEqualTo("foo");
				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task ExecuteGetterCallback_WhenParameterTypeDoesNotMatch_ShouldNotExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int, int> indexerSetup = new();
				indexerSetup.OnGet.Do(() => { callCount++; });
				IndexerGetterAccess<int, int, string> access = new("p1", 1, "p2", 2, "p3", "expect-int");

				string result = indexerSetup.DoGetResult(access, "foo");

				await That(result).IsEqualTo("foo");
				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task ExecuteGetterCallback_WhenTypesAndNumberMatch_ShouldExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int, int> indexerSetup = new();
				indexerSetup.OnGet.Do(() => { callCount++; });
				IndexerGetterAccess<int, int, int> access = new("p1", 1, "p2", 2, "p3", 3);

				string result = indexerSetup.DoGetResult(access, "foo");

				await That(callCount).IsEqualTo(1);
			}

			[Fact]
			public async Task ExecuteSetterCallback_WhenGenericTypeDoesNotMatch_ShouldNotExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int, int> indexerSetup = new();
				indexerSetup.OnSet.Do(() => { callCount++; });
				IndexerSetterAccess<int, int, int, int> access = new("p1", 1, "p2", 2, "p3", 3, 99);

				indexerSetup.DoSetResult(access, 2L);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task ExecuteSetterCallback_WhenNumberOfParametersDoesNotMatch_ShouldNotExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int, int> indexerSetup = new();
				indexerSetup.OnSet.Do(() => { callCount++; });
				IndexerSetterAccess<int, int, int, int, string> access = new("p1", 1, "p2", 2, "p3", 3, "p4", 4, "bar");

				indexerSetup.DoSetResult(access, "foo");

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task ExecuteSetterCallback_WhenParameterTypeDoesNotMatch_ShouldNotExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int, int> indexerSetup = new();
				indexerSetup.OnSet.Do(() => { callCount++; });
				IndexerSetterAccess<int, int, string, string> access = new("p1", 1, "p2", 2, "p3", "expect-int", "bar");

				indexerSetup.DoSetResult(access, "foo");

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task ExecuteSetterCallback_WhenTypesAndNumberMatch_ShouldExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int, int> indexerSetup = new();
				indexerSetup.OnSet.Do(() => { callCount++; });
				IndexerSetterAccess<int, int, int, string> access = new("p1", 1, "p2", 2, "p3", 3, "bar");

				indexerSetup.DoSetResult(access, "foo");

				await That(callCount).IsEqualTo(1);
			}

			[Fact]
			public async Task InParallel_ShouldInvokeParallelCallbacksAlways()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				int callCount3 = 0;
				IIndexerService sut = IIndexerService.CreateMock();

				sut.Mock.Setup[It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()]
					.OnGet.Do(() => { callCount1++; })
					.OnGet.Do((p1, _, _) => { callCount2 += p1; }).InParallel()
					.OnGet.Do((p1, _, _) => { callCount3 += p1; });

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
				IIndexerService sut = IIndexerService.CreateMock();

				sut.Mock.Setup[It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()]
					.OnGet.Do((p1, _, _) => { sum += p1; }).Only(times);

				_ = sut[1, 2, 3];
				_ = sut[2, 2, 3];
				_ = sut[3, 2, 3];
				_ = sut[4, 2, 3];

				await That(sum).IsEqualTo(expectedValue);
			}

			[Fact]
			public async Task Only_ShouldStopExecutingCallbackAfterTheGivenTimes()
			{
				List<int> invocations = [];
				IIndexerService sut = IIndexerService.CreateMock();
				sut.Mock.Setup[It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()]
					.OnGet.Do((i, _, _, _) => { invocations.Add(i); })
					.Only(4);

				for (int i = 0; i < 20; i++)
				{
					_ = sut[i, 2 * i, 3 * i];
				}

				await That(invocations).IsEqualTo([0, 1, 2, 3,]);
			}

			[Fact]
			public async Task Only_WithWhen_ShouldStopExecutingCallbackAfterTheGivenTimes()
			{
				List<int> invocations = [];
				IIndexerService sut = IIndexerService.CreateMock();
				sut.Mock.Setup[It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()]
					.OnGet.Do((i, _, _, _) => { invocations.Add(i); })
					.When(x => x > 2)
					.Only(4);

				for (int i = 0; i < 20; i++)
				{
					_ = sut[i, 2 * i, 3 * i];
				}

				await That(invocations).IsEqualTo([3, 4, 5, 6,]);
			}

			[Fact]
			public async Task OnlyOnce_ShouldDeactivateCallbackAfterFirstExecution()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				IIndexerService sut = IIndexerService.CreateMock();

				sut.Mock.Setup[It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()]
					.OnGet.Do(() => { callCount1++; })
					.OnGet.Do((p1, _, _) => { callCount2 += p1; }).OnlyOnce();

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
				IIndexerService sut = IIndexerService.CreateMock();
				sut.Mock.Setup[It.Satisfies<int>(i => i < 4), It.Satisfies<int>(i => i < 4),
						It.Satisfies<int>(i => i < 4)]
					.OnGet.Do(() => { callCount++; });

				_ = sut[1, 5, 1]; // no
				_ = sut[3, 1, 2]; // yes
				_ = sut[2, 2, 3]; // yes
				_ = sut[1, 1, 4]; // no
				_ = sut[1, 1, -4]; // yes
				_ = sut[6, 2, 1]; // no
				_ = sut[6, 7, 8]; // no

				await That(callCount).IsEqualTo(3);
			}

			[Fact]
			public async Task ShouldExecuteGetterCallbacksWithValue()
			{
				int callCount = 0;
				IIndexerService sut = IIndexerService.CreateMock();
				sut.Mock.Setup[It.Satisfies<int>(i => i < 4), It.Satisfies<int>(i => i < 4),
						It.Satisfies<int>(i => i < 4)]
					.OnGet.Do((v1, v2, v3) => { callCount += v1 * v2 * v3; });

				_ = sut[1, 5, 1]; // no
				_ = sut[3, 1, 2]; // yes (6)
				_ = sut[2, 2, 3]; // yes (12)
				_ = sut[1, 1, 4]; // no
				_ = sut[1, 1, -4]; // yes (-4)
				_ = sut[6, 2, 1]; // no
				_ = sut[6, 7, 8]; // no

				await That(callCount).IsEqualTo(14);
			}

			[Fact]
			public async Task ShouldIncludeIncrementingAccessCounter()
			{
				List<int> invocations = [];
				IIndexerService sut = IIndexerService.CreateMock();
				sut.Mock.Setup[It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()]
					.OnGet.Do((i, _, _, _, _) => { invocations.Add(i); });

				for (int i = 0; i < 10; i++)
				{
					_ = sut[10 * i, 20 * i, 30 * i];
				}

				await That(invocations).IsEqualTo([0, 1, 2, 3, 4, 5, 6, 7, 8, 9,]);
			}

			[Fact]
			public async Task ShouldInvokeCallbacksInSequence()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				IIndexerService sut = IIndexerService.CreateMock();

				sut.Mock.Setup[It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()]
					.OnGet.Do(() => { callCount1++; })
					.OnGet.Do((p1, _, _) => { callCount2 += p1; });

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
				IIndexerService sut = IIndexerService.CreateMock();
				sut.Mock.Setup[It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()]
					.OnGet.Do((i, _, _, _) => { invocations.Add(i); })
					.When(x => x is > 3 and < 9);

				for (int i = 0; i < 20; i++)
				{
					_ = sut[i, 2 * i, 3 * i];
				}

				await That(invocations).IsEqualTo([4, 5, 6, 7, 8,]);
			}

			[Fact]
			public async Task WithoutCallback_IIndexerSetupCallbackBuilder_ShouldNotThrow()
			{
				IIndexerService sut = IIndexerService.CreateMock();
				IIndexerSetupCallbackBuilder<string, int, int, int> setup =
					sut.Mock.Setup[It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()];

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
			public async Task WithoutCallback_IIndexerSetupCallbackWhenBuilder_ShouldNotThrow()
			{
				IIndexerService sut = IIndexerService.CreateMock();
				IIndexerSetupCallbackWhenBuilder<string, int, int, int> setup =
					sut.Mock.Setup[It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()];

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

			private sealed class MyIndexerSetup<T1, T2, T3>()
				: IndexerSetup<string, T1, T2, T3>(
					(IParameterMatch<T1>)It.IsAny<T1>(),
					(IParameterMatch<T2>)It.IsAny<T2>(),
					(IParameterMatch<T3>)It.IsAny<T3>())
			{
				private readonly ValueStorage _storage = new();

				public T DoGetResult<T>(
					IndexerAccess indexerAccess, T value, MockBehavior? behavior = null)
					=> GetResult(indexerAccess, behavior ?? MockBehavior.Default, _storage, value);

				public void DoSetResult<T>(
					IndexerAccess indexerAccess, T value, MockBehavior? behavior = null)
					=> SetResult(indexerAccess, behavior ?? MockBehavior.Default, _storage, value);
			}
		}

		public sealed class With4Levels
		{
			[Fact]
			public async Task Callback_WhenLengthDoesNotMatch_ShouldIgnore()
			{
				int callCount = 0;
				IIndexerService sut = IIndexerService.CreateMock();
				sut.Mock.Setup[It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()]
					.OnGet.Do(() => { callCount++; });

				_ = sut[1];
				_ = sut[2, 2];
				_ = sut[3, 3, 3];
				_ = sut[4, 4, 4, 4];

				await That(callCount).IsEqualTo(1);
			}

			[Fact]
			public async Task ExecuteGetterCallback_WhenGenericTypeDoesNotMatch_ShouldNotExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int, int, int> indexerSetup = new();
				indexerSetup.OnGet.Do(() => { callCount++; });
				IndexerGetterAccess<string, string, string, string> access =
					new("p1", "a", "p2", "b", "p3", "c", "p4", "d");

				long result = indexerSetup.DoGetResult(access, 2L);

				await That(result).IsEqualTo(2L);
				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task ExecuteGetterCallback_WhenNumberOfParametersDoesNotMatch_ShouldNotExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int, int, int> indexerSetup = new();
				indexerSetup.OnGet.Do(() => { callCount++; });
				IndexerGetterAccess<int, int, int> access = new("p1", 1, "p2", 2, "p3", 3);

				string result = indexerSetup.DoGetResult(access, "foo");

				await That(result).IsEqualTo("foo");
				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task ExecuteGetterCallback_WhenParameterTypeDoesNotMatch_ShouldNotExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int, int, int> indexerSetup = new();
				indexerSetup.OnGet.Do(() => { callCount++; });
				IndexerGetterAccess<int, int, int, string> access =
					new("p1", 1, "p2", 2, "p3", 3, "p4", "expect-int");

				string result = indexerSetup.DoGetResult(access, "foo");

				await That(result).IsEqualTo("foo");
				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task ExecuteGetterCallback_WhenTypesAndNumberMatch_ShouldExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int, int, int> indexerSetup = new();
				indexerSetup.OnGet.Do(() => { callCount++; });
				IndexerGetterAccess<int, int, int, int> access =
					new("p1", 1, "p2", 2, "p3", 3, "p4", 4);

				string result = indexerSetup.DoGetResult(access, "foo");

				await That(callCount).IsEqualTo(1);
			}

			[Fact]
			public async Task ExecuteSetterCallback_WhenGenericTypeDoesNotMatch_ShouldNotExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int, int, int> indexerSetup = new();
				indexerSetup.OnSet.Do(() => { callCount++; });
				IndexerSetterAccess<int, int, int, int, int> access =
					new("p1", 1, "p2", 2, "p3", 3, "p4", 4, 99);

				indexerSetup.DoSetResult(access, 2L);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task ExecuteSetterCallback_WhenNumberOfParametersDoesNotMatch_ShouldNotExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int, int, int> indexerSetup = new();
				indexerSetup.OnSet.Do(() => { callCount++; });
				IndexerSetterAccess<int, int, int, string> access =
					new("p1", 1, "p2", 2, "p3", 3, "bar");

				indexerSetup.DoSetResult(access, "foo");

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task ExecuteSetterCallback_WhenParameterTypeDoesNotMatch_ShouldNotExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int, int, int> indexerSetup = new();
				indexerSetup.OnSet.Do(() => { callCount++; });
				IndexerSetterAccess<int, int, int, string, string> access =
					new("p1", 1, "p2", 2, "p3", 3, "p4", "expect-int", "bar");

				indexerSetup.DoSetResult(access, "foo");

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task ExecuteSetterCallback_WhenTypesAndNumberMatch_ShouldExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int, int, int> indexerSetup = new();
				indexerSetup.OnSet.Do(() => { callCount++; });
				IndexerSetterAccess<int, int, int, int, string> access =
					new("p1", 1, "p2", 2, "p3", 3, "p4", 4, "bar");

				indexerSetup.DoSetResult(access, "foo");

				await That(callCount).IsEqualTo(1);
			}

			[Fact]
			public async Task InParallel_ShouldInvokeParallelCallbacksAlways()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				int callCount3 = 0;
				IIndexerService sut = IIndexerService.CreateMock();

				sut.Mock.Setup[It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()]
					.OnGet.Do(() => { callCount1++; })
					.OnGet.Do((p1, _, _, _) => { callCount2 += p1; }).InParallel()
					.OnGet.Do((p1, _, _, _) => { callCount3 += p1; });

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
				IIndexerService sut = IIndexerService.CreateMock();

				sut.Mock.Setup[It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()]
					.OnGet.Do((p1, _, _, _) => { sum += p1; }).Only(times);

				_ = sut[1, 2, 3, 4];
				_ = sut[2, 2, 3, 4];
				_ = sut[3, 2, 3, 4];
				_ = sut[4, 2, 3, 4];

				await That(sum).IsEqualTo(expectedValue);
			}

			[Fact]
			public async Task Only_ShouldStopExecutingCallbackAfterTheGivenTimes()
			{
				List<int> invocations = [];
				IIndexerService sut = IIndexerService.CreateMock();
				sut.Mock.Setup[It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()]
					.OnGet.Do((i, _, _, _, _) => { invocations.Add(i); })
					.Only(4);

				for (int i = 0; i < 20; i++)
				{
					_ = sut[i, 2 * i, 3 * i, 4 * i];
				}

				await That(invocations).IsEqualTo([0, 1, 2, 3,]);
			}

			[Fact]
			public async Task Only_WithWhen_ShouldStopExecutingCallbackAfterTheGivenTimes()
			{
				List<int> invocations = [];
				IIndexerService sut = IIndexerService.CreateMock();
				sut.Mock.Setup[It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()]
					.OnGet.Do((i, _, _, _, _) => { invocations.Add(i); })
					.When(x => x > 2)
					.Only(4);

				for (int i = 0; i < 20; i++)
				{
					_ = sut[i, 2 * i, 3 * i, 4 * i];
				}

				await That(invocations).IsEqualTo([3, 4, 5, 6,]);
			}

			[Fact]
			public async Task OnlyOnce_ShouldDeactivateCallbackAfterFirstExecution()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				IIndexerService sut = IIndexerService.CreateMock();

				sut.Mock.Setup[It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()]
					.OnGet.Do(() => { callCount1++; })
					.OnGet.Do((p1, _, _, _) => { callCount2 += p1; }).OnlyOnce();

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
				IIndexerService sut = IIndexerService.CreateMock();
				sut.Mock.Setup[It.Satisfies<int>(i => i < 5), It.Satisfies<int>(i => i < 5),
						It.Satisfies<int>(i => i < 5), It.Satisfies<int>(i => i < 5)]
					.OnGet.Do(() => { callCount++; });

				_ = sut[1, 1, 5, 1]; // no
				_ = sut[3, 1, 2, 4]; // yes
				_ = sut[4, 2, 2, 3]; // yes
				_ = sut[1, 1, 1, 5]; // no
				_ = sut[1, 5, 1, 1]; // no
				_ = sut[1, 1, 1, -4]; // yes
				_ = sut[6, 2, 1, 3]; // no
				_ = sut[6, 7, 8, 9]; // no

				await That(callCount).IsEqualTo(3);
			}

			[Fact]
			public async Task ShouldExecuteGetterCallbacksWithValue()
			{
				int callCount = 0;
				IIndexerService sut = IIndexerService.CreateMock();
				sut.Mock.Setup[It.Satisfies<int>(i => i < 5), It.Satisfies<int>(i => i < 5),
						It.Satisfies<int>(i => i < 5), It.Satisfies<int>(i => i < 5)]
					.OnGet.Do((v1, v2, v3, v4) => { callCount += v1 * v2 * v3 * v4; });

				_ = sut[1, 5, 1, 3]; // no
				_ = sut[3, 1, 2, 4]; // yes (24)
				_ = sut[2, 2, 3, 1]; // yes (12)
				_ = sut[1, 1, 5, 3]; // no
				_ = sut[1, 1, -4, 2]; // yes (-8)
				_ = sut[6, 2, 1, 3]; // no
				_ = sut[6, 7, 8, 9]; // no

				await That(callCount).IsEqualTo(28);
			}

			[Fact]
			public async Task ShouldIncludeIncrementingAccessCounter()
			{
				List<int> invocations = [];
				IIndexerService sut = IIndexerService.CreateMock();
				sut.Mock.Setup[It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()]
					.OnGet.Do((i, _, _, _, _, _) => { invocations.Add(i); });

				for (int i = 0; i < 10; i++)
				{
					_ = sut[10 * i, 20 * i, 30 * i, 40 * i];
				}

				await That(invocations).IsEqualTo([0, 1, 2, 3, 4, 5, 6, 7, 8, 9,]);
			}

			[Fact]
			public async Task ShouldInvokeCallbacksInSequence()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				IIndexerService sut = IIndexerService.CreateMock();

				sut.Mock.Setup[It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()]
					.OnGet.Do(() => { callCount1++; })
					.OnGet.Do((p1, _, _, _) => { callCount2 += p1; });

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
				IIndexerService sut = IIndexerService.CreateMock();
				sut.Mock.Setup[It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()]
					.OnGet.Do((i, _, _, _, _) => { invocations.Add(i); })
					.When(x => x is > 3 and < 9);

				for (int i = 0; i < 20; i++)
				{
					_ = sut[i, 2 * i, 3 * i, 4 * i];
				}

				await That(invocations).IsEqualTo([4, 5, 6, 7, 8,]);
			}

			[Fact]
			public async Task WithoutCallback_IIndexerSetupCallbackBuilder_ShouldNotThrow()
			{
				IIndexerService sut = IIndexerService.CreateMock();
				IIndexerSetupCallbackBuilder<string, int, int, int, int> setup =
					sut.Mock.Setup[It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()];

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
			public async Task WithoutCallback_IIndexerSetupCallbackWhenBuilder_ShouldNotThrow()
			{
				IIndexerService sut = IIndexerService.CreateMock();
				IIndexerSetupCallbackWhenBuilder<string, int, int, int, int> setup =
					sut.Mock.Setup[It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()];

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

			private sealed class MyIndexerSetup<T1, T2, T3, T4>()
				: IndexerSetup<string, T1, T2, T3, T4>(
					(IParameterMatch<T1>)It.IsAny<T1>(),
					(IParameterMatch<T2>)It.IsAny<T2>(),
					(IParameterMatch<T3>)It.IsAny<T3>(),
					(IParameterMatch<T4>)It.IsAny<T4>())
			{
				private readonly ValueStorage _storage = new();

				public T DoGetResult<T>(
					IndexerAccess indexerAccess, T value, MockBehavior? behavior = null)
					=> GetResult(indexerAccess, behavior ?? MockBehavior.Default, _storage, value);

				public void DoSetResult<T>(
					IndexerAccess indexerAccess, T value, MockBehavior? behavior = null)
					=> SetResult(indexerAccess, behavior ?? MockBehavior.Default, _storage, value);
			}
		}

		public sealed class With5Levels
		{
			[Fact]
			public async Task Callback_WhenLengthDoesNotMatch_ShouldIgnore()
			{
				int callCount = 0;
				IIndexerService sut = IIndexerService.CreateMock();
				sut.Mock.Setup[It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>()]
					.OnGet.Do(() => { callCount++; });

				_ = sut[1];
				_ = sut[2, 2];
				_ = sut[3, 3, 3];
				_ = sut[4, 4, 4, 4];
				_ = sut[5, 5, 5, 5, 5];

				await That(callCount).IsEqualTo(1);
			}

			[Fact]
			public async Task ExecuteGetterCallback_WhenGenericTypeDoesNotMatch_ShouldNotExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int, int, int, int> indexerSetup = new();
				indexerSetup.OnGet.Do(() => { callCount++; });
				IndexerGetterAccess<string, string, string, string, string> access =
					new("p1", "a", "p2", "b", "p3", "c", "p4", "d", "p5", "e");

				long result = indexerSetup.DoGetResult(access, 2L);

				await That(result).IsEqualTo(2L);
				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task ExecuteGetterCallback_WhenNumberOfParametersDoesNotMatch_ShouldNotExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int, int, int, int> indexerSetup = new();
				indexerSetup.OnGet.Do(() => { callCount++; });
				IndexerGetterAccess<int, int, int, int> access =
					new("p1", 1, "p2", 2, "p3", 3, "p4", 4);

				string result = indexerSetup.DoGetResult(access, "foo");

				await That(result).IsEqualTo("foo");
				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task ExecuteGetterCallback_WhenParameterTypeDoesNotMatch_ShouldNotExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int, int, int, int> indexerSetup = new();
				indexerSetup.OnGet.Do(() => { callCount++; });
				IndexerGetterAccess<int, int, int, int, string> access =
					new("p1", 1, "p2", 2, "p3", 3, "p4", 4, "p5", "expect-int");

				string result = indexerSetup.DoGetResult(access, "foo");

				await That(result).IsEqualTo("foo");
				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task ExecuteGetterCallback_WhenTypesAndNumberMatch_ShouldExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int, int, int, int> indexerSetup = new();
				indexerSetup.OnGet.Do(() => { callCount++; });
				IndexerGetterAccess<int, int, int, int, int> access =
					new("p1", 1, "p2", 2, "p3", 3, "p4", 4, "p5", 5);

				string result = indexerSetup.DoGetResult(access, "foo");

				await That(callCount).IsEqualTo(1);
			}

			[Fact]
			public async Task ExecuteSetterCallback_WhenGenericTypeDoesNotMatch_ShouldNotExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int, int, int, int> indexerSetup = new();
				indexerSetup.OnSet.Do(() => { callCount++; });
				IndexerSetterAccess<int, int, int, int, int, int> access =
					new("p1", 1, "p2", 2, "p3", 3, "p4", 4, "p5", 5, 99);

				indexerSetup.DoSetResult(access, 2L);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task ExecuteSetterCallback_WhenNumberOfParametersDoesNotMatch_ShouldNotExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int, int, int, int> indexerSetup = new();
				indexerSetup.OnSet.Do(() => { callCount++; });
				IndexerSetterAccess<int, int, int, int, string> access =
					new("p1", 1, "p2", 2, "p3", 3, "p4", 4, "bar");

				indexerSetup.DoSetResult(access, "foo");

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task ExecuteSetterCallback_WhenParameterTypeDoesNotMatch_ShouldNotExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int, int, int, int> indexerSetup = new();
				indexerSetup.OnSet.Do(() => { callCount++; });
				IndexerSetterAccess<int, int, int, int, string, string> access =
					new("p1", 1, "p2", 2, "p3", 3, "p4", 4, "p5", "expect-int", "bar");

				indexerSetup.DoSetResult(access, "foo");

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task ExecuteSetterCallback_WhenTypesAndNumberMatch_ShouldExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int, int, int, int> indexerSetup = new();
				indexerSetup.OnSet.Do(() => { callCount++; });
				IndexerSetterAccess<int, int, int, int, int, string> access =
					new("p1", 1, "p2", 2, "p3", 3, "p4", 4, "p5", 5, "bar");

				indexerSetup.DoSetResult(access, "foo");

				await That(callCount).IsEqualTo(1);
			}

			[Fact]
			public async Task InParallel_ShouldInvokeParallelCallbacksAlways()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				int callCount3 = 0;
				IIndexerService sut = IIndexerService.CreateMock();

				sut.Mock.Setup[It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()]
					.OnGet.Do(() => { callCount1++; })
					.OnGet.Do((p1, _, _, _, _) => { callCount2 += p1; }).InParallel()
					.OnGet.Do((p1, _, _, _, _) => { callCount3 += p1; });

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
				IIndexerService sut = IIndexerService.CreateMock();

				sut.Mock.Setup[It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()]
					.OnGet.Do((p1, _, _, _, _) => { sum += p1; }).Only(times);

				_ = sut[1, 2, 3, 4, 5];
				_ = sut[2, 2, 3, 4, 5];
				_ = sut[3, 2, 3, 4, 5];
				_ = sut[4, 2, 3, 4, 5];

				await That(sum).IsEqualTo(expectedValue);
			}

			[Fact]
			public async Task Only_ShouldStopExecutingCallbackAfterTheGivenTimes()
			{
				List<int> invocations = [];
				IIndexerService sut = IIndexerService.CreateMock();
				sut.Mock.Setup[It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()]
					.OnGet.Do((i, _, _, _, _, _) => { invocations.Add(i); })
					.Only(4);

				for (int i = 0; i < 20; i++)
				{
					_ = sut[i, 2 * i, 3 * i, 4 * i, 5 * i];
				}

				await That(invocations).IsEqualTo([0, 1, 2, 3,]);
			}

			[Fact]
			public async Task Only_WithWhen_ShouldStopExecutingCallbackAfterTheGivenTimes()
			{
				List<int> invocations = [];
				IIndexerService sut = IIndexerService.CreateMock();
				sut.Mock.Setup[It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()]
					.OnGet.Do((i, _, _, _, _, _) => { invocations.Add(i); })
					.When(x => x > 2)
					.Only(4);

				for (int i = 0; i < 20; i++)
				{
					_ = sut[i, 2 * i, 3 * i, 4 * i, 5 * i];
				}

				await That(invocations).IsEqualTo([3, 4, 5, 6,]);
			}

			[Fact]
			public async Task OnlyOnce_ShouldDeactivateCallbackAfterFirstExecution()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				IIndexerService sut = IIndexerService.CreateMock();

				sut.Mock.Setup[It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()]
					.OnGet.Do(() => { callCount1++; })
					.OnGet.Do((p1, _, _, _, _) => { callCount2 += p1; }).OnlyOnce();

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
				IIndexerService sut = IIndexerService.CreateMock();
				sut.Mock.Setup[It.Satisfies<int>(i => i < 6), It.Satisfies<int>(i => i < 6),
						It.Satisfies<int>(i => i < 6), It.Satisfies<int>(i => i < 6), It.Satisfies<int>(i => i < 6)]
					.OnGet.Do(() => { callCount++; });

				_ = sut[1, 1, 1, 6, 1]; // no
				_ = sut[1, 3, 1, 2, 4]; // yes
				_ = sut[2, 4, 2, 1, 3]; // yes
				_ = sut[1, 1, 1, 1, 6]; // no
				_ = sut[1, 1, 6, 1, 1]; // no
				_ = sut[1, 1, 1, 1, -4]; // yes
				_ = sut[1, 6, 2, 1, 3]; // no
				_ = sut[6, 7, 8, 9, 10]; // no

				await That(callCount).IsEqualTo(3);
			}

			[Fact]
			public async Task ShouldExecuteGetterCallbacksWithValue()
			{
				int callCount = 0;
				IIndexerService sut = IIndexerService.CreateMock();
				sut.Mock.Setup[It.Satisfies<int>(i => i < 6), It.Satisfies<int>(i => i < 6),
						It.Satisfies<int>(i => i < 6), It.Satisfies<int>(i => i < 6), It.Satisfies<int>(i => i < 6)]
					.OnGet.Do((v1, v2, v3, v4, v5) => { callCount += v1 * v2 * v3 * v4 * v5; });

				_ = sut[1, 1, 1, 7, 1]; // no
				_ = sut[1, 3, 1, 2, 4]; // yes (24)
				_ = sut[2, 4, 2, 1, 3]; // yes (48)
				_ = sut[1, 1, 1, 1, 7]; // no
				_ = sut[1, 1, 7, 1, 1]; // no
				_ = sut[1, 3, 3, 1, -4]; // yes (-36)
				_ = sut[1, 7, 2, 1, 3]; // no
				_ = sut[6, 7, 8, 9, 10]; // no

				await That(callCount).IsEqualTo(36);
			}

			[Fact]
			public async Task ShouldIncludeIncrementingAccessCounter()
			{
				List<int> invocations = [];
				IIndexerService sut = IIndexerService.CreateMock();
				sut.Mock.Setup[It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()]
					.OnGet.Do((i, _, _, _, _, _, _) => { invocations.Add(i); });

				for (int i = 0; i < 10; i++)
				{
					_ = sut[10 * i, 20 * i, 30 * i, 40 * i, 50 * i];
				}

				await That(invocations).IsEqualTo([0, 1, 2, 3, 4, 5, 6, 7, 8, 9,]);
			}

			[Fact]
			public async Task ShouldInvokeCallbacksInSequence()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				IIndexerService sut = IIndexerService.CreateMock();

				sut.Mock.Setup[It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()]
					.OnGet.Do(() => { callCount1++; })
					.OnGet.Do((p1, _, _, _, _) => { callCount2 += p1; });

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
				IIndexerService sut = IIndexerService.CreateMock();
				sut.Mock.Setup[It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()]
					.OnGet.Do((p1, _, _, _, _, _) => { invocations.Add(p1); })
					.When(x => x is > 3 and < 9);

				for (int i = 0; i < 20; i++)
				{
					_ = sut[i, 2 * i, 3 * i, 4 * i, 5 * i];
				}

				await That(invocations).IsEqualTo([4, 5, 6, 7, 8,]);
			}

			[Fact]
			public async Task WithoutCallback_IIndexerSetupCallbackBuilder_ShouldNotThrow()
			{
				IIndexerService sut = IIndexerService.CreateMock();
				IIndexerSetupCallbackBuilder<string, int, int, int, int, int> setup =
					sut.Mock.Setup[It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()];

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
			public async Task WithoutCallback_IIndexerSetupCallbackWhenBuilder_ShouldNotThrow()
			{
				IIndexerService sut = IIndexerService.CreateMock();
				IIndexerSetupCallbackWhenBuilder<string, int, int, int, int, int> setup =
					sut.Mock.Setup[It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()];

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

			private sealed class MyIndexerSetup<T1, T2, T3, T4, T5>()
				: IndexerSetup<string, T1, T2, T3, T4, T5>(
					(IParameterMatch<T1>)It.IsAny<T1>(),
					(IParameterMatch<T2>)It.IsAny<T2>(),
					(IParameterMatch<T3>)It.IsAny<T3>(),
					(IParameterMatch<T4>)It.IsAny<T4>(),
					(IParameterMatch<T5>)It.IsAny<T5>())
			{
				private readonly ValueStorage _storage = new();

				public T DoGetResult<T>(
					IndexerAccess indexerAccess, T value, MockBehavior? behavior = null)
					=> GetResult(indexerAccess, behavior ?? MockBehavior.Default, _storage, value);

				public void DoSetResult<T>(
					IndexerAccess indexerAccess, T value, MockBehavior? behavior = null)
					=> SetResult(indexerAccess, behavior ?? MockBehavior.Default, _storage, value);
			}
		}
	}
}
