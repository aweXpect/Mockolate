using System.Collections.Generic;

namespace Mockolate.SourceGenerators.Tests.Snapshot;

public sealed class MockGenerationSnapshotAcceptance
{
	/// <summary>
	///     Execute this test to overwrite the expected snapshot files for
	///     <see cref="MockGenerationSnapshotTests" /> with the current generator output.
	/// </summary>
	[Fact(Explicit = true)]
	public void AcceptSnapshotChanges()
	{
		foreach (SnapshotScenario scenario in MockGenerationSnapshotTests.Scenarios)
		{
			GeneratorResult result = MockGenerationSnapshotTests.RunGenerator(scenario);
			IReadOnlyDictionary<string, string> generated =
				MockGenerationSnapshotTests.NormalizeSources(result);
			SnapshotStorage.SetExpected(scenario.Name, generated);
		}
	}
}
