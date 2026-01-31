# Methods

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

## Async Methods

For `Task<T>` or `ValueTask<T>` methods, use `.ReturnsAsync(…)` or `ThrowsAsync(…)`:

```csharp
sut.SetupMock.Method.DispenseAsync(It.IsAny<string>(), It.IsAny<int>())
    .ReturnsAsync((_, v) => v)            // First execution returns the value of the `int` parameter
	.ThrowsAsync(new TimeoutException())  // Second execution throws a TimeoutException;
	.ReturnsAsync(0).Forever();           // Subsequent executions return 0
```
