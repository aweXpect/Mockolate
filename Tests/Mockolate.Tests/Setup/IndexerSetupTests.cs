using Mockolate.Exceptions;

namespace Mockolate.Tests.Setup;

public sealed class IndexerSetupTests
{
	[Fact]
	public async Task InitializeWith_ShouldInitializeMatchingIndexers()
	{
		Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
		IMock sut = mock;
		mock.Setup.Indexer(With.Matching<int>(i => i < 4))
			.InitializeWith("foo");

		string result2 = mock.Subject[2];
		string result3 = mock.Subject[3];
		string result4 = mock.Subject[4];

		await That(result2).IsEqualTo("foo");
		await That(result3).IsEqualTo("foo");
		await That(result4).IsNull();
	}

	[Fact]
	public async Task InitializeWith_Callback_ShouldInitializeMatchingIndexers()
	{
		Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
		IMock sut = mock;
		mock.Setup.Indexer(With.Matching<int>(i => i < 4))
			.InitializeWith(v => $"foo-{v}");

		string result2 = mock.Subject[2];
		string result3 = mock.Subject[3];
		string result4 = mock.Subject[4];

		await That(result2).IsEqualTo("foo-2");
		await That(result3).IsEqualTo("foo-3");
		await That(result4).IsNull();
	}

	[Fact]
	public async Task InitializeWith_Twice_ShouldThrowMockException()
	{
		Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
		IMock sut = mock;
		var setup = mock.Setup.Indexer(With.Any<int>())
			.InitializeWith("foo");

		void Act()
			=> setup.InitializeWith("bar");

		await That(Act).Throws<MockException>()
			.WithMessage("The indexer is already initialized. You cannot initialize it twice.");
	}

	[Fact]
	public async Task InitializeWith_Callback_Twice_ShouldThrowMockException()
	{
		Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
		IMock sut = mock;
		var setup = mock.Setup.Indexer(With.Any<int>())
			.InitializeWith("foo");

		void Act()
			=> setup.InitializeWith(_ => "bar");

		await That(Act).Throws<MockException>()
			.WithMessage("The indexer is already initialized. You cannot initialize it twice.");
	}

	[Fact]
	public async Task Callback_WhenLengthDoesNotMatch_ShouldIgnore()
	{
		int callCount = 0;
		Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
		IMock sut = mock;
		mock.Setup.Indexer(With.Any<int>())
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
		IMock sut = mock;
		mock.Setup.Indexer(With.Value(2))
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
		IMock sut = mock;
		mock.Setup.Indexer(With.Any<int>())
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
		IMock sut = mock;
		mock.Setup.Indexer(With.Matching<int>(i => i < 4))
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
		IMock sut = mock;
		mock.Setup.Indexer(With.Matching<int>(i => i < 4))
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
		IMock sut = mock;
		mock.Setup.Indexer(With.Matching<int>(i => i < 4))
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
		IMock sut = mock;
		mock.Setup.Indexer(With.Matching<int>(i => i < 4))
			.OnSet(() => { callCount++; });

		mock.Subject[1] = "";
		mock.Subject[2] = "";
		mock.Subject[3] = "";
		mock.Subject[4] = "";
		mock.Subject[5] = "";

		await That(callCount).IsEqualTo(3);
	}

	public sealed class WithTwoLevels
	{
		[Fact]
		public async Task InitializeWith_ShouldInitializeMatchingIndexers()
		{
			Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
			IMock sut = mock;
			mock.Setup.Indexer(With.Matching<int>(i => i < 4), With.Matching<int>(i => i < 4))
				.InitializeWith("foo");

			string result12 = mock.Subject[1, 2];
			string result13 = mock.Subject[2, 3];
			string result14 = mock.Subject[1, 4];
			string result41 = mock.Subject[4, 1];

			await That(result12).IsEqualTo("foo");
			await That(result13).IsEqualTo("foo");
			await That(result14).IsNull();
			await That(result41).IsNull();
		}

		[Fact]
		public async Task InitializeWith_Callback_ShouldInitializeMatchingIndexers()
		{
			Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
			IMock sut = mock;
			mock.Setup.Indexer(With.Matching<int>(i => i < 4), With.Matching<int>(i => i < 4))
				.InitializeWith((v1, v2) => $"foo-{v1}-{v2}");

			string result12 = mock.Subject[1, 2];
			string result13 = mock.Subject[2, 3];
			string result14 = mock.Subject[1, 4];
			string result41 = mock.Subject[4, 1];

			await That(result12).IsEqualTo("foo-1-2");
			await That(result13).IsEqualTo("foo-2-3");
			await That(result14).IsNull();
			await That(result41).IsNull();
		}

		[Fact]
		public async Task InitializeWith_Twice_ShouldThrowMockException()
		{
			Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
			IMock sut = mock;
			var setup = mock.Setup.Indexer(With.Any<int>(), With.Any<int>())
				.InitializeWith("foo");

			void Act()
				=> setup.InitializeWith("bar");

			await That(Act).Throws<MockException>()
				.WithMessage("The indexer is already initialized. You cannot initialize it twice.");
		}

