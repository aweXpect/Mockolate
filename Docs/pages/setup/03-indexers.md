# Indexers

Set up indexers with argument matchers. Supports initialization, returns/throws sequences, and callbacks.

```csharp
sut.SetupMock.Indexer(It.IsAny<string>())
    .InitializeWith(type => 20)
    .OnGet.Do(type => Console.WriteLine($"Stock for {type} was read"));

sut.SetupMock.Indexer(It.Is("Dark"))
    .InitializeWith(10)
    .OnSet.Do((value, type) => Console.WriteLine($"Set [{type}] to {value}"));
```

- `.InitializeWith(…)` can take a value or a callback with parameters.
- `.Returns(…)` and `.Throws(…)` support direct values, callbacks, and callbacks with parameters and/or the current
  value.
- `.OnGet.Do(…)` and `.OnSet.Do(…)` support callbacks with or without parameters.
- `.Returns(…)` and `.Throws(…)` can be chained to define a sequence of behaviors, which are cycled through on each
  call.
- Use `.SkippingBaseClass(…)` to override the base class behavior for a specific indexer (only for class mocks).
- When you specify overlapping setups, the most recently defined setup takes precedence.

## Advanced Features

**Advanced Indexer Returns**

Return a value based on the previous value:

```csharp
sut.SetupMock.Indexer(It.Is("Dark"))
    .Returns((string type, int current) => current + 10);  // Increment by 10 each read
```

**Advanced Callbacks**

Access invocation counter and values in callbacks:

```csharp
// Getter with counter, parameter, and current value
sut.SetupMock.Indexer(It.IsAny<string>())
    .OnGet.Do((int count, string type, int value) => 
        Console.WriteLine($"Read #{count} for {type}, current value: {value}"));

// Setter with counter, parameter, and new value
sut.SetupMock.Indexer(It.IsAny<string>())
    .OnSet.Do((int count, string type, int newValue) => 
        Console.WriteLine($"Set #{count} for {type} to {newValue}"));
```

**Note**:
You can use the same [parameter matching](https://awexpect.com/docs/mockolate/setup/parameter-matching)
and [interaction](https://awexpect.com/docs/mockolate/setup/parameter-matching#parameter-interaction) options as for
methods.
