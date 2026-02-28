using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mockolate.Setup;

namespace Mockolate.Web;

public static partial class HttpClientExtensions
{
	/// <inheritdoc cref="HttpClientExtensions" />
	extension(IReturnMethodSetup<Task<HttpResponseMessage>, HttpRequestMessage, CancellationToken> setup)
	{
		/// <summary>
		///     Asynchronously returns a <see cref="HttpResponseMessage" /> with the given <paramref name="statusCode" />.
		/// </summary>
		public IReturnMethodSetupReturnBuilder<Task<HttpResponseMessage>, HttpRequestMessage, CancellationToken>
			ReturnsAsync(HttpStatusCode statusCode)
			=> setup.ReturnsAsync(new HttpResponseMessage(statusCode));

		/// <summary>
		///     Asynchronously returns a <see cref="HttpResponseMessage" /> with the given <paramref name="statusCode" />
		///     and string <paramref name="content" />.
		/// </summary>
		/// <remarks>
		///     Uses default encoding (UTF-8) and default media type ("text/plain") for the content.<br />
		///     If you need a different encoding, use the overload that takes explicit media type or an <see cref="HttpContent" />.
		/// </remarks>
		public IReturnMethodSetupReturnBuilder<Task<HttpResponseMessage>, HttpRequestMessage, CancellationToken>
			ReturnsAsync(HttpStatusCode statusCode, string content)
			=> setup.ReturnsAsync(new HttpResponseMessage(statusCode)
			{
				Content = new StringContent(content),
			});

		/// <summary>
		///     Asynchronously returns a <see cref="HttpResponseMessage" /> with the given <paramref name="statusCode" />,
		///     string <paramref name="content" /> and <paramref name="mediaType" />.
		/// </summary>
		/// <remarks>
		///     Uses default encoding (UTF-8) for the content.<br />
		///     If you need a different encoding, use the overload that takes an <see cref="HttpContent" />.
		/// </remarks>
		public IReturnMethodSetupReturnBuilder<Task<HttpResponseMessage>, HttpRequestMessage, CancellationToken>
			ReturnsAsync(HttpStatusCode statusCode, string content, string mediaType)
			=> setup.ReturnsAsync(new HttpResponseMessage(statusCode)
			{
				Content = new StringContent(content, Encoding.UTF8, mediaType),
			});

		/// <summary>
		///     Asynchronously returns a <see cref="HttpResponseMessage" /> with the given <paramref name="statusCode" />
		///     and <paramref name="bytes" />.
		/// </summary>
		/// <remarks>
		///     Uses default encoding (UTF-8) and default media type ("text/plain") for the content.<br />
		///     If you need a different encoding, use the overload that takes explicit media type or an <see cref="HttpContent" />.
		/// </remarks>
		public IReturnMethodSetupReturnBuilder<Task<HttpResponseMessage>, HttpRequestMessage, CancellationToken>
			ReturnsAsync(HttpStatusCode statusCode, byte[] bytes)
			=> setup.ReturnsAsync(new HttpResponseMessage(statusCode)
			{
				Content = new ByteArrayContent(bytes),
			});

		/// <summary>
		///     Asynchronously returns a <see cref="HttpResponseMessage" /> with the given <paramref name="statusCode" />,
		///     <paramref name="bytes" /> and <paramref name="mediaType" />.
		/// </summary>
		/// <remarks>
		///     Uses default encoding (UTF-8) for the content.<br />
		///     If you need a different encoding, use the overload that takes an <see cref="HttpContent" />.
		/// </remarks>
		public IReturnMethodSetupReturnBuilder<Task<HttpResponseMessage>, HttpRequestMessage, CancellationToken>
			ReturnsAsync(HttpStatusCode statusCode, byte[] bytes, string mediaType)
		{
			ByteArrayContent? content = new(bytes);
			content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
			return setup.ReturnsAsync(new HttpResponseMessage(statusCode)
			{
				Content = content,
			});
		}

		/// <summary>
		///     Asynchronously returns a <see cref="HttpResponseMessage" /> with the given <paramref name="statusCode" />
		///     and <paramref name="content" />.
		/// </summary>
		public IReturnMethodSetupReturnBuilder<Task<HttpResponseMessage>, HttpRequestMessage, CancellationToken>
			ReturnsAsync(HttpStatusCode statusCode, HttpContent content)
			=> setup.ReturnsAsync(new HttpResponseMessage(statusCode)
			{
				Content = content,
			});
	}
}
