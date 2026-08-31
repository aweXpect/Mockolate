# Create mocks

You can create mocks for interfaces and classes. For classes without a default constructor, provide constructor
arguments as an array to `CreateMock([…])`:

```csharp
// Create a mock of an interface
IChocolateDispenser sut = IChocolateDispenser.CreateMock();

// Create a mock of a class
MyChocolateDispenser classMock = MyChocolateDispenser.CreateMock();

// For classes without a default constructor:
MyChocolateDispenserWithCtor classWithArgsMock = MyChocolateDispenserWithCtor.CreateMock("Dark", 42);
```

## Customizing mock behavior

You can control the default behavior of the mock by providing a `MockBehavior`:

```csharp
IChocolateDispenser strictMock = IChocolateDispenser.CreateMock(MockBehavior.Default.ThrowingWhenNotSetup());

// For classes with constructor parameters and custom behavior:
MockBehavior behavior = new MockBehavior { ThrowWhenNotSetup = true };
MyChocolateDispenser classMock = MyChocolateDispenser.CreateMock(behavior, "Dark", 42);
```

**`MockBehavior` options**

| Option                                | Default           | Purpose                                                                                                                                                                                                               |
|---------------------------------------|-------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `SkipBaseClass`                       | `false`           | When `true`, the mock does not call any base class implementations. Otherwise, the base class implementation is used as the default value when no explicit setup matches.                                             |
| `ThrowWhenNotSetup`                   | `false`           | When `true`, the mock throws when no matching setup is found. Otherwise, it returns a default value (see `DefaultValue` below).                                                                                       |
| `SkipInteractionRecording`            | `false`           | When `true`, interactions are not recorded - setups, returns, callbacks, and base-class delegation still work, but `.Verify.X()` throws a `MockException`. Useful in performance-sensitive scenarios.                 |
| `DefaultValue`                        | sensible defaults | Customizes how default values are generated for unset methods and properties (see below).                                                                                                                             |
| `Initialize<T>(...)`                  | -                 | Automatically applies the given setups to all mocks of type `T` when they are created.                                                                                                                                |
| `UseConstructorParametersFor<T>(...)` | -                 | Configures default constructor parameters for mocks of type `T`, unless explicit parameters are supplied to `CreateMock([…])`. The `Func<object?[]>` overload defers parameter resolution until each mock is created. |

**Default value generation**

The default `IDefaultValueGenerator` provides sensible defaults for the most common cases:

- Empty collections for collection types (e.g., `IEnumerable<T>`, `List<T>`)
- Empty string for `string`
- Completed tasks for `Task`, `Task<T>`, `ValueTask`, and `ValueTask<T>`
- Tuples with recursively defaulted values
- `null` for other reference types

You can register custom factories per type using `.WithDefaultValueFor<T>()`:

```csharp
MockBehavior behavior = MockBehavior.Default
  .WithDefaultValueFor<string>(() => "default")
  .WithDefaultValueFor<int>(() => 42);
IChocolateDispenser sut = IChocolateDispenser.CreateMock(behavior);
```

For full control, implement `IDefaultValueGenerator` directly and assign it to `MockBehavior.DefaultValue`.

**Using a shared behavior**

You can reuse a `MockBehavior` instance across multiple mock creations to apply consistent, centrally configured
behavior:

```csharp
MockBehavior behavior = MockBehavior.Default.ThrowingWhenNotSetup();

IChocolateDispenser sut1 = IChocolateDispenser.CreateMock(behavior);
ILemonadeDispenser sut2 = ILemonadeDispenser.CreateMock(behavior);
```

This is especially useful when you need consistent mock setups across multiple tests or for different types.

## Setups

Specify setups during mock creation using the `CreateMock` overload with a setup callback. These setups also apply to
virtual interactions in the constructor.

```csharp
IChocolateDispenser sut = IChocolateDispenser.CreateMock(setup =>
{
    setup.Dispense(It.IsAny<string>(), It.IsAny<int>()).Returns(true);
    setup.TotalDispensed.InitializeWith(0);
});
```

You can combine the setup callback with a `MockBehavior` and constructor parameters in the same call.

## Implementing additional interfaces

You can specify additional interfaces that the mock also implements using `.Implementing<T>()`:

```csharp
// return type is a MyChocolateDispenser that also implements ILemonadeDispenser
MyChocolateDispenser sut = MyChocolateDispenser.CreateMock().Implementing<ILemonadeDispenser>();

// Create a mock implementing multiple interfaces with inline setups
IChocolateDispenser sut2 = IChocolateDispenser.CreateMock()
    .Implementing<ILemonadeDispenser>(setup => setup.DispenseLemonade(It.IsAny<int>()).Returns(true));
```

**Accessing the additional interface's mock surface**

Use `Mock.As<T>()` to reach the `Setup` and `Verify` properties for an additional interface added via
`.Implementing<T>()`:

