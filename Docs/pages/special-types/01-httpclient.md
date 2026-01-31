# HttpClient

Mockolate supports mocking `HttpClient` out of the box, with no special configuration required. You can set up, use, and
verify HTTP interactions just like with any other interface or class.

**Example: Mocking HttpClient for a Chocolate Dispenser Service**

```csharp
HttpClient httpClient = Mock.Create<HttpClient>();
httpClient.SetupMock.Method
    .PostAsync(
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
// GET
httpClient.SetupMock.Method
    .GetAsync(It.IsAny<string>())
    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NoContent));

// POST
httpClient.SetupMock.Method
    .PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>())
    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

// PUT
httpClient.SetupMock.Method
    .PutAsync(It.IsAny<string>(), It.IsAny<HttpContent>())
    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

// DELETE
httpClient.SetupMock.Method
    .DeleteAsync(It.IsAny<string>())
    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NoContent));

// PATCH
httpClient.SetupMock.Method
    .PatchAsync(It.IsAny<string>(), It.IsAny<HttpContent>())
    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
```

For all HTTP methods you can add an optional cancellation token parameter.
If no parameter is provided, it matches any `CancellationToken`:

```csharp
var cts = new CancellationTokenSource();
httpClient.SetupMock.Method
    .GetAsync(It.IsAny<string>(), It.Is(cts.Token))
    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

await httpClient.GetAsync("https://example.com", cts.Token);
```

## URI matching

Use `It.IsUri(string?)` to match URIs using a wildcard pattern against the string representation of the URI.
The pattern supports `*` to match zero or more characters and `?` to match a single character.

### Scheme

Filter requests by URI scheme using `.ForHttps()` or `.ForHttp()`:

```csharp
// Match only HTTPS requests
httpClient.VerifyMock.Invoked
    .GetAsync(It.IsUri("*aweXpect.com*").ForHttps())
    .Once();

// Match only HTTP requests
httpClient.VerifyMock.Invoked
    .GetAsync(It.IsUri("*aweXpect.com*").ForHttp())
    .Never();
```

### Host

Filter requests by host using `.WithHost(string?)`. You can provide a wildcard pattern to match against the host name:

```csharp
httpClient.VerifyMock.Invoked
    .GetAsync(It.IsUri().WithHost("*aweXpect.com*"))
    .Once();
```

### Port

Filter requests on a specific port using `.WithPort(int)`:

```csharp
httpClient.VerifyMock.Invoked
    .GetAsync(It.IsUri().WithPort(443))
    .Once();
```

### Path

Filter requests by path using `.WithPath(string?)`. You can provide a wildcard pattern to match against the path:

```csharp
httpClient.VerifyMock.Invoked
    .GetAsync(It.IsUri().WithPath("/foo/*"))
    .Once();
```

### Query

Filter requests by query using `.WithQuery(string?)`. You can provide a wildcard pattern to match against the query:

```csharp
httpClient.VerifyMock.Invoked
    .GetAsync(It.IsUri().WithQuery("*x=42*"))
    .Once();
```

## Content Matching

### String content

Use `It.IsStringContent(string?)` to match string content types, optionally providing an expected media type header
value:

```csharp
httpClient.SetupMock.Method
    .PostAsync(
        It.IsAny<string>(),
        It.IsStringContent("application/json").WithBodyMatching("*\"type\": \"Dark\"*"))
    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
```

To verify against the string content, use the following methods:

- `.WithBody(string)`: to match content exactly as provided
- `.WithBodyMatching(string)`: to match content using wildcard patterns
- `.WithBodyMatching(string).AsRegex()`: to match content using regular expressions

### Binary content

Use `It.IsBinaryContent(string?)` to match binary content types, optionally providing an expected media type header
value:

```csharp
httpClient.SetupMock.Method
    .PostAsync(
        It.IsAny<string>(),
        It.IsBinaryContent("application/octet-stream"))
    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
```

To verify against the binary content, use the following methods:

- `.EqualTo(byte[])`: to match content exactly as provided
- `.Containing(byte[])`: to match content to contain the provided byte sequence in order
