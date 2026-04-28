using Mockolate.Exceptions;
using Mockolate.Parameters;
using Mockolate.Verify;

namespace Mockolate.Tests.Verify;

public class MockRegistryVerifyTests
{
	public sealed class FailureMessageTests
	{
		[Fact]
		public async Task IndexerGotTyped_FourKeys_FailureMessageIncludesGotIndexerPrefix()
		{
			FastMockInteractions store = new(1);
			store.InstallIndexerGetter<int, string, bool, double>(0);
			MockRegistry registry = new(MockBehavior.Default, store);

			void Act()
			{
				registry.IndexerGotTyped(new object(), 0,
					(IParameterMatch<int>)It.Is(1),
					(IParameterMatch<string>)It.Is<string>("a"),
					(IParameterMatch<bool>)It.Is(true),
					(IParameterMatch<double>)It.Is(1.0),
					() => "[1, \"a\", true, 1.0]").Once();
			}

			await That(Act).Throws<MockVerificationException>()
				.WithMessage("*got indexer [1, \"a\", true, 1.0]*").AsWildcard();
		}

		[Fact]
		public async Task IndexerGotTyped_OneKey_FailureMessageIncludesGotIndexerPrefix()
		{
			FastMockInteractions store = new(1);
			store.InstallIndexerGetter<int>(0);
			MockRegistry registry = new(MockBehavior.Default, store);

			void Act()
			{
				registry.IndexerGotTyped(new object(), 0,
					(IParameterMatch<int>)It.Is(1),
					() => "[1]").Once();
			}

			await That(Act).Throws<MockVerificationException>()
				.WithMessage("*got indexer [1]*").AsWildcard();
		}

		[Fact]
		public async Task IndexerGotTyped_ThreeKeys_FailureMessageIncludesGotIndexerPrefix()
		{
			FastMockInteractions store = new(1);
			store.InstallIndexerGetter<int, string, bool>(0);
			MockRegistry registry = new(MockBehavior.Default, store);

			void Act()
			{
				registry.IndexerGotTyped(new object(), 0,
					(IParameterMatch<int>)It.Is(1),
					(IParameterMatch<string>)It.Is<string>("a"),
					(IParameterMatch<bool>)It.Is(true),
					() => "[1, \"a\", true]").Once();
			}

			await That(Act).Throws<MockVerificationException>()
				.WithMessage("*got indexer [1, \"a\", true]*").AsWildcard();
		}

		[Fact]
		public async Task IndexerGotTyped_TwoKeys_FailureMessageIncludesGotIndexerPrefix()
		{
			FastMockInteractions store = new(1);
			store.InstallIndexerGetter<int, string>(0);
			MockRegistry registry = new(MockBehavior.Default, store);

			void Act()
			{
				registry.IndexerGotTyped(new object(), 0,
					(IParameterMatch<int>)It.Is(1),
					(IParameterMatch<string>)It.Is<string>("a"),
					() => "[1, \"a\"]").Once();
			}

			await That(Act).Throws<MockVerificationException>()
				.WithMessage("*got indexer [1, \"a\"]*").AsWildcard();
		}

		[Fact]
		public async Task IndexerSetTyped_FourKeys_FailureMessageIncludesSetIndexerPrefix()
		{
			FastMockInteractions store = new(1);
			store.InstallIndexerSetter<int, string, bool, double, char>(0);
			MockRegistry registry = new(MockBehavior.Default, store);

			void Act()
			{
				registry.IndexerSetTyped(new object(), 0,
					(IParameterMatch<int>)It.Is(1),
					(IParameterMatch<string>)It.Is<string>("a"),
					(IParameterMatch<bool>)It.Is(true),
					(IParameterMatch<double>)It.Is(1.0),
					(IParameterMatch<char>)It.Is('z'),
					() => "[1, \"a\", true, 1.0]").Once();
			}

			await That(Act).Throws<MockVerificationException>()
				.WithMessage("*set indexer [1, \"a\", true, 1.0] to *z*").AsWildcard();
		}

		[Fact]
		public async Task IndexerSetTyped_OneKey_FailureMessageIncludesSetIndexerPrefix()
		{
			FastMockInteractions store = new(1);
			store.InstallIndexerSetter<int, string>(0);
			MockRegistry registry = new(MockBehavior.Default, store);

			void Act()
			{
				registry.IndexerSetTyped(new object(), 0,
					(IParameterMatch<int>)It.Is(1),
					(IParameterMatch<string>)It.Is<string>("a"),
					() => "[1]").Once();
			}

			await That(Act).Throws<MockVerificationException>()
				.WithMessage("*set indexer [1] to *a*").AsWildcard();
		}

