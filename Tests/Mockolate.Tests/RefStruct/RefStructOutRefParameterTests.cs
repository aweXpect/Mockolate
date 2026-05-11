#if NET10_0_OR_GREATER
using GcPacket = Mockolate.Tests.GeneratorCoverage.Packet;
using Mockolate.Setup;
using Mockolate.Tests.GeneratorCoverage;

namespace Mockolate.Tests.RefStruct;

/// <summary>
///     End-to-end coverage for `out`, `ref`, and `ref readonly` ref-struct parameters routed
///     through <see cref="Mockolate.Parameters.IRefStructOutParameter{T}" /> /
///     <see cref="Mockolate.Parameters.IRefStructRefParameter{T}" />. Uses the
///     <see cref="GeneratorCoverage.Packet" /> ref struct (single-int ctor) to isolate this
///     scenario from the payload-carrying <see cref="Packet" /> used by other RefStruct tests.
/// </summary>
/// <remarks>
///     Ref struct values cannot survive an <c>await</c> boundary, so each test captures
///     <see cref="GeneratorCoverage.Packet.Id" /> into an int before awaiting the assertion.
/// </remarks>
public sealed class RefStructOutRefParameterTests
{
	[Fact]
	public async Task OutRefStruct_WithIsOut_AssignsSetterValue()
	{
		IRefStructConsumer sut = IRefStructConsumer.CreateMock();
		sut.Mock.Setup.Produce(It.IsOut<GcPacket>(() => new GcPacket(7)));

		GcPacket packet = default;
		sut.Produce(out packet);
		int id = packet.Id;

		await That(id).IsEqualTo(7);
	}

	[Fact]
	public async Task OutRefStruct_WithIsAnyRefStructOut_AssignsDefault()
	{
		IRefStructConsumer sut = IRefStructConsumer.CreateMock();
		sut.Mock.Setup.Produce(It.IsAnyRefStructOut<GcPacket>());

		GcPacket packet = new GcPacket(99);
		sut.Produce(out packet);
		int id = packet.Id;

		await That(id).IsEqualTo(0);
	}

	[Fact]
	public async Task OutRefStruct_NoSetup_AssignsDefault()
	{
		IRefStructConsumer sut = IRefStructConsumer.CreateMock();

		GcPacket packet = new GcPacket(99);
		sut.Produce(out packet);
		int id = packet.Id;

		await That(id).IsEqualTo(0);
	}

	[Fact]
	public async Task RefRefStruct_WithIsRef_TransformsValue()
	{
		IRefStructConsumer sut = IRefStructConsumer.CreateMock();
		sut.Mock.Setup.Mutate(It.IsRef<GcPacket>(p => new GcPacket(p.Id + 1)));

		GcPacket packet = new GcPacket(41);
		sut.Mutate(ref packet);
		int id = packet.Id;

		await That(id).IsEqualTo(42);
	}

	[Fact]
	public async Task RefRefStruct_WithIsAnyRefStructRef_LeavesValueUnchanged()
	{
		IRefStructConsumer sut = IRefStructConsumer.CreateMock();
		sut.Mock.Setup.Mutate(It.IsAnyRefStructRef<GcPacket>());

		GcPacket packet = new GcPacket(13);
		sut.Mutate(ref packet);
		int id = packet.Id;

		await That(id).IsEqualTo(13);
	}

	[Fact]
	public async Task RefRefStruct_WithPredicate_OnlyTransformsWhenPredicateHolds()
	{
		IRefStructConsumer sut = IRefStructConsumer.CreateMock();
		sut.Mock.Setup.Mutate(It.IsRef<GcPacket>(p => p.Id == 41, p => new GcPacket(99)));

		GcPacket matching = new GcPacket(41);
		sut.Mutate(ref matching);
		int matchingId = matching.Id;

		GcPacket nonMatching = new GcPacket(1);
		sut.Mutate(ref nonMatching);
		int nonMatchingId = nonMatching.Id;

		await That(matchingId).IsEqualTo(99);
		await That(nonMatchingId).IsEqualTo(1);
	}

