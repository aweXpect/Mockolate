# Mockolate Documentation Gaps Analysis

This document identifies features that are implemented in Mockolate but not documented in README.md.

## Summary

After analyzing the codebase and comparing it with the README.md, I've identified **12 major feature groups** that are either completely missing from documentation or inadequately documented.

---

## 1. Mock.Wrap Feature ⭐ HIGH PRIORITY

**Status**: Completely undocumented

**Features**:
- `Mock.Wrap<T>(instance)` - Wrap existing instances with mock tracking
- Forward calls to real instance while tracking interactions
- Verify interactions on wrapped instances

**Suggested Documentation**:
```markdown
### Wrapping Existing Instances

You can wrap an existing instance with mock tracking using `Mock.Wrap<T>()`. This allows you to track interactions with a real object:

```csharp
var realDispenser = new ChocolateDispenser();
var wrappedDispenser = Mock.Wrap<IChocolateDispenser>(realDispenser);

// Calls are forwarded to the real instance
wrappedDispenser.Dispense("Dark", 5);

// But you can still verify interactions
wrappedDispenser.VerifyMock.Invoked.Dispense(It.Is("Dark"), It.Is(5)).Once();
```
```

---

## 2. Delegate Mocking

**Status**: Mentioned in example but not documented as a feature

**Features**:
- Mock `Action<T>`, `Func<T>`, and custom delegates
- `SetupMock.Delegate()` for setup
- `VerifyMock.Invoked()` for delegate verification
- Full parameter matching support

**Suggested Documentation**:
```markdown
### Mocking Delegates

Mockolate supports mocking delegates including `Action<T>`, `Func<T>`, and custom delegates:

```csharp
// Create a mock delegate
var myDelegate = Mock.Create<Action<string, int>>();

// Setup the delegate
myDelegate.SetupMock.Delegate(It.Is("Dark"), It.IsAny<int>())
    .Do((type, amount) => Console.WriteLine($"Dispensed {amount} {type}"));

// Invoke the delegate
myDelegate("Dark", 5);

// Verify invocation
myDelegate.VerifyMock.Invoked(It.Is("Dark"), It.Is(5)).Once();
```

Custom delegates are also supported:
```csharp
var customDelegate = Mock.Create<ChocolateDispensedDelegate>();
customDelegate.SetupMock.Delegate(It.IsAny<string>(), It.IsAny<int>())
    .Do((type, amount) => { /* handler */ });
```
```

---

## 3. Advanced Parameter Matching

**Status**: Partially documented, missing several important matchers

**Missing Features**:
- **Ref/Out parameters**: `It.IsRef<T>()`, `It.IsOut<T>()`, `It.IsAnyRef<T>()`, `It.IsAnyOut<T>()`
- **Span parameters**: `It.IsSpan<T>()`, `It.IsReadOnlySpan<T>()`, `It.IsAnySpan<T>()`, `It.IsAnyReadOnlySpan<T>()`
- **Regex matching**: `.AsRegex()` extension
- **Custom equality comparers**: `.Using(IEqualityComparer<T>)`
- **Custom parameter predicates**: `Match.Parameters(Func<NamedParameterValue[], bool>)`

**Suggested Documentation**:
```markdown
#### Ref and Out Parameters

```csharp
// Setup with ref parameter that modifies the value
int value = 10;
sut.SetupMock.Method.TryDispense(
    It.Is("Dark"), 
    It.IsRef<int>(v => v * 2)  // Doubles the ref parameter
).Returns(true);

sut.TryDispense("Dark", ref value);  // value is now 20

// Setup with out parameter
sut.SetupMock.Method.TryGetStock(
    It.Is("Dark"),
    It.IsOut<int>(() => 42)  // Sets out parameter to 42
).Returns(true);

// Verify ref/out parameters
sut.VerifyMock.Invoked.TryDispense(
    It.Is("Dark"),
    It.IsRef<int>()
).Once();
```

#### Span Parameters

```csharp
// Setup with Span<T>
sut.SetupMock.Method.ProcessBatch(
    It.IsSpan<string>(arr => arr.Length > 0)
).Returns(true);

