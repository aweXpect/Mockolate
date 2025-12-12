using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Mockolate.Analyzers;

/// <summary>
///     Analyzer that ensures all generic arguments to Mock.Wrap&lt;T&gt; invocations are wrappable.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class WrappabilityAnalyzer : DiagnosticAnalyzer
{
	/// <inheritdoc cref="DiagnosticAnalyzer.SupportedDiagnostics" />
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
	[
		Rules.WrappabilityRule,
	];

	/// <inheritdoc cref="DiagnosticAnalyzer.Initialize(AnalysisContext)" />
	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();

		context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
	}

	private static void AnalyzeInvocation(OperationAnalysisContext context)
	{
		if (context.Operation is not IInvocationOperation invocation ||
		    !IsWrapMethod(invocation.TargetMethod))
		{
			return;
		}

		ITypeSymbol? typeArgumentSymbol = GetInvocationTypeArguments(invocation);
		if (typeArgumentSymbol is not null)
		{
			if (typeArgumentSymbol is ITypeParameterSymbol || IsOpenGeneric(typeArgumentSymbol))
			{
				return;
			}

			if (!IsWrappable(typeArgumentSymbol, out string? reason))
			{
				context.ReportDiagnostic(Diagnostic.Create(
					Rules.WrappabilityRule,
					GetTypeArgumentLocation(invocation.Syntax, typeArgumentSymbol) ?? invocation.Syntax.GetLocation(),
					typeArgumentSymbol.ToDisplayString(),
					reason));
			}
		}
	}

	private static ITypeSymbol? GetInvocationTypeArguments(IInvocationOperation invocation)
	{
		if (invocation.TargetMethod is { IsGenericMethod: true, TypeArguments.Length: > 0, } method)
		{
			return method.TypeArguments[0];
		}

		return null;
	}

	private static bool IsWrapMethod(IMethodSymbol method)
		=> method.Name == "Wrap" &&
		   method.GetAttributes().Any(a =>
			   a.AttributeClass is
			   {
				   Name: "MockGeneratorAttribute",
				   ContainingNamespace:
				   {
					   Name: "Mockolate",
					   ContainingNamespace.IsGlobalNamespace: true,
				   },
			   });

	private static bool IsOpenGeneric(ITypeSymbol typeSymbol)
		=> typeSymbol is INamedTypeSymbol nts &&
		   nts.IsGenericType &&
		   nts.TypeArguments.Any(a => a.TypeKind == TypeKind.TypeParameter);

	private static bool IsWrappable(ITypeSymbol typeSymbol, [NotNullWhen(false)] out string? reason)
	{
		if (typeSymbol.TypeKind != TypeKind.Interface)
		{
			reason = "only interface types can be wrapped";
			return false;
		}

		reason = null;
		return true;
	}

	private static Location? GetTypeArgumentLocation(SyntaxNode syntax, ITypeSymbol typeSymbol)
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