		[Fact]
		public async Task IndexerSetTyped_ThreeKeys_FailureMessageIncludesSetIndexerPrefix()
		{
			FastMockInteractions store = new(1);
			store.InstallIndexerSetter<int, string, bool, double>(0);
			MockRegistry registry = new(MockBehavior.Default, store);

			void Act()
			{
				registry.IndexerSetTyped(new object(), 0,
					(IParameterMatch<int>)It.Is(1),
					(IParameterMatch<string>)It.Is<string>("a"),
					(IParameterMatch<bool>)It.Is(true),
					(IParameterMatch<double>)It.Is(1.0),
					() => "[1, \"a\", true]").Once();
			}

			await That(Act).Throws<MockVerificationException>()
				.WithMessage("*set indexer [1, \"a\", true] to *1*").AsWildcard();
		}

		[Fact]
		public async Task IndexerSetTyped_TwoKeys_FailureMessageIncludesSetIndexerPrefix()
		{
			FastMockInteractions store = new(1);
			store.InstallIndexerSetter<int, string, bool>(0);
			MockRegistry registry = new(MockBehavior.Default, store);

			void Act()
			{
				registry.IndexerSetTyped(new object(), 0,
					(IParameterMatch<int>)It.Is(1),
					(IParameterMatch<string>)It.Is<string>("a"),
					(IParameterMatch<bool>)It.Is(true),
					() => "[1, \"a\"]").Once();
			}

			await That(Act).Throws<MockVerificationException>()
				.WithMessage("*set indexer [1, \"a\"] to *true*").AsWildcard();
		}

		[Fact]
		public async Task SubscribedToTyped_FailureMessageIncludesSubscribedToEventPrefix()
		{
			FastMockInteractions store = new(1);
			store.InstallEventSubscribe(0);
			MockRegistry registry = new(MockBehavior.Default, store);

			void Act()
			{
				registry.SubscribedToTyped(new object(), 0, "Tick").Once();
			}

			await That(Act).Throws<MockVerificationException>()
				.WithMessage("*subscribed to event Tick*").AsWildcard();
		}

		[Fact]
		public async Task UnsubscribedFromTyped_FailureMessageIncludesUnsubscribedFromEventPrefix()
		{
			FastMockInteractions store = new(1);
			store.InstallEventUnsubscribe(0);
			MockRegistry registry = new(MockBehavior.Default, store);

			void Act()
			{
				registry.UnsubscribedFromTyped(new object(), 0, "Tick").Once();
			}

			await That(Act).Throws<MockVerificationException>()
				.WithMessage("*unsubscribed from event Tick*").AsWildcard();
		}

		[Fact]
		public async Task VerifyMethodTyped_FourParameters_FailureMessageIncludesInvokedMethodPrefix()
		{
			FastMockInteractions store = new(1);
			store.InstallMethod<int, string, bool, double>(0);
			MockRegistry registry = new(MockBehavior.Default, store);

			void Act()
			{
				registry.VerifyMethod(new object(), 0, "Foo",
					(IParameterMatch<int>)It.Is(1),
					(IParameterMatch<string>)It.Is<string>("a"),
					(IParameterMatch<bool>)It.Is(true),
					(IParameterMatch<double>)It.Is(1.0),
					() => "Foo(1, \"a\", true, 1.0)").Once();
			}

			await That(Act).Throws<MockVerificationException>()
				.WithMessage("*invoked method Foo(1, \"a\", true, 1.0)*").AsWildcard();
		}

		[Fact]
		public async Task VerifyMethodTyped_OneParameter_FailureMessageIncludesInvokedMethodPrefix()
		{
			FastMockInteractions store = new(1);
			store.InstallMethod<int>(0);
			MockRegistry registry = new(MockBehavior.Default, store);

			void Act()
			{
				registry.VerifyMethod(new object(), 0, "Foo",
					(IParameterMatch<int>)It.Is(1),
					() => "Foo(1)").Once();
			}

			await That(Act).Throws<MockVerificationException>()
				.WithMessage("*invoked method Foo(1)*").AsWildcard();
		}