		[Fact]
		public async Task InitializeWith_Callback_Twice_ShouldThrowMockException()
		{
			Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
			IMock sut = mock;
			var setup = mock.Setup.Indexer(With.Any<int>(), With.Any<int>())
				.InitializeWith("foo");

			void Act()
				=> setup.InitializeWith((_, _) => "bar");

			await That(Act).Throws<MockException>()
				.WithMessage("The indexer is already initialized. You cannot initialize it twice.");
		}

		[Fact]
		public async Task Callback_WhenLengthDoesNotMatch_ShouldIgnore()
		{
			int callCount = 0;
			Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
			IMock sut = mock;
			mock.Setup.Indexer(With.Any<int>(), With.Any<int>())
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
			IMock sut = mock;
			mock.Setup.Indexer(With.Any<int>(), With.Any<int>())
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
			IMock sut = mock;
			mock.Setup.Indexer(With.Any<int>(), With.Any<int>())
				.OnSet(() => { callCount1++; })
				.OnSet((value, v1, v2) => { callCount2 += v1 * v2 + value.Length; })
				.OnSet(v => { callCount3 += v.Length; });

			mock.Subject[2, 3] = "foo";  // 6 + 3
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
			IMock sut = mock;
			mock.Setup.Indexer(With.Matching<int>(i => i < 4), With.Matching<int>(i => i < 4))
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
			IMock sut = mock;
			mock.Setup.Indexer(With.Matching<int>(i => i < 4), With.Matching<int>(i => i < 4))
				.OnGet((v1, v2) => { callCount += v1 * v2; });

			_ = mock.Subject[5, 1];  // no
			_ = mock.Subject[3, 2];  // yes (6)
			_ = mock.Subject[2, 3];  // yes (6)
			_ = mock.Subject[1, 4];  // no
			_ = mock.Subject[1, -4]; // yes (-4)
			_ = mock.Subject[8, 6];  // no

			await That(callCount).IsEqualTo(8);
		}

		[Fact]
		public async Task ShouldExecuteSetterCallbacks()
		{
			int callCount = 0;
			Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
			IMock sut = mock;
			mock.Setup.Indexer(With.Matching<int>(i => i < 4), With.Matching<int>(i => i < 4))
				.OnSet(v => { callCount += v.Length; });

			mock.Subject[1,  1] = "a";         // yes (1)
			mock.Subject[1,  2] = "bb";        // yes (2)
			mock.Subject[1,  3] = "ccc";       // yes (3)
			mock.Subject[1,  4] = "dddd";      // no
			mock.Subject[1,  5] = "eeeee";     // no
			mock.Subject[6,  1] = "ffffff";    // no
			mock.Subject[6,  7] = "ggggggg";   // no
			mock.Subject[8, -9] = "hhhhhhhh";  // no
			mock.Subject[3,  3] = "iiiiiiiii"; // yes (9)

			await That(callCount).IsEqualTo(15);
		}

