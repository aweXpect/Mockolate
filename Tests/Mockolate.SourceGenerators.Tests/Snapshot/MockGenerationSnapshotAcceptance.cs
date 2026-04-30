using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace Mockolate.SourceGenerators.Tests.Snapshot;

public sealed class MockGenerationSnapshotAcceptance
{
	/// <summary>
	///     Execute this test to overwrite the expected snapshot files for
	///     <see cref="MockGenerationSnapshotTests" /> with the current generator output.
	/// </summary>
	[Fact(Explicit = true)]
	public async Task AcceptSnapshotChanges()
	{
		(string Scenario, string[] CoverageFiles, string MainBody, Type[] AssemblyTypes)[] scenarios =
		[
			(
				nameof(MockGenerationSnapshotTests.BaseClass_WithMultipleAdditionalInterfaces_CanBeCreated),
				["ComprehensiveAbstractClass.cs", "ICombinationParts.cs",],
				"""
				ComprehensiveAbstractClass sut = ComprehensiveAbstractClass.CreateMock()
					.Implementing<ICombinationMockA>()
					.Implementing<ICombinationMockB>();
				""",
				[]
			),
			(
				nameof(MockGenerationSnapshotTests.ComprehensiveAbstractClass_CanBeCreated),
				["ComprehensiveAbstractClass.cs",],
				"""
				ComprehensiveAbstractClass sut = ComprehensiveAbstractClass.CreateMock();
				""",
				[]
			),
			(
				nameof(MockGenerationSnapshotTests.ComprehensiveDelegate_CanBeCreated),
				["ComprehensiveDelegate.cs",],
				"""
				ComprehensiveDelegate sut = ComprehensiveDelegate.CreateMock();
				""",
				[]
			),
			(
				nameof(MockGenerationSnapshotTests.ComprehensiveInterface_CanBeCreated),
				["ComprehensiveDelegate.cs", "IComprehensiveInterface.cs",],
				"""
				IComprehensiveInterface sut = IComprehensiveInterface.CreateMock();
				""",
				[]
			),
			(
				nameof(MockGenerationSnapshotTests.HttpClient_CanBeCreated),
				[],
				"""
				System.Net.Http.HttpClient sut = System.Net.Http.HttpClient.CreateMock();
				""",
				[typeof(HttpClient), typeof(HttpStatusCode),]
			),
			(
				nameof(MockGenerationSnapshotTests.KeywordEdgeCases_CanBeCreated),
				["KeywordEdgeCases.cs",],
				"""
				IKeywordEdgeCases sut = IKeywordEdgeCases.CreateMock();
				""",
				[]
			),
			(
				nameof(MockGenerationSnapshotTests.RefStructConsumer_CanBeCreated),
				["IRefStructConsumer.cs",],
				"""
				IRefStructConsumer sut = IRefStructConsumer.CreateMock();
				""",
				[]
			),
			(
				nameof(MockGenerationSnapshotTests.StaticAbstractMembers_CanBeCreated),
				["IStaticAbstractMembers.cs",],
				"""
				IStaticAbstractMembers sut = IStaticAbstractMembers.CreateMock();
				""",
				[]
			),
		];

		foreach ((string scenario, string[] coverageFiles, string mainBody, Type[] assemblyTypes) in scenarios)
		{
			GeneratorResult result = MockGenerationSnapshotTests
				.RunGenerator(coverageFiles, mainBody, assemblyTypes);
			IReadOnlyDictionary<string, string> generated =
				MockGenerationSnapshotTests.NormalizeSources(result);
			SnapshotStorage.SetExpected(scenario, generated);
		}

		await That(scenarios).IsNotEmpty();
	}
}