	[Fact]
	public async Task RefReadOnlyRefStruct_PassesValueUnchanged()
	{
		IRefStructConsumer sut = IRefStructConsumer.CreateMock();

		GcPacket packet = new GcPacket(17);
		sut.Inspect(in packet);
		int idAfter = packet.Id;

		await That(idAfter).IsEqualTo(17);
	}

	[Fact]
	public async Task OutSpan_WithIsOutSpan_AssignsSetterArray()
	{
		IRefStructConsumer sut = IRefStructConsumer.CreateMock();
		sut.Mock.Setup.ProduceSpan(It.IsOutSpan<int>(() => new SpanWrapper<int>(new int[] { 1, 2, 3 })));

		System.Span<int> span = default;
		sut.ProduceSpan(out span);
		int length = span.Length;
		int first = span[0];
		int last = span[2];

		await That(length).IsEqualTo(3);
		await That(first).IsEqualTo(1);
		await That(last).IsEqualTo(3);
	}

	[Fact]
	public async Task OutSpan_WithIsAnyOutSpan_AssignsDefault()
	{
		IRefStructConsumer sut = IRefStructConsumer.CreateMock();
		sut.Mock.Setup.ProduceSpan(It.IsAnyOutSpan<int>());

		System.Span<int> span = new int[] { 99 };
		sut.ProduceSpan(out span);
		int length = span.Length;

		await That(length).IsEqualTo(0);
	}

	[Fact]
	public async Task OutSpan_NoSetup_AssignsDefault()
	{
		IRefStructConsumer sut = IRefStructConsumer.CreateMock();

		System.Span<int> span = new int[] { 99 };
		sut.ProduceSpan(out span);
		int length = span.Length;

		await That(length).IsEqualTo(0);
	}

	[Fact]
	public async Task OutSpan_WithDoCallback_RunsAction()
	{
		IRefStructConsumer sut = IRefStructConsumer.CreateMock();
		int observedFirst = 0;
		int observedLength = 0;
		sut.Mock.Setup.ProduceSpan(
			It.IsOutSpan<int>(() => new SpanWrapper<int>(new int[] { 7, 8 }))
				.Do(w =>
				{
					observedFirst = w.SpanValues[0];
					observedLength = w.SpanValues.Length;
				}));

		System.Span<int> span = default;
		sut.ProduceSpan(out span);

		await That(observedFirst).IsEqualTo(7);
		await That(observedLength).IsEqualTo(2);
	}

	[Fact]
	public async Task RefSpan_WithIsRefSpan_TransformsArray()
	{
		IRefStructConsumer sut = IRefStructConsumer.CreateMock();
		sut.Mock.Setup.MutateSpan(It.IsRefSpan<int>(w => new SpanWrapper<int>(new int[] { w.SpanValues[0] + 1 })));

		System.Span<int> span = new int[] { 41 };
		sut.MutateSpan(ref span);
		int first = span[0];
		int length = span.Length;

		await That(first).IsEqualTo(42);
		await That(length).IsEqualTo(1);
	}

	[Fact]
	public async Task RefSpan_WithIsAnyRefSpan_LeavesValueUnchanged()
	{
		IRefStructConsumer sut = IRefStructConsumer.CreateMock();
		sut.Mock.Setup.MutateSpan(It.IsAnyRefSpan<int>());

		System.Span<int> span = new int[] { 13, 14 };
		sut.MutateSpan(ref span);
		int first = span[0];
		int length = span.Length;

		await That(first).IsEqualTo(13);
		await That(length).IsEqualTo(2);
	}

	[Fact]
	public async Task RefSpan_WithPredicate_OnlyTransformsWhenPredicateHolds()
	{
		IRefStructConsumer sut = IRefStructConsumer.CreateMock();
		sut.Mock.Setup.MutateSpan(
			It.IsRefSpan<int>(
				w => w.SpanValues.Length > 0 && w.SpanValues[0] == 41,
				w => new SpanWrapper<int>(new int[] { 99 })));

		System.Span<int> matching = new int[] { 41 };
		sut.MutateSpan(ref matching);
		int matchingFirst = matching[0];

		System.Span<int> nonMatching = new int[] { 1 };
		sut.MutateSpan(ref nonMatching);
		int nonMatchingFirst = nonMatching[0];

		await That(matchingFirst).IsEqualTo(99);
		await That(nonMatchingFirst).IsEqualTo(1);
	}

