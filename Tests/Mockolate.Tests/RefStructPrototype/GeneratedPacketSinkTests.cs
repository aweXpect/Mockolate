#if NET9_0_OR_GREATER
using System;
using System.Linq;
using Mockolate.Interactions;
using Mockolate.Parameters;
using Mockolate.Setup;

namespace Mockolate.Tests.RefStructPrototype;

/// <summary>
///     A ref struct used for end-to-end generator tests. Separate from <c>DataPacket</c> so the
///     hand-written <c>PacketSinkMock</c> prototype is not entangled with the generator-exercising
///     interfaces below.
/// </summary>
public readonly ref struct Packet(int id, ReadOnlySpan<byte> payload)
{
	public int Id { get; } = id;
	public ReadOnlySpan<byte> Payload { get; } = payload;
}

/// <summary>
///     Generator-target interface: single void method with a single ref-struct parameter. This
///     is the smallest non-trivial shape that exercises the ref-struct setup pipeline.
/// </summary>
public interface IGeneratedPacketSink
{
	void Consume(Packet packet);
}

/// <summary>
///     Generator-target: ref-struct parameter plus a non-ref-struct parameter in the same
///     signature. Proves mixed parameters route through <c>RefStructVoidMethodSetup&lt;T1, T2&gt;</c>.
/// </summary>
public interface IGeneratedPacketWriter
{
	void Write(Packet packet, int priority);
}

/// <summary>
///     Generator-target: non-void return with a ref-struct parameter. Uses
///     <c>RefStructReturnMethodSetup&lt;int, Packet&gt;</c>.
/// </summary>
public interface IGeneratedPacketParser
{
	int TryParse(Packet packet);
}

/// <summary>
///     Generator-target: indexer keyed by a ref struct. Not wired up in commit E — both
///     accessors throw <c>NotSupportedException</c>. The analyzer will flag this pattern at
///     compile time in commit F.
/// </summary>
public interface IGeneratedPacketLookup
{
	string this[Packet key] { get; }
}

/// <summary>
///     Generator-target: an arity-5 ref-struct-parameter void method. The runtime types for
///     arity 1-4 are hand-written; arity 5+ are generator-emitted into
///     <c>RefStructMethodSetups.g.cs</c>.
/// </summary>
public interface IBigPacketSink
{
	void Absorb(Packet p1, Packet p2, Packet p3, Packet p4, Packet p5);
}

/// <summary>
///     Generator-target: an arity-6 ref-struct-parameter return method that mixes Packet with
///     non-ref-struct types (int, string), verifying <c>allows ref struct</c> is satisfied by any
///     type and that return-side wiring works at extended arity.
/// </summary>
public interface IBigPacketParser
{
	int TryParse(Packet p1, Packet p2, int offset, Packet p4, Packet p5, string format);
}

public sealed class GeneratedPacketSinkTests
{
	public sealed class VoidArity1Tests
	{
		[Fact]
		public async Task SetupThrows_ShouldThrowConfiguredException()
		{
			IGeneratedPacketSink sut = IGeneratedPacketSink.CreateMock();
			sut.Mock.Setup.Consume(It.IsAnyRefStruct<Packet>())
				.Throws<InvalidOperationException>();

			void Act() => sut.Consume(new Packet(1, []));

			await That(Act).Throws<InvalidOperationException>();
		}

		[Fact]
		public async Task SetupPredicateInspectingSpanPayload_ShouldOnlyActOnMatch()
		{
			IGeneratedPacketSink sut = IGeneratedPacketSink.CreateMock();
			// The predicate reads into the inline Span — the whole point of ref-struct mocking.
			sut.Mock.Setup.Consume(It.IsRefStruct<Packet>(p =>
					p.Payload.Length > 0 && p.Payload[0] == 0xFF))
				.Throws<InvalidOperationException>();

			byte[] hit = [0xFF, 0x01];
			byte[] miss = [0x00, 0x01];

			void ActHit() => sut.Consume(new Packet(1, hit));
			void ActMiss() => sut.Consume(new Packet(2, miss));

			await That(ActHit).Throws<InvalidOperationException>();
			await That(ActMiss).DoesNotThrow();
		}

		[Fact]
		public async Task NoSetup_ShouldBeNoOp()
		{
			IGeneratedPacketSink sut = IGeneratedPacketSink.CreateMock();

			void Act() => sut.Consume(new Packet(42, []));

			await That(Act).DoesNotThrow();
		}

