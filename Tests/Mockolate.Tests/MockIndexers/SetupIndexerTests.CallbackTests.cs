namespace Mockolate.Tests.MockIndexers;

public sealed partial class SetupIndexerTests
{
	public sealed class CallbackTests
	{
		[Fact]
		public async Task OnGet_WhenLengthDoesNotMatch_ShouldIgnore()
		{
			int callCount = 0;
			Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
			mock.Setup.Indexer(WithAny<int>())
				.OnGet(() => { callCount++; });

			_ = mock.Subject[1];
			_ = mock.Subject[2, 2];
			_ = mock.Subject[3, 3, 3];

			await That(callCount).IsEqualTo(1);
		}

		[Fact]
		public async Task ShouldExecuteAllGetterCallbacks()
		{
			int callCount1 = 0;
			int callCount2 = 0;
			int callCount3 = 0;
			Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
			mock.Setup.Indexer(With(2))
				.OnGet(() => { callCount1++; })
				.OnGet(v => { callCount2 += v; })
				.OnGet(() => { callCount3++; });

			_ = mock.Subject[2];
			_ = mock.Subject[2];

			await That(callCount1).IsEqualTo(2);
			await That(callCount2).IsEqualTo(4);
			await That(callCount3).IsEqualTo(2);
		}

		[Fact]
		public async Task ShouldExecuteAllSetterCallbacks()
		{
			int callCount1 = 0;
			int callCount2 = 0;
			int callCount3 = 0;
			Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
			mock.Setup.Indexer(WithAny<int>())
				.OnSet(() => { callCount1++; })
				.OnSet((_, v) => { callCount2 += v; })
				.OnSet(_ => { callCount3++; });

			mock.Subject[2] = "foo";
			mock.Subject[2] = "bar";

			await That(callCount1).IsEqualTo(2);
			await That(callCount2).IsEqualTo(4);
			await That(callCount3).IsEqualTo(2);
		}

		[Fact]
		public async Task ShouldExecuteGetterCallbacks()
		{
			int callCount = 0;
			Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
			mock.Setup.Indexer(With<int>(i => i < 4))
				.OnGet(() => { callCount++; });

			_ = mock.Subject[1];
			_ = mock.Subject[2];
			_ = mock.Subject[3];
			_ = mock.Subject[4];
			_ = mock.Subject[5];
			_ = mock.Subject[6];

			await That(callCount).IsEqualTo(3);
		}

		[Fact]
		public async Task ShouldExecuteGetterCallbacksWithValue()
		{
			int callCount = 0;
			Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
			mock.Setup.Indexer(With<int>(i => i < 4))
				.OnGet(v => { callCount += v; });

			_ = mock.Subject[1];
			_ = mock.Subject[2];
			_ = mock.Subject[3];
			_ = mock.Subject[4];
			_ = mock.Subject[5];

			await That(callCount).IsEqualTo(6);
		}

		[Fact]
		public async Task ShouldExecuteSetterCallbacks()
		{
			int callCount = 0;
			Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
			mock.Setup.Indexer(With<int>(i => i < 4))
				.OnSet(_ => { callCount++; });

			mock.Subject[1] = "";
			mock.Subject[2] = "";
			mock.Subject[3] = "";
			mock.Subject[4] = "";
			mock.Subject[5] = "";
			mock.Subject[6] = "";

			await That(callCount).IsEqualTo(3);
		}

		[Fact]
		public async Task ShouldExecuteSetterCallbacksWithoutAnyValue()
		{
			int callCount = 0;
			Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
			mock.Setup.Indexer(With<int>(i => i < 4))
				.OnSet(() => { callCount++; });

			mock.Subject[1] = "";
			mock.Subject[2] = "";
			mock.Subject[3] = "";
			mock.Subject[4] = "";
			mock.Subject[5] = "";

			await That(callCount).IsEqualTo(3);
		}

		public sealed class With2Levels
		{
			[Fact]
			public async Task Callback_WhenLengthDoesNotMatch_ShouldIgnore()
			{
				int callCount = 0;
				Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
				mock.Setup.Indexer(WithAny<int>(), WithAny<int>())
					.OnGet(() => { callCount++; });

				_ = mock.Subject[1];
				_ = mock.Subject[2, 2];
				_ = mock.Subject[3, 3, 3];

				await That(callCount).IsEqualTo(1);
			}

