using System.Net.Http;
using Mockolate.Exceptions;
using Mockolate.Verify;
using Mockolate.Web;

namespace Mockolate.Tests.Web;

public sealed partial class HttpClientExtensionsTests
{
	public sealed partial class Verify
	{
		public sealed class ExpectationMessageTests
		{
			[Fact]
			public async Task DeleteAsync_StringUri_FailingVerification_ShouldMentionSendAsyncInMessage()
			{
				HttpClient httpClient = HttpClient.CreateMock();

				void Act()
				{
					httpClient.Mock.Verify.DeleteAsync(It.Matches("*aweXpect*")).AtLeastOnce();
				}

				await That(Act).Throws<MockVerificationException>()
					.WithMessage("*SendAsync*").AsWildcard();
			}

			[Fact]
			public async Task DeleteAsync_Uri_FailingVerification_ShouldMentionSendAsyncInMessage()
			{
				HttpClient httpClient = HttpClient.CreateMock();

				void Act()
				{
					httpClient.Mock.Verify.DeleteAsync(It.IsUri("*aweXpect*")).AtLeastOnce();
				}

				await That(Act).Throws<MockVerificationException>()
					.WithMessage("*SendAsync*").AsWildcard();
			}

			[Fact]
			public async Task PutAsync_StringUri_FailingVerification_ShouldMentionSendAsyncInMessage()
			{
				HttpClient httpClient = HttpClient.CreateMock();

				void Act()
				{
					httpClient.Mock.Verify.PutAsync(It.Matches("*aweXpect*"), It.IsAny<HttpContent>()).AtLeastOnce();
				}

				await That(Act).Throws<MockVerificationException>()
					.WithMessage("*SendAsync*").AsWildcard();
			}

			[Fact]
			public async Task PutAsync_Uri_FailingVerification_ShouldMentionSendAsyncInMessage()
			{
				HttpClient httpClient = HttpClient.CreateMock();

				void Act()
				{
					httpClient.Mock.Verify.PutAsync(It.IsUri("*aweXpect*"), It.IsAny<HttpContent>()).AtLeastOnce();
				}

				await That(Act).Throws<MockVerificationException>()
					.WithMessage("*SendAsync*").AsWildcard();
			}

#if NET8_0_OR_GREATER
			[Fact]
			public async Task PatchAsync_StringUri_FailingVerification_ShouldMentionSendAsyncInMessage()
			{
				HttpClient httpClient = HttpClient.CreateMock();

				void Act()
				{
					httpClient.Mock.Verify.PatchAsync(It.Matches("*aweXpect*"), It.IsAny<HttpContent>()).AtLeastOnce();
				}

				await That(Act).Throws<MockVerificationException>()
					.WithMessage("*SendAsync*").AsWildcard();
			}

			[Fact]
			public async Task PatchAsync_Uri_FailingVerification_ShouldMentionSendAsyncInMessage()
			{
				HttpClient httpClient = HttpClient.CreateMock();

				void Act()
				{
					httpClient.Mock.Verify.PatchAsync(It.IsUri("*aweXpect*"), It.IsAny<HttpContent>()).AtLeastOnce();
				}

				await That(Act).Throws<MockVerificationException>()
					.WithMessage("*SendAsync*").AsWildcard();
			}
#endif
		}
	}
}
