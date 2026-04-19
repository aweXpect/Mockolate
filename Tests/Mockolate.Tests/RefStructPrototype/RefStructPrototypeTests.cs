#if NET9_0_OR_GREATER
using System;
using System.Linq;
using Mockolate.Interactions;
using Mockolate.Parameters;
using Mockolate.Setup;

namespace Mockolate.Tests.RefStructPrototype;

public sealed class RefStructPrototypeTests
{
	public sealed class MatcherTests
	{
		[Fact]
		public async Task IsAnyRefStruct_ShouldMatchAnyValue()
		{
			IParameter<DataPacket> sut = It.IsAnyRefStruct<DataPacket>();

			bool result = ((IParameterMatch<DataPacket>)sut).Matches(new DataPacket(1, []));

			await That(result).IsTrue();
		}

		[Fact]
		public async Task IsAnyRefStruct_ToString_ShouldIncludeTypeName()
		{
			IParameter<DataPacket> sut = It.IsAnyRefStruct<DataPacket>();

			string? result = sut.ToString();

			await That(result).IsEqualTo("It.IsAnyRefStruct<DataPacket>()");
		}

		[Fact]
		public async Task IsRefStruct_WithPredicate_ShouldFilterById()
		{
			IParameter<DataPacket> sut = It.IsRefStruct<DataPacket>(p => p.Id > 100);
			IParameterMatch<DataPacket> match = (IParameterMatch<DataPacket>)sut;

			bool high = match.Matches(new DataPacket(500, []));
			bool low = match.Matches(new DataPacket(1, []));

			await That(high).IsTrue();
			await That(low).IsFalse();
		}

		[Fact]
		public async Task IsRefStruct_PredicateSeesSpanPayload()
		{
			// Demonstrates the predicate can read into the ref struct's inline Span — which is the
			// whole reason ref-struct parameters exist. A non-ref-struct matcher could never do this
			// because the Span couldn't flow through the parameter type.
			IParameter<DataPacket> sut = It.IsRefStruct<DataPacket>(p =>
				p.Payload.Length > 0 && p.Payload[0] == 0xFF);
			IParameterMatch<DataPacket> match = (IParameterMatch<DataPacket>)sut;

			byte[] hit = [0xFF, 0x01];
			byte[] miss = [0x00, 0x01];

			bool matchedHit = match.Matches(new DataPacket(1, hit));
			bool matchedMiss = match.Matches(new DataPacket(1, miss));

			await That(matchedHit).IsTrue();
			await That(matchedMiss).IsFalse();
		}

		[Fact]
		public async Task IsAnyRefStruct_NonGenericMatches_ReturnsFalseForBoxedSlot()
		{
			// The non-generic IParameter.Matches(object?) is the covariance-safe fallback and is
			// deliberately inert for ref-struct matchers: the boxed path can never be a ref struct.
			IParameter sut = It.IsAnyRefStruct<DataPacket>();

			bool result = sut.Matches(null);

			await That(result).IsFalse();
		}
	}

	public sealed class SetupTests
	{
		[Fact]
		public async Task Consume_WithoutSetup_ShouldRunWithoutThrowing()
		{
			PacketSinkMock mock = new(MockBehavior.Default);
			IPacketSink sut = mock;

			sut.Consume(new DataPacket(1, []));

			await That(mock.Verify.ConsumeCount).IsEqualTo(1);
		}

		[Fact]
		public async Task Consume_WithAnyMatcher_Throws_ShouldThrowConfiguredException()
		{
			PacketSinkMock mock = new(MockBehavior.Default);
			IPacketSink sut = mock;

			mock.Setup.Consume(It.IsAnyRefStruct<DataPacket>())
				.Throws<InvalidOperationException>();

			void Act() => sut.Consume(new DataPacket(1, []));

			await That(Act).Throws<InvalidOperationException>();
		}

		[Fact]
		public async Task Consume_ThrowsExceptionInstance_ShouldThrowSameInstance()
		{
			PacketSinkMock mock = new(MockBehavior.Default);
			IPacketSink sut = mock;

			InvalidOperationException expected = new("boom");
			mock.Setup.Consume(It.IsAnyRefStruct<DataPacket>()).Throws(expected);

			try
			{
				sut.Consume(new DataPacket(1, []));
				await That(false).IsTrue().Because("expected throw");
			}
			catch (InvalidOperationException actual)
			{
				await That(actual).IsSameAs(expected);
			}
		}

