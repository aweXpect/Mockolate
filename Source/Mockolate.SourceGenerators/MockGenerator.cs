using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Mockolate.SourceGenerators.Entities;
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
			"MockGeneratorAttribute.g.cs",
			SourceText.From(Sources.Sources.MockGeneratorAttribute(), Encoding.UTF8)));

		context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
			"Mock.g.cs",
			SourceText.From(Sources.Sources.MockClass(), Encoding.UTF8)));

		IncrementalValueProvider<ImmutableArray<MockClass>> expectationsToRegister = context.SyntaxProvider
			.CreateSyntaxProvider(
				static (s, _) => s.IsCreateMethodInvocation(),
				(ctx, _) => ctx.Node.ExtractMockOrMockFactoryCreateSyntaxOrDefault(ctx.SemanticModel))
			.SelectMany(static (mocks, _) => mocks)
			.Collect();

		context.RegisterSourceOutput(expectationsToRegister,
			(spc, source) => Execute([..source.Distinct(),], spc));
	}

	private static void Execute(ImmutableArray<MockClass> mocksToGenerate, SourceProductionContext context)
	{
		(string Name, MockClass MockClass)[] namedMocksToGenerate = CreateNames(mocksToGenerate);
		foreach ((string Name, MockClass MockClass) mockToGenerate in namedMocksToGenerate)
		{
			if (!IsValidMockDeclaration(mockToGenerate.MockClass))
			{
				continue;
			}

			context.AddSource($"MockFor{mockToGenerate.Name}.g.cs",
				SourceText.From(Sources.Sources.ForMock(mockToGenerate.Name, mockToGenerate.MockClass), Encoding.UTF8));
			if (mockToGenerate.MockClass.AdditionalImplementations.Any() && mockToGenerate.MockClass.Delegate is null)
			{
				context.AddSource($"MockFor{mockToGenerate.Name}Extensions.g.cs",
					SourceText.From(
						Sources.Sources.ForMockCombinationExtensions(mockToGenerate.Name, mockToGenerate.MockClass, mockToGenerate.MockClass.DistinctAdditionalImplementations()),
						Encoding.UTF8));
			}
		}

		foreach ((string name, Class extensionToGenerate) in GetDistinctExtensions(mocksToGenerate))
		{
			context.AddSource($"MockFor{name}Extensions.g.cs",
				SourceText.From(Sources.Sources.ForMockExtensions(name, extensionToGenerate), Encoding.UTF8));
		}

		HashSet<int> indexerSetups = new();
		foreach (int item in mocksToGenerate
			         .SelectMany(m => m.AllProperties())
			         .Where(m => m.IndexerParameters?.Count > 4)
			         .Select(m => m.IndexerParameters!.Value.Count))
		{
			indexerSetups.Add(item);
		}

		if (indexerSetups.Any())
		{
			context.AddSource("IndexerSetups.g.cs",
				SourceText.From(Sources.Sources.IndexerSetups(indexerSetups), Encoding.UTF8));
		}

		HashSet<(int, bool)> methodSetups = new();
		foreach ((int Count, bool) item in mocksToGenerate
			         .SelectMany(m => m.AllMethods())
			         .Where(m => m.Parameters.Count > 4)
			         .Select(m => (m.Parameters.Count, m.ReturnType == Type.Void)))
		{
			methodSetups.Add(item);
		}

		if (methodSetups.Any())
		{
			context.AddSource("MethodSetups.g.cs",
				SourceText.From(Sources.Sources.MethodSetups(methodSetups), Encoding.UTF8));
		}

		const int dotNetFuncActionParameterLimit = 16;
		if (methodSetups.Any(x => x.Item1 >= dotNetFuncActionParameterLimit))
		{
			context.AddSource("ActionFunc.g.cs",
				SourceText.From(
					Sources.Sources.ActionFunc(methodSetups
						.Where(x => x.Item1 >= dotNetFuncActionParameterLimit)
						.SelectMany<(int, bool), int>(x => [x.Item1, x.Item1 + 1,])
						.Where(x => x > dotNetFuncActionParameterLimit)
						.Distinct()), Encoding.UTF8));
		}

		if (methodSetups.Any(x => !x.Item2))
		{
			context.AddSource("ReturnsAsyncExtensions.g.cs",
				SourceText.From(Sources.Sources.ReturnsAsyncExtensions(methodSetups
					.Where(x => !x.Item2).Select(x => x.Item1).ToArray()), Encoding.UTF8));
		}

		context.AddSource("MockRegistration.g.cs",
			SourceText.From(
				Sources.Sources.MockRegistration(namedMocksToGenerate,
					namedMocksToGenerate.Any(x => IsValidMockDeclaration(x.MockClass))), Encoding.UTF8));

		context.AddSource("MockBehaviorExtensions.g.cs",
			SourceText.From(Sources.Sources.MockBehaviorExtensions(mocksToGenerate), Encoding.UTF8));
	}

	private static List<(string Name, Class Class)> GetDistinctExtensions(ImmutableArray<MockClass> mocksToGenerate)
	{
		HashSet<(string?, string)> classNames = new();
		List<(string Name, Class MockClass)> result = new();
		foreach (MockClass mockToGenerate in mocksToGenerate)
		{
			if (classNames.Add((mockToGenerate.Namespace, mockToGenerate.ClassName)))
			{
				int suffix = 1;
				string actualName = mockToGenerate.GetClassNameWithoutDots();
				while (result.Any(r => r.Name.Equals(actualName, StringComparison.OrdinalIgnoreCase)))
				{
					actualName = $"{mockToGenerate.GetClassNameWithoutDots()}_{suffix++}";
				}

				result.Add((actualName, mockToGenerate));
			}

			foreach (Class item in mockToGenerate.AdditionalImplementations.Where(x => x.IsInterface))
			{
				if (classNames.Add((item.Namespace, item.ClassName)))
				{
					int suffix = 1;
					string actualName = item.GetClassNameWithoutDots();
					while (result.Any(r => r.Name.Equals(actualName, StringComparison.OrdinalIgnoreCase)))
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
			while (result.Any(r => actualName.Equals(r.Name, StringComparison.OrdinalIgnoreCase)))
			{
				actualName = $"{name}_{suffix++}";
			}

			result[i] = (actualName, mockClass);
		}

		return result;
	}

	private static bool IsValidMockDeclaration(MockClass mockClass)
		=> (mockClass.IsInterface || mockClass.Constructors is { Count: > 0, }) &&
		   mockClass.AdditionalImplementations.All(x => x.IsInterface);
}
