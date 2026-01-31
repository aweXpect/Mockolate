# HttpClient

Mockolate supports mocking `HttpClient` out of the box, with no special configuration required. You can set up, use, and
verify HTTP interactions just like with any other interface or class.

**Example: Mocking HttpClient for a Chocolate Dispenser Service**

```csharp
HttpClient httpClient = Mock.Create<HttpClient>();
httpClient.SetupMock.Method.PostAsync(
		It.IsAny<string>(),
		It.IsStringContent())
	.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

HttpResponseMessage result = await httpClient.PostAsync("https://aweXpect.com/api/chocolate/dispense",
	new StringContent("""
	                  { "type": "Dark", "amount": 3 }
	                  """, Encoding.UTF8, "application/json"));

await That(result.IsSuccessStatusCode).IsTrue();
httpClient.VerifyMock.Invoked.PostAsync(
	It.IsUri("*aweXpect.com/api/chocolate/dispense*").ForHttps(),
	It.IsStringContent("application/json").WithBodyMatching("*\"type\": \"Dark\"*\"amount\": 3*")).Once();
```

**Notes:**

- The custom extensions for the `HttpClient` are in the `Mockolate.Web` namespace.
- Under the hood, the setups, requests and verifications are forwarded to a mocked `HttpMessageHandler`.
  As they therefore all forward to the `SendAsync` method, you can mix using a string or an `Uri` parameter in setup or
  verification.

## All HTTP Methods

Mockolate supports all standard HTTP methods:

```csharp
// PUT
httpClient.SetupMock.Method.PutAsync(
    It.IsAny<string>(), It.IsAny<HttpContent>())
    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

// DELETE
httpClient.SetupMock.Method.DeleteAsync(It.IsAny<string>())
    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NoContent));

// PATCH
httpClient.SetupMock.Method.PatchAsync(
    It.IsAny<string>(), It.IsAny<HttpContent>())
    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
```

## Binary Content Matching

Use `It.IsBinaryContent()` to match binary content types:

```csharp
httpClient.SetupMock.Method.PostAsync(
    It.IsAny<string>(),
    It.IsBinaryContent("application/octet-stream"))
    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
```

## URI Scheme Filtering

Filter requests by URI scheme using `.ForHttps()` or `.ForHttp()`:

```csharp
// Match only HTTPS requests
httpClient.VerifyMock.Invoked.GetAsync(
    It.IsUri("*api.example.com*").ForHttps())
    .Once();

// Match only HTTP requests
httpClient.VerifyMock.Invoked.GetAsync(
    It.IsUri("*localhost*").ForHttp())
    .Never();
```

## Content Body Matching

Use `.WithBodyMatching()` to match against the content body:

```csharp
httpClient.VerifyMock.Invoked.PostAsync(
    It.IsAny<string>(),
    It.IsStringContent("application/json")
        .WithBodyMatching("*\"type\": \"Dark\"*"))
    .Once();
```

## CancellationToken Support

All HTTP methods support CancellationToken parameters:

```csharp
var cts = new CancellationTokenSource();
httpClient.SetupMock.Method.GetAsync(
    It.IsAny<string>(), It.IsAny<CancellationToken>())
    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

await httpClient.GetAsync("https://example.com", cts.Token);
```