		[Fact]
		public async Task Consume_ThrowsFactory_ShouldInvokeFactoryPerCall()
		{
			PacketSinkMock mock = new(MockBehavior.Default);
			IPacketSink sut = mock;

			int factoryCalls = 0;
			mock.Setup.Consume(It.IsRefStruct<DataPacket>(d => d.Id == 1))
				.Throws(() =>
				{
					factoryCalls++;
					return new InvalidOperationException($"call #{factoryCalls}");
				});

			try { sut.Consume(new DataPacket(1, [])); } catch (InvalidOperationException) { }
			try { sut.Consume(new DataPacket(2, [])); } catch (InvalidOperationException) { }

			await That(factoryCalls).IsEqualTo(1);
		}

		[Fact]
		public async Task Consume_PredicateMatches_ShouldThrowOnlyWhenPredicateHolds()
		{
			PacketSinkMock mock = new(MockBehavior.Default);
			IPacketSink sut = mock;

			mock.Setup.Consume(It.IsRefStruct<DataPacket>(p => p.Id == 42))
				.Throws<InvalidOperationException>();

			void ActMatch() => sut.Consume(new DataPacket(42, []));
			void ActMiss() => sut.Consume(new DataPacket(7, []));

			await That(ActMatch).Throws<InvalidOperationException>();
			await That(ActMiss).DoesNotThrow();
		}

		[Fact]
		public async Task Consume_MultipleSetups_LatestMatchingWins()
		{
			PacketSinkMock mock = new(MockBehavior.Default);
			IPacketSink sut = mock;

			mock.Setup.Consume(It.IsAnyRefStruct<DataPacket>())
				.Throws<InvalidOperationException>();
			mock.Setup.Consume(It.IsRefStruct<DataPacket>(p => p.Id == 42))
				.Throws<NotSupportedException>();

			void ActSpecific() => sut.Consume(new DataPacket(42, []));
			void ActFallback() => sut.Consume(new DataPacket(1, []));

			// Registered last with a matching predicate → wins over the any-matcher above.
			await That(ActSpecific).Throws<NotSupportedException>();
			// Doesn't match the predicate → falls through to the earlier any-matcher.
			await That(ActFallback).Throws<InvalidOperationException>();
		}

		[Fact]
		public async Task Consume_DoesNotThrow_OverridesEarlierThrows()
		{
			PacketSinkMock mock = new(MockBehavior.Default);
			IPacketSink sut = mock;

			IRefStructVoidMethodSetup<DataPacket> setup = mock.Setup.Consume(
				It.IsAnyRefStruct<DataPacket>());
			setup.Throws<InvalidOperationException>();
			setup.DoesNotThrow();

			sut.Consume(new DataPacket(1, []));

			await That(mock.Verify.ConsumeCount).IsEqualTo(1);
		}

		[Fact]
		public async Task Consume_Setup_InspectsSpanPayloadAtCallTime()
		{
			PacketSinkMock mock = new(MockBehavior.Default);
			IPacketSink sut = mock;

			mock.Setup.Consume(It.IsRefStruct<DataPacket>(p =>
				p.Payload.Length == 3 && p.Payload[0] == 0xFF))
				.Throws<InvalidOperationException>();

			byte[] hit = [0xFF, 0x00, 0x01];
			byte[] miss = [0x00, 0x00, 0x01];

			void ActHit() => sut.Consume(new DataPacket(1, hit));
			void ActMiss() => sut.Consume(new DataPacket(2, miss));

			await That(ActHit).Throws<InvalidOperationException>();
			await That(ActMiss).DoesNotThrow();
		}
	}

	public sealed class VerifyTests
	{
		[Fact]
		public async Task ConsumeCount_ReflectsEveryInvocation()
		{
			PacketSinkMock mock = new(MockBehavior.Default);
			IPacketSink sut = mock;

			sut.Consume(new DataPacket(1, []));
			sut.Consume(new DataPacket(2, []));
			sut.Consume(new DataPacket(3, []));

			await That(mock.Verify.ConsumeCount).IsEqualTo(3);
		}

