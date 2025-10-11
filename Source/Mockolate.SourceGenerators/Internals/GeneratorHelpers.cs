using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mockolate.SourceGenerators.Entities;

namespace Mockolate.SourceGenerators.Internals;

internal static class GeneratorHelpers
{
	internal static bool IsCreateMethodInvocation(this SyntaxNode node)
		=> node is InvocationExpressionSyntax
		{
			Expression: MemberAccessExpressionSyntax
			{
				Name: GenericNameSyntax { Identifier.Text: "Create", },
			},
		};

	private static bool IsCreateInvocationOnMockOrMockFactory(this ISymbol? symbol)
		=> symbol?.ContainingType.ContainingNamespace.ContainingNamespace.IsGlobalNamespace == true &&
		   symbol.ContainingType.ContainingNamespace.Name == "Mockolate" &&
		   (symbol.ContainingType.Name == "Mock" ||
		    symbol.ContainingType.ContainingType.Name == "Mock" && symbol.ContainingType.Name == "Factory");

	internal static MockClass? ExtractMockOrMockFactoryCreateSyntaxOrDefault(
		this SyntaxNode syntaxNode, SemanticModel semanticModel)
	{
		InvocationExpressionSyntax invocationSyntax = (InvocationExpressionSyntax)syntaxNode;
		MemberAccessExpressionSyntax memberAccessExpressionSyntax = (MemberAccessExpressionSyntax)invocationSyntax.Expression;
		var genericNameSyntax = (GenericNameSyntax)(memberAccessExpressionSyntax!.Name);
		if (semanticModel.GetSymbolInfo(syntaxNode).Symbol.IsCreateInvocationOnMockOrMockFactory())
		{
			ITypeSymbol[] genericTypes = genericNameSyntax.TypeArgumentList.Arguments
				.Select(t => semanticModel.GetTypeInfo(t).Type)
				.Where(t => t is not null)
				.Cast<ITypeSymbol>()
				.ToArray();
			return new MockClass(genericTypes);
		}

		return null;
	}
}
