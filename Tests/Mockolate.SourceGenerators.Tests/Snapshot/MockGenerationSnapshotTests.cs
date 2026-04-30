using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;

namespace Mockolate.SourceGenerators.Tests.Snapshot;

/// <summary>
///     Snapshot coverage for the source generator output. Each scenario mirrors a fact in
///     <c>Mockolate.ExampleTests.MockCreationTests</c> and reuses the same
///     <c>Tests/Mockolate.ExampleTests/GeneratorCoverage</c> source files as input, so the
///     example types remain the single source of truth for "every special case in the
///     source generator". The full set of generated <c>.g.cs</c> files is diffed against
///     <c>Tests/Mockolate.SourceGenerators.Tests/Snapshot/Expected/&lt;scenario&gt;/</c>.
///     When a scenario fails because the change was intentional, run
///     <see cref="MockGenerationSnapshotAcceptance.AcceptSnapshotChanges" />.
/// </summary>
public sealed partial class MockGenerationSnapshotTests
{
	internal static readonly IReadOnlyList<SnapshotScenario> Scenarios =
	[
		new(
			"BaseClass_WithMultipleAdditionalInterfaces_CanBeCreated",
			["ComprehensiveAbstractClass.cs", "ICombinationParts.cs",],
			"""
			ComprehensiveAbstractClass sut = ComprehensiveAbstractClass.CreateMock()
				.Implementing<ICombinationMockA>()
				.Implementing<ICombinationMockB>();
			""",
			[]),
		new(
			"ComprehensiveAbstractClass_CanBeCreated",
			["ComprehensiveAbstractClass.cs",],
			"""
			ComprehensiveAbstractClass sut = ComprehensiveAbstractClass.CreateMock();
			""",
			[]),
		new(
			"ComprehensiveDelegate_CanBeCreated",
			["ComprehensiveDelegate.cs",],
			"""
			ComprehensiveDelegate sut = ComprehensiveDelegate.CreateMock();
			""",
			[]),
		new(
			"ComprehensiveInterface_CanBeCreated",
			["ComprehensiveDelegate.cs", "IComprehensiveInterface.cs",],
			"""
			IComprehensiveInterface sut = IComprehensiveInterface.CreateMock();
			""",
			[]),
		new(
			"HttpClient_CanBeCreated",
			[],
			"""
			System.Net.Http.HttpClient sut = System.Net.Http.HttpClient.CreateMock();
			""",
			[typeof(HttpClient), typeof(HttpStatusCode),]),
		new(
			"KeywordEdgeCases_CanBeCreated",
			["KeywordEdgeCases.cs",],
			"""
			IKeywordEdgeCases sut = IKeywordEdgeCases.CreateMock();
			""",
			[]),
		new(
			"RefStructConsumer_CanBeCreated",
			["IRefStructConsumer.cs",],
			"""
			IRefStructConsumer sut = IRefStructConsumer.CreateMock();
			""",
			[]),
		new(
			"StaticAbstractMembers_CanBeCreated",
			["IStaticAbstractMembers.cs",],
			"""
			IStaticAbstractMembers sut = IStaticAbstractMembers.CreateMock();
			""",
			[]),
	];

	public static IEnumerable<string> ScenarioNames
	{
		get
		{
			foreach (SnapshotScenario scenario in Scenarios)
			{
				yield return scenario.Name;
			}
		}
	}

	[Test]
	[MethodDataSource(nameof(ScenarioNames))]
	public async Task GeneratorOutput_MatchesExpectedSnapshot(string scenarioName)
	{
		SnapshotScenario scenario = Scenarios.Single(s => s.Name == scenarioName);
		GeneratorResult result = RunGenerator(scenario);
		await That(result.Diagnostics).IsEmpty();

		IReadOnlyDictionary<string, string> generated = NormalizeSources(result);
		IReadOnlyDictionary<string, string> expected = SnapshotStorage.GetExpected(scenarioName);

		await That(generated.Keys).IsEqualTo(expected.Keys).InAnyOrder();

		foreach (string fileName in expected.Keys)
		{
			await That(StripConfigSpecificLines(generated[fileName]))
				.IsEqualTo(StripConfigSpecificLines(expected[fileName]))
				.IgnoringNewlineStyle();
		}
	}

	internal static GeneratorResult RunGenerator(SnapshotScenario scenario)
	{
		List<string> sources = new();
		foreach (string coverageFile in scenario.CoverageFiles)
		{
			sources.Add("#nullable enable\n" + SnapshotStorage.ReadCoverageFile(coverageFile));
		}

		string usingDirective = scenario.CoverageFiles.Length > 0
			? "using Mockolate.ExampleTests.GeneratorCoverage;\n"
			: string.Empty;
		string program = $$"""
		                   #nullable enable
		                   using System;
		                   using Mockolate;
		                   {{usingDirective}}
		                   namespace Mockolate.SourceGenerators.Tests.SnapshotDriver;

		                   public class Program
		                   {
		                   	public static void Main(string[] args)
		                   	{
		                   		{{scenario.MainBody}}
		                   	}
		                   }
		                   """;
		sources.Add(program);

		return Generator.Run(
			sources.ToArray(),
			DocumentationMode.Parse,
			["NET9_0_OR_GREATER", "NET10_0_OR_GREATER",],
			scenario.AssemblyTypes);
	}

	internal static IReadOnlyDictionary<string, string> NormalizeSources(GeneratorResult result)
	{
		Dictionary<string, string> normalized = new();
		foreach (KeyValuePair<string, string> source in result.Sources
			         .OrderBy(s => s.Key, StringComparer.Ordinal))
		{
			normalized[source.Key] = source.Value.Replace("\r\n", "\n");
		}

		return normalized;
	}

	// The source generator gates `[DebuggerNonUserCode]` behind `#if !DEBUG`, so its output
	// depends on whether the generator dll was built in Debug or Release. Strip those tokens
	// on both sides so the snapshot test passes regardless of the build configuration used to
	// produce the generator. The attribute appears either on its own indented line or inline
	// directly after an opening brace, so the pattern allows optional leading tabs/spaces.
	[GeneratedRegex(@"[ \t]*\[global::System\.Diagnostics\.DebuggerNonUserCode\]\r?\n?", RegexOptions.Compiled)]
	private static partial Regex DebuggerNonUserCodeRegex { get; }

	private static string StripConfigSpecificLines(string content)
		=> DebuggerNonUserCodeRegex.Replace(content, string.Empty);
}