		[Fact]
		public async Task VerifyMethodTyped_Parameterless_FailureMessageIncludesInvokedMethodPrefix()
		{
			FastMockInteractions store = new(1);
			store.InstallMethod(0);
			MockRegistry registry = new(MockBehavior.Default, store);

			void Act()
			{
				registry.VerifyMethod(new object(), 0, "Foo", () => "Foo()").Once();
			}

			await That(Act).Throws<MockVerificationException>()
				.WithMessage("*invoked method Foo()*").AsWildcard();
		}

		[Fact]
		public async Task VerifyMethodTyped_ThreeParameters_FailureMessageIncludesInvokedMethodPrefix()
		{
			FastMockInteractions store = new(1);
			store.InstallMethod<int, string, bool>(0);
			MockRegistry registry = new(MockBehavior.Default, store);

			void Act()
			{
				registry.VerifyMethod(new object(), 0, "Foo",
					(IParameterMatch<int>)It.Is(1),
					(IParameterMatch<string>)It.Is<string>("a"),
					(IParameterMatch<bool>)It.Is(true),
					() => "Foo(1, \"a\", true)").Once();
			}

			await That(Act).Throws<MockVerificationException>()
				.WithMessage("*invoked method Foo(1, \"a\", true)*").AsWildcard();
		}

		[Fact]
		public async Task VerifyMethodTyped_TwoParameters_FailureMessageIncludesInvokedMethodPrefix()
		{
			FastMockInteractions store = new(1);
			store.InstallMethod<int, string>(0);
			MockRegistry registry = new(MockBehavior.Default, store);

			void Act()
			{
				registry.VerifyMethod(new object(), 0, "Foo",
					(IParameterMatch<int>)It.Is(1),
					(IParameterMatch<string>)It.Is<string>("a"),
					() => "Foo(1, \"a\")").Once();
			}

			await That(Act).Throws<MockVerificationException>()
				.WithMessage("*invoked method Foo(1, \"a\")*").AsWildcard();
		}

		[Fact]
		public async Task VerifyPropertyTyped_Getter_FailureMessageIncludesGotPropertyPrefix()
		{
			FastMockInteractions store = new(1);
			store.InstallPropertyGetter(0);
			MockRegistry registry = new(MockBehavior.Default, store);

			void Act()
			{
				registry.VerifyPropertyTyped(new object(), 0, "X").Once();
			}

			await That(Act).Throws<MockVerificationException>()
				.WithMessage("*got property X*").AsWildcard();
		}

		[Fact]
		public async Task VerifyPropertyTyped_Setter_FailureMessageIncludesSetPropertyPrefix()
		{
			FastMockInteractions store = new(1);
			store.InstallPropertySetter<int>(0);
			MockRegistry registry = new(MockBehavior.Default, store);

			void Act()
			{
				registry.VerifyPropertyTyped(new object(), 0, "X",
					(IParameterMatch<int>)It.Is(7)).Once();
			}

			await That(Act).Throws<MockVerificationException>()
				.WithMessage("*set property X to *7*").AsWildcard();
		}
	}

	public sealed class TryGetBufferBoundaryTests
	{
		[Fact]
		public async Task VerifyMethodTyped_WithMemberIdEqualToBufferLength_ShouldFallToSlowPath()
		{
			// Pins the `(uint)memberId < (uint)buffers.Length` boundary in TryGetBuffer.
			// Mutation `<=` would let memberId == buffers.Length pass the bounds check and
			// IndexOutOfRangeException-crash on `buffers[memberId]`. Asserting the verify
			// resolves cleanly via the slow-path fallback documents the boundary.
			FastMockInteractions store = new(1);
			store.InstallMethod(0);
			MockRegistry registry = new(MockBehavior.Default, store);

			void Act()
			{
				registry.VerifyMethod(new object(), 1, "Foo", () => "Foo()").Never();
			}

			await That(Act).DoesNotThrow();
		}

		[Fact]
		public async Task VerifyMethodTyped_WithMemberIdFarOutOfRange_ShouldFallToSlowPath()
		{
			FastMockInteractions store = new(1);
			store.InstallMethod(0);
			MockRegistry registry = new(MockBehavior.Default, store);

			void Act()
			{
				registry.VerifyMethod(new object(), 99, "Foo", () => "Foo()").Never();
			}

			await That(Act).DoesNotThrow();
		}
	}

