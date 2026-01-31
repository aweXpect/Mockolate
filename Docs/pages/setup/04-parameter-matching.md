# Parameter Matching

Mockolate provides flexible parameter matching for method setups and verifications:

## Parameter Matchers

### Basic Matchers

- `It.IsAny<T>()`: Matches any value of type `T`.
- `It.Is<T>(value)`: Matches a specific value.
- `It.IsOneOf<T>(params T[] values)`: Matches any of the given values.
- `It.IsNull<T>()`: Matches null.
- `It.IsTrue()`/`It.IsFalse()`: Matches boolean true/false.
- `It.IsInRange(min, max)`: Matches a number within the given range. You can append `.Exclusive()` to exclude the
  minimum and maximum value.
- `It.Satisfies<T>(predicate)`: Matches values based on a predicate.

### String Matching

- `It.Matches(pattern)`: Matches strings using wildcard patterns (`*` and `?`).

#### Regular Expressions  
Use `.AsRegex()` to enable regular expression matching for `It.Matches()`:

```csharp
// Example: Match email addresses
sut.SetupMock.Method.ValidateEmail(It.Matches(@"^\w+@\w+\.\w+$").AsRegex())
  .Returns(true);

bool result = sut.ValidateEmail("user@example.com");

// Case-sensitive regex
sut.SetupMock.Method.Process(It.Matches("^[A-Z]+$").AsRegex().CaseSensitive())
  .Returns(1);
```

### Ref and Out Parameters

- `It.IsRef<T>(setter)`: Matches any `ref` parameter and sets a new value using the setter function.
- `It.IsRef<T>(predicate, setter)`: Matches `ref` parameters that satisfy the predicate and sets a new value.
- `It.IsRef<T>(predicate)`: Matches `ref` parameters that satisfy the predicate without changing the value.
- `It.IsAnyRef<T>()`: Matches any `ref` parameter without restrictions.
- `It.IsOut<T>(setter)`: Matches any `out` parameter and sets a value using the setter function.
- `It.IsAnyOut<T>()`: Matches any `out` parameter without restrictions and sets the parameter to the default value of
  `T`.

```csharp
// Example: Setup with out parameter
sut.SetupMock.Method.TryParse(It.IsAny<string>(), It.IsOut(() => 42))
    .Returns(true);

int result;
bool success = sut.TryParse("abc", out result);
// result == 42, success == true

// Example: Setup with ref parameter
sut.SetupMock.Method.Increment(It.IsRef<int>(v => v + 1))
    .Returns(true);

int value = 5;
sut.Increment(ref value);
// value == 6
```

### Span Parameters (.NET 8+)

- `It.IsSpan<T>(predicate)`: Matches `Span<T>` parameters that satisfy the predicate.
- `It.IsAnySpan<T>()`: Matches any `Span<T>` parameter.
- `It.IsReadOnlySpan<T>(predicate)`: Matches `ReadOnlySpan<T>` parameters that satisfy the predicate.
- `It.IsAnyReadOnlySpan<T>()`: Matches any `ReadOnlySpan<T>` parameter.

**Note:**  
As `ref struct` types cannot be stored directly, it is converted to an array internally and the `predicate` receives
this array for evaluation.

```csharp
// Example: Setup with Span parameter
sut.SetupMock.Method.Process(It.IsSpan<byte>(data => data.Length > 0))
    .Returns(true);

Span<byte> buffer = new byte[] { 1, 2, 3 };
bool result = sut.Process(buffer);
// result == true
```

### Custom Equality Comparers

Use `.Using(IEqualityComparer<T>)` to provide custom equality comparison for `It.Is()` and `It.IsOneOf()`:

```csharp
// Example: Case-insensitive string comparison
var comparer = StringComparer.OrdinalIgnoreCase;
sut.SetupMock.Method.Process(It.Is("hello").Using(comparer))
    .Returns(42);

int result = sut.Process("HELLO");
// result == 42
```

## Parameter Predicates

When the method name is unique (no overloads), you can use flexible parameter matching:

- `Match.AnyParameters()`: Matches any parameter combination.
- `Match.Parameters(Func<NamedParameterValue[], bool> predicate)`: Matches parameters based on a custom predicate.

```csharp
// Example: Custom parameter predicate
sut.SetupMock.Method.Process(Match.Parameters(args => 
    args.Length == 2 && 
    args[0].Value is string s && s.StartsWith("test") &&
    args[1].Value is int i && i > 0))
    .Returns(true);

bool result = sut.Process("test123", 5);
// result == true
```

## Parameter Interaction

### Callbacks

With `.Do`, you can register a callback for individual parameters of a method setup. This allows you to implement side
effects or checks directly when the method or indexer is called.

**Example: Do for method parameter**

```csharp
int lastAmount = 0;
sut.SetupMock.Method.Dispense(It.Is("Dark"), It.IsAny<int>().Do(amount => lastAmount = amount));
sut.Dispense("Dark", 42);
// lastAmount == 42
```

### Monitor

With `.Monitor(out monitor)`, you can track the actual
values passed during test execution and analyze them afterward.

**Example: Monitor for method parameter**

```csharp
Mockolate.ParameterMonitor<int> monitor;
sut.SetupMock.Method.Dispense(It.Is("Dark"), It.IsAny<int>().Monitor(out monitor));
sut.Dispense("Dark", 5);
sut.Dispense("Dark", 7);
// monitor.Values == [5, 7]
```