			[Fact]
			public async Task ShouldExecuteAllGetterCallbacks()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				int callCount3 = 0;
				Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
				mock.Setup.Indexer(WithAny<int>(), WithAny<int>())
					.OnGet(() => { callCount1++; })
					.OnGet((v1, v2) => { callCount2 += v1 * v2; })
					.OnGet(() => { callCount3++; });

				_ = mock.Subject[2, 3];
				_ = mock.Subject[4, 5];

				await That(callCount1).IsEqualTo(2);
				await That(callCount2).IsEqualTo(26);
				await That(callCount3).IsEqualTo(2);
			}

			[Fact]
			public async Task ShouldExecuteAllSetterCallbacks()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				int callCount3 = 0;
				Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
				mock.Setup.Indexer(WithAny<int>(), WithAny<int>())
					.OnSet(() => { callCount1++; })
					.OnSet((value, v1, v2) => { callCount2 += (v1 * v2) + value.Length; })
					.OnSet(v => { callCount3 += v.Length; });

				mock.Subject[2, 3] = "foo"; // 6 + 3
				mock.Subject[4, 5] = "bart"; // 20 + 4

				await That(callCount1).IsEqualTo(2);
				await That(callCount2).IsEqualTo(9 + 24);
				await That(callCount3).IsEqualTo(7);
			}

			[Fact]
			public async Task ShouldExecuteGetterCallbacks()
			{
				int callCount = 0;
				Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
				mock.Setup.Indexer(With<int>(i => i < 4), With<int>(i => i < 4))
					.OnGet(() => { callCount++; });

				_ = mock.Subject[5, 1]; // no
				_ = mock.Subject[3, 2]; // yes
				_ = mock.Subject[2, 3]; // yes
				_ = mock.Subject[1, 4]; // no
				_ = mock.Subject[1, -4]; // yes
				_ = mock.Subject[8, 6]; // no

				await That(callCount).IsEqualTo(3);
			}

			[Fact]
			public async Task ShouldExecuteGetterCallbacksWithValue()
			{
				int callCount = 0;
				Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
				mock.Setup.Indexer(With<int>(i => i < 4), With<int>(i => i < 4))
					.OnGet((v1, v2) => { callCount += v1 * v2; });

				_ = mock.Subject[5, 1]; // no
				_ = mock.Subject[3, 2]; // yes (6)
				_ = mock.Subject[2, 3]; // yes (6)
				_ = mock.Subject[1, 4]; // no
				_ = mock.Subject[1, -4]; // yes (-4)
				_ = mock.Subject[8, 6]; // no

				await That(callCount).IsEqualTo(8);
			}

			[Fact]
			public async Task ShouldExecuteSetterCallbacks()
			{
				int callCount = 0;
				Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
				mock.Setup.Indexer(With<int>(i => i < 4), With<int>(i => i < 4))
					.OnSet(v => { callCount += v.Length; });

				mock.Subject[1, 1] = "a"; // yes (1)
				mock.Subject[1, 2] = "bb"; // yes (2)
				mock.Subject[1, 3] = "ccc"; // yes (3)
				mock.Subject[1, 4] = "dddd"; // no
				mock.Subject[1, 5] = "eeeee"; // no
				mock.Subject[6, 1] = "ffffff"; // no
				mock.Subject[6, 7] = "ggggggg"; // no
				mock.Subject[8, -9] = "hhhhhhhh"; // no
				mock.Subject[3, 3] = "iiiiiiiii"; // yes (9)

				await That(callCount).IsEqualTo(15);
			}

