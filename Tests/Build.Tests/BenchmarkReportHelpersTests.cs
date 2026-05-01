namespace Build.Tests;

public sealed class BenchmarkReportHelpersTests
{
	[Fact]
	public async Task ApplyHeaderAndPrefixStripping_ShouldRemoveCommonPrefixFromDataRow()
	{
		TableRow row = new(BenchmarkTableParser.DataRowStartIndex, ["PropRead_Mockolate", "1",]);

		string[] result = BenchmarkReport.ApplyHeaderAndPrefixStripping(row, "PropRead_");

		await That(result).IsEqualTo(["Mockolate", "1",]);
	}

	[Fact]
	public async Task ApplyHeaderAndPrefixStripping_ShouldStripTrailingUnderscoreFromHeader()
	{
		TableRow header = new(0, ["Method", "Mean",]);

		string[] result = BenchmarkReport.ApplyHeaderAndPrefixStripping(header, "PropRead_");

		await That(result).IsEqualTo(["PropRead", "Mean",]);
	}

	[Fact]
	public async Task ApplyHeaderAndPrefixStripping_WhenCommonPrefixIsNull_ShouldReturnTokensUnchanged()
	{
		TableRow row = new(BenchmarkTableParser.DataRowStartIndex, ["Foo", "1",]);

		string[] result = BenchmarkReport.ApplyHeaderAndPrefixStripping(row, null);

		await That(result).IsEqualTo(["Foo", "1",]);
	}

	[Fact]
	public async Task DetermineDroppedColumnIndices_ShouldMatchCaseInsensitivelyAndReturnIndicesInHeaderOrder()
	{
		int[] indices = BenchmarkReport.DetermineDroppedColumnIndices(
			"| Method | Mean | Gen0 | gen1 | Allocated |",
			["GEN0", "Gen1",]);

		await That(indices).IsEqualTo([2, 3,]);
	}

	[Fact]
	public async Task DetermineDroppedColumnIndices_WhenNoColumnsMatch_ShouldReturnEmpty()
	{
		int[] indices = BenchmarkReport.DetermineDroppedColumnIndices(
			"| Method | Mean | Allocated |",
			["Gen0",]);

		await That(indices).IsEqualTo(Array.Empty<int>());
	}

	[Fact]
	public async Task FindCommonRowPrefix_ShouldIgnoreHeaderAndSeparatorRows()
	{
		List<TableRow> rows =
		[
			new(0, ["Method", "Mean",]),
			new(1, ["---", "---",]),
		];

		string prefix = BenchmarkReport.FindCommonRowPrefix(rows);

		await That(prefix).IsNull();
	}

	[Fact]
	public async Task FindCommonRowPrefix_ShouldSkipBaselineMarkerRow()
	{
		List<TableRow> rows =
		[
			new(BenchmarkTableParser.DataRowStartIndex, ["baseline*", "1",]),
			new(3, ["Indexer_Mockolate", "1",]),
			new(4, ["Indexer_Moq", "1",]),
		];

		string prefix = BenchmarkReport.FindCommonRowPrefix(rows);

		await That(prefix).IsEqualTo("Indexer_");
	}

	[Fact]
	public async Task FindCommonRowPrefix_ShouldSkipEmptyDataRowAndUseRemainingRows()
	{
		List<TableRow> rows =
		[
			new(BenchmarkTableParser.DataRowStartIndex, ["Indexer_Mockolate", "1",]),
			new(3, ["Indexer_Moq", "1",]),
			new(4, []),
			new(5, ["Indexer_Mockolate", "10",]),
			new(6, ["Indexer_Moq", "10",]),
		];

		string prefix = BenchmarkReport.FindCommonRowPrefix(rows);

		await That(prefix).IsEqualTo("Indexer_");
	}

	[Fact]
	public async Task FindCommonRowPrefix_WhenADataRowHasNoUnderscore_ShouldReturnNull()
	{
		List<TableRow> rows =
		[
			new(BenchmarkTableParser.DataRowStartIndex, ["NoUnderscore", "1",]),
		];

		string prefix = BenchmarkReport.FindCommonRowPrefix(rows);

		await That(prefix).IsNull();
	}

	[Fact]
	public async Task FindCommonRowPrefix_WhenAllDataRowsShareAPrefix_ShouldReturnPrefixWithTrailingUnderscore()
	{
		List<TableRow> rows =
		[
			new(0, ["Method", "Mean",]),
			new(1, ["---", "---",]),
			new(BenchmarkTableParser.DataRowStartIndex, ["PropRead_Mockolate", "1",]),
			new(3, ["PropRead_Imposter", "2",]),
		];

		string prefix = BenchmarkReport.FindCommonRowPrefix(rows);

		await That(prefix).IsEqualTo("PropRead_");
	}

	[Fact]
	public async Task FindCommonRowPrefix_WhenDataRowsDisagree_ShouldReturnNull()
	{
		List<TableRow> rows =
		[
			new(BenchmarkTableParser.DataRowStartIndex, ["A_Mockolate", "1",]),
			new(3, ["B_Imposter", "2",]),
		];

		string prefix = BenchmarkReport.FindCommonRowPrefix(rows);

		await That(prefix).IsNull();
	}

	[Fact]
	public async Task RemoveColumns_ShouldDropTheGivenIndicesAndPreserveTheRest()
	{
		string result = BenchmarkReport.RemoveColumns(
			"| Method | Mean | Gen0 | Gen1 | Allocated |",
			[2, 3,]);

		await That(result).IsEqualTo("| Method | Mean | Allocated |");
	}

	[Fact]
	public async Task RemoveColumns_WhenIndicesAreEmpty_ShouldReturnLineUnchanged()
	{
		string original = "| Method | Mean |";

		string result = BenchmarkReport.RemoveColumns(original, []);

		await That(result).IsEqualTo(original);
	}

	[Fact]
	public async Task SplitTableRow_ShouldReturnTrimmedInteriorCells()
	{
		string[] tokens = BenchmarkReport.SplitTableRow("|  Foo |  bar baz  | 42 |");

		await That(tokens).IsEqualTo(["Foo", "bar baz", "42",]);
	}

	[Fact]
	public async Task SplitTableRow_WhenLineHasNoPipe_ShouldReturnEmptyArray()
	{
		string[] tokens = BenchmarkReport.SplitTableRow("hello world");

		await That(tokens).IsEqualTo(Array.Empty<string>());
	}
}
