using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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

	/// <summary>
	///     Ensures that the given branch exists in the repository. If it does not, creates it pointing at the head
	///     of <paramref name="sourceBranch" />. Returns <see langword="true" /> if the branch was newly created.
	/// </summary>
	public static async Task<bool> EnsureBranchExistsAsync(string branch, string sourceBranch, string githubToken)
	{
		using HttpClient client = CreateGithubClient(githubToken);
		HttpResponseMessage exists = await client.GetAsync(
			$"{RepositoryApiBaseUrl}/git/ref/heads/{Uri.EscapeDataString(branch)}");
		if (exists.IsSuccessStatusCode)
		{
			return false;
		}

		HttpResponseMessage source = await client.GetAsync(
			$"{RepositoryApiBaseUrl}/git/ref/heads/{Uri.EscapeDataString(sourceBranch)}");
		if (!source.IsSuccessStatusCode)
		{
			string errorContent = await source.Content.ReadAsStringAsync();
			throw new InvalidOperationException(
				$"Could not read source branch '{sourceBranch}': {errorContent}");
		}

		string sourceContent = await source.Content.ReadAsStringAsync();
		using JsonDocument document = JsonDocument.Parse(sourceContent);
		string sha = document.RootElement.GetProperty("object").GetProperty("sha").GetString();

		string body = JsonSerializer.Serialize(new
		{
			@ref = $"refs/heads/{branch}",
			sha,
		});
		HttpResponseMessage create = await client.PostAsync($"{RepositoryApiBaseUrl}/git/refs",
			new StringContent(body, Encoding.UTF8, "application/json"));
		if (!create.IsSuccessStatusCode)
		{
			string errorContent = await create.Content.ReadAsStringAsync();
			throw new InvalidOperationException(
				$"Could not create branch '{branch}' from '{sourceBranch}': {errorContent}");
		}

		Log.Information("Created branch '{Branch}' from '{SourceBranch}' at {Sha}.", branch, sourceBranch,
			sha?.Substring(0, 8));
		return true;
	}

	/// <summary>
	///     Reads a file from the given <paramref name="branch" /> using the GitHub contents API.
	///     Returns <see langword="null" /> if the file does not exist on that branch.
	/// </summary>
	public static async Task<GithubFile> ReadBranchFileAsync(string path, string branch, string githubToken)
	{
		using HttpClient client = CreateGithubClient(githubToken);
		HttpResponseMessage response = await client.GetAsync(
			$"{RepositoryApiBaseUrl}/contents/{path}?ref={Uri.EscapeDataString(branch)}");
		if (!response.IsSuccessStatusCode)
		{
			return null;
		}

		string responseContent = await response.Content.ReadAsStringAsync();
		using JsonDocument document = JsonDocument.Parse(responseContent);
		string sha = document.RootElement.GetProperty("sha").GetString();
		string downloadUrl =
			$"https://raw.githubusercontent.com/{Owner}/{Repo}/refs/heads/{branch}/{path}";
		using HttpClient rawClient = new();
		rawClient.DefaultRequestHeaders.UserAgent.ParseAdd(Repo);
		string content = await rawClient.GetStringAsync(downloadUrl);
		return new GithubFile(content, sha);
	}

	/// <summary>
	///     Writes a file to the given <paramref name="branch" /> using the GitHub contents API.
	/// </summary>
	public static async Task WriteBranchFileAsync(string path, string branch, string commitMessage, string content,
		string existingSha, string githubToken)
	{
		using HttpClient client = CreateGithubClient(githubToken);
		GithubUpdateFile body = new(commitMessage, Base64Encode(content), existingSha, branch);
		HttpResponseMessage response = await client.PutAsync(
			$"{RepositoryApiBaseUrl}/contents/{path}",
			new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"));
		if (response.IsSuccessStatusCode)
		{
			Log.Information("Updated {Path} on branch '{Branch}'.", path, branch);
		}
		else
		{
			string responseContent = await response.Content.ReadAsStringAsync();
			throw new InvalidOperationException(
				$"Could not update '{path}' on branch '{branch}': {responseContent}");
		}
	}

	/// <summary>
	///     Lists successful runs of the given workflow on the given branch (newest first), returning the run id
	///     plus the head commit metadata (sha, author, date, message).
	/// </summary>
	public static async Task<List<WorkflowRunInfo>> ListSuccessfulRunsAsync(string workflowFileName, string branch,
		int count, string githubToken)
	{
		using HttpClient client = CreateGithubClient(githubToken);
		string url = $"{RepositoryApiBaseUrl}/actions/workflows/{Uri.EscapeDataString(workflowFileName)}/runs" +
		             $"?status=success&branch={Uri.EscapeDataString(branch)}&per_page={count}";
		HttpResponseMessage response = await client.GetAsync(url);
		string responseContent = await response.Content.ReadAsStringAsync();
		if (!response.IsSuccessStatusCode)
		{
			Log.Warning(
				$"Could not list recent runs for workflow '{workflowFileName}' on branch '{branch}': {responseContent}");
			return new List<WorkflowRunInfo>();
		}

		List<WorkflowRunInfo> result = new();
		using JsonDocument document = JsonDocument.Parse(responseContent);
		foreach (JsonElement run in document.RootElement.GetProperty("workflow_runs").EnumerateArray())
		{
			long id = run.GetProperty("id").GetInt64();
			string sha = run.GetProperty("head_sha").GetString();
			string headBranch = run.TryGetProperty("head_branch", out JsonElement hb) ? hb.GetString() : null;
			string createdAt = run.TryGetProperty("created_at", out JsonElement ca) ? ca.GetString() : null;
			string author = null;
			string message = null;
			string commitDate = createdAt;
			if (run.TryGetProperty("head_commit", out JsonElement hc) && hc.ValueKind == JsonValueKind.Object)
			{
				if (hc.TryGetProperty("message", out JsonElement m))
				{
					message = m.GetString()?.Split('\n')[0]?.Trim();
				}

				if (hc.TryGetProperty("timestamp", out JsonElement ts))
				{
					commitDate = ts.GetString() ?? createdAt;
				}

				if (hc.TryGetProperty("author", out JsonElement aut) && aut.ValueKind == JsonValueKind.Object &&
				    aut.TryGetProperty("name", out JsonElement an))
				{
					author = an.GetString();
				}
			}

			result.Add(new WorkflowRunInfo(id, sha, headBranch, author, commitDate, message));
		}

		return result;
	}

	private static HttpClient CreateGithubClient(string githubToken)
	{
		HttpClient client = new();
		client.DefaultRequestHeaders.UserAgent.ParseAdd(Repo);
		if (!string.IsNullOrEmpty(githubToken))
		{
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", githubToken);
		}

		return client;
	}

	private static string Base64Encode(string plainText)
	{
		byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
		return Convert.ToBase64String(plainTextBytes);
	}

	public record GithubFile(string Content, string Sha);

	public record WorkflowRunInfo(long Id, string Sha, string Branch, string Author, string Date, string Message);

	// ReSharper disable InconsistentNaming
	// ReSharper disable NotAccessedPositionalProperty.Local
	private record GithubUpdateFile(string message, string content, string sha, string branch);
	// ReSharper restore NotAccessedPositionalProperty.Local
	// ReSharper restore InconsistentNaming
}
