# Setup

Set up return values or behaviors for methods, properties, and indexers on your mock. Control how the mock responds to calls in your tests.

## Method Setup

Use `mock.Setup.Method.MethodName(...)` to set up methods. You can specify argument matchers for each parameter.

```csharp
mock.Setup.Method.AddUser(With.Any<string>())
    .Returns(name => new User(Guid.NewGuid(), name));

mock.Setup.Method.TryDelete(With.Any<Guid>(), With.Out<User?>(() => new User(id, "Alice")))
    .Returns(true);

mock.Setup.Method.DoSomething(With.Matching<int>(x => x > 0))
    .Throws(() => new InvalidOperationException());

mock.Setup.Method.DoWork()
    .Callback(() => Console.WriteLine("Method called!"));
```

- Use `.Callback(...)` to run code when the method is called. Supports parameterless or parameter callbacks.
- Use `.Returns(...)` to specify the value to return. You can provide a direct value, a callback, or a callback with parameters.
- Use `.Throws(...)` to specify an exception to throw. Supports direct exceptions, exception factories, or factories with parameters.
- Use `.Returns(...)` and `.Throws(...)` repeatedly to define a sequence of return values or exceptions (cycled on each call).

**Async Methods**

For `Task<T>` or `ValueTask<T>` methods, use `.ReturnsAsync(...)`:

```csharp
mock.Setup.Method.GetValueAsync(With.Any<int>())
    .ReturnsAsync(i => i * 2);
```

### Argument Matching

Mockolate provides flexible argument matching for method setups and verifications:

- `With.Any<T>()`: Matches any value of type `T`.
- `With.Matching<T>(predicate)`: Matches values based on a predicate.
- `With.Value<T>(value)`: Matches a specific value.
- `With.Null<T>()`: Matches null.
- `With.Out<T>(...)`/`With.Ref<T>(...)`: Matches and sets out/ref parameters, supports value setting and predicates.
- For .NET 8+: `With.ValueBetween<T>(min).And(max)` matches a range (numeric types).

## Property Setup

Set up property getters and setters to control or verify property access on your mocks. Supports auto-properties and indexers.

**Initialization**

You can initialize properties so they work like normal properties (setter changes the value, getter returns the last set value):

```csharp
mock.Setup.Property.MyProperty.InitializeWith(42);
```

**Returns / Throws**

Alternatively, set up properties with `Returns` and `Throws` (supports sequences):

```csharp
mock.Setup.Property.MyProperty
    .Returns(1)
    .Returns(2)
    .Throws(new Exception("Error"))
    .Returns(4);
```

**Callbacks**

Register callbacks on the setter or getter:

```csharp
mock.Setup.Property.MyProperty.OnGet(() => Console.WriteLine("MyProperty was read!"));
mock.Setup.Property.MyProperty.OnSet((oldValue, newValue) => Console.WriteLine($"Changed from {oldValue} to {newValue}!"));
```

## Indexer Setup

Set up indexers with argument matchers. Supports initialization, returns/throws sequences, and callbacks.

```csharp
mock.Setup.Indexer(With.Any<int>())
    .InitializeWith(index => index * index)
    .Returns((v, index) => v + 1)
    .OnGet(index => Console.WriteLine($"Indexer this[{index}] was read"));

mock.Setup.Indexer(With.Any<int>(), With.Matching<int>(i => i > 0))
    .InitializeWith((i, j) => $"init-{i}-{j}")
    .Returns((v, i, j) => $"{v}-{i}-{j}")
    .OnSet((value, i, j) => Console.WriteLine($"Set [{i},{j}] to {value}"));
```

- `.InitializeWith(...)` can take a value or a callback with parameters.
- `.Returns(...)` and `.Throws(...)` support direct values, callbacks, and callbacks with parameters and/or the current value.
- `.OnGet(...)` and `.OnSet(...)` support callbacks with or without parameters.
- `.Returns(...)` and `.Throws(...)` can be chained to define a sequence of behaviors, which are cycled through on each call.
