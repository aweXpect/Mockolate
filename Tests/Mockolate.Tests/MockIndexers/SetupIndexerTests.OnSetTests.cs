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
			sut.SetupMock.Indexer(Any<int>())
				.OnSet((i, _, _) => { invocations.Add(i); })
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
			sut.SetupMock.Indexer(Any<int>())
				.OnSet((i, _, _) => { invocations.Add(i); })
				.When(x => x > 2)
				.For(4);

			for (int i = 0; i < 20; i++)
			{
				sut[i] = $"{i}";
			}

			await That(invocations).IsEqualTo([3, 4, 5, 6,]);
		}

		[Fact]
		public async Task ShouldExecuteAllSetterCallbacks()
		{
			int callCount1 = 0;
			int callCount2 = 0;
			int callCount3 = 0;
			IIndexerService mock = Mock.Create<IIndexerService>();
			mock.SetupMock.Indexer(Any<int>())
				.OnSet(() => { callCount1++; })
				.OnSet((_, v) => { callCount2 += v; })
				.OnSet(_ => { callCount3++; });

			mock[2] = "foo";
			mock[2] = "bar";

			await That(callCount1).IsEqualTo(2);
			await That(callCount2).IsEqualTo(4);
			await That(callCount3).IsEqualTo(2);
		}

		[Fact]
		public async Task ShouldExecuteSetterCallbacks()
		{
			int callCount = 0;
			IIndexerService mock = Mock.Create<IIndexerService>();
			mock.SetupMock.Indexer(With<int>(i => i < 4))
				.OnSet(_ => { callCount++; });

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
			mock.SetupMock.Indexer(With<int>(i => i < 4))
				.OnSet(() => { callCount++; });

			mock[1] = "";
			mock[2] = "";
			mock[3] = "";
			mock[4] = "";
			mock[5] = "";

			await That(callCount).IsEqualTo(3);
		}

		[Fact]
		public async Task When_ShouldOnlyExecuteCallbackWhenInvocationCountMatches()
		{
			List<int> invocations = [];
			IIndexerService sut = Mock.Create<IIndexerService>();
			sut.SetupMock.Indexer(Any<int>())
				.OnSet((i, _, _) => { invocations.Add(i); })
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
				sut.SetupMock.Indexer(Any<int>(), Any<int>())
					.OnSet((i, _, _, _) => { invocations.Add(i); })
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
				sut.SetupMock.Indexer(Any<int>(), Any<int>())
					.OnSet((i, _, _, _) => { invocations.Add(i); })
					.When(x => x > 2)
					.For(4);

				for (int i = 0; i < 20; i++)
				{
					sut[i, 2 * i] = $"{i}";
				}

				await That(invocations).IsEqualTo([3, 4, 5, 6,]);
			}

			[Fact]
			public async Task ShouldExecuteAllSetterCallbacks()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				int callCount3 = 0;
				IIndexerService mock = Mock.Create<IIndexerService>();
				mock.SetupMock.Indexer(Any<int>(), Any<int>())
					.OnSet(() => { callCount1++; })
					.OnSet((value, v1, v2) => { callCount2 += (v1 * v2) + value.Length; })
					.OnSet(v => { callCount3 += v.Length; });

				mock[2, 3] = "foo"; // 6 + 3
				mock[4, 5] = "bart"; // 20 + 4

				await That(callCount1).IsEqualTo(2);
				await That(callCount2).IsEqualTo(9 + 24);
				await That(callCount3).IsEqualTo(7);
			}

			[Fact]
			public async Task ShouldExecuteSetterCallbacks()
			{
				int callCount = 0;
				IIndexerService mock = Mock.Create<IIndexerService>();
				mock.SetupMock.Indexer(With<int>(i => i < 4), With<int>(i => i < 4))
					.OnSet(v => { callCount += v.Length; });

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
				mock.SetupMock.Indexer(With<int>(i => i < 4), With<int>(i => i < 4))
					.OnSet(() => { callCount++; });

				mock[1, 1] = ""; // yes
				mock[1, 2] = ""; // yes
				mock[1, 3] = ""; // yes
				mock[1, 4] = ""; // no
				mock[5, 1] = ""; // no
				mock[2, 1] = ""; // yes

				await That(callCount).IsEqualTo(4);
			}

			[Fact]
			public async Task When_ShouldOnlyExecuteCallbackWhenInvocationCountMatches()
			{
				List<int> invocations = [];
				IIndexerService sut = Mock.Create<IIndexerService>();
				sut.SetupMock.Indexer(Any<int>(), Any<int>())
					.OnSet((i, _, _, _) => { invocations.Add(i); })
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
				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>())
					.OnSet((i, _, _, _, _) => { invocations.Add(i); })
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
				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>())
					.OnSet((i, _, _, _, _) => { invocations.Add(i); })
					.When(x => x > 2)
					.For(4);

				for (int i = 0; i < 20; i++)
				{
					sut[i, 2 * i, 3 * i] = $"{i}";
				}

				await That(invocations).IsEqualTo([3, 4, 5, 6,]);
			}

			[Fact]
			public async Task ShouldExecuteAllSetterCallbacks()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				int callCount3 = 0;
				IIndexerService mock = Mock.Create<IIndexerService>();
				mock.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>())
					.OnSet(() => { callCount1++; })
					.OnSet((value, v1, v2, v3) => { callCount2 += (v1 * v2 * v3) + value.Length; })
					.OnSet(v => { callCount3 += v.Length; });

				mock[1, 2, 3] = "foo"; // 6 + 3
				mock[4, 5, 6] = "bart"; // 120 + 4

				await That(callCount1).IsEqualTo(2);
				await That(callCount2).IsEqualTo(9 + 124);
				await That(callCount3).IsEqualTo(7);
			}

			[Fact]
			public async Task ShouldExecuteSetterCallbacks()
			{
				int callCount = 0;
				IIndexerService mock = Mock.Create<IIndexerService>();
				mock.SetupMock.Indexer(With<int>(i => i < 4), With<int>(i => i < 4),
						With<int>(i => i < 4))
					.OnSet(v => { callCount += v.Length; });

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
				mock.SetupMock.Indexer(With<int>(i => i < 4), With<int>(i => i < 4),
						With<int>(i => i < 4))
					.OnSet(() => { callCount++; });

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
			public async Task When_ShouldOnlyExecuteCallbackWhenInvocationCountMatches()
			{
				List<int> invocations = [];
				IIndexerService sut = Mock.Create<IIndexerService>();
				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>())
					.OnSet((i, _, _, _, _) => { invocations.Add(i); })
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
				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>(), Any<int>())
					.OnSet((i, _, _, _, _, _) => { invocations.Add(i); })
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
				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>(), Any<int>())
					.OnSet((i, _, _, _, _, _) => { invocations.Add(i); })
					.When(x => x > 2)
					.For(4);

				for (int i = 0; i < 20; i++)
				{
					sut[i, 2 * i, 3 * i, 4 * i] = $"{i}";
				}

				await That(invocations).IsEqualTo([3, 4, 5, 6,]);
			}

			[Fact]
			public async Task ShouldExecuteAllSetterCallbacks()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				int callCount3 = 0;
				IIndexerService mock = Mock.Create<IIndexerService>();
				mock.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>(), Any<int>())
					.OnSet(() => { callCount1++; })
					.OnSet((value, v1, v2, v3, v4) => { callCount2 += (v1 * v2 * v3 * v4) + value.Length; })
					.OnSet(v => { callCount3 += v.Length; });

				mock[1, 2, 3, 4] = "foo"; // 24 + 3
				mock[4, 5, 6, 7] = "bart"; // 840 + 4

				await That(callCount1).IsEqualTo(2);
				await That(callCount2).IsEqualTo(27 + 844);
				await That(callCount3).IsEqualTo(7);
			}

			[Fact]
			public async Task ShouldExecuteSetterCallbacks()
			{
				int callCount = 0;
				IIndexerService mock = Mock.Create<IIndexerService>();
				mock.SetupMock.Indexer(With<int>(i => i < 5), With<int>(i => i < 5),
						With<int>(i => i < 5), With<int>(i => i < 5))
					.OnSet(v => { callCount += v.Length; });

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
				mock.SetupMock.Indexer(With<int>(i => i < 5), With<int>(i => i < 5),
						With<int>(i => i < 5), With<int>(i => i < 5))
					.OnSet(() => { callCount++; });

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
			public async Task When_ShouldOnlyExecuteCallbackWhenInvocationCountMatches()
			{
				List<int> invocations = [];
				IIndexerService sut = Mock.Create<IIndexerService>();
				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>(), Any<int>())
					.OnSet((i, _, _, _, _, _) => { invocations.Add(i); })
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
				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>(), Any<int>(), Any<int>())
					.OnSet((i, _, _, _, _, _, _) => { invocations.Add(i); })
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
				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>(), Any<int>(), Any<int>())
					.OnSet((i, _, _, _, _, _, _) => { invocations.Add(i); })
					.When(x => x > 2)
					.For(4);

				for (int i = 0; i < 20; i++)
				{
					sut[i, 2 * i, 3 * i, 4 * i, 5 * i] = $"{i}";
				}

				await That(invocations).IsEqualTo([3, 4, 5, 6,]);
			}

			[Fact]
			public async Task ShouldExecuteAllSetterCallbacks()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				int callCount3 = 0;
				IIndexerService mock = Mock.Create<IIndexerService>();
				mock.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>(), Any<int>(), Any<int>())
					.OnSet(() => { callCount1++; })
					.OnSet((value, v1, v2, v3, v4, v5) => { callCount2 += (v1 * v2 * v3 * v4 * v5) + value.Length; })
					.OnSet(v => { callCount3 += v.Length; });

				mock[1, 2, 3, 4, 5] = "foo"; // 120 + 3
				mock[4, 5, 6, 7, 8] = "bart"; // 6720 + 4

				await That(callCount1).IsEqualTo(2);
				await That(callCount2).IsEqualTo(123 + 6724);
				await That(callCount3).IsEqualTo(7);
			}

			[Fact]
			public async Task ShouldExecuteSetterCallbacks()
			{
				int callCount = 0;
				IIndexerService mock = Mock.Create<IIndexerService>();
				mock.SetupMock.Indexer(With<int>(i => i < 6), With<int>(i => i < 6),
						With<int>(i => i < 6), With<int>(i => i < 6), With<int>(i => i < 6))
					.OnSet(v => { callCount += v.Length; });

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
				mock.SetupMock.Indexer(With<int>(i => i < 6), With<int>(i => i < 6),
						With<int>(i => i < 6), With<int>(i => i < 6), With<int>(i => i < 6))
					.OnSet(() => { callCount++; });

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
			public async Task When_ShouldOnlyExecuteCallbackWhenInvocationCountMatches()
			{
				List<int> invocations = [];
				IIndexerService sut = Mock.Create<IIndexerService>();
				sut.SetupMock.Indexer(Any<int>(), Any<int>(), Any<int>(), Any<int>(), Any<int>())
					.OnSet((i, _, _, _, _, _, _) => { invocations.Add(i); })
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
