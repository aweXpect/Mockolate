# Setup

Set up return values or behaviors for methods, properties, and indexers on your mock. Control how the mock responds to
calls in your tests.

## Method Setup

Use `mock.SetupMock.Method.MethodName(…)` to set up methods. You can specify argument matchers for each parameter.

```csharp
// Setup Dispense to decrease stock and raise event
sut.SetupMock.Method.Dispense(It.Is("Dark"), It.IsAny<int>())
    .Returns((type, amount) =>
    {
        var current = sut[type];
        if (current >= amount)
        {
            sut[type] = current - amount;
            sut.RaiseOnMock.ChocolateDispensed(type, amount);
            return true;
        }
        return false;
    });

// Setup method with callback
sut.SetupMock.Method.Dispense(It.Is("White"), It.IsAny<int>())
    .Do((type, amount) => Console.WriteLine($"Dispensed {amount} {type} chocolate."));

// Setup method to throw
sut.SetupMock.Method.Dispense(It.Is("Green"), It.IsAny<int>())
    .Throws<InvalidChocolateException>();
```

- Use `.Do(…)` to run code when the method is called. Supports parameterless or parameter callbacks.
- Use `.Returns(…)` to specify the value to return. You can provide a direct value, a callback, or a callback with
  parameters.
- Use `.Throws(…)` to specify an exception to throw. Supports direct exceptions, exception factories, or factories with
  parameters.
- Use `.Returns(…)` and `.Throws(…)` repeatedly to define a sequence of return values or exceptions (cycled on each
  call).
- Use `.SkippingBaseClass(…)` to override the base class behavior for a specific method (only for class mocks).
- When you specify overlapping setups, the most recently defined setup takes precedence.

**Async Methods**

For `Task<T>` or `ValueTask<T>` methods, use `.ReturnsAsync(…)`:

```csharp
sut.SetupMock.Method.DispenseAsync(It.IsAny<string>(), It.IsAny<int>())
    .ReturnsAsync(true);
```

#### Parameter Matching

Mockolate provides flexible parameter matching for method setups and verifications:

**Basic Matchers:**

- `It.IsAny<T>()`: Matches any value of type `T`.
- `It.Is<T>(value)`: Matches a specific value.
- `It.IsOneOf<T>(params T[] values)`: Matches any of the given values.
- `It.IsNull<T>()`: Matches null.
- `It.IsTrue()`/`It.IsFalse()`: Matches boolean true/false.
- `It.IsInRange(min, max)`: Matches a number within the given range. You can append `.Exclusive()` to exclude the
  minimum and maximum value.
- `It.Satisfies<T>(predicate)`: Matches values based on a predicate.

**String Matching:**

- `It.Matches<string>(pattern)`: Matches strings using wildcard patterns (`*` and `?`).

**Ref and Out Parameters:**

- `It.IsRef<T>(setter)`: Matches any `ref` parameter and sets a new value using the setter function.
- `It.IsRef<T>(predicate, setter)`: Matches `ref` parameters that satisfy the predicate and sets a new value.
- `It.IsRef<T>(predicate)`: Matches `ref` parameters that satisfy the predicate without changing the value.
- `It.IsRef<T>()`: Matches any `ref` parameter (verification only).
- `It.IsAnyRef<T>()`: Matches any `ref` parameter without restrictions.
- `It.IsOut<T>(setter)`: Matches any `out` parameter and sets a value using the setter function.
- `It.IsOut<T>()`: Matches any `out` parameter (verification only).
- `It.IsAnyOut<T>()`: Matches any `out` parameter without restrictions.

```csharp
// Example: Setup with out parameter
sut.SetupMock.Method.TryParse(It.IsAny<string>(), It.IsOut(() => 42))
    .Returns(true);

int result;
bool success = sut.TryParse("123", out result);
// result == 42, success == true

// Example: Setup with ref parameter
sut.SetupMock.Method.Increment(It.IsRef<int>(v => v + 1))
    .Returns(true);

int value = 5;
sut.Increment(ref value);
// value == 6
```

**Span Parameters (.NET 8+):**

- `It.IsSpan<T>(predicate)`: Matches `Span<T>` parameters that satisfy the predicate.
- `It.IsAnySpan<T>()`: Matches any `Span<T>` parameter.
- `It.IsReadOnlySpan<T>(predicate)`: Matches `ReadOnlySpan<T>` parameters that satisfy the predicate.
- `It.IsAnyReadOnlySpan<T>()`: Matches any `ReadOnlySpan<T>` parameter.

