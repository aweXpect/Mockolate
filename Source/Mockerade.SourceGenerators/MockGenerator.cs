using System.Collections.Immutable;
using System.Text;
using Mockerade.SourceGenerators.Entities;
using Mockerade.SourceGenerators.Internals;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Mockerade.SourceGenerators;

/// <summary>
///     The <see cref="IIncrementalGenerator" /> for the registration of mocks.
/// </summary>
[Generator]
public class MockGenerator : IIncrementalGenerator
{
	void IIncrementalGenerator.Initialize(IncrementalGeneratorInitializationContext context)
	{
		context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
			"Mock.g.cs",
			SourceText.From(SourceGeneration.Mock, Encoding.UTF8)));

		IncrementalValueProvider<ImmutableArray<MockClass?>> expectationsToRegister = context.SyntaxProvider
			.CreateSyntaxProvider(
				static (s, _) => s.IsMockForInvocationExpressionSyntax(),
				(ctx, _) => GetSemanticTargetForGeneration(ctx))
			.Where(static m => m is not null)
			.Collect();

		context.RegisterSourceOutput(expectationsToRegister,
			(spc, source) => Execute([..source.Where(t => t != null).Distinct().Cast<MockClass>(),], spc));
	}

	private static MockClass? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
	{
		if (context.Node.TryExtractGenericNameSyntax(context.SemanticModel, out GenericNameSyntax? genericNameSyntax))
		{
			SemanticModel semanticModel = context.SemanticModel;

			ITypeSymbol[] types = genericNameSyntax.TypeArgumentList.Arguments
				.Select(t => semanticModel.GetTypeInfo(t).Type)
				.Where(t => t is not null)
				.Cast<ITypeSymbol>()
				.ToArray();
			MockClass mockClassClass = new(types);
			return mockClassClass;
		}

		return null;
	}

	private static void Execute(ImmutableArray<MockClass> mocksToGenerate, SourceProductionContext context)
	{
		foreach (var mockToGenerate in mocksToGenerate)
		{
			string result = SourceGeneration.GetMockClass(mockToGenerate);
			// Create a separate class file for each mock
			context.AddSource(mockToGenerate.FileName, SourceText.From(result, Encoding.UTF8));
		}

		context.AddSource("Mock.Registration.g.cs",
			SourceText.From(SourceGeneration.RegisterMocks(mocksToGenerate), Encoding.UTF8));
	}
}
