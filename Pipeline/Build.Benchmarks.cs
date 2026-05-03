using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Git;
using Octokit;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

// ReSharper disable AllUnderscoreLocalParameterName

namespace Build;

partial class Build
{
	private const string BenchmarkBranch = "benchmarks";
	private const string BenchmarkDataPath = "Docs/pages/static/js/data.js";
	private const string BenchmarkLimitedDataPath = "Docs/pages/static/js/limited-data.js";
	private const int BenchmarkLimit = 50;

	[Parameter("Filter for BenchmarkDotNet - Default is '*'")] readonly string BenchmarkFilter = "*";

	[Parameter("Maximum number of historic Build runs to seed via 'BenchmarkSeedHistory' - Default is 30")]
	readonly int BenchmarkSeedRunLimit = 30;

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
					await gitHubClient.Issue.Comment.GetAllForIssue(BuildExtensions.Owner, BuildExtensions.Repo, prId);
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
					await gitHubClient.Issue.Comment.Create(BuildExtensions.Owner, BuildExtensions.Repo, prId, body);
				}
				else
				{
					Log.Information($"Update comment:\n{body}");
					await gitHubClient.Issue.Comment.Update(BuildExtensions.Owner, BuildExtensions.Repo, commentId.Value, body);
				}
			}
		});

	Target Benchmarks => _ => _
		.DependsOn(BenchmarkDotNet)
		.DependsOn(BenchmarkResult);

	Target PublishBenchmarkReport => _ => _
		.Description("Aggregates BenchmarkDotNet JSON results from the matrix Benchmarks-* artifacts of the " +
		             "current workflow run, then commits an updated data.js / limited-data.js to the " +
		             $"'{BenchmarkBranch}' branch.")
		.Requires(() => GithubToken)
		.Executes(async () =>
		{
			await "Benchmarks-".DownloadArtifactsStartingWith(ArtifactsDirectory, GithubToken);

			List<string> benchmarkReports = LoadBenchmarkJsonReports(ArtifactsDirectory / "Benchmarks" / "results");
			if (benchmarkReports.Count == 0)
			{
				Log.Warning("Skip benchmark report: no benchmark JSON reports found.");
				return;
			}

			PageBenchmarkReportGenerator.CommitInfo commitInfo = ReadCurrentCommitInfo();
			Log.Information(
				"Appending benchmark data for commit {Sha} ({Author}, {Date}): {Message}",
				commitInfo.Sha, commitInfo.Author, commitInfo.Date, commitInfo.Message);

			await BuildExtensions.EnsureBranchExistsAsync(BenchmarkBranch, "main", GithubToken);

			BuildExtensions.GithubFile dataFile =
				await BuildExtensions.ReadBranchFileAsync(BenchmarkDataPath, BenchmarkBranch, GithubToken);
			BuildExtensions.GithubFile limitedFile =
				await BuildExtensions.ReadBranchFileAsync(BenchmarkLimitedDataPath, BenchmarkBranch, GithubToken);

			(string updated, string limited) = PageBenchmarkReportGenerator.Append(
				commitInfo,
				dataFile?.Content ?? string.Empty,
				benchmarkReports,
				BenchmarkLimit);

			if (string.IsNullOrWhiteSpace(updated))
			{
				Log.Information("No changes to publish (commit already recorded).");
				return;
			}

			string commitMessage =
				$"Update benchmark for {commitInfo.Sha.Substring(0, 8)}: {commitInfo.Message} by {commitInfo.Author}";
			await BuildExtensions.WriteBranchFileAsync(BenchmarkDataPath, BenchmarkBranch, commitMessage, updated,
				dataFile?.Sha, GithubToken);
			await BuildExtensions.WriteBranchFileAsync(BenchmarkLimitedDataPath, BenchmarkBranch, commitMessage,
				limited, limitedFile?.Sha, GithubToken);
		});

	Target BenchmarkSeedHistory => _ => _
		.Description("Seeds the benchmark data files on the 'benchmarks' branch with results from past " +
		             "successful 'Build' workflow runs on main, by downloading their Benchmarks-* artifacts. " +
		             "One-shot tool; safe to re-run (commits already recorded are skipped).")
		.Requires(() => GithubToken)
		.Executes(async () =>
		{
			List<BuildExtensions.WorkflowRunInfo> runs = await BuildExtensions.ListSuccessfulRunsAsync(
				"build.yml", "main", BenchmarkSeedRunLimit, GithubToken);
			if (runs.Count == 0)
			{
				Log.Warning("No successful 'Build' runs found on main - nothing to seed.");
				return;
			}

			runs.Reverse();
			Log.Information("Seeding from {Count} historical runs (oldest first).", runs.Count);

			await BuildExtensions.EnsureBranchExistsAsync(BenchmarkBranch, "main", GithubToken);

			BuildExtensions.GithubFile dataFile =
				await BuildExtensions.ReadBranchFileAsync(BenchmarkDataPath, BenchmarkBranch, GithubToken);
			BuildExtensions.GithubFile limitedFile =
				await BuildExtensions.ReadBranchFileAsync(BenchmarkLimitedDataPath, BenchmarkBranch, GithubToken);

			string accumulatedContent = dataFile?.Content ?? string.Empty;
			string accumulatedLimited = limitedFile?.Content ?? string.Empty;
			string lastFullSha = dataFile?.Sha;
			string lastLimitedSha = limitedFile?.Sha;
			int updatedRuns = 0;

			foreach (BuildExtensions.WorkflowRunInfo run in runs)
			{
				AbsolutePath runDirectory = ArtifactsDirectory / "SeedHistory" / run.Id.ToString();
				runDirectory.CreateOrCleanDirectory();
				try
				{
					await BuildExtensions.DownloadArtifactsFromRunStartingWith(run.Id, "Benchmarks-",
						runDirectory, GithubToken);
				}
				catch (Exception ex)
				{
					Log.Warning(ex, "Skipping run #{RunId} ({Sha}): could not download artifacts.", run.Id,
						run.Sha?.Substring(0, 8));
					continue;
				}

				List<string> benchmarkReports = LoadBenchmarkJsonReports(runDirectory / "Benchmarks" / "results");
				if (benchmarkReports.Count == 0)
				{
					Log.Information("Run #{RunId} ({Sha}) had no benchmark JSON reports - skipping.", run.Id,
						run.Sha?.Substring(0, 8));
					continue;
				}

				PageBenchmarkReportGenerator.CommitInfo commitInfo = new(
					run.Sha,
					run.Author ?? "unknown",
					run.Date ?? string.Empty,
					run.Message ?? string.Empty);

				(string updated, string limited) = PageBenchmarkReportGenerator.Append(
					commitInfo,
					accumulatedContent,
					benchmarkReports,
					BenchmarkLimit);

				if (string.IsNullOrWhiteSpace(updated))
				{
					Log.Information("Run #{RunId} ({Sha}) already recorded - skipping.", run.Id,
						run.Sha?.Substring(0, 8));
					continue;
				}

				accumulatedContent = updated;
				accumulatedLimited = limited;
				updatedRuns++;
				Log.Information("Seeded run #{RunId} ({Sha}): {Message}", run.Id,
					run.Sha?.Substring(0, 8), commitInfo.Message);
			}

			if (updatedRuns == 0)
			{
				Log.Information("No new benchmark data to seed.");
				return;
			}

			string commitMessage = $"Seed historic benchmark data ({updatedRuns} runs)";
			await BuildExtensions.WriteBranchFileAsync(BenchmarkDataPath, BenchmarkBranch, commitMessage,
				accumulatedContent, lastFullSha, GithubToken);
			await BuildExtensions.WriteBranchFileAsync(BenchmarkLimitedDataPath, BenchmarkBranch, commitMessage,
				accumulatedLimited, lastLimitedSha, GithubToken);
			Log.Information("Seeded benchmark data from {Count} historical runs.", updatedRuns);
		});

	private static List<string> LoadBenchmarkJsonReports(AbsolutePath resultsDirectory)
	{
		List<string> reports = new();
		if (!Directory.Exists(resultsDirectory))
		{
			return reports;
		}

		foreach (string file in Directory.GetFiles(resultsDirectory, "*-report-full-compressed.json"))
		{
			reports.Add(File.ReadAllText(file));
		}

		return reports;
	}

	private static PageBenchmarkReportGenerator.CommitInfo ReadCurrentCommitInfo()
	{
		Output[] lines = GitTasks.Git("log -1").ToArray();
		string commitId = null, author = null, date = null, message = null;
		foreach (string line in lines.Select(x => x.Text))
		{
			if (commitId == null && line.StartsWith("commit "))
			{
				commitId = line.Substring("commit ".Length).Substring(0, 40);
				continue;
			}

			if (author == null && line.StartsWith("Author: "))
			{
				author = line.Substring("Author: ".Length);
				int index = author.IndexOf(" <", StringComparison.Ordinal);
				if (index > 0)
				{
					author = author.Substring(0, index);
				}

				continue;
			}

			if (date == null && line.StartsWith("Date:   "))
			{
				date = line.Substring("Date:   ".Length);
				continue;
			}

			if (commitId != null && author != null && date != null && !string.IsNullOrWhiteSpace(line))
			{
				message = line.Trim();
				break;
			}
		}

		return new PageBenchmarkReportGenerator.CommitInfo(commitId, author, date, message);
	}

	async Task<string> DownloadBaselineBenchmarks(AbsolutePath baselineDirectory)
	{
		long[] candidateRunIds = await BuildExtensions.FindRecentSuccessfulRunIds("build.yml", "main", 10, GithubToken);
		if (candidateRunIds.Length == 0)
		{
			Log.Information("No successful main 'Build' run found - skipping baseline column.");
			return null;
		}

		AbsolutePath resultsDirectory = baselineDirectory / "Benchmarks" / "results";
		foreach (long runId in candidateRunIds)
		{
			baselineDirectory.CreateOrCleanDirectory();
			try
			{
				await BuildExtensions.DownloadArtifactsFromRunStartingWith(runId, "Benchmarks-",
					baselineDirectory, GithubToken);
			}
			catch (Exception ex)
			{
				Log.Warning(ex, "Failed to download artifacts from main run #{RunId}, trying older run.", runId);
				continue;
			}

			if (Directory.Exists(resultsDirectory))
			{
				Log.Information("Loaded baseline benchmark results from main run #{RunId}.", runId);
				return resultsDirectory;
			}

			Log.Information("Main run #{RunId} did not contain benchmark results, trying older run.", runId);
		}

		Log.Information(
			"No baseline benchmark results found in the last {Count} successful main runs - skipping baseline column.",
			candidateRunIds.Length);
		return null;
	}

	static string CreateBenchmarkCommentBody(string[] files, string baselineResultsDirectory)
	{
		List<BenchmarkReportFile> inputs = new();
		foreach (string file in files)
		{
			string[] reportLines = File.ReadAllLines(file);
			string[] baselineLines = null;
			if (baselineResultsDirectory != null)
			{
				string baselineFile = Path.Combine(baselineResultsDirectory, Path.GetFileName(file));
				if (File.Exists(baselineFile))
				{
					baselineLines = File.ReadAllLines(baselineFile);
				}
			}

			inputs.Add(new BenchmarkReportFile(reportLines, baselineLines));
		}

		return BenchmarkReport.BuildBody(inputs, BenchmarkReport.DefaultColumnsToRemove);
	}
}
