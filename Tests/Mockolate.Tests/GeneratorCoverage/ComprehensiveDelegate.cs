#if NET10_0_OR_GREATER
using System;

namespace Mockolate.Tests.GeneratorCoverage;

/// <summary>
///     Custom delegate exercising the delegate-mock generator path with ref/out/in
///     parameters and a <see cref="Span{T}" /> return.
/// </summary>
public delegate Span<char> ComprehensiveDelegate(int x, ref int y, out string z, in long w);
#endif
