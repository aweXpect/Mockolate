using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Mockolate.Analyzers.Helpers;

namespace Mockolate.Analyzers;

/// <summary>
///     An analyzer that checks that all <c>VerificationResult</c> invocations are properly used.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UseVerificationAnalyzer : DiagnosticAnalyzer
{
	/// <inheritdoc />
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Rules.UseVerificationRule,];

	/// <inheritdoc />
	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();

		context.RegisterOperationAction(AnalyzeOperation, OperationKind.Invocation);
	}

	private static void AnalyzeOperation(OperationAnalysisContext context)
	{
		if (context.Operation is IInvocationOperation invocationOperation)
		{
			ITypeSymbol? returnType = invocationOperation.Type;
			if (returnType is INamedTypeSymbol namedReturnType &&
			    namedReturnType.MatchesFullName("Mockolate", "Verify", "VerificationResult"))
			{
				CheckIsUsed(context, invocationOperation);
			}
		}
	}

	private static void CheckIsUsed(OperationAnalysisContext context, IInvocationOperation invocationOperation)
	{
		if (IsOperationUsed(invocationOperation))
		{
			return;
		}

		context.ReportDiagnostic(
			Diagnostic.Create(Rules.UseVerificationRule, context.Operation.Syntax.GetLocation())
		);
	}

#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
	private static bool IsOperationUsed(IInvocationOperation invocationOperation)
	{
		IOperation? parent = invocationOperation.Parent;

		// Walk up the parent chain
		while (parent != null)
		{
			if (parent is
			    // Any assignment (including to variables, fields, properties, etc.)
			    IAssignmentOperation or
			    // Any variable initializer (e.g., var x = ...)
			    IVariableInitializerOperation or
			    // Used as an argument to another invocation)
			    IArgumentOperation or
			    // Used in a return statement
			    IReturnOperation or
			    // Used in a conditional expression
			    IConditionalOperation or
			    // Used in a member access or chained invocation (e.g. .Once(), .AtLeastOnce())
			    IInvocationOperation or IMemberReferenceOperation)
			{
				return true;
			}

			// If the parent is an ExpressionStatement, check if the operation is the same as the invocationOperation
			if (parent is IExpressionStatementOperation expr)
			{
				return expr.Operation != invocationOperation;
			}

			// If the parent is a block operation, we've reached the top of the statement without finding a usage
			if (parent is IBlockOperation)
			{
				return false;
			}

			parent = parent.Parent;
		}

		return false;
	}
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
}
