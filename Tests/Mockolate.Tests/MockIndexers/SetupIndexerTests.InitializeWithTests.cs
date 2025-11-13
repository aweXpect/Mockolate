using Mockolate.Exceptions;
using Mockolate.Setup;

namespace Mockolate.Tests.MockIndexers;

public sealed partial class SetupIndexerTests
{
	public sealed class InitializeWithTests
	{
		[Fact]
		public async Task InitializeWith_Callback_ShouldInitializeMatchingIndexers()
		{
			IIndexerService mock = Mock.Create<IIndexerService>();
			mock.SetupMock.Indexer(With<int>(i => i < 4))
				.InitializeWith(v => $"foo-{v}");

			string result2 = mock[2];
			string result3 = mock[3];
			string result4 = mock[4];

			await That(result2).IsEqualTo("foo-2");
			await That(result3).IsEqualTo("foo-3");
			await That(result4).IsEmpty();
		}

		[Fact]
		public async Task InitializeWith_Callback_Twice_ShouldThrowMockException()
		{
			IIndexerService mock = Mock.Create<IIndexerService>();
			IndexerSetup<string, int> setup = mock.SetupMock.Indexer(WithAny<int>())
				.InitializeWith("foo");

			void Act()
			{
				setup.InitializeWith(_ => "bar");
			}

			await That(Act).Throws<MockException>()
				.WithMessage("The indexer is already initialized. You cannot initialize it twice.");
		}

		[Fact]
		public async Task InitializeWith_ShouldInitializeMatchingIndexers()
		{
			IIndexerService mock = Mock.Create<IIndexerService>();
			mock.SetupMock.Indexer(With<int>(i => i < 4))
				.InitializeWith("foo");

			string result2 = mock[2];
			string result3 = mock[3];
			string result4 = mock[4];

			await That(result2).IsEqualTo("foo");
			await That(result3).IsEqualTo("foo");
			await That(result4).IsEmpty();
		}

		[Fact]
		public async Task InitializeWith_ShouldSupportNull()
		{
			IIndexerService mock = Mock.Create<IIndexerService>();
			mock.SetupMock.Indexer(WithAny<string?>(), With(1), With(2))
				.InitializeWith(42);
			mock.SetupMock.Indexer(With("foo"), With(1), With(2))
				.InitializeWith((int?)null);

			int? result1 = mock["bar", 1, 2];
			int? result2 = mock["foo", 1, 2];

			await That(result1).IsEqualTo(42);
			await That(result2).IsNull();
		}

		[Fact]
		public async Task InitializeWith_Twice_ShouldThrowMockException()
		{
			IIndexerService mock = Mock.Create<IIndexerService>();
			IndexerSetup<string, int> setup = mock.SetupMock.Indexer(WithAny<int>())
				.InitializeWith("foo");

			void Act()
			{
				setup.InitializeWith("bar");
			}

			await That(Act).Throws<MockException>()
				.WithMessage("The indexer is already initialized. You cannot initialize it twice.");
		}

		public sealed class With2Levels
		{
			[Fact]
			public async Task InitializeWith_Callback_ShouldInitializeMatchingIndexers()
			{
				IIndexerService mock = Mock.Create<IIndexerService>();
				mock.SetupMock.Indexer(With<int>(i => i < 4), With<int>(i => i < 4))
					.InitializeWith((v1, v2) => $"foo-{v1}-{v2}");

				string result12 = mock[1, 2];
				string result13 = mock[2, 3];
				string result14 = mock[1, 4];
				string result41 = mock[4, 1];

				await That(result12).IsEqualTo("foo-1-2");
				await That(result13).IsEqualTo("foo-2-3");
				await That(result14).IsEmpty();
				await That(result41).IsEmpty();
			}

			[Fact]
			public async Task InitializeWith_Callback_Twice_ShouldThrowMockException()
			{
				IIndexerService mock = Mock.Create<IIndexerService>();
				IndexerSetup<string, int, int> setup = mock.SetupMock.Indexer(WithAny<int>(), WithAny<int>())
					.InitializeWith("foo");

				void Act()
				{
					setup.InitializeWith((_, _) => "bar");
				}

				await That(Act).Throws<MockException>()
					.WithMessage("The indexer is already initialized. You cannot initialize it twice.");
			}

			[Fact]
			public async Task InitializeWith_ShouldInitializeMatchingIndexers()
			{
				IIndexerService mock = Mock.Create<IIndexerService>();
				mock.SetupMock.Indexer(With<int>(i => i < 4), With<int>(i => i < 4))
					.InitializeWith("foo");

				string result12 = mock[1, 2];
				string result13 = mock[2, 3];
				string result14 = mock[1, 4];
				string result41 = mock[4, 1];

				await That(result12).IsEqualTo("foo");
				await That(result13).IsEqualTo("foo");
				await That(result14).IsEmpty();
				await That(result41).IsEmpty();
			}