		[Fact]
		public async Task RecordedInteraction_IsRefStructMethodInvocation_WithoutParameterValue()
		{
			PacketSinkMock mock = new(MockBehavior.Default);
			IPacketSink sut = mock;

			sut.Consume(new DataPacket(999, []));

			RefStructMethodInvocation? recorded =
				mock.Registry.Interactions.OfType<RefStructMethodInvocation>().SingleOrDefault();

			await That(recorded).IsNotNull();
			await That(recorded!.Name).IsEqualTo("Consume");
			await That(recorded.ParameterNames.Single()).IsEqualTo("packet");
			// The ref struct value is NOT on the recorded interaction — post-hoc verify can only
			// count and filter by name. Matching against the value requires setup-time matchers.
		}

		[Fact]
		public async Task RecordedInteraction_ToString_ShowsRefStructPlaceholder()
		{
			PacketSinkMock mock = new(MockBehavior.Default);
			IPacketSink sut = mock;

			sut.Consume(new DataPacket(1, []));

			RefStructMethodInvocation recorded =
				mock.Registry.Interactions.OfType<RefStructMethodInvocation>().Single();

			string? rendered = recorded.ToString();

			await That(rendered).IsEqualTo("invoke method Consume(packet: <ref struct>)");
		}

		[Fact]
		public async Task MultiArityInteraction_RendersAllParameterNames()
		{
			RefStructMethodInvocation invocation = new("Encode", "left", "right", "scratch");

			string? rendered = invocation.ToString();

			await That(rendered).IsEqualTo(
				"invoke method Encode(left: <ref struct>, right: <ref struct>, scratch: <ref struct>)");
		}
	}

	public sealed class MultiAritySetupTests
	{
		[Fact]
		public async Task Arity2_Matches_AllMatchersMustAccept()
		{
			RefStructVoidMethodSetup<DataPacket, DataPacket> setup = new(
				"Encode",
				It.IsRefStruct<DataPacket>(p => p.Id == 1) as IParameterMatch<DataPacket>,
				It.IsRefStruct<DataPacket>(p => p.Id == 2) as IParameterMatch<DataPacket>);

			bool bothMatch = setup.Matches(new DataPacket(1, []), new DataPacket(2, []));
			bool firstOnly = setup.Matches(new DataPacket(1, []), new DataPacket(99, []));
			bool secondOnly = setup.Matches(new DataPacket(99, []), new DataPacket(2, []));

			await That(bothMatch).IsTrue();
			await That(firstOnly).IsFalse();
			await That(secondOnly).IsFalse();
		}

		[Fact]
		public async Task Arity2_Invoke_ThrowsConfiguredException()
		{
			RefStructVoidMethodSetup<DataPacket, DataPacket> setup = new("Encode");
			((IRefStructVoidMethodSetup<DataPacket, DataPacket>)setup)
				.Throws<InvalidOperationException>();

			void Act() => setup.Invoke(new DataPacket(1, []), new DataPacket(2, []));

			await That(Act).Throws<InvalidOperationException>();
		}

		[Fact]
		public async Task Arity3_Matches_AllMatchersMustAccept()
		{
			RefStructVoidMethodSetup<DataPacket, DataPacket, DataPacket> setup = new(
				"Fold",
				It.IsRefStruct<DataPacket>(p => p.Id == 1) as IParameterMatch<DataPacket>,
				null,
				It.IsRefStruct<DataPacket>(p => p.Id == 3) as IParameterMatch<DataPacket>);

			bool match = setup.Matches(
				new DataPacket(1, []), new DataPacket(99, []), new DataPacket(3, []));
			bool miss = setup.Matches(
				new DataPacket(2, []), new DataPacket(99, []), new DataPacket(3, []));

			await That(match).IsTrue();
			await That(miss).IsFalse();
		}

		[Fact]
		public async Task Arity4_Invoke_ThrowsFromFactory()
		{
			RefStructVoidMethodSetup<DataPacket, DataPacket, DataPacket, DataPacket> setup =
				new("Merge");
			((IRefStructVoidMethodSetup<DataPacket, DataPacket, DataPacket, DataPacket>)setup)
				.Throws(() => new NotSupportedException("arity-4"));

			void Act() => setup.Invoke(
				new DataPacket(1, []), new DataPacket(2, []),
				new DataPacket(3, []), new DataPacket(4, []));

			await That(Act).Throws<NotSupportedException>().WithMessage("arity-4");
		}
	}

