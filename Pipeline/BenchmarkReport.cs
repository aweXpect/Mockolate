using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
// ReSharper disable once RedundantUsingDirective
using System;
// ReSharper disable once RedundantUsingDirective
using System.Collections.Generic;

namespace Build;

/// <summary>
///     Pure logic for transforming BenchmarkDotNet GitHub-flavored markdown reports
///     into the body posted as a PR comment.
/// </summary>
static class BenchmarkReport
{
    // ReSharper disable once UnusedMember.Global
    public static readonly string[] DefaultColumnsToRemove = ["RatioSD", "Gen0", "Gen1", "Gen2",];

    public static string BuildBody(IReadOnlyList<BenchmarkReportFile> files, string[] columnsToRemove)
    {
        StringBuilder sb = new();
        sb.AppendLine("## :rocket: Benchmark Results");
        var anyBaselineInjected = false;
        foreach (var file in files)
        {
            var baselineRows = LoadBaselineRows(file.BaselineLines, columnsToRemove);
            var injectBaseline = baselineRows.Count > 0;
            var count = 0;
            sb.AppendLine();
            sb.AppendLine("<details>");
            sb.AppendLine("<summary>Details</summary>");
            BenchmarkTableParser parser = new(columnsToRemove);
            List<TableRow> tableBuffer = new();
            var tableMeanIndex = -1;
            foreach (var line in file.ReportLines)
            {
                if (line.StartsWith("```"))
                {
                    FlushTableBuffer(sb, tableBuffer, tableMeanIndex, baselineRows, injectBaseline,
                        ref anyBaselineInjected);
                    tableMeanIndex = -1;

                    count++;
                    if (count == 1)
                    {
                        sb.AppendLine("<pre>");
                    }
                    else if (count == 2)
                    {
                        sb.AppendLine("</pre>");
                        sb.AppendLine("</details>");
                        sb.AppendLine();
                    }

                    parser.Reset();
                    continue;
                }

                if (!parser.TryConsume(line, out var rowIndex, out _, out var filteredTokens))
                {
                    FlushTableBuffer(sb, tableBuffer, tableMeanIndex, baselineRows, injectBaseline,
                        ref anyBaselineInjected);
                    tableMeanIndex = -1;
                    sb.AppendLine(line);
                    continue;
                }

                if (rowIndex == 0) tableMeanIndex = parser.MeanIndex;

                tableBuffer.Add(new TableRow(rowIndex, filteredTokens));
            }

            FlushTableBuffer(sb, tableBuffer, tableMeanIndex, baselineRows, injectBaseline, ref anyBaselineInjected);
        }

        if (anyBaselineInjected)
        {
            sb.AppendLine();
            sb.AppendLine(
                "> `baseline*` rows show the corresponding Mockolate benchmark from the most recent successful main branch build with results, for regression comparison.");
        }

        return sb.ToString();
    }

    internal static Dictionary<string, string[]> LoadBaselineRows(string[]? baselineLines, string[] columnsToRemove)
    {
        Dictionary<string, string[]> result = new();
        if (baselineLines == null) return result;

        BenchmarkTableParser parser = new(columnsToRemove);
        foreach (var line in baselineLines)
        {
            if (!parser.TryConsume(line, out var rowIndex, out _, out var tokens)) continue;

            if (rowIndex >= BenchmarkTableParser.DataRowStartIndex
                && parser.MeanIndex > 0
                && tokens.Length > parser.MeanIndex)
            {
                var key = string.Join("|", tokens.Take(parser.MeanIndex));
                result[key] = tokens;
            }
        }

        return result;
    }

