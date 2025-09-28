# Mockerade

[![Nuget](https://img.shields.io/nuget/v/Mockerade)](https://www.nuget.org/packages/Mockerade)
[![Build](https://github.com/Mockerade/Mockerade/actions/workflows/build.yml/badge.svg)](https://github.com/Mockerade/Mockerade/actions/workflows/build.yml)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Mockerade_Mockerade&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=Mockerade_Mockerade)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=Mockerade_Mockerade&metric=coverage)](https://sonarcloud.io/summary/overall?id=Mockerade_Mockerade)

**Mockerade** is a modern, strongly-typed mocking framework for .NET, powered by source generators. It enables fast, compile-time validated mocks for interfaces and classes, supporting .NET Standard 2.0, .NET 8, .NET 10, and .NET Framework 4.8.

- **Source generator-based**: No runtime proxy generation, fast and reliable.
- **Strongly-typed**: Setup and verify mocks with full IntelliSense and compile-time safety.
- **Event support**: Easily raise and verify events.
- **Flexible argument matching**: Use `With.Any<T>()`, `With.Matching<T>(predicate)`, and `With.Out<T>()` for advanced setups.

## Getting Started

1. Install the [`Mockerade`](https://www.nuget.org/packages/Mockerade) nuget package
   ```ps
   dotnet add package Mockerade
   ```

2. Create a mock
   ```csharp
   using Mockerade;
   
   var mock = Mock.For<IMyInterface>();
   ```

## Features

### Setup

Set up return values or behaviors for methods and properties on your mock. This allows you to control how the mock responds to calls in your tests.

```csharp
mock.Setup.AddUser(With.Any<string>())
    .Returns(new User(Guid.NewGuid(), "Alice"));

mock.Setup.Property.Get().Returns(42);
```
- Use `.Returns(value)` to specify the value to return.
- You can also set up void methods and property setters.

### Argument Matching

Mockerade provides flexible argument matching for method setups and verifications:
- `With.Any<T>()`: Matches any value of type `T`.
- `With.Matching<T>(predicate)`: Matches values based on a predicate.
- `With.Out<T>(valueFactory)`: Matches and sets out parameters.

```csharp
mock.Setup.AddUser(With<string>.Matching(name => name.StartsWith("A")))
    .Returns(new User(Guid.NewGuid(), "Alicia"));

mock.Setup.TryDelete(With.Any<Guid>(), With.Out<User?>(() => new User(id, "Alice")))
    .Returns(true);
```

### Event Raising

Easily raise events on your mock to test event handlers in your code:

```csharp
mock.Raises.UsersChanged(this, EventArgs.Empty);
```
- Use the `Raises` property to trigger events declared on the mocked interface or class.
- This allows you to simulate notifications and test event-driven logic.

### Verification

Verify that methods or properties were called with specific arguments and how many times:

```csharp
mock.Invoked.AddUser("Bob").Invocations.Count(); // e.g., 1
mock.Invoked.TryDelete(id, With.Out<User?>()).Invocations.Count();
```
- Use the `Invoked` property to access invocation history for each method or property.
- You can assert on the number of invocations or inspect the arguments used.