		[Fact]
		public async Task ConsumeCount_ViaInteractions_ReflectsEveryInvocation()
		{
			IGeneratedPacketSink sut = IGeneratedPacketSink.CreateMock();

			sut.Consume(new Packet(1, []));
			sut.Consume(new Packet(2, []));
			sut.Consume(new Packet(3, []));

			// Ref-struct methods do not generate Verify.XXX entries — the interaction type
			// cannot carry the parameter value. Count via the IMock.MockRegistry instead.
			int count = ((IMock)sut).MockRegistry.Interactions
				.OfType<RefStructMethodInvocation>()
				.Count(i => i.Name.EndsWith(".Consume", StringComparison.Ordinal));

			await That(count).IsEqualTo(3);
		}

		[Fact]
		public async Task RecordedInteraction_StoresParameterNameButNoValue()
		{
			IGeneratedPacketSink sut = IGeneratedPacketSink.CreateMock();

			sut.Consume(new Packet(999, []));

			RefStructMethodInvocation? recorded = ((IMock)sut).MockRegistry.Interactions
				.OfType<RefStructMethodInvocation>()
				.SingleOrDefault();

			await That(recorded).IsNotNull();
			await That(recorded!.Name).EndsWith(".Consume");
			await That(recorded.ParameterNames.Single()).IsEqualTo("packet");
		}

		[Fact]
		public async Task LatestMatchingSetupWins()
		{
			IGeneratedPacketSink sut = IGeneratedPacketSink.CreateMock();
			sut.Mock.Setup.Consume(It.IsAnyRefStruct<Packet>())
				.Throws<InvalidOperationException>();
			sut.Mock.Setup.Consume(It.IsRefStruct<Packet>(p => p.Id == 42))
				.Throws<NotSupportedException>();

			void ActSpecific() => sut.Consume(new Packet(42, []));
			void ActFallback() => sut.Consume(new Packet(1, []));

			await That(ActSpecific).Throws<NotSupportedException>();
			await That(ActFallback).Throws<InvalidOperationException>();
		}

		[Fact]
		public async Task ThrowWhenNotSetup_ShouldThrowMockNotSetupException()
		{
			MockBehavior behavior = MockBehavior.Default with { ThrowWhenNotSetup = true };
			IGeneratedPacketSink sut = IGeneratedPacketSink.CreateMock(behavior);

			void Act() => sut.Consume(new Packet(1, []));

			await That(Act).Throws<global::Mockolate.Exceptions.MockNotSetupException>();
		}
	}

	public sealed class MixedParameterTests
	{
		[Fact]
		public async Task Write_WithMatcherForRefStructAndValueParam_Matches()
		{
			IGeneratedPacketWriter sut = IGeneratedPacketWriter.CreateMock();
			sut.Mock.Setup.Write(
					It.IsRefStruct<Packet>(p => p.Id == 1),
					It.IsAny<int>())
				.Throws<InvalidOperationException>();

			void ActHit() => sut.Write(new Packet(1, []), priority: 5);
			void ActMiss() => sut.Write(new Packet(2, []), priority: 5);

			await That(ActHit).Throws<InvalidOperationException>();
			await That(ActMiss).DoesNotThrow();
		}

		[Fact]
		public async Task Write_WithPriorityMatcher_GatesOnNonRefStructParameter()
		{
			IGeneratedPacketWriter sut = IGeneratedPacketWriter.CreateMock();
			sut.Mock.Setup.Write(
					It.IsAnyRefStruct<Packet>(),
					It.Satisfies<int>(p => p > 10))
				.Throws<InvalidOperationException>();

			void ActHit() => sut.Write(new Packet(1, []), priority: 99);
			void ActMiss() => sut.Write(new Packet(1, []), priority: 1);

			await That(ActHit).Throws<InvalidOperationException>();
			await That(ActMiss).DoesNotThrow();
		}
	}

	public sealed class ReturnMethodTests
	{
		[Fact]
		public async Task TryParse_ReturnsConfiguredValue()
		{
			IGeneratedPacketParser sut = IGeneratedPacketParser.CreateMock();
			sut.Mock.Setup.TryParse(It.IsAnyRefStruct<Packet>()).Returns(42);

			int result = sut.TryParse(new Packet(1, []));

			await That(result).IsEqualTo(42);
		}

		[Fact]
		public async Task TryParse_NoReturnConfigured_ReturnsFrameworkDefault()
		{
			IGeneratedPacketParser sut = IGeneratedPacketParser.CreateMock();

			int result = sut.TryParse(new Packet(7, []));

			await That(result).IsEqualTo(0);
		}

