using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Mockerade.SourceGenerators.Internals;

internal static class GeneratorHelpers
{
	internal static bool IsMockForInvocationExpressionSyntax(this SyntaxNode node)
		=> node is InvocationExpressionSyntax
		{
			Expression: MemberAccessExpressionSyntax
			{
				Name: GenericNameSyntax { Identifier.Text : "For", }
			}
		};

	internal static bool TryExtractGenericNameSyntax(this SyntaxNode syntaxNode,
		SemanticModel semanticModel,
		[NotNullWhen(true)] out GenericNameSyntax? genericNameSyntax)
	{
		if (syntaxNode is InvocationExpressionSyntax i && i.Expression is MemberAccessExpressionSyntax m &&
		    m.Name is GenericNameSyntax value)
		{
			ISymbol? symbol = semanticModel.GetSymbolInfo(syntaxNode).Symbol;
			genericNameSyntax = value;
			return symbol?.ContainingType.ContainingNamespace.ContainingNamespace.IsGlobalNamespace == true &&
			       symbol.ContainingType.ContainingNamespace.Name == "Mockerade" &&
				   symbol.ContainingType.Name == "Mock";
		}

		genericNameSyntax = null;
		return false;
	}
}
