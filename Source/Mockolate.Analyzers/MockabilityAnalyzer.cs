using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Mockolate.Analyzers;

/// <summary>
///     Analyzer that ensures all types used with <c>CreateMock()</c> and <c>Implementing&lt;T&gt;()</c> are mockable.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MockabilityAnalyzer : DiagnosticAnalyzer
{
	/// <inheritdoc cref="DiagnosticAnalyzer.SupportedDiagnostics" />
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
	[
		Rules.MockabilityRule,
	];

	/// <inheritdoc cref="DiagnosticAnalyzer.Initialize(AnalysisContext)" />
	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();

		context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
	}

	private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
	{
		if (context.Node is not InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax memberAccess, } invocation)
		{
			return;
		}

		string methodName = memberAccess.Name.Identifier.ValueText;
		if (methodName != "CreateMock" && methodName != "Implementing")
		{
			return;
		}

		// Resolve the method if possible. When C# 14 extension member syntax is not fully
		// supported by the host Roslyn version, the call may be unresolved (null symbol).
		SymbolInfo invocationInfo = context.SemanticModel.GetSymbolInfo(invocation);
		IMethodSymbol? method = invocationInfo.Symbol as IMethodSymbol
		                        ?? invocationInfo.CandidateSymbols.OfType<IMethodSymbol>().FirstOrDefault();

		// If the method resolves to something outside the Mockolate namespace, skip it.
		if (method is not null && !IsInMockolateNamespace(method))
		{
			return;
		}

		if (methodName == "CreateMock")
		{
			AnalyzeCreateMock(context, memberAccess, method);
		}
		else
		{
			AnalyzeImplementing(context, invocation, memberAccess, method);
		}
	}

	private static void AnalyzeCreateMock(SyntaxNodeAnalysisContext context,
		MemberAccessExpressionSyntax memberAccess,
		IMethodSymbol? method)
	{
		// The receiver of CreateMock() is always a type name, not a value.
		// GetSymbolInfo on the receiver returns the type symbol directly.
		ITypeSymbol? receiverType = GetReceiverType(context, memberAccess, method);
		if (receiverType is null || receiverType is ITypeParameterSymbol || AnalyzerHelpers.IsOpenGeneric(receiverType))
		{
			return;
		}

		if (!IsMockable(receiverType, out string? reason))
		{
			context.ReportDiagnostic(Diagnostic.Create(
				Rules.MockabilityRule,
				memberAccess.Expression.GetLocation(),
				receiverType.ToDisplayString(),
				reason));
		}
	}

	private static void AnalyzeImplementing(SyntaxNodeAnalysisContext context,
		InvocationExpressionSyntax invocation,
		MemberAccessExpressionSyntax memberAccess,
		IMethodSymbol? method)
	{
		// Get the type argument — from the resolved method if available, otherwise from syntax.
		ITypeSymbol? typeArgument = method is not null
			? AnalyzerHelpers.GetSingleInvocationTypeArgumentOrNull(method)
			: GetTypeArgumentFromSyntax(context, memberAccess);

		if (typeArgument is null || typeArgument is ITypeParameterSymbol || AnalyzerHelpers.IsOpenGeneric(typeArgument))
		{
			return;
		}

		Location typeArgumentLocation = AnalyzerHelpers.GetTypeArgumentLocation(invocation, typeArgument) ??
		                                invocation.GetLocation();

		if (!IsMockable(typeArgument, out string? reason))
		{
			context.ReportDiagnostic(Diagnostic.Create(
				Rules.MockabilityRule,
				typeArgumentLocation,
				typeArgument.ToDisplayString(),
				reason));
			return;
		}

		if (typeArgument.TypeKind != TypeKind.Interface)
		{
			context.ReportDiagnostic(Diagnostic.Create(
				Rules.MockabilityRule,
				typeArgumentLocation,
				typeArgument.ToDisplayString(),
				"You can only implement additional interfaces"));
		}
	}

	private static ITypeSymbol? GetReceiverType(SyntaxNodeAnalysisContext context,
		MemberAccessExpressionSyntax memberAccess,
		IMethodSymbol? method)
	{
		// For a static call on a type (e.g. IFoo.CreateMock()), the receiver is a type
		// expression. GetSymbolInfo returns the ITypeSymbol directly.
		SymbolInfo receiverInfo = context.SemanticModel.GetSymbolInfo(memberAccess.Expression);
		if (receiverInfo.Symbol is ITypeSymbol typeFromReceiver)
		{
			return typeFromReceiver;
		}

		// Fallback: derive from the C# 14 extension parameter on the resolved method.
		return method?.ContainingType.ExtensionParameter?.Type;
	}

	private static ITypeSymbol? GetTypeArgumentFromSyntax(SyntaxNodeAnalysisContext context,
		MemberAccessExpressionSyntax memberAccess)
	{
		if (memberAccess.Name is GenericNameSyntax { TypeArgumentList.Arguments: { Count: > 0, } args, })
		{
			return context.SemanticModel.GetTypeInfo(args[0]).Type;
		}

		return null;
	}

	private static bool IsInMockolateNamespace(ISymbol symbol)
		=> symbol.ContainingNamespace is { Name: "Mockolate", ContainingNamespace.IsGlobalNamespace: true, };

	private static bool IsMockable(ITypeSymbol typeSymbol, [NotNullWhen(false)] out string? reason)
	{
		if (typeSymbol.TypeKind == TypeKind.Struct)
		{
			reason = "type is a struct";
			return false;
		}

		if (typeSymbol.TypeKind == TypeKind.Enum)
		{
			reason = "type is an enum";
			return false;
		}

		if (typeSymbol.IsRecord)
		{
			reason = "type is a record";
			return false;
		}

		if (typeSymbol.IsSealed && typeSymbol.TypeKind != TypeKind.Delegate)
		{
			reason = "type is sealed";
			return false;
		}

		if (typeSymbol.ContainingNamespace?.IsGlobalNamespace == true)
		{
			reason = "type is declared in the global namespace";
			return false;
		}

		if (typeSymbol.TypeKind != TypeKind.Interface &&
		    typeSymbol.TypeKind != TypeKind.Class &&
		    typeSymbol.TypeKind != TypeKind.Delegate)
		{
			reason = $"type kind '{typeSymbol.TypeKind}' is not supported";
			return false;
		}

		reason = null;
		return true;
	}
}
