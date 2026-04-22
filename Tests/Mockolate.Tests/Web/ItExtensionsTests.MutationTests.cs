using System.Net.Http;
using Mockolate.Exceptions;
using Mockolate.Verify;
using Mockolate.Web;

namespace Mockolate.Tests.Web;

public sealed class ItExtensionsMutationTests
{
	[Fact]
	public async Task IsHttpContent_WithBytes_CalledTwice_ShouldUseFirstPredicate()
	{
		HttpClient httpClient = HttpClient.CreateMock();
		byte[] payload = { 1, 2, 3, };

		await httpClient.PostAsync("https://aweXpect.com", new ByteArrayContent(payload));

		// The first WithBytes predicate matches (length == 3) while the second does not.
		// Original (??=): first wins, verification succeeds.
		// Mutant (=): second wins, verification fails.
		await That(httpClient.Mock.Verify.PostAsync(
				It.IsAny<string?>(),
				It.IsHttpContent()
					.WithBytes(b => b.Length == 3)
					.WithBytes(b => b.Length == 99)))
			.Once();
	}

	[Fact]
	public async Task IsHttpContent_WithMultipleWithString_FailingVerification_ShouldJoinPredicatesWithAnd()
	{
		HttpClient httpClient = HttpClient.CreateMock();

		void Act()
			=> httpClient.Mock.Verify.PostAsync(
					It.IsAny<string?>(),
					It.IsHttpContent()
						.WithString(s => s.StartsWith("a"))
						.WithString(s => s.EndsWith("z")))
				.AtLeastOnce();

		await That(Act).Throws<MockVerificationException>()
			.WithMessage("*and*").AsWildcard();
	}

	[Fact]
	public async Task IsUri_WithTrailingSlashOnRequestAndNoSuffixWildcardInPattern_ShouldStillMatch()
	{
		HttpClient httpClient = HttpClient.CreateMock();

		await httpClient.GetAsync("https://aweXpect.com/");

		await That(httpClient.Mock.Verify.GetAsync(It.IsUri("https://aweXpect.com")))
			.Once();
	}
}
