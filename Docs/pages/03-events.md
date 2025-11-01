# Mock events

Easily raise events on your mock to test event handlers in your code.

## Usage

Use the strongly-typed `Raise` property on your mock to trigger events declared on the mocked interface or class. The method signature matches the event delegate.

```csharp
// Arrange: subscribe a handler to the event
mock.Subject.UsersChanged += (sender, args) => { /* handler code */ };

// Act: raise the event
mock.Raise.UsersChanged(this, EventArgs.Empty);
```

- Use the `Raise` property to trigger events declared on the mocked interface or class.
- Only currently subscribed handlers will be invoked.
- Simulate notifications and test event-driven logic in your code.

## Example

```csharp
int callCount = 0;
mock.Subject.UsersChanged += (sender, args) => callCount++;

mock.Raise.UsersChanged(this, EventArgs.Empty);
mock.Raise.UsersChanged(this, EventArgs.Empty);

// callCount == 2
```

You can subscribe and unsubscribe handlers as needed. Only handlers subscribed at the time of raising the event will be called.
