using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Mockolate.SourceGenerators.Tests.TestHelpers;

public static class Generator
{
	public static GeneratorResult Run(string source)
	{
		MockGenerator generator = new();
		CSharpParseOptions parseOptions = new(LanguageVersion.Latest);
		SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source, parseOptions);
		PortableExecutableReference[] references =
		[
			MetadataReference.CreateFromFile(typeof(MockBehavior).Assembly.Location),
		];

		CSharpCompilation compilation = CSharpCompilation.Create(
			"TestAssembly",
			[syntaxTree,],
			references,
			new CSharpCompilationOptions(OutputKind.ConsoleApplication));

		GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
		driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation outputCompilation,
			out ImmutableArray<Diagnostic> diagnostics);

		GeneratorDriverRunResult runResult = driver.GetRunResult();
		GeneratedSource[] generatedSources = runResult.Results
			.SelectMany(runResult => runResult.GeneratedSources
				.Select(source => new GeneratedSource(source.HintName, source.SourceText.ToString())))
			.ToArray();
		string[] diagnosticMessages = diagnostics.Select(d => d.ToString()).ToArray();
		return new GeneratorResult(generatedSources, diagnosticMessages);
	}
}
