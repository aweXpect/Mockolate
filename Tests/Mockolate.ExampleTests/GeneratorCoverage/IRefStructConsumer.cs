#if NET10_0_OR_GREATER
namespace Mockolate.ExampleTests.GeneratorCoverage;

public readonly ref struct Packet(int id)
{
	public int Id { get; } = id;
}

/// <summary>
///     Isolates the ref-struct setup pipeline: ref-struct method parameters at arity &gt; 4
///     trigger <c>RefStructMethodSetups.g.cs</c>, and ref-struct indexer keys at arity &gt; 4
///     trigger the ref-struct indexer-setup variant.
/// </summary>
public interface IRefStructConsumer
{
	string this[Packet k1, int k2, Packet k3, int k4, Packet k5] { get; set; }
	void Consume5(Packet p1, Packet p2, Packet p3, Packet p4, Packet p5);
}
#endif
