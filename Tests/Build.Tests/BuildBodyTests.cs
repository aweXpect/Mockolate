namespace Build.Tests;

public sealed class BuildBodyTests
{
	[Fact]
	public async Task BuildBody_BaselineKeyShouldUseTokensUpToTheMeanColumn()
	{
		string[] report =
		[
			"| Method | InvocationCount | Mean | Allocated |",
			"|--------|----------------:|-----:|----------:|",
			"| Foo_Mockolate | 10 | 100 ns | 1 KB |",
		];
		string[] baselineMatchingByMethodOnly =
		[
			"| Method | InvocationCount | Mean | Allocated |",
			"|--------|----------------:|-----:|----------:|",
			"| Foo_Mockolate | 99 | 95 ns | 1 KB |",
		];
		BenchmarkReportFile file = new(report, baselineMatchingByMethodOnly);

		string body = BenchmarkReport.BuildBody([file,], _columnsToRemove);

		await That(body).DoesNotContain("baseline*");
	}

	[Fact]
	public async Task BuildBody_ShouldDropConfiguredColumnsFromHeaderAndDataRows()
	{
		string[] withGen2 =
		[
			"| Method | Mean | Gen2 | Allocated |",
			"|--------|-----:|-----:|----------:|",
			"| Foo_Mockolate | 100 ns | 0.1 | 1 KB |",
			"| Foo_Imposter  | 200 ns | 0.2 | 2 KB |",
		];
		BenchmarkReportFile file = new(withGen2, null);

		string body = BenchmarkReport.BuildBody([file,], _columnsToRemove);

		await That(body).DoesNotContain("Gen2");
		await That(body).DoesNotContain("0.1");
		await That(body).DoesNotContain("0.2");
	}

	[Fact]
	public async Task BuildBody_WhenBaselineHasNoMatchingKey_ShouldOmitInjectionAndOmitNote()
	{
		string[] baseline =
		[
			"| Method | Mean | Allocated |",
			"|--------|-----:|----------:|",
			"| Bar_Mockolate | 95 ns | 1 KB |",
		];
		BenchmarkReportFile file = new(_reportLines, baseline);

		string body = BenchmarkReport.BuildBody([file,], _columnsToRemove);

		await That(body).DoesNotContain("baseline*");
	}

	[Fact]
	public async Task BuildBody_WhenBdnPreBoldsBaselineFile_ShouldEmitItalicBaselineWithoutEmbeddedBold()
	{
		string[] report =
		[
			"| Method | N | Mean | Allocated |",
			"|--------|---|-----:|----------:|",
			"| **Indexer_Mockolate** | **1** | **900 ns** | **3.81 KB** |",
		];
		string[] baseline =
		[
			"| Method | N | Mean | Allocated |",
			"|--------|---|-----:|----------:|",
			"| **Indexer_Mockolate** | **1** | **842 ns** | **3.9 KB** |",
		];
		BenchmarkReportFile file = new(report, baseline);

		string body = BenchmarkReport.BuildBody([file,], _columnsToRemove);

		await That(body).Contains("| _baseline*_ | _1_ | _842 ns_ | _3.9 KB_ |");
		await That(body).DoesNotContain("_**");
		await That(body).DoesNotContain("**_");
	}

	[Fact]
	public async Task BuildBody_WhenBdnPreBoldsTheBaselineRow_ShouldNotEmitDoubleBoldOrBreakPrefix()
	{
		string[] report =
		[
			"| Method | N | Mean | Allocated |",
			"|--------|---|-----:|----------:|",
			"| **Indexer_Mockolate** | **1** | **923 ns** | **3.81 KB** |",
			"| Indexer_Moq | 1 | 212 ns | 20.37 KB |",
			"| Indexer_Imposter | 1 | 947 ns | 5.16 KB |",
		];
		BenchmarkReportFile file = new(report, null);

		string body = BenchmarkReport.BuildBody([file,], _columnsToRemove);

		await That(body).DoesNotContain("****");
		string[] lines = body.Split(Environment.NewLine);
		await That(lines).Contains("| Indexer | N | Mean | Allocated |");
		await That(lines).Contains("| **Mockolate** | **1** | **923 ns** | **3.81 KB** |");
		await That(lines).Contains("| Moq | 1 | 212 ns | 20.37 KB |");
	}

	[Fact]
	public async Task BuildBody_WhenInvocationCountAndMeanMatch_ShouldInjectBaseline()
	{
		string[] report =
		[
			"| Method | InvocationCount | Mean | Allocated |",
			"|--------|----------------:|-----:|----------:|",
			"| Foo_Mockolate | 10 | 100 ns | 1 KB |",
		];
		string[] baseline =
		[
			"| Method | InvocationCount | Mean | Allocated |",
			"|--------|----------------:|-----:|----------:|",
			"| Foo_Mockolate | 10 | 95 ns | 1 KB |",
		];
		BenchmarkReportFile file = new(report, baseline);

		string body = BenchmarkReport.BuildBody([file,], _columnsToRemove);

		await That(body).Contains("_baseline*_");
		await That(body).Contains("_95 ns_");
	}

