#if NET10_0_OR_GREATER
using System;

namespace Mockolate.ExampleTests.GeneratorCoverage;

/// <summary>
///     Isolated coverage for the C# 11 <c>static abstract</c> interface members,
///     including the <c>static abstract event</c> shape that previously surfaced
///     CS8920 when paired with the standard mock-setup parent interface.
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
