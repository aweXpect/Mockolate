# Indexers

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
You can use the same [parameter matching](https://awexpect.com/docs/mockolate/setup/parameter-matching)
and [interaction](https://awexpect.com/docs/mockolate/setup/parameter-matching#parameter-interaction) options as for
methods.
