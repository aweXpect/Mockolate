# Scenarios

Scenarios let you define multiple sets of setups on a single mock and switch between them at runtime. This is useful
when the collaborator behaves differently depending on its internal state &mdash; for example a connection that starts
disconnected, becomes connected after `Connect()`, and times out after a failure.

A mock always has an *active scenario* (the empty string `""` by default). When a member is accessed, Mockolate looks
for a matching setup in the active scenario first, then falls back to the default scope.

## Defining scenarios

Use `InScenario(name)` to scope setups to a named scenario. It returns a scope whose `Setup` property mirrors
`sut.Mock.Setup` but targets the scenario's bucket:

```csharp
sut.Mock.InScenario("disconnected").Setup.Ping().Throws<TimeoutException>();
sut.Mock.InScenario("connected").Setup.Ping().Returns(true);
```

A callback overload batches multiple setups into the same scenario:

```csharp
sut.Mock.InScenario("connected", scope =>
{
    scope.Setup.Ping().Returns(true);
    scope.Setup.Send(It.IsAny<string>()).Returns(true);
});
```

Setups registered via `InScenario` do **not** leak into the default scope.

## Switching scenarios

Chain `.TransitionTo(name)` on any method, property, or indexer setup to change the active scenario when the setup
fires. The transition runs as a parallel side-effect &mdash; it does not replace the return value or throw behaviour.

```csharp
sut.Mock.InScenario("disconnected")
    .Setup.Connect()
    .Returns(true)
    .TransitionTo("connected");

sut.Mock.InScenario("connected")
    .Setup.Ping()
    .Throws<TimeoutException>()
    .TransitionTo("disconnected");
```

You can also change the active scenario manually via `sut.Mock.TransitionTo("connected");`, which is useful for
arranging the starting state.

### Resolution rules

When dispatching a call, Mockolate looks up setups in this order:

1. The active scenario's bucket (if non-empty).
2. The default bucket (setups registered via `sut.Mock.Setup.*`).
3. The mock's default behaviour.

Scenario setups add to, rather than replace, the default scope &mdash; register catch-alls in the default scope and
override specific members per scenario.