		[Fact]
		public async Task TryParse_ReturnsFunc_InvokedPerCall()
		{
			IGeneratedPacketParser sut = IGeneratedPacketParser.CreateMock();
			int counter = 0;
			sut.Mock.Setup.TryParse(It.IsAnyRefStruct<Packet>())
				.Returns(() => ++counter);

			int first = sut.TryParse(new Packet(1, []));
			int second = sut.TryParse(new Packet(2, []));

			await That(first).IsEqualTo(1);
			await That(second).IsEqualTo(2);
		}

		[Fact]
		public async Task TryParse_PredicateDictatesWhichSetupMatches()
		{
			IGeneratedPacketParser sut = IGeneratedPacketParser.CreateMock();
			sut.Mock.Setup.TryParse(It.IsRefStruct<Packet>(p => p.Id > 100)).Returns(100);
			sut.Mock.Setup.TryParse(It.IsRefStruct<Packet>(p => p.Id < 10)).Returns(1);

			int high = sut.TryParse(new Packet(500, []));
			int low = sut.TryParse(new Packet(3, []));
			int mid = sut.TryParse(new Packet(50, []));

			await That(high).IsEqualTo(100);
			await That(low).IsEqualTo(1);
			// Nothing matches → framework default.
			await That(mid).IsEqualTo(0);
		}

		[Fact]
		public async Task TryParse_ThrowsConfiguredException()
		{
			IGeneratedPacketParser sut = IGeneratedPacketParser.CreateMock();
			sut.Mock.Setup.TryParse(It.IsAnyRefStruct<Packet>())
				.Throws<InvalidOperationException>();

			void Act() => sut.TryParse(new Packet(1, []));

			await That(Act).Throws<InvalidOperationException>();
		}
	}

	public sealed class ExtendedArityTests
	{
		[Fact]
		public async Task VoidArity5_SetupThrows_ShouldThrowConfiguredException()
		{
			IBigPacketSink sut = IBigPacketSink.CreateMock();
			sut.Mock.Setup.Absorb(
					It.IsAnyRefStruct<Packet>(),
					It.IsAnyRefStruct<Packet>(),
					It.IsAnyRefStruct<Packet>(),
					It.IsAnyRefStruct<Packet>(),
					It.IsAnyRefStruct<Packet>())
				.Throws<InvalidOperationException>();

			void Act() => sut.Absorb(
				new Packet(1, []),
				new Packet(2, []),
				new Packet(3, []),
				new Packet(4, []),
				new Packet(5, []));

			await That(Act).Throws<InvalidOperationException>();
		}

		[Fact]
		public async Task VoidArity5_NoSetup_IsNoOp()
		{
			IBigPacketSink sut = IBigPacketSink.CreateMock();

			void Act() => sut.Absorb(
				new Packet(1, []),
				new Packet(2, []),
				new Packet(3, []),
				new Packet(4, []),
				new Packet(5, []));

			await That(Act).DoesNotThrow();
		}

		[Fact]
		public async Task VoidArity5_PredicateMatchesSelectively()
		{
			IBigPacketSink sut = IBigPacketSink.CreateMock();
			sut.Mock.Setup.Absorb(
					It.IsAnyRefStruct<Packet>(),
					It.IsAnyRefStruct<Packet>(),
					It.IsAnyRefStruct<Packet>(),
					It.IsAnyRefStruct<Packet>(),
					It.IsRefStruct<Packet>(p => p.Id == 99))
				.Throws<InvalidOperationException>();

			void ActHit() => sut.Absorb(
				new Packet(1, []), new Packet(2, []), new Packet(3, []),
				new Packet(4, []), new Packet(99, []));
			void ActMiss() => sut.Absorb(
				new Packet(1, []), new Packet(2, []), new Packet(3, []),
				new Packet(4, []), new Packet(5, []));

			await That(ActHit).Throws<InvalidOperationException>();
			await That(ActMiss).DoesNotThrow();
		}

		[Fact]
		public async Task ReturnArity6_ReturnsConfiguredValue()
		{
			IBigPacketParser sut = IBigPacketParser.CreateMock();
			sut.Mock.Setup.TryParse(
					It.IsAnyRefStruct<Packet>(),
					It.IsAnyRefStruct<Packet>(),
					It.IsAny<int>(),
					It.IsAnyRefStruct<Packet>(),
					It.IsAnyRefStruct<Packet>(),
					It.IsAny<string>())
				.Returns(1234);

			int result = sut.TryParse(
				new Packet(1, []), new Packet(2, []), offset: 10,
				new Packet(4, []), new Packet(5, []), format: "x");

			await That(result).IsEqualTo(1234);
		}

