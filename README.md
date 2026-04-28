# Mockolate

<img align="right" width="200" src="Docs/logo_256x256.png" alt="Mockolate logo" />

[![Nuget](https://img.shields.io/nuget/v/Mockolate)](https://www.nuget.org/packages/Mockolate)
[![Build](https://github.com/aweXpect/Mockolate/actions/workflows/build.yml/badge.svg)](https://github.com/aweXpect/Mockolate/actions/workflows/build.yml)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=aweXpect_Mockolate&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=aweXpect_Mockolate)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=aweXpect_Mockolate&metric=coverage)](https://sonarcloud.io/summary/overall?id=aweXpect_Mockolate)
[![Mutation testing badge](https://img.shields.io/endpoint?style=flat&url=https%3A%2F%2Fbadge-api.stryker-mutator.io%2Fgithub.com%2FaweXpect%2FMockolate%2Fmain)](https://dashboard.stryker-mutator.io/reports/github.com/aweXpect/Mockolate/main)

**Mockolate** is a modern, strongly-typed, AOT-compatible mocking library for .NET, powered by source generators.
It enables fast, compile-time validated mocking with .NET Standard 2.0, .NET 8, .NET 10 and .NET Framework 4.8.

- **Source generator-based**: No runtime proxy generation.
- **Fast**: Direct dispatch with no reflection or dynamic proxies.
- **Strongly-typed**: Compile-time safety and IntelliSense support.
- **AOT compatible**: Works with Native AOT and trimming.
- **Modern C#**: First-class support for ref structs, static interface members, and current language features.

## Why Mockolate

|                | Reflection-based mocks (Moq, NSubstitute, …) | Mockolate                  |
|----------------|----------------------------------------------|----------------------------|
| AOT / trimming | not supported                                | supported                  |
| Validation     | runtime exceptions                           | analyzers + compile errors |
| Setup API      | `Expression<Func<…>>` trees                  | regular method calls       |
| Hot path       | dynamic-proxy dispatch                       | direct dispatch            |

For side-by-side setup, usage, and verification syntax against Moq, NSubstitute, and FakeItEasy, see the
[full code comparison](https://awexpect.com/docs/mockolate/comparison).

Already on Moq? The companion package [`Mockolate.Migration`](https://github.com/aweXpect/Mockolate.Migration) ships
analyzers and code fixers that translate common Moq patterns to Mockolate syntax in-place: point it at an existing test
project and apply the suggested fixes.

## Getting Started

1. **Check prerequisites**  
   Install the [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0), because Mockolate leverages
   C# 14 extension members (the projects can still target any supported framework).

2. **Install the package**
   ```powershell
   dotnet add package Mockolate
   ```

3. **Create and use a mock**
   ```csharp
   using Mockolate;

   public interface IChocolateDispenser
   {
       bool Dispense(string type, int amount);
   }

   // Create a mock
   IChocolateDispenser sut = IChocolateDispenser.CreateMock();

   // Setup: Dispense returns true for any Dark chocolate request
   sut.Mock.Setup.Dispense("Dark", It.IsAny<int>()).Returns(true);

   // Act
   bool success = sut.Dispense("Dark", 4);

   // Verify
   sut.Mock.Verify.Dispense("Dark", It.IsAny<int>()).Once();
   ```

   For a richer walkthrough combining properties, indexers, events, and stateful setup,
   see [A complete example](#a-complete-example) at the end of the README.

## Create mocks

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

### Customizing mock behavior

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

### Setups

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

### Implementing additional interfaces

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

**Notes:**

- Only the first type can be a class; additional types must be interfaces.

### Wrapping existing instances

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
- You can still set up custom behavior that overrides the wrapped instance's behavior.
- Protected members are not forwarded to the wrapped instance; the base class implementation is used instead.
- Verification works the same as with regular mocks.

## Setup

Set up return values or behaviors for methods, properties, and indexers on your mock. Control how the mock responds to
calls in your tests.

### Properties

Set up property getters and setters to control or verify property access on your mocks.

**Initialization**

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

**Returns / Throws**

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

**Callbacks**

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

*Notes:*

- Use `.SkippingBaseClass(…)` to override the base class behavior for a specific property (only for class mocks).
- All callbacks and return values support more advanced features like conditional execution, frequency control,
  parallel execution, and access to the invocation counter.
  See [Advanced callback features](#advanced-callback-features) for details.

### Methods

Use `sut.Mock.Setup.MethodName(…)` to set up methods. You can specify argument matchers for each parameter.

**Returns / Throws**

Use `.Returns(…)` to specify the value to return. You can provide a direct value, a callback, or a callback with
parameters.
Use `.Throws(…)` to specify an exception to throw. Supports direct exceptions, exception factories, or factories with
parameters.

You can call `.Returns(…)` and `.Throws(…)` multiple times to define a sequence of return values or exceptions (cycled
on each call).

```csharp
// Setup Dispense to decrease stock and raise event
sut.Mock.Setup.Dispense(It.Is("Dark"), It.IsAny<int>())
    .Returns((type, amount) =>
    {
        var current = sut[type];
        if (current >= amount)
        {
            sut[type] = current - amount;
            sut.Mock.Raise.ChocolateDispensed(type, amount);
            return true;
        }
        return false;
    });

// Setup method to throw
sut.Mock.Setup.Dispense(It.Is("Green"), It.IsAny<int>())
    .Throws<InvalidChocolateException>();

// Sequence of returns and throws
sut.Mock.Setup.Dispense(It.IsAny<string>(), It.IsAny<int>())
    .Returns(true)
    .Throws(new Exception("Error"))
    .Returns(false);
```

**Async Methods**

For async methods returning `Task`/`Task<T>` or `ValueTask`/`ValueTask<T>`, use `.ReturnsAsync(…)` or `ThrowsAsync(…)`
respectively:

```csharp
sut.Mock.Setup.DispenseAsync(It.IsAny<string>(), It.IsAny<int>())
    .ReturnsAsync((_, v) => v)            // First execution returns the value of the `int` parameter
    .ThrowsAsync(new TimeoutException())  // Second execution throws a TimeoutException
    .ReturnsAsync(0).Forever();           // Subsequent executions return 0
```

**Callbacks**

Use `.Do(…)` to run code when the method is called. Supports parameterless or parameter callbacks.

```csharp
// Setup method with callback
sut.Mock.Setup.Dispense(It.Is("White"), It.IsAny<int>())
    .Do((type, amount) => Console.WriteLine($"Dispensed {amount} {type} chocolate."));
```

*Notes:*

- Use `.SkippingBaseClass(…)` to override the base class behavior for a specific method (only for class mocks).
- When you specify overlapping setups, the most recently defined setup takes precedence.
- All callbacks and return values support more advanced features like conditional execution, frequency control,
  parallel execution, and access to the invocation counter.
  See [Advanced callback features](#advanced-callback-features) for details.

### Indexers

Set up indexers with argument matchers. Supports initialization, returns/throws sequences, and callbacks.

```csharp
sut.Mock.Setup[It.IsAny<string>()]
    .InitializeWith(type => 20)
    .OnGet.Do(type => Console.WriteLine($"Stock for {type} was read"));

sut.Mock.Setup[It.Is("Dark")]
    .InitializeWith(10)
    .OnSet.Do((type, value) => Console.WriteLine($"Set [{type}] to {value}"));
```

**Initialization**

You can initialize indexers so they work like normal indexers (setter changes the value, getter returns the last set
value):

```csharp
sut.Mock.Setup[It.IsAny<string>()].InitializeWith(42);
```

**Returns / Throws**

Set up indexers with `Returns` and `Throws` (supports sequences):

```csharp
sut.Mock.Setup[It.IsAny<string>()]
    .Returns(1)
    .Returns(2)
    .Throws(new Exception("Error"))
    .Returns(4);
```

You can also return a value based on the previous value:

```csharp
sut.Mock.Setup[It.IsAny<string>()]
    .Returns(current => current + 10);  // Increment by 10 each read
```

**Callbacks**

Register callbacks on the setter or getter of the indexer:

```csharp
sut.Mock.Setup[It.IsAny<string>()].OnGet
    .Do(() => Console.WriteLine("Indexer was read!"));
sut.Mock.Setup[It.IsAny<string>()].OnSet
    .Do(newValue => Console.WriteLine($"Changed indexer to {newValue}!") );
```

Callbacks can also receive the indexer parameters and the current value:

```csharp
// Getter with the current value
sut.Mock.Setup[It.IsAny<string>()]
    .OnGet.Do((string index, int value) => 
        Console.WriteLine($"Read this[{index}] current value: {value}"));

// Setter with the new value
sut.Mock.Setup[It.IsAny<string>()]
    .OnSet.Do((string index, int newValue) => 
        Console.WriteLine($"Set this[{index}] to {newValue}"));
```

Callbacks also support sequences, similar to `Returns` and `Throws`:

```csharp
sut.Mock.Setup[It.IsAny<string>()].OnGet
    .Do(() => Console.WriteLine("Execute on all even read interactions"))
    .Do(() => Console.WriteLine("Execute on all odd read interactions"));
```

**Notes:**

- All callbacks support more advanced features like conditional execution, frequency control, parallel execution, and
  access to the invocation counter.
  See [Advanced callback features](#advanced-callback-features) for
  details.
- You can use the same [parameter matching](#parameter-matching)
  and [interaction](#parameter-interaction) options as for
  methods.
- Use `.SkippingBaseClass(…)` to override the base class behavior for a specific indexer (only for class mocks).
- When you specify overlapping setups, the most recently defined setup takes precedence.

### Parameter Matching

Mockolate provides flexible parameter matching for method setups and verifications:

#### Parameter Matchers

**Basic Matchers**

- `It.IsAny<T>()`: Matches any value of type `T`.
- `It.Is<T>(value)`: Matches a specific value.
- `It.IsNot<T>(value)`: Matches any value not equal to `value`.
- `It.IsOneOf<T>(params IEnumerable<T> values)`: Matches any of the given values.
- `It.IsNotOneOf<T>(params IEnumerable<T> values)`: Matches any value that is not in the given set.
- `It.IsNull<T>()`: Matches null.
- `It.IsNotNull<T>()`: Matches any non-null value.
- `It.IsTrue()`/`It.IsFalse()`: Matches boolean true/false.
- `It.IsInRange(min, max)`: Matches a number within the given range. You can append `.Exclusive()` to exclude the
  minimum and maximum value.
- `It.Satisfies<T>(predicate)`: Matches values based on a predicate.

*Note:*  
You can also directly use a value (equivalent to wrapping it in `It.Is<T>(value)`). For methods and indexers with
up to 4 parameters, you can freely mix matchers with direct values in any position. For members with more than
4 parameters, Mockolate still supports explicit values, but limits arbitrary per-parameter mixing to avoid a
combinatorial explosion of overloads. You may need to use direct values for all explicit-value-capable parameters
and wrap the remaining ones, or use matchers for all parameters.

**String Matching**

- `It.Matches(pattern)`: Matches strings using wildcard patterns (`*` and `?`).

**Regular Expressions**  
Use `.AsRegex()` to enable regular expression matching for `It.Matches()`:

```csharp
// Example: Match email addresses
sut.Mock.Setup.ValidateEmail(It.Matches(@"^\w+@\w+\.\w+$").AsRegex())
    .Returns(true);

bool result = sut.ValidateEmail("user@example.com");

// Case-sensitive regex
sut.Mock.Setup.Process(It.Matches("^[A-Z]+$").AsRegex().CaseSensitive())
    .Returns(1);
```

**Ref and Out Parameters**

- `It.IsRef<T>(setter)`: Matches any `ref` parameter and sets a new value using the setter function.
- `It.IsRef<T>(predicate, setter)`: Matches `ref` parameters that satisfy the predicate and sets a new value.
- `It.IsRef<T>(predicate)`: Matches `ref` parameters that satisfy the predicate without changing the value.
- `It.IsAnyRef<T>()`: Matches any `ref` parameter without restrictions.
- `It.IsOut<T>(setter)`: Matches any `out` parameter and sets a value using the setter function.
- `It.IsAnyOut<T>()`: Matches any `out` parameter without restrictions and sets the parameter to the default value of
  `T`.

```csharp
// Example: Setup with out parameter
sut.Mock.Setup.TryParse(It.IsAny<string>(), It.IsOut(() => 42))
    .Returns(true);

int result;
bool success = sut.TryParse("abc", out result);
// result == 42, success == true

// Example: Setup with ref parameter
sut.Mock.Setup.Increment(It.IsRef<int>(v => v + 1))
    .Returns(true);

int value = 5;
sut.Increment(ref value);
// value == 6
```

**Collection Matchers**

- `It.Contains<T>(item)`: Matches a collection parameter that contains `item`.
- `It.SequenceEquals<T>(params IEnumerable<T> values)`: Matches a collection parameter whose elements equal `values` in
  the same order.

Both matchers support method parameters declared as `IEnumerable<T>`, `ICollection<T>`, `IList<T>`,
`IReadOnlyCollection<T>`, `IReadOnlyList<T>`, `T[]`, `List<T>`, `Queue<T>` or `Stack<T>`.
`It.Contains<T>` additionally supports the unordered shapes `ISet<T>` and `HashSet<T>`; `It.SequenceEquals<T>`
intentionally
does not, because their enumeration order is not guaranteed.

Append `.Using(IEqualityComparer<T>)` to either matcher to control element equality.

```csharp
// Example: Match a list that contains a specific item
sut.Mock.Setup.Process(It.Contains(5))
    .Returns(true);

bool result = sut.Process(new[] { 1, 2, 5 });
// result == true

// Example: Match a sequence of items in order
sut.Mock.Setup.Process(It.SequenceEquals("a", "b", "c"))
    .Returns(true);

bool match = sut.Process(new[] { "a", "b", "c" });
// match == true

// Example: Case-insensitive containment
sut.Mock.Setup.Process(It.Contains("HELLO").Using(StringComparer.OrdinalIgnoreCase))
    .Returns(true);
```

**Custom Equality Comparers**

Use `.Using(IEqualityComparer<T>)` to provide custom equality comparison for `It.Is()` and `It.IsOneOf()`:

```csharp
// Example: Case-insensitive string comparison
var comparer = StringComparer.OrdinalIgnoreCase;
sut.Mock.Setup.Process(It.Is("hello").Using(comparer))
    .Returns(42);

int result = sut.Process("HELLO");
// result == 42
```

**Span Parameters (.NET 8+)**

- `It.IsSpan<T>(predicate)`: Matches `Span<T>` parameters that satisfy the predicate.
- `It.IsAnySpan<T>()`: Matches any `Span<T>` parameter.
- `It.IsReadOnlySpan<T>(predicate)`: Matches `ReadOnlySpan<T>` parameters that satisfy the predicate.
- `It.IsAnyReadOnlySpan<T>()`: Matches any `ReadOnlySpan<T>` parameter.

*Note:*  
As `ref struct` types cannot be stored directly, it is converted to an array internally and the `predicate` receives
this array for evaluation.

```csharp
// Example: Setup with Span parameter
sut.Mock.Setup.Process(It.IsSpan<byte>(data => data.Length > 0))
    .Returns(true);

Span<byte> buffer = new byte[] { 1, 2, 3 };
bool result = sut.Process(buffer);
// result == true
```

**Ref Struct Parameters (.NET 9+)**

You can mock methods and indexers that take custom `ref struct` parameters (e.g. `Utf8JsonReader`
or your own `ref struct Packet`) using these matchers:

- `It.IsAnyRefStruct<T>()`: Matches any ref-struct value of type `T`.
- `It.IsRefStruct<T>(predicate)`: Matches ref-struct values that satisfy the predicate. The
  predicate can read the struct's fields at the time the call is made.
- `It.IsRefStructBy<T, TKey>(projection)` / `It.IsRefStructBy<T, TKey>(projection, predicate)`:
  For ref-struct-keyed indexers, projects the key to an equatable value so writes and reads can
  be correlated. Works at any arity - apply it to every ref-struct slot and non-ref-struct slots
  contribute their raw value to the composite dispatch key (see *Indexer storage* in the remarks).

```csharp
public readonly ref struct Packet(int id, ReadOnlySpan<byte> payload)
{
    public int Id { get; } = id;
    public ReadOnlySpan<byte> Payload { get; } = payload;
}

public interface IPacketSink
{
    void Consume(Packet packet);
    int TryParse(Packet packet);
    string this[Packet key] { get; set; }
}

// Match on the live ref struct, including its Span contents.
sut.Mock.Setup.Consume(It.IsRefStruct<Packet>(p =>
        p.Payload.Length > 0 && p.Payload[0] == 0xFF))
    .Throws<InvalidOperationException>();

// Return a value from a ref-struct-parameter method.
sut.Mock.Setup.TryParse(It.IsAnyRefStruct<Packet>()).Returns(42);

// Ref-struct-keyed indexer: Returns for get, OnSet for observed writes.
sut.Mock.Setup[It.IsAnyRefStruct<Packet>()]
    .Returns("got")
    .OnSet(value => { /* observed write */ });

// Correlate writes and reads by projecting the key to an equatable value.
sut.Mock.Setup[It.IsRefStructBy<Packet, int>(p => p.Id)].Returns("fallback");
sut[new Packet(1, [])] = "written";
string a = sut[new Packet(1, [])];  // "written" matched by Id
string b = sut[new Packet(2, [])];  // "fallback" no write under Id=2
```

*Remarks*

The ref-struct pipeline uses a narrower API than the rest of Mockolate. A handful of fluent
features are unavailable because the C# language does not let `ref struct` values flow through
generic delegates:

- **Setup surface.** Only `Returns(value)`, `Returns(factory)`, `Throws*`, `OnSet(Action<TValue>)`
  (indexer setters), and `SkippingBaseClass` are available. The `.Do(...)` callback and the
  `Callbacks<T>` builder (`InParallel`, `When`, `For`, `Only`, `TransitionTo`) are not offered
  for ref-struct parameters.
- **Verify.** `Verify` counts calls to the method but cannot match on the parameter value after
  the fact - the ref-struct value isn't retained past the call. Use a setup-time matcher to
  filter at call time.
- **Indexer storage.** By default, values written through a ref-struct-keyed indexer setter are
  not read back by the getter. Apply `It.IsRefStructBy<T, TKey>(projection)` to every ref-struct
  slot to enable write-then-read correlation keyed by the projections; non-ref-struct slots
  contribute their raw value as part of the composite dispatch key. If any ref-struct slot is
  matched without a projection, storage stays inactive for that setup.

The following cases are rejected at compile time with diagnostic `Mockolate0003`:

- Targeting older than .NET 9 (the feature relies on `allows ref struct`, a .NET 9 / C# 13
  feature).
- `out` / `ref` / `ref readonly` parameters of a ref-struct type.
- Methods that return a custom ref struct. (`Span<T>` / `ReadOnlySpan<T>` returns are supported.)

#### Parameter Predicates

When the method name is unique (no overloads), you can use argument matchers from the `Match` class for more flexible
parameters matching:

- `Match.AnyParameters()`: Matches any parameter combination.
- `Match.Parameters(Func<object?[], bool> predicate)`: Matches parameters based on a custom predicate.

```csharp
// Example: Custom parameter predicate
sut.Mock.Setup.Process(Match.Parameters(args =>
    args.Length == 2 &&
    args[0] is string s && s.StartsWith("test") &&
    args[1] is int i && i > 0))
    .Returns(true);

bool result = sut.Process("test123", 5);
// result == true
```

#### Parameter Interaction

**Callbacks**

With `.Do`, you can register a callback for individual parameters of a method setup. This allows you to implement side
effects or checks directly when the method or indexer is called.

**Example: Do for method parameter**

```csharp
int lastAmount = 0;
sut.Mock.Setup.Dispense(It.Is("Dark"), It.IsAny<int>().Do(amount => lastAmount = amount));
sut.Dispense("Dark", 42);
// lastAmount == 42
```

**Monitor**

With `.Monitor(out monitor)`, you can track the actual
values passed during test execution and analyze them afterward.

**Example: Monitor for method parameter**

```csharp
Mockolate.ParameterMonitor<int> monitor;
sut.Mock.Setup.Dispense(It.Is("Dark"), It.IsAny<int>().Monitor(out monitor));
sut.Dispense("Dark", 5);
sut.Dispense("Dark", 7);
// monitor.Values == [5, 7]
```

## Mock events

Easily raise events on your mock to test event handlers in your code.

### Raise

Use the strongly-typed `Raise` property on your mock to trigger events declared on the mocked interface or class. The
method signature matches the event delegate.

```csharp
// Arrange: subscribe a handler to the event
sut.ChocolateDispensed += (type, amount) => { /* handler code */ };

// Act: raise the event
sut.Mock.Raise.ChocolateDispensed("Dark", 5);
```

- Use the `Raise` property to trigger events declared on the mocked interface or class.
- Only currently subscribed handlers will be invoked.
- Simulate notifications and test event-driven logic in your code.

**Example:**

```csharp
int dispensedAmount = 0;
sut.ChocolateDispensed += (type, amount) => dispensedAmount += amount;

sut.Mock.Raise.ChocolateDispensed("Dark", 3);
sut.Mock.Raise.ChocolateDispensed("Milk", 2);

// dispensedAmount == 5
```

You can subscribe and unsubscribe handlers as needed. Only handlers subscribed at the time of raising the event will be
called.

## Verify interactions

You can verify that methods, properties, indexers, or events were called or accessed with specific arguments and how
many times, using the `Verify` API:

Supported call count verifications (in the `Mockolate.Verify` namespace):

- `.Never()`: The interaction never occurred
- `.Once()`: The interaction occurred exactly once
- `.Twice()`: The interaction occurred exactly twice
- `.Exactly(n)`: The interaction occurred exactly n times
- `.AtLeastOnce()`: The interaction occurred at least once
- `.AtLeastTwice()`: The interaction occurred at least twice
- `.AtLeast(n)`: The interaction occurred at least n times
- `.AtMostOnce()`: The interaction occurred at most once
- `.AtMostTwice()`: The interaction occurred at most twice
- `.AtMost(n)`: The interaction occurred at most n times
- `.Between(min, max)`: The interaction occurred between min and max times (inclusive)
- `.Times(predicate)`: The interaction count matches the predicate

If the invocations run in a background thread, you can use `Within(TimeSpan)` to specify a timeout in which to wait for
the expected interactions to occur:

```csharp
// Wait up to 1 second for Dispense("Dark", 5) to be invoked
sut.Mock.Verify.Dispense(It.Is("Dark"), It.Is(5))
    .Within(TimeSpan.FromSeconds(1))
    .AtLeastOnce();
```

You can also use `WithCancellation(CancellationToken)` to wait for the expected interactions until the cancellation
token is canceled. If you combine this with the `Within` method, both the timeout and the cancellation token are
respected.

In both cases, it will block the test execution until the expected interaction occurs or the timeout is reached.
If the interaction does not occur within the specified time, a `MockVerificationException` will be thrown.

If you need truly asynchronous verification without blocking the test thread, you can use the
[aweXpect.Mockolate](https://awexpect.com/aweXpect.Mockolate) NuGet package, which integrates Mockolate's verification
API with [aweXpect](https://awexpect.com) and offers an awaitable `Within(TimeSpan)` variant.

### Properties

You can verify access to property getter and setter:

```csharp
// Verify that the property 'TotalDispensed' was read at least once
sut.Mock.Verify.TotalDispensed.Got().AtLeastOnce();

// Verify that the property 'TotalDispensed' was set to 42 exactly once
sut.Mock.Verify.TotalDispensed.Set(It.Is(42)).Once();
```

**Note:**  
The setter value also supports argument matchers.

### Methods

You can verify that methods were invoked with specific arguments and how many times:

```csharp
// Verify that Dispense("Dark", 5) was invoked at least once
sut.Mock.Verify.Dispense(It.Is("Dark"), It.Is(5))
    .AtLeastOnce();

// Verify that Dispense was never invoked with "White" and any amount
sut.Mock.Verify.Dispense(It.Is("White"), It.IsAny<int>())
    .Never();

// Verify that Dispense was invoked exactly twice with any type and any amount
sut.Mock.Verify.Dispense(Match.AnyParameters())
    .Exactly(2);

// Verify that Dispense was invoked between 3 and 5 times (inclusive)
sut.Mock.Verify.Dispense(It.IsAny<string>(), It.IsAny<int>())
    .Between(3, 5);

// Verify that Dispense was invoked an even number of times
sut.Mock.Verify.Dispense(It.IsAny<string>(), It.IsAny<int>())
    .Times(count => count % 2 == 0);
```

You can also verify that a specific setup was invoked a specific number of times:

```csharp
IMockSetup setup = sut.Mock.Setup.Dispense(It.Is("Dark"), It.Is(5)).Returns(true);
// Act
sut.Mock.VerifySetup(setup).AtLeastOnce();
```

### Indexers

You can verify access to indexer getter and setter:

```csharp
// Verify that the indexer was read with key "Dark" exactly once
sut.Mock.Verify[It.Is("Dark")].Got().Once();

// Verify that the indexer was set with key "Milk" to value 7 at least once
sut.Mock.Verify[It.Is("Milk")].Set(7).AtLeastOnce();
```

**Note:**  
The keys and value also supports argument matchers.

### Events

You can verify event subscriptions and unsubscriptions:

```csharp
// Verify that the event 'ChocolateDispensed' was subscribed to at least once
sut.Mock.Verify.ChocolateDispensed.Subscribed().AtLeastOnce();

// Verify that the event 'ChocolateDispensed' was unsubscribed from exactly once
sut.Mock.Verify.ChocolateDispensed.Unsubscribed().Once();
```

### Argument Matchers

You can use argument matchers from the `It` class to verify calls with flexible conditions:

- `It.IsAny<T>()`: Matches any value of type `T`.
- `It.Is<T>(value)`: Matches a specific value. With `.Using(IEqualityComparer<T>)`, you can provide a custom equality
  comparer.
- `It.IsNot<T>(value)`: Matches any value not equal to `value`.
- `It.IsOneOf<T>(params IEnumerable<T> values)`: Matches any of the given values. With `.Using(IEqualityComparer<T>)`,
  you can provide a custom equality comparer.
- `It.IsNotOneOf<T>(params IEnumerable<T> values)`: Matches any value that is not in the given set.
- `It.IsNull<T>()`: Matches null.
- `It.IsNotNull<T>()`: Matches any non-null value.
- `It.IsTrue()`/`It.IsFalse()`: Matches boolean true/false.
- `It.IsInRange(min, max)`: Matches a number within the given range. You can append `.Exclusive()` to exclude the
  minimum and maximum value.
- `It.IsOut<T>()`: Matches any out parameter of type `T`.
- `It.IsRef<T>()`: Matches any ref parameter of type `T`.
- `It.IsSpan<T>(predicate)` / `It.IsAnySpan<T>()`: Matches `Span<T>` parameters (.NET 8+).
- `It.IsReadOnlySpan<T>(predicate)` / `It.IsAnyReadOnlySpan<T>()`: Matches `ReadOnlySpan<T>` parameters (.NET 8+).
- `It.Matches<string>(pattern)`: Matches strings using wildcard patterns (`*` and `?`). With `.AsRegex()`, you can use
  regular expressions instead.
- `It.Contains<T>(item)`: Matches a collection parameter that contains `item`. With `.Using(IEqualityComparer<T>)`, you
  can provide a custom equality comparer.
- `It.SequenceEquals<T>(params IEnumerable<T> values)`: Matches a collection parameter whose elements equal `values` in
  order. With `.Using(IEqualityComparer<T>)`, you can provide a custom equality comparer.
- `It.Satisfies<T>(predicate)`: Matches values based on a predicate.

*Note:* Custom `ref struct` matchers (`It.IsRefStruct<T>`, `It.IsAnyRefStruct<T>`, `It.IsRefStructBy<T,TKey>`) only
apply at setup time - `Verify` counts calls to ref-struct members but cannot match on the value after the fact, since
the ref-struct value isn't retained past the call.

**Example:**

```csharp
sut.Mock.Verify.Dispense(It.Is<string>(t => t.StartsWith("D")), It.IsAny<int>()).Once();
sut.Mock.Verify.Dispense(It.Is("Milk"), It.IsAny<int>()).AtLeastOnce();
```

### Call Ordering

Use `Then` to verify that calls occurred in a specific order:

```csharp
sut.Mock.Verify.Dispense(It.Is("Dark"), It.Is(2)).Then(
    m => m.Dispense(It.Is("Dark"), It.Is(3))
);
```

You can chain multiple calls for strict order verification:

```csharp
sut.Mock.Verify.Dispense(It.Is("Dark"), It.Is(1)).Then(
    m => m.Dispense(It.Is("Milk"), It.Is(2)),
    m => m.Dispense(It.Is("White"), It.Is(3)));
```

If the order is incorrect or a call is missing, a `MockVerificationException` will be thrown with a descriptive message.

## Advanced Features

### Working with protected members

Mockolate allows you to set up and verify protected virtual members on class mocks.

Protected members can be set up, raised, and verified just like instance members, but through the `Mock.SetupProtected`,
`Mock.RaiseProtected`, and `Mock.VerifyProtected` properties:

**Example**

```csharp
public abstract class ChocolateDispenser
{
    protected virtual bool DispenseInternal(string type, int amount) => true;
    protected virtual int InternalStock { get; set; }
}

ChocolateDispenser sut = ChocolateDispenser.CreateMock();
```

#### Setup

```csharp
// Setup protected method
sut.Mock.SetupProtected.DispenseInternal(
    It.Is("Dark"), It.IsAny<int>())
    .Returns(true);

// Setup protected property
sut.Mock.SetupProtected.InternalStock.InitializeWith(100);
```

**Notes:**

- Protected members can be set up and verified just like public members.
- All setup options (`.Returns()`, `.Throws()`, `.Do()`, `.InitializeWith()`, etc.) work with protected members.

#### Verification

```csharp
// Verify protected method was invoked
sut.Mock.VerifyProtected.DispenseInternal(
    It.Is("Dark"), It.IsAny<int>()).Once();

// Verify protected property was read
sut.Mock.VerifyProtected.InternalStock.Got().AtLeastOnce();

// Verify protected property was set
sut.Mock.VerifyProtected.InternalStock.Set(It.Is(100)).Once();

// Verify protected indexer was read
sut.Mock.VerifyProtected[It.Is(0)].Got().Once();

// Verify protected indexer was set
sut.Mock.VerifyProtected[It.Is(0)].Set(It.Is(42)).Once();
```

**Note:**

- All verification options (argument matchers, count assertions) work the same for protected members as for public
  members.

### Advanced callback features

#### Conditional callbacks (`When`)

Execute callbacks conditionally based on the zero-based invocation counter using `.When()`:

```csharp
sut.Mock.Setup.Dispense(It.Is("Dark"), It.IsAny<int>())
    .Do(() => Console.WriteLine("Called!")).When(count => count >= 2);  // The first two calls are skipped
```

#### Limit invocations (`Only`)

Control after how many times a callback should no longer be executed:

```csharp
// Execute up to 3 times
sut.Mock.Setup.Dispense(It.IsAny<string>(), It.IsAny<int>())
    .Do(() => Console.WriteLine("Up to 3 times")).Only(3);

// Executes the callback only once
sut.Mock.Setup.TotalDispensed
    .Throws(new Exception("This exception is thrown only once")).OnlyOnce();
```

#### Repeat invocations (`For`)

Control how many times a callback should be repeated:

```csharp
sut.Mock.Setup.Dispense(It.IsAny<string>(), It.IsAny<int>())
    .Do(() => Console.WriteLine("First three times")).For(3)
    .Do(() => Console.WriteLine("Next three times")).For(3);

sut.Mock.Setup.TotalDispensed
    .Returns(10).For(1)
    .Returns(20).For(2)
    .Returns(30).For(3);
// Reads: 10, 20, 20, 30, 30, 30, 0, 0, 0, 0 …
```

**Repeat `Forever`**

If you have a sequence of callbacks, you can mark the last one to repeat indefinitely using `.Forever()` to avoid
repeating the sequence from start:

```csharp
sut.Mock.Setup.Dispense(It.IsAny<string>(), It.IsAny<int>())
    .Returns(true).For(2)      // Returns true the first two times
    .Returns(false).Forever(); // Then always returns false
```

#### Parallel callbacks

When you specify multiple callbacks, they are executed sequentially by default. You can change this behavior to always
run specific callbacks in parallel using `.InParallel()`:

```csharp
sut.Mock.Setup.Dispense(It.IsAny<string>(), It.IsAny<int>())
    .Do(() => { Console.WriteLine("Runs every second iteration"); })
    .Do(() => { Console.WriteLine("Runs always in parallel"); }).InParallel()
    .Do(() => { Console.WriteLine("Runs every other iteration"); });
```

**Note:**
Parallel execution via `.InParallel()` only applies to callbacks defined via `Do`, not to other setup callbacks like
`Returns` or `Throws`.

#### Invocation counter

Access the zero-based invocation counter in callbacks:

```csharp
sut.Mock.Setup.Dispense(It.IsAny<string>(), It.IsAny<int>())
    .Do((count, _, _) => Console.WriteLine($"Call #{count}"));

sut.Mock.Setup.TotalDispensed.OnGet
    .Do((count, value) => Console.WriteLine($"Read #{count}, value: {value}"));

// Indexer setter - count, then the indexer key(s), then the new value
sut.Mock.Setup[It.IsAny<string>()].OnSet
    .Do((count, type, newValue) =>
        Console.WriteLine($"Set #{count}: this[{type}] = {newValue}"));
```

### Monitor interactions

Mockolate tracks all interactions with mocks on the mock object. To only track interactions within a given scope, you
can use a `MockMonitor<T>`:

```csharp
var sut = IChocolateDispenser.CreateMock();
var monitor = sut.Mock.Monitor();

sut.Dispense("Dark", 1); // Not monitored
using (monitor.Run())
{
    sut.Dispense("Dark", 2); // Monitored
}
sut.Dispense("Dark", 3); // Not monitored

// Verifications on the monitor only count interactions during the lifetime scope of the `IDisposable`
monitor.Verify.Dispense(It.Is("Dark"), It.IsAny<int>()).Once();
```

#### Clear all interactions

For simpler scenarios you can directly clear all recorded interactions on a mock using `ClearAllInteractions` on the
setup:

```csharp
IChocolateDispenser sut = IChocolateDispenser.CreateMock();

sut.Dispense("Dark", 1);
// Clears all previously recorded interactions
sut.Mock.ClearAllInteractions();
sut.Dispense("Dark", 2);

sut.Mock.Verify.Dispense(It.Is("Dark"), It.IsAny<int>()).Once();
```

### Check for unexpected interactions

#### That all interactions are verified

You can check if all interactions with the mock have been verified using `VerifyThatAllInteractionsAreVerified`:

```csharp
// Returns true if all interactions have been verified before
bool allVerified = sut.Mock.VerifyThatAllInteractionsAreVerified();
```

This is useful for ensuring that your test covers all interactions and that no unexpected calls were made.
If any interaction was not verified, this method returns `false`.

#### That all setups are used

You can check if all registered setups on the mock have been used with `VerifyThatAllSetupsAreUsed`:

```csharp
// Returns true if all setups have been used
bool allUsed = sut.Mock.VerifyThatAllSetupsAreUsed();
```

This is useful for ensuring that your test setup and test execution match.
If any setup was not used, this method returns `false`.

### Static interface members (.NET 8+)

Mockolate supports mocking static abstract and static virtual members on interfaces (.NET 8+). Static member
invocations use async-flow scoping, meaning each mock instance has its own isolated static member context,
this makes parallel test execution safe.

Static members can be set up, raised, and verified just like instance members, but through the `Mock.SetupStatic`,
`Mock.RaiseStatic`, and `Mock.VerifyStatic` properties:

```csharp
// Setup static members
sut.Mock.SetupStatic.AbstractStaticMethod().Returns("some-value");
sut.Mock.SetupStatic.AbstractStaticProperty.Returns("some-value");

// Raise static events
sut.Mock.RaiseStatic.AbstractStaticEvent(value);

// Verify static interactions
sut.Mock.VerifyStatic.AbstractStaticMethod().Once();
sut.Mock.VerifyStatic.AbstractStaticProperty.Got().Once();
sut.Mock.VerifyStatic.AbstractStaticEvent.Subscribed().Once();
```

**Notes:**

- Static member scoping is implemented via `AsyncLocal<MockRegistry>`. When you call
  `sut.Mock.SetupStatic.Method()`, it creates an async-flow scope that routes static member invocations to that
  specific mock instance.
- Each mock instance has an independent static member context, so parallel tests will not interfere with each other.

### Scenarios

Scenarios let you define multiple sets of setups on a single mock and switch between them at runtime. This is useful
when the collaborator behaves differently depending on its internal state &mdash; for example a connection that starts
disconnected, becomes connected after `Connect()`, and times out after a failure.

A mock always has an *active scenario* (the empty string `""` by default). When a member is accessed, Mockolate looks
for a matching setup in the active scenario first, then falls back to the default scope.

#### Defining scenarios

Use `InScenario(name)` to scope setups to a named scenario. It returns a scope whose `Setup` property mirrors
`sut.Mock.Setup` but targets the scenario's bucket:

```csharp
sut.Mock.InScenario("disconnected").Setup.Ping().Throws<TimeoutException>();
sut.Mock.InScenario("connected").Setup.Ping().Returns(true);
```

A callback overload batches multiple setups into the same scenario:

```csharp
sut.Mock.InScenario("connected", scope =>
{
    scope.Setup.Ping().Returns(true);
    scope.Setup.Send(It.IsAny<string>()).Returns(true);
});
```

Setups registered via `InScenario` do **not** leak into the default scope.

#### Switching scenarios

Chain `.TransitionTo(name)` on any method, property, indexer, or event subscription/unsubscription setup to change
the active scenario when the setup fires. The transition runs as a parallel side-effect &mdash; it does not replace
the return value or throw behaviour.

```csharp
sut.Mock.InScenario("disconnected")
    .Setup.Connect()
    .Returns(true)
    .TransitionTo("connected");

sut.Mock.InScenario("connected")
    .Setup.Ping()
    .Throws<TimeoutException>()
    .TransitionTo("disconnected");
```

You can also change the active scenario manually via `sut.Mock.TransitionTo("connected");`, which is useful for
arranging the starting state.

**Resolution rules**

When dispatching a call, Mockolate looks up setups in this order:

1. The active scenario's bucket (if non-empty).
2. The default bucket (setups registered via `sut.Mock.Setup.*`).
3. The mock's default behaviour.

Scenario setups add to, rather than replace, the default scope &mdash; register catch-alls in the default scope and
override specific members per scenario.

## Special Types

### HttpClient

Mockolate supports mocking `HttpClient` out of the box, with no special configuration required. You can set up, use, and
verify HTTP interactions just like with any other interface or class.

**Example: Mocking HttpClient for a Chocolate Dispenser Service**

```csharp
HttpClient httpClient = HttpClient.CreateMock();
httpClient.Mock.Setup
    .PostAsync(
        It.IsAny<string>(),
        It.IsHttpContent())
    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

HttpResponseMessage result = await httpClient.PostAsync("https://aweXpect.com/api/chocolate/dispense",
    new StringContent("""
                      { "type": "Dark", "amount": 3 }
                      """, Encoding.UTF8, "application/json"));

await That(result.IsSuccessStatusCode).IsTrue();
httpClient.Mock.Verify.PostAsync(
    It.IsUri("*aweXpect.com/api/chocolate/dispense*").ForHttps(),
    It.IsHttpContent("application/json").WithStringMatching("*\"type\": \"Dark\"*\"amount\": 3*")).Once();
```

**Notes:**

- The custom extensions for the `HttpClient` are in the `Mockolate.Web` namespace.
- Under the hood, the setups, requests and verifications are forwarded to a mocked `HttpMessageHandler`.
  As they therefore all forward to the `SendAsync` method, you can mix using a string or an `Uri` parameter in setup or
  verification.

#### All HTTP Methods

Mockolate supports all standard HTTP methods:

```csharp
// GET
httpClient.Mock.Setup
    .GetAsync(It.IsAny<string>())
    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NoContent));

// POST
httpClient.Mock.Setup
    .PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>())
    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

// PUT
httpClient.Mock.Setup
    .PutAsync(It.IsAny<string>(), It.IsAny<HttpContent>())
    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

// DELETE
httpClient.Mock.Setup
    .DeleteAsync(It.IsAny<string>())
    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NoContent));

// PATCH
httpClient.Mock.Setup
    .PatchAsync(It.IsAny<string>(), It.IsAny<HttpContent>())
    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
```

For all HTTP methods you can add an optional cancellation token parameter.
If no parameter is provided, it matches any `CancellationToken`:

```csharp
var cts = new CancellationTokenSource();
httpClient.Mock.Setup
    .GetAsync(It.IsAny<string>(), It.Is(cts.Token))
    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

await httpClient.GetAsync("https://example.com", cts.Token);
```

#### URI matching

Use `It.IsUri(string?)` to match URIs using a wildcard pattern against the string representation of the URI.
The pattern supports `*` to match zero or more characters and `?` to match a single character.

**Scheme**

Filter requests by URI scheme using `.ForHttps()` or `.ForHttp()`:

```csharp
// Match only HTTPS requests
httpClient.Mock.Verify
    .GetAsync(It.IsUri("*aweXpect.com*").ForHttps())
    .Once();

// Match only HTTP requests
httpClient.Mock.Verify
    .GetAsync(It.IsUri("*aweXpect.com*").ForHttp())
    .Never();
```

**Host**

Filter requests by host using `.WithHost(string)`. You can provide a wildcard pattern to match against the host name:

```csharp
httpClient.Mock.Verify
    .GetAsync(It.IsUri().WithHost("*aweXpect.com*"))
    .Once();
```

**Port**

Filter requests on a specific port using `.WithPort(int)`:

```csharp
httpClient.Mock.Verify
    .GetAsync(It.IsUri().WithPort(443))
    .Once();
```

**Path**

Filter requests by path using `.WithPath(string)`. You can provide a wildcard pattern to match against the path:

```csharp
httpClient.Mock.Verify
    .GetAsync(It.IsUri().WithPath("/foo/*"))
    .Once();
```

**Query**

Filter requests by query parameters using `.WithQuery(...)`. You can provide one or many key-value pairs or a raw query
string to match against the query parameters. The order of the key-value pairs does not matter:

```csharp
// Match query string containing "x=42"
httpClient.Mock.Verify
    .GetAsync(It.IsUri().WithQuery("x", "42"))
    .Once();
// Match query string containing "x=42" and "y=foo" (in any order)
httpClient.Mock.Verify
    .GetAsync(It.IsUri().WithQuery(("x", "42"), ("y", "foo")))
    .Once();
// Match query string containing "x=42" and "y=foo" (in any order)
httpClient.Mock.Verify
    .GetAsync(It.IsUri().WithQuery("x=42&y=foo"))
    .Once();
```

#### Content Matching

Use `It.IsHttpContent(string?)` to match the HTTP content, optionally providing an expected media type header value:

```csharp
httpClient.Mock.Setup
    .PostAsync(
        It.IsAny<string>(),
        It.IsHttpContent("application/json"))
    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
```

**String content**

To verify against the string content, use the following methods:

- `.WithString(Func<string, bool>)`: to match the string content against the given predicate
- `.WithString(string)`: to match the content exactly as provided
- `.WithStringMatching(string)`: to match the content using wildcard patterns
- `.WithStringMatching(string).AsRegex()`: to match the content using regular expressions

```csharp
httpClient.Mock.Setup
    .PostAsync(
        It.IsAny<string>(),
        It.IsHttpContent("application/json").WithStringMatching("*\"type\": \"Dark\"*"))
    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
```

*Notes:*

- Add the `.IgnoringCase()` modifier to make the string matching case-insensitive.

**Binary content**

To verify against the binary content, use the following methods:

- `.WithBytes(Func<byte[], bool>)`: to match the binary content against the given predicate
- `.WithBytes(byte[])`: to match the content exactly as provided

```csharp
httpClient.Mock.Setup
    .PostAsync(
        It.IsAny<string>(),
        It.IsHttpContent("application/octet-stream").WithBytes([0x01, 0x02, 0x03, ]))
    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
```

**Form data content**

To verify against the URL-encoded form data content, use the following methods:

- `.WithFormData(string, string)`: checks that the form-data content contains the provided key-value pair
- `.WithFormData(IEnumerable<(string, string)>)`: checks that the form-data content contains the provided key-value
  pairs
- `.WithFormData(string)`: checks that the form-data content contains the provided raw form data string

```csharp
httpClient.Mock.Setup
    .PostAsync(
        It.IsAny<string>(),
        It.IsHttpContent("application/x-www-form-urlencoded").WithFormData("my-key", "my-value"))
    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
```

*Notes:*

- Similar to the query parameter matching, the order of the form-data key-value pairs does not matter.
- Add the `.Exactly()` modifier to also check that no other form-data is present.

**Header matching**

To verify against the HTTP content headers, use the following methods:

- `.WithHeaders(string, HttpHeaderValue)`: checks that the content headers contain the provided key-value pair
- `.WithHeaders(IEnumerable<(string, HttpHeaderValue)>)`: checks that the content headers contain all key-value pairs
- `.WithHeaders(string)`: checks that the content headers contain the provided raw headers

```csharp
httpClient.Mock.Setup
    .PostAsync(
        It.IsAny<string>(),
        It.IsHttpContent().WithHeaders(("Content-Type", "application/json"), ("X-My-Header", "my-value")))
    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
httpClient.Mock.Setup
    .PostAsync(
        It.IsAny<string>(),
        It.IsHttpContent().WithHeaders("""
        		                           Content-Type: application/json
        		                           X-My-Header: my-value
        		                           """))
    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
```

*Notes:*

- By default, only the content headers are checked, not the headers in the corresponding `HttpRequestMessage`.
  If you want to check both, add the `.IncludingRequestHeaders()` modifier.

#### Return Message

Overloads of `.ReturnsAsync` simplify specifying the return value for HTTP method setups.

```csharp
httpClient.Mock.Setup.GetAsync(It.IsAny<Uri>())
    // Returns a response with status code 200 OK and no content
    .ReturnsAsync(HttpStatusCode.OK);

httpClient.Mock.Setup.GetAsync(It.IsAny<Uri>())
    // Returns a response with status code 200 OK and a string content "some string content"
    .ReturnsAsync(HttpStatusCode.OK, "some string content");

httpClient.Mock.Setup.GetAsync(It.IsAny<Uri>())
    // Returns a response with status code 200 OK and a JSON content {"foo":"bar"}
    .ReturnsAsync(HttpStatusCode.OK, "{\"foo\":\"bar\"}", "application/json");

byte[] bytes = new byte[] { /* ... */ };

httpClient.Mock.Setup.GetAsync(It.IsAny<Uri>())
    // Returns a response with status code 200 OK and a binary content with the provided bytes
    .ReturnsAsync(HttpStatusCode.OK, bytes);

httpClient.Mock.Setup.GetAsync(It.IsAny<Uri>())
    // Returns a response with status code 200 OK and a PNG image content with the provided bytes
    .ReturnsAsync(HttpStatusCode.OK, bytes, "image/png");
```

#### Whole-request matching

Use `It.IsHttpRequestMessage(HttpMethod?)` together with `Setup.SendAsync(...)` to match against the entire
`HttpRequestMessage`, optionally restricted to a specific HTTP method. This is the catch-all entry point that the
verb-specific overloads (`GetAsync`, `PostAsync`, …) route through internally - reach for it when you need the full
request (e.g. to inspect headers, or to handle non-standard verbs like `HEAD` or `OPTIONS`).

Chain builders to add constraints:

- `.WhoseUriIs(string)` / `.WhoseUriIs(Action<IUriParameter>)`: same surface as `It.IsUri(...)`.
- `.WhoseContentIs(Action<IHttpContentParameter>)` / `.WhoseContentIs(string mediaType, …)`: same surface as
  `It.IsHttpContent(...)`.
- `.WithHeaders(...)`: require specific request headers.

```csharp
// Match any POST to /api/chocolate/dispense with a JSON body containing "Dark"
httpClient.Mock.Setup
    .SendAsync(It.IsHttpRequestMessage(HttpMethod.Post)
        .WhoseUriIs(uri => uri.WithPath("/api/chocolate/dispense"))
        .WhoseContentIs("application/json", c => c.WithStringMatching("*\"type\": \"Dark\"*")))
    .ReturnsAsync(HttpStatusCode.OK);
```

### Delegates

Mockolate supports mocking delegates including `Action`, `Func<T>`, and custom delegates.

**Setup**

Use `sut.Mock.Setup(…)` to configure delegate behavior.

```csharp
// Mock Action delegate
Action myAction = Action.CreateMock();
myAction.Mock.Setup().Do(() => Console.WriteLine("Action invoked!"));

// Mock Func<T> delegate
Func<int> myFunc = Func<int>.CreateMock();
myFunc.Mock.Setup().Returns(42);
```

For custom delegates with parameters:

```csharp
// Define a custom delegate (typically declared at type level)
public delegate int Calculate(int x, string operation);

// Create and setup the mock
Calculate calculator = Calculate.CreateMock();
calculator.Mock.Setup(It.IsAny<int>(), It.Is("add"))
    .Returns((x, operation) => x + 10);
```

Delegates with `ref` and `out` parameters are also supported:

```csharp
// Define a custom delegate (typically declared at type level)
public delegate void ProcessData(int input, ref int value, out int result);

// Create and setup the mock
ProcessData processor = ProcessData.CreateMock();
processor.Mock.Setup(It.IsAny<int>(), It.IsRef<int>(v => v + 1), It.IsOut(() => 100));
```

- Use `.Do(…)` to run code when the delegate is invoked.
- Use `.Returns(…)` to specify the return value for `Func<T>` delegates.
- Use `.Throws(…)` to specify an exception to throw.
- Use `.Returns(…)` and `.Throws(…)` repeatedly to define a sequence of behaviors.
- Full [parameter matching](#parameter-matching) support for delegate
  parameters including `ref` and `out` parameters.

**Verification**

You can verify that delegates were invoked with specific arguments:

```csharp
// Verify Action was invoked at least once
Action myAction = Action.CreateMock();
myAction.Invoke();
myAction.Mock.Verify().AtLeastOnce();

// Verify Func<T> was invoked exactly once
Func<int> myFunc = Func<int>.CreateMock();
_ = myFunc();
myFunc.Mock.Verify().Once();
```

For custom delegates with parameters:

```csharp
// Define a custom delegate (typically declared at type level)
public delegate int Calculate(int x, string operation);

// Create, invoke, and verify the mock
Calculate calculator = Calculate.CreateMock();
_ = calculator(5, "add");
calculator.Mock.Verify(It.IsAny<int>(), It.Is("add")).Once();
```

Delegates with `ref` and `out` parameters are also supported:

```csharp
// Define a custom delegate (typically declared at type level)
public delegate void ProcessData(int input, ref int value, out int result);

// Create, invoke, and verify the mock
ProcessData processor = ProcessData.CreateMock();
int val = 0;
processor(1, ref val, out int res);
processor.Mock.Verify(It.IsAny<int>(), It.IsRef<int>(), It.IsOut<int>()).Once();
```

**Note:**  
Delegate parameters also support [argument matchers](#argument-matchers).

## A complete example

The following example combines properties, indexers, events, methods, stateful setup, and verification
into a single end-to-end scenario:

```csharp
using Mockolate;

public delegate void ChocolateDispensedDelegate(string type, int amount);
public interface IChocolateDispenser
{
    int this[string type] { get; set; }
    int TotalDispensed { get; set; }
    bool Dispense(string type, int amount);
    event ChocolateDispensedDelegate ChocolateDispensed;
}

// Create a mock of IChocolateDispenser
IChocolateDispenser sut = IChocolateDispenser.CreateMock();

// Setup: Initial stock of 10 for Dark chocolate
sut.Mock.Setup["Dark"].InitializeWith(10);
// Setup: Dispense decreases Dark chocolate if enough, returns true/false
sut.Mock.Setup.Dispense("Dark", It.IsAny<int>())
    .Returns((type, amount) =>
    {
        int current = sut[type];
        if (current >= amount)
        {
            sut[type] = current - amount;
            sut.Mock.Raise.ChocolateDispensed(type, amount);
            return true;
        }
        return false;
    });

// Track dispensed amount via event
int dispensedAmount = 0;
sut.ChocolateDispensed += (type, amount) =>
{
    dispensedAmount += amount;
};

// Act: Try to dispense chocolates
bool gotChoc1 = sut.Dispense("Dark", 4); // true
bool gotChoc2 = sut.Dispense("Dark", 5); // true
bool gotChoc3 = sut.Dispense("Dark", 6); // false

// Verify: Check interactions
sut.Mock.Verify.Dispense("Dark", It.IsAny<int>()).Exactly(3);
```

## Analyzers

Mockolate ships with some Roslyn analyzers to help you adopt best practices and catch issues early, at compile time.
All rules provide actionable messages and link to identifiers for easy filtering.

### Mockolate0001

`Verify` methods only return a `VerificationResult` and do not directly throw. You have to specify how often you expect
the call to happen, e.g. `.AtLeastOnce()`, `.Exactly(n)`, etc. or use the verification result in any other way.

**Example:**

```csharp
IChocolateDispenser sut = IChocolateDispenser.CreateMock();
sut.Dispense("Dark", 1);
// Analyzer Mockolate0001: Add a count assertion like .AtLeastOnce() or use the result.
sut.Mock.Verify.Dispense(It.Is("Dark"), It.IsAny<int>());
```

The included code fixer suggests to add the `.AtLeastOnce()` count assertion:

```csharp
sut.Mock.Verify.Dispense(It.Is("Dark"), It.IsAny<int>()).AtLeastOnce();
```

### Mockolate0002

Mocked types must be mockable. This rule will prevent you from using unsupported types:

- `CreateMock()`  
  Type must be an interface, a delegate or a supported class (e.g. not sealed)
- `Implementing<T>()`  
  Type must be an interface

### Mockolate0003

A mocked member's signature routes through the ref-struct pipeline in a way Mockolate can't
emit setup surface for. The warning fires in two distinct situations.

**1. Compilation prerequisites not met**

The ref-struct setup pipeline requires both:

- A target framework of .NET 9 or later (Mockolate's ref-struct setup types are
  `#if NET9_0_OR_GREATER`-gated).
- An effective C# language version of 13 or later (uses the `allows ref struct` anti-constraint).

When either prerequisite is missing, the warning fires for any member that passes a non-`Span<T>` /
non-`ReadOnlySpan<T>` ref struct by value, or uses one as an indexer key. Upgrade the target
framework and/or `<LangVersion>` to resolve it.

**2. Signature shapes that are never supported**

These fire on every compilation target, including .NET 9+ / C# 13+:

- Parameters marked `out`, `ref`, or `ref readonly` whose type is a non-`Span<T>` /
  non-`ReadOnlySpan<T>` ref struct - the mock can't round-trip the value through
  `IOutParameter<T>` / `IRefParameter<T>` when `T` is a ref struct.
- Methods returning a non-`Span<T>` / non-`ReadOnlySpan<T>` ref struct.

**Note:**
`Span<T>` and `ReadOnlySpan<T>` flow through the existing `SpanWrapper` / `ReadOnlySpanWrapper`
fallback and are never flagged. On .NET 9+ with C# 13+, by-value custom ref-struct parameters and
ref-struct-keyed indexers (getter-only, setter-only, and get+set) are fully supported.

See the Ref Struct Parameters section for the supported surface.
