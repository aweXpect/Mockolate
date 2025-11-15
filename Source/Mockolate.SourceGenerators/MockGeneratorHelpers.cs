using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mockolate.SourceGenerators.Entities;

namespace Mockolate.SourceGenerators;

internal static class MockGeneratorHelpers
{
	internal static bool IsCreateMethodInvocation(this SyntaxNode node)
		=> node is InvocationExpressionSyntax
		{
			Expression: MemberAccessExpressionSyntax
			{
				Name: GenericNameSyntax { Arity: > 0, },
			},
		};

	private static bool IsCreateInvocationOnMockOrMockFactory(this SymbolInfo symbolInfo)
	{
		ISymbol? symbol = symbolInfo.Symbol;
		if (symbol != null)
		{
			return symbol.GetAttributes().Any(a =>
				a.AttributeClass?.ContainingNamespace.ContainingNamespace.IsGlobalNamespace == true &&
				a.AttributeClass.ContainingNamespace.Name == "Mockolate" &&
				a.AttributeClass.Name == "MockGeneratorAttribute");
		}

		return symbolInfo.CandidateSymbols.Any(s => s.GetAttributes().Any(a =>
			a.AttributeClass?.ContainingNamespace.ContainingNamespace.IsGlobalNamespace == true &&
			a.AttributeClass.ContainingNamespace.Name == "Mockolate" &&
			a.AttributeClass.Name == "MockGeneratorAttribute"));
	}

	internal static IEnumerable<MockClass> ExtractMockOrMockFactoryCreateSyntaxOrDefault(
		this SyntaxNode syntaxNode, SemanticModel semanticModel)
	{
		InvocationExpressionSyntax invocationSyntax = (InvocationExpressionSyntax)syntaxNode;
		MemberAccessExpressionSyntax memberAccessExpressionSyntax =
			(MemberAccessExpressionSyntax)invocationSyntax.Expression;
		GenericNameSyntax genericNameSyntax = (GenericNameSyntax)memberAccessExpressionSyntax.Name;
		if (semanticModel.GetSymbolInfo(syntaxNode).IsCreateInvocationOnMockOrMockFactory())
		{
			ITypeSymbol[] genericTypes = genericNameSyntax.TypeArgumentList.Arguments
				.Select(t => semanticModel.GetTypeInfo(t).Type)
				.Where(t => t is not null)
				.Cast<ITypeSymbol>()
				.ToArray();
			if (genericTypes.Length > 0 && IsMockable(genericTypes[0], true) &&
			    // Ignore types from the global namespace, as they are not generated correctly.
			    genericTypes.All(x => !x.ContainingNamespace.IsGlobalNamespace))
			{
				yield return new MockClass(genericTypes);

				foreach (MockClass? additionalMockClass in DiscoverMockableTypes(genericTypes))
				{
					yield return additionalMockClass;
				}
			}
		}
	}

	private static IEnumerable<MockClass> DiscoverMockableTypes(IEnumerable<ITypeSymbol> initialTypes)
	{
		Queue<ITypeSymbol> typesToProcess = new(initialTypes);
		HashSet<ITypeSymbol> processedTypes = new(SymbolEqualityComparer.Default);

		while (typesToProcess.Count > 0)
		{
			ITypeSymbol currentType = typesToProcess.Dequeue();

			foreach (ITypeSymbol propertyType in currentType.GetMembers()
				         .OfType<IPropertySymbol>()
				         .Select(p => p.Type))
			{
				if (propertyType.TypeKind == TypeKind.Interface &&
				    IsMockable(propertyType, false) &&
				    processedTypes.Add(propertyType))
				{
					yield return new MockClass([propertyType,]);
					typesToProcess.Enqueue(propertyType);
				}
			}

			foreach (ITypeSymbol methodType in currentType.GetMembers()
				         .OfType<IMethodSymbol>()
				         .Where(m => !m.ReturnsVoid)
				         .Select(m => m.ReturnType))
			{
				if (methodType.TypeKind == TypeKind.Interface &&
				    IsMockable(methodType, false) &&
				    processedTypes.Add(methodType))
				{
					yield return new MockClass([methodType,]);
					typesToProcess.Enqueue(methodType);
				}
			}
		}
	}

	private static bool IsMockable(ITypeSymbol typeSymbol, bool includeSystemNamespace)
		=> typeSymbol is
		   {
			   IsRecord: false,
			   ContainingNamespace: not null,
			   TypeKind: TypeKind.Interface or TypeKind.Class or TypeKind.Delegate,
		   } &&
		   (includeSystemNamespace || !IsSystemNamespace(typeSymbol.ContainingNamespace)) &&
		   (!typeSymbol.IsSealed || typeSymbol.TypeKind == TypeKind.Delegate);

	private static bool IsSystemNamespace(INamespaceSymbol? namespaceSymbol)
	{
		if (namespaceSymbol is null || namespaceSymbol.IsGlobalNamespace)
		{
			return false;
		}

		if (namespaceSymbol is { Name: "System", ContainingNamespace.IsGlobalNamespace: true, })
		{
			return true;
		}

		return IsSystemNamespace(namespaceSymbol.ContainingNamespace);
	}
}