// Verify Span invocation
sut.VerifyMock.Invoked.ProcessBatch(
    It.IsAnySpan<string>()
).Once();
```

#### Regular Expression Matching

```csharp
// Use regex for string matching
sut.SetupMock.Method.Dispense(
    It.Matches("^Dark.*").AsRegex(),  // Regex pattern
    It.IsAny<int>()
).Returns(true);
```

#### Custom Equality Comparers

```csharp
var comparer = StringComparer.OrdinalIgnoreCase;
sut.SetupMock.Method.Dispense(
    It.Is("dark").Using(comparer),  // Case-insensitive match
    It.IsAny<int>()
).Returns(true);
```
```

---

## 4. Protected Member Support

**Status**: Completely undocumented

**Features**:
- Setup protected methods and properties
- Verify protected member access
- `SetupMock.Protected.Method()`, `.Property()`
- `VerifyMock.Protected.Invoked()`, `.Got()`, `.Set()`

**Suggested Documentation**:
```markdown
### Working with Protected Members

Mockolate allows you to setup and verify protected methods and properties:

```csharp
var sut = Mock.Create<MyChocolateDispenser>();

// Setup protected method
sut.SetupMock.Protected.Method("DispenseInternal",
    It.Is("Dark"), It.IsAny<int>())
    .Returns(true);

// Setup protected property
sut.SetupMock.Protected.Property("InternalStock").InitializeWith(100);

// Verify protected method was called
sut.VerifyMock.Protected.Invoked("DispenseInternal",
    It.Is("Dark"), It.IsAny<int>()).Once();

// Verify protected property was accessed
sut.VerifyMock.Protected.Got("InternalStock").AtLeastOnce();
```
```

---

## 5. Advanced Callback Features

**Status**: Partially documented, missing several options

**Missing Features**:
- `.When(Func<int, bool>)` - Conditional callback execution
- `.For(int times)` - Execute callback N times
- `.Only(int times)` - Execute callback up to N times
- `.InParallel()` - Execute callback in parallel
- Invocation counter in callbacks (`Action<int>`)

**Suggested Documentation**:
```markdown
#### Conditional Callbacks

Execute callbacks conditionally based on invocation count:

```csharp
sut.SetupMock.Method.Dispense(It.Is("Dark"), It.IsAny<int>())
    .Do(() => Console.WriteLine("Called!"))
    .When(count => count <= 3);  // Only first 3 invocations
```

#### Frequency Control

Control how many times a callback executes:

```csharp
// Execute exactly 5 times
sut.SetupMock.Method.Dispense(It.IsAny<string>(), It.IsAny<int>())
    .Do(() => Console.WriteLine("First 5 calls"))
    .For(5);

// Execute up to 3 times
sut.SetupMock.Method.Dispense(It.IsAny<string>(), It.IsAny<int>())
    .Do(() => Console.WriteLine("Up to 3 calls"))
    .Only(3);
```

#### Parallel Callbacks

Execute callbacks in parallel:

```csharp
sut.SetupMock.Method.Dispense(It.IsAny<string>(), It.IsAny<int>())
    .Do(() => { /* parallel work */ })
    .InParallel();
```

#### Invocation Counter

Access the invocation counter in callbacks:

```csharp
sut.SetupMock.Method.Dispense(It.IsAny<string>(), It.IsAny<int>())
    .Do((int count) => Console.WriteLine($"Call #{count}"));

sut.SetupMock.Property.TotalDispensed
    .OnGet.Do((int count, int value) => 
        Console.WriteLine($"Read #{count}, value: {value}"));
```
```

---

## 6. Property and Indexer Advanced Features

**Status**: Partially documented, missing several options

**Missing Features**:
- `.Returns(Func<T, T>)` - Return value based on previous value
- `.OnGet.Do(Action<int, T>)` - Getter callback with counter and value
- `.OnSet.Do(Action<int, T>)` - Setter callback with counter and value
- `.Register()` - Register setup without value

**Suggested Documentation**:
```markdown
#### Advanced Property Returns

Return a value based on the previous value:

```csharp
sut.SetupMock.Property.TotalDispensed
    .Returns((current) => current + 10);  // Increment by 10 each read
```

#### Advanced Callbacks

Access invocation counter and values in callbacks:

```csharp
// Getter with counter and current value
sut.SetupMock.Property.TotalDispensed
    .OnGet.Do((int count, int value) => 
        Console.WriteLine($"Read #{count}, current value: {value}"));

