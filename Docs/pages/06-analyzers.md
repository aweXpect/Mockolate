# Analyzers

Mockolate ships with some Roslyn analyzers to help you adopt best practices and catch issues early, at compile time.
All rules provide actionable messages and link to identifiers for easy filtering.

## Mockolate0001

`Verify` methods only return a `VerificationResult` and do not directly throw. You have to specify how often you expect
the call to happen, e.g. `.AtLeastOnce()`, `.Exactly(n)`, etc. or use the verification result in any other way.

**Example:**

```csharp
var sut = Mock.Create<IChocolateDispenser>();
sut.Dispense("Dark", 1);
// Analyzer Mockolate0001: Add a count assertion like .AtLeastOnce() or use the result.
sut.VerifyMock.Invoked.Dispense(It.Is("Dark"), It.IsAny<int>());
```

The included code fixer suggests to add the `.AtLeastOnce()` count assertion:

```csharp
sut.VerifyMock.Invoked.Dispense(It.Is("Dark"), It.IsAny<int>()).AtLeastOnce();
```

## Mockolate0002

Mock arguments must be mockable (interfaces or supported classes).
This rule will prevent you from using unsupported types (e.g. sealed classes) when using `Mock.Create<T>()`.

## Mockolate0003

Wrap arguments must be interfaces.
This rule will prevent you from using any other type when using `Mock.Wrap<T>(T instance)`.
