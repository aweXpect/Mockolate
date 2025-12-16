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
			if (genericTypes.Length > 0 && IsMockable(genericTypes[0]) &&
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
		// Depth limit to prevent excessive traversal of nested types
		// Depth 2 allows discovering types up to 2 levels deep:
		// Level 0: Explicitly mocked type (e.g., IMyInterface1)
		// Level 1: Types from properties/methods (e.g., IMyInterface2)
		// Level 2: Types from properties/methods of Level 1 types (e.g., IMyInterface3)
		const int maxDepth = 2;
		
		Queue<(ITypeSymbol Type, int Depth)> typesToProcess = new();
		foreach (ITypeSymbol initialType in initialTypes)
		{
			typesToProcess.Enqueue((initialType, 0));
		}
		
		HashSet<ITypeSymbol> processedTypes = new(SymbolEqualityComparer.Default);

		while (typesToProcess.Count > 0)
		{
			(ITypeSymbol currentType, int currentDepth) = typesToProcess.Dequeue();

			// Stop traversal if we've reached the maximum depth
			if (currentDepth >= maxDepth)
			{
				continue;
			}

			// Process all members in a single pass
			foreach (ISymbol member in currentType.GetMembers())
			{
				ITypeSymbol? typeToCheck = member switch
				{
					IPropertySymbol property => property.Type,
					IMethodSymbol method when !method.ReturnsVoid => method.ReturnType,
					_ => null
				};

				if (typeToCheck != null &&
				    typeToCheck.TypeKind == TypeKind.Interface &&
				    IsMockable(typeToCheck) &&
				    processedTypes.Add(typeToCheck))
				{
					yield return new MockClass([typeToCheck,]);
					typesToProcess.Enqueue((typeToCheck, currentDepth + 1));
				}
			}
		}
	}

	private static bool IsMockable(ITypeSymbol typeSymbol)
		=> typeSymbol is
		   {
			   IsRecord: false,
			   IsReadOnly: false,
			   ContainingNamespace: not null,
			   TypeKind: TypeKind.Interface or TypeKind.Class or TypeKind.Delegate,
		   } and INamedTypeSymbol namedTypeSymbol &&
		   // Ignore open generic types
		   (!namedTypeSymbol.IsGenericType ||
		    namedTypeSymbol.TypeArguments.All(a => a.TypeKind != TypeKind.TypeParameter)) &&
		   (!typeSymbol.IsSealed || typeSymbol.TypeKind == TypeKind.Delegate);
}
