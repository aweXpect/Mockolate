using Mockolate.Exceptions;

namespace Mockolate.Tests.MockIndexers;

public sealed partial class SetupIndexerTests
{
	public sealed class InitializeWithTests
	{
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
			await That(result4).IsEmpty();
		}

		[Fact]
		public async Task InitializeWith_Callback_Twice_ShouldThrowMockException()
		{
			Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
			var setup = mock.Setup.Indexer(With.Any<int>())
				.InitializeWith("foo");

			void Act()
				=> setup.InitializeWith(_ => "bar");

			await That(Act).Throws<MockException>()
				.WithMessage("The indexer is already initialized. You cannot initialize it twice.");
		}

		[Fact]
		public async Task InitializeWith_ShouldInitializeMatchingIndexers()
		{
			Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
			mock.Setup.Indexer(With.Matching<int>(i => i < 4))
				.InitializeWith("foo");

			string result2 = mock.Subject[2];
			string result3 = mock.Subject[3];
			string result4 = mock.Subject[4];

			await That(result2).IsEqualTo("foo");
			await That(result3).IsEqualTo("foo");
			await That(result4).IsEmpty();
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

		public sealed class With2Levels
		{
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
				await That(result14).IsEmpty();
				await That(result41).IsEmpty();
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
			public async Task InitializeWith_ShouldInitializeMatchingIndexers()
			{
				Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
				mock.Setup.Indexer(With.Matching<int>(i => i < 4), With.Matching<int>(i => i < 4))
					.InitializeWith("foo");

				string result12 = mock.Subject[1, 2];
				string result13 = mock.Subject[2, 3];
				string result14 = mock.Subject[1, 4];
				string result41 = mock.Subject[4, 1];

				await That(result12).IsEqualTo("foo");
				await That(result13).IsEqualTo("foo");
				await That(result14).IsEmpty();
				await That(result41).IsEmpty();
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
		}

		public sealed class With3Levels
		{
			[Fact]
			public async Task InitializeWith_Callback_ShouldInitializeMatchingIndexers()
			{
				Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
				IMock sut = mock;
				mock.Setup.Indexer(With.Matching<int>(i => i < 4), With.Matching<int>(i => i < 4),
						With.Matching<int>(i => i < 4))
					.InitializeWith((v1, v2, v3) => $"foo-{v1}-{v2}-{v3}");

				string result123 = mock.Subject[1, 2, 3];
				string result231 = mock.Subject[2, 3, 1];
				string result114 = mock.Subject[1, 1, 4];
				string result141 = mock.Subject[1, 4, 1];
				string result411 = mock.Subject[4, 1, 1];

				await That(result123).IsEqualTo("foo-1-2-3");
				await That(result231).IsEqualTo("foo-2-3-1");
				await That(result114).IsEmpty();
				await That(result141).IsEmpty();
				await That(result411).IsEmpty();
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
			public async Task InitializeWith_ShouldInitializeMatchingIndexers()
			{
				Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
				IMock sut = mock;
				mock.Setup.Indexer(With.Matching<int>(i => i < 4), With.Matching<int>(i => i < 4),
						With.Matching<int>(i => i < 4))
					.InitializeWith("foo");

				string result123 = mock.Subject[1, 2, 3];
				string result231 = mock.Subject[2, 3, 1];
				string result114 = mock.Subject[1, 1, 4];
				string result141 = mock.Subject[1, 4, 1];
				string result411 = mock.Subject[4, 1, 1];

				await That(result123).IsEqualTo("foo");
				await That(result231).IsEqualTo("foo");
				await That(result114).IsEmpty();
				await That(result141).IsEmpty();
				await That(result411).IsEmpty();
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
		}

		public sealed class With4Levels
		{
			[Fact]
			public async Task InitializeWith_Callback_ShouldInitializeMatchingIndexers()
			{
				Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
				IMock sut = mock;
				mock.Setup.Indexer(With.Matching<int>(i => i < 5), With.Matching<int>(i => i < 5),
						With.Matching<int>(i => i < 5), With.Matching<int>(i => i < 5))
					.InitializeWith((v1, v2, v3, v4) => $"foo-{v1}-{v2}-{v3}-{v4}");

				string result1234 = mock.Subject[1, 2, 3, 4];
				string result2341 = mock.Subject[2, 3, 4, 1];
				string result1114 = mock.Subject[1, 1, 1, 5];
				string result1141 = mock.Subject[1, 1, 5, 1];
				string result1411 = mock.Subject[1, 5, 1, 1];
				string result4111 = mock.Subject[5, 1, 1, 1];

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
				Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
				IMock sut = mock;
				var setup = mock.Setup.Indexer(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
					.InitializeWith("foo");

				void Act()
					=> setup.InitializeWith((_, _, _, _) => "bar");

				await That(Act).Throws<MockException>()
					.WithMessage("The indexer is already initialized. You cannot initialize it twice.");
			}

			[Fact]
			public async Task InitializeWith_ShouldInitializeMatchingIndexers()
			{
				Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
				IMock sut = mock;
				mock.Setup.Indexer(With.Matching<int>(i => i < 5), With.Matching<int>(i => i < 5),
						With.Matching<int>(i => i < 5), With.Matching<int>(i => i < 5))
					.InitializeWith("foo");

				string result1234 = mock.Subject[1, 2, 3, 4];
				string result2341 = mock.Subject[2, 3, 4, 1];
				string result1114 = mock.Subject[1, 1, 1, 5];
				string result1141 = mock.Subject[1, 1, 5, 1];
				string result1411 = mock.Subject[1, 5, 1, 1];
				string result4111 = mock.Subject[5, 1, 1, 1];

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
				Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
				IMock sut = mock;
				var setup = mock.Setup.Indexer(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>())
					.InitializeWith("foo");

				void Act()
					=> setup.InitializeWith("bar");

				await That(Act).Throws<MockException>()
					.WithMessage("The indexer is already initialized. You cannot initialize it twice.");
			}
		}

		public sealed class With5Levels
		{
			[Fact]
			public async Task InitializeWith_Callback_ShouldInitializeMatchingIndexers()
			{
				Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
				mock.Setup.Indexer(With.Matching<int>(i => i < 6), With.Matching<int>(i => i < 6),
						With.Matching<int>(i => i < 6), With.Matching<int>(i => i < 6), With.Matching<int>(i => i < 6))
					.InitializeWith((v1, v2, v3, v4, v5) => $"foo-{v1}-{v2}-{v3}-{v4}-{v5}");

				string result12345 = mock.Subject[1, 2, 3, 4, 5];
				string result52341 = mock.Subject[5, 2, 3, 4, 1];
				string result11116 = mock.Subject[1, 1, 1, 1, 6];
				string result11161 = mock.Subject[1, 1, 1, 6, 1];
				string result11611 = mock.Subject[1, 1, 6, 1, 1];
				string result16111 = mock.Subject[1, 6, 1, 1, 1];
				string result61111 = mock.Subject[6, 1, 1, 1, 1];

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
				Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
				IMock sut = mock;
				var setup = mock.Setup.Indexer(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>(),
						With.Any<int>())
					.InitializeWith("foo");

				void Act()
					=> setup.InitializeWith((_, _, _, _, _) => "bar");

				await That(Act).Throws<MockException>()
					.WithMessage("The indexer is already initialized. You cannot initialize it twice.");
			}

			[Fact]
			public async Task InitializeWith_ShouldInitializeMatchingIndexers()
			{
				Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
				mock.Setup.Indexer(With.Matching<int>(i => i < 6), With.Matching<int>(i => i < 6),
						With.Matching<int>(i => i < 6), With.Matching<int>(i => i < 6), With.Matching<int>(i => i < 6))
					.InitializeWith("foo");

				string result12345 = mock.Subject[1, 2, 3, 4, 5];
				string result52341 = mock.Subject[5, 2, 3, 4, 1];
				string result11116 = mock.Subject[1, 1, 1, 1, 6];
				string result11161 = mock.Subject[1, 1, 1, 6, 1];
				string result11611 = mock.Subject[1, 1, 6, 1, 1];
				string result16111 = mock.Subject[1, 6, 1, 1, 1];
				string result61111 = mock.Subject[6, 1, 1, 1, 1];

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
				Mock<IIndexerService> mock = Mock.Create<IIndexerService>();
				IMock sut = mock;
				var setup = mock.Setup.Indexer(With.Any<int>(), With.Any<int>(), With.Any<int>(), With.Any<int>(),
						With.Any<int>())
					.InitializeWith("foo");

				void Act()
					=> setup.InitializeWith("bar");

				await That(Act).Throws<MockException>()
					.WithMessage("The indexer is already initialized. You cannot initialize it twice.");
			}
		}
	}
}