// Setter with counter and new value
sut.SetupMock.Property.TotalDispensed
    .OnSet.Do((int count, int newValue) => 
        Console.WriteLine($"Set #{count} to {newValue}"));
```

#### Register Without Value

Register a setup without providing a value (useful with `ThrowWhenNotSetup`):

```csharp
var strictMock = Mock.Create<IChocolateDispenser>(
    new MockBehavior { ThrowWhenNotSetup = true });

// Register property without value - won't throw
strictMock.SetupMock.Property.TotalDispensed.Register();
```
```

---

## 7. Additional Verification Features

**Status**: Partially documented, missing some count verifications

**Missing Features**:
- `.Between(min, max)` - Between min and max times
- `.Times(Func<int, bool>)` - Custom predicate verification
- Protected member verification (see section 4)

**Suggested Documentation**:
```markdown
#### Additional Count Verifications

```csharp
// Between min and max (inclusive)
sut.VerifyMock.Invoked.Dispense(It.IsAny<string>(), It.IsAny<int>())
    .Between(3, 5);

// Custom predicate
sut.VerifyMock.Invoked.Dispense(It.IsAny<string>(), It.IsAny<int>())
    .Times(count => count % 2 == 0);  // Even number of calls
```
```

---

## 8. Interaction Tracking

**Status**: Completely undocumented

**Features**:
- `.Interactions` property - Access all recorded interactions
- `.ClearAllInteractions()` - Clear interaction history
- `MockMonitor.Run()` - Session-based monitoring

**Suggested Documentation**:
```markdown
### Interaction Tracking

Mockolate tracks all interactions with mocks. You can access and manage these interactions:

```csharp
var sut = Mock.Create<IChocolateDispenser>();

sut.Dispense("Dark", 5);
sut.Dispense("Milk", 3);

// Access all interactions
foreach (var interaction in sut.Interactions)
{
    Console.WriteLine($"Interaction: {interaction}");
}

// Clear interaction history
sut.ClearAllInteractions();
```

#### Mock Monitoring Sessions

Use `MockMonitor.Run()` to track interactions within a specific scope:

```csharp
using (var monitor = MockMonitor.Run())
{
    sut.Dispense("Dark", 5);
    
    // Access interactions within this session
    var interactions = monitor.GetInteractions();
}
```
```

---

## 9. HttpClient Extended Features

**Status**: Partially documented, missing several matchers and methods

**Missing Features**:
- `It.IsBinaryContent()` matcher
- `.ForHttps()`, `.ForHttp()` URI scheme filters
- `.WithBodyMatching()` content body matching
- PUT, DELETE, PATCH methods (only GET and POST shown)
- All CancellationToken variants

**Suggested Documentation**:
```markdown
### Extended HttpClient Features

#### All HTTP Methods

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

#### Binary Content Matching

```csharp
httpClient.SetupMock.Method.PostAsync(
    It.IsAny<string>(),
    It.IsBinaryContent("application/octet-stream"))
    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
```

#### URI Scheme Filtering

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

#### Content Body Matching

```csharp
httpClient.VerifyMock.Invoked.PostAsync(
    It.IsAny<string>(),
    It.IsStringContent("application/json")
        .WithBodyMatching("*\"type\": \"Dark\"*"))
    .Once();
```

#### CancellationToken Support

All HTTP methods support CancellationToken parameters:

```csharp
var cts = new CancellationTokenSource();
httpClient.SetupMock.Method.GetAsync(
    It.IsAny<string>(), It.IsAny<CancellationToken>())
    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

await httpClient.GetAsync("https://example.com", cts.Token);
```
```

---

## 10. Mock Behavior Advanced Options

**Status**: Partially documented, missing several options

**Missing Features**:
- `.WithDefaultValueFor<T>()` - Custom default value factories
- `.Initialize<T>(Action<int, IMockSetup<T>>)` - Initialize with counter
- `.UseConstructorParametersFor<T>()` - Constructor parameter configuration

**Suggested Documentation**:
```markdown
### Advanced Mock Behavior

#### Custom Default Value Factories

Provide custom default values for specific types:

```csharp
var behavior = new MockBehavior()
    .WithDefaultValueFor<string>(() => "default")
    .WithDefaultValueFor<int>(() => 42);

