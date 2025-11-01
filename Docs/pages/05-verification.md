# Verification

Verify that methods or properties were called with specific arguments and how many times:

```csharp
mock.Verify.Invoked.AddUser("Bob").AtLeastOnce();
mock.Verify.Invoked.TryDelete(id, With.Out<User?>()).Never();
mock.Verify.Invoked.DoSomething(With.Any<int>()).Exactly(2);
```

- Supports `.Never()`, `Once()`, `Twice()`, `Exactly(n)`, `.AtLeastOnce()`, `.AtLeastTwice()`, `.AtLeast(n)`, `.AtMostOnce()`, `.AtMostTwice()`, `.AtMost(n)` for call count verification.
- Verify arguments with matchers.


## Call Ordering
Use `Then` to verify that calls occurred in a specific order:

```csharp
mock.Verify.Invoked.AddUser("Alice").Then(
    m => m.Invoked.DeleteUser("Alice")
);
```
