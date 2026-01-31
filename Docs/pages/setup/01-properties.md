# Properties

Set up property getters and setters to control or verify property access on your mocks.

## Initialization

You can initialize properties so they work like normal properties (setter changes the value, getter returns the last set
value):

```csharp
sut.SetupMock.Property.TotalDispensed.InitializeWith(42);
```

You can also register a setup without providing a value (useful when `ThrowWhenNotSetup` is enabled):

```csharp
var strictMock = Mock.Create<IChocolateDispenser>(MockBehavior.Default.ThrowingWhenNotSetup());

// Register property without value - won't throw
strictMock.SetupMock.Property.TotalDispensed.Register();
```

## Returns / Throws

Set up properties with `Returns` and `Throws` (supports sequences):

```csharp
sut.SetupMock.Property.TotalDispensed
    .Returns(1)
    .Returns(2)
    .Throws(new Exception("Error"))
    .Returns(4);
```

You can also return a value based on the previous value:

```csharp
sut.SetupMock.Property.TotalDispensed
    .Returns(current => current + 10);  // Increment by 10 each read
```

## Callbacks

Register callbacks on the setter or getter:

```csharp
sut.SetupMock.Property.TotalDispensed.OnGet
    .Do(() => Console.WriteLine("TotalDispensed was read!"));
sut.SetupMock.Property.TotalDispensed.OnSet
    .Do(newValue => Console.WriteLine($"Changed to {newValue}!") );
```

Callbacks can also receive the current value:

```csharp
// Getter with the current value
sut.SetupMock.Property.TotalDispensed
    .OnGet.Do(value => 
        Console.WriteLine($"Read TotalDispensed current value: {value}"));

// Setter with the new value
sut.SetupMock.Property.TotalDispensed
    .OnSet.Do(newValue => 
        Console.WriteLine($"Set TotalDispensed to {newValue}"));
```

Callbacks also support sequences, similar to `Returns` and `Throws`:

```csharp
sut.SetupMock.Property.TotalDispensed.OnGet
    .Do(() => Console.WriteLine("Execute on all even read interactions"))
    .Do(() => Console.WriteLine("Execute on all odd read interactions"));
```

**Notes:**

- All callbacks support more advanced features like conditional execution, frequency control, parallel execution, and
  access to the invocation counter.
  See [Advanced callback features](https://awexpect.com/docs/mockolate/advanced-features/advanced-callback-features)
  for details.
