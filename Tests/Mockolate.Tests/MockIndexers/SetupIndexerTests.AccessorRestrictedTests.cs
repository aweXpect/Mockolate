using System.Collections.Generic;
using Mockolate.Setup;

namespace Mockolate.Tests.MockIndexers;

public sealed partial class SetupIndexerTests
{
	public sealed class AccessorRestrictedTests
	{
		[Fact]
		public async Task SetOnlyIndexer_OnSet_ShouldFireAndRecordTheWrite()
		{
			IAccessorService sut = IAccessorService.CreateMock();
			List<string> written = new();
			sut.Mock.Setup[It.IsAny<string>()].OnSet.Do(value => written.Add(value));

			sut["name"] = "Ada";

			await That(written).IsEqualTo(["Ada",]);
			await That(sut.Mock.Verify[It.Is("name")].Set("Ada")).Once()
				.Because("the setter facade must be keyed on the setter member id, not the getter's");
		}

		[Fact]
		public async Task SetOnlyIndexer_VerifyOtherValue_ShouldNotMatch()
		{
			IAccessorService sut = IAccessorService.CreateMock();

			sut["name"] = "Ada";

			await That(sut.Mock.Verify[It.Is("name")].Set("Grace")).Never();
		}

		[Fact]
		public async Task SetOnlyIndexer_VerifyWithParameterMatcher_ShouldMatch()
		{
			IAccessorService sut = IAccessorService.CreateMock();

			sut["name"] = "Ada";

			await That(sut.Mock.Verify[It.IsAny<string>()].Set(It.IsAny<string>())).Once();
		}

		[Theory]
		[InlineData(false, 1)]
		[InlineData(true, 0)]
		public async Task SetOnlyClassIndexer_ShouldSkipCallingBaseWhenRequested(bool skipBaseClass,
			int expectedCallCount)
		{
			AccessorService sut = AccessorService.CreateMock();
			sut.Mock.Setup[It.IsAny<int>()].SkippingBaseClass(skipBaseClass);

			sut[4] = 1;

			await That(sut.SetterCallCount).IsEqualTo(expectedCallCount);
		}

		[Fact]
		public async Task GetOnlyIndexer_ChainedReturns_ShouldStayOnGetterOnlySurface()
		{
			IAccessorService sut = IAccessorService.CreateMock();
			IIndexerGetterOnlySetup<int, int> chained = sut.Mock.Setup[It.IsAny<int>()]
				.Returns(1).OnlyOnce();
			chained.Returns(() => 2);

			await That(sut[1]).IsEqualTo(1);
			await That(sut[1]).IsEqualTo(2);
		}

		[Fact]
		public async Task GetOnlyIndexer_ReturnsForever_ShouldUseTheLastValueForever()
		{
			IAccessorService sut = IAccessorService.CreateMock();
			sut.Mock.Setup[It.IsAny<int>()]
				.Returns(2)
				.Returns(4).Forever();

			int[] result = new int[4];
			for (int i = 0; i < 4; i++)
			{
				result[i] = sut[i];
			}

			await That(result).IsEqualTo([2, 4, 4, 4,]);
		}

		[Fact]
		public async Task GetOnlyIndexer_ReturnsWhen_ShouldOnlyUseValueWhenPredicateIsTrue()
		{
			IAccessorService sut = IAccessorService.CreateMock();
			sut.Mock.Setup[It.IsAny<int>()]
				.Returns(() => 4).When(i => i > 0);

			int result1 = sut[1];
			int result2 = sut[1];

			await That(result1).IsEqualTo(0);
			await That(result2).IsEqualTo(4);
		}

		[Fact]
		public async Task GetOnlyIndexer_ReturnsCallbackWithValue_ShouldReturnExpectedValue()
		{
			IAccessorService sut = IAccessorService.CreateMock();
			sut.Mock.Setup[It.IsAny<int>()]
				.InitializeWith(3)
				.Returns((_, v) => 4 * v);

			int result = sut[7];

			await That(result).IsEqualTo(12);
		}

		[Fact]
		public async Task GetOnlyIndexer_InitializeWithGenerator_ShouldSeedTheReadValue()
		{
			IAccessorService sut = IAccessorService.CreateMock();
			sut.Mock.Setup[It.IsAny<int>()]
				.InitializeWith(key => 10 * key);

			await That(sut[3]).IsEqualTo(30);
			await That(sut[5]).IsEqualTo(50);
		}

