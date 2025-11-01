# Event Raising

Easily raise events on your mock to test event handlers in your code:

```csharp
mock.Raises.UsersChanged(this, EventArgs.Empty);
```

- Use the `Raises` property to trigger events declared on the mocked interface or class.
- Simulate notifications and test event-driven logic.
