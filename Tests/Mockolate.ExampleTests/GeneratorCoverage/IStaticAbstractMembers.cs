#if NET10_0_OR_GREATER
using System;

namespace Mockolate.ExampleTests.GeneratorCoverage;

/// <summary>
///     Isolated coverage for the C# 11 <c>static abstract</c> interface members.
///     Kept in its own type because mixing static abstracts into the larger
///     <see cref="IComprehensiveInterface" /> surface currently surfaces a
///     CS8920 (interface cannot be used as type argument) downstream.
/// </summary>
public interface IStaticAbstractMembers
{
	static abstract int AbstractStaticProperty { get; set; }
	static virtual int VirtualStaticProperty { get; } = 0;
	static abstract int AbstractStaticMethod();
	static virtual int VirtualStaticMethod() => 0;
	static abstract event Action<int> AbstractStaticEvent;
}
#endif
