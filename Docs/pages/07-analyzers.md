# Analyzers

Mockolate ships with some Roslyn analyzers to help you adopt best practices and catch issues early, at compile time.
All rules provide actionable messages and link to identifiers for easy filtering.

## Mockolate0001

`Verify` methods only return a `VerificationResult` and do not directly throw. You have to specify how often you expect
the call to happen, e.g. `.AtLeastOnce()`, `.Exactly(n)`, etc. or use the verification result in any other way.

**Example:**

```csharp
IChocolateDispenser sut = IChocolateDispenser.CreateMock();
sut.Dispense("Dark", 1);
// Analyzer Mockolate0001: Add a count assertion like .AtLeastOnce() or use the result.
sut.Mock.Verify.Dispense(It.Is("Dark"), It.IsAny<int>());
```

The included code fixer suggests to add the `.AtLeastOnce()` count assertion:

```csharp
sut.Mock.Verify.Dispense(It.Is("Dark"), It.IsAny<int>()).AtLeastOnce();
```

## Mockolate0002

Mocked types must be mockable. This rule will prevent you from using unsupported types:

- `CreateMock()`  
  Type must be an interface, a delegate or a supported class (e.g. not sealed)
- `Implementing<T>()`  
  Type must be an interface

## Mockolate0003

Ref-struct parameter mocking is not supported on this compilation. This warning fires when the
signature of a mocked member routes through the ref-struct pipeline but the current build
environment can't emit the setup surface:

- Compilation target is older than .NET 9 — the ref-struct pipeline requires the `allows ref struct`
  anti-constraint introduced in C# 13 / .NET 9.
- Parameter is `out`, `ref`, or `ref readonly` and its type is a non-`Span<T>` / non-`ReadOnlySpan<T>`
  ref struct — the mock can't round-trip the value through `IOutParameter<T>` / `IRefParameter<T>`
  when `T` is a ref struct.
- Method returns a non-`Span<T>` / non-`ReadOnlySpan<T>` ref struct — currently unsupported.

`Span<T>` and `ReadOnlySpan<T>` flow through the existing `SpanWrapper` / `ReadOnlySpanWrapper`
fallback and are not flagged. Custom ref-struct parameters and indexer keys (both get and set) ARE
supported on .NET 9+ compilation targets.

See the [Ref Struct Parameters](setup/parameter-matching#ref-struct-parameters-net-9) section
for the supported surface.