		[Fact]
		public async Task GetOnlyIndexer_Throws_ShouldIterateThroughAllRegisteredExceptions()
		{
			IAccessorService sut = IAccessorService.CreateMock();
			sut.Mock.Setup[It.IsAny<int>()]
				.Throws<InvalidOperationException>()
				.Throws(new Exception("foo"))
				.Throws(() => new Exception("bar"))
				.Throws(k => new Exception($"baz-{k}"))
				.Throws((k, v) => new Exception($"qux-{k}-{v}"));

			void Act() => _ = sut[6];

			await That(Act).Throws<InvalidOperationException>();
			Exception? result2 = Record.Exception(Act);
			Exception? result3 = Record.Exception(Act);
			Exception? result4 = Record.Exception(Act);
			Exception? result5 = Record.Exception(Act);
			await That(result2).HasMessage("foo");
			await That(result3).HasMessage("bar");
			await That(result4).HasMessage("baz-6");
			await That(result5).HasMessage("qux-6-0");
		}

		[Fact]
		public async Task GetOnlyIndexer_OnGetChain_ShouldStayOnGetterOnlySurface()
		{
			int callCount1 = 0;
			int callCount2 = 0;
			IAccessorService sut = IAccessorService.CreateMock();
			IIndexerGetterOnlySetup<int, int> chained = sut.Mock.Setup[It.IsAny<int>()]
				.OnGet.Do(() => { callCount1++; })
				.OnGet.Do(k => { callCount2++; }).OnlyOnce();
			chained.Returns(9);

			int result = sut[1];
			_ = sut[2];
			_ = sut[3];
			_ = sut[4];

			await That(result).IsEqualTo(9);
			await That(callCount1).IsEqualTo(3);
			await That(callCount2).IsEqualTo(1);
		}

		[Fact]
		public async Task GetOnlyIndexer_OnGetWhen_ShouldOnlyInvokeCallbackWhenPredicateIsTrue()
		{
			int callCount = 0;
			IAccessorService sut = IAccessorService.CreateMock();
			sut.Mock.Setup[It.IsAny<int>()]
				.OnGet.Do(() => { callCount++; }).When(i => i > 0);

			_ = sut[1];
			_ = sut[1];
			_ = sut[1];

			await That(callCount).IsEqualTo(2);
		}

		[Fact]
		public async Task GetOnlyIndexer_OnGetInParallel_ShouldInvokeParallelCallbacksAlways()
		{
			int callCount1 = 0;
			int callCount2 = 0;
			int callCount3 = 0;
			IAccessorService sut = IAccessorService.CreateMock();
			sut.Mock.Setup[It.IsAny<int>()]
				.OnGet.Do(() => { callCount1++; })
				.OnGet.Do(k => { callCount2++; }).InParallel()
				.OnGet.Do((k, v) => { callCount3++; });

			_ = sut[1];
			_ = sut[2];
			_ = sut[3];
			_ = sut[4];

			await That(callCount1).IsEqualTo(2);
			await That(callCount2).IsEqualTo(4);
			await That(callCount3).IsEqualTo(2);
		}

		[Fact]
		public async Task GetOnlyIndexer_OnGetFor_ShouldRepeatCallbackTheGivenNumberOfTimes()
		{
			int callCount1 = 0;
			int callCount2 = 0;
			IAccessorService sut = IAccessorService.CreateMock();
			sut.Mock.Setup[It.IsAny<int>()]
				.OnGet.Do(() => { callCount1++; }).For(2)
				.OnGet.Do(() => { callCount2++; });

			for (int i = 0; i < 6; i++)
			{
				_ = sut[1];
			}

			await That(callCount1).IsEqualTo(4);
			await That(callCount2).IsEqualTo(2);
		}

		[Fact]
		public async Task GetOnlyIndexer_OnGetTransitionTo_ShouldSwitchScenario()
		{
			IAccessorService sut = IAccessorService.CreateMock();
			sut.Mock.InScenario("a").Setup[It.IsAny<int>()]
				.OnGet.Do(() => { })
				.OnGet.TransitionTo("b");
			sut.Mock.TransitionTo("a");

			_ = sut[1];

			await That(((IMock)sut).MockRegistry.Scenario).IsEqualTo("b");
		}

