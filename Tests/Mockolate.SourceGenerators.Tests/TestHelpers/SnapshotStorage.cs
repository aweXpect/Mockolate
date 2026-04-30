using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Mockolate.SourceGenerators.Tests.TestHelpers;

public static class SnapshotStorage
{
	public static string ReadCoverageFile(string coverageFileName)
	{
		string path = CombinedPaths("Tests", "Mockolate.Tests", "GeneratorCoverage",
			coverageFileName);
		return File.ReadAllText(path);
	}

	public static IReadOnlyDictionary<string, string> GetExpected(string scenario)
	{
		string folder = ExpectedFolder(scenario);
		Dictionary<string, string> result = new();
		if (!Directory.Exists(folder))
		{
			return result;
		}

		foreach (string file in Directory.GetFiles(folder).OrderBy(f => f, StringComparer.Ordinal))
		{
			string content = File.ReadAllText(file).Replace("\r\n", "\n");
			result[Path.GetFileName(file)] = content;
		}

		return result;
	}

	public static void SetExpected(string scenario, IReadOnlyDictionary<string, string> sources)
	{
		string folder = ExpectedFolder(scenario);
		if (Directory.Exists(folder))
		{
			Directory.Delete(folder, true);
		}

		Directory.CreateDirectory(folder);
		foreach (KeyValuePair<string, string> source in sources)
		{
			string content = source.Value
				.Replace("\r\n", "\n")
				.Replace("\n", Environment.NewLine);
			File.WriteAllText(Path.Combine(folder, source.Key), content);
		}
	}

	private static string ExpectedFolder(string scenario) =>
		CombinedPaths("Tests", "Mockolate.SourceGenerators.Tests", "Snapshot", "Expected", scenario);

	private static string CombinedPaths(params string[] paths) =>
		Path.GetFullPath(Path.Combine(paths.Prepend(GetSolutionDirectory()).ToArray()));

	private static string GetSolutionDirectory([CallerFilePath] string path = "") =>
		Path.Combine(Path.GetDirectoryName(path)!, "..", "..", "..");
}
