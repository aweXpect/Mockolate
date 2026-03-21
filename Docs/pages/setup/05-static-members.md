# Static Interface Members (.NET 8+)

Mockolate supports mocking static abstract and virtual members on interfaces, including static methods, static
properties, and static events. This feature requires .NET 8 or later.

## Setup

Use `SetupStatic` on your mock to configure behavior for static members:

```csharp
public interface ICalculatorService
{
    static abstract int Add(int a, int b);
    static abstract int Result { get; }
    static abstract event Action<int> Calculated;
}

ICalculatorService sut = ICalculatorService.CreateMock();

// Setup static method
sut.Mock.SetupStatic.Add(It.IsAny<int>(), It.IsAny<int>()).Returns(42);

// Setup static property
sut.Mock.SetupStatic.Result.Returns(100);
```

## Verification

Use `VerifyStatic` on your mock to verify static member interactions:

```csharp
// Verify static method was called
sut.Mock.VerifyStatic.Add(It.IsAny<int>(), It.IsAny<int>()).Once();

// Verify static property was read
sut.Mock.VerifyStatic.Result.Got().Once();
```

## Events

You can raise and verify static events using `RaiseStatic` and `VerifyStatic`:

```csharp
// Subscribe to the static event
ICalculatorService.Calculated += value => Console.WriteLine($"Result: {value}");

// Raise the static event
sut.Mock.RaiseStatic.Calculated(42);

// Verify event subscriptions
sut.Mock.VerifyStatic.Calculated.Subscribed().Once();
sut.Mock.VerifyStatic.Calculated.Unsubscribed().Never();
```

## Async-flow scoping

Static member invocations use async-flow scoping (`AsyncLocal<MockRegistration>`) to route static member calls to
the correct mock instance. This means:

- Each mock instance has its own isolated static member context, even within the same process.
- Parallel test execution is safe: each test's mock setup and verification are fully isolated.
- The static member context is scoped to the current async call chain. Static member calls in unrelated tasks or
  threads will not see the mock setup.

```csharp
// These can run in parallel safely
await Task.WhenAll(
    Task.Run(async () =>
    {
        ICalculatorService sut = ICalculatorService.CreateMock();
        sut.Mock.SetupStatic.Add(It.IsAny<int>(), It.IsAny<int>()).Returns(1);
        // static Add() returns 1 in this context
    }),
    Task.Run(async () =>
    {
        ICalculatorService sut = ICalculatorService.CreateMock();
        sut.Mock.SetupStatic.Add(It.IsAny<int>(), It.IsAny<int>()).Returns(2);
        // static Add() returns 2 in this context
    })
);
```

**Notes:**

- Static interface members require .NET 8 or later.
- Only `static abstract` and `static virtual` members on interfaces can be mocked.
- Static class members (non-interface) cannot be mocked.
- The `SetupStatic` and `VerifyStatic` properties are only generated when the interface has static abstract/virtual members.
