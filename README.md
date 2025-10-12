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

2. Create a mock
   ```csharp
   using Mockolate;
   
   var mock = Mock.Create<IMyInterface>();
   ```


## Features

### Mock Creation

- Create mocks for interfaces and classes:
  ```csharp
  var mock = Mock.Create<IMyInterface>();
  var classMock = Mock.Create<MyVirtualClass>();
  ```
- Provide a `MockBehavior` to control the default behavior of the mock.
- Use a `Mock.Factory` to pass a common behavior to all created mocks.

### Setup / Arrange

Set up return values or behaviors for methods and properties on your mock. Control how the mock responds to calls in your tests.

#### Method setup
```csharp
mock.Setup.AddUser(With.Any<string>())
    .Returns(name => new User(Guid.NewGuid(), name));
```

- Use `.Callback(…)` to run code when the method is called.
- Use `.Returns(…)` to specify the value to return. You can provide a direct value or a callback to generate values on demand.
- Use `.Throws(…)` to specify an exception to throw when the method is executed.
- Use `.Returns(…)` and `.Throws(…)` repeatedly to define a sequence of return values.

**Argument Matching**

Mockolate provides flexible argument matching for method setups and verifications:

- `With.Any<T>()`: Matches any value of type `T`.
- `With.Matching<T>(predicate)`: Matches values based on a predicate.
- `With.Ref<T>(…)`/`With.Out<T>(…)`: Matches and sets ref or out parameters.

```csharp
mock.Setup.AddUser(With.Matching<string>(name => name.StartsWith("A")))
    .Returns(new User(Guid.NewGuid(), "Alicia"));

mock.Setup.TryDelete(With.Any<Guid>(), With.Out<User?>(() => new User(id, "Alice")))
    .Returns(true);
```

#### Property Setup

Set up property getters and setters to control or verify property access on your mocks. Supports auto-properties and indexers.

**Initialization**  
You can initialize properties and they will work like normal properties (setter changes the value, getter returns the last set value).

```csharp
mock.Setup.Property.MyProperty.InitializeWith(42);
```

**Returns / Throws**  
Alternatively you can set up the properties similar to methods with `Returns` and `Throws`.
```csharp
mock.Setup.Property.MyProperty
	.Returns(1)
	.Returns(2)
	.Throws(new Exception("Error"))
	.Returns(4);
```

**Callbacks**
Callbacks can be registered on the setter or getter.
```csharp
mock.Setup.Property.MyProperty.OnGet(() => Console.WriteLine("MyProperty was read!"));
mock.Setup.Property.MyProperty.OnSet(value => Console.WriteLine($"Set MyProperty to {value}}!"));
```

**Indexers**
Indexers are supported as well.
```csharp
mock.Setup.Indexer(With.Any<int>())
	.InitializeWith(v => v*v)
	.OnGet(v => Console.WriteLine($"Indexer this[{v}] was read"));
```

### Event Raising

Easily raise events on your mock to test event handlers in your code:

```csharp
mock.Raises.UsersChanged(this, EventArgs.Empty);
```

- Use the `Raises` property to trigger events declared on the mocked interface or class.
- Simulate notifications and test event-driven logic.

### Verification

Verify that methods or properties were called with specific arguments and how many times:

```csharp
mock.Verify.Invoked.AddUser("Bob").AtLeastOnce();
mock.Verify.Invoked.TryDelete(id, With.Out<User?>()).Never();
mock.Verify.Invoked.DoSomething().Exactly(2);
```

- Use `.Never()`, `.AtLeastOnce()`, `.AtMost(n)`, `.Exactly(n)` for call count verification.
- Verify arguments with matchers.


#### Call Ordering

Verify that calls occurred in a specific order:

```csharp
mock.Verify.Invoked.AddUser("Alice").Then(
    m => m.Invoked.DeleteUser("Alice")
);
```

