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
		"CS0116" /* A namespace cannot directly contain members such as fields or methods */,
		"CS1520" /* Method must have a return type */,
		"CS0710" /* Static classes cannot have instance constructors */,
		"CS1513" /* } expected */,
		"CS1002" /* ; expected */,
		"CS1003" /* Syntax error, ',' expected */,
		"CS0119" /* 'identifier' is a type, which is not valid in the given context */,
		"CS0246" /* A namespace cannot directly contain members such as fields or methods */,
		"CS0305" /* Using the generic type 'type' requires 1 type arguments */,
		"CS8803" /* Top-level statements must precede namespace and type declarations */,
		"CS1022" /* Type or namespace definition, or end-of-file expected */,
		"CS0708" /* 'member': cannot declare instance members in a static class */,
		"CS0103" /* The name 'name' does not exist in the current context */,
		"CS7022" /* The entry point of the program is global code */,
		"CS8802" /* Only one compilation unit can have top-level statements */,
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

	private static List<PortableExecutableReference> GetReferences(Type[] types) =>
		AppDomain.CurrentDomain.GetAssemblies()
			.Where(x => !x.IsDynamic && !string.IsNullOrWhiteSpace(x.Location))
			.Select(x => x.Location)
			.Concat([
				typeof(MockBehavior).Assembly.Location,
				..types.Select(t => t.Assembly.Location),
			])
			.Distinct()
			.Select(loc => MetadataReference.CreateFromFile(loc))
			.ToList();
}
