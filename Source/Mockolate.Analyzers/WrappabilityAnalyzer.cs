using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
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
			if (typeArgumentSymbol is ITypeParameterSymbol || AnalyzerHelpers.IsOpenGeneric(typeArgumentSymbol))
			{
				return;
			}

			if (!IsWrappable(typeArgumentSymbol, out string? reason))
			{
				context.ReportDiagnostic(Diagnostic.Create(
					Rules.WrappabilityRule,
					AnalyzerHelpers.GetTypeArgumentLocation(invocation.Syntax, typeArgumentSymbol) ??
					invocation.Syntax.GetLocation(),
					typeArgumentSymbol.ToDisplayString(),
					reason));
			}
		}
	}

	private static ITypeSymbol? GetInvocationTypeArguments(IInvocationOperation invocation)
		=> AnalyzerHelpers.GetSingleInvocationTypeArgumentOrNull(invocation.TargetMethod);

	private static bool IsWrapMethod(IMethodSymbol method)
		=> method.Name == "Wrap" &&
		   method.ContainingType is
		   {
			   Name: "Mock",
			   ContainingNamespace:
			   {
				   Name: "Mockolate",
				   ContainingNamespace.IsGlobalNamespace: true,
			   },
		   } &&
		   AnalyzerHelpers.HasMockGeneratorAttribute(method);

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
}
