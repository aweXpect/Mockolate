#if NET10_0_OR_GREATER
namespace Mockolate.ExampleTests.GeneratorCoverage;

/// <summary>
///     Parts of an <c>Implementing&lt;&gt;().Implementing&lt;&gt;()</c> chain.
///     Both interfaces declare <c>Run</c> and <c>Value</c> with the same signature so the
///     combination mock is forced into explicit interface implementations on the overlap.
/// </summary>
public interface ICombinationMockA
{
	int Value { get; }
	void Run();
}

public interface ICombinationMockB
{
	int Value { get; }
	void Run();
}
#endif
