#if NET10_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;

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

/// <summary>
///     Class-only generator branches: multiple constructors with different shapes,
///     a constructor parameter named <c>mockRegistry</c> that collides with the
///     synthetic mock-registry parameter and must be renamed, <c>required</c> +
///     <c>[SetsRequiredMembers]</c>, virtual / abstract / protected / sealed-override
///     members, and a nested type.
/// </summary>
public abstract class ComprehensiveAbstractClass : MyAbstractBase
{
	[SetsRequiredMembers]
	public ComprehensiveAbstractClass() : base(0) { Name = ""; }

	[SetsRequiredMembers]
	public ComprehensiveAbstractClass(int v, string text = "x") : base(v) { Name = text; }

	[SetsRequiredMembers]
	protected ComprehensiveAbstractClass(int mockRegistry, bool _) : base(mockRegistry) { Name = ""; }

	[SetsRequiredMembers]
	public ComprehensiveAbstractClass(string name) : base(0) { Name = name; }

	public required string Name { get; init; }

	public virtual int V { get; set; }

	public abstract int A();

	protected virtual int P() => 0;

	public sealed override string ToString() => "";

	public class Inner
	{
		public class Payload
		{
			public int X;
		}
	}
}
#endif