    internal static void FlushTableBuffer(StringBuilder sb, List<TableRow> tableBuffer, int meanIndex,
        Dictionary<string, string[]> baselineRows, bool injectBaseline, ref bool anyBaselineInjected)
    {
        if (tableBuffer.Count == 0) return;

        var commonPrefix = FindCommonRowPrefix(tableBuffer);

        var headerRow = tableBuffer[0].RowIndex == 0 ? tableBuffer[0] : null;
        var allocatedIndex = headerRow != null ? FindColumnIndex(headerRow.Tokens, "Allocated") : -1;
        var ratioIndex = headerRow != null ? FindColumnIndex(headerRow.Tokens, "Ratio") : -1;
        var allocRatioIndex = headerRow != null ? FindColumnIndex(headerRow.Tokens, "Alloc Ratio") : -1;

        foreach (var row in tableBuffer)
        {
            if (row.RowIndex >= BenchmarkTableParser.DataRowStartIndex && row.Tokens.Length == 0)
            {
                if (headerRow != null && headerRow.Tokens.Length > 0)
                {
                    var empty = new string[headerRow.Tokens.Length];
                    Array.Fill(empty, string.Empty);
                    sb.AppendLine(JoinTokens(empty));
                }

                continue;
            }

            var displayTokens = ApplyHeaderAndPrefixStripping(row, commonPrefix);
            var displayLine = JoinTokens(displayTokens);

            var isMockolateRow = row.RowIndex >= BenchmarkTableParser.DataRowStartIndex
                                 && row.Tokens.Length > 0
                                 && row.Tokens[0].Contains("_Mockolate", StringComparison.OrdinalIgnoreCase);

            if (isMockolateRow && injectBaseline && meanIndex > 0)
            {
                var key = string.Join("|", row.Tokens.Take(meanIndex));
                if (baselineRows.TryGetValue(key, out var baselineTokens) && baselineTokens.Length > 0)
                {
                    var modifiedBaseline = (string[])baselineTokens.Clone();
                    RecomputeBaselineRatio(modifiedBaseline, row.Tokens, meanIndex, ratioIndex);
                    RecomputeBaselineRatio(modifiedBaseline, row.Tokens, allocatedIndex, allocRatioIndex);

                    var italicized = new string[modifiedBaseline.Length];
                    for (var i = 0; i < modifiedBaseline.Length; i++)
                    {
                        var content = i == 0 ? "baseline*" : modifiedBaseline[i];
                        italicized[i] = string.IsNullOrWhiteSpace(content) ? content : $"_{content}_";
                    }

                    sb.AppendLine(JoinTokens(italicized));
                    anyBaselineInjected = true;
                }
            }

            if (isMockolateRow)
                MakeLineBold(sb, displayLine);
            else
                sb.AppendLine(displayLine);
        }

        tableBuffer.Clear();
    }

    internal static void RecomputeBaselineRatio(string[] baselineTokens, string[] mockolateTokens,
        int valueIndex, int ratioIndex)
    {
        if (valueIndex <= 0 || ratioIndex <= 0) return;

        if (valueIndex >= baselineTokens.Length || valueIndex >= mockolateTokens.Length
                                                || ratioIndex >= baselineTokens.Length)
            return;

        if (!TryParseQuantity(baselineTokens[valueIndex], out var baselineValue)
            || !TryParseQuantity(mockolateTokens[valueIndex], out var mockolateValue)
            || mockolateValue <= 0)
            return;

        baselineTokens[ratioIndex] = (baselineValue / mockolateValue).ToString("F2", CultureInfo.InvariantCulture);
    }

    internal static string[] ApplyHeaderAndPrefixStripping(TableRow row, string? commonPrefix)
    {
        if (commonPrefix == null || row.Tokens.Length == 0) return row.Tokens;

        var result = (string[])row.Tokens.Clone();
        if (row.RowIndex == 0)
            result[0] = commonPrefix.TrimEnd('_');
        else if (row.RowIndex >= BenchmarkTableParser.DataRowStartIndex
                 && result[0].StartsWith(commonPrefix, StringComparison.Ordinal))
            result[0] = result[0].Substring(commonPrefix.Length);

        return result;
    }

    internal static string? FindCommonRowPrefix(List<TableRow> rows)
    {
        string? prefix = null;
        foreach (var row in rows)
        {
            if (row.RowIndex < BenchmarkTableParser.DataRowStartIndex) continue;

            if (row.Tokens.Length == 0) continue;

            var name = row.Tokens[0];
            if (string.Equals(name, "baseline*", StringComparison.Ordinal)) continue;

            var underscoreIdx = name.IndexOf('_');
            if (underscoreIdx <= 0) return null;

            var thisPrefix = name.Substring(0, underscoreIdx + 1);
            if (prefix == null)
                prefix = thisPrefix;
            else if (!string.Equals(prefix, thisPrefix, StringComparison.Ordinal)) return null;
        }

        return prefix;
    }

    internal static string JoinTokens(string[] tokens)
    {
        StringBuilder sb = new();
        sb.Append('|');
        foreach (var token in tokens)
        {
            sb.Append(' ');
            sb.Append(token);
            sb.Append(" |");
        }

        return sb.ToString();
    }

    internal static int FindColumnIndex(string[] tokens, string columnName) =>
        Array.FindIndex(tokens, t => string.Equals(t, columnName, StringComparison.OrdinalIgnoreCase));