```csharp
MyChocolateDispenser sut = MyChocolateDispenser.CreateMock()
    .Implementing<ILemonadeDispenser>();

// Set up and verify members of the additional interface
sut.Mock.As<ILemonadeDispenser>().Setup.DispenseLemonade(It.IsAny<int>()).Returns(true);
sut.Mock.As<ILemonadeDispenser>().Verify.DispenseLemonade(5).Once();
```

The returned mock shares the registry of the original - recorded interactions, scenario state, and setups apply
across all faces of the same instance.

Where the mock is already typed as the additional interface, its own `Mock` accessor reaches the same surface, so
the `As<T>()` hop can be skipped:

```csharp
((ILemonadeDispenser)sut).Mock.Setup.DispenseLemonade(It.IsAny<int>()).Returns(true);
((ILemonadeDispenser)sut).Mock.Verify.DispenseLemonade(5).Once();
```

`Mock.As<T>()` throws a `MockException` when the mock does not implement `T`.

**Reaching members hidden with `new`**

`Mock.As<T>()` also reaches a base interface whose members the mocked interface hides with `new`. No
`.Implementing<T>()` is needed here - the base interface is already part of the mocked type - and (for methods,
properties, and events) the two slots stay separate members, each with its own setups and recorded interactions
(indexers are an exception; see Notes below):
```csharp
public interface IChocolateShelfBase
{
    int Restock();
}

public interface IChocolateShelf : IChocolateShelfBase
{
    new int Restock();
}

IChocolateShelf sut = IChocolateShelf.CreateMock();
sut.Mock.Setup.Restock().Returns(42);
sut.Mock.As<IChocolateShelfBase>().Setup.Restock().Returns(43);

int viaDerived = sut.Restock();                     // 42
int viaBase = ((IChocolateShelfBase)sut).Restock(); // 43

sut.Mock.Verify.Restock().Once();
sut.Mock.As<IChocolateShelfBase>().Verify.Restock().Once();
```

**When the class already implements the interface**

`.Implementing<T>()` is also useful when the mocked class itself implements `T`, because it makes members the
class implements non-virtually reachable:

```csharp
public interface ICalculator
{
    int Add(int a, int b);
    int Multiply(int a, int b);
}

public abstract class Calculator : ICalculator
{
    public int Add(int a, int b) => a + b;          // not virtual
    public abstract int Multiply(int a, int b);
}

Calculator sut = Calculator.CreateMock().Implementing<ICalculator>();

// `Multiply` is overridable, so the class and the interface are one member:
// either surface configures it, and both calls are recorded against it.
sut.Mock.Setup.Multiply(It.IsAny<int>(), It.IsAny<int>()).Returns(7);
int viaClass = sut.Multiply(3, 4);                   // 7
int viaInterface = ((ICalculator)sut).Multiply(3, 4); // 7

// `Add` is not virtual, so the class call cannot be intercepted - only the interface slot can.
sut.Mock.As<ICalculator>().Setup.Add(It.IsAny<int>(), It.IsAny<int>()).Returns(99);
int addViaClass = sut.Add(1, 2);                     // 3 - the real implementation
int addViaInterface = ((ICalculator)sut).Add(1, 2);  // 99
```

**Notes:**

- Only the first type can be a class; additional types must be interfaces.
- Members the class implements non-virtually are only mockable through the interface; cast the mock (or type
  your subject as the interface) to reach them.
- `.Implementing<T>()` returns a new instance rather than modifying the one it was called on. Keep the returned
  mock - the original reference does not implement `T`.
- On class mocks, `.Implementing<T>()` reuses the constructor parameters the mock was created with, so
  `CreateMock([…])` arguments carry over.
- Indexers are keyed by their parameter signature rather than by the declaring interface, so a `new` indexer and
  the base indexer it hides share the same storage.

## Wrapping existing instances

You can wrap an existing instance with mock tracking using `.Wrapping(instance)`. This allows you to track interactions
with a real object:

```csharp
MyChocolateDispenser realDispenser = new MyChocolateDispenser();
IChocolateDispenser wrappedDispenser = IChocolateDispenser.CreateMock().Wrapping(realDispenser);

// Calls are forwarded to the real instance
wrappedDispenser.Dispense("Dark", 5);

// But you can still verify interactions
wrappedDispenser.Mock.Verify.Dispense(It.Is("Dark"), It.Is(5)).Once();
```

**Notes:**

- Both interface and class types can be wrapped.
- All public calls are forwarded to the wrapped instance.
- Interface members are forwarded through the interface that declares them, so each interface view of the mock
  reaches the matching member on the wrapped instance. This also covers members that hide a base member with
  `new` and same-named members declared by several base interfaces.
- You can still set up custom behavior that overrides the wrapped instance's behavior.
- Protected members are not forwarded to the wrapped instance; the base class implementation is used instead.
- Init-only properties are not forwarded to the wrapped instance, since it is already constructed.
- Verification works the same as with regular mocks.
