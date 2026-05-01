using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Build;

/// <summary>
///     Pure logic for transforming BenchmarkDotNet GitHub-flavored markdown reports
///     into the body posted as a PR comment.
/// </summary>
internal static class BenchmarkReport
{
	public static readonly string[] DefaultColumnsToRemove = ["RatioSD", "Gen0", "Gen1", "Gen2",];

	public static string BuildBody(IReadOnlyList<BenchmarkReportFile> files, string[] columnsToRemove)
	{
		StringBuilder sb = new();
		sb.AppendLine("## :rocket: Benchmark Results");
		bool anyBaselineInjected = false;
		foreach (BenchmarkReportFile file in files)
		{
			Dictionary<string, string[]> baselineRows = LoadBaselineRows(file.BaselineLines, columnsToRemove);
			bool injectBaseline = baselineRows.Count > 0;
			int count = 0;
			sb.AppendLine();
			sb.AppendLine("<details>");
			sb.AppendLine("<summary>Details</summary>");
			BenchmarkTableParser parser = new(columnsToRemove);
			List<TableRow> tableBuffer = new();
			int tableMeanIndex = -1;
			foreach (string line in file.ReportLines)
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

				if (!parser.TryConsume(line, out int rowIndex, out _, out string[] filteredTokens))
				{
					FlushTableBuffer(sb, tableBuffer, tableMeanIndex, baselineRows, injectBaseline,
						ref anyBaselineInjected);
					tableMeanIndex = -1;
					sb.AppendLine(line);
					continue;
				}

				if (rowIndex == 0)
				{
					tableMeanIndex = parser.MeanIndex;
				}

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

	internal static Dictionary<string, string[]> LoadBaselineRows(string[] baselineLines, string[] columnsToRemove)
	{
		Dictionary<string, string[]> result = new();
		if (baselineLines == null)
		{
			return result;
		}

		BenchmarkTableParser parser = new(columnsToRemove);
		foreach (string line in baselineLines)
		{
			if (!parser.TryConsume(line, out int rowIndex, out _, out string[] tokens))
			{
				continue;
			}

			if (rowIndex >= BenchmarkTableParser.DataRowStartIndex
			    && parser.MeanIndex > 0
			    && tokens.Length > parser.MeanIndex)
			{
				string key = string.Join("|", tokens.Take(parser.MeanIndex));
				result[key] = tokens;
			}
		}

		return result;
	}

	internal static void FlushTableBuffer(StringBuilder sb, List<TableRow> tableBuffer, int meanIndex,
		Dictionary<string, string[]> baselineRows, bool injectBaseline, ref bool anyBaselineInjected)
	{
		if (tableBuffer.Count == 0)
		{
			return;
		}

		string commonPrefix = FindCommonRowPrefix(tableBuffer);

		TableRow headerRow = tableBuffer[0].RowIndex == 0 ? tableBuffer[0] : null;
		int allocatedIndex = headerRow != null ? FindColumnIndex(headerRow.Tokens, "Allocated") : -1;
		int ratioIndex = headerRow != null ? FindColumnIndex(headerRow.Tokens, "Ratio") : -1;
		int allocRatioIndex = headerRow != null ? FindColumnIndex(headerRow.Tokens, "Alloc Ratio") : -1;

		foreach (TableRow row in tableBuffer)
		{
			if (row.RowIndex >= BenchmarkTableParser.DataRowStartIndex && row.Tokens.Length == 0)
			{
				if (headerRow != null && headerRow.Tokens.Length > 0)
				{
					string[] empty = new string[headerRow.Tokens.Length];
					Array.Fill(empty, string.Empty);
					sb.AppendLine(JoinTokens(empty));
				}

				continue;
			}

			string[] displayTokens = ApplyHeaderAndPrefixStripping(row, commonPrefix);
			string displayLine = JoinTokens(displayTokens);

			bool isMockolateRow = row.RowIndex >= BenchmarkTableParser.DataRowStartIndex
			                      && row.Tokens.Length > 0
			                      && row.Tokens[0].Contains("_Mockolate", StringComparison.OrdinalIgnoreCase);

			if (isMockolateRow && injectBaseline && meanIndex > 0)
			{
				string key = string.Join("|", row.Tokens.Take(meanIndex));
				if (baselineRows.TryGetValue(key, out string[] baselineTokens) && baselineTokens.Length > 0)
				{
					string[] modifiedBaseline = (string[])baselineTokens.Clone();
					RecomputeBaselineRatio(modifiedBaseline, row.Tokens, meanIndex, ratioIndex);
					RecomputeBaselineRatio(modifiedBaseline, row.Tokens, allocatedIndex, allocRatioIndex);

					string[] italicized = new string[modifiedBaseline.Length];
					for (int i = 0; i < modifiedBaseline.Length; i++)
					{
						string content = i == 0 ? "baseline*" : modifiedBaseline[i];
						italicized[i] = string.IsNullOrWhiteSpace(content) ? content : $"_{content}_";
					}

					sb.AppendLine(JoinTokens(italicized));
					anyBaselineInjected = true;
				}
			}

			if (isMockolateRow)
			{
				MakeLineBold(sb, displayLine);
			}
			else
			{
				sb.AppendLine(displayLine);
			}
		}

		tableBuffer.Clear();
	}

	internal static void RecomputeBaselineRatio(string[] baselineTokens, string[] mockolateTokens,
		int valueIndex, int ratioIndex)
	{
		if (valueIndex <= 0 || ratioIndex <= 0)
		{
			return;
		}

		if (valueIndex >= baselineTokens.Length || valueIndex >= mockolateTokens.Length
		                                        || ratioIndex >= baselineTokens.Length)
		{
			return;
		}

		if (!TryParseQuantity(baselineTokens[valueIndex], out double baselineValue)
		    || !TryParseQuantity(mockolateTokens[valueIndex], out double mockolateValue)
		    || mockolateValue <= 0)
		{
			return;
		}

		baselineTokens[ratioIndex] = (baselineValue / mockolateValue).ToString("F2", CultureInfo.InvariantCulture);
	}

	internal static string[] ApplyHeaderAndPrefixStripping(TableRow row, string commonPrefix)
	{
		if (commonPrefix == null || row.Tokens.Length == 0)
		{
			return row.Tokens;
		}

		string[] result = (string[])row.Tokens.Clone();
		if (row.RowIndex == 0)
		{
			result[0] = commonPrefix.TrimEnd('_');
		}
		else if (row.RowIndex >= BenchmarkTableParser.DataRowStartIndex
		         && result[0].StartsWith(commonPrefix, StringComparison.Ordinal))
		{
			result[0] = result[0].Substring(commonPrefix.Length);
		}

		return result;
	}

	internal static string FindCommonRowPrefix(List<TableRow> rows)
	{
		string prefix = null;
		foreach (TableRow row in rows)
		{
			if (row.RowIndex < BenchmarkTableParser.DataRowStartIndex)
			{
				continue;
			}

			if (row.Tokens.Length == 0)
			{
				continue;
			}

			string name = row.Tokens[0];
			if (string.Equals(name, "baseline*", StringComparison.Ordinal))
			{
				continue;
			}

			int underscoreIdx = name.IndexOf('_');
			if (underscoreIdx <= 0)
			{
				return null;
			}

			string thisPrefix = name.Substring(0, underscoreIdx + 1);
			if (prefix == null)
			{
				prefix = thisPrefix;
			}
			else if (!string.Equals(prefix, thisPrefix, StringComparison.Ordinal))
			{
				return null;
			}
		}

		return prefix;
	}

	internal static string JoinTokens(string[] tokens)
	{
		StringBuilder sb = new();
		sb.Append('|');
		foreach (string token in tokens)
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
		if (string.IsNullOrWhiteSpace(value))
		{
			return false;
		}

		string trimmed = value.Trim();
		int spaceIdx = trimmed.LastIndexOf(' ');
		string numberPart = spaceIdx > 0 ? trimmed.Substring(0, spaceIdx).Trim() : trimmed;
		string unitPart = spaceIdx > 0 ? trimmed.Substring(spaceIdx + 1).Trim() : string.Empty;

		if (!double.TryParse(numberPart,
			NumberStyles.Float | NumberStyles.AllowThousands,
			CultureInfo.InvariantCulture,
			out double number))
		{
			return false;
		}

		double multiplier = unitPart switch
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

		if (double.IsNaN(multiplier))
		{
			return false;
		}

		normalized = number * multiplier;
		return true;
	}

	internal static int[] DetermineDroppedColumnIndices(string headerLine, string[] columnsToRemove)
	{
		string[] tokens = SplitTableRow(headerLine);
		List<int> indices = new();
		for (int i = 0; i < tokens.Length; i++)
		{
			foreach (string columnName in columnsToRemove)
			{
				if (string.Equals(tokens[i], columnName, StringComparison.OrdinalIgnoreCase))
				{
					indices.Add(i);
					break;
				}
			}
		}

		return indices.ToArray();
	}

	internal static string RemoveColumns(string line, int[] droppedColumnIndices)
	{
		if (droppedColumnIndices.Length == 0)
		{
			return line;
		}

		string[] tokens = SplitTableRow(line);
		StringBuilder result = new();
		result.Append('|');
		for (int i = 0; i < tokens.Length; i++)
		{
			if (Array.IndexOf(droppedColumnIndices, i) >= 0)
			{
				continue;
			}

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
		{
			return token.Substring(2, token.Length - 4);
		}

		return token;
	}

	internal static string[] SplitTableRow(string line)
	{
		string[] parts = line.Split('|', StringSplitOptions.TrimEntries);
		if (parts.Length < 2)
		{
			return [];
		}

		string[] tokens = new string[parts.Length - 2];
		Array.Copy(parts, 1, tokens, 0, tokens.Length);
		return tokens;
	}

	internal static void MakeLineBold(StringBuilder sb, string line)
	{
		string[] tokens = line.Split("|", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
		sb.Append('|');
		foreach (string token in tokens)
		{
			sb.Append(" **");
			sb.Append(token);
			sb.Append("** |");
		}

		sb.AppendLine();
	}
}

internal sealed record BenchmarkReportFile(string[] ReportLines, string[] BaselineLines);

internal sealed record TableRow(int RowIndex, string[] Tokens);

/// <summary>
///     Parses the markdown tables produced by BenchmarkDotNet's GitHub exporter.
///     Tables are framed by a header row (index 0), a separator row like <c>|---|---|</c> (index 1), and one or more
///     data rows (index >= <see cref="DataRowStartIndex" />). The parser auto-resets on the first non-table line that
///     follows a table, so a single instance can walk a document that contains multiple tables.
/// </summary>
internal sealed class BenchmarkTableParser
{
	public const int DataRowStartIndex = 2;

	readonly string[] _columnsToRemove;
	int[] _droppedColumnIndices;
	int _nextRowIndex;

	public BenchmarkTableParser(string[] columnsToRemove)
	{
		_columnsToRemove = columnsToRemove;
	}

	public int MeanIndex { get; private set; } = -1;

	public bool TryConsume(string line, out int rowIndex, out string filteredLine, out string[] tokens)
	{
		rowIndex = -1;
		filteredLine = null;
		tokens = null;
		if (!line.StartsWith('|') || !line.EndsWith('|'))
		{
			if (_nextRowIndex > 0)
			{
				Reset();
			}

			return false;
		}

		_droppedColumnIndices ??= BenchmarkReport.DetermineDroppedColumnIndices(line, _columnsToRemove);
		filteredLine = BenchmarkReport.RemoveColumns(line, _droppedColumnIndices);
		tokens = filteredLine.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < tokens.Length; i++)
		{
			tokens[i] = BenchmarkReport.StripMarkdownBold(tokens[i]);
		}

		if (_nextRowIndex == 0)
		{
			MeanIndex = Array.FindIndex(tokens,
				t => string.Equals(t, "Mean", StringComparison.OrdinalIgnoreCase));
		}

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
