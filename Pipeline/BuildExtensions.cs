using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.SonarScanner;
using Serilog;

namespace Build;

public static class BuildExtensions
{
	internal const string Owner = "Testably";
	internal const string Repo = "Mockolate";
	private const string RepositoryApiBaseUrl = "https://api.github.com/repos/" + Owner + "/" + Repo;

	public static SonarScannerBeginSettings SetPullRequestOrBranchName(
		this SonarScannerBeginSettings settings,
		GitHubActions gitHubActions,
		GitVersion gitVersion)
	{
		if (gitHubActions?.IsPullRequest == true)
		{
			Log.Information("Use pull request analysis");
			return settings
				.SetPullRequestKey(gitHubActions.PullRequestNumber.ToString())
				.SetPullRequestBranch(gitHubActions.Ref)
				.SetPullRequestBase(gitHubActions.BaseRef);
		}

		if (gitHubActions?.Ref.StartsWith("refs/tags/", StringComparison.OrdinalIgnoreCase) == true)
		{
			string version = gitHubActions.Ref.Substring("refs/tags/".Length);
			string branchName = "release/" + version;
			Log.Information("Use release branch analysis for '{BranchName}'", branchName);
			return settings.SetBranchName(branchName);
		}

		Log.Information("Use branch analysis for '{BranchName}'", gitVersion.BranchName);
		return settings.SetBranchName(gitVersion.BranchName);
	}

	public static Task DownloadArtifactTo(this string artifactName, string artifactsDirectory, string githubToken)
		=> DownloadArtifactsWhere(name => name.Equals(artifactName, StringComparison.OrdinalIgnoreCase),
			artifactsDirectory, githubToken);

	public static Task DownloadArtifactsStartingWith(this string artifactNamePrefix, string artifactsDirectory,
		string githubToken)
		=> DownloadArtifactsWhere(name => name.StartsWith(artifactNamePrefix, StringComparison.OrdinalIgnoreCase),
			artifactsDirectory, githubToken);

	public static async Task<long[]> FindRecentSuccessfulRunIds(string workflowFileName, string branch, int count,
		string githubToken)
	{
		if (string.IsNullOrEmpty(githubToken))
		{
			throw new ArgumentException("A GitHub token is required.", nameof(githubToken));
		}

		using HttpClient client = new();
		client.DefaultRequestHeaders.UserAgent.ParseAdd("Mockolate");
		client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", githubToken);

		string url = $"{RepositoryApiBaseUrl}/actions/workflows/{Uri.EscapeDataString(workflowFileName)}/runs" +
		             $"?status=success&branch={Uri.EscapeDataString(branch)}&per_page={count}";
		HttpResponseMessage response = await client.GetAsync(url);
		string responseContent = await response.Content.ReadAsStringAsync();
		if (!response.IsSuccessStatusCode)
		{
			Log.Warning(
				$"Could not find recent runs for workflow '{workflowFileName}' on branch '{branch}': {responseContent}");
			return [];
		}

		try
		{
			using JsonDocument jsonDocument = JsonDocument.Parse(responseContent);
			JsonElement runs = jsonDocument.RootElement.GetProperty("workflow_runs");
			long[] ids = new long[runs.GetArrayLength()];
			for (int i = 0; i < ids.Length; i++)
			{
				ids[i] = runs[i].GetProperty("id").GetInt64();
			}

			return ids;
		}
		catch (Exception e) when (e is JsonException or KeyNotFoundException or InvalidOperationException)
		{
			Log.Error($"Could not parse workflow runs response: {e.Message}\n{responseContent}");
			return [];
		}
	}

	public static Task DownloadArtifactsFromRunStartingWith(long runId, string artifactNamePrefix,
		string artifactsDirectory, string githubToken)
		=> DownloadArtifactsFromRun(runId.ToString(),
			name => name.StartsWith(artifactNamePrefix, StringComparison.OrdinalIgnoreCase),
			artifactsDirectory, githubToken);

	private static Task DownloadArtifactsWhere(Func<string, bool> namePredicate, string artifactsDirectory,
		string githubToken)
	{
		string runId = Environment.GetEnvironmentVariable("WorkflowRunId");
		if (string.IsNullOrEmpty(runId))
		{
			Log.Information("Skip downloading artifacts, because no 'WorkflowRunId' environment variable is set.");
			return Task.CompletedTask;
		}

		return DownloadArtifactsFromRun(runId, namePredicate, artifactsDirectory, githubToken);
	}

	private static async Task DownloadArtifactsFromRun(string runId, Func<string, bool> namePredicate,
		string artifactsDirectory, string githubToken)
	{
		using HttpClient client = new();
		client.DefaultRequestHeaders.UserAgent.ParseAdd("Mockolate");
		client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", githubToken);
		HttpResponseMessage response = await client.GetAsync(
			$"{RepositoryApiBaseUrl}/actions/runs/{runId}/artifacts");

		string responseContent = await response.Content.ReadAsStringAsync();
		if (!response.IsSuccessStatusCode)
		{
			throw new InvalidOperationException(
				$"Could not find artifacts for run #{runId}': {responseContent}");
		}

		try
		{
			using JsonDocument jsonDocument = JsonDocument.Parse(responseContent);
			foreach (JsonElement artifact in jsonDocument.RootElement.GetProperty("artifacts").EnumerateArray())
			{
				string name = artifact.GetProperty("name").GetString()!;
				if (namePredicate(name))
				{
					long artifactId = artifact.GetProperty("id").GetInt64();
					HttpResponseMessage fileResponse = await client.GetAsync(
						$"{RepositoryApiBaseUrl}/actions/artifacts/{artifactId}/zip");
					if (fileResponse.IsSuccessStatusCode)
					{
						using ZipArchive archive = new(await fileResponse.Content.ReadAsStreamAsync());
						archive.ExtractToDirectory(artifactsDirectory, true);
						Log.Information(
							$"Extracted artifact '{name}' (#{artifactId}) with {archive.Entries.Count} entries to {artifactsDirectory}:\n - {string.Join("\n - ", archive.Entries.Select(entry => $"{entry.Name} ({entry.Length})"))}");
					}
					else
					{
						string fileResponseContent = await fileResponse.Content.ReadAsStringAsync();
						throw new InvalidOperationException(
							$"Could not download the artifacts with id #{artifactId}': {fileResponseContent}");
					}
				}
			}
		}
		catch (JsonException e)
		{
			Log.Error($"Could not parse JSON: {e.Message}\n{responseContent}");
		}
	}
}
