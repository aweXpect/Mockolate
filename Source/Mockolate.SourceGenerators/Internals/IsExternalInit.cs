using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable
// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices;

/// <summary>
///     Shim required by C# 9's `init` accessor when targeting frameworks (netstandard2.0) that don't
///     define System.Runtime.CompilerServices.IsExternalInit. Lets the source generator project use
///     positional record syntax for cache-key value types.
/// </summary>
[ExcludeFromCodeCoverage]
[DebuggerNonUserCode]
internal static class IsExternalInit;

#pragma warning restore
