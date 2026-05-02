using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Mockolate.SourceGenerators.Tests.TestHelpers;

public static partial class SnapshotStorage
{
    [GeneratedRegex(@"[ \t]*\[global::System\.Diagnostics\.DebuggerNonUserCode\]\r?\n?", RegexOptions.Compiled)]
    private static partial Regex DebuggerNonUserCodeRegex { get; }

    public static string ReadCoverageFile(string coverageFileName)
    {
        var path = CombinedPaths("Tests", "Mockolate.Tests", "GeneratorCoverage",
            coverageFileName);
        return File.ReadAllText(path);
    }

    public static IReadOnlyDictionary<string, string> GetExpected(string scenario)
    {
        var folder = ExpectedFolder(scenario);
        Dictionary<string, string> result = new();
        if (!Directory.Exists(folder)) return result;

        foreach (var file in Directory.GetFiles(folder).OrderBy(f => f, StringComparer.Ordinal))
        {
            var content = File.ReadAllText(file).Replace("\r\n", "\n");
            result[Path.GetFileName(file)] = content;
        }

        return result;
    }

    public static void SetExpected(string scenario, IReadOnlyDictionary<string, string> sources)
    {
        var folder = ExpectedFolder(scenario);
        if (Directory.Exists(folder)) Directory.Delete(folder, true);

        Directory.CreateDirectory(folder);
        foreach (var source in sources)
        {
            var content = StripConfigSpecificLines(source.Value
                .Replace("\r\n", "\n")
                .Replace("\n", Environment.NewLine));
            File.WriteAllText(Path.Combine(folder, source.Key), content);
        }
    }

    /// <summary>
    ///     The source generator gates `[DebuggerNonUserCode]` behind `#if !DEBUG`, so its output
    ///     depends on whether the generator dll was built in Debug or Release. Strip those tokens
    ///     on both sides so the snapshot test passes regardless of the build configuration used to
    ///     produce the generator. The attribute appears either on its own indented line or inline
    ///     directly after an opening brace, so the pattern allows optional leading tabs/spaces.
    /// </summary>
    internal static string StripConfigSpecificLines(string content)
        => DebuggerNonUserCodeRegex.Replace(content, string.Empty);

    private static string ExpectedFolder(string scenario) =>
        CombinedPaths("Tests", "Mockolate.SourceGenerators.Tests", "Snapshot", "Expected", scenario);

    private static string CombinedPaths(params string[] paths) =>
        Path.GetFullPath(Path.Combine(paths.Prepend(GetSolutionDirectory()).ToArray()));

    private static string GetSolutionDirectory([CallerFilePath] string path = "") =>
        Path.Combine(Path.GetDirectoryName(path)!, "..", "..", "..");
}
