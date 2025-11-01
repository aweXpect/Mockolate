using Microsoft.CodeAnalysis;

namespace Mockolate.Analyzers.Helpers;

internal static class SymbolExtensions
{
	public static bool MatchesFullName(this INamedTypeSymbol methodSymbol,
		string namespace1,
		string namespace2,
		string name)
		=> methodSymbol.ContainingNamespace?.ContainingNamespace?.ContainingNamespace?.IsGlobalNamespace == true &&
		   methodSymbol.ContainingNamespace?.ContainingNamespace?.Name == namespace1 &&
		   methodSymbol.ContainingNamespace?.Name == namespace2 &&
		   methodSymbol.Name == name;
}
