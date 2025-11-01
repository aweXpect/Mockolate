# Mockolate

<img align="right" width="200" src="Docs/logo_256x256.png" alt="Mockolate logo" />

[![Nuget](https://img.shields.io/nuget/v/Mockolate)](https://www.nuget.org/packages/Mockolate)
[![Build](https://github.com/aweXpect/Mockolate/actions/workflows/build.yml/badge.svg)](https://github.com/aweXpect/Mockolate/actions/workflows/build.yml)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=aweXpect_Mockolate&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=aweXpect_Mockolate)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=aweXpect_Mockolate&metric=coverage)](https://sonarcloud.io/summary/overall?id=aweXpect_Mockolate)
[![Mutation testing badge](https://img.shields.io/endpoint?style=flat&url=https%3A%2F%2Fbadge-api.stryker-mutator.io%2Fgithub.com%2FaweXpect%2FMockolate%2Fmain)](https://dashboard.stryker-mutator.io/reports/github.com/aweXpect/Mockolate/main)

**Mockolate** is a modern, strongly-typed mocking library for .NET, powered by source generators. It enables fast,
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

   // Create a mock for IChocolateDispenser
   var mock = Mock.Create<IChocolateDispenser>();
   
   // Setup: Initial stock of 10 for Dark chocolate
   mock.Setup.Indexer("Dark").InitializeWith(10);
   // Setup: Dispense decreases Dark chocolate if enough, returns true/false
   mock.Setup.Method.Dispense("Dark", With.Any<int>())
       .Returns((type, amount) =>
       {
           var current = mock.Subject[type];
           if (current >= amount)
           {
               mock.Subject[type] = current - amount;
               mock.Raise.ChocolateDispensed(type, amount);
               return true;
           }
           return false;
       });
   
   // Track dispensed amount via event
   int dispensedAmount = 0;
   mock.Subject.ChocolateDispensed += (type, amount) => dispensedAmount += amount;
   
   // Act: Try to dispense chocolates
   bool gotChoc1 = mock.Subject.Dispense("Dark", 4); // true
   bool gotChoc2 = mock.Subject.Dispense("Dark", 5); // true
   bool gotChoc3 = mock.Subject.Dispense("Dark", 6); // false
   
   // Verify: Check interactions
   mock.Verify.Invoked.Dispense("Dark", With.Any<int>()).Exactly(3);
   
   // Output: "Dispensed events: 9. Got chocolate? True, True, False"
   Console.WriteLine($"Dispensed amount: {dispensedAmount}. Got chocolate? {gotChoc1}, {gotChoc2}, {gotChoc3}");

   public delegate void ChocolateDispensedDelegate(string type, int amount);
   public interface IChocolateDispenser
   {
       int this[string type] { get; set; }
       int TotalDispensed { get; set; }
       bool Dispense(string type, int amount);
       event ChocolateDispensedDelegate ChocolateDispensed;
   }
   ```

## Features

### Create mocks

You can create mocks for interfaces and classes. For classes without a default constructor, use
`BaseClass.WithConstructorParameters(…)` to provide constructor arguments:

```csharp
// Create a mock for an interface
var mock = Mock.Create<IChocolateDispenser>();

// Create a mock for a class
var classMock = Mock.Create<MyChocolateDispenser>();

// For classes without a default constructor:
var classWithArgsMock = Mock.Create<MyChocolateDispenserWithCtor>(
    BaseClass.WithConstructorParameters("Dark", 42)
);

// Specify up to 8 additional interfaces for the mock:
var mock3 = factory.Create<MyChocolateDispenser, ILemonadeDispenser>();
```

**Notes:**

- Only the first generic type can be a class; additional types must be interfaces.
- Sealed classes cannot be mocked and will throw a `MockException`.

#### Customizing mock behavior

You can control the default behavior of the mock by providing a `MockBehavior`:

```csharp
var strictMock = Mock.Create<IChocolateDispenser>(new MockBehavior { ThrowWhenNotSetup = true });

// For classes with constructor parameters and custom behavior:
var classMock = Mock.Create<MyChocolateDispenser>(
    BaseClass.WithConstructorParameters("Dark", 42),
    new MockBehavior { ThrowWhenNotSetup = true }
);
```

##### `MockBehavior` options

- `ThrowWhenNotSetup` (bool):
	- If `true`, the mock will throw an exception when a method or property is called without a setup.
	- If `false`, the mock will return a default value (see `DefaultValue`).
- `BaseClassBehavior` (enum):
	- Controls how the mock interacts with base class members. Options:
		- `DoNotCallBaseClass`: Do not call base class implementation (default).
		- `OnlyCallBaseClass`: Only call base class implementation.
		- `UseBaseClassAsDefaultValue`: Use base class as a fallback for default values.
- `DefaultValue` (IDefaultValueGenerator):
	- Customizes how default values are generated for methods/properties that are not set up.

#### Using a factory for shared behavior

Use `Mock.Factory` to create multiple mocks with a shared behavior:

```csharp
var behavior = new MockBehavior { ThrowWhenNotSetup = true };
var factory = new Mock.Factory(behavior);

var mock1 = factory.Create<IChocolateDispenser>();
var mock2 = factory.Create<ILemonadeDispenser>();
```

### Setup

Set up return values or behaviors for methods, properties, and indexers on your mock. Control how the mock responds to
calls in your tests.

#### Method Setup

Use `mock.Setup.Method.MethodName(…)` to set up methods. You can specify argument matchers for each parameter.

```csharp
// Setup Dispense to decrease stock and raise event
mock.Setup.Method.Dispense("Dark", With.Any<int>())
    .Returns((type, amount) =>
    {
        var current = mock.Subject[type];
        if (current >= amount)
        {
            mock.Subject[type] = current - amount;
            mock.Raise.ChocolateDispensed(type, amount);
            return true;
        }
        return false;
    });

// Setup method to throw
mock.Setup.Method.Dispense("White", With.Any<int>())
    .Callback((type, amount) => Console.WriteLine($"Disposed {amount} {type} chocolate."));

// Setup method to throw
mock.Setup.Method.Dispense("Green", With.Any<int>())
    .Throws(() => new InvalidChocolateException());
```

- Use `.Callback(…)` to run code when the method is called. Supports parameterless or parameter callbacks.
- Use `.Returns(…)` to specify the value to return. You can provide a direct value, a callback, or a callback with
  parameters.
- Use `.Throws(…)` to specify an exception to throw. Supports direct exceptions, exception factories, or factories with
  parameters.
- Use `.Returns(…)` and `.Throws(…)` repeatedly to define a sequence of return values or exceptions (cycled on each
  call).

**Async Methods**

For `Task<T>` or `ValueTask<T>` methods, use `.ReturnsAsync(…)`:

```csharp
mock.Setup.Method.DispenseAsync(With.Any<string>(), With.Any<int>())
    .ReturnsAsync(true);
```

##### Argument Matching

Mockolate provides flexible argument matching for method setups and verifications:

- `With.Any<T>()`: Matches any value of type `T`.
- `With.Matching<T>(predicate)`: Matches values based on a predicate.
- `With.Value<T>(value)`: Matches a specific value.
- `With.Null<T>()`: Matches null.
- `With.Out<T>(…)`/`With.Ref<T>(…)`: Matches and sets out/ref parameters, supports value setting and predicates.
- For .NET 8+: `With.ValueBetween<T>(min).And(max)` matches a range (numeric types).

#### Property Setup

Set up property getters and setters to control or verify property access on your mocks.

**Initialization**

You can initialize properties so they work like normal properties (setter changes the value, getter returns the last set
value):

```csharp
mock.Setup.Property.TotalDispensed.InitializeWith(42);
```

**Returns / Throws**

Alternatively, set up properties with `Returns` and `Throws` (supports sequences):

```csharp
mock.Setup.Property.TotalDispensed
    .Returns(1)
    .Returns(2)
    .Throws(new Exception("Error"))
    .Returns(4);
```

**Callbacks**

Register callbacks on the setter or getter:

```csharp
mock.Setup.Property.TotalDispensed.OnGet(() => Console.WriteLine("TotalDispensed was read!"));
mock.Setup.Property.TotalDispensed.OnSet((oldValue, newValue) => Console.WriteLine($"Changed from {oldValue} to {newValue}!") );
```

#### Indexer Setup

Set up indexers with argument matchers. Supports initialization, returns/throws sequences, and callbacks.

```csharp
mock.Setup.Indexer(With.Any<string>())
    .InitializeWith(type => 20)
    .OnGet(type => Console.WriteLine($"Stock for {type} was read"));

mock.Setup.Indexer("Dark")
    .InitializeWith(10)
    .OnSet((value, type) => Console.WriteLine($"Set [{type}] to {value}"));
```

- `.InitializeWith(…)` can take a value or a callback with parameters.
- `.Returns(…)` and `.Throws(…)` support direct values, callbacks, and callbacks with parameters and/or the current
  value.
- `.OnGet(…)` and `.OnSet(…)` support callbacks with or without parameters.
- `.Returns(…)` and `.Throws(…)` can be chained to define a sequence of behaviors, which are cycled through on each
  call.

### Mock events

Easily raise events on your mock to test event handlers in your code.

#### Raise

Use the strongly-typed `Raise` property on your mock to trigger events declared on the mocked interface or class. The
method signature matches the event delegate.

```csharp
// Arrange: subscribe a handler to the event
mock.Subject.ChocolateDispensed += (type, amount) => { /* handler code */ };

// Act: raise the event
mock.Raise.ChocolateDispensed("Dark", 5);
```

- Use the `Raise` property to trigger events declared on the mocked interface or class.
- Only currently subscribed handlers will be invoked.
- Simulate notifications and test event-driven logic in your code.

**Example:**

```csharp
int dispensedAmount = 0;
mock.Subject.ChocolateDispensed += (type, amount) => dispensedAmount += amount;

mock.Raise.ChocolateDispensed("Dark", 3);
mock.Raise.ChocolateDispensed("Milk", 2);

// dispensedAmount == 5
```

You can subscribe and unsubscribe handlers as needed. Only handlers subscribed at the time of raising the event will be
called.

### Verify interactions

You can verify that methods, properties, indexers, or events were called or accessed with specific arguments and how
many times, using the `Verify` API:

Supported call count verifications in the `Mockolate.Verify` namespace:

- `.Never()`
- `.Once()`
- `.Twice()`
- `.Exactly(n)`
- `.AtLeastOnce()`
- `.AtLeastTwice()`
- `.AtLeast(n)`
- `.AtMostOnce()`
- `.AtMostTwice()`
- `.AtMost(n)`

#### Methods

You can verify that methods were invoked with specific arguments and how many times:

```csharp
// Verify that Dispense("Dark", 5) was invoked at least once
mock.Verify.Invoked.Dispense("Dark", 5).AtLeastOnce();

// Verify that Dispense was never invoked with "White" and any amount
mock.Verify.Invoked.Dispense("White", With.Any<int>()).Never();

// Verify that Dispense was invoked exactly twice with any type and any amount
mock.Verify.Invoked.Dispense(With.Any<string>(), With.Any<int>()).Exactly(2);
```

##### Argument Matchers

You can use argument matchers from the `With` class to verify calls with flexible conditions:

- `With.Any<T>()` � matches any value of type `T`
- `With.Null<T>()` � matches `null`
- `With.Matching<T>(predicate)` � matches values satisfying a predicate
- `With.Value(value)` � matches a specific value
- `With.Out<T>()` � matches any out parameter of type `T`
- `With.Ref<T>()` � matches any ref parameter of type `T`
- `With.Out<T>(setter)` � matches and sets an out parameter
- `With.Ref<T>(setter)` � matches and sets a ref parameter
- `With.ValueBetween<T>(min).And(max)` � matches a value between min and max (for numeric types, .NET 8+)

**Example:**

```csharp
mock.Verify.Invoked.Dispense(With.Matching<string>(t => t.StartsWith("D")), With.ValueBetween(1).And(10)).Once();
mock.Verify.Invoked.Dispense("Milk", With.ValueBetween(1).And(5)).AtLeastOnce();
```

#### Properties

You can verify access to property getter and setter:

```csharp
// Verify that the property 'TotalDispensed' was read at least once
mock.Verify.Got.TotalDispensed().AtLeastOnce();

// Verify that the property 'TotalDispensed' was set to 42 exactly once
mock.Verify.Set.TotalDispensed(42).Once();
```

**Note:**  
The setter value also supports argument matchers.

#### Indexers

You can verify access to indexer getter and setter:

```csharp
// Verify that the indexer was read with key "Dark" exactly once
mock.Verify.GotIndexer("Dark").Once();

// Verify that the indexer was set with key "Milk" to value 7 at least once
mock.Verify.SetIndexer("Milk", 7).AtLeastOnce();
```

**Note:**  
The keys and value also supports argument matchers.

#### Events

You can verify event subscriptions and unsubscriptions:

```csharp
// Verify that the event 'ChocolateDispensed' was subscribed to at least once
mock.Verify.SubscribedTo.ChocolateDispensed().AtLeastOnce();

// Verify that the event 'ChocolateDispensed' was unsubscribed from exactly once
mock.Verify.UnsubscribedFrom.ChocolateDispensed().Once();
```

#### Call Ordering

Use `Then` to verify that calls occurred in a specific order:

```csharp
mock.Verify.Invoked.Dispense("Dark", 2).Then(
    m => m.Invoked.Dispense("Dark", 3)
);
```

You can chain multiple calls for strict order verification:

```csharp
mock.Verify.Invoked.Dispense("Dark", 1).Then(
    m => m.Invoked.Dispense("Milk", 2),
    m => m.Invoked.Dispense("White", 3));
```

If the order is incorrect or a call is missing, a `MockVerificationException` will be thrown with a descriptive message.

#### Check for unexpected interactions

You can check if all interactions with the mock have been verified using `ThatAllInteractionsAreVerified`:

```csharp
// Returns true if all interactions have been verified before
bool allVerified = mock.Verify.ThatAllInteractionsAreVerified();
```

This is useful for ensuring that your test covers all interactions and that no unexpected calls were made.
If any interaction was not verified, this method returns `false`.