		[Fact]
		public async Task ReturnArity6_NonRefStructParameterGates_Matches()
		{
			IBigPacketParser sut = IBigPacketParser.CreateMock();
			sut.Mock.Setup.TryParse(
					It.IsAnyRefStruct<Packet>(),
					It.IsAnyRefStruct<Packet>(),
					It.Satisfies<int>(o => o > 0),
					It.IsAnyRefStruct<Packet>(),
					It.IsAnyRefStruct<Packet>(),
					It.IsAny<string>())
				.Throws<InvalidOperationException>();

			void ActHit() => sut.TryParse(
				new Packet(1, []), new Packet(2, []), offset: 7,
				new Packet(4, []), new Packet(5, []), format: "x");
			void ActMiss() => sut.TryParse(
				new Packet(1, []), new Packet(2, []), offset: -1,
				new Packet(4, []), new Packet(5, []), format: "x");

			await That(ActHit).Throws<InvalidOperationException>();
			await That(ActMiss).DoesNotThrow();
		}
	}

	public sealed class IndexerGetterTests
	{
		[Fact]
		public async Task Returns_ConfiguredValue()
		{
			IGeneratedPacketLookup sut = IGeneratedPacketLookup.CreateMock();
			sut.Mock.Setup[It.IsAnyRefStruct<Packet>()].Returns("hit");

			string result = sut[new Packet(1, [])];

			await That(result).IsEqualTo("hit");
		}

		[Fact]
		public async Task Returns_Factory_InvokedPerCall()
		{
			IGeneratedPacketLookup sut = IGeneratedPacketLookup.CreateMock();
			int calls = 0;
			sut.Mock.Setup[It.IsAnyRefStruct<Packet>()].Returns(() => $"call-{++calls}");

			string first = sut[new Packet(1, [])];
			string second = sut[new Packet(2, [])];

			await That(first).IsEqualTo("call-1");
			await That(second).IsEqualTo("call-2");
		}

		[Fact]
		public async Task Throws_ConfiguredException()
		{
			IGeneratedPacketLookup sut = IGeneratedPacketLookup.CreateMock();
			sut.Mock.Setup[It.IsAnyRefStruct<Packet>()].Throws<System.Collections.Generic.KeyNotFoundException>();

			string Act() => sut[new Packet(1, [])];

			await That(Act).Throws<System.Collections.Generic.KeyNotFoundException>();
		}

		[Fact]
		public async Task Predicate_FiltersByKey_PayloadReadable()
		{
			// Predicate reads the inline Span on the ref-struct key — the whole point of the
			// ref-struct pipeline: the payload flows through to the matcher without ever being
			// captured in a field.
			IGeneratedPacketLookup sut = IGeneratedPacketLookup.CreateMock();
			sut.Mock.Setup[It.IsRefStruct<Packet>(p =>
					p.Payload.Length > 0 && p.Payload[0] == 0xFF)]
				.Returns("matched");

			byte[] hitBytes = [0xFF, 0x01];
			byte[] missBytes = [0x00, 0x01];

			string hit = sut[new Packet(1, hitBytes)];
			string miss = sut[new Packet(2, missBytes)];

			await That(hit).IsEqualTo("matched");
			// Nothing matches -> framework default; Mockolate's default for string is "".
			await That(miss).IsEqualTo("");
		}

		[Fact]
		public async Task NoSetup_ReturnsFrameworkDefault()
		{
			IGeneratedPacketLookup sut = IGeneratedPacketLookup.CreateMock();

			string result = sut[new Packet(42, [])];

			await That(result).IsEqualTo("");
		}

		[Fact]
		public async Task RecordedInteraction_UsesRefStructMethodInvocation()
		{
			IGeneratedPacketLookup sut = IGeneratedPacketLookup.CreateMock();

			_ = sut[new Packet(7, [])];

			RefStructMethodInvocation? recorded = ((IMock)sut).MockRegistry.Interactions
				.OfType<RefStructMethodInvocation>()
				.SingleOrDefault();

			await That(recorded).IsNotNull();
			await That(recorded!.Name).EndsWith(".get_Item");
			await That(recorded.ParameterNames.Single()).IsEqualTo("key");
		}
	}
}
#endif
