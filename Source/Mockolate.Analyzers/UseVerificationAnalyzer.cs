using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Mockolate.Analyzers.Helpers;

namespace Mockolate.Analyzers;

/// <summary>
///     An analyzer that checks that all <c>Expect.That</c> expectations are awaited.
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
			var returnType = invocationOperation.Type;
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

	private static bool IsOperationUsed(IInvocationOperation invocationOperation)
	{
		IOperation? parent = invocationOperation.Parent;

		// Walk up the parent chain
		while (parent != null)
		{
			// Any assignment (including to variables, fields, properties, etc.)
			if (parent is IAssignmentOperation)
			{
				return true;
			}

			// Any variable initializer (e.g., var x = ...)
			if (parent is IVariableInitializerOperation)
			{
				return true;
			}

			// Used as an argument to another invocation
			if (parent is IArgumentOperation)
			{
				return true;
			}

			// Used in a return statement
			if (parent is IReturnOperation)
			{
				return true;
			}

			// Used in a conditional expression
			if (parent is IConditionalOperation)
			{
				return true;
			}

			// Used in a member access or chained invocation (e.g. .Once(), .AtLeastOnce())
			if (parent is IInvocationOperation || parent is IMemberReferenceOperation)
			{
				return true;
			}

			// If the parent is an ExpressionStatement, the result is discarded (not used)
			if (parent is IExpressionStatementOperation expr)
			{
				return expr.Operation != invocationOperation;
			}

			if (parent is IBlockOperation)
			{
				return false;
			}

			parent = parent.Parent;
		}

		return false;
	}
}
