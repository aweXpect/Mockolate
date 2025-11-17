# Setup

Set up return values or behaviors for methods, properties, and indexers on your mock. Control how the mock responds to
calls in your tests.

## Method Setup

Use `mock.SetupMock.Method.MethodName(…)` to set up methods. You can specify argument matchers for each parameter.

```csharp
// Setup Dispense to decrease stock and raise event
sut.SetupMock.Method.Dispense(With("Dark"), Any<int>())
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
sut.SetupMock.Method.Dispense(With("White"), Any<int>())
    .Callback((type, amount) => Console.WriteLine($"Dispensed {amount} {type} chocolate."));

// Setup method to throw
sut.SetupMock.Method.Dispense(With("Green"), Any<int>())
    .Throws<InvalidChocolateException>();
```

- Use `.Callback(…)` to run code when the method is called. Supports parameterless or parameter callbacks.
- Use `.Returns(…)` to specify the value to return. You can provide a direct value, a callback, or a callback with
  parameters.
- Use `.Throws(…)` to specify an exception to throw. Supports direct exceptions, exception factories, or factories with
  parameters.
- Use `.Returns(…)` and `.Throws(…)` repeatedly to define a sequence of return values or exceptions (cycled on each
  call).
- Use `.CallingBaseClass(…)` to override the base class behavior for a specific method (only for class mocks).
- When you specify overlapping setups, the most recently defined setup takes precedence.

**Async Methods**

For `Task<T>` or `ValueTask<T>` methods, use `.ReturnsAsync(…)`:

```csharp
sut.SetupMock.Method.DispenseAsync(Any<string>(), Any<int>())
    .ReturnsAsync(true);
```

### Argument Matching

Mockolate provides flexible argument matching for method setups and verifications:

- `Match.Any<T>()`: Matches any value of type `T`.
- `Match.With<T>(predicate)`: Matches values based on a predicate.
- `Match.With<T>(value)`: Matches a specific value.
- `Match.Null<T>()`: Matches null.
- `Match.Out<T>(…)`/`Match.Ref<T>(…)`: Matches and sets out/ref parameters, supports value setting and
  predicates.

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
sut.SetupMock.Indexer(Any<string>())
    .InitializeWith(type => 20)
    .OnGet(type => Console.WriteLine($"Stock for {type} was read"));

sut.SetupMock.Indexer(With("Dark"))
    .InitializeWith(10)
    .OnSet((value, type) => Console.WriteLine($"Set [{type}] to {value}"));
```

- `.InitializeWith(…)` can take a value or a callback with parameters.
- `.Returns(…)` and `.Throws(…)` support direct values, callbacks, and callbacks with parameters and/or the current
  value.
- `.OnGet(…)` and `.OnSet(…)` support callbacks with or without parameters.
- `.Returns(…)` and `.Throws(…)` can be chained to define a sequence of behaviors, which are cycled through on each
  call.
- Use `.CallingBaseClass(…)` to override the base class behavior for a specific indexer (only for class mocks).
- When you specify overlapping setups, the most recently defined setup takes precedence.
