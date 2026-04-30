using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
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
	[Fact]
	public async Task BaseClass_WithMultipleAdditionalInterfaces_CanBeCreated()
		=> await VerifySnapshot(
			[
				"ComprehensiveAbstractClass.cs",
				"ICombinationParts.cs",
			],
			"""
			ComprehensiveAbstractClass sut = ComprehensiveAbstractClass.CreateMock()
				.Implementing<ICombinationMockA>()
				.Implementing<ICombinationMockB>();
			""");

	[Fact]
	public async Task ComprehensiveAbstractClass_CanBeCreated()
		=> await VerifySnapshot(
			[
				"ComprehensiveAbstractClass.cs",
			],
			"""
			ComprehensiveAbstractClass sut = ComprehensiveAbstractClass.CreateMock();
			""");

	[Fact]
	public async Task ComprehensiveDelegate_CanBeCreated()
		=> await VerifySnapshot(
			[
				"ComprehensiveDelegate.cs",
			],
			"""
			ComprehensiveDelegate sut = ComprehensiveDelegate.CreateMock();
			""");

	[Fact]
	public async Task ComprehensiveInterface_CanBeCreated()
		=> await VerifySnapshot(
			[
				"ComprehensiveDelegate.cs",
				"IComprehensiveInterface.cs",
			],
			"""
			IComprehensiveInterface sut = IComprehensiveInterface.CreateMock();
			""");

	[Fact]
	public async Task HttpClient_CanBeCreated()
		=> await VerifySnapshot(
			[],
			"""
			System.Net.Http.HttpClient sut = System.Net.Http.HttpClient.CreateMock();
			""",
			[typeof(HttpClient), typeof(HttpStatusCode),]);

	[Fact]
	public async Task KeywordEdgeCases_CanBeCreated()
		=> await VerifySnapshot(
			[
				"KeywordEdgeCases.cs",
			],
			"""
			IKeywordEdgeCases sut = IKeywordEdgeCases.CreateMock();
			""");

	[Fact]
	public async Task RefStructConsumer_CanBeCreated()
		=> await VerifySnapshot(
			[
				"IRefStructConsumer.cs",
			],
			"""
			IRefStructConsumer sut = IRefStructConsumer.CreateMock();
			""");

	[Fact]
	public async Task StaticAbstractMembers_CanBeCreated()
		=> await VerifySnapshot(
			[
				"IStaticAbstractMembers.cs",
			],
			"""
			IStaticAbstractMembers sut = IStaticAbstractMembers.CreateMock();
			""");

	internal static GeneratorResult RunGenerator(string[] coverageFiles, string mainBody,
		params Type[] assemblyTypes)
	{
		List<string> sources = new();
		foreach (string coverageFile in coverageFiles)
		{
			sources.Add("#nullable enable\n" + SnapshotStorage.ReadCoverageFile(coverageFile));
		}

		string usingDirective = coverageFiles.Length > 0
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
		                   		{{mainBody}}
		                   	}
		                   }
		                   """;
		sources.Add(program);

		return Generator.Run(
			sources.ToArray(),
			DocumentationMode.Parse,
			["NET9_0_OR_GREATER", "NET10_0_OR_GREATER",],
			assemblyTypes);
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

	private static async Task VerifySnapshot(string[] coverageFiles, string mainBody,
		Type[]? assemblyTypes = null, [CallerMemberName] string scenario = "")
	{
		GeneratorResult result = RunGenerator(coverageFiles, mainBody, assemblyTypes ?? []);
		await That(result.Diagnostics).IsEmpty();

		IReadOnlyDictionary<string, string> generated = NormalizeSources(result);
		IReadOnlyDictionary<string, string> expected = SnapshotStorage.GetExpected(scenario);

		await That(generated.Keys).IsEqualTo(expected.Keys).InAnyOrder();

		foreach (string fileName in expected.Keys)
		{
			await That(StripConfigSpecificLines(generated[fileName]))
				.IsEqualTo(StripConfigSpecificLines(expected[fileName]))
				.IgnoringNewlineStyle();
		}
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
