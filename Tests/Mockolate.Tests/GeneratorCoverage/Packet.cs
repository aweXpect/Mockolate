#if NET10_0_OR_GREATER
namespace Mockolate.Tests.GeneratorCoverage;

public readonly ref struct Packet(int id)
{
	public int Id { get; } = id;
}
#endif