			[Fact]
			public async Task InitializeWith_Twice_ShouldThrowMockException()
			{
				IIndexerService mock = Mock.Create<IIndexerService>();
				IndexerSetup<string, int, int> setup = mock.SetupMock.Indexer(WithAny<int>(), WithAny<int>())
					.InitializeWith("foo");

				void Act()
				{
					setup.InitializeWith("bar");
				}

				await That(Act).Throws<MockException>()
					.WithMessage("The indexer is already initialized. You cannot initialize it twice.");
			}
		}

		public sealed class With3Levels
		{
			[Fact]
			public async Task InitializeWith_Callback_ShouldInitializeMatchingIndexers()
			{
				IIndexerService mock = Mock.Create<IIndexerService>();
				mock.SetupMock.Indexer(With<int>(i => i < 4), With<int>(i => i < 4),
						With<int>(i => i < 4))
					.InitializeWith((v1, v2, v3) => $"foo-{v1}-{v2}-{v3}");

				string result123 = mock[1, 2, 3];
				string result231 = mock[2, 3, 1];
				string result114 = mock[1, 1, 4];
				string result141 = mock[1, 4, 1];
				string result411 = mock[4, 1, 1];

				await That(result123).IsEqualTo("foo-1-2-3");
				await That(result231).IsEqualTo("foo-2-3-1");
				await That(result114).IsEmpty();
				await That(result141).IsEmpty();
				await That(result411).IsEmpty();
			}

			[Fact]
			public async Task InitializeWith_Callback_Twice_ShouldThrowMockException()
			{
				IIndexerService mock = Mock.Create<IIndexerService>();
				IndexerSetup<string, int, int, int> setup = mock.SetupMock
					.Indexer(WithAny<int>(), WithAny<int>(), WithAny<int>())
					.InitializeWith("foo");

				void Act()
				{
					setup.InitializeWith((_, _, _) => "bar");
				}

				await That(Act).Throws<MockException>()
					.WithMessage("The indexer is already initialized. You cannot initialize it twice.");
			}

			[Fact]
			public async Task InitializeWith_ShouldInitializeMatchingIndexers()
			{
				IIndexerService mock = Mock.Create<IIndexerService>();
				mock.SetupMock.Indexer(With<int>(i => i < 4), With<int>(i => i < 4),
						With<int>(i => i < 4))
					.InitializeWith("foo");

				string result123 = mock[1, 2, 3];
				string result231 = mock[2, 3, 1];
				string result114 = mock[1, 1, 4];
				string result141 = mock[1, 4, 1];
				string result411 = mock[4, 1, 1];

				await That(result123).IsEqualTo("foo");
				await That(result231).IsEqualTo("foo");
				await That(result114).IsEmpty();
				await That(result141).IsEmpty();
				await That(result411).IsEmpty();
			}

			[Fact]
			public async Task InitializeWith_Twice_ShouldThrowMockException()
			{
				IIndexerService mock = Mock.Create<IIndexerService>();
				IndexerSetup<string, int, int, int> setup = mock.SetupMock
					.Indexer(WithAny<int>(), WithAny<int>(), WithAny<int>())
					.InitializeWith("foo");

				void Act()
				{
					setup.InitializeWith("bar");
				}

				await That(Act).Throws<MockException>()
					.WithMessage("The indexer is already initialized. You cannot initialize it twice.");
			}
		}

		public sealed class With4Levels
		{
			[Fact]
			public async Task InitializeWith_Callback_ShouldInitializeMatchingIndexers()
			{
				IIndexerService mock = Mock.Create<IIndexerService>();
				mock.SetupMock.Indexer(With<int>(i => i < 5), With<int>(i => i < 5),
						With<int>(i => i < 5), With<int>(i => i < 5))
					.InitializeWith((v1, v2, v3, v4) => $"foo-{v1}-{v2}-{v3}-{v4}");

				string result1234 = mock[1, 2, 3, 4];
				string result2341 = mock[2, 3, 4, 1];
				string result1114 = mock[1, 1, 1, 5];
				string result1141 = mock[1, 1, 5, 1];
				string result1411 = mock[1, 5, 1, 1];
				string result4111 = mock[5, 1, 1, 1];

				await That(result1234).IsEqualTo("foo-1-2-3-4");
				await That(result2341).IsEqualTo("foo-2-3-4-1");
				await That(result1114).IsEmpty();
				await That(result1141).IsEmpty();
				await That(result1411).IsEmpty();
				await That(result4111).IsEmpty();
			}

			[Fact]
			public async Task InitializeWith_Callback_Twice_ShouldThrowMockException()
			{
				IIndexerService mock = Mock.Create<IIndexerService>();
				IndexerSetup<string, int, int, int, int> setup = mock.SetupMock
					.Indexer(WithAny<int>(), WithAny<int>(), WithAny<int>(), WithAny<int>())
					.InitializeWith("foo");

				void Act()
				{
					setup.InitializeWith((_, _, _, _) => "bar");
				}

				await That(Act).Throws<MockException>()
					.WithMessage("The indexer is already initialized. You cannot initialize it twice.");
			}

