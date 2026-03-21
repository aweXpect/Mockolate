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

## Initialization

You can initialize indexers so they work like normal indexers (setter changes the value, getter returns the last set
value):

```csharp
sut.SetupMock.Indexer(It.IsAny<string>()).InitializeWith(42);
```

## Returns / Throws

Set up indexers with `Returns` and `Throws` (supports sequences):

```csharp
sut.SetupMock.Indexer(It.IsAny<string>())
    .Returns(1)
    .Returns(2)
    .Throws(new Exception("Error"))
    .Returns(4);
```

You can also return a value based on the previous value:

```csharp
sut.SetupMock.Indexer(It.IsAny<string>())
    .Returns(current => current + 10);  // Increment by 10 each read
```

## Callbacks

Register callbacks on the setter or getter of the indexer:

```csharp
sut.SetupMock.Indexer(It.IsAny<string>()).OnGet
    .Do(() => Console.WriteLine("Indexer was read!"));
sut.SetupMock.Indexer(It.IsAny<string>()).OnSet
    .Do(newValue => Console.WriteLine($"Changed indexer to {newValue}!") );
```

Callbacks can also receive the indexer parameters and the current value:

```csharp
// Getter with the current value
sut.SetupMock.Indexer(It.IsAny<string>())
    .OnGet.Do((string index, int value) => 
        Console.WriteLine($"Read this[{index}] current value: {value}"));

// Setter with the new value
sut.SetupMock.Indexer(It.IsAny<string>())
    .OnSet.Do((string index, int newValue) => 
        Console.WriteLine($"Set this[{index}] to {newValue}"));
```

Callbacks also support sequences, similar to `Returns` and `Throws`:

```csharp
sut.SetupMock.Indexer(It.IsAny<string>()).OnGet
    .Do(() => Console.WriteLine("Execute on all even read interactions"))
    .Do(() => Console.WriteLine("Execute on all odd read interactions"));
```

**Notes:**

- All callbacks support more advanced features like conditional execution, frequency control, parallel execution, and
  access to the invocation counter.
  See [Advanced callback features](https://awexpect.com/docs/mockolate/advanced-features/advanced-callback-features) for
  details.
- You can use the same [parameter matching](https://awexpect.com/docs/mockolate/setup/parameter-matching)
  and [interaction](https://awexpect.com/docs/mockolate/setup/parameter-matching#parameter-interaction) options as for
  methods.
- Use `.SkippingBaseClass(…)` to override the base class behavior for a specific indexer (only for class mocks).
- When you specify overlapping setups, the most recently defined setup takes precedence.
