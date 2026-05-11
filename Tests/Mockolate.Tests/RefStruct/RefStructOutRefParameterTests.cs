#if NET10_0_OR_GREATER
using GcPacket = Mockolate.Tests.GeneratorCoverage.Packet;
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
}
#endif
