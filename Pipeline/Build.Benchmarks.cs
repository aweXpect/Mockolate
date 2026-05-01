using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;
using Octokit;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

// ReSharper disable AllUnderscoreLocalParameterName

namespace Build;

partial class Build
{
	[Parameter("Filter for BenchmarkDotNet - Default is '*'")] readonly string BenchmarkFilter = "*";

	Target BenchmarkDotNet => _ => _
		.Executes(() =>
		{
			AbsolutePath benchmarkDirectory = ArtifactsDirectory / "Benchmarks";
			benchmarkDirectory.CreateOrCleanDirectory();

			DotNetBuild(s => s
				.SetProjectFile(Solution.Benchmarks.Mockolate_Benchmarks)
				.SetConfiguration(Configuration.Release)
				.EnableNoLogo());

			DotNet(
				$"{Solution.Benchmarks.Mockolate_Benchmarks.Name}.dll --exporters json --filter {BenchmarkFilter} --artifacts \"{benchmarkDirectory}\"",
				Solution.Benchmarks.Mockolate_Benchmarks.Directory / "bin" / "Release");
		});

	Target BenchmarkResult => _ => _
		.After(BenchmarkDotNet)
		.Executes(async () =>
		{
			if (!Directory.Exists(ArtifactsDirectory / "Benchmarks" / "results"))
			{
				Log.Information("Skip benchmark result, because no results directory was generated.");
				return;
			}

			string[] files = Directory.GetFiles(ArtifactsDirectory / "Benchmarks" / "results", "*-report-github.md");
			if (files.Length == 0)
			{
				Log.Information("Skip benchmark result, because no report file was generated.");
				return;
			}

			foreach (string file in files)
			{
				string fileContent = await File.ReadAllTextAsync(file);
				Log.Information("Report ({FileName}):\n {FileContent}", Path.GetFileName(file), fileContent);
			}

			if (GitHubActions?.IsPullRequest == true)
			{
				File.WriteAllText(ArtifactsDirectory / "PR.txt", GitHubActions.PullRequestNumber.ToString());
			}
		});

	Target BenchmarkComment => _ => _
		.Executes(async () =>
		{
			await "Benchmarks-".DownloadArtifactsStartingWith(ArtifactsDirectory, GithubToken);
			if (!Directory.Exists(ArtifactsDirectory / "Benchmarks" / "results"))
			{
				Log.Information("Skip benchmark comment, because no results directory was generated.");
				return;
			}

			string[] files = Directory.GetFiles(ArtifactsDirectory / "Benchmarks" / "results", "*-report-github.md");
			if (files.Length == 0)
			{
				Log.Information("Skip benchmark comment, because no report file was generated.");
				return;
			}

			if (!File.Exists(ArtifactsDirectory / "PR.txt"))
			{
				Log.Information("Skip writing a comment, as no PR number was specified.");
				return;
			}

			AbsolutePath baselineDirectory = ArtifactsDirectory / "Baseline";
			string baselineResultsDirectory = await DownloadBaselineBenchmarks(baselineDirectory);

			string prNumber = File.ReadAllText(ArtifactsDirectory / "PR.txt");
			string body = CreateBenchmarkCommentBody(files, baselineResultsDirectory);
			Log.Debug("Pull request number: {PullRequestId}", prNumber);
			if (int.TryParse(prNumber, out int prId))
			{
				GitHubClient gitHubClient = new(new ProductHeaderValue("Nuke"));
				Credentials tokenAuth = new(GithubToken);
				gitHubClient.Credentials = tokenAuth;
				IReadOnlyList<IssueComment> comments =
					await gitHubClient.Issue.Comment.GetAllForIssue("aweXpect", "Mockolate", prId);
				long? commentId = null;
				Log.Information($"Found {comments.Count} comments");
				foreach (IssueComment comment in comments)
				{
					if (comment.Body.Contains("## :rocket: Benchmark Results"))
					{
						Log.Information($"Found comment: {comment.Body}");
						commentId = comment.Id;
					}
				}

				if (commentId == null)
				{
					Log.Information($"Create comment:\n{body}");
					await gitHubClient.Issue.Comment.Create("aweXpect", "Mockolate", prId, body);
				}
				else
				{
					Log.Information($"Update comment:\n{body}");
					await gitHubClient.Issue.Comment.Update("aweXpect", "Mockolate", commentId.Value, body);
				}
			}
		});

	Target Benchmarks => _ => _
		.DependsOn(BenchmarkDotNet)
		.DependsOn(BenchmarkResult);

	async Task<string> DownloadBaselineBenchmarks(AbsolutePath baselineDirectory)
	{
		long? runId = await BuildExtensions.FindLatestSuccessfulRun("build.yml", "main", GithubToken);
		if (runId == null)
		{
			Log.Information("No successful main 'Build' run found - skipping baseline column.");
			return null;
		}

		baselineDirectory.CreateOrCleanDirectory();
		await BuildExtensions.DownloadArtifactsFromRunStartingWith(runId.Value, "Benchmarks-",
			baselineDirectory, GithubToken);

		string resultsDirectory = baselineDirectory / "Benchmarks" / "results";
		if (!Directory.Exists(resultsDirectory))
		{
			Log.Information("Baseline run #{RunId} did not contain benchmark results - skipping baseline column.",
				runId.Value);
			return null;
		}

		Log.Information("Loaded baseline benchmark results from main run #{RunId}.", runId.Value);
		return resultsDirectory;
	}

