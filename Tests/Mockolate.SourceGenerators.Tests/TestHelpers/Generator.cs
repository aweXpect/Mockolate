using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Mockolate.SourceGenerators.Tests.TestHelpers;

public static class Generator
{
	private static string[] NoWarn = [
		"CS8019" /* Unnecessary using directive. */,
		"CS8321" /* The local function is declared but never used */,
		// TODO: Remove the following errors when tests work with extension syntax
		"CS0106" /* The modifier 'public' is not valid for this item */,
		"CS1520" /* Method must have a return type */,
		"CS0710" /* Static classes cannot have instance constructors */,
		"CS1513" /* } expected */,
		"CS1022" /* Type or namespace definition, or end-of-file expected */,
		"CS0708" /* 'member': cannot declare instance members in a static class */,
		"CS0103" /* The name 'name' does not exist in the current context */,
	];

	public static GeneratorResult Run(string source, params Type[] assemblyTypes)
	{
		MockGenerator generator = new();
		CSharpParseOptions parseOptions = new(LanguageVersion.Latest);
		SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source, parseOptions);

		CSharpCompilation compilation = CSharpCompilation.Create(
			"TestAssembly",
			[syntaxTree,],
			GetReferences(assemblyTypes),
			new CSharpCompilationOptions(OutputKind.ConsoleApplication));

		GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
		driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation outputCompilation,
			out ImmutableArray<Diagnostic> diagnostics);

		var compilationDiagnostics = outputCompilation.GetDiagnostics();
		GeneratorDriverRunResult runResult = driver.GetRunResult();
		Dictionary<string, string> generatedSources = runResult.Results
			.SelectMany(runResult => runResult.GeneratedSources)
			.ToDictionary(source => source.HintName, source => source.SourceText.ToString());
		string[] diagnosticMessages = [
			..compilationDiagnostics.Where(x => !NoWarn.Contains(x.Id)).Select(ToDiagnosticString),
			..diagnostics.Where(x => !NoWarn.Contains(x.Id)).Select(ToDiagnosticString),
		];
		return new GeneratorResult(generatedSources, diagnosticMessages);
	}

	private static string ToDiagnosticString(Diagnostic d)
	{
		var result = d.ToString();
		if (result.StartsWith("Mockolate.SourceGenerators\\Mockolate.SourceGenerators.MockGenerator\\"))
		{
			result = result["Mockolate.SourceGenerators\\Mockolate.SourceGenerators.MockGenerator\\".Length..];
		}

		return result;
	}

	public static List<PortableExecutableReference> GetReferences(Type[] types) =>
		AppDomain.CurrentDomain.GetAssemblies()
			.Where(x => !x.IsDynamic && !string.IsNullOrWhiteSpace(x.Location))
			.Select(x => MetadataReference.CreateFromFile(x.Location))
			.Concat([
                MetadataReference.CreateFromFile(typeof(MockBehavior).Assembly.Location),
				..types.Select(t => MetadataReference.CreateFromFile(t.Assembly.Location)),
			])
		.Distinct()
			.ToList();
}
