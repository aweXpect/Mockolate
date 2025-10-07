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
			context.AddSource($"For{mockToGenerate.Name}.g.cs",
				SourceText.From(SourceGeneration.ForMock(mockToGenerate.Name, mockToGenerate.MockClass), Encoding.UTF8));
			context.AddSource($"For{mockToGenerate.Name}.Extensions.g.cs",
				SourceText.From(SourceGeneration.ForMockExtensions(mockToGenerate.Name, mockToGenerate.MockClass), Encoding.UTF8));
		}

		HashSet<(int, bool)> methodSetups = new();


		foreach ((string name, Class extensionToGenerate) in GetDistinctExtensions(mocksToGenerate))
		{
			context.AddSource($"For{name}.SetupExtensions.g.cs",
				SourceText.From(SourceGeneration.ForMockSetupExtensions(name, extensionToGenerate), Encoding.UTF8));
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
			context.AddSource("MethodSetups.g.cs",
				SourceText.From(SourceGeneration.GetMethodSetups(methodSetups), Encoding.UTF8));
		}

		if (methodSetups.Any(x => !x.Item2))
		{
			context.AddSource("ReturnsAsyncExtensions.g.cs",
				SourceText.From(SourceGeneration.GetReturnsAsyncExtensions(methodSetups
				.Where(x => !x.Item2).Select(x => x.Item1).ToArray()), Encoding.UTF8));
		}

		context.AddSource("MockRegistration.g.cs",
			SourceText.From(SourceGeneration.MockRegistration(namedMocksToGenerate), Encoding.UTF8));
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