			[Fact]
			public async Task InitializeWith_ShouldInitializeMatchingIndexers()
			{
				IIndexerService mock = Mock.Create<IIndexerService>();
				mock.SetupMock.Indexer(With<int>(i => i < 5), With<int>(i => i < 5),
						With<int>(i => i < 5), With<int>(i => i < 5))
					.InitializeWith("foo");

				string result1234 = mock[1, 2, 3, 4];
				string result2341 = mock[2, 3, 4, 1];
				string result1114 = mock[1, 1, 1, 5];
				string result1141 = mock[1, 1, 5, 1];
				string result1411 = mock[1, 5, 1, 1];
				string result4111 = mock[5, 1, 1, 1];

				await That(result1234).IsEqualTo("foo");
				await That(result2341).IsEqualTo("foo");
				await That(result1114).IsEmpty();
				await That(result1141).IsEmpty();
				await That(result1411).IsEmpty();
				await That(result4111).IsEmpty();
			}

			[Fact]
			public async Task InitializeWith_Twice_ShouldThrowMockException()
			{
				IIndexerService mock = Mock.Create<IIndexerService>();
				IndexerSetup<string, int, int, int, int> setup = mock.SetupMock
					.Indexer(WithAny<int>(), WithAny<int>(), WithAny<int>(), WithAny<int>())
					.InitializeWith("foo");

				void Act()
				{
					setup.InitializeWith("bar");
				}

				await That(Act).Throws<MockException>()
					.WithMessage("The indexer is already initialized. You cannot initialize it twice.");
			}
		}

		public sealed class With5Levels
		{
			[Fact]
			public async Task InitializeWith_Callback_ShouldInitializeMatchingIndexers()
			{
				IIndexerService mock = Mock.Create<IIndexerService>();
				mock.SetupMock.Indexer(With<int>(i => i < 6), With<int>(i => i < 6),
						With<int>(i => i < 6), With<int>(i => i < 6), With<int>(i => i < 6))
					.InitializeWith((v1, v2, v3, v4, v5) => $"foo-{v1}-{v2}-{v3}-{v4}-{v5}");

				string result12345 = mock[1, 2, 3, 4, 5];
				string result52341 = mock[5, 2, 3, 4, 1];
				string result11116 = mock[1, 1, 1, 1, 6];
				string result11161 = mock[1, 1, 1, 6, 1];
				string result11611 = mock[1, 1, 6, 1, 1];
				string result16111 = mock[1, 6, 1, 1, 1];
				string result61111 = mock[6, 1, 1, 1, 1];

				await That(result12345).IsEqualTo("foo-1-2-3-4-5");
				await That(result52341).IsEqualTo("foo-5-2-3-4-1");
				await That(result11116).IsEmpty();
				await That(result11161).IsEmpty();
				await That(result11611).IsEmpty();
				await That(result16111).IsEmpty();
				await That(result61111).IsEmpty();
			}

			[Fact]
			public async Task InitializeWith_Callback_Twice_ShouldThrowMockException()
			{
				IIndexerService mock = Mock.Create<IIndexerService>();
				IndexerSetup<string, int, int, int, int, int> setup = mock.SetupMock.Indexer(WithAny<int>(),
						WithAny<int>(), WithAny<int>(), WithAny<int>(),
						WithAny<int>())
					.InitializeWith("foo");

				void Act()
				{
					setup.InitializeWith((_, _, _, _, _) => "bar");
				}

				await That(Act).Throws<MockException>()
					.WithMessage("The indexer is already initialized. You cannot initialize it twice.");
			}

			[Fact]
			public async Task InitializeWith_ShouldInitializeMatchingIndexers()
			{
				IIndexerService mock = Mock.Create<IIndexerService>();
				mock.SetupMock.Indexer(With<int>(i => i < 6), With<int>(i => i < 6),
						With<int>(i => i < 6), With<int>(i => i < 6), With<int>(i => i < 6))
					.InitializeWith("foo");

				string result12345 = mock[1, 2, 3, 4, 5];
				string result52341 = mock[5, 2, 3, 4, 1];
				string result11116 = mock[1, 1, 1, 1, 6];
				string result11161 = mock[1, 1, 1, 6, 1];
				string result11611 = mock[1, 1, 6, 1, 1];
				string result16111 = mock[1, 6, 1, 1, 1];
				string result61111 = mock[6, 1, 1, 1, 1];

				await That(result12345).IsEqualTo("foo");
				await That(result52341).IsEqualTo("foo");
				await That(result11116).IsEmpty();
				await That(result11161).IsEmpty();
				await That(result11611).IsEmpty();
				await That(result16111).IsEmpty();
				await That(result61111).IsEmpty();
			}

			[Fact]
			public async Task InitializeWith_Twice_ShouldThrowMockException()
			{
				IIndexerService mock = Mock.Create<IIndexerService>();
				IndexerSetup<string, int, int, int, int, int> setup = mock.SetupMock.Indexer(WithAny<int>(),
						WithAny<int>(), WithAny<int>(), WithAny<int>(),
						WithAny<int>())
					.InitializeWith("foo");

				void Act()
				{
					setup.InitializeWith("bar");
				}

				await That(Act).Throws<MockException>()
					.WithMessage("The indexer is already initialized. You cannot initialize it twice.");
			}
		}
	}
}
