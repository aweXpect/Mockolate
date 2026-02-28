# HttpClient

Mockolate supports mocking `HttpClient` out of the box, with no special configuration required. You can set up, use, and
verify HTTP interactions just like with any other interface or class.

**Example: Mocking HttpClient for a Chocolate Dispenser Service**

```csharp
HttpClient httpClient = Mock.Create<HttpClient>();
httpClient.SetupMock.Method
    .PostAsync(
        It.IsAny<string>(),
        It.IsHttpContent())
    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

HttpResponseMessage result = await httpClient.PostAsync("https://aweXpect.com/api/chocolate/dispense",
    new StringContent("""
                      { "type": "Dark", "amount": 3 }
                      """, Encoding.UTF8, "application/json"));

await That(result.IsSuccessStatusCode).IsTrue();
httpClient.VerifyMock.Invoked.PostAsync(
    It.IsUri("*aweXpect.com/api/chocolate/dispense*").ForHttps(),
    It.IsHttpContent("application/json").WithStringMatching("*\"type\": \"Dark\"*\"amount\": 3*")).Once();
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

Filter requests by host using `.WithHost(string)`. You can provide a wildcard pattern to match against the host name:

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

Filter requests by path using `.WithPath(string)`. You can provide a wildcard pattern to match against the path:

```csharp
httpClient.VerifyMock.Invoked
    .GetAsync(It.IsUri().WithPath("/foo/*"))
    .Once();
```

### Query

Filter requests by query parameters using `.WithQuery(...)`. You can provide one or many key-value pairs or a raw query
string to match against the query parameters. The order of the key-value pairs does not matter:

```csharp
// Match query string containing "x=42"
httpClient.VerifyMock.Invoked
    .GetAsync(It.IsUri().WithQuery("x", "42"))
    .Once();
// Match query string containing "x=42" and "y=foo" (in any order)
httpClient.VerifyMock.Invoked
    .GetAsync(It.IsUri().WithQuery(("x", "42"), ("y", "foo")))
    .Once();
// Match query string containing "x=42" and "y=foo" (in any order)
httpClient.VerifyMock.Invoked
    .GetAsync(It.IsUri().WithQuery("x=42&y=foo"))
    .Once();
```

## Content Matching

Use `It.IsHttpContent(string?)` to match the HTTP content, optionally providing an expected media type header value:

```csharp
httpClient.SetupMock.Method
    .PostAsync(
        It.IsAny<string>(),
        It.IsHttpContent("application/json"))
    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
```

### String content

To verify against the string content, use the following methods:

- `.WithString(Func<string, bool>)`: to match the string content against the given predicate
- `.WithString(string)`: to match the content exactly as provided
- `.WithStringMatching(string)`: to match the content using wildcard patterns
- `.WithStringMatching(string).AsRegex()`: to match the content using regular expressions

```csharp
httpClient.SetupMock.Method
    .PostAsync(
        It.IsAny<string>(),
        It.IsHttpContent("application/json").WithStringMatching("*\"type\": \"Dark\"*"))
    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
```

**Notes:**

- Add the `.IgnoringCase()` modifier to make the string matching case-insensitive.

### Binary content

To verify against the binary content, use the following methods:

- `.WithBytes(Func<byte[], bool>)`: to match the binary content against the given predicate
- `.WithBytes(byte[])`: to match the content exactly as provided

```csharp
httpClient.SetupMock.Method
    .PostAsync(
        It.IsAny<string>(),
        It.IsHttpContent("application/octet-stream").WithBytes([0x01, 0x02, 0x03, ]))
    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
```

### Form data content

To verify against the URL-encoded form data content, use the following methods:

- `.WithFormData(string, string)`: checks that the form-data content contains the provided key-value pair
- `.WithFormData(IEnumerable<(string, string)>)`: checks that the form-data content contains the provided key-value
  pairs
- `.WithFormData(string)`: checks that the form-data content contains the provided raw form data string

```csharp
httpClient.SetupMock.Method
    .PostAsync(
        It.IsAny<string>(),
        It.IsHttpContent("application/x-www-form-urlencoded").WithFormData("my-key", "my-value"))
    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
```

**Notes:**

- Similar to the query parameter matching, the order of the form-data key-value pairs does not matter.
- Add the `.Exactly()` modifier to also check that no other form-data is present.

### Header matching

To verify against the HTTP content headers, use the following methods:

- `.WithHeaders(string, HttpHeaderValue)`: checks that the content headers contain the provided key-value pair
- `.WithHeaders(IEnumerable<(string, HttpHeaderValue)>)`: checks that the content headers contain all key-value pairs
- `.WithHeaders(string)`: checks that the content headers contain the provided raw headers

```csharp
httpClient.SetupMock.Method
    .PostAsync(
        It.IsAny<string>(),
        It.IsHttpContent().WithHeaders(("Content-Type", "application/json"), ("X-My-Header", "my-value")))
    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
httpClient.SetupMock.Method
    .PostAsync(
        It.IsAny<string>(),
        It.IsHttpContent().WithHeaders("""
        		                           Content-Type: application/json
        		                           X-My-Header: my-value
        		                           """))
    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
```

**Notes:**

- By default, only the content headers are checked, not the headers in the corresponding `HttpRequestMessage`.
  If you want to check both, add the `.IncludingRequestHeaders()` modifier.

## Return Message

Overloads of `.ReturnsAsync` simplify specifying the return value for HTTP method setups.

```csharp
httpClient.SetupMock.Method.GetAsync(It.IsAny<Uri>())
    // Returns a response with status code 200 OK and no content
    .ReturnsAsync(HttpStatusCode.OK);

httpClient.SetupMock.Method.GetAsync(It.IsAny<Uri>())
    // Returns a response with status code 200 OK and a string content "some string content"
    .ReturnsAsync(HttpStatusCode.OK, "some string content");

httpClient.SetupMock.Method.GetAsync(It.IsAny<Uri>())
    // Returns a response with status code 200 OK and a JSON content {"foo":"bar"}
    .ReturnsAsync(HttpStatusCode.OK, "{\"foo\":\"bar\"}", "application/json");

byte[] bytes = new byte[] { /* ... */ };

httpClient.SetupMock.Method.GetAsync(It.IsAny<Uri>())
    // Returns a response with status code 200 OK and a binary content with the provided bytes
    .ReturnsAsync(HttpStatusCode.OK, bytes);

httpClient.SetupMock.Method.GetAsync(It.IsAny<Uri>())
    // Returns a response with status code 200 OK and a PNG image content with the provided bytes
    .ReturnsAsync(HttpStatusCode.OK, bytes, "image/png");
```