			[Fact]
			public async Task ShouldExecuteSetterCallbacksWithoutAnyValue()
			{
				int callCount = 0;
				Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
				mock.Setup.Indexer(With<int>(i => i < 4), With<int>(i => i < 4))
					.OnSet(() => { callCount++; });

				mock.Subject[1, 1] = ""; // yes
				mock.Subject[1, 2] = ""; // yes
				mock.Subject[1, 3] = ""; // yes
				mock.Subject[1, 4] = ""; // no
				mock.Subject[5, 1] = ""; // no
				mock.Subject[2, 1] = ""; // yes

				await That(callCount).IsEqualTo(4);
			}
		}

		public sealed class With3Levels
		{
			[Fact]
			public async Task Callback_WhenLengthDoesNotMatch_ShouldIgnore()
			{
				int callCount = 0;
				Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
				mock.Setup.Indexer(WithAny<int>(), WithAny<int>(), WithAny<int>())
					.OnGet(() => { callCount++; });

				_ = mock.Subject[1];
				_ = mock.Subject[2, 2];
				_ = mock.Subject[3, 3, 3];
				_ = mock.Subject[4, 4, 4, 4];

				await That(callCount).IsEqualTo(1);
			}

			[Fact]
			public async Task ShouldExecuteAllGetterCallbacks()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				int callCount3 = 0;
				Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
				mock.Setup.Indexer(WithAny<int>(), WithAny<int>(), WithAny<int>())
					.OnGet(() => { callCount1++; })
					.OnGet((v1, v2, v3) => { callCount2 += v1 * v2 * v3; })
					.OnGet(() => { callCount3++; });

				_ = mock.Subject[1, 2, 3]; // 6
				_ = mock.Subject[4, 5, 6]; // 120

				await That(callCount1).IsEqualTo(2);
				await That(callCount2).IsEqualTo(126);
				await That(callCount3).IsEqualTo(2);
			}

			[Fact]
			public async Task ShouldExecuteAllSetterCallbacks()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				int callCount3 = 0;
				Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
				mock.Setup.Indexer(WithAny<int>(), WithAny<int>(), WithAny<int>())
					.OnSet(() => { callCount1++; })
					.OnSet((value, v1, v2, v3) => { callCount2 += (v1 * v2 * v3) + value.Length; })
					.OnSet(v => { callCount3 += v.Length; });

				mock.Subject[1, 2, 3] = "foo"; // 6 + 3
				mock.Subject[4, 5, 6] = "bart"; // 120 + 4

				await That(callCount1).IsEqualTo(2);
				await That(callCount2).IsEqualTo(9 + 124);
				await That(callCount3).IsEqualTo(7);
			}

			[Fact]
			public async Task ShouldExecuteGetterCallbacks()
			{
				int callCount = 0;
				Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
				mock.Setup.Indexer(With<int>(i => i < 4), With<int>(i => i < 4),
						With<int>(i => i < 4))
					.OnGet(() => { callCount++; });

				_ = mock.Subject[1, 5, 1]; // no
				_ = mock.Subject[3, 1, 2]; // yes
				_ = mock.Subject[2, 2, 3]; // yes
				_ = mock.Subject[1, 1, 4]; // no
				_ = mock.Subject[1, 1, -4]; // yes
				_ = mock.Subject[6, 2, 1]; // no
				_ = mock.Subject[6, 7, 8]; // no

				await That(callCount).IsEqualTo(3);
			}

			[Fact]
			public async Task ShouldExecuteGetterCallbacksWithValue()
			{
				int callCount = 0;
				Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
				mock.Setup.Indexer(With<int>(i => i < 4), With<int>(i => i < 4),
						With<int>(i => i < 4))
					.OnGet((v1, v2, v3) => { callCount += v1 * v2 * v3; });

				_ = mock.Subject[1, 5, 1]; // no
				_ = mock.Subject[3, 1, 2]; // yes (6)
				_ = mock.Subject[2, 2, 3]; // yes (12)
				_ = mock.Subject[1, 1, 4]; // no
				_ = mock.Subject[1, 1, -4]; // yes (-4)
				_ = mock.Subject[6, 2, 1]; // no
				_ = mock.Subject[6, 7, 8]; // no

				await That(callCount).IsEqualTo(14);
			}

			[Fact]
			public async Task ShouldExecuteSetterCallbacks()
			{
				int callCount = 0;
				Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
				mock.Setup.Indexer(With<int>(i => i < 4), With<int>(i => i < 4),
						With<int>(i => i < 4))
					.OnSet(v => { callCount += v.Length; });

				mock.Subject[1, 1, 1] = "a"; // yes (1)
				mock.Subject[1, 2, 1] = "bb"; // yes (2)
				mock.Subject[3, 1, 2] = "ccc"; // yes (3)
				mock.Subject[1, 1, 4] = "dddd"; // no
				mock.Subject[1, 5, 1] = "eeeee"; // no
				mock.Subject[6, 1, 1] = "ffffff"; // no
				mock.Subject[6, 7, 8] = "ggggggg"; // no
				mock.Subject[8, -9, 1] = "hhhhhhhh"; // no
				mock.Subject[3, 3, 3] = "iiiiiiiii"; // yes (9)

				await That(callCount).IsEqualTo(15);
			}

			[Fact]
			public async Task ShouldExecuteSetterCallbacksWithoutAnyValue()
			{
				int callCount = 0;
				Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
				mock.Setup.Indexer(With<int>(i => i < 4), With<int>(i => i < 4),
						With<int>(i => i < 4))
					.OnSet(() => { callCount++; });

				mock.Subject[1, 1, 1] = ""; // yes
				mock.Subject[1, 1, 2] = ""; // yes
				mock.Subject[1, 3, 1] = ""; // yes
				mock.Subject[1, 1, 4] = ""; // no
				mock.Subject[1, 5, 1] = ""; // no
				mock.Subject[6, 1, 1] = ""; // no
				mock.Subject[2, 1, 1] = ""; // yes

				await That(callCount).IsEqualTo(4);
			}
		}

		public sealed class With4Levels
		{
			[Fact]
			public async Task Callback_WhenLengthDoesNotMatch_ShouldIgnore()
			{
				int callCount = 0;
				Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
				mock.Setup.Indexer(WithAny<int>(), WithAny<int>(), WithAny<int>(), WithAny<int>())
					.OnGet(() => { callCount++; });

				_ = mock.Subject[1];
				_ = mock.Subject[2, 2];
				_ = mock.Subject[3, 3, 3];
				_ = mock.Subject[4, 4, 4, 4];

				await That(callCount).IsEqualTo(1);
			}

			[Fact]
			public async Task ShouldExecuteAllGetterCallbacks()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				int callCount3 = 0;
				Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
				mock.Setup.Indexer(WithAny<int>(), WithAny<int>(), WithAny<int>(), WithAny<int>())
					.OnGet(() => { callCount1++; })
					.OnGet((v1, v2, v3, v4) => { callCount2 += v1 * v2 * v3 * v4; })
					.OnGet(() => { callCount3++; });

				_ = mock.Subject[1, 2, 3, 4]; // 24
				_ = mock.Subject[4, 5, 6, 7]; // 840

				await That(callCount1).IsEqualTo(2);
				await That(callCount2).IsEqualTo(864);
				await That(callCount3).IsEqualTo(2);
			}

			[Fact]
			public async Task ShouldExecuteAllSetterCallbacks()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				int callCount3 = 0;
				Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
				mock.Setup.Indexer(WithAny<int>(), WithAny<int>(), WithAny<int>(), WithAny<int>())
					.OnSet(() => { callCount1++; })
					.OnSet((value, v1, v2, v3, v4) => { callCount2 += (v1 * v2 * v3 * v4) + value.Length; })
					.OnSet(v => { callCount3 += v.Length; });

				mock.Subject[1, 2, 3, 4] = "foo"; // 24 + 3
				mock.Subject[4, 5, 6, 7] = "bart"; // 840 + 4

				await That(callCount1).IsEqualTo(2);
				await That(callCount2).IsEqualTo(27 + 844);
				await That(callCount3).IsEqualTo(7);
			}

			[Fact]
			public async Task ShouldExecuteGetterCallbacks()
			{
				int callCount = 0;
				Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
				mock.Setup.Indexer(With<int>(i => i < 5), With<int>(i => i < 5),
						With<int>(i => i < 5), With<int>(i => i < 5))
					.OnGet(() => { callCount++; });

				_ = mock.Subject[1, 1, 5, 1]; // no
				_ = mock.Subject[3, 1, 2, 4]; // yes
				_ = mock.Subject[4, 2, 2, 3]; // yes
				_ = mock.Subject[1, 1, 1, 5]; // no
				_ = mock.Subject[1, 5, 1, 1]; // no
				_ = mock.Subject[1, 1, 1, -4]; // yes
				_ = mock.Subject[6, 2, 1, 3]; // no
				_ = mock.Subject[6, 7, 8, 9]; // no

				await That(callCount).IsEqualTo(3);
			}

			[Fact]
			public async Task ShouldExecuteGetterCallbacksWithValue()
			{
				int callCount = 0;
				Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
				mock.Setup.Indexer(With<int>(i => i < 5), With<int>(i => i < 5),
						With<int>(i => i < 5), With<int>(i => i < 5))
					.OnGet((v1, v2, v3, v4) => { callCount += v1 * v2 * v3 * v4; });

				_ = mock.Subject[1, 5, 1, 3]; // no
				_ = mock.Subject[3, 1, 2, 4]; // yes (24)
				_ = mock.Subject[2, 2, 3, 1]; // yes (12)
				_ = mock.Subject[1, 1, 5, 3]; // no
				_ = mock.Subject[1, 1, -4, 2]; // yes (-8)
				_ = mock.Subject[6, 2, 1, 3]; // no
				_ = mock.Subject[6, 7, 8, 9]; // no

				await That(callCount).IsEqualTo(28);
			}

			[Fact]
			public async Task ShouldExecuteSetterCallbacks()
			{
				int callCount = 0;
				Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
				mock.Setup.Indexer(With<int>(i => i < 5), With<int>(i => i < 5),
						With<int>(i => i < 5), With<int>(i => i < 5))
					.OnSet(v => { callCount += v.Length; });

				mock.Subject[1, 1, 1, 1] = "a"; // yes (1)
				mock.Subject[1, 2, 1, 3] = "bb"; // yes (2)
				mock.Subject[3, 1, 2, 4] = "ccc"; // yes (3)
				mock.Subject[1, 1, 4, 3] = "dddd"; // yes (4)
				mock.Subject[1, 5, 1, 1] = "eeeee"; // no
				mock.Subject[6, 1, 1, 1] = "ffffff"; // no
				mock.Subject[6, 7, 8, 9] = "ggggggg"; // no
				mock.Subject[8, -9, 1, 3] = "hhhhhhhh"; // no
				mock.Subject[4, 4, 4, 4] = "iiiiiiiii"; // yes (9)

				await That(callCount).IsEqualTo(19);
			}

			[Fact]
			public async Task ShouldExecuteSetterCallbacksWithoutAnyValue()
			{
				int callCount = 0;
				Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
				mock.Setup.Indexer(With<int>(i => i < 5), With<int>(i => i < 5),
						With<int>(i => i < 5), With<int>(i => i < 5))
					.OnSet(() => { callCount++; });

				mock.Subject[1, 1, 1, 1] = ""; // yes
				mock.Subject[1, 1, 2, 2] = ""; // yes
				mock.Subject[1, 3, 1, 3] = ""; // yes
				mock.Subject[1, 1, 4, 4] = ""; // yes
				mock.Subject[1, 5, 1, 1] = ""; // no
				mock.Subject[6, 1, 1, 1] = ""; // no
				mock.Subject[2, 1, 1, 3] = ""; // yes

				await That(callCount).IsEqualTo(5);
			}
		}

		public sealed class With5Levels
		{
			[Fact]
			public async Task Callback_WhenLengthDoesNotMatch_ShouldIgnore()
			{
				int callCount = 0;
				Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
				mock.Setup.Indexer(WithAny<int>(), WithAny<int>(), WithAny<int>(), WithAny<int>(), WithAny<int>())
					.OnGet(() => { callCount++; });

				_ = mock.Subject[1];
				_ = mock.Subject[2, 2];
				_ = mock.Subject[3, 3, 3];
				_ = mock.Subject[4, 4, 4, 4];
				_ = mock.Subject[5, 5, 5, 5, 5];

				await That(callCount).IsEqualTo(1);
			}

			[Fact]
			public async Task ShouldExecuteAllGetterCallbacks()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				int callCount3 = 0;
				Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
				mock.Setup.Indexer(WithAny<int>(), WithAny<int>(), WithAny<int>(), WithAny<int>(), WithAny<int>())
					.OnGet(() => { callCount1++; })
					.OnGet((v1, v2, v3, v4, v5) => { callCount2 += v1 * v2 * v3 * v4 * v5; })
					.OnGet(() => { callCount3++; });

				_ = mock.Subject[1, 2, 3, 4, 5]; // 120
				_ = mock.Subject[4, 5, 6, 7, 8]; // 6720

				await That(callCount1).IsEqualTo(2);
				await That(callCount2).IsEqualTo(6840);
				await That(callCount3).IsEqualTo(2);
			}

			[Fact]
			public async Task ShouldExecuteAllSetterCallbacks()
			{
				int callCount1 = 0;
				int callCount2 = 0;
				int callCount3 = 0;
				Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
				mock.Setup.Indexer(WithAny<int>(), WithAny<int>(), WithAny<int>(), WithAny<int>(), WithAny<int>())
					.OnSet(() => { callCount1++; })
					.OnSet((value, v1, v2, v3, v4, v5) => { callCount2 += (v1 * v2 * v3 * v4 * v5) + value.Length; })
					.OnSet(v => { callCount3 += v.Length; });

				mock.Subject[1, 2, 3, 4, 5] = "foo"; // 120 + 3
				mock.Subject[4, 5, 6, 7, 8] = "bart"; // 6720 + 4

				await That(callCount1).IsEqualTo(2);
				await That(callCount2).IsEqualTo(123 + 6724);
				await That(callCount3).IsEqualTo(7);
			}

			[Fact]
			public async Task ShouldExecuteGetterCallbacks()
			{
				int callCount = 0;
				Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
				mock.Setup.Indexer(With<int>(i => i < 6), With<int>(i => i < 6),
						With<int>(i => i < 6), With<int>(i => i < 6), With<int>(i => i < 6))
					.OnGet(() => { callCount++; });

				_ = mock.Subject[1, 1, 1, 6, 1]; // no
				_ = mock.Subject[1, 3, 1, 2, 4]; // yes
				_ = mock.Subject[2, 4, 2, 1, 3]; // yes
				_ = mock.Subject[1, 1, 1, 1, 6]; // no
				_ = mock.Subject[1, 1, 6, 1, 1]; // no
				_ = mock.Subject[1, 1, 1, 1, -4]; // yes
				_ = mock.Subject[1, 6, 2, 1, 3]; // no
				_ = mock.Subject[6, 7, 8, 9, 10]; // no

				await That(callCount).IsEqualTo(3);
			}

			[Fact]
			public async Task ShouldExecuteGetterCallbacksWithValue()
			{
				int callCount = 0;
				Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
				mock.Setup.Indexer(With<int>(i => i < 6), With<int>(i => i < 6),
						With<int>(i => i < 6), With<int>(i => i < 6), With<int>(i => i < 6))
					.OnGet((v1, v2, v3, v4, v5) => { callCount += v1 * v2 * v3 * v4 * v5; });

				_ = mock.Subject[1, 1, 1, 7, 1]; // no
				_ = mock.Subject[1, 3, 1, 2, 4]; // yes (24)
				_ = mock.Subject[2, 4, 2, 1, 3]; // yes (48)
				_ = mock.Subject[1, 1, 1, 1, 7]; // no
				_ = mock.Subject[1, 1, 7, 1, 1]; // no
				_ = mock.Subject[1, 3, 3, 1, -4]; // yes (-36)
				_ = mock.Subject[1, 7, 2, 1, 3]; // no
				_ = mock.Subject[6, 7, 8, 9, 10]; // no

				await That(callCount).IsEqualTo(36);
			}

			[Fact]
			public async Task ShouldExecuteSetterCallbacks()
			{
				int callCount = 0;
				Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
				mock.Setup.Indexer(With<int>(i => i < 6), With<int>(i => i < 6),
						With<int>(i => i < 6), With<int>(i => i < 6), With<int>(i => i < 6))
					.OnSet(v => { callCount += v.Length; });

				mock.Subject[1, 1, 1, 1, 1] = "a"; // yes (1)
				mock.Subject[4, 1, 2, 1, 3] = "bb"; // yes (2)
				mock.Subject[1, 3, 1, 2, 4] = "ccc"; // yes (3)
				mock.Subject[2, 1, 1, 4, 3] = "dddd"; // yes (4)
				mock.Subject[1, 1, 5, 1, 1] = "eeeee"; // yes (5)
				mock.Subject[1, 6, 1, 1, 1] = "ffffff"; // no
				mock.Subject[5, 6, 7, 8, 9] = "ggggggg"; // no
				mock.Subject[8, 8, -9, 1, 3] = "hhhhhhhh"; // no
				mock.Subject[5, 5, 5, 5, 5] = "iiiiiiiii"; // yes (9)

				await That(callCount).IsEqualTo(24);
			}

			[Fact]
			public async Task ShouldExecuteSetterCallbacksWithoutAnyValue()
			{
				int callCount = 0;
				Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
				mock.Setup.Indexer(With<int>(i => i < 6), With<int>(i => i < 6),
						With<int>(i => i < 6), With<int>(i => i < 6), With<int>(i => i < 6))
					.OnSet(() => { callCount++; });

				mock.Subject[1, 1, 1, 1, 1] = ""; // yes
				mock.Subject[1, 1, 1, 2, 2] = ""; // yes
				mock.Subject[1, 1, 3, 1, 3] = ""; // yes
				mock.Subject[1, 1, 1, 4, 4] = ""; // yes
				mock.Subject[1, 1, 6, 1, 1] = ""; // no
				mock.Subject[1, 6, 1, 1, 1] = ""; // no
				mock.Subject[1, 2, 1, 1, 3] = ""; // yes

				await That(callCount).IsEqualTo(5);
			}
		}
	}
}
