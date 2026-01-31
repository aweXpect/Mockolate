# Mockolate

<img align="right" width="200" src="Docs/logo_256x256.png" alt="Mockolate logo" />

[![Nuget](https://img.shields.io/nuget/v/Mockolate)](https://www.nuget.org/packages/Mockolate)
[![Build](https://github.com/aweXpect/Mockolate/actions/workflows/build.yml/badge.svg)](https://github.com/aweXpect/Mockolate/actions/workflows/build.yml)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=aweXpect_Mockolate&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=aweXpect_Mockolate)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=aweXpect_Mockolate&metric=coverage)](https://sonarcloud.io/summary/overall?id=aweXpect_Mockolate)
[![Mutation testing badge](https://img.shields.io/endpoint?style=flat&url=https%3A%2F%2Fbadge-api.stryker-mutator.io%2Fgithub.com%2FaweXpect%2FMockolate%2Fmain)](https://dashboard.stryker-mutator.io/reports/github.com/aweXpect/Mockolate/main)

**Mockolate** is a modern, strongly-typed, AOT-compatible mocking library for .NET, powered by source generators. It
enables fast,
compile-time validated mocks for interfaces and classes, supporting .NET Standard 2.0, .NET 8, .NET 10, and .NET
Framework 4.8.

- **Source generator-based**: No runtime proxy generation, fast and reliable.
- **Strongly-typed**: Setup and verify mocks with full IntelliSense and compile-time safety.
- **AOT compatible**: Works with NativeAOT and trimming.

## Getting Started

1. Install the [`Mockolate`](https://www.nuget.org/packages/Mockolate) nuget package
   ```ps
   dotnet add package Mockolate
   ```

2. Create and use the mock
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
   
   // Create a mock for IChocolateDispenser
   var sut = Mock.Create<IChocolateDispenser>();
   
   // Setup: Initial stock of 10 for Dark chocolate
   sut.SetupMock.Indexer(It.Is("Dark")).InitializeWith(10);
   // Setup: Dispense decreases Dark chocolate if enough, returns true/false
   sut.SetupMock.Method.Dispense(It.Is("Dark"), It.IsAny<int>())
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
   
   // Track dispensed amount via event
   int dispensedAmount = 0;
   sut.ChocolateDispensed += (type, amount)
   {
       dispensedAmount += amount;
   }
   
   // Act: Try to dispense chocolates
   bool gotChoc1 = sut.Dispense("Dark", 4); // true
   bool gotChoc2 = sut.Dispense("Dark", 5); // true
   bool gotChoc3 = sut.Dispense("Dark", 6); // false
   
   // Verify: Check interactions
   sut.VerifyMock.Invoked.Dispense(It.Is("Dark"), It.IsAny<int>()).Exactly(3);
   
   // Output: "Dispensed amount: 9. Got chocolate? True, True, False"
   Console.WriteLine($"Dispensed amount: {dispensedAmount}. Got chocolate? {gotChoc1}, {gotChoc2}, {gotChoc3}");
   ```

## Create mocks

You can create mocks for interfaces and classes. For classes without a default constructor, use
`BaseClass.WithConstructorParameters(…)` to provide constructor arguments:

```csharp
// Create a mock for an interface
var sut = Mock.Create<IChocolateDispenser>();

// Create a mock for a class
var classMock = Mock.Create<MyChocolateDispenser>();

// For classes without a default constructor:
var classWithArgsMock = Mock.Create<MyChocolateDispenserWithCtor>(
    BaseClass.WithConstructorParameters("Dark", 42)
);

// Specify up to 8 additional interfaces for the mock:
var sut2 = Mock.Create<MyChocolateDispenser, ILemonadeDispenser>();
```

**Notes:**

- Only the first generic type can be a class; additional types must be interfaces.
- Sealed classes cannot be mocked and will throw a `MockException`.

### Customizing mock behavior

You can control the default behavior of the mock by providing a `MockBehavior`:

```csharp
var strictMock = Mock.Create<IChocolateDispenser>(new MockBehavior { ThrowWhenNotSetup = true });

// For classes with constructor parameters and custom behavior:
var classMock = Mock.Create<MyChocolateDispenser>(
    BaseClass.WithConstructorParameters("Dark", 42),
    new MockBehavior { ThrowWhenNotSetup = true }
);
```

**`MockBehavior` options**

- `ThrowWhenNotSetup` (bool):
	- If `false` (default), the mock will return a default value (see `DefaultValue`).
	- If `true`, the mock will throw an exception when a method or property is called without a setup.
- `SkipBaseClass` (bool):
	- If `false` (default), the mock will call the base class implementation and use its return values as default
	  values, if no explicit setup is defined.
	- If `true`, the mock will not call any base class implementations.
- `Initialize<T>(params Action<IMockSetup<T>>[] setups)`:
	- Automatically initialize all mocks of type T with the given setups when they are created.
	- The callback can optionally receive an additional counter parameter which allows to differentiate between multiple
	  instances. This is useful when you want to ensure that you can distinguish between different automatically created
	  instances.
- `DefaultValue` (IDefaultValueGenerator):
	- Customizes how default values are generated for methods/properties that are not set up.
	- The default implementation provides sensible defaults for the most common use cases:
		- Empty collections for collection types (e.g., `IEnumerable<T>`, `List<T>`, etc.)
		- Empty string for `string`
		- Completed tasks for `Task`, `Task<T>`, `ValueTask` and `ValueTask<T>`
		- Tuples with recursively defaulted values
		- `null` for other reference types
	- You can provide custom default values for specific types using `.WithDefaultValueFor<T>()`:
	  ```csharp
	  var behavior = MockBehavior.Default
	      .WithDefaultValueFor<string>(() => "default")
	      .WithDefaultValueFor<int>(() => 42);
	  var sut = Mock.Create<IChocolateDispenser>(behavior);
	  ```
	  This is useful when you want mocks to return specific default values for certain types instead of the standard
	  defaults (e.g., `null`, `0`, empty strings).
- `.UseConstructorParametersFor<T>(object?[])`:
	- Configures constructor parameters to use when creating mocks of type `T`, unless explicit parameters are provided
	  during mock creation via `BaseClass.WithConstructorParameters(…)`.

### Using a factory for shared behavior

Use `Mock.Factory` to create multiple mocks with a shared behavior:

```csharp
var behavior = new MockBehavior { ThrowWhenNotSetup = true };
var factory = new Mock.Factory(behavior);

var sut1 = factory.Create<IChocolateDispenser>();
var sut2 = factory.Create<ILemonadeDispenser>();
```

Using a factory allows you to create multiple mocks with identical, centrally configured behavior. This is especially
useful when you need consistent mock setups across multiple tests or for different types.

### Wrapping existing instances

You can wrap an existing instance with mock tracking using `Mock.Wrap<T>()`. This allows you to track interactions with
a real object:

```csharp
var realDispenser = new ChocolateDispenser();
var wrappedDispenser = Mock.Wrap<IChocolateDispenser>(realDispenser);

// Calls are forwarded to the real instance
wrappedDispenser.Dispense("Dark", 5);

// But you can still verify interactions
wrappedDispenser.VerifyMock.Invoked.Dispense(It.Is("Dark"), It.Is(5)).Once();
```

**Notes:**

- Only interface types can be wrapped with `Mock.Wrap<T>()`.
- All calls are forwarded to the wrapped instance.
- You can still set up custom behavior that overrides the wrapped instance's behavior.
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
sut.SetupMock.Property.TotalDispensed.InitializeWith(42);
```

**Returns / Throws**

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

**Callbacks**

Register callbacks on the setter or getter:

```csharp
sut.SetupMock.Property.TotalDispensed.OnGet
    .Do(() => Console.WriteLine("TotalDispensed was read!"));
sut.SetupMock.Property.TotalDispensed.OnSet
    .Do(newValue => Console.WriteLine($"Changed to {newValue}!") );
```

Callbacks can also receive the invocation counter and current value:

```csharp
// Getter with counter and current value
sut.SetupMock.Property.TotalDispensed
    .OnGet.Do((int count, int value) => 
        Console.WriteLine($"[#{count}] Read TotalDispensed current value: {value}"));

// Setter with counter and new value
sut.SetupMock.Property.TotalDispensed
    .OnSet.Do((int count, int newValue) => 
        Console.WriteLine($"[#{count}] Set TotalDispensed to {newValue}"));
```

Callbacks also support sequences, similar to `Returns` and `Throws`:

```csharp
sut.SetupMock.Property.TotalDispensed.OnGet
	.Do(() => Console.WriteLine("Execute on all even read interactions"))
	.Do(() => Console.WriteLine("Execute on all odd read interactions"));
```

*Note:*  
All callbacks support more advanced features like conditional execution, frequency control, parallel execution, and
access to the invocation counter.
See [Advanced callback features](#advanced-callback-features) for
details.

**Register**

Register a setup without providing a value (useful when `ThrowWhenNotSetup` is enabled):

```csharp
var strictMock = Mock.Create<IChocolateDispenser>(MockBehavior.Default.ThrowingWhenNotSetup());

// Register property without value - won't throw
strictMock.SetupMock.Property.TotalDispensed.Register();
```

### Methods

Use `mock.SetupMock.Method.MethodName(…)` to set up methods. You can specify argument matchers for each parameter.

```csharp
// Setup Dispense to decrease stock and raise event
sut.SetupMock.Method.Dispense(It.Is("Dark"), It.IsAny<int>())
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
sut.SetupMock.Method.Dispense(It.Is("White"), It.IsAny<int>())
    .Do((type, amount) => Console.WriteLine($"Dispensed {amount} {type} chocolate."));

// Setup method to throw
sut.SetupMock.Method.Dispense(It.Is("Green"), It.IsAny<int>())
    .Throws<InvalidChocolateException>();
```

- Use `.Do(…)` to run code when the method is called. Supports parameterless or parameter callbacks.
- Use `.Returns(…)` to specify the value to return. You can provide a direct value, a callback, or a callback with
  parameters.
- Use `.Throws(…)` to specify an exception to throw. Supports direct exceptions, exception factories, or factories with
  parameters.
- Use `.Returns(…)` and `.Throws(…)` repeatedly to define a sequence of return values or exceptions (cycled on each
  call).
- Use `.SkippingBaseClass(…)` to override the base class behavior for a specific method (only for class mocks).
- When you specify overlapping setups, the most recently defined setup takes precedence.

**Async Methods**

For `Task<T>` or `ValueTask<T>` methods, use `.ReturnsAsync(…)` or `ThrowsAsync(…)`:

```csharp
sut.SetupMock.Method.DispenseAsync(It.IsAny<string>(), It.IsAny<int>())
    .ReturnsAsync((_, v) => v)            // First execution returns the value of the `int` parameter
    .ThrowsAsync(new TimeoutException())  // Second execution throws a TimeoutException
    .ReturnsAsync(0).Forever();           // Subsequent executions return 0
```

### Indexers

Set up indexers with argument matchers. Supports initialization, returns/throws sequences, and callbacks.

```csharp
sut.SetupMock.Indexer(It.IsAny<string>())
    .InitializeWith(type => 20)
    .OnGet.Do(type => Console.WriteLine($"Stock for {type} was read"));

sut.SetupMock.Indexer(It.Is("Dark"))
    .InitializeWith(10)
    .OnSet.Do((value, type) => Console.WriteLine($"Set [{type}] to {value}"));
```

**Initialization**

You can initialize indexers so they work like normal indexers (setter changes the value, getter returns the last set
value):

```csharp
sut.SetupMock.Indexer(It.IsAny<string>()).InitializeWith(42);
```

**Returns / Throws**

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

**Callbacks**

Register callbacks on the setter or getter of the indexer:

```csharp
sut.SetupMock.Indexer(It.IsAny<string>()).OnGet
    .Do(() => Console.WriteLine("Indexer was read!"));
sut.SetupMock.Indexer(It.IsAny<string>()).OnSet
    .Do(newValue => Console.WriteLine($"Changed indexer to {newValue}!") );
sut.SetupMock.Indexer(It.IsAny<string>()).OnSet
    .Do((index, newValue) => Console.WriteLine($"Changed this[{index}] to {newValue}!") );
```

Callbacks can also receive the invocation counter and current value:

```csharp
// Getter with counter and current value
sut.SetupMock.Indexer(It.IsAny<string>())
    .OnGet.Do((int count, string index, int value) => 
        Console.WriteLine($"[#{count}] Read this[{index}] current value: {value}"));

// Setter with counter and new value
sut.SetupMock.Indexer(It.IsAny<string>())
    .OnSet.Do((int count, string index, int newValue) => 
        Console.WriteLine($"[#{count}] Set this[{index}] to {newValue}"));
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
- `It.IsOneOf<T>(params T[] values)`: Matches any of the given values.
- `It.IsNull<T>()`: Matches null.
- `It.IsTrue()`/`It.IsFalse()`: Matches boolean true/false.
- `It.IsInRange(min, max)`: Matches a number within the given range. You can append `.Exclusive()` to exclude the
  minimum and maximum value.
- `It.Satisfies<T>(predicate)`: Matches values based on a predicate.

**String Matching**

- `It.Matches(pattern)`: Matches strings using wildcard patterns (`*` and `?`).

**Regular Expressions**  
Use `.AsRegex()` to enable regular expression matching for `It.Matches()`:

  ```csharp
  // Example: Match email addresses
  sut.SetupMock.Method.ValidateEmail(It.Matches(@"^\w+@\w+\.\w+$").AsRegex())
      .Returns(true);
  
  bool result = sut.ValidateEmail("user@example.com");
  
  // Case-sensitive regex
  sut.SetupMock.Method.Process(It.Matches("^[A-Z]+$").AsRegex().CaseSensitive())
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
sut.SetupMock.Method.TryParse(It.IsAny<string>(), It.IsOut(() => 42))
    .Returns(true);

int result;
bool success = sut.TryParse("abc", out result);
// result == 42, success == true

// Example: Setup with ref parameter
sut.SetupMock.Method.Increment(It.IsRef<int>(v => v + 1))
    .Returns(true);

int value = 5;
sut.Increment(ref value);
// value == 6
```

**Span Parameters (.NET 8+)**

- `It.IsSpan<T>(predicate)`: Matches `Span<T>` parameters that satisfy the predicate.
- `It.IsAnySpan<T>()`: Matches any `Span<T>` parameter.
- `It.IsReadOnlySpan<T>(predicate)`: Matches `ReadOnlySpan<T>` parameters that satisfy the predicate.
- `It.IsAnyReadOnlySpan<T>()`: Matches any `ReadOnlySpan<T>` parameter.

**Note:**  
As `ref struct` types cannot be stored directly, it is converted to an array internally and the `predicate` receives
this array for evaluation.

```csharp
// Example: Setup with Span parameter
sut.SetupMock.Method.Process(It.IsSpan<byte>(data => data.Length > 0))
    .Returns(true);

Span<byte> buffer = new byte[] { 1, 2, 3 };
bool result = sut.Process(buffer);
// result == true
```

**Custom Equality Comparers**

Use `.Using(IEqualityComparer<T>)` to provide custom equality comparison for `It.Is()` and `It.IsOneOf()`:

```csharp
// Example: Case-insensitive string comparison
var comparer = StringComparer.OrdinalIgnoreCase;
sut.SetupMock.Method.Process(It.Is("hello").Using(comparer))
    .Returns(42);

int result = sut.Process("HELLO");
// result == 42
```

#### Parameter Predicates

When the method name is unique (no overloads), you can use flexible parameter matching:

- `Match.AnyParameters()`: Matches any parameter combination.
- `Match.Parameters(Func<NamedParameterValue[], bool> predicate)`: Matches parameters based on a custom predicate.

```csharp
// Example: Custom parameter predicate
sut.SetupMock.Method.Process(Match.Parameters(args => 
    args.Length == 2 && 
    args[0].Value is string s && s.StartsWith("test") &&
    args[1].Value is int i && i > 0))
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
sut.SetupMock.Method.Dispense(It.Is("Dark"), It.IsAny<int>().Do(amount => lastAmount = amount));
sut.Dispense("Dark", 42);
// lastAmount == 42
```

**Monitor**

With `.Monitor(out monitor)`, you can track the actual
values passed during test execution and analyze them afterward.

**Example: Monitor for method parameter**

```csharp
Mockolate.ParameterMonitor<int> monitor;
sut.SetupMock.Method.Dispense(It.Is("Dark"), It.IsAny<int>().Monitor(out monitor));
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
sut.RaiseOnMock.ChocolateDispensed("Dark", 5);
```

- Use the `Raise` property to trigger events declared on the mocked interface or class.
- Only currently subscribed handlers will be invoked.
- Simulate notifications and test event-driven logic in your code.

**Example:**

```csharp
int dispensedAmount = 0;
sut.ChocolateDispensed += (type, amount) => dispensedAmount += amount;

sut.RaiseOnMock.ChocolateDispensed("Dark", 3);
sut.RaiseOnMock.ChocolateDispensed("Milk", 2);

// dispensedAmount == 5
```

You can subscribe and unsubscribe handlers as needed. Only handlers subscribed at the time of raising the event will be
called.

## Verify interactions

You can verify that methods, properties, indexers, or events were called or accessed with specific arguments and how
many times, using the `Verify` API:

Supported call count verifications in the `Mockolate.VerifyMock` namespace:

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

### Properties

You can verify access to property getter and setter:

```csharp
// Verify that the property 'TotalDispensed' was read at least once
sut.VerifyMock.Got.TotalDispensed().AtLeastOnce();

// Verify that the property 'TotalDispensed' was set to 42 exactly once
sut.VerifyMock.Set.TotalDispensed(It.Is(42)).Once();
```

**Note:**  
The setter value also supports argument matchers.

### Methods

You can verify that methods were invoked with specific arguments and how many times:

```csharp
// Verify that Dispense("Dark", 5) was invoked at least once
sut.VerifyMock.Invoked.Dispense(It.Is("Dark"), It.Is(5))
    .AtLeastOnce();

// Verify that Dispense was never invoked with "White" and any amount
sut.VerifyMock.Invoked.Dispense(It.Is("White"), It.IsAny<int>())
    .Never();

// Verify that Dispense was invoked exactly twice with any type and any amount
sut.VerifyMock.Invoked.Dispense(Match.AnyParameters())
    .Exactly(2);

// Verify that Dispense was invoked between 3 and 5 times (inclusive)
sut.VerifyMock.Invoked.Dispense(It.IsAny<string>(), It.IsAny<int>())
    .Between(3, 5);

// Verify that Dispense was invoked an even number of times
sut.VerifyMock.Invoked.Dispense(It.IsAny<string>(), It.IsAny<int>())
    .Times(count => count % 2 == 0);
```

### Indexers

You can verify access to indexer getter and setter:

```csharp
// Verify that the indexer was read with key "Dark" exactly once
sut.VerifyMock.GotIndexer(It.Is("Dark")).Once();

// Verify that the indexer was set with key "Milk" to value 7 at least once
sut.VerifyMock.SetIndexer(It.Is("Milk"), 7).AtLeastOnce();
```

**Note:**  
The keys and value also supports argument matchers.

### Events

You can verify event subscriptions and unsubscriptions:

```csharp
// Verify that the event 'ChocolateDispensed' was subscribed to at least once
sut.VerifyMock.SubscribedTo.ChocolateDispensed().AtLeastOnce();

// Verify that the event 'ChocolateDispensed' was unsubscribed from exactly once
sut.VerifyMock.UnsubscribedFrom.ChocolateDispensed().Once();
```

### Argument Matchers

You can use argument matchers from the `With` class to verify calls with flexible conditions:

- `It.IsAny<T>()`: Matches any value of type `T`.
- `It.Is<T>(value)`: Matches a specific value. With `.Using(IEqualityComparer<T>)`, you can provide a custom equality
  comparer.
- `It.IsOneOf<T>(params T[] values)`: Matches any of the given values. With `.Using(IEqualityComparer<T>)`, you can
  provide a custom equality comparer.
- `It.IsNull<T>()`: Matches null.
- `It.IsTrue()`/`It.IsFalse()`: Matches boolean true/false.
- `It.IsInRange(min, max)`: Matches a number within the given range. You can append `.Exclusive()` to exclude the
  minimum and maximum value.
- `It.IsOut<T>()`: Matches any out parameter of type `T`
- `It.IsRef<T>()`: Matches any ref parameter of type `T`
- `It.Matches<string>(pattern)`: Matches strings using wildcard patterns (`*` and `?`). With `.AsRegex()`, you can use
  regular expressions instead.
- `It.Satisfies<T>(predicate)`: Matches values based on a predicate.

**Example:**

```csharp
sut.VerifyMock.Invoked.Dispense(It.Is<string>(t => t.StartsWith("D")), It.IsAny<int>()).Once();
sut.VerifyMock.Invoked.Dispense(It.Is("Milk"), It.IsAny<int>()).AtLeastOnce();
```

### Call Ordering

Use `Then` to verify that calls occurred in a specific order:

```csharp
sut.VerifyMock.Invoked.Dispense(It.Is("Dark"), It.Is(2)).Then(
    m => m.Invoked.Dispense(It.Is("Dark"), It.Is(3))
);
```

You can chain multiple calls for strict order verification:

```csharp
sut.VerifyMock.Invoked.Dispense(It.Is("Dark"), It.Is(1)).Then(
    m => m.Invoked.Dispense(It.Is("Milk"), It.Is(2)),
    m => m.Invoked.Dispense(It.Is("White"), It.Is(3)));
```

If the order is incorrect or a call is missing, a `MockVerificationException` will be thrown with a descriptive message.

## Advanced Features

### Working with protected members

Mockolate allows you to set up and verify protected virtual members on class mocks. Access protected members using the
`.Protected` property:

**Example**

```csharp
public abstract class ChocolateDispenser
{
    protected virtual bool DispenseInternal(string type, int amount) => true;
    protected virtual int InternalStock { get; set; }
}

var sut = Mock.Create<ChocolateDispenser>();
```

#### Setup

```csharp
// Setup protected method
sut.SetupMock.Protected.Method.DispenseInternal(
    It.Is("Dark"), It.IsAny<int>())
    .Returns(true);

// Setup protected property
sut.SetupMock.Protected.Property.InternalStock.InitializeWith(100);
```

**Notes:**

- Protected members can be set up and verified just like public members, using the `.Protected` accessor.
- All setup options (`.Returns()`, `.Throws()`, `.Do()`, `.InitializeWith()`, etc.) work with protected members.

#### Verification

```csharp
// Verify protected method was invoked
sut.VerifyMock.Invoked.Protected.DispenseInternal(
    It.Is("Dark"), It.IsAny<int>()).Once();

// Verify protected property was read
sut.VerifyMock.Got.Protected.InternalStock().AtLeastOnce();

// Verify protected property was set
sut.VerifyMock.Set.Protected.InternalStock(It.Is(100)).Once();

// Verify protected indexer was read
sut.VerifyMock.GotProtectedIndexer(It.Is(0)).Once();

// Verify protected indexer was set
sut.VerifyMock.SetProtectedIndexer(It.Is(0), It.Is(42)).Once();
```

**Note:**

- All verification options (argument matchers, count assertions) work the same for protected members as for public
  members.
- Protected indexers are supported using `.GotProtectedIndexer()`/`.SetProtectedIndexer()` for verification.

### Advanced callback features

#### Conditional callbacks

Execute callbacks conditionally based on the zero-based invocation counter using `.When()`:

```csharp
sut.SetupMock.Method.Dispense(It.Is("Dark"), It.IsAny<int>())
    .Do(() => Console.WriteLine("Called!"))
    .When(count => count >= 2);  // The first two calls are skipped
```

#### Frequency control

Control how many times a callback executes:

```csharp
// Execute up to 3 times
sut.SetupMock.Method.Dispense(It.IsAny<string>(), It.IsAny<int>())
    .Do(() => Console.WriteLine("Up to 3 times"))
    .Only(3);

// Executes the callback only once
sut.SetupMock.Method.Dispense(It.IsAny<string>(), It.IsAny<int>())
    .Do(() => Console.WriteLine("Only once"))
    .OnlyOnce();
```

#### Parallel callbacks

When you specify multiple callbacks, they are executed sequentially by default. You can change this behavior to always
run specific callbacks in parallel using `.InParallel()`:

```csharp
sut.SetupMock.Method.Dispense(It.IsAny<string>(), It.IsAny<int>())
    .Do(() => { Console.WriteLine("Runs every second iteration"); })
    .Do(() => { Console.WriteLine("Runs always in parallel"); }).InParallel()
    .Do(() => { Console.WriteLine("Runs every other iteration"); });
```

#### Invocation counter

Access the zero-based invocation counter in callbacks:

```csharp
sut.SetupMock.Method.Dispense(It.IsAny<string>(), It.IsAny<int>())
    .Do((count, _, _) => Console.WriteLine($"Call #{count}"));

sut.SetupMock.Property.TotalDispensed.OnGet
	.Do((count, value) => Console.WriteLine($"Read #{count}, value: {value}"));
```

### Monitor interactions

Mockolate tracks all interactions with mocks on the mock object. To only track interactions within a given scope, you
can use a `MockMonitor<T>`:

```csharp
var sut = Mock.Create<IChocolateDispenser>();
var monitor = new MockMonitor<IChocolateDispenser>(sut);

sut.Dispense("Dark", 1); // Not monitored
using (monitor.Run())
{
    sut.Dispense("Dark", 2); // Monitored
}
sut.Dispense("Dark", 3); // Not monitored

// Verifications on the monitor only count interactions during the lifetime scope of the `IDisposable`
monitor.Verify.Invoked.Dispense(It.Is("Dark"), It.IsAny<int>()).Once();
```

Alternatively, you can use the `MonitorMock()` extension method to create an already running monitor directly from the
mock:

```csharp
var sut = Mock.Create<IChocolateDispenser>();

sut.Dispense("Dark", 1); // Not monitored
using var scope = sut.MonitorMock(out var monitor);
sut.Dispense("Dark", 2); // Monitored
sut.Dispense("Dark", 3); // Monitored

// Verifications on the monitor only count interactions during the lifetime scope of the `IDisposable`
monitor.Verify.Invoked.Dispense(It.Is("Dark"), It.IsAny<int>()).Twice();
```

#### Clear all interactions

For simpler scenarios you can directly clear all recorded interactions on a mock using `ClearAllInteractions` on the
setup:

```csharp
var sut = Mock.Create<IChocolateDispenser>();

sut.Dispense("Dark", 1);
// Clears all previously recorded interactions
sut.SetupMock.ClearAllInteractions();
sut.Dispense("Dark", 2);

sut.VerifyMock.Invoked.Dispense(It.Is("Dark"), It.IsAny<int>()).Once();
```

### Check for unexpected interactions

#### That all interactions are verified

You can check if all interactions with the mock have been verified using `ThatAllInteractionsAreVerified`:

```csharp
// Returns true if all interactions have been verified before
bool allVerified = sut.VerifyMock.ThatAllInteractionsAreVerified();
```

This is useful for ensuring that your test covers all interactions and that no unexpected calls were made.
If any interaction was not verified, this method returns `false`.

#### That all setups are used

You can check if all registered setups on the mock have been used using `ThatAllSetupsAreUsed`:

```csharp
// Returns true if all setups have been used
bool allUsed = sut.VerifyMock.ThatAllSetupsAreUsed();
```

This is useful for ensuring that your test setup and test execution match.
If any setup was not used, this method returns `false`.

## Special Types

### HttpClient

Mockolate supports mocking `HttpClient` out of the box, with no special configuration required. You can set up, use, and
verify HTTP interactions just like with any other interface or class.

**Example: Mocking HttpClient for a Chocolate Dispenser Service**

```csharp
HttpClient httpClient = Mock.Create<HttpClient>();
httpClient.SetupMock.Method.PostAsync(
		It.IsAny<string>(),
		It.IsStringContent())
	.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

HttpResponseMessage result = await httpClient.PostAsync("https://aweXpect.com/api/chocolate/dispense",
	new StringContent("""
	                  { "type": "Dark", "amount": 3 }
	                  """, Encoding.UTF8, "application/json"));

await That(result.IsSuccessStatusCode).IsTrue();
httpClient.VerifyMock.Invoked.PostAsync(
	It.IsUri("*aweXpect.com/api/chocolate/dispense*").ForHttps(),
	It.IsStringContent("application/json").WithBodyMatching("*\"type\": \"Dark\"*\"amount\": 3*")).Once();
```

**Notes:**

- The custom extensions for the `HttpClient` are in the `Mockolate.Web` namespace.
- Under the hood, the setups, requests and verifications are forwarded to a mocked `HttpMessageHandler`.
  As they therefore all forward to the `SendAsync` method, you can mix using a string or an `Uri` parameter in setup or
  verification.

### Delegates

Mockolate supports mocking delegates including `Action`, `Func<T>`, and custom delegates.

**Setup**

Use `SetupMock.Delegate(…)` to configure delegate behavior.

```csharp
// Mock Action delegate
Action myAction = Mock.Create<Action>();
myAction.SetupMock.Delegate().Do(() => Console.WriteLine("Action invoked!"));

// Mock Func<T> delegate
Func<int> myFunc = Mock.Create<Func<int>>();
myFunc.SetupMock.Delegate().Returns(42);
```

For custom delegates with parameters:

```csharp
// Define a custom delegate (typically declared at type level)
public delegate int Calculate(int x, string operation);

// Create and setup the mock
Calculate calculator = Mock.Create<Calculate>();
calculator.SetupMock.Delegate(It.IsAny<int>(), It.Is("add"))
    .Returns((x, operation) => x + 10);
```

Delegates with `ref` and `out` parameters are also supported:

```csharp
// Define a custom delegate (typically declared at type level)
public delegate void ProcessData(int input, ref int value, out int result);

// Create and setup the mock
ProcessData processor = Mock.Create<ProcessData>();
processor.SetupMock.Delegate(It.IsAny<int>(), It.IsRef<int>(v => v + 1), It.IsOut(() => 100));
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
Action myAction = Mock.Create<Action>();
myAction.Invoke();
myAction.VerifyMock.Invoked().AtLeastOnce();

// Verify Func<T> was invoked exactly once
Func<int> myFunc = Mock.Create<Func<int>>();
_ = myFunc();
myFunc.VerifyMock.Invoked().Once();
```

For custom delegates with parameters:

```csharp
// Define a custom delegate (typically declared at type level)
public delegate int Calculate(int x, string operation);

// Create, invoke, and verify the mock
Calculate calculator = Mock.Create<Calculate>();
_ = calculator(5, "add");
calculator.VerifyMock.Invoked(It.IsAny<int>(), It.Is("add")).Once();
```

Delegates with `ref` and `out` parameters are also supported:

```csharp
// Define a custom delegate (typically declared at type level)
public delegate void ProcessData(int input, ref int value, out int result);

// Create, invoke, and verify the mock
ProcessData processor = Mock.Create<ProcessData>();
int val = 0;
processor(1, ref val, out int res);
processor.VerifyMock.Invoked(It.IsAny<int>(), It.IsRef<int>(), It.IsOut<int>()).Once();
```

**Note:**  
Delegate parameters also support [argument matchers](#argument-matchers).

## Analyzers

Mockolate ships with some Roslyn analyzers to help you adopt best practices and catch issues early, at compile time.
All rules provide actionable messages and link to identifiers for easy filtering.

### Mockolate0001

`Verify` methods only return a `VerificationResult` and do not directly throw. You have to specify how often you expect
the call to happen, e.g. `.AtLeastOnce()`, `.Exactly(n)`, etc. or use the verification result in any other way.

**Example:**

```csharp
var sut = Mock.Create<IChocolateDispenser>();
sut.Dispense("Dark", 1);
// Analyzer Mockolate0001: Add a count assertion like .AtLeastOnce() or use the result.
sut.VerifyMock.Invoked.Dispense(It.Is("Dark"), It.IsAny<int>());
```

The included code fixer suggests to add the `.AtLeastOnce()` count assertion:

```csharp
sut.VerifyMock.Invoked.Dispense(It.Is("Dark"), It.IsAny<int>()).AtLeastOnce();
```

### Mockolate0002

Mock arguments must be mockable (interfaces or supported classes).
This rule will prevent you from using unsupported types (e.g. sealed classes) when using `Mock.Create<T>()`.

### Mockolate0003

Wrap type arguments must be interfaces.
This rule will prevent you from using non-interface types as the type parameter when using `Mock.Wrap<T>(T instance)`.
