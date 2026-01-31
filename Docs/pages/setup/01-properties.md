# Properties

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
sut.SetupMock.Property.TotalDispensed.OnGet.Do(() => Console.WriteLine("TotalDispensed was read!"));
sut.SetupMock.Property.TotalDispensed.OnSet.Do((oldValue, newValue) => Console.WriteLine($"Changed from {oldValue} to {newValue}!") );
```

## Advanced Features

**Advanced Property Returns**

Return a value based on the previous value:

```csharp
sut.SetupMock.Property.TotalDispensed
    .Returns((current) => current + 10);  // Increment by 10 each read
```

**Advanced Callbacks**

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

**Register Without Value**

Register a setup without providing a value (useful with `ThrowWhenNotSetup`):

```csharp
var strictMock = Mock.Create<IChocolateDispenser>(
    new MockBehavior { ThrowWhenNotSetup = true });

// Register property without value - won't throw
strictMock.SetupMock.Property.TotalDispensed.Register();
```
