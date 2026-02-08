using System.Net.Http;
using Mockolate.Parameters;

namespace Mockolate.Web;

/// <summary>
///     A parameter of type <typeparamref name="T" /> that also gets a <see cref="HttpRequestMessage" />.
/// </summary>
internal interface IHttpRequestMessagePropertyParameter<T> : IParameter<T>
{
	/// <summary>
	///     Matches the property of type <typeparamref name="T" /> while also considering the
	///     <paramref name="requestMessage" />.
	/// </summary>
	bool Matches(T value, HttpRequestMessage? requestMessage);
}
