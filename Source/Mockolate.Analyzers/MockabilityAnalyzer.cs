using System.Collections.Generic;
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
	private static readonly DiagnosticDescriptor s_refStructRule = new(
		"Mockolate0004",
		new LocalizableResourceString(nameof(Resources.Mockolate0004Title),
			Resources.ResourceManager, typeof(Resources)),
		new LocalizableResourceString(nameof(Resources.Mockolate0004MessageFormat),
			Resources.ResourceManager, typeof(Resources)),
		"Usage",
		DiagnosticSeverity.Warning,
		true,
		new LocalizableResourceString(nameof(Resources.Mockolate0004Description),
			Resources.ResourceManager, typeof(Resources)));

	/// <inheritdoc cref="DiagnosticAnalyzer.SupportedDiagnostics" />
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
		=> ImmutableArray.Create(Rules.MockabilityRule, s_refStructRule);

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
			return;
		}

		ReportRefStructIssuesForType(context, receiverType, memberAccess.Expression.GetLocation());
	}

	/// <summary>
	///     Emits <see cref="s_refStructRule" /> for each method/indexer on
	///     <paramref name="type" /> (and its base/interface hierarchy) that the ref-struct generator
	///     pipeline cannot safely emit on the current compilation. Multiple issues are reported per
	///     <c>CreateMock</c> site; users get a complete picture of what to fix.
	/// </summary>
	private static void ReportRefStructIssuesForType(SyntaxNodeAnalysisContext context, ITypeSymbol type,
		Location location)
	{
		bool supportsRefStructPipeline = CompilationSupportsRefStructPipeline(context.Compilation);

		foreach (ISymbol member in EnumerateAllMembers(type))
		{
			switch (member)
			{
				case IMethodSymbol { MethodKind: MethodKind.Ordinary, } m
					when TryGetRefStructIssue(m, supportsRefStructPipeline, out string? issue):
					context.ReportDiagnostic(Diagnostic.Create(
						s_refStructRule,
						location,
						type.ToDisplayString(),
						m.Name,
						issue));
					break;
				case IPropertySymbol { IsIndexer: true, } p
					when TryGetRefStructIssueForIndexer(p, supportsRefStructPipeline, out string? issue):
					context.ReportDiagnostic(Diagnostic.Create(
						s_refStructRule,
						location,
						type.ToDisplayString(),
						"this[]",
						issue));
					break;
			}
		}

		if (type.TypeKind == TypeKind.Delegate && type is INamedTypeSymbol { DelegateInvokeMethod: { } invoke, } &&
		    TryGetRefStructIssue(invoke, supportsRefStructPipeline, out string? delegateIssue))
		{
			context.ReportDiagnostic(Diagnostic.Create(
				s_refStructRule,
				location,
				type.ToDisplayString(),
				"Invoke",
				delegateIssue));
		}
	}

	/// <summary>
	///     Yields the members of <paramref name="type" />: for interfaces every inherited interface
	///     member is visited; for classes every virtual/abstract member that the mock overrides.
	/// </summary>
	private static IEnumerable<ISymbol> EnumerateAllMembers(ITypeSymbol type)
	{
		foreach (ISymbol m in type.GetMembers())
		{
			yield return m;
		}

		foreach (INamedTypeSymbol iface in type.AllInterfaces)
		{
			foreach (ISymbol m in iface.GetMembers())
			{
				yield return m;
			}
		}

		for (ITypeSymbol? t = type.BaseType; t is not null; t = t.BaseType)
		{
			foreach (ISymbol m in t.GetMembers())
			{
				yield return m;
			}
		}
	}

	/// <summary>
	///     Mirrors <c>Helpers.NeedsRefStructPipeline</c> from the source generator: a type is
	///     ref-like AND it's not <c>Span&lt;T&gt;</c>/<c>ReadOnlySpan&lt;T&gt;</c> (which the
	///     generator handles via wrapper boxing).
	/// </summary>
	private static bool NeedsRefStructPipeline(ITypeSymbol type)
	{
		if (!type.IsRefLikeType)
		{
			return false;
		}

		return !(type.ContainingNamespace is { Name: "System", ContainingNamespace.IsGlobalNamespace: true, } &&
		         type.Name is "Span" or "ReadOnlySpan");
	}

	/// <summary>
	///     Detects the net9.0+ compilation via the presence of
	///     <c>Mockolate.Setup.IRefStructVoidMethodSetup`1</c> (which itself is
	///     <c>#if NET9_0_OR_GREATER</c>-gated). Present → the ref-struct pipeline can be emitted;
	///     absent → all ref-struct parameter methods are unsupported.
	/// </summary>
	private static bool CompilationSupportsRefStructPipeline(Compilation compilation)
		=> compilation.GetTypeByMetadataName("Mockolate.Setup.IRefStructVoidMethodSetup`1") is not null;

	private static bool TryGetRefStructIssue(IMethodSymbol method, bool supportsRefStructPipeline,
		[NotNullWhen(true)] out string? issue)
	{
		bool hasRefStructParam = false;
		int refStructArity = 0;
		foreach (IParameterSymbol p in method.Parameters)
		{
			if (!NeedsRefStructPipeline(p.Type))
			{
				continue;
			}

			hasRefStructParam = true;
			refStructArity++;

			if (p.RefKind is RefKind.Out or RefKind.Ref or RefKind.RefReadOnlyParameter)
			{
				issue = "out/ref ref-struct parameters are not supported";
				return true;
			}
		}

		if (hasRefStructParam && !supportsRefStructPipeline)
		{
			issue =
				"ref-struct parameter mocking requires .NET 9 or later (uses the 'allows ref struct' anti-constraint)";
			return true;
		}

		// The generator emits the narrow surface for ref-struct methods of arity 1-4 only.
		if (hasRefStructParam && method.Parameters.Length > 4)
		{
			issue = "ref-struct parameter methods with arity > 4 are not supported";
			return true;
		}

		// Ref-struct returns are out of scope unless they go through the Span wrapper.
		if (NeedsRefStructPipeline(method.ReturnType))
		{
			issue = "methods returning a non-span ref struct are not supported";
			return true;
		}

		issue = null;
		return false;
	}

	private static bool TryGetRefStructIssueForIndexer(IPropertySymbol indexer, bool supportsRefStructPipeline,
		[NotNullWhen(true)] out string? issue)
	{
		foreach (IParameterSymbol p in indexer.Parameters)
		{
			if (!NeedsRefStructPipeline(p.Type))
			{
				continue;
			}

			if (!supportsRefStructPipeline)
			{
				issue =
					"ref-struct-keyed indexers require .NET 9 or later, and full getter/setter support is not yet implemented";
				return true;
			}

			// Runtime emits NotSupportedException; surface at build time.
			issue = "indexers with ref-struct keys are not yet supported by the generator";
			return true;
		}

		issue = null;
		return false;
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
