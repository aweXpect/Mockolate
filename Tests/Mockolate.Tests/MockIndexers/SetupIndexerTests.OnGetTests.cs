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
			IndexerGetterAccess access = new(0, [
				new NamedParameterValue("p1", 1),
			]);

			long result = indexerSetup.DoExecuteGetterCallback(access, 2L);

			await That(result).IsEqualTo(2L);
			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task ExecuteGetterCallback_WhenNumberOfParametersDoesNotMatch_ShouldNotExecute()
		{
			int callCount = 0;
			MyIndexerSetup<int> indexerSetup = new();
			indexerSetup.OnGet.Do(() => { callCount++; });
			IndexerGetterAccess access = new(0, [
				new NamedParameterValue("p1", 1),
				new NamedParameterValue("p2", 1),
			]);

			string result = indexerSetup.DoExecuteGetterCallback(access, "foo");

			await That(result).IsEqualTo("foo");
			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task ExecuteGetterCallback_WhenParameterTypeDoesNotMatch_ShouldNotExecute()
		{
			int callCount = 0;
			MyIndexerSetup<int> indexerSetup = new();
			indexerSetup.OnGet.Do(() => { callCount++; });
			IndexerGetterAccess access = new(0, [
				new NamedParameterValue("p1", "expect-int"),
			]);

			string result = indexerSetup.DoExecuteGetterCallback(access, "foo");

			await That(result).IsEqualTo("foo");
			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task ExecuteGetterCallback_WhenTypesAndNumberMatch_ShouldExecute()
		{
			int callCount = 0;
			MyIndexerSetup<int> indexerSetup = new();
			indexerSetup.OnGet.Do(() => { callCount++; });
			IndexerGetterAccess access = new(0, [new NamedParameterValue("p1", 1),]);

			string result = indexerSetup.DoExecuteGetterCallback(access, "foo");

			await That(result).IsEqualTo("foo");
			await That(callCount).IsEqualTo(1);
		}

		[Fact]
		public async Task ExecuteSetterCallback_WhenGenericTypeDoesNotMatch_ShouldNotExecute()
		{
			int callCount = 0;
			MyIndexerSetup<int> indexerSetup = new();
			indexerSetup.OnSet.Do(() => { callCount++; });
			IndexerSetterAccess access = new(0, [
				new NamedParameterValue("p1", 1),
			], "bar");

			indexerSetup.DoExecuteSetterCallback(access, 2L);

			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task ExecuteSetterCallback_WhenNumberOfParametersDoesNotMatch_ShouldNotExecute()
		{
			int callCount = 0;
			MyIndexerSetup<int> indexerSetup = new();
			indexerSetup.OnSet.Do(() => { callCount++; });
			IndexerSetterAccess access = new(0, [
				new NamedParameterValue("p1", 1),
				new NamedParameterValue("p2", 1),
			], "bar");

			indexerSetup.DoExecuteSetterCallback(access, "foo");

			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task ExecuteSetterCallback_WhenParameterTypeDoesNotMatch_ShouldNotExecute()
		{
			int callCount = 0;
			MyIndexerSetup<int> indexerSetup = new();
			indexerSetup.OnSet.Do(() => { callCount++; });
			IndexerSetterAccess access = new(0, [
				new NamedParameterValue("p1", "expect-int"),
			], "bar");

			indexerSetup.DoExecuteSetterCallback(access, "foo");

			await That(callCount).IsEqualTo(0);
		}

		[Fact]
		public async Task ExecuteSetterCallback_WhenTypesAndNumberMatch_ShouldExecute()
		{
			int callCount = 0;
			MyIndexerSetup<int> indexerSetup = new();
			indexerSetup.OnSet.Do(() => { callCount++; });
			IndexerSetterAccess access = new(0, [new NamedParameterValue("p1", 1),], "bar");

			indexerSetup.DoExecuteSetterCallback(access, "foo");

			await That(callCount).IsEqualTo(1);
		}

		[Fact]
		public async Task For_ShouldStopExecutingCallbackAfterTheGivenTimes()
		{
			List<int> invocations = [];
			IIndexerService sut = Mock.Create<IIndexerService>();
			sut.SetupMock.Indexer(It.IsAny<int>())
				.OnGet.Do((i, _) => { invocations.Add(i); })
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
				.OnGet.Do((i, _) => { invocations.Add(i); })
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
			IIndexerService sut = Mock.Create<IIndexerService>();

			sut.SetupMock.Indexer(It.IsAny<int>())
				.OnGet.Do(v => { sum += v; }).Only(times);

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
			IIndexerService mock = Mock.Create<IIndexerService>();
			mock.SetupMock.Indexer(It.Satisfies<int>(i => i < 4))
				.OnGet.Do(() => { callCount++; });

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
			mock.SetupMock.Indexer(It.Satisfies<int>(i => i < 4))
				.OnGet.Do(v => { callCount += v; });

			_ = mock[1];
			_ = mock[2];
			_ = mock[3];
			_ = mock[4];
			_ = mock[5];

			await That(callCount).IsEqualTo(6);
		}

		[Fact]
		public async Task ShouldIncludeIncrementingAccessCounter()
		{
			List<int> invocations = [];
			IIndexerService sut = Mock.Create<IIndexerService>();
			sut.SetupMock.Indexer(It.IsAny<int>())
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
			IIndexerService sut = Mock.Create<IIndexerService>();

			sut.SetupMock.Indexer(It.IsAny<int>())
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
			IIndexerService sut = Mock.Create<IIndexerService>();
			sut.SetupMock.Indexer(It.IsAny<int>())
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
			IIndexerService mock = Mock.Create<IIndexerService>();
			mock.SetupMock.Indexer(It.IsAny<int>())
				.OnGet.Do(() => { callCount++; });

			_ = mock[1];
			_ = mock[2, 2];
			_ = mock[3, 3, 3];

			await That(callCount).IsEqualTo(1);
		}

		[Fact]
		public async Task WithoutCallback_IIndexerSetupCallbackBuilder_ShouldNotThrow()
		{
			IIndexerService mock = Mock.Create<IIndexerService>();
			IIndexerSetupCallbackBuilder<string, int> setup =
				(IIndexerSetupCallbackBuilder<string, int>)mock.SetupMock.Indexer(It.IsAny<int>());

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
			IIndexerService mock = Mock.Create<IIndexerService>();
			IIndexerSetupCallbackWhenBuilder<string, int> setup =
				(IIndexerSetupCallbackWhenBuilder<string, int>)mock.SetupMock.Indexer(It.IsAny<int>());

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
				new NamedParameter("p1", (IParameter)It.IsAny<T1>()))
		{
			public T DoExecuteGetterCallback<T>(
				IndexerGetterAccess indexerGetterAccess, T value, MockBehavior? behavior = null)
				=> ExecuteGetterCallback(indexerGetterAccess, value, behavior ?? MockBehavior.Default);

			public void DoExecuteSetterCallback<T>(
				IndexerSetterAccess indexerSetterAccess, T value, MockBehavior? behavior = null)
				=> ExecuteSetterCallback(indexerSetterAccess, value, behavior ?? MockBehavior.Default);
		}

		public sealed class With2Levels
		{
			[Fact]
			public async Task Callback_WhenLengthDoesNotMatch_ShouldIgnore()
			{
				int callCount = 0;
				IIndexerService mock = Mock.Create<IIndexerService>();
				mock.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>())
					.OnGet.Do(() => { callCount++; });

				_ = mock[1];
				_ = mock[2, 2];
				_ = mock[3, 3, 3];

				await That(callCount).IsEqualTo(1);
			}

			[Fact]
			public async Task ExecuteGetterCallback_WhenGenericTypeDoesNotMatch_ShouldNotExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int> indexerSetup = new();
				indexerSetup.OnGet.Do(() => { callCount++; });
				IndexerGetterAccess access = new(0, [
					new NamedParameterValue("p1", 1),
					new NamedParameterValue("p2", 2),
				]);

				long result = indexerSetup.DoExecuteGetterCallback(access, 2L);

				await That(result).IsEqualTo(2L);
				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task ExecuteGetterCallback_WhenNumberOfParametersDoesNotMatch_ShouldNotExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int> indexerSetup = new();
				indexerSetup.OnGet.Do(() => { callCount++; });
				IndexerGetterAccess access = new(0, [
					new NamedParameterValue("p1", 1),
					new NamedParameterValue("p2", 2),
					new NamedParameterValue("p3", 3),
				]);

				string result = indexerSetup.DoExecuteGetterCallback(access, "foo");

				await That(result).IsEqualTo("foo");
				await That(callCount).IsEqualTo(0);
			}

			[Theory]
			[InlineData(1, "expect-int")]
			[InlineData("expect-int", 2)]
			public async Task ExecuteGetterCallback_WhenParameterTypeDoesNotMatch_ShouldNotExecute(
				object? v1, object? v2)
			{
				int callCount = 0;
				MyIndexerSetup<int, int> indexerSetup = new();
				indexerSetup.OnGet.Do(() => { callCount++; });
				IndexerGetterAccess access = new(0, [
					new NamedParameterValue("p1", v1),
					new NamedParameterValue("p2", v2),
				]);

				string result = indexerSetup.DoExecuteGetterCallback(access, "foo");

				await That(result).IsEqualTo("foo");
				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task ExecuteGetterCallback_WhenTypesAndNumberMatch_ShouldExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int> indexerSetup = new();
				indexerSetup.OnGet.Do(() => { callCount++; });
				IndexerGetterAccess access = new(0, [
					new NamedParameterValue("p1", 1),
					new NamedParameterValue("p2", 2),
				]);

				string result = indexerSetup.DoExecuteGetterCallback(access, "foo");

				await That(result).IsEqualTo("foo");
				await That(callCount).IsEqualTo(1);
			}

			[Fact]
			public async Task ExecuteSetterCallback_WhenGenericTypeDoesNotMatch_ShouldNotExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int> indexerSetup = new();
				indexerSetup.OnSet.Do(() => { callCount++; });
				IndexerSetterAccess access = new(0, [
					new NamedParameterValue("p1", 1),
					new NamedParameterValue("p2", 2),
				], "bar");

				indexerSetup.DoExecuteSetterCallback(access, 2L);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task ExecuteSetterCallback_WhenNumberOfParametersDoesNotMatch_ShouldNotExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int> indexerSetup = new();
				indexerSetup.OnSet.Do(() => { callCount++; });
				IndexerSetterAccess access = new(0, [
					new NamedParameterValue("p1", 1),
					new NamedParameterValue("p2", 2),
					new NamedParameterValue("p3", 3),
				], "bar");

				indexerSetup.DoExecuteSetterCallback(access, "foo");

				await That(callCount).IsEqualTo(0);
			}

			[Theory]
			[InlineData(1, "expect-int")]
			[InlineData("expect-int", 2)]
			public async Task ExecuteSetterCallback_WhenParameterTypeDoesNotMatch_ShouldNotExecute(
				object? v1, object? v2)
			{
				int callCount = 0;
				MyIndexerSetup<int, int> indexerSetup = new();
				indexerSetup.OnSet.Do(() => { callCount++; });
				IndexerSetterAccess access = new(0, [
					new NamedParameterValue("p1", v1),
					new NamedParameterValue("p2", v2),
				], "bar");

				indexerSetup.DoExecuteSetterCallback(access, "foo");

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task ExecuteSetterCallback_WhenTypesAndNumberMatch_ShouldExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int> indexerSetup = new();
				indexerSetup.OnSet.Do(() => { callCount++; });
				IndexerSetterAccess access = new(0, [
					new NamedParameterValue("p1", 1),
					new NamedParameterValue("p2", 2),
				], "bar");

				indexerSetup.DoExecuteSetterCallback(access, "foo");

				await That(callCount).IsEqualTo(1);
			}

			[Fact]
			public async Task For_ShouldStopExecutingCallbackAfterTheGivenTimes()
			{
				List<int> invocations = [];
				IIndexerService sut = Mock.Create<IIndexerService>();
				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>())
					.OnGet.Do((i, _, _) => { invocations.Add(i); })
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
					.OnGet.Do((i, _, _) => { invocations.Add(i); })
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
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>())
					.OnGet.Do((p1, _) => { sum += p1; }).Only(times);

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
				IIndexerService mock = Mock.Create<IIndexerService>();
				mock.SetupMock.Indexer(It.Satisfies<int>(i => i < 4), It.Satisfies<int>(i => i < 4))
					.OnGet.Do(() => { callCount++; });

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
				mock.SetupMock.Indexer(It.Satisfies<int>(i => i < 4), It.Satisfies<int>(i => i < 4))
					.OnGet.Do((v1, v2) => { callCount += v1 * v2; });

				_ = mock[5, 1]; // no
				_ = mock[3, 2]; // yes (6)
				_ = mock[2, 3]; // yes (6)
				_ = mock[1, 4]; // no
				_ = mock[1, -4]; // yes (-4)
				_ = mock[8, 6]; // no

				await That(callCount).IsEqualTo(8);
			}

			[Fact]
			public async Task ShouldIncludeIncrementingAccessCounter()
			{
				List<int> invocations = [];
				IIndexerService sut = Mock.Create<IIndexerService>();
				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>())
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
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>())
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
				IIndexerService sut = Mock.Create<IIndexerService>();
				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>())
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
				IIndexerService mock = Mock.Create<IIndexerService>();
				IIndexerSetupCallbackBuilder<string, int, int> setup =
					(IIndexerSetupCallbackBuilder<string, int, int>)mock.SetupMock.Indexer(It.IsAny<int>(),
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
			public async Task WithoutCallback_IIndexerSetupCallbackWhenBuilder_ShouldNotThrow()
			{
				IIndexerService mock = Mock.Create<IIndexerService>();
				IIndexerSetupCallbackWhenBuilder<string, int, int> setup =
					(IIndexerSetupCallbackWhenBuilder<string, int, int>)mock.SetupMock.Indexer(It.IsAny<int>(),
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

			private sealed class MyIndexerSetup<T1, T2>()
				: IndexerSetup<string, T1, T2>(
					new NamedParameter("p1", (IParameter)It.IsAny<T1>()),
					new NamedParameter("p2", (IParameter)It.IsAny<T2>()))
			{
				public T DoExecuteGetterCallback<T>(
					IndexerGetterAccess indexerGetterAccess, T value, MockBehavior? behavior = null)
					=> ExecuteGetterCallback(indexerGetterAccess, value, behavior ?? MockBehavior.Default);

				public void DoExecuteSetterCallback<T>(
					IndexerSetterAccess indexerSetterAccess, T value, MockBehavior? behavior = null)
					=> ExecuteSetterCallback(indexerSetterAccess, value, behavior ?? MockBehavior.Default);
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
					.OnGet.Do(() => { callCount++; });

				_ = mock[1];
				_ = mock[2, 2];
				_ = mock[3, 3, 3];
				_ = mock[4, 4, 4, 4];

				await That(callCount).IsEqualTo(1);
			}

			[Fact]
			public async Task ExecuteGetterCallback_WhenGenericTypeDoesNotMatch_ShouldNotExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int, int> indexerSetup = new();
				indexerSetup.OnGet.Do(() => { callCount++; });
				IndexerGetterAccess access = new(0, [
					new NamedParameterValue("p1", 1),
					new NamedParameterValue("p2", 2),
					new NamedParameterValue("p3", 3),
				]);

				long result = indexerSetup.DoExecuteGetterCallback(access, 2L);

				await That(result).IsEqualTo(2L);
				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task ExecuteGetterCallback_WhenNumberOfParametersDoesNotMatch_ShouldNotExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int, int> indexerSetup = new();
				indexerSetup.OnGet.Do(() => { callCount++; });
				IndexerGetterAccess access = new(0, [
					new NamedParameterValue("p1", 1),
					new NamedParameterValue("p2", 2),
					new NamedParameterValue("p3", 3),
					new NamedParameterValue("p4", 4),
				]);

				string result = indexerSetup.DoExecuteGetterCallback(access, "foo");

				await That(result).IsEqualTo("foo");
				await That(callCount).IsEqualTo(0);
			}

			[Theory]
			[InlineData(1, 2, "expect-int")]
			[InlineData(1, "expect-int", 3)]
			[InlineData("expect-int", 2, 3)]
			public async Task ExecuteGetterCallback_WhenParameterTypeDoesNotMatch_ShouldNotExecute(
				object? v1, object? v2, object? v3)
			{
				int callCount = 0;
				MyIndexerSetup<int, int, int> indexerSetup = new();
				indexerSetup.OnGet.Do(() => { callCount++; });
				IndexerGetterAccess access = new(0, [
					new NamedParameterValue("p1", v1),
					new NamedParameterValue("p2", v2),
					new NamedParameterValue("p3", v3),
				]);

				string result = indexerSetup.DoExecuteGetterCallback(access, "foo");

				await That(result).IsEqualTo("foo");
				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task ExecuteGetterCallback_WhenTypesAndNumberMatch_ShouldExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int, int> indexerSetup = new();
				indexerSetup.OnGet.Do(() => { callCount++; });
				IndexerGetterAccess access = new(0, [
					new NamedParameterValue("p1", 1),
					new NamedParameterValue("p2", 2),
					new NamedParameterValue("p3", 3),
				]);

				string result = indexerSetup.DoExecuteGetterCallback(access, "foo");

				await That(result).IsEqualTo("foo");
				await That(callCount).IsEqualTo(1);
			}

			[Fact]
			public async Task ExecuteSetterCallback_WhenGenericTypeDoesNotMatch_ShouldNotExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int, int> indexerSetup = new();
				indexerSetup.OnSet.Do(() => { callCount++; });
				IndexerSetterAccess access = new(0, [
					new NamedParameterValue("p1", 1),
					new NamedParameterValue("p2", 2),
					new NamedParameterValue("p3", 3),
				], "bar");

				indexerSetup.DoExecuteSetterCallback(access, 2L);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task ExecuteSetterCallback_WhenNumberOfParametersDoesNotMatch_ShouldNotExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int, int> indexerSetup = new();
				indexerSetup.OnSet.Do(() => { callCount++; });
				IndexerSetterAccess access = new(0, [
					new NamedParameterValue("p1", 1),
					new NamedParameterValue("p2", 2),
					new NamedParameterValue("p3", 3),
					new NamedParameterValue("p4", 4),
				], "bar");

				indexerSetup.DoExecuteSetterCallback(access, "foo");

				await That(callCount).IsEqualTo(0);
			}

			[Theory]
			[InlineData(1, 2, "expect-int")]
			[InlineData(1, "expect-int", 3)]
			[InlineData("expect-int", 2, 3)]
			public async Task ExecuteSetterCallback_WhenParameterTypeDoesNotMatch_ShouldNotExecute(
				object? v1, object? v2, object? v3)
			{
				int callCount = 0;
				MyIndexerSetup<int, int, int> indexerSetup = new();
				indexerSetup.OnSet.Do(() => { callCount++; });
				IndexerSetterAccess access = new(0, [
					new NamedParameterValue("p1", v1),
					new NamedParameterValue("p2", v2),
					new NamedParameterValue("p3", v3),
				], "bar");

				indexerSetup.DoExecuteSetterCallback(access, "foo");

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task ExecuteSetterCallback_WhenTypesAndNumberMatch_ShouldExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int, int> indexerSetup = new();
				indexerSetup.OnSet.Do(() => { callCount++; });
				IndexerSetterAccess access = new(0, [
					new NamedParameterValue("p1", 1),
					new NamedParameterValue("p2", 2),
					new NamedParameterValue("p3", 3),
				], "bar");

				indexerSetup.DoExecuteSetterCallback(access, "foo");

				await That(callCount).IsEqualTo(1);
			}

			[Fact]
			public async Task For_ShouldStopExecutingCallbackAfterTheGivenTimes()
			{
				List<int> invocations = [];
				IIndexerService sut = Mock.Create<IIndexerService>();
				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.OnGet.Do((i, _, _, _) => { invocations.Add(i); })
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
					.OnGet.Do((i, _, _, _) => { invocations.Add(i); })
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
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.OnGet.Do((p1, _, _) => { sum += p1; }).Only(times);

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
				IIndexerService mock = Mock.Create<IIndexerService>();
				mock.SetupMock.Indexer(It.Satisfies<int>(i => i < 4), It.Satisfies<int>(i => i < 4),
						It.Satisfies<int>(i => i < 4))
					.OnGet.Do(() => { callCount++; });

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
				mock.SetupMock.Indexer(It.Satisfies<int>(i => i < 4), It.Satisfies<int>(i => i < 4),
						It.Satisfies<int>(i => i < 4))
					.OnGet.Do((v1, v2, v3) => { callCount += v1 * v2 * v3; });

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
			public async Task ShouldIncludeIncrementingAccessCounter()
			{
				List<int> invocations = [];
				IIndexerService sut = Mock.Create<IIndexerService>();
				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
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
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
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
				IIndexerService sut = Mock.Create<IIndexerService>();
				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
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
				IIndexerService mock = Mock.Create<IIndexerService>();
				IIndexerSetupCallbackBuilder<string, int, int, int> setup =
					(IIndexerSetupCallbackBuilder<string, int, int, int>)mock.SetupMock.Indexer(It.IsAny<int>(),
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
			public async Task WithoutCallback_IIndexerSetupCallbackWhenBuilder_ShouldNotThrow()
			{
				IIndexerService mock = Mock.Create<IIndexerService>();
				IIndexerSetupCallbackWhenBuilder<string, int, int, int> setup =
					(IIndexerSetupCallbackWhenBuilder<string, int, int, int>)mock.SetupMock.Indexer(It.IsAny<int>(),
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

			private sealed class MyIndexerSetup<T1, T2, T3>()
				: IndexerSetup<string, T1, T2, T3>(
					new NamedParameter("p1", (IParameter)It.IsAny<T1>()),
					new NamedParameter("p2", (IParameter)It.IsAny<T2>()),
					new NamedParameter("p3", (IParameter)It.IsAny<T3>()))
			{
				public T DoExecuteGetterCallback<T>(
					IndexerGetterAccess indexerGetterAccess, T value, MockBehavior? behavior = null)
					=> ExecuteGetterCallback(indexerGetterAccess, value, behavior ?? MockBehavior.Default);

				public void DoExecuteSetterCallback<T>(
					IndexerSetterAccess indexerSetterAccess, T value, MockBehavior? behavior = null)
					=> ExecuteSetterCallback(indexerSetterAccess, value, behavior ?? MockBehavior.Default);
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
					.OnGet.Do(() => { callCount++; });

				_ = mock[1];
				_ = mock[2, 2];
				_ = mock[3, 3, 3];
				_ = mock[4, 4, 4, 4];

				await That(callCount).IsEqualTo(1);
			}

			[Fact]
			public async Task ExecuteGetterCallback_WhenGenericTypeDoesNotMatch_ShouldNotExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int, int, int> indexerSetup = new();
				indexerSetup.OnGet.Do(() => { callCount++; });
				IndexerGetterAccess access = new(0, [
					new NamedParameterValue("p1", 1),
					new NamedParameterValue("p2", 2),
					new NamedParameterValue("p3", 3),
					new NamedParameterValue("p4", 4),
				]);

				long result = indexerSetup.DoExecuteGetterCallback(access, 2L);

				await That(result).IsEqualTo(2L);
				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task ExecuteGetterCallback_WhenNumberOfParametersDoesNotMatch_ShouldNotExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int, int, int> indexerSetup = new();
				indexerSetup.OnGet.Do(() => { callCount++; });
				IndexerGetterAccess access = new(0, [
					new NamedParameterValue("p1", 1),
					new NamedParameterValue("p2", 2),
					new NamedParameterValue("p3", 3),
					new NamedParameterValue("p4", 4),
					new NamedParameterValue("p5", 5),
				]);

				string result = indexerSetup.DoExecuteGetterCallback(access, "foo");

				await That(result).IsEqualTo("foo");
				await That(callCount).IsEqualTo(0);
			}

			[Theory]
			[InlineData(1, 2, 3, "expect-int")]
			[InlineData(1, 2, "expect-int", 4)]
			[InlineData(1, "expect-int", 3, 4)]
			[InlineData("expect-int", 2, 3, 4)]
			public async Task ExecuteGetterCallback_WhenParameterTypeDoesNotMatch_ShouldNotExecute(
				object? v1, object? v2, object? v3, object? v4)
			{
				int callCount = 0;
				MyIndexerSetup<int, int, int, int> indexerSetup = new();
				indexerSetup.OnGet.Do(() => { callCount++; });
				IndexerGetterAccess access = new(0, [
					new NamedParameterValue("p1", v1),
					new NamedParameterValue("p2", v2),
					new NamedParameterValue("p3", v3),
					new NamedParameterValue("p4", v4),
				]);

				string result = indexerSetup.DoExecuteGetterCallback(access, "foo");

				await That(result).IsEqualTo("foo");
				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task ExecuteGetterCallback_WhenTypesAndNumberMatch_ShouldExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int, int, int> indexerSetup = new();
				indexerSetup.OnGet.Do(() => { callCount++; });
				IndexerGetterAccess access = new(0, [
					new NamedParameterValue("p1", 1),
					new NamedParameterValue("p2", 2),
					new NamedParameterValue("p3", 3),
					new NamedParameterValue("p4", 4),
				]);

				string result = indexerSetup.DoExecuteGetterCallback(access, "foo");

				await That(result).IsEqualTo("foo");
				await That(callCount).IsEqualTo(1);
			}

			[Fact]
			public async Task ExecuteSetterCallback_WhenGenericTypeDoesNotMatch_ShouldNotExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int, int, int> indexerSetup = new();
				indexerSetup.OnSet.Do(() => { callCount++; });
				IndexerSetterAccess access = new(0, [
					new NamedParameterValue("p1", 1),
					new NamedParameterValue("p2", 2),
					new NamedParameterValue("p3", 3),
					new NamedParameterValue("p4", 4),
				], "bar");

				indexerSetup.DoExecuteSetterCallback(access, 2L);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task ExecuteSetterCallback_WhenNumberOfParametersDoesNotMatch_ShouldNotExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int, int, int> indexerSetup = new();
				indexerSetup.OnSet.Do(() => { callCount++; });
				IndexerSetterAccess access = new(0, [
					new NamedParameterValue("p1", 1),
					new NamedParameterValue("p2", 2),
					new NamedParameterValue("p3", 3),
					new NamedParameterValue("p4", 4),
					new NamedParameterValue("p5", 5),
				], "bar");

				indexerSetup.DoExecuteSetterCallback(access, "foo");

				await That(callCount).IsEqualTo(0);
			}

			[Theory]
			[InlineData(1, 2, 3, "expect-int")]
			[InlineData(1, 2, "expect-int", 4)]
			[InlineData(1, "expect-int", 3, 4)]
			[InlineData("expect-int", 2, 3, 4)]
			public async Task ExecuteSetterCallback_WhenParameterTypeDoesNotMatch_ShouldNotExecute(
				object? v1, object? v2, object? v3, object? v4)
			{
				int callCount = 0;
				MyIndexerSetup<int, int, int, int> indexerSetup = new();
				indexerSetup.OnSet.Do(() => { callCount++; });
				IndexerSetterAccess access = new(0, [
					new NamedParameterValue("p1", v1),
					new NamedParameterValue("p2", v2),
					new NamedParameterValue("p3", v3),
					new NamedParameterValue("p4", v4),
				], "bar");

				indexerSetup.DoExecuteSetterCallback(access, "foo");

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task ExecuteSetterCallback_WhenTypesAndNumberMatch_ShouldExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int, int, int> indexerSetup = new();
				indexerSetup.OnSet.Do(() => { callCount++; });
				IndexerSetterAccess access = new(0, [
					new NamedParameterValue("p1", 1),
					new NamedParameterValue("p2", 2),
					new NamedParameterValue("p3", 3),
					new NamedParameterValue("p4", 4),
				], "bar");

				indexerSetup.DoExecuteSetterCallback(access, "foo");

				await That(callCount).IsEqualTo(1);
			}

			[Fact]
			public async Task For_ShouldStopExecutingCallbackAfterTheGivenTimes()
			{
				List<int> invocations = [];
				IIndexerService sut = Mock.Create<IIndexerService>();
				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.OnGet.Do((i, _, _, _, _) => { invocations.Add(i); })
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
					.OnGet.Do((i, _, _, _, _) => { invocations.Add(i); })
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
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
					.OnGet.Do((p1, _, _, _) => { sum += p1; }).Only(times);

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
				IIndexerService mock = Mock.Create<IIndexerService>();
				mock.SetupMock.Indexer(It.Satisfies<int>(i => i < 5), It.Satisfies<int>(i => i < 5),
						It.Satisfies<int>(i => i < 5), It.Satisfies<int>(i => i < 5))
					.OnGet.Do(() => { callCount++; });

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
				mock.SetupMock.Indexer(It.Satisfies<int>(i => i < 5), It.Satisfies<int>(i => i < 5),
						It.Satisfies<int>(i => i < 5), It.Satisfies<int>(i => i < 5))
					.OnGet.Do((v1, v2, v3, v4) => { callCount += v1 * v2 * v3 * v4; });

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
			public async Task ShouldIncludeIncrementingAccessCounter()
			{
				List<int> invocations = [];
				IIndexerService sut = Mock.Create<IIndexerService>();
				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
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
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
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
				IIndexerService sut = Mock.Create<IIndexerService>();
				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())
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
				IIndexerService mock = Mock.Create<IIndexerService>();
				IIndexerSetupCallbackBuilder<string, int, int, int, int> setup =
					(IIndexerSetupCallbackBuilder<string, int, int, int, int>)mock.SetupMock.Indexer(It.IsAny<int>(),
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
			public async Task WithoutCallback_IIndexerSetupCallbackWhenBuilder_ShouldNotThrow()
			{
				IIndexerService mock = Mock.Create<IIndexerService>();
				IIndexerSetupCallbackWhenBuilder<string, int, int, int, int> setup =
					(IIndexerSetupCallbackWhenBuilder<string, int, int, int, int>)mock.SetupMock.Indexer(
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

			private sealed class MyIndexerSetup<T1, T2, T3, T4>()
				: IndexerSetup<string, T1, T2, T3, T4>(
					new NamedParameter("p1", (IParameter)It.IsAny<T1>()),
					new NamedParameter("p2", (IParameter)It.IsAny<T2>()),
					new NamedParameter("p3", (IParameter)It.IsAny<T3>()),
					new NamedParameter("p4", (IParameter)It.IsAny<T4>()))
			{
				public T DoExecuteGetterCallback<T>(
					IndexerGetterAccess indexerGetterAccess, T value, MockBehavior? behavior = null)
					=> ExecuteGetterCallback(indexerGetterAccess, value, behavior ?? MockBehavior.Default);

				public void DoExecuteSetterCallback<T>(
					IndexerSetterAccess indexerSetterAccess, T value, MockBehavior? behavior = null)
					=> ExecuteSetterCallback(indexerSetterAccess, value, behavior ?? MockBehavior.Default);
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
					.OnGet.Do(() => { callCount++; });

				_ = mock[1];
				_ = mock[2, 2];
				_ = mock[3, 3, 3];
				_ = mock[4, 4, 4, 4];
				_ = mock[5, 5, 5, 5, 5];

				await That(callCount).IsEqualTo(1);
			}

			[Fact]
			public async Task ExecuteGetterCallback_WhenGenericTypeDoesNotMatch_ShouldNotExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int, int, int, int> indexerSetup = new();
				indexerSetup.OnGet.Do(() => { callCount++; });
				IndexerGetterAccess access = new(0, [
					new NamedParameterValue("p1", 1),
					new NamedParameterValue("p2", 2),
					new NamedParameterValue("p3", 3),
					new NamedParameterValue("p4", 4),
					new NamedParameterValue("p5", 5),
				]);

				long result = indexerSetup.DoExecuteGetterCallback(access, 2L);

				await That(result).IsEqualTo(2L);
				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task ExecuteGetterCallback_WhenNumberOfParametersDoesNotMatch_ShouldNotExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int, int, int, int> indexerSetup = new();
				indexerSetup.OnGet.Do(() => { callCount++; });
				IndexerGetterAccess access = new(0, [
					new NamedParameterValue("p1", 1),
					new NamedParameterValue("p2", 2),
					new NamedParameterValue("p3", 3),
					new NamedParameterValue("p4", 4),
					new NamedParameterValue("p5", 5),
					new NamedParameterValue("p6", 6),
				]);

				string result = indexerSetup.DoExecuteGetterCallback(access, "foo");

				await That(result).IsEqualTo("foo");
				await That(callCount).IsEqualTo(0);
			}

			[Theory]
			[InlineData(1, 2, 3, 4, "expect-int")]
			[InlineData(1, 2, 3, "expect-int", 5)]
			[InlineData(1, 2, "expect-int", 4, 5)]
			[InlineData(1, "expect-int", 3, 4, 5)]
			[InlineData("expect-int", 2, 3, 4, 5)]
			public async Task ExecuteGetterCallback_WhenParameterTypeDoesNotMatch_ShouldNotExecute(
				object? v1, object? v2, object? v3, object? v4, object? v5)
			{
				int callCount = 0;
				MyIndexerSetup<int, int, int, int, int> indexerSetup = new();
				indexerSetup.OnGet.Do(() => { callCount++; });
				IndexerGetterAccess access = new(0, [
					new NamedParameterValue("p1", v1),
					new NamedParameterValue("p2", v2),
					new NamedParameterValue("p3", v3),
					new NamedParameterValue("p4", v4),
					new NamedParameterValue("p5", v5),
				]);

				string result = indexerSetup.DoExecuteGetterCallback(access, "foo");

				await That(result).IsEqualTo("foo");
				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task ExecuteGetterCallback_WhenTypesAndNumberMatch_ShouldExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int, int, int, int> indexerSetup = new();
				indexerSetup.OnGet.Do(() => { callCount++; });
				IndexerGetterAccess access = new(0, [
					new NamedParameterValue("p1", 1),
					new NamedParameterValue("p2", 2),
					new NamedParameterValue("p3", 3),
					new NamedParameterValue("p4", 4),
					new NamedParameterValue("p5", 5),
				]);

				string result = indexerSetup.DoExecuteGetterCallback(access, "foo");

				await That(result).IsEqualTo("foo");
				await That(callCount).IsEqualTo(1);
			}

			[Fact]
			public async Task ExecuteSetterCallback_WhenGenericTypeDoesNotMatch_ShouldNotExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int, int, int, int> indexerSetup = new();
				indexerSetup.OnSet.Do(() => { callCount++; });
				IndexerSetterAccess access = new(0, [
					new NamedParameterValue("p1", 1),
					new NamedParameterValue("p2", 2),
					new NamedParameterValue("p3", 3),
					new NamedParameterValue("p4", 4),
					new NamedParameterValue("p5", 5),
				], "bar");

				indexerSetup.DoExecuteSetterCallback(access, 2L);

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task ExecuteSetterCallback_WhenNumberOfParametersDoesNotMatch_ShouldNotExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int, int, int, int> indexerSetup = new();
				indexerSetup.OnSet.Do(() => { callCount++; });
				IndexerSetterAccess access = new(0, [
					new NamedParameterValue("p1", 1),
					new NamedParameterValue("p2", 2),
					new NamedParameterValue("p3", 3),
					new NamedParameterValue("p4", 4),
					new NamedParameterValue("p5", 5),
					new NamedParameterValue("p6", 6),
				], "bar");

				indexerSetup.DoExecuteSetterCallback(access, "foo");

				await That(callCount).IsEqualTo(0);
			}

			[Theory]
			[InlineData(1, 2, 3, 4, "expect-int")]
			[InlineData(1, 2, 3, "expect-int", 5)]
			[InlineData(1, 2, "expect-int", 4, 5)]
			[InlineData(1, "expect-int", 3, 4, 5)]
			[InlineData("expect-int", 2, 3, 4, 5)]
			public async Task ExecuteSetterCallback_WhenParameterTypeDoesNotMatch_ShouldNotExecute(
				object? v1, object? v2, object? v3, object? v4, object? v5)
			{
				int callCount = 0;
				MyIndexerSetup<int, int, int, int, int> indexerSetup = new();
				indexerSetup.OnSet.Do(() => { callCount++; });
				IndexerSetterAccess access = new(0, [
					new NamedParameterValue("p1", v1),
					new NamedParameterValue("p2", v2),
					new NamedParameterValue("p3", v3),
					new NamedParameterValue("p4", v4),
					new NamedParameterValue("p5", v5),
				], "bar");

				indexerSetup.DoExecuteSetterCallback(access, "foo");

				await That(callCount).IsEqualTo(0);
			}

			[Fact]
			public async Task ExecuteSetterCallback_WhenTypesAndNumberMatch_ShouldExecute()
			{
				int callCount = 0;
				MyIndexerSetup<int, int, int, int, int> indexerSetup = new();
				indexerSetup.OnSet.Do(() => { callCount++; });
				IndexerSetterAccess access = new(0, [
					new NamedParameterValue("p1", 1),
					new NamedParameterValue("p2", 2),
					new NamedParameterValue("p3", 3),
					new NamedParameterValue("p4", 4),
					new NamedParameterValue("p5", 5),
				], "bar");

				indexerSetup.DoExecuteSetterCallback(access, "foo");

				await That(callCount).IsEqualTo(1);
			}

			[Fact]
			public async Task For_ShouldStopExecutingCallbackAfterTheGivenTimes()
			{
				List<int> invocations = [];
				IIndexerService sut = Mock.Create<IIndexerService>();
				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.OnGet.Do((i, _, _, _, _, _) => { invocations.Add(i); })
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
					.OnGet.Do((i, _, _, _, _, _) => { invocations.Add(i); })
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
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
					.OnGet.Do((p1, _, _, _, _) => { sum += p1; }).Only(times);

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
				IIndexerService mock = Mock.Create<IIndexerService>();
				mock.SetupMock.Indexer(It.Satisfies<int>(i => i < 6), It.Satisfies<int>(i => i < 6),
						It.Satisfies<int>(i => i < 6), It.Satisfies<int>(i => i < 6), It.Satisfies<int>(i => i < 6))
					.OnGet.Do(() => { callCount++; });

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
				mock.SetupMock.Indexer(It.Satisfies<int>(i => i < 6), It.Satisfies<int>(i => i < 6),
						It.Satisfies<int>(i => i < 6), It.Satisfies<int>(i => i < 6), It.Satisfies<int>(i => i < 6))
					.OnGet.Do((v1, v2, v3, v4, v5) => { callCount += v1 * v2 * v3 * v4 * v5; });

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
			public async Task ShouldIncludeIncrementingAccessCounter()
			{
				List<int> invocations = [];
				IIndexerService sut = Mock.Create<IIndexerService>();
				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
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
				IIndexerService sut = Mock.Create<IIndexerService>();

				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
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
				IIndexerService sut = Mock.Create<IIndexerService>();
				sut.SetupMock.Indexer(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
						It.IsAny<int>())
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
				IIndexerService mock = Mock.Create<IIndexerService>();
				IIndexerSetupCallbackBuilder<string, int, int, int, int, int> setup =
					(IIndexerSetupCallbackBuilder<string, int, int, int, int, int>)mock.SetupMock.Indexer(
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
			public async Task WithoutCallback_IIndexerSetupCallbackWhenBuilder_ShouldNotThrow()
			{
				IIndexerService mock = Mock.Create<IIndexerService>();
				IIndexerSetupCallbackWhenBuilder<string, int, int, int, int, int> setup =
					(IIndexerSetupCallbackWhenBuilder<string, int, int, int, int, int>)mock.SetupMock.Indexer(
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

			private sealed class MyIndexerSetup<T1, T2, T3, T4, T5>()
				: IndexerSetup<string, T1, T2, T3, T4, T5>(
					new NamedParameter("p1", (IParameter)It.IsAny<T1>()),
					new NamedParameter("p2", (IParameter)It.IsAny<T2>()),
					new NamedParameter("p3", (IParameter)It.IsAny<T3>()),
					new NamedParameter("p4", (IParameter)It.IsAny<T4>()),
					new NamedParameter("p5", (IParameter)It.IsAny<T5>()))
			{
				public T DoExecuteGetterCallback<T>(
					IndexerGetterAccess indexerGetterAccess, T value, MockBehavior? behavior = null)
					=> ExecuteGetterCallback(indexerGetterAccess, value, behavior ?? MockBehavior.Default);

				public void DoExecuteSetterCallback<T>(
					IndexerSetterAccess indexerSetterAccess, T value, MockBehavior? behavior = null)
					=> ExecuteSetterCallback(indexerSetterAccess, value, behavior ?? MockBehavior.Default);
			}
		}
	}
}
