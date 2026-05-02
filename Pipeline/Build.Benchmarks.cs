using System;
using System.Collections.Generic;
using System.IO;
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
