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

#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
	private static void Execute(ImmutableArray<MockClass> mocksToGenerate, SourceProductionContext context)
	{
		IEnumerable<(string FileName, string Name, Class Class, (string Name, Class Class)[]? AdditionalClasses)> namedMocksToGenerate = CreateNames(mocksToGenerate);

		HashSet<(string, string)> combinationSet = new();
		foreach ((string FileName, string Name, Class Class, (string Name, Class Class)[]? AdditionalClasses) mockToGenerate in namedMocksToGenerate)
		{
			if (mockToGenerate.Class is MockClass { Delegate: not null, } mockClass)
			{
				context.AddSource($"Mock.{mockToGenerate.FileName}.g.cs",
					SourceText.From(Sources.Sources.MockDelegate(mockToGenerate.Name, mockClass, mockClass.Delegate), Encoding.UTF8));
			}
			else if (mockToGenerate.AdditionalClasses is null)
			{
				context.AddSource($"Mock.{mockToGenerate.FileName}.g.cs",
					SourceText.From(Sources.Sources.MockClass(mockToGenerate.Name, mockToGenerate.Class), Encoding.UTF8));
			}
			else
			{
				context.AddSource($"Mock.{mockToGenerate.FileName}.g.cs",
					SourceText.From(Sources.Sources.MockCombinationClass(mockToGenerate.FileName, mockToGenerate.Name, mockToGenerate.Class, mockToGenerate.AdditionalClasses, combinationSet), Encoding.UTF8));
			}
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
			context.AddSource("ReturnsThrowsAsyncExtensions.g.cs",
				SourceText.From(Sources.Sources.ReturnsThrowsAsyncExtensions(methodSetups
					.Where(x => !x.Item2).Select(x => x.Item1).ToArray()), Encoding.UTF8));
		}

		context.AddSource("MockBehaviorExtensions.g.cs",
			SourceText.From(Sources.Sources.MockBehaviorExtensions(mocksToGenerate), Encoding.UTF8));
	}
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high

	private static IEnumerable<(string FileName, string Name, Class Class, (string Name, Class Class)[]? AdditionalClasses)> CreateNames(ImmutableArray<MockClass> mocksToGenerate)
	{
		HashSet<string> classNames = new(StringComparer.OrdinalIgnoreCase);
		Dictionary<Class, string> baseClassNames = new(Class.EqualityComparer);
		foreach (Class @class in mocksToGenerate.Where(IsValidMockDeclaration).SelectMany(x => x.AllImplementations()).Distinct(Class.EqualityComparer))
		{
			string name = @class.GetClassNameWithoutDots();
			int suffix = 1;
			string actualName = name;
			while (!classNames.Add(actualName))
			{
				actualName = $"{name}_{suffix++}";
			}

			baseClassNames.Add(@class, actualName);
			yield return (actualName, actualName, @class, null);
		}

		foreach (MockClass mockClass in mocksToGenerate.Where(IsValidMockDeclaration).Where(x => x.AdditionalImplementations.Any()))
		{
			string name = mockClass.GetClassNameWithoutDots() + "__" +
			              string.Join("__", mockClass.AdditionalImplementations.Select(t => t.GetClassNameWithoutDots()));

			int suffix = 1;
			string actualName = name;
			while (!classNames.Add(actualName))
			{
				actualName = $"{name}_{suffix++}";
			}

			yield return (actualName, GetValueOrDefault(baseClassNames, mockClass), mockClass, [
				..mockClass.AdditionalImplementations
					.Select(additional => (GetValueOrDefault(baseClassNames, additional), additional)),
			]);
		}

		static string GetValueOrDefault(Dictionary<Class, string> dictionary, Class key)
		{
			if (dictionary.TryGetValue(key, out string? value))
			{
				return value;
			}

			return "";
		}
	}

	private static bool IsValidMockDeclaration(MockClass mockClass)
		=> (mockClass.IsInterface || mockClass.Constructors is { Count: > 0, }) &&
		   mockClass.AdditionalImplementations.All(x => x.IsInterface);
}
