#if NET8_0_OR_GREATER
using System.Net.Http;

namespace Mockolate.ExampleTests.TestData;

/// <summary>
///     Local stub mirroring <c>Microsoft.Extensions.Http.IHttpClientFactory</c> so the example
///     test can demonstrate mocking the factory without taking a runtime dependency on
///     <c>Microsoft.Extensions.Http</c>.
/// </summary>
public interface IHttpClientFactory
{
	HttpClient CreateClient(string name);
}
#endif
