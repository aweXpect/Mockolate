# Scenarios

Scenarios let you define multiple sets of setups on a single mock and switch between them at runtime. This is useful
when the system under test interacts with a collaborator that behaves differently depending on its internal state —
for example, a connection that starts disconnected, becomes connected after `Connect()`, and times out after a
failure.

Think of scenarios as named buckets of setups. A mock always has an *active scenario* (the empty string `""` by
default). When a method, property, indexer, or event is accessed, Mockolate first looks for a matching setup in the
active scenario's bucket, then falls back to the default bucket.

## Defining scenarios

Use `InScenario(name)` to register setups inside a named scenario. It returns a scope that exposes a `Setup` property
whose behaviour mirrors `sut.Mock.Setup`, but targets the named scenario's bucket:

```csharp
IConnection sut = IConnection.CreateMock();

sut.Mock.InScenario("disconnected").Setup.Ping().Throws<TimeoutException>();
sut.Mock.InScenario("connected").Setup.Ping().Returns(true);
```

There is also an overload that takes a callback — useful when you want to register several setups in the same
scenario:

```csharp
sut.Mock.InScenario("connected", scope =>
{
    scope.Setup.Ping().Returns(true);
    scope.Setup.Send(It.IsAny<string>()).Returns(true);
});
```

Setups registered via `InScenario` do **not** leak into the default scope — `sut.Mock.Setup.Ping()` and
`sut.Mock.InScenario("connected").Setup.Ping()` register into separate buckets.

## Switching scenarios from a setup

Use `.TransitionTo(name)` on any method, property, or indexer setup callback chain to change the active scenario when
that setup fires. The transition is a side-effect that runs in parallel with the setup's other callbacks — it does not
replace the return value or throw behaviour.

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

In this example, calling `Connect()` while in the `"disconnected"` scenario returns `true` and flips the active
scenario to `"connected"`. Subsequent calls to `Ping()` throw `TimeoutException` and transition back to
`"disconnected"`.

## Switching scenarios manually

You can also set the active scenario directly by assigning `MockRegistry.Scenario`:

```csharp
((IMock)sut).MockRegistry.Scenario = "connected";
```

This is handy for test arrangement when the starting state is something other than the default.

## Resolution rules

When dispatching a call:

1. If the active scenario is non-empty, Mockolate looks for a matching setup inside that scenario's bucket.
2. If no match is found, Mockolate falls back to the default bucket (setups registered via `sut.Mock.Setup.*`).
3. If no match is found there either, the default behaviour of the mock applies (see
   [`MockBehavior`](../01-create-mocks.md)).

This means scenario setups *add to*, rather than replace, the default scope — you can register a catch-all in the
default scope and override specific methods per scenario.
