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
	///     Get the type argument from an invocation operation.
	/// </summary>
	public static ITypeSymbol? GetSingleInvocationTypeArgumentOrNull(IMethodSymbol method)
	{
		if (method is { IsGenericMethod: true, TypeArguments.Length: > 0, })
		{
			return method.TypeArguments[0];
		}

		return null;
	}

	/// <summary>
	///     Check if a method has the MockGeneratorAttribute.
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
	///     Check if a type is an open generic.
	/// </summary>
	public static bool IsOpenGeneric(ITypeSymbol typeSymbol)
		=> typeSymbol is INamedTypeSymbol nts &&
		   nts.IsGenericType &&
		   nts.TypeArguments.Any(a => a.TypeKind == TypeKind.TypeParameter);

	/// <summary>
	///     Get the location of a type argument in an invocation expression.
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
			string minimal = typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
			string fullyQualified = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
				.Replace("global::", string.Empty);

			return genericNameSyntax.TypeArgumentList.Arguments
				.Where(typeSyntax =>
				{
					string text = typeSyntax.ToString();
					return string.Equals(text, minimal, StringComparison.Ordinal)
					       || string.Equals(text, fullyQualified, StringComparison.Ordinal)
					       || text.EndsWith('.' + minimal, StringComparison.Ordinal);
				})
				.Select(typeSyntax => typeSyntax.GetLocation())
				.FirstOrDefault();
		}

		return null;
	}
}
