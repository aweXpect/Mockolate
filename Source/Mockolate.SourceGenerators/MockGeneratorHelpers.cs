using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mockolate.SourceGenerators.Entities;

namespace Mockolate.SourceGenerators;

internal static class MockGeneratorHelpers
{
	internal static bool IsCreateMethodInvocation(this SyntaxNode node)
		=> node is InvocationExpressionSyntax
		{
			Expression: MemberAccessExpressionSyntax
			{
				Name: GenericNameSyntax,
			},
		};

	private static bool IsCreateInvocationOnMockOrMockFactory(this ISymbol? symbol)
		=> symbol?.GetAttributes().Any(a =>
			a.AttributeClass?.ContainingNamespace.ContainingNamespace.IsGlobalNamespace == true &&
			a.AttributeClass.ContainingNamespace.Name == "Mockolate" &&
			a.AttributeClass.Name == "MockGeneratorAttribute") == true;

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
			if (genericTypes.Length == 0 || !IsMockable(genericTypes[0]))
			{
				return null;
			}

			return new MockClass(genericTypes);
		}

		return null;
	}

	private static bool IsMockable(ITypeSymbol typeSymbol)
	{
		return typeSymbol.TypeKind != TypeKind.TypeParameter && (!typeSymbol.IsSealed || typeSymbol.TypeKind == TypeKind.Delegate);
	}
}
