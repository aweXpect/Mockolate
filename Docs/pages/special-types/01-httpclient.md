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
