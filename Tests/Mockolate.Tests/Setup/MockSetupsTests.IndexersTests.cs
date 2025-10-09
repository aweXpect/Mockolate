using aweXpect;
using Mockolate.Setup;

namespace Mockolate.Tests.Setup;

public sealed partial class MockSetupsTests
{
	public sealed class IndexerTests
	{
		[Fact]
		public async Task ShouldUseInitializedValue()
		{
			var mock = Mock.Create<IIndexerService>();
			IMock sut = mock;
			mock.SetupIndexer(With.Any<int>()).InitializeWith(2, "foo");

			var result1 = mock.Object[2];
			var result2 = mock.Object[3];

			await That(result1).IsEqualTo("foo");
			await That(result2).IsNull();
		}

		[Fact]
		public async Task ShouldExecuteGetterCallbacks()
		{
			int callCount = 0;
			var mock = Mock.Create<IIndexerService>();
			IMock sut = mock;
			mock.SetupIndexer(With.Any<int>()).For(1, setup => setup.OnGet(() => { callCount++; }));

			_ = mock.Object[1];
			_ = mock.Object[2];
			_ = mock.Object[1];

			await That(callCount).IsEqualTo(2);
		}

		[Fact]
		public async Task ShouldExecuteAllGetterCallbacks()
		{
			int callCount1 = 0;
			int callCount2 = 0;
			int callCount3 = 0;
			var mock = Mock.Create<IIndexerService>();
			IMock sut = mock;
			mock.SetupIndexer(With.Any<int>()).For(2, setup => setup
				.OnGet(() => { callCount1++; })
				.OnGet(v => { callCount2 += v; })
				.OnGet(() => { callCount3++; }));

			_ = mock.Object[2];
			_ = mock.Object[2];

			await That(callCount1).IsEqualTo(2);
			await That(callCount2).IsEqualTo(4);
			await That(callCount3).IsEqualTo(2);
		}

		[Fact]
		public async Task ShouldExecuteGetterCallbacksForAllMatchingParameters()
		{
			int callCount = 0;
			var mock = Mock.Create<IIndexerService>();
			IMock sut = mock;
			mock.SetupIndexer(With.Any<int>()).For(With.Matching<int>(i => i < 4), setup => setup.OnGet(() => { callCount++; }));

			_ = mock.Object[1];
			_ = mock.Object[2];
			_ = mock.Object[3];
			_ = mock.Object[4];
			_ = mock.Object[5];
			_ = mock.Object[6];

			await That(callCount).IsEqualTo(3);
		}

		[Fact]
		public async Task ShouldExecuteGetterCallbacksWithValue()
		{
			int callCount = 0;
			var mock = Mock.Create<IIndexerService>();
			IMock sut = mock;
			mock.SetupIndexer(With.Any<int>()).For(With.Matching<int>(i => i < 4), setup => setup.OnGet(v => { callCount += v; }));

			_ = mock.Object[1];
			_ = mock.Object[2];
			_ = mock.Object[3];
			_ = mock.Object[4];
			_ = mock.Object[5];

			await That(callCount).IsEqualTo(6);
		}

		[Fact]
		public async Task ShouldExecuteSetterCallbacks()
		{
			int callCount = 0;
			var mock = Mock.Create<IIndexerService>();
			IMock sut = mock;
			mock.SetupIndexer(With.Any<int>()).For(1, setup => setup.OnSet(_ => { callCount++; }));

			mock.Object[1] = "foo";
			mock.Object[2] = "foo";
			mock.Object[1] = "foo";

			await That(callCount).IsEqualTo(2);
		}

		[Fact]
		public async Task ShouldExecuteAllSetterCallbacks()
		{
			int callCount1 = 0;
			int callCount2 = 0;
			int callCount3 = 0;
			var mock = Mock.Create<IIndexerService>();
			IMock sut = mock;
			mock.SetupIndexer(With.Any<int>()).For(2, setup => setup
				.OnSet(_ => { callCount1++; })
				.OnSet((v,_) => { callCount2 += v; })
				.OnSet(_ => { callCount3++; }));

			mock.Object[2] = "foo";
			mock.Object[2] = "bar";

			await That(callCount1).IsEqualTo(2);
			await That(callCount2).IsEqualTo(4);
			await That(callCount3).IsEqualTo(2);
		}