	[Fact]
	public async Task BuildBody_WhenTableHasEmptyRowSeparatingParameterGroups_ShouldNotEmitBareSeparator()
	{
		string[] report =
		[
			"| Method | N | Mean | Allocated |",
			"|--------|---|-----:|----------:|",
			"| Indexer_Mockolate | 1 | 100 ns | 1 KB |",
			"| Indexer_Moq | 1 | 200 ns | 2 KB |",
			"|         |   |      |          |",
			"| Indexer_Mockolate | 10 | 1000 ns | 10 KB |",
			"| Indexer_Moq | 10 | 2000 ns | 20 KB |",
		];
		BenchmarkReportFile file = new(report, null);

		string body = BenchmarkReport.BuildBody([file,], _columnsToRemove);

		await That(body).DoesNotContain($"{Environment.NewLine}|{Environment.NewLine}");
	}

	[Fact]
	public async Task BuildBody_WhenTableHasEmptyRowSeparatingParameterGroups_ShouldStillStripCommonPrefix()
	{
		string[] report =
		[
			"| Method | N | Mean | Allocated |",
			"|--------|---|-----:|----------:|",
			"| Indexer_Mockolate | 1 | 100 ns | 1 KB |",
			"| Indexer_Moq | 1 | 200 ns | 2 KB |",
			"|         |   |      |          |",
			"| Indexer_Mockolate | 10 | 1000 ns | 10 KB |",
			"| Indexer_Moq | 10 | 2000 ns | 20 KB |",
		];
		BenchmarkReportFile file = new(report, null);

		string body = BenchmarkReport.BuildBody([file,], _columnsToRemove);

		string[] lines = body.Split(Environment.NewLine);
		await That(lines).Contains("| Indexer | N | Mean | Allocated |");
		await That(lines).Contains("| **Mockolate** | **1** | **100 ns** | **1 KB** |");
		await That(lines).Contains("| **Mockolate** | **10** | **1000 ns** | **10 KB** |");
		await That(lines).Contains("| Moq | 1 | 200 ns | 2 KB |");
		await That(lines).Contains("| Moq | 10 | 2000 ns | 20 KB |");
	}

	[Fact]
	public async Task BuildBody_WithBaseline_ShouldInjectItalicBaselineRowBeforeMockolateRow()
	{
		string[] baseline =
		[
			"| Method | Mean | Allocated |",
			"|--------|-----:|----------:|",
			"| Foo_Mockolate | 95 ns | 1 KB |",
		];
		BenchmarkReportFile file = new(_reportLines, baseline);

		string body = BenchmarkReport.BuildBody([file,], _columnsToRemove);

		string[] lines = body.Split(Environment.NewLine);
		int baselineIdx = Array.IndexOf(lines, "| _baseline*_ | _95 ns_ | _1 KB_ |");
		int mockolateIdx = Array.IndexOf(lines, "| **Mockolate** | **100 ns** | **1 KB** |");
		await That(baselineIdx).IsGreaterThan(-1);
		await That(mockolateIdx).IsGreaterThan(baselineIdx);
		await That(body).Contains(
			"`baseline*` rows show the corresponding Mockolate benchmark");
	}

	[Fact]
	public async Task BuildBody_WithoutBaseline_ShouldNotEmitBaselineNote()
	{
		BenchmarkReportFile file = new(_reportLines, null);

		string body = BenchmarkReport.BuildBody([file,], _columnsToRemove);

		await That(body).DoesNotContain("baseline*");
	}

	[Fact]
	public async Task BuildBody_WithoutBaseline_ShouldRenderHeaderAndStripCommonPrefix()
	{
		BenchmarkReportFile file = new(_reportLines, null);

		string body = BenchmarkReport.BuildBody([file,], _columnsToRemove);

		string[] lines = body.Split(Environment.NewLine);
		await That(lines[0]).IsEqualTo("## :rocket: Benchmark Results");
		await That(lines).Contains("<pre>");
		await That(lines).Contains("preamble line");
		await That(lines).Contains("</pre>");
		await That(lines).Contains("| Foo | Mean | Allocated |");
		await That(lines).Contains("| **Mockolate** | **100 ns** | **1 KB** |");
		await That(lines).Contains("| Imposter | 200 ns | 2 KB |");
	}

	private static readonly string[] _columnsToRemove = ["RatioSD", "Gen0", "Gen1", "Gen2",];

	private static readonly string[] _reportLines =
	[
		"```",
		"preamble line",
		"```",
		"| Method | Mean | Allocated |",
		"|--------|-----:|----------:|",
		"| Foo_Mockolate | 100 ns | 1 KB |",
		"| Foo_Imposter  | 200 ns | 2 KB |",
	];
}
