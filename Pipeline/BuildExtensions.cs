using System;
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

	public static async Task DownloadArtifactTo(this string artifactName, string artifactsDirectory, string githubToken)
	{
		string runId = Environment.GetEnvironmentVariable("WorkflowRunId");
		if (string.IsNullOrEmpty(runId))
		{
			Log.Information("Skip downloading artifacts, because no 'WorkflowRunId' environment variable is set.");
			return;
		}

		using HttpClient client = new();
		client.DefaultRequestHeaders.UserAgent.ParseAdd("Mockerade");
		client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", githubToken);
		HttpResponseMessage response = await client.GetAsync(
			$"https://api.github.com/repos/Mockerade/Mockerade/actions/runs/{runId}/artifacts");

		string responseContent = await response.Content.ReadAsStringAsync();
		if (!response.IsSuccessStatusCode)
		{
			throw new InvalidOperationException(
				$"Could not find artifacts for run #{runId}': {responseContent}");
		}

		try
		{
			JsonDocument jsonDocument = JsonDocument.Parse(responseContent);
			foreach (JsonElement artifact in jsonDocument.RootElement.GetProperty("artifacts").EnumerateArray())
			{
				string name = artifact.GetProperty("name").GetString()!;
				if (name.Equals(artifactName, StringComparison.OrdinalIgnoreCase))
				{
					long artifactId = artifact.GetProperty("id").GetInt64();
					HttpResponseMessage fileResponse = await client.GetAsync(
						$"https://api.github.com/repos/Mockerade/Mockerade/actions/artifacts/{artifactId}/zip");
					if (fileResponse.IsSuccessStatusCode)
					{
						using ZipArchive archive = new(await fileResponse.Content.ReadAsStreamAsync());
						archive.ExtractToDirectory(artifactsDirectory);
						Log.Information(
							$"Extracted artifact #{artifactId} with {archive.Entries.Count} entries to {artifactsDirectory}:\n - {string.Join("\n - ", archive.Entries.Select(entry => $"{entry.Name} ({entry.Length})"))}");
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
