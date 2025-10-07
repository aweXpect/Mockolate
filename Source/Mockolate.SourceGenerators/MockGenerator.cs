using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Mockolate.SourceGenerators.Entities;
using Mockolate.SourceGenerators.Internals;
using Type = Mockolate.SourceGenerators.Entities.Type;

namespace Mockolate.SourceGenerators;

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
			SourceText.From(SourceGeneration.Mock(), Encoding.UTF8)));

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
			MockClass mockClass = new(types);
			return mockClass;
		}

		return null;
	}

	private static void Execute(ImmutableArray<MockClass> mocksToGenerate, SourceProductionContext context)
	{
		(string Name, MockClass MockClass)[] namedMocksToGenerate = CreateNames(mocksToGenerate);
		foreach ((string Name, MockClass MockClass) mockToGenerate in namedMocksToGenerate)
		{
			string result = SourceGeneration.GetMockClass(mockToGenerate.Name, mockToGenerate.MockClass);
			// Create a separate class file for each mock
			string fileName = $"For{mockToGenerate.Name}.g.cs";
			context.AddSource(fileName, SourceText.From(result, Encoding.UTF8));
		}

		HashSet<(int, bool)> methodSetups = new();


		foreach ((string name, Class extensionToGenerate) in GetDistinctExtensions(mocksToGenerate))
		{
			string result = SourceGeneration.GetExtensionClass(name, extensionToGenerate);
			// Create a separate class file for each mock extension
			string fileName = $"ExtensionsFor{name}.g.cs";
			context.AddSource(fileName, SourceText.From(result, Encoding.UTF8));
		}

		foreach ((int Count, bool) item in mocksToGenerate
			         .SelectMany(m => m.Methods)
			         .Where(m => m.Parameters.Count > 4)
			         .Select(m => (m.Parameters.Count, m.ReturnType == Type.Void)))
		{
			methodSetups.Add(item);
		}

		if (methodSetups.Any())
		{
			string result = SourceGeneration.GetMethodSetups(methodSetups);
			string fileName = "MethodSetups.g.cs";
			context.AddSource(fileName, SourceText.From(result, Encoding.UTF8));
		}

		if (methodSetups.Any(x => !x.Item2))
		{
			string result =
				SourceGeneration.GetReturnsAsyncExtensions(methodSetups.Where(x => !x.Item2).Select(x => x.Item1)
					.ToArray());
			string fileName = "ReturnsAsyncExtensions.g.cs";
			context.AddSource(fileName, SourceText.From(result, Encoding.UTF8));
		}

		context.AddSource("MockRegistration.g.cs",
			SourceText.From(SourceGeneration.RegisterMocks(namedMocksToGenerate), Encoding.UTF8));
	}

	private static List<(string Name, Class Class)> GetDistinctExtensions(ImmutableArray<MockClass> mocksToGenerate)
	{
		HashSet<(string, string)> classNames = new();
		List<(string Name, Class MockClass)> result = new();
		foreach (MockClass? mockToGenerate in mocksToGenerate)
		{
			if (classNames.Add((mockToGenerate.Namespace, mockToGenerate.ClassName)))
			{
				int suffix = 1;
				string actualName = mockToGenerate.GetClassNameWithoutDots();
				while (result.Any(r => r.Name == actualName))
				{
					actualName = $"{mockToGenerate.GetClassNameWithoutDots()}_{suffix++}";
				}

				result.Add((actualName, mockToGenerate));
			}

			foreach (Class? item in mockToGenerate.AdditionalImplementations)
			{
				if (classNames.Add((item.Namespace, item.ClassName)))
				{
					int suffix = 1;
					string actualName = item.GetClassNameWithoutDots();
					while (result.Any(r => r.Name == actualName))
					{
						actualName = $"{item.GetClassNameWithoutDots()}_{suffix++}";
					}

					result.Add((actualName, item));
				}
			}
		}

		return result;
	}

	private static (string Name, MockClass MockClass)[] CreateNames(ImmutableArray<MockClass> mocksToGenerate)
	{
		(string Name, MockClass MockClass)[] result = new (string Name, MockClass MockClass)[mocksToGenerate.Length];
		for (int i = 0; i < mocksToGenerate.Length; i++)
		{
			MockClass mockClass = mocksToGenerate[i];
			string name = mockClass.GetClassNameWithoutDots();
			if (mockClass.AdditionalImplementations.Any())
			{
				name += "_" + string.Join("_",
					mockClass.AdditionalImplementations.Select(t => t.GetClassNameWithoutDots()));
			}

			int suffix = 1;
			string actualName = name;
			while (result.Any(r => r.Name == actualName))
			{
				actualName = $"{name}_{suffix++}";
			}

			result[i] = (actualName, mockClass);
		}

		return result;
	}
}