var sut = Mock.Create<IChocolateDispenser>(behavior);
```

#### Initialize with Counter

Initialize mocks with a counter to track creation:

```csharp
var behavior = new MockBehavior()
    .Initialize<IChocolateDispenser>((count, mock) => 
    {
        mock.SetupMock.Property.TotalDispensed.InitializeWith(count * 10);
    });
```

#### Constructor Parameters Configuration

Configure constructor parameters for specific types:

```csharp
var behavior = new MockBehavior()
    .UseConstructorParametersFor<MyChocolateDispenser>("Dark", 100);

var sut = Mock.Create<MyChocolateDispenser>(behavior);
```
```

---

## 11. Special Method Overrides

**Status**: Completely undocumented

**Features**:
- Override `ToString()`, `Equals()`, `GetHashCode()` on mocks
- Customize mock identity and string representation

**Suggested Documentation**:
```markdown
### Overriding Special Methods

You can override `ToString()`, `Equals()`, and `GetHashCode()` on your mocks:

```csharp
var sut = Mock.Create<IChocolateDispenser>();

// Override ToString
sut.SetupMock.Method.ToString()
    .Returns("Custom Mock Dispenser");

// Override Equals
sut.SetupMock.Method.Equals(It.IsAny<object>())
    .Returns((obj) => ReferenceEquals(sut, obj));

// Override GetHashCode
sut.SetupMock.Method.GetHashCode()
    .Returns(12345);

Console.WriteLine(sut.ToString());  // "Custom Mock Dispenser"
```
```

---

## 12. Async Exception Handling

**Status**: Partially documented, missing ThrowsAsync

**Missing Features**:
- `.ThrowsAsync<TException>()` - Throw exceptions from async methods

**Suggested Documentation**:
```markdown
### Async Exception Handling

Throw exceptions from async methods:

```csharp
sut.SetupMock.Method.DispenseAsync(It.Is("Invalid"), It.IsAny<int>())
    .ThrowsAsync<InvalidOperationException>();

// Or with a specific exception
sut.SetupMock.Method.DispenseAsync(It.Is("Error"), It.IsAny<int>())
    .ThrowsAsync(new InvalidChocolateException("Bad chocolate!"));

// Or with a factory
sut.SetupMock.Method.DispenseAsync(It.IsAny<string>(), It.IsAny<int>())
    .ThrowsAsync((type, amount) => new Exception($"Cannot dispense {amount} {type}"));
```
```

---

## Recommendations

### Priority for Documentation

**High Priority** (Core features that users need):
1. Mock.Wrap - Completely missing, very useful feature
2. Delegate Mocking - Mentioned but not properly documented
3. Protected Member Support - Advanced but important feature
4. Advanced Parameter Matching (Ref/Out/Span) - Critical for certain scenarios

**Medium Priority** (Enhances existing documented features):
5. Advanced Callback Features - Extends existing callback documentation
6. Property/Indexer Advanced Features - Completes existing sections
7. Additional Verification Features - Rounds out verification options
8. HttpClient Extended Features - Completes HttpClient section

**Low Priority** (Nice to have, advanced use cases):
9. Interaction Tracking - Advanced debugging feature
10. Mock Behavior Advanced Options - Edge cases
11. Special Method Overrides - Rare use case
12. Async Exception Handling - Minor completion of async docs

### Suggested Action Items

For each priority group, create a GitHub issue with:
- Title: "Document [Feature Group Name]"
- Labels: `documentation`, `enhancement`
- Description: Include relevant sections from this document
- Acceptance Criteria: Clear checklist of what needs to be documented

### Documentation Structure Recommendations

Consider organizing the README into these main sections:
1. **Getting Started** (current)
2. **Creating Mocks** (current, add Mock.Wrap)
3. **Setup** (current, expand with missing features)
4. **Verification** (current, add missing verifiers)
5. **Advanced Features** (new section for Protected, Delegates, etc.)
6. **Special Type Support** (HttpClient, Delegates, Spans)
7. **Interaction Tracking** (new section)
8. **Analyzers** (current)

This would make the documentation more discoverable and easier to navigate.