```csharp
// Example: Setup with Span parameter
sut.SetupMock.Method.Process(It.IsSpan<byte>(data => data.Length > 0))
    .Returns(true);

Span<byte> buffer = new byte[] { 1, 2, 3 };
bool result = sut.Process(buffer);
// result == true
```

**Custom Equality Comparers:**

Use `.Using(IEqualityComparer<T>)` to provide custom equality comparison for `It.Is()` and `It.IsOneOf()`:

```csharp
// Example: Case-insensitive string comparison
var comparer = StringComparer.OrdinalIgnoreCase;
sut.SetupMock.Method.Process(It.Is("hello").Using(comparer))
    .Returns(42);

int result = sut.Process("HELLO");
// result == 42
```

**Regular Expression Matching:**

Use `.AsRegex()` to enable regular expression matching for `It.Matches()`:

```csharp
// Example: Match email addresses
sut.SetupMock.Method.ValidateEmail(It.Matches(@"^\w+@\w+\.\w+$").AsRegex())
    .Returns(true);

bool result = sut.ValidateEmail("user@example.com");
// result == true

// Case-sensitive regex
sut.SetupMock.Method.Process(It.Matches("^[A-Z]+$").AsRegex().CaseSensitive())
    .Returns(1);
```

**Custom Parameter Predicates:**

When the method name is unique (no overloads), you can use flexible parameter matching:

- `Match.AnyParameters()`: Matches any parameters.
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

#### Parameter Interaction

With `Do`, you can register a callback for individual parameters of a method setup. This allows you to implement side
effects or checks directly when the method or indexer is called. With `.Monitor(out monitor)`, you can track the actual
values passed during test execution and analyze them afterwards.

**Example: Do for method parameter**

```csharp
int lastAmount = 0;
sut.SetupMock.Method.Dispense(It.Is("Dark"), It.IsAny<int>().Do(amount => lastAmount = amount));
sut.Dispense("Dark", 42);
// lastAmount == 42
```

**Example: Monitor for method parameter**

```csharp
Mockolate.ParameterMonitor<int> monitor;
sut.SetupMock.Method.Dispense(It.Is("Dark"), It.IsAny<int>().Monitor(out monitor));
sut.Dispense("Dark", 5);
sut.Dispense("Dark", 7);
// monitor.Values == [5, 7]
```

## Property Setup

Set up property getters and setters to control or verify property access on your mocks.

**Initialization**

You can initialize properties so they work like normal properties (setter changes the value, getter returns the last set
value):

```csharp
sut.SetupMock.Property.TotalDispensed.InitializeWith(42);
```

**Returns / Throws**

Alternatively, set up properties with `Returns` and `Throws` (supports sequences):

```csharp
sut.SetupMock.Property.TotalDispensed
    .Returns(1)
    .Returns(2)
    .Throws(new Exception("Error"))
    .Returns(4);
```

**Callbacks**

Register callbacks on the setter or getter:

```csharp
sut.SetupMock.Property.TotalDispensed.OnGet(() => Console.WriteLine("TotalDispensed was read!"));
sut.SetupMock.Property.TotalDispensed.OnSet((oldValue, newValue) => Console.WriteLine($"Changed from {oldValue} to {newValue}!") );
```

## Indexer Setup

Set up indexers with argument matchers. Supports initialization, returns/throws sequences, and callbacks.

```csharp
sut.SetupMock.Indexer(It.IsAny<string>())
    .InitializeWith(type => 20)
    .OnGet(type => Console.WriteLine($"Stock for {type} was read"));

sut.SetupMock.Indexer(It.Is("Dark"))
    .InitializeWith(10)
    .OnSet((value, type) => Console.WriteLine($"Set [{type}] to {value}"));
```

- `.InitializeWith(…)` can take a value or a callback with parameters.
- `.Returns(…)` and `.Throws(…)` support direct values, callbacks, and callbacks with parameters and/or the current
  value.
- `.OnGet(…)` and `.OnSet(…)` support callbacks with or without parameters.
- `.Returns(…)` and `.Throws(…)` can be chained to define a sequence of behaviors, which are cycled through on each
  call.
- Use `.SkippingBaseClass(…)` to override the base class behavior for a specific indexer (only for class mocks).
- When you specify overlapping setups, the most recently defined setup takes precedence.

**Note**:
You can use the same [parameter matching](#parameter-matching) and [interaction](#parameter-interaction) options as for
methods.
