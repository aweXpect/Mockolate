using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Mockolate.Analyzers;

/// <summary>
///     Helper methods for analyzers.
/// </summary>
internal static class AnalyzerHelpers
{
	/// <summary>
	///     Gets the type argument from an invocation operation.
	/// </summary>
	public static ITypeSymbol? GetInvocationTypeArguments(IMethodSymbol method)
	{
		if (method is { IsGenericMethod: true, TypeArguments.Length: > 0, })
		{
			return method.TypeArguments[0];
		}

		return null;
	}

	/// <summary>
	///     Checks if a method has the MockGeneratorAttribute.
	/// </summary>
	public static bool HasMockGeneratorAttribute(IMethodSymbol method)
		=> method.GetAttributes().Any(a =>
			a.AttributeClass is
			{
				Name: "MockGeneratorAttribute",
				ContainingNamespace:
				{
					Name: "Mockolate",
					ContainingNamespace.IsGlobalNamespace: true,
				},
			});

	/// <summary>
	///     Checks if a type is an open generic.
	/// </summary>
	public static bool IsOpenGeneric(ITypeSymbol typeSymbol)
		=> typeSymbol is INamedTypeSymbol nts &&
		   nts.IsGenericType &&
		   nts.TypeArguments.Any(a => a.TypeKind == TypeKind.TypeParameter);

	/// <summary>
	///     Gets the location of a type argument in an invocation expression.
	/// </summary>
	public static Location? GetTypeArgumentLocation(SyntaxNode syntax, ITypeSymbol typeSymbol)
	{
		if (syntax is InvocationExpressionSyntax
		    {
			    Expression: MemberAccessExpressionSyntax
			    {
				    Name: GenericNameSyntax genericNameSyntax,
			    },
		    })
		{
			return genericNameSyntax.TypeArgumentList.Arguments
				.Where(typeSyntax => string.Equals(typeSyntax.ToString(),
					typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
					StringComparison.Ordinal))
				.Select(typeSyntax => typeSyntax.GetLocation())
				.FirstOrDefault();
		}

		return null;
	}
}