		[Fact]
		public async Task SetOnlyIndexer_OnSetChain_ShouldStayOnSetterOnlySurface()
		{
			int callCount1 = 0;
			int callCount2 = 0;
			IAccessorService sut = IAccessorService.CreateMock();
			IIndexerSetterOnlySetup<string, string> chained = sut.Mock.Setup[It.IsAny<string>()]
				.OnSet.Do(() => { callCount1++; })
				.OnSet.Do(v => { callCount2++; }).OnlyOnce();
			chained.SkippingBaseClass();

			sut["k"] = "a";
			sut["k"] = "b";
			sut["k"] = "c";
			sut["k"] = "d";

			await That(callCount1).IsEqualTo(3);
			await That(callCount2).IsEqualTo(1);
		}

		[Fact]
		public async Task SetOnlyIndexer_OnSetWhen_ShouldOnlyInvokeCallbackWhenPredicateIsTrue()
		{
			int callCount = 0;
			IAccessorService sut = IAccessorService.CreateMock();
			sut.Mock.Setup[It.IsAny<string>()]
				.OnSet.Do(() => { callCount++; }).When(i => i > 0);

			sut["k"] = "a";
			sut["k"] = "b";
			sut["k"] = "c";

			await That(callCount).IsEqualTo(2);
		}

		[Fact]
		public async Task SetOnlyIndexer_OnSetInParallel_ShouldInvokeParallelCallbacksAlways()
		{
			int callCount1 = 0;
			int callCount2 = 0;
			int callCount3 = 0;
			IAccessorService sut = IAccessorService.CreateMock();
			sut.Mock.Setup[It.IsAny<string>()]
				.OnSet.Do(() => { callCount1++; })
				.OnSet.Do((k, v) => { callCount2++; }).InParallel()
				.OnSet.Do(() => { callCount3++; });

			sut["k"] = "a";
			sut["k"] = "b";
			sut["k"] = "c";
			sut["k"] = "d";

			await That(callCount1).IsEqualTo(2);
			await That(callCount2).IsEqualTo(4);
			await That(callCount3).IsEqualTo(2);
		}

		[Fact]
		public async Task SetOnlyIndexer_OnSetFor_ShouldRepeatCallbackTheGivenNumberOfTimes()
		{
			int callCount1 = 0;
			int callCount2 = 0;
			IAccessorService sut = IAccessorService.CreateMock();
			sut.Mock.Setup[It.IsAny<string>()]
				.OnSet.Do(() => { callCount1++; }).For(2)
				.OnSet.Do(() => { callCount2++; });

			for (int i = 0; i < 6; i++)
			{
				sut["k"] = "a";
			}

			await That(callCount1).IsEqualTo(4);
			await That(callCount2).IsEqualTo(2);
		}

		[Fact]
		public async Task SetOnlyIndexer_OnSetTransitionTo_ShouldSwitchScenario()
		{
			IAccessorService sut = IAccessorService.CreateMock();
			sut.Mock.InScenario("a").Setup[It.IsAny<string>()]
				.OnSet.Do(() => { })
				.OnSet.TransitionTo("b");
			sut.Mock.TransitionTo("a");

			sut["k"] = "x";

			await That(((IMock)sut).MockRegistry.Scenario).IsEqualTo("b");
		}

		[Fact]
		public async Task GetOnlyIndexer_Got_ShouldMatchOnlyMatchingKeys()
		{
			IAccessorService sut = IAccessorService.CreateMock();

			_ = sut[1];

			await That(sut.Mock.Verify[It.Is(1)].Got()).Once();
			await That(sut.Mock.Verify[It.Is(2)].Got()).Never();
		}

		[Fact]
		public async Task GetOnlyTwoKeyIndexer_ReturnsAndGot_ShouldUseTheNarrowedSurface()
		{
			IAccessorService sut = IAccessorService.CreateMock();
			sut.Mock.Setup[It.IsAny<int>(), It.IsAny<int>()]
				.Returns((k1, k2) => k1 + k2);

			int result = sut[20, 22];

			await That(result).IsEqualTo(42);
			await That(sut.Mock.Verify[It.Is(20), It.Is(22)].Got()).Once();
			await That(sut.Mock.Verify[It.Is(20), It.Is(23)].Got()).Never();
		}

