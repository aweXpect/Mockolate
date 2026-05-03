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

A mocked member's signature routes through the ref-struct pipeline in a way Mockolate can't
emit setup surface for. The warning fires in two distinct situations.

**1. Compilation prerequisites not met**

The ref-struct setup pipeline requires both:

- A target framework of .NET 9 or later (Mockolate's ref-struct setup types are
  `#if NET9_0_OR_GREATER`-gated).
- An effective C# language version of 13 or later (uses the `allows ref struct` anti-constraint).

When either prerequisite is missing, the warning fires for any member that passes a non-`Span<T>` /
non-`ReadOnlySpan<T>` ref struct by value, or uses one as an indexer key. Upgrade the target
framework and/or `<LangVersion>` to resolve it.

**2. Signature shapes that are never supported**

These fire on every compilation target, including .NET 9+ / C# 13+:

- Parameters marked `out`, `ref`, or `ref readonly` whose type is a non-`Span<T>` /
  non-`ReadOnlySpan<T>` ref struct - the mock can't round-trip the value through
  `IOutParameter<T>` / `IRefParameter<T>` when `T` is a ref struct.
- Methods returning a non-`Span<T>` / non-`ReadOnlySpan<T>` ref struct.

**Note:**
`Span<T>` and `ReadOnlySpan<T>` flow through the existing `SpanWrapper` / `ReadOnlySpanWrapper`
fallback and are never flagged. On .NET 9+ with C# 13+, by-value custom ref-struct parameters and
ref-struct-keyed indexers (getter-only, setter-only, and get+set) are fully supported.

See the [Ref Struct Parameters](setup/parameter-matching#ref-struct-parameters-net-9) section
for the supported surface.