		[Fact]
		public async Task ShouldExecuteSetterCallbacksWithoutAnyValue()
		{
			int callCount = 0;
			Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
			IMock sut = mock;
			mock.Setup.Indexer(With.Matching<int>(i => i < 4), With.Matching<int>(i => i < 4))
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

	public sealed class WithThreeLevels
	{
		[Fact]
		public async Task InitializeWith_ShouldInitializeMatchingIndexers()
		{
			Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
			IMock sut = mock;
			mock.Setup.Indexer(With.Matching<int>(i => i < 4), With.Matching<int>(i => i < 4), With.Matching<int>(i => i < 4))
				.InitializeWith("foo");

			string result123 = mock.Subject[1, 2, 3];
			string result231 = mock.Subject[2, 3, 1];
			string result114 = mock.Subject[1, 1, 4];
			string result141 = mock.Subject[1, 4, 1];
			string result411 = mock.Subject[4, 1, 1];

			await That(result123).IsEqualTo("foo");
			await That(result231).IsEqualTo("foo");
			await That(result114).IsNull();
			await That(result141).IsNull();
			await That(result411).IsNull();
		}

		[Fact]
		public async Task InitializeWith_Callback_ShouldInitializeMatchingIndexers()
		{
			Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
			IMock sut = mock;
			mock.Setup.Indexer(With.Matching<int>(i => i < 4), With.Matching<int>(i => i < 4), With.Matching<int>(i => i < 4))
				.InitializeWith((v1, v2, v3) => $"foo-{v1}-{v2}-{v3}");

			string result123 = mock.Subject[1, 2, 3];
			string result231 = mock.Subject[2, 3, 1];
			string result114 = mock.Subject[1, 1, 4];
			string result141 = mock.Subject[1, 4, 1];
			string result411 = mock.Subject[4, 1, 1];

			await That(result123).IsEqualTo("foo-1-2-3");
			await That(result231).IsEqualTo("foo-2-3-1");
			await That(result114).IsNull();
			await That(result141).IsNull();
			await That(result411).IsNull();
		}

		[Fact]
		public async Task InitializeWith_Twice_ShouldThrowMockException()
		{
			Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
			IMock sut = mock;
			var setup = mock.Setup.Indexer(With.Any<int>(), With.Any<int>(), With.Any<int>())
				.InitializeWith("foo");

			void Act()
				=> setup.InitializeWith("bar");

			await That(Act).Throws<MockException>()
				.WithMessage("The indexer is already initialized. You cannot initialize it twice.");
		}

		[Fact]
		public async Task InitializeWith_Callback_Twice_ShouldThrowMockException()
		{
			Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
			IMock sut = mock;
			var setup = mock.Setup.Indexer(With.Any<int>(), With.Any<int>(), With.Any<int>())
				.InitializeWith("foo");

			void Act()
				=> setup.InitializeWith((_, _, _) => "bar");

			await That(Act).Throws<MockException>()
				.WithMessage("The indexer is already initialized. You cannot initialize it twice.");
		}

		[Fact]
		public async Task Callback_WhenLengthDoesNotMatch_ShouldIgnore()
		{
			int callCount = 0;
			Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
			IMock sut = mock;
			mock.Setup.Indexer(With.Any<int>(), With.Any<int>(), With.Any<int>())
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
			IMock sut = mock;
			mock.Setup.Indexer(With.Any<int>(), With.Any<int>(), With.Any<int>())
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
			IMock sut = mock;
			mock.Setup.Indexer(With.Any<int>(), With.Any<int>(), With.Any<int>())
				.OnSet(() => { callCount1++; })
				.OnSet((value, v1, v2, v3) => { callCount2 += v1 * v2 * v3 + value.Length; })
				.OnSet(v => { callCount3 += v.Length; });

			mock.Subject[1, 2, 3] = "foo";  // 6 + 3
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
			IMock sut = mock;
			mock.Setup.Indexer(With.Matching<int>(i => i < 4), With.Matching<int>(i => i < 4), With.Matching<int>(i => i < 4))
				.OnGet(() => { callCount++; });

			_ = mock.Subject[1, 5, 1];  // no
			_ = mock.Subject[3, 1, 2];  // yes
			_ = mock.Subject[2, 2, 3];  // yes
			_ = mock.Subject[1, 1, 4];  // no
			_ = mock.Subject[1, 1, -4]; // yes
			_ = mock.Subject[6, 2, 1];  // no
			_ = mock.Subject[6, 7, 8];  // no

			await That(callCount).IsEqualTo(3);
		}

		[Fact]
		public async Task ShouldExecuteGetterCallbacksWithValue()
		{
			int callCount = 0;
			Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
			IMock sut = mock;
			mock.Setup.Indexer(With.Matching<int>(i => i < 4), With.Matching<int>(i => i < 4), With.Matching<int>(i => i < 4))
				.OnGet((v1, v2, v3) => { callCount += v1 * v2 * v3; });

			_ = mock.Subject[1, 5, 1];  // no
			_ = mock.Subject[3, 1, 2];  // yes (6)
			_ = mock.Subject[2, 2, 3];  // yes (12)
			_ = mock.Subject[1, 1, 4];  // no
			_ = mock.Subject[1, 1, -4]; // yes (-4)
			_ = mock.Subject[6, 2, 1];  // no
			_ = mock.Subject[6, 7, 8];  // no

			await That(callCount).IsEqualTo(14);
		}

		[Fact]
		public async Task ShouldExecuteSetterCallbacks()
		{
			int callCount = 0;
			Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
			IMock sut = mock;
			mock.Setup.Indexer(With.Matching<int>(i => i < 4), With.Matching<int>(i => i < 4), With.Matching<int>(i => i < 4))
				.OnSet(v => { callCount += v.Length; });

			mock.Subject[1,  1, 1] = "a";         // yes (1)
			mock.Subject[1,  2, 1] = "bb";        // yes (2)
			mock.Subject[3,  1, 2] = "ccc";       // yes (3)
			mock.Subject[1,  1, 4] = "dddd";      // no
			mock.Subject[1,  5, 1] = "eeeee";     // no
			mock.Subject[6,  1, 1] = "ffffff";    // no
			mock.Subject[6,  7, 8] = "ggggggg";   // no
			mock.Subject[8, -9, 1] = "hhhhhhhh";  // no
			mock.Subject[3,  3, 3] = "iiiiiiiii"; // yes (9)

			await That(callCount).IsEqualTo(15);
		}

		[Fact]
		public async Task ShouldExecuteSetterCallbacksWithoutAnyValue()
		{
			int callCount = 0;
			Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
			IMock sut = mock;
			mock.Setup.Indexer(With.Matching<int>(i => i < 4), With.Matching<int>(i => i < 4), With.Matching<int>(i => i < 4))
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

	public interface IIndexerService
	{
		string this[int index] { get; set; }
		string this[int index1, int index2] { get; set; }
		string this[int index1, int index2, int index3] { get; set; }
	}
}
