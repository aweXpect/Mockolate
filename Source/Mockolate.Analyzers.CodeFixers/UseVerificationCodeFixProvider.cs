using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Text;

namespace Mockolate.Analyzers.CodeFixers;

/// <summary>
///     A code fix provider that appends `AtLeastOnce()` to all VerificationResults that are not used.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseVerificationCodeFixProvider))]
[Shared]
public class UseVerificationCodeFixProvider : CodeFixProvider
{
	/// <inheritdoc />
	public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = [Rules.UseVerificationRule.Id,];

	/// <inheritdoc />
	public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

	/// <inheritdoc />
	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		foreach (Diagnostic? diagnostic in context.Diagnostics)
		{
			TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;

			SyntaxNode? root = await context.Document
				.GetSyntaxRootAsync(context.CancellationToken)
				.ConfigureAwait(false);

			SyntaxNode? diagnosticNode = root?.FindNode(diagnosticSpan);
			if (diagnosticNode is ExpressionSyntax expressionSyntax)
			{
				ExpressionSyntax? upperMostExpression = expressionSyntax
					.AncestorsAndSelf().OfType<ExpressionSyntax>().Last();

				context.RegisterCodeFix(
					CodeAction.Create(
						Resources.Mockolate0001CodeFixTitle,
						c => AppendAtLeastOnce(context.Document, upperMostExpression, c),
						nameof(Resources.Mockolate0001CodeFixTitle)),
					diagnostic);
			}
		}
	}

	/// <summary>
	///     Executed on the quick fix action raised by the user.
	/// </summary>
	/// <param name="document">Affected source file.</param>
	/// <param name="expressionSyntax">Highlighted class declaration Syntax Node.</param>
	/// <param name="cancellationToken">Any fix is cancellable by the user, so we should support the cancellation token.</param>
	private static async Task<Document> AppendAtLeastOnce(Document document, ExpressionSyntax expressionSyntax, CancellationToken cancellationToken)
	{
		DocumentEditor? editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

		var atLeastOnceIdentifier = SyntaxFactory.IdentifierName("AtLeastOnce");
		var atLeastOnceInvocation = SyntaxFactory.InvocationExpression(
			SyntaxFactory.MemberAccessExpression(
				SyntaxKind.SimpleMemberAccessExpression,
				expressionSyntax,
				atLeastOnceIdentifier
			)
		);

		// Replace the original expression with the new invocation
		editor.ReplaceNode(expressionSyntax, atLeastOnceInvocation);

		return editor.GetChangedDocument();
	}
}