		[Fact]
		public async Task SetOnlyTwoKeyIndexer_OnSetAndVerify_ShouldUseTheNarrowedSurface()
		{
			List<string> written = new();
			IAccessorService sut = IAccessorService.CreateMock();
			sut.Mock.Setup[It.IsAny<string>(), It.IsAny<string>()]
				.OnSet.Do((k1, k2, v) => written.Add($"{k1}-{k2}-{v}"));

			sut["a", "b"] = "c";

			await That(written).IsEqualTo(["a-b-c",]);
			await That(sut.Mock.Verify[It.Is("a"), It.Is("b")].Set("c")).Once();
			await That(sut.Mock.Verify[It.Is("a"), It.Is("b")].Set("d")).Never();
		}

		[Fact]
		public async Task SetOnlyFourKeyIndexer_Set_ShouldVerifyTheWrite()
		{
			IAccessorService sut = IAccessorService.CreateMock();

			sut[1, 2, 3, 4] = "x";

			await That(sut.Mock.Verify[It.Is(1), It.Is(2), It.Is(3), It.Is(4)].Set("x")).Once();
			await That(sut.Mock.Verify[It.Is(1), It.Is(2), It.Is(3), It.Is(5)].Set(It.IsAny<string>())).Never();
		}

		[Fact]
		public async Task GetOnlyFiveKeyIndexer_ChainedReturns_ShouldStayOnGetterOnlySurface()
		{
			IAccessorService sut = IAccessorService.CreateMock();
			IIndexerGetterOnlySetup<int, int, int, int, int, int> chained = sut.Mock
				.Setup[It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()]
				.Returns(1).OnlyOnce();
			chained.Returns((k1, k2, k3, k4, k5) => k1 + k2 + k3 + k4 + k5);

			await That(sut[1, 2, 3, 4, 5]).IsEqualTo(1);
			await That(sut[1, 2, 3, 4, 5]).IsEqualTo(15);
			await That(sut.Mock.Verify[It.Is(1), It.Is(2), It.Is(3), It.Is(4), It.Is(5)].Got()).Exactly(2)
				.Because("the narrowed surface is generated per-compilation for indexers with more than four keys");
			await That(sut.Mock.Verify[It.Is(1), It.Is(2), It.Is(3), It.Is(4), It.Is(6)].Got()).Never();
		}

		[Fact]
		public async Task GetOnlyFiveKeyIndexer_ReturnsForever_ShouldUseTheLastValueForever()
		{
			IAccessorService sut = IAccessorService.CreateMock();
			sut.Mock.Setup[It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()]
				.Returns(2)
				.Returns(4).Forever();

			int[] result = new int[4];
			for (int i = 0; i < 4; i++)
			{
				result[i] = sut[i, i, i, i, i];
			}

			await That(result).IsEqualTo([2, 4, 4, 4,]);
		}

		[Fact]
		public async Task SetOnlyFiveKeyIndexer_OnSetAndVerify_ShouldUseTheNarrowedSurface()
		{
			List<string> written = new();
			IAccessorService sut = IAccessorService.CreateMock();
			sut.Mock
				.Setup[It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()]
				.OnSet.Do((k1, k2, k3, k4, k5, v) => written.Add($"{k1}{k2}{k3}{k4}{k5}-{v}"));

			sut["a", "b", "c", "d", "e"] = "x";

			await That(written).IsEqualTo(["abcde-x",]);
			await That(sut.Mock.Verify[It.Is("a"), It.Is("b"), It.Is("c"), It.Is("d"), It.Is("e")].Set("x")).Once();
			await That(sut.Mock.Verify[It.Is("a"), It.Is("b"), It.Is("c"), It.Is("d"), It.Is("e")].Set("y")).Never();
		}

		public interface IAccessorService
		{
			int this[int key] { get; }
			string this[string key] { set; }
			int this[int key1, int key2] { get; }
			string this[string key1, string key2] { set; }
			string this[int key1, int key2, int key3, int key4] { set; }
			int this[int key1, int key2, int key3, int key4, int key5] { get; }
			string this[string key1, string key2, string key3, string key4, string key5] { set; }
		}

		public class AccessorService
		{
			public int SetterCallCount { get; private set; }

			public virtual int this[int key]
			{
				set => SetterCallCount += value;
			}
		}
	}
}
