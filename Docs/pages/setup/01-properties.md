# Properties

Set up property getters and setters to control or verify property access on your mocks.

## Initialization

You can initialize properties so they work like normal properties (setter changes the value, getter returns the last set
value):

```csharp
sut.Mock.Setup.TotalDispensed.InitializeWith(42);
```

You can also register a setup without providing a value (useful when `ThrowWhenNotSetup` is enabled):

```csharp
IChocolateDispenser sut = IChocolateDispenser.CreateMock(MockBehavior.Default.ThrowingWhenNotSetup());

// Register property without value - won't throw
sut.Mock.Setup.TotalDispensed.Register();
```

## Returns / Throws

Set up properties with `Returns` and `Throws` (supports sequences):

```csharp
sut.Mock.Setup.TotalDispensed
    .Returns(1)
    .Returns(2)
    .Throws(new Exception("Error"))
    .Returns(4);
```

You can also return a value based on the previous value:

```csharp
sut.Mock.Setup.TotalDispensed
    .Returns(current => current + 10);  // Increment by 10 each read
```

## Callbacks

Register callbacks on the setter or getter:

```csharp
sut.Mock.Setup.TotalDispensed.OnGet
    .Do(() => Console.WriteLine("TotalDispensed was read!"));
sut.Mock.Setup.TotalDispensed.OnSet
    .Do(newValue => Console.WriteLine($"Changed to {newValue}!") );
```

Callbacks can also receive the current value:

```csharp
// Getter with the current value
sut.Mock.Setup.TotalDispensed
    .OnGet.Do(value => 
        Console.WriteLine($"Read TotalDispensed current value: {value}"));

// Setter with the new value
sut.Mock.Setup.TotalDispensed
    .OnSet.Do(newValue => 
        Console.WriteLine($"Set TotalDispensed to {newValue}"));
```

Callbacks also support sequences, similar to `Returns` and `Throws`:

```csharp
sut.Mock.Setup.TotalDispensed.OnGet
    .Do(() => Console.WriteLine("Execute on all even read interactions"))
    .Do(() => Console.WriteLine("Execute on all odd read interactions"));
```

## Properties with only one accessor

The setup and verify surfaces only offer the accessors the mock actually intercepts.
`Register()` and `SkippingBaseClass(…)` stay available on both, but the accessor-specific members do
not: a get-only property has no `OnSet`, and a set-only property has neither `OnGet` nor the
`Returns`/`Throws` read-sequence nor `InitializeWith`, since there is no getter to read the value
back.

```csharp
public interface IChocolateInventory
{
    int RemainingBars { get; }        // no setter
    string LastCountedBy { set; }     // no getter
}

IChocolateInventory sut = IChocolateInventory.CreateMock();

sut.Mock.Setup.RemainingBars.Returns(3);
sut.Mock.Setup.RemainingBars.Register();
sut.Mock.Setup.RemainingBars.OnSet…                 // does not compile
sut.Mock.Setup.RemainingBars.Returns(3).OnSet…      // does not compile

sut.Mock.Setup.LastCountedBy.OnSet.Do(value => { });
sut.Mock.Setup.LastCountedBy.Register();
sut.Mock.Setup.LastCountedBy.Returns("Ada")…        // does not compile
sut.Mock.Setup.LastCountedBy.InitializeWith("Ada")… // does not compile
sut.Mock.Setup.LastCountedBy.OnSet.Do(value => { }).OnGet… // does not compile
```

The verify facade likewise offers the intercepted accessor only:

```csharp
_ = sut.RemainingBars;
sut.LastCountedBy = "Ada";

await That(sut.Mock.Verify.RemainingBars.Got()).Once();
await That(sut.Mock.Verify.LastCountedBy.Set("Ada")).Once();

sut.Mock.Verify.RemainingBars.Set(3)…               // does not compile
sut.Mock.Verify.LastCountedBy.Got()…                // does not compile
```

This also applies when the property declares an accessor the mock cannot see, such as
`{ get; internal set; }` on a type from an assembly that does not grant `InternalsVisibleTo`. Writes
never reach the mock in that case, so configuring or verifying one could only ever report zero
interactions. See [Mockolate0002](../analyzers#mockolate0002) for when such a type is mockable at all.

Both facades are fully restricted: the fluent builders returned by `Returns`, `Throws`, `Do` and
`TransitionTo` stay on the narrowed surface, so no amount of chaining reaches the accessor the mock
does not intercept.

**Notes:**

- Use `.SkippingBaseClass(…)` to override the base class behavior for a specific property (only for class mocks).
- All callbacks and return values support more advanced features like conditional execution, frequency control,
  parallel execution, and access to the invocation counter.
  See [Advanced callback features](../advanced-features/advanced-callback-features) for details.
