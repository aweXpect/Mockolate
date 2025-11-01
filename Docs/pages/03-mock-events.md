# Mock events

Easily raise events on your mock to test event handlers in your code.

## Raise

Use the strongly-typed `Raise` property on your mock to trigger events declared on the mocked interface or class. The method signature matches the event delegate.

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

You can subscribe and unsubscribe handlers as needed. Only handlers subscribed at the time of raising the event will be called.
