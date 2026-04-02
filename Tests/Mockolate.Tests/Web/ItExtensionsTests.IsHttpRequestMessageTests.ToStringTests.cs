using System.Net.Http;
using Mockolate.Web;

namespace Mockolate.Tests.Web;

public sealed partial class ItExtensionsTests
{
	public sealed partial class IsHttpRequestMessageTests
	{
		public sealed class ToStringTests
		{
			[Fact]
			public async Task Default_ShouldReturnExpectedValue()
			{
				ItExtensions.IHttpRequestMessageParameter sut = It.IsHttpRequestMessage();

				string? result = sut.ToString();

				await That(result).IsEqualTo("a Http-Request");
			}

			[Fact]
			public async Task MultipleConditions_ShouldReturnExpectedValue()
			{
				ItExtensions.IHttpRequestMessageParameter sut = It.IsHttpRequestMessage(HttpMethod.Post)
					.WhoseUriIs("https://example.com")
					.WhoseContentIs("application/json");

				string? result = sut.ToString();

				await That(result).IsEqualTo(
					"POST-Request with Uri matching \"https://example.com\" and Http content with media type \"application/json\"");
			}

			[Fact]
			public async Task WhoseContentIsAction_ShouldReturnExpectedValue()
			{
				ItExtensions.IHttpRequestMessageParameter sut =
					It.IsHttpRequestMessage().WhoseContentIs(c => c.WithMediaType("text/plain"));

				string? result = sut.ToString();

				await That(result).IsEqualTo(
					"a Http-Request with Http content with media type \"text/plain\"");
			}

			[Fact]
			public async Task WhoseContentIsMediaType_ShouldReturnExpectedValue()
			{
				ItExtensions.IHttpRequestMessageParameter sut =
					It.IsHttpRequestMessage().WhoseContentIs("application/json");

				string? result = sut.ToString();

				await That(result).IsEqualTo(
					"a Http-Request with Http content with media type \"application/json\"");
			}

			[Fact]
			public async Task WhoseUriIsAction_ShouldReturnExpectedValue()
			{
				ItExtensions.IHttpRequestMessageParameter sut =
					It.IsHttpRequestMessage().WhoseUriIs(u => u.ForHttps().WithHost("example.com"));

				string? result = sut.ToString();

				await That(result).IsEqualTo(
					"a Http-Request with https Uri with host matching \"example.com\"");
			}

			[Fact]
			public async Task WhoseUriIsString_ShouldReturnExpectedValue()
			{
				ItExtensions.IHttpRequestMessageParameter sut =
					It.IsHttpRequestMessage().WhoseUriIs("https://example.com");

				string? result = sut.ToString();

				await That(result).IsEqualTo("a Http-Request with Uri matching \"https://example.com\"");
			}

			[Fact]
			public async Task WithHeaders_ShouldReturnExpectedValue()
			{
				ItExtensions.IHttpRequestMessageParameter sut =
					It.IsHttpRequestMessage().WithHeaders(("x-my-header", "my-value"));

				string? result = sut.ToString();

				await That(result).IsEqualTo("a Http-Request with header \"x-my-header: my-value\"");
			}

			[Fact]
			public async Task WithMethod_ShouldReturnExpectedValue()
			{
				ItExtensions.IHttpRequestMessageParameter sut =
					It.IsHttpRequestMessage(HttpMethod.Get);

				string? result = sut.ToString();

				await That(result).IsEqualTo("GET-Request");
			}
		}
	}
}
