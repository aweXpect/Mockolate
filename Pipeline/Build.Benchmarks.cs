using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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

			string prNumber = File.ReadAllText(ArtifactsDirectory / "PR.txt");
			string body = CreateBenchmarkCommentBody(files);
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

	string CreateBenchmarkCommentBody(string[] files)
	{
		StringBuilder sb = new();
		sb.AppendLine("## :rocket: Benchmark Results");
		string[] columnsToRemove = ["RatioSD", "Gen0", "Gen1", "Gen2",];
		foreach (string file in files)
		{
			int count = 0;
			string[] lines = File.ReadAllLines(file);
			sb.AppendLine();
			sb.AppendLine("<details>");
			sb.AppendLine("<summary>Details</summary>");
			int[] droppedColumnIndices = null;
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
					continue;
				}

				if (line.StartsWith('|') && line.EndsWith('|'))
				{
					droppedColumnIndices ??= DetermineDroppedColumnIndices(line, columnsToRemove);
					string filteredLine = RemoveColumns(line, droppedColumnIndices);
					if (filteredLine.Contains("_Mockolate", StringComparison.OrdinalIgnoreCase))
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

		string body = sb.ToString();
		return body;
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