	public sealed class SlowPathPredicateTests
	{
		[Fact]
		public async Task VerifyMethodTyped_FourParam_SlowPath_AllMatchersTrue_Counts()
		{
			// Positive case for the same slow-path lambda — the matched record must be counted.
			FastMockInteractions store = new(0);
			MockRegistry registry = new(MockBehavior.Default, store);

			IMockInteractions interactions = store;
			interactions.RegisterInteraction(
				new MethodInvocation<int, string, bool, double>("Foo", 1, "ok", true, 1.0));

			void Act()
			{
				registry.VerifyMethod(new object(), 0, "Foo",
					(IParameterMatch<int>)It.Is(1),
					(IParameterMatch<string>)It.Is<string>("ok"),
					(IParameterMatch<bool>)It.Is(true),
					(IParameterMatch<double>)It.Is(1.0),
					() => "Foo(1, \"ok\", true, 1.0)").Once();
			}

			await That(Act).DoesNotThrow();
		}

		[Fact]
		public async Task VerifyMethodTyped_FourParam_SlowPath_RejectsWhenLastMatcherFails()
		{
			FastMockInteractions store = new(0);
			MockRegistry registry = new(MockBehavior.Default, store);

			IMockInteractions interactions = store;
			interactions.RegisterInteraction(
				new MethodInvocation<int, string, bool, double>("Foo", 1, "ok", true, 9.9));

			void Act()
			{
				registry.VerifyMethod(new object(), 0, "Foo",
					(IParameterMatch<int>)It.Is(1),
					(IParameterMatch<string>)It.Is<string>("ok"),
					(IParameterMatch<bool>)It.Is(true),
					(IParameterMatch<double>)It.Is(1.0),
					() => "Foo(1, \"ok\", true, 1.0)").Once();
			}

			await That(Act).Throws<MockVerificationException>();
		}

		[Fact]
		public async Task VerifyMethodTyped_ThreeParam_SlowPath_RejectsWhenLastMatcherFails()
		{
			FastMockInteractions store = new(0);
			MockRegistry registry = new(MockBehavior.Default, store);

			IMockInteractions interactions = store;
			interactions.RegisterInteraction(new MethodInvocation<int, string, bool>("Foo", 1, "ok", false));

			void Act()
			{
				registry.VerifyMethod(new object(), 0, "Foo",
					(IParameterMatch<int>)It.Is(1),
					(IParameterMatch<string>)It.Is<string>("ok"),
					(IParameterMatch<bool>)It.Is(true),
					() => "Foo(1, \"ok\", true)").Once();
			}

			await That(Act).Throws<MockVerificationException>();
		}

		[Fact]
		public async Task VerifyMethodTyped_TwoParam_SlowPath_RejectsWhenFirstMatcherFails()
		{
			// The "logical mutation" survivors live in the slow-path lambda used when TryGetBuffer
			// returns null. With an out-of-range memberId we force that branch; then we record an
			// interaction that satisfies match2 but NOT match1, and verify it is correctly excluded.
			// This kills the mutation that drops the `&& match1` term from the AND-chain.
			FastMockInteractions store = new(0);
			MockRegistry registry = new(MockBehavior.Default, store);

			IMockInteractions interactions = store;
			interactions.RegisterInteraction(new MethodInvocation<int, string>("Foo", 99, "ok"));

			void Act()
			{
				registry.VerifyMethod(new object(), 0, "Foo",
					(IParameterMatch<int>)It.Is(1),
					(IParameterMatch<string>)It.Is<string>("ok"),
					() => "Foo(1, \"ok\")").Once();
			}

			await That(Act).Throws<MockVerificationException>();
		}

		[Fact]
		public async Task VerifyMethodTyped_TwoParam_SlowPath_RejectsWhenSecondMatcherFails()
		{
			FastMockInteractions store = new(0);
			MockRegistry registry = new(MockBehavior.Default, store);

			IMockInteractions interactions = store;
			interactions.RegisterInteraction(new MethodInvocation<int, string>("Foo", 1, "no"));

			void Act()
			{
				registry.VerifyMethod(new object(), 0, "Foo",
					(IParameterMatch<int>)It.Is(1),
					(IParameterMatch<string>)It.Is<string>("ok"),
					() => "Foo(1, \"ok\")").Once();
			}

			await That(Act).Throws<MockVerificationException>();
		}
	}
}