	string CreateBenchmarkCommentBody(string[] files, string baselineResultsDirectory)
	{
		StringBuilder sb = new();
		sb.AppendLine("## :rocket: Benchmark Results");
		string[] columnsToRemove = ["RatioSD", "Gen0", "Gen1", "Gen2",];
		bool anyBaselineInjected = false;
		foreach (string file in files)
		{
			Dictionary<string, string[]> baselineRows = LoadBaselineRows(
				baselineResultsDirectory == null ? null : Path.Combine(baselineResultsDirectory, Path.GetFileName(file)),
				columnsToRemove);
			bool injectBaseline = baselineRows.Count > 0;
			int count = 0;
			string[] lines = File.ReadAllLines(file);
			sb.AppendLine();
			sb.AppendLine("<details>");
			sb.AppendLine("<summary>Details</summary>");
			int[] droppedColumnIndices = null;
			int meanIndex = -1;
			int tableRowIndex = 0;
			foreach (string line in lines)
			{
				if (line.StartsWith("```"))
				{
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

					droppedColumnIndices = null;
					meanIndex = -1;
					tableRowIndex = 0;
					continue;
				}

				if (line.StartsWith('|') && line.EndsWith('|'))
				{
					droppedColumnIndices ??= DetermineDroppedColumnIndices(line, columnsToRemove);
					string filteredLine = RemoveColumns(line, droppedColumnIndices);
					string[] filteredTokens = filteredLine.Split('|',
						StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

					if (tableRowIndex == 0)
					{
						meanIndex = Array.FindIndex(filteredTokens,
							t => string.Equals(t, "Mean", StringComparison.OrdinalIgnoreCase));
					}

					tableRowIndex++;

					bool isMockolateRow = filteredLine.Contains("_Mockolate", StringComparison.OrdinalIgnoreCase);

					if (isMockolateRow && injectBaseline && meanIndex > 0)
					{
						string key = string.Join("|", filteredTokens.Take(meanIndex));
						if (baselineRows.TryGetValue(key, out string[] baselineTokens) && baselineTokens.Length > 0)
						{
							string[] modifiedBaseline = new string[baselineTokens.Length];
							for (int i = 0; i < baselineTokens.Length; i++)
							{
								string content = i == 0 ? "baseline*" : baselineTokens[i];
								modifiedBaseline[i] = string.IsNullOrWhiteSpace(content) ? content : $"_{content}_";
							}

							sb.AppendLine(JoinTokens(modifiedBaseline));
							anyBaselineInjected = true;
						}
					}

					if (isMockolateRow)
					{
						MakeLineBold(sb, filteredLine);
						continue;
					}

					sb.AppendLine(filteredLine);
					continue;
				}

				sb.AppendLine(line);
			}
		}

		if (anyBaselineInjected)
		{
			sb.AppendLine();
			sb.AppendLine(
				"> `baseline*` rows show the corresponding Mockolate benchmark from the latest successful main branch build, for regression comparison.");
		}

		string body = sb.ToString();
		return body;
	}

	static Dictionary<string, string[]> LoadBaselineRows(string baselineFile, string[] columnsToRemove)
	{
		Dictionary<string, string[]> result = new();
		if (baselineFile == null || !File.Exists(baselineFile))
		{
			return result;
		}

		int[] droppedColumnIndices = null;
		int meanIndex = -1;
		int tableRowIndex = 0;
		foreach (string line in File.ReadAllLines(baselineFile))
		{
			if (!line.StartsWith('|') || !line.EndsWith('|'))
			{
				if (tableRowIndex > 0)
				{
					droppedColumnIndices = null;
					meanIndex = -1;
					tableRowIndex = 0;
				}

				continue;
			}

			droppedColumnIndices ??= DetermineDroppedColumnIndices(line, columnsToRemove);
			string filtered = RemoveColumns(line, droppedColumnIndices);
			string[] tokens = filtered.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
			if (tableRowIndex == 0)
			{
				meanIndex = Array.FindIndex(tokens,
					t => string.Equals(t, "Mean", StringComparison.OrdinalIgnoreCase));
			}
			else if (tableRowIndex >= 2 && meanIndex > 0 && meanIndex <= tokens.Length)
			{
				string key = string.Join("|", tokens.Take(meanIndex));
				result[key] = tokens;
			}

			tableRowIndex++;
		}

		return result;
	}

	static string JoinTokens(string[] tokens)
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

	static int[] DetermineDroppedColumnIndices(string headerLine, string[] columnsToRemove)
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

	static string RemoveColumns(string line, int[] droppedColumnIndices)
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

	static string[] SplitTableRow(string line)
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

	static void MakeLineBold(StringBuilder sb, string line)
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
