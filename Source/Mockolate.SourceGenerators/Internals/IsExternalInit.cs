// Shim required by C# 9's `init` accessor when targeting frameworks (netstandard2.0) that don't
// define System.Runtime.CompilerServices.IsExternalInit. Lets the source generator project use
// positional record syntax for cache-key value types.
namespace System.Runtime.CompilerServices;

internal static class IsExternalInit
{
}
