using System;
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
	///     Yields the members of <paramref name="type" /> that the generator will actually emit a mock
	///     override for. Mirrors <c>Source/Mockolate.SourceGenerators/Entities/Class.cs</c>: every
	///     non-sealed member on an interface; every non-sealed <c>virtual</c>/<c>abstract</c> member on
	///     a class (including its base hierarchy). Members are de-duplicated by signature so an
	///     override and its virtual base don't produce two diagnostics for the same logical method.
	/// </summary>
	private static IEnumerable<ISymbol> EnumerateAllMembers(ITypeSymbol type)
	{
		bool isInterface = type.TypeKind == TypeKind.Interface;
		HashSet<string> seen = new(StringComparer.Ordinal);

		foreach ((ISymbol member, _) in GetCandidateMembers(type, isInterface)
			         .Where(c => IsOverriddenByGenerator(c.member, c.isInterface) &&
			                     seen.Add(GetSignatureKey(c.member))))
		{
			yield return member;
		}
	}

	private static IEnumerable<(ISymbol member, bool isInterface)> GetCandidateMembers(ITypeSymbol type,
		bool isInterface)
	{
		foreach (ISymbol m in type.GetMembers())
		{
			yield return (m, isInterface);
		}

		foreach (INamedTypeSymbol iface in type.AllInterfaces)
		{
			foreach (ISymbol m in iface.GetMembers())
			{
				yield return (m, true);
			}
		}

		if (isInterface)
		{
			yield break;
		}

		for (ITypeSymbol? t = type.BaseType;
		     t is not null && t.SpecialType != SpecialType.System_Object;
		     t = t.BaseType)
		{
			foreach (ISymbol m in t.GetMembers())
			{
				yield return (m, false);
			}
		}
	}

	/// <summary>
	///     Matches the member filter in <c>Class.cs</c>: <c>!IsSealed</c> for all kinds, and for class
	///     mocks the member must be abstract or virtual (since the generator can only override
	///     abstract/virtual members).
	/// </summary>
	private static bool IsOverriddenByGenerator(ISymbol member, bool isInterface)
	{
		if (member.IsSealed)
		{
			return false;
		}

		return isInterface || member.IsAbstract || member.IsVirtual;
	}

	/// <summary>
	///     Produces a containing-type-independent signature key so members are de-duplicated across
	///     the type hierarchy. Covers methods (name + generic arity + parameter RefKind/type) and
	///     indexers (parameter RefKind/type). RefKind is part of C#'s overload signature
	///     (<c>M(int)</c> vs <c>M(ref int)</c> vs <c>M(in int)</c> vs <c>M(out int)</c> are
	///     distinct), so collapsing on type alone would hide ref-struct diagnostics on one of the
	///     overloads.
	/// </summary>
	private static string GetSignatureKey(ISymbol member)
	{
		switch (member)
		{
			case IMethodSymbol method:
				return "M:" + method.Name + "`" + method.Arity + "(" +
				       string.Join(",", method.Parameters.Select(FormatParameter)) + ")";
			case IPropertySymbol { IsIndexer: true, } indexer:
				return "I:(" +
				       string.Join(",", indexer.Parameters.Select(FormatParameter)) + ")";
			default:
				return member.Kind + ":" + member.Name;
		}
	}

	private static string FormatParameter(IParameterSymbol parameter)
		=> parameter.RefKind == RefKind.None
			? parameter.Type.ToDisplayString()
			: parameter.RefKind.ToString().ToLowerInvariant() + " " + parameter.Type.ToDisplayString();

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
		foreach (IParameterSymbol p in method.Parameters)
		{
			if (!NeedsRefStructPipeline(p.Type))
			{
				continue;
			}

			hasRefStructParam = true;

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

		// Note: no arity ceiling for ref-struct methods. Arities 1-4 are hand-written types in
		// Source/Mockolate/Setup/; arity 5+ are emitted by the generator into
		// RefStructMethodSetups.g.cs.

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
		if (!indexer.Parameters.Any(p => NeedsRefStructPipeline(p.Type)))
		{
			issue = null;
			return false;
		}

		if (!supportsRefStructPipeline)
		{
			issue =
				"ref-struct-keyed indexers require .NET 9 or later";
			return true;
		}

		// Ref-struct-keyed indexers (getter-only, setter-only, and get+set) are fully supported
		// via the ref-struct pipeline.
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
