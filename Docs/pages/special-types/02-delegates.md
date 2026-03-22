# Delegates

Mockolate supports mocking delegates including `Action`, `Func<T>`, and custom delegates.

## Setup

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
- Full [parameter matching](https://awexpect.com/docs/mockolate/setup#parameter-matching) support for delegate
  parameters including `ref` and `out` parameters.

## Verification

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
Delegate parameters also
support [argument matchers](https://awexpect.com/docs/mockolate/verify-interactions#argument-matchers).
