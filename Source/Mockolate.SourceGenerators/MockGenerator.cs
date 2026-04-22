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
	private static SourceText ToSource(string source)
		=> SourceText.From(Sources.Sources.ExpandCrefs(source), Encoding.UTF8);

	void IIncrementalGenerator.Initialize(IncrementalGeneratorInitializationContext context)
	{
		context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
			"Mock.g.cs",
			ToSource(Sources.Sources.MockClass())));

		IncrementalValueProvider<ImmutableArray<MockClass>> expectationsToRegister = context.SyntaxProvider
			.CreateSyntaxProvider(
				static (s, _) => s.IsCreateMethodInvocation(),
				(ctx, _) => ctx.Node.ExtractMockOrMockFactoryCreateSyntaxOrDefault(ctx.SemanticModel))
			.SelectMany(static (mocks, _) => mocks)
			.Collect();

		IncrementalValueProvider<bool> hasOverloadResolutionPriority = context.CompilationProvider
			.Select(static (compilation, _) => HasAttribute(compilation, "System.Runtime.CompilerServices.OverloadResolutionPriorityAttribute"));

		context.RegisterSourceOutput(expectationsToRegister.Combine(hasOverloadResolutionPriority),
			(spc, source) => Execute([..source.Left.Distinct(),], source.Right, spc));

		static bool HasAttribute(Compilation c, string attributeName)
		{
			INamedTypeSymbol? attributeSymbol = c.GetTypeByMetadataName(attributeName);
			return attributeSymbol != null &&
			       (attributeSymbol.DeclaredAccessibility == Accessibility.Public ||
			        (attributeSymbol.DeclaredAccessibility == Accessibility.Internal &&
			         SymbolEqualityComparer.Default.Equals(attributeSymbol.ContainingAssembly, c.Assembly)));
		}
	}

#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
	private static void Execute(ImmutableArray<MockClass> mocksToGenerate, bool hasOverloadResolutionPriority, SourceProductionContext context)
	{
		IEnumerable<(string FileName, string Name, Class Class, (string Name, Class Class)[]? AdditionalClasses)> namedMocksToGenerate = CreateNames(mocksToGenerate);

		HashSet<(string, string)> combinationSet = new();
		foreach ((string FileName, string Name, Class Class, (string Name, Class Class)[]? AdditionalClasses) mockToGenerate in namedMocksToGenerate)
		{
			if (mockToGenerate.Class is MockClass { Delegate: not null, } mockClass)
			{
				context.AddSource($"Mock.{mockToGenerate.FileName}.g.cs",
					ToSource(Sources.Sources.MockDelegate(mockToGenerate.Name, mockClass, mockClass.Delegate)));
			}
			else if (mockToGenerate.AdditionalClasses is null)
			{
				context.AddSource($"Mock.{mockToGenerate.FileName}.g.cs",
					ToSource(Sources.Sources.MockClass(mockToGenerate.Name, mockToGenerate.Class, hasOverloadResolutionPriority)));
			}
			else
			{
				context.AddSource($"Mock.{mockToGenerate.FileName}.g.cs",
					ToSource(Sources.Sources.MockCombinationClass(mockToGenerate.FileName, mockToGenerate.Name, mockToGenerate.Class, mockToGenerate.AdditionalClasses, combinationSet)));
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
				ToSource(Sources.Sources.IndexerSetups(indexerSetups)));
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
				ToSource(Sources.Sources.MethodSetups(methodSetups)));
		}

		const int dotNetFuncActionParameterLimit = 16;
		if (methodSetups.Any(x => x.Item1 >= dotNetFuncActionParameterLimit))
		{
			context.AddSource("ActionFunc.g.cs",
				ToSource(Sources.Sources.ActionFunc(methodSetups
					.Where(x => x.Item1 >= dotNetFuncActionParameterLimit)
					.SelectMany<(int, bool), int>(x => [x.Item1, x.Item1 + 1,])
					.Where(x => x > dotNetFuncActionParameterLimit)
					.Distinct())));
		}

		if (methodSetups.Any(x => !x.Item2))
		{
			context.AddSource("ReturnsThrowsAsyncExtensions.g.cs",
				ToSource(Sources.Sources.ReturnsThrowsAsyncExtensions(methodSetups
					.Where(x => !x.Item2).Select(x => x.Item1).ToArray())));
		}

		// Ref-struct method setups for arity > 4. The hand-written types in
		// Source/Mockolate/Setup/RefStruct{Void,Return}MethodSetup.cs cover arities 1-4; the
		// generator mirrors the same shape for arity 5+.
		HashSet<(int, bool)> refStructMethodSetups = new();
		foreach ((int Count, bool) item in mocksToGenerate
			         .SelectMany(m => m.AllMethods())
			         .Where(m => m.Parameters.Count > 4 &&
			                     m.Parameters.Any(p => p.NeedsRefStructPipeline()))
			         .Select(m => (m.Parameters.Count, m.ReturnType == Type.Void)))
		{
			refStructMethodSetups.Add(item);
		}

		// Ref-struct-keyed indexer setups for arity > 4. Same split as methods: arities 1-4 are
		// hand-written; arity 5+ is generator-emitted. Per arity we track whether the getter,
		// setter, or both accessors need to be emitted — a single mock source may declare
		// indexers with different accessor combinations at the same arity.
		Dictionary<int, (bool HasGetter, bool HasSetter)> refStructIndexerArities = new();
		foreach (Property indexer in mocksToGenerate
			         .SelectMany(m => m.AllProperties())
			         .Where(p => p is { IsIndexer: true, IndexerParameters: not null, } &&
			                     p.IndexerParameters.Value.Count > 4 &&
			                     p.IndexerParameters.Value.Any(kp => kp.NeedsRefStructPipeline())))
		{
			int arity = indexer.IndexerParameters!.Value.Count;
			bool hasGetter = indexer.Getter is not null;
			bool hasSetter = indexer.Setter is not null;
			if (refStructIndexerArities.TryGetValue(arity, out (bool, bool) existing))
			{
				refStructIndexerArities[arity] = (existing.Item1 || hasGetter, existing.Item2 || hasSetter);
			}
			else
			{
				refStructIndexerArities[arity] = (hasGetter, hasSetter);
			}
		}

		if (refStructMethodSetups.Any() || refStructIndexerArities.Any())
		{
			context.AddSource("RefStructMethodSetups.g.cs",
				ToSource(Sources.Sources.RefStructMethodSetups(refStructMethodSetups, refStructIndexerArities)));
		}

		context.AddSource("MockBehaviorExtensions.g.cs",
			ToSource(Sources.Sources.MockBehaviorExtensions(mocksToGenerate)));
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