	[Fact]
	public async Task OutReadOnlySpan_WithIsOutReadOnlySpan_AssignsSetterArray()
	{
		IRefStructConsumer sut = IRefStructConsumer.CreateMock();
		sut.Mock.Setup.ProduceReadOnlySpan(
			It.IsOutReadOnlySpan<int>(() => new ReadOnlySpanWrapper<int>(new int[] { 5, 6, 7 })));

		System.ReadOnlySpan<int> span = default;
		sut.ProduceReadOnlySpan(out span);
		int length = span.Length;
		int first = span[0];

		await That(length).IsEqualTo(3);
		await That(first).IsEqualTo(5);
	}

	[Fact]
	public async Task OutReadOnlySpan_WithIsAnyOutReadOnlySpan_AssignsDefault()
	{
		IRefStructConsumer sut = IRefStructConsumer.CreateMock();
		sut.Mock.Setup.ProduceReadOnlySpan(It.IsAnyOutReadOnlySpan<int>());

		System.ReadOnlySpan<int> span = new int[] { 99 };
		sut.ProduceReadOnlySpan(out span);
		int length = span.Length;

		await That(length).IsEqualTo(0);
	}

	[Fact]
	public async Task OutReadOnlySpan_NoSetup_AssignsDefault()
	{
		IRefStructConsumer sut = IRefStructConsumer.CreateMock();

		System.ReadOnlySpan<int> span = new int[] { 99 };
		sut.ProduceReadOnlySpan(out span);
		int length = span.Length;

		await That(length).IsEqualTo(0);
	}

	[Fact]
	public async Task RefReadOnlySpan_WithIsRefReadOnlySpan_TransformsArray()
	{
		IRefStructConsumer sut = IRefStructConsumer.CreateMock();
		sut.Mock.Setup.MutateReadOnlySpan(
			It.IsRefReadOnlySpan<int>(
				w => new ReadOnlySpanWrapper<int>(new int[] { w.ReadOnlySpanValues[0] + 1 })));

		System.ReadOnlySpan<int> span = new int[] { 41 };
		sut.MutateReadOnlySpan(ref span);
		int first = span[0];

		await That(first).IsEqualTo(42);
	}

	[Fact]
	public async Task RefReadOnlySpan_WithIsAnyRefReadOnlySpan_LeavesValueUnchanged()
	{
		IRefStructConsumer sut = IRefStructConsumer.CreateMock();
		sut.Mock.Setup.MutateReadOnlySpan(It.IsAnyRefReadOnlySpan<int>());

		System.ReadOnlySpan<int> span = new int[] { 13 };
		sut.MutateReadOnlySpan(ref span);
		int first = span[0];

		await That(first).IsEqualTo(13);
	}

	[Fact]
	public async Task RefReadOnlySpan_WithPredicateOnly_GatesMatchWithoutMutating()
	{
		IRefStructConsumer sut = IRefStructConsumer.CreateMock();
		sut.Mock.Setup.MutateReadOnlySpan(
			It.IsRefReadOnlySpan<int>(w => w.ReadOnlySpanValues.Length == 1));

		System.ReadOnlySpan<int> span = new int[] { 42 };
		sut.MutateReadOnlySpan(ref span);
		int first = span[0];
		int length = span.Length;

		await That(first).IsEqualTo(42);
		await That(length).IsEqualTo(1);
	}

	[Fact]
	public async Task RefReadOnlySpan_WithIsAnyRefStructRef_MatchesViaRefStructPipeline()
	{
		IRefStructConsumer sut = IRefStructConsumer.CreateMock();
		sut.Mock.Setup.InspectSpan(It.IsAnyRefStructRef<System.Span<int>>());

		System.Span<int> span = new int[] { 17, 18, 19 };
		sut.InspectSpan(in span);
		int length = span.Length;
		int first = span[0];

		await That(length).IsEqualTo(3);
		await That(first).IsEqualTo(17);
	}
}
#endif
