#if NET9_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.Linq;
using Mockolate.Interactions;
using Mockolate.Parameters;
using Mockolate.Setup;

namespace Mockolate.Tests.RefStructPrototype;

/// <summary>
///     A ref struct carrying a correlation id and an inline span of bytes. Motivating example for
///     the ref-struct mocking prototype: a realistic ref struct that couldn't flow through the
///     existing <c>IParameter&lt;T&gt;</c> shape before the anti-constraint changes.
/// </summary>
public readonly ref struct DataPacket(int id, ReadOnlySpan<byte> payload)
{
	public int Id { get; } = id;
	public ReadOnlySpan<byte> Payload { get; } = payload;
	public int Length => Payload.Length;
}

/// <summary>
///     The interface under test. In a shipped implementation, users would write this type and call
///     <c>Mock.Create&lt;IPacketSink&gt;()</c> to get a generated mock. The prototype hand-writes
///     the mock in <see cref="PacketSinkMock" /> because the generator hasn't been wired up yet.
/// </summary>
public interface IPacketSink
{
	void Consume(DataPacket packet);
}

/// <summary>
///     Hand-written mock for <see cref="IPacketSink" />. Stands in for what the source generator
///     would emit once Phase 3 wiring lands. The prototype deliberately owns its setup storage
///     locally rather than going through <see cref="MockRegistry" />'s internal
///     <c>Setup.Methods</c>, because that API takes a <c>Func&lt;T, bool&gt;</c> predicate which
///     would need a ref-struct-safe overload to participate in the lookup — a separate change.
/// </summary>
public sealed class PacketSinkMock : IPacketSink
{
	private readonly List<RefStructVoidMethodSetup<DataPacket>> _consumeSetups = [];

	public PacketSinkMock(MockBehavior behavior)
	{
		Registry = new MockRegistry(behavior);
		Setup = new SetupFacade(this);
		Verify = new VerifyFacade(this);
	}

	/// <summary>
	///     The underlying <see cref="MockRegistry" />. Interactions are recorded here so the
	///     existing verify infrastructure continues to work for count-based queries.
	/// </summary>
	public MockRegistry Registry { get; }

	/// <summary>
	///     Fluent setup for the mocked methods.
	/// </summary>
	public SetupFacade Setup { get; }

	/// <summary>
	///     Fluent verification for the mocked methods.
	/// </summary>
	public VerifyFacade Verify { get; }

	/// <inheritdoc />
	public void Consume(DataPacket packet)
	{
		Registry.RegisterInteraction(new RefStructMethodInvocation("Consume", "packet"));

		// Latest-first iteration so the most recently registered matching setup wins —
		// matches the resolution order used by MockSetups.MethodSetups.GetMatching.
		// No closure: `packet` stays on the stack, no ref-struct capture.
		for (int i = _consumeSetups.Count - 1; i >= 0; i--)
		{
			RefStructVoidMethodSetup<DataPacket> setup = _consumeSetups[i];
			if (setup.Matches(packet))
			{
				setup.Invoke(packet);
				return;
			}
		}
	}

	public sealed class SetupFacade(PacketSinkMock owner)
	{
		/// <summary>
		///     Sets up <see cref="IPacketSink.Consume(DataPacket)" /> to match packets accepted
		///     by <paramref name="matcher" />.
		/// </summary>
		public IRefStructVoidMethodSetup<DataPacket> Consume(IParameter<DataPacket> matcher)
		{
			RefStructVoidMethodSetup<DataPacket> setup =
				new("Consume", (IParameterMatch<DataPacket>)matcher);
			owner._consumeSetups.Add(setup);
			return setup;
		}
	}

	public sealed class VerifyFacade(PacketSinkMock owner)
	{
		/// <summary>
		///     Count of <see cref="IPacketSink.Consume(DataPacket)" /> invocations recorded on the
		///     mock.
		/// </summary>
		/// <remarks>
		///     Post-hoc matcher-on-value verification is NOT supported for ref struct parameters —
		///     the captured interaction (<see cref="RefStructMethodInvocation" />) stores only the
		///     method name. Callers can use the setup-side matcher to filter at call time, then
		///     read this count.
		/// </remarks>
		public int ConsumeCount
			=> owner.Registry.Interactions.OfType<RefStructMethodInvocation>()
				.Count(i => i.Name == "Consume");
	}
}
#endif
