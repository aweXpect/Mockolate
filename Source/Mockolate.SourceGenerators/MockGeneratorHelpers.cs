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
		IAssemblySymbol sourceAssembly = semanticModel.Compilation.Assembly;

		// 1. Generische Create-Methoden wie bisher
		if (syntaxNode is InvocationExpressionSyntax invocationSyntax &&
			invocationSyntax.Expression is MemberAccessExpressionSyntax memberAccessExpressionSyntax &&
			memberAccessExpressionSyntax.Name is GenericNameSyntax genericNameSyntax)
		{
			if (semanticModel.GetSymbolInfo(syntaxNode).IsCreateInvocationOnMockOrMockFactory())
			{
				ITypeSymbol[] genericTypes = genericNameSyntax.TypeArgumentList.Arguments
					.Select(t => semanticModel.GetTypeInfo(t).Type)
					.Where(t => t is not null)
					.Cast<ITypeSymbol>()
					.ToArray();
				if (genericTypes.Length > 0 && IsMockable(genericTypes[0]) &&
				    genericTypes.All(x => !x.ContainingNamespace.IsGlobalNamespace))
				{
					yield return new MockClass(genericTypes, sourceAssembly);

					foreach (MockClass? additionalMockClass in DiscoverMockableTypes(genericTypes, sourceAssembly))
					{
						yield return additionalMockClass;
					}
				}
			}
		}

		// 2. Statische, nicht-generische Methoden mit MockGenerator-Attribut
		if (syntaxNode is InvocationExpressionSyntax staticInvocation)
		{
			SymbolInfo symbolInfo = semanticModel.GetSymbolInfo(staticInvocation);
			IMethodSymbol? methodSymbol = symbolInfo.Symbol as IMethodSymbol ?? symbolInfo.CandidateSymbols.OfType<IMethodSymbol>().FirstOrDefault();
			if (methodSymbol != null && methodSymbol.IsStatic && !methodSymbol.IsGenericMethod)
			{
				if (methodSymbol.GetAttributes().Any(a =>
					a.AttributeClass?.ContainingNamespace.ContainingNamespace.IsGlobalNamespace == true &&
					a.AttributeClass.ContainingNamespace.Name == "Mockolate" &&
					a.AttributeClass.Name == "MockGeneratorAttribute"))
				{
					ITypeSymbol returnType = methodSymbol.ReturnType;
					if (IsMockable(returnType) && !returnType.ContainingNamespace.IsGlobalNamespace)
					{
						yield return new MockClass([returnType], sourceAssembly);

						foreach (MockClass? additionalMockClass in DiscoverMockableTypes([returnType], sourceAssembly))
						{
							yield return additionalMockClass;
						}
					}
				}
			}
		}
	}

#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
	private static IEnumerable<MockClass> DiscoverMockableTypes(IEnumerable<ITypeSymbol> initialTypes,
		IAssemblySymbol sourceAssembly)
	{
		Queue<ITypeSymbol> typesToProcess = new(initialTypes);
		HashSet<ITypeSymbol> processedTypes = new(typesToProcess, SymbolEqualityComparer.Default);

		while (typesToProcess.Count > 0)
		{
			ITypeSymbol currentType = typesToProcess.Dequeue();

			// When using HttpClient as a mock, we also have to create a mock for the HttpMessageHandler, that can be used as constructor parameter.
			if (currentType.Name == "HttpClient" && currentType.ToDisplayString() == "System.Net.Http.HttpClient")
			{
				ITypeSymbol httpMessageHandlerType = currentType.GetMembers()
					.OfType<IMethodSymbol>()
					.Where(m => m.MethodKind == MethodKind.Constructor)
					.SelectMany(c => c.Parameters)
					.Select(p => p.Type)
					.First(t => t.Name == "HttpMessageHandler");
				if (processedTypes.Add(httpMessageHandlerType))
				{
					yield return new MockClass([httpMessageHandlerType,], sourceAssembly);
					typesToProcess.Enqueue(httpMessageHandlerType);
				}
			}
		}
	}
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high

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
