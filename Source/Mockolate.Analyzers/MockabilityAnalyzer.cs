using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Mockolate.Analyzers;

/// <summary>
///     Analyzer that ensures all generic arguments to Mock.Create&lt;T&gt; invocations are mockable.
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

		context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
	}

	private static void AnalyzeInvocation(OperationAnalysisContext context)
	{
		if (context.Operation is not IInvocationOperation invocation ||
		    !HasMockGeneratorAttribute(invocation.TargetMethod))
		{
			return;
		}

		ITypeSymbol? typeArgumentSymbol = GetInvocationTypeArguments(invocation);
		if (typeArgumentSymbol is not null)
		{
			if (typeArgumentSymbol is ITypeParameterSymbol || AnalyzerHelpers.IsOpenGeneric(typeArgumentSymbol))
			{
				return;
			}

			if (!IsMockable(typeArgumentSymbol, out string? reason))
			{
				context.ReportDiagnostic(Diagnostic.Create(
					Rules.MockabilityRule,
					AnalyzerHelpers.GetTypeArgumentLocation(invocation.Syntax, typeArgumentSymbol) ??
					invocation.Syntax.GetLocation(),
					typeArgumentSymbol.ToDisplayString(),
					reason));
			}
		}
	}

	private static ITypeSymbol? GetInvocationTypeArguments(IInvocationOperation invocation)
		=> AnalyzerHelpers.GetSingleInvocationTypeArgumentOrNull(invocation.TargetMethod);

	private static bool HasMockGeneratorAttribute(IMethodSymbol method)
		=> AnalyzerHelpers.HasMockGeneratorAttribute(method);

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

		if (typeSymbol.IsStatic)
		{
			reason = "type is static";
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