		[Fact]
		public async Task ShouldExecuteSetterCallbacksForAllMatchingParameters()
		{
			int callCount = 0;
			var mock = Mock.Create<IIndexerService>();
			IMock sut = mock;
			mock.SetupIndexer(With.Any<int>()).For(With.Matching<int>(i => i < 4), setup =>
				setup.OnSet(_ => { callCount++; }));

			mock.Object[1] = "";
			mock.Object[2] = "";
			mock.Object[3] = "";
			mock.Object[4] = "";
			mock.Object[5] = "";
			mock.Object[6] = "";

			await That(callCount).IsEqualTo(3);
		}

		[Fact]
		public async Task ShouldExecuteSetterCallbacksWithValue()
		{
			int callCount = 0;
			var mock = Mock.Create<IIndexerService>();
			IMock sut = mock;
			mock.SetupIndexer(With.Any<int>()).For(With.Matching<int>(i => i < 4), setup
				=> setup.OnSet((v, _) => { callCount += v; }));

			mock.Object[1] = "";
			mock.Object[2] = "";
			mock.Object[3] = "";
			mock.Object[4] = "";
			mock.Object[5] = "";

			await That(callCount).IsEqualTo(6);
		}

		[Fact]
		public async Task WithoutSetup_ShouldStoreLastValue()
		{
			var mock = Mock.Create<IIndexerService>();
			IMock sut = mock;

			var result0 = mock.Object[1];
			mock.Object[1] = "foo";
			var result1 = mock.Object[1];
			var result2 = mock.Object[2];

			await That(result0).IsNull();
			await That(result1).IsEqualTo("foo");
			await That(result2).IsNull();
		}

		public sealed class ThreeLevels
		{
			[Fact]
			public async Task ShouldUseInitializedValue()
			{
				var mock = Mock.Create<IIndexerService>();
				IMock sut = mock;
				mock.SetupIndexer(With.Any<string>(), With.Any<int>(), With.Any<int>()).InitializeWith("foo", 1, 2, 42);

				var result1 = mock.Object["foo", 1, 2];
				var result2 = mock.Object["bar", 1, 2];

				await That(result1).IsEqualTo(42);
				await That(result2).IsEqualTo(0);
			}

			[Fact]
			public async Task WithoutSetup_ShouldStoreLastValue()
			{
				var mock = Mock.Create<IIndexerService>();
				IMock sut = mock;

				var result0 = mock.Object["foo", 1, 2];
				sut.SetIndexer(42, "foo", 1, 2);
				var result1 = mock.Object["foo", 1, 2];
				var result2 = mock.Object["bar", 1, 2];
				var result3 = mock.Object["foo", 2, 2];
				var result4 = mock.Object["foo", 1, 3];

				await That(result0).IsEqualTo(default(int));
				await That(result1).IsEqualTo(42);
				await That(result2).IsEqualTo(default(int));
				await That(result3).IsEqualTo(default(int));
				await That(result4).IsEqualTo(default(int));
			}
		}

		public sealed class TwoLevels
		{
			[Fact]
			public async Task ShouldUseInitializedValue()
			{
				var mock = Mock.Create<IIndexerService>();
				IMock sut = mock;
				mock.SetupIndexer(With.Any<int?>(), With.Any<int>()).InitializeWith(2, 3, "foo");

				var result1 = mock.Object[2, 3];
				var result2 = mock.Object[null, 4];

				await That(result1).IsEqualTo("foo");
				await That(result2).IsNull();
			}

			[Fact]
			public async Task WithoutSetup_ShouldStoreLastValue()
			{
				var mock = Mock.Create<IIndexerService>();
				IMock sut = mock;

				var result0 = mock.Object[1, 2];
				mock.Object[1, 2] = "foo";
				var result1 = mock.Object[1, 2];
				var result2 = mock.Object[2, 2];

				await That(result0).IsNull();
				await That(result1).IsEqualTo("foo");
				await That(result2).IsNull();
			}

			[Fact]
			public async Task SetOnDifferentLevel_ShouldNotBeUsed()
			{
				var mock = Mock.Create<IIndexerService>();
				IMock sut = mock;

				mock.Object[1] = "foo";
				var result1 = mock.Object[1, 2];
				var result2 = mock.Object[2, 1];

				await That(result1).IsNull();
				await That(result2).IsNull();
			}

			[Fact]
			public async Task ShouldSupportNullAsParameter()
			{
				var mock = Mock.Create<IIndexerService>();
				IMock sut = mock;

				mock.Object[null, 2] = "foo";
				var result1 = mock.Object[null, 2];
				var result2 = mock.Object[0, 2];

				await That(result1).IsEqualTo("foo");
				await That(result2).IsNull();
			}
		}

		public interface IIndexerService
		{
			string this[int index] { get; set; }
			string this[int? index1, int index2] { get; set; }
			int this[string index1, int index2, int index3] { get; set; }
		}
	}
}
