namespace Mockolate.Tests.GeneratorCoverage;

/// <summary>
///     Base class without a parameterless constructor — forces the mock to chain a
///     <c>: base(...)</c> call from every generated constructor.
/// </summary>
public abstract class MyAbstractBase
{
	protected MyAbstractBase(int seed) { Seed = seed; }

	public int Seed { get; }
}