    internal static bool TryParseQuantity(string value, out double normalized)
    {
        normalized = 0;
        if (string.IsNullOrWhiteSpace(value)) return false;

        var trimmed = value.Trim();
        var spaceIdx = trimmed.LastIndexOf(' ');
        var numberPart = spaceIdx > 0 ? trimmed.Substring(0, spaceIdx).Trim() : trimmed;
        var unitPart = spaceIdx > 0 ? trimmed.Substring(spaceIdx + 1).Trim() : string.Empty;

        if (!double.TryParse(numberPart,
                NumberStyles.Float | NumberStyles.AllowThousands,
                CultureInfo.InvariantCulture,
                out var number))
            return false;

        var multiplier = unitPart switch
        {
            "" => 1,
            "ps" => 0.001,
            "ns" => 1,
            "μs" => 1000,
            "us" => 1000,
            "ms" => 1_000_000,
            "s" => 1_000_000_000,
            "B" => 1,
            "KB" => 1024,
            "MB" => 1024d * 1024,
            "GB" => 1024d * 1024 * 1024,
            _ => double.NaN,
        };

        if (double.IsNaN(multiplier)) return false;

        normalized = number * multiplier;
        return true;
    }

    internal static int[] DetermineDroppedColumnIndices(string headerLine, string[] columnsToRemove)
    {
        var tokens = SplitTableRow(headerLine);
        List<int> indices = new();
        for (var i = 0; i < tokens.Length; i++)
            foreach (var columnName in columnsToRemove)
                if (string.Equals(tokens[i], columnName, StringComparison.OrdinalIgnoreCase))
                {
                    indices.Add(i);
                    break;
                }

        return indices.ToArray();
    }

    internal static string RemoveColumns(string line, int[] droppedColumnIndices)
    {
        if (droppedColumnIndices.Length == 0) return line;

        var tokens = SplitTableRow(line);
        StringBuilder result = new();
        result.Append('|');
        for (var i = 0; i < tokens.Length; i++)
        {
            if (Array.IndexOf(droppedColumnIndices, i) >= 0) continue;

            result.Append(' ');
            result.Append(tokens[i]);
            result.Append(" |");
        }

        return result.ToString();
    }

    internal static string StripMarkdownBold(string token)
    {
        if (token.Length >= 4 && token.StartsWith("**", StringComparison.Ordinal)
                              && token.EndsWith("**", StringComparison.Ordinal))
            return token.Substring(2, token.Length - 4);

        return token;
    }

    internal static string[] SplitTableRow(string line)
    {
        var parts = line.Split('|', StringSplitOptions.TrimEntries);
        if (parts.Length < 2) return [];

        var tokens = new string[parts.Length - 2];
        Array.Copy(parts, 1, tokens, 0, tokens.Length);
        return tokens;
    }

    internal static void MakeLineBold(StringBuilder sb, string line)
    {
        var tokens = line.Split("|", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        sb.Append('|');
        foreach (var token in tokens)
        {
            sb.Append(" **");
            sb.Append(token);
            sb.Append("** |");
        }

        sb.AppendLine();
    }
}

sealed record BenchmarkReportFile(string[] ReportLines, string[]? BaselineLines);

sealed record TableRow(int RowIndex, string[] Tokens);

/// <summary>
///     Parses the markdown tables produced by BenchmarkDotNet's GitHub exporter.
///     Tables are framed by a header row (index 0), a separator row like <c>|---|---|</c> (index 1), and one or more
///     data rows (index >= <see cref="DataRowStartIndex" />). The parser auto-resets on the first non-table line that
///     follows a table, so a single instance can walk a document that contains multiple tables.
/// </summary>
sealed class BenchmarkTableParser
{
    public const int DataRowStartIndex = 2;

    // ReSharper disable InconsistentNaming
    readonly string[] _columnsToRemove;
    int[]? _droppedColumnIndices;
    int _nextRowIndex;
    // ReSharper restore InconsistentNaming

    public BenchmarkTableParser(string[] columnsToRemove)
    {
        _columnsToRemove = columnsToRemove;
    }

    public int MeanIndex { get; private set; } = -1;

    public bool TryConsume(string line, out int rowIndex,
        [NotNullWhen(true)] out string? filteredLine,
        [NotNullWhen(true)] out string[]? tokens)
    {
        rowIndex = -1;
        if (!line.StartsWith('|') || !line.EndsWith('|'))
        {
            if (_nextRowIndex > 0) Reset();

            filteredLine = null;
            tokens = null;
            return false;
        }

        _droppedColumnIndices ??= BenchmarkReport.DetermineDroppedColumnIndices(line, _columnsToRemove);
        filteredLine = BenchmarkReport.RemoveColumns(line, _droppedColumnIndices);
        tokens = filteredLine.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < tokens.Length; i++) tokens[i] = BenchmarkReport.StripMarkdownBold(tokens[i]);

        if (_nextRowIndex == 0)
            MeanIndex = Array.FindIndex(tokens,
                t => string.Equals(t, "Mean", StringComparison.OrdinalIgnoreCase));

        rowIndex = _nextRowIndex;
        _nextRowIndex++;
        return true;
    }

    public void Reset()
    {
        _droppedColumnIndices = null;
        MeanIndex = -1;
        _nextRowIndex = 0;
    }
}