	public sealed class ReturnSetupTests
	{
		[Fact]
		public async Task Arity1_Returns_Value_ReturnsConfiguredValue()
		{
			RefStructReturnMethodSetup<int, DataPacket> setup = new(
				"TryParse", It.IsAnyRefStruct<DataPacket>() as IParameterMatch<DataPacket>);
			((IRefStructReturnMethodSetup<int, DataPacket>)setup).Returns(42);

			int result = setup.Invoke(new DataPacket(1, []));

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task Arity1_Returns_Factory_InvokedPerCall()
		{
			int calls = 0;
			RefStructReturnMethodSetup<int, DataPacket> setup = new("TryParse");
			((IRefStructReturnMethodSetup<int, DataPacket>)setup)
				.Returns(() => ++calls);

			int first = setup.Invoke(new DataPacket(1, []));
			int second = setup.Invoke(new DataPacket(2, []));

			await That(first).IsEqualTo(1);
			await That(second).IsEqualTo(2);
		}

		[Fact]
		public async Task Arity1_NoReturnConfigured_UsesDefaultFactory()
		{
			RefStructReturnMethodSetup<string, DataPacket> setup = new("Decode");

			string result = setup.Invoke(new DataPacket(1, []), static () => "fallback");

			await That(result).IsEqualTo("fallback");
		}

		[Fact]
		public async Task Arity1_Throws_BeatsReturn_WhenBothConfigured()
		{
			RefStructReturnMethodSetup<int, DataPacket> setup = new("TryParse");
			((IRefStructReturnMethodSetup<int, DataPacket>)setup)
				.Returns(42)
				.Throws<InvalidOperationException>();

			void Act() => setup.Invoke(new DataPacket(1, []));

			await That(Act).Throws<InvalidOperationException>();
		}

		[Fact]
		public async Task Arity1_HasReturnValue_ReflectsConfiguration()
		{
			RefStructReturnMethodSetup<int, DataPacket> setup = new("TryParse");
			bool before = setup.HasReturnValue;

			((IRefStructReturnMethodSetup<int, DataPacket>)setup).Returns(7);

			await That(before).IsFalse();
			await That(setup.HasReturnValue).IsTrue();
		}

		[Fact]
		public async Task Arity2_Matches_AllMatchersMustAccept()
		{
			RefStructReturnMethodSetup<bool, DataPacket, DataPacket> setup = new(
				"Compare",
				It.IsRefStruct<DataPacket>(p => p.Id == 1) as IParameterMatch<DataPacket>,
				It.IsRefStruct<DataPacket>(p => p.Id == 2) as IParameterMatch<DataPacket>);
			((IRefStructReturnMethodSetup<bool, DataPacket, DataPacket>)setup).Returns(true);

			bool match = setup.Matches(new DataPacket(1, []), new DataPacket(2, []));
			bool miss = setup.Matches(new DataPacket(1, []), new DataPacket(99, []));

			await That(match).IsTrue();
			await That(miss).IsFalse();
		}

		[Fact]
		public async Task Arity3_Invoke_ReturnsConfiguredValue()
		{
			RefStructReturnMethodSetup<string, DataPacket, DataPacket, DataPacket> setup =
				new("Blend");
			((IRefStructReturnMethodSetup<string, DataPacket, DataPacket, DataPacket>)setup)
				.Returns("blended");

			string result = setup.Invoke(
				new DataPacket(1, []), new DataPacket(2, []), new DataPacket(3, []));

			await That(result).IsEqualTo("blended");
		}

		[Fact]
		public async Task Arity4_Invoke_RunsThrowOverReturn()
		{
			RefStructReturnMethodSetup<int, DataPacket, DataPacket, DataPacket, DataPacket> setup =
				new("Combine");
			((IRefStructReturnMethodSetup<int, DataPacket, DataPacket, DataPacket, DataPacket>)setup)
				.Returns(99)
				.Throws(new NotSupportedException());

			void Act() => setup.Invoke(
				new DataPacket(1, []), new DataPacket(2, []),
				new DataPacket(3, []), new DataPacket(4, []));

			await That(Act).Throws<NotSupportedException>();
		}
	}
}
#endif
