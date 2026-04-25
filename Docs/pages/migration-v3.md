# Migrating to Mockolate v3.0

Mockolate v3.0 ships the **D refactor** — per-member typed interaction storage that
makes the hot path several times faster. Most consumers do not need to change anything:
mocks created via `T.CreateMock()`, fluent setups (`sut.Mock.Setup.X(...)`),
verifications (`sut.Mock.Verify.X(...)`), monitors and scenarios all behave identically.

This guide lists the public API breaks introduced in v3.0 and how to update each one.
See [`Docs/pages/performance.md`](./performance.md) for the measured speedups.

## Quick reference

| Area | What changed |
|---|---|
| `MockRegistry.Interactions` | Now returns `IMockInteractions` (was `MockInteractions`) |
| `MockRegistry.GetUnusedSetups(...)` | Parameter type widened to `IMockInteractions` |
| `IMockInteractions` interface | Widened to expose count, enumeration, events, `Clear`, `GetUnverifiedInteractions`, `SkipInteractionRecording` |
| `IVerificationResult.MockInteractions` | Removed — use `IVerificationResult.Interactions` |
| `MockMonitor` / `MockMonitor<T>` ctors | `MockInteractions` overloads removed; only the `IMockInteractions` overloads remain |
| `VerificationResult<TVerify>` ctors | `MockInteractions` overloads removed; only the `IMockInteractions` overloads remain |
| `ReturnMethodSetup<T...>.Matches` / `VoidMethodSetup<T...>.Matches` | Abstract signature no longer takes parameter-name strings |
| `WithParameters` nested ctor | Now takes the parameter-name strings up front |
| `MethodInvocation<T...>` ctor / properties | `parameterName*` ctor parameters and `ParameterName*` properties removed |
| `IndexerGetterAccess<T...>` / `IndexerSetterAccess<T...>` ctor / properties | Same as `MethodInvocation` |
| `IParametersMatch.Matches` / `INamedParametersMatch.Matches` | Now take `ReadOnlySpan<...>` instead of `T[]` |
| `MockRegistry.GetMethodSetup<T>(string, Func<T, bool>)` | Removed — use `GetMethodSetupSnapshot(memberId)` (hot path) or `GetMethodSetups<T>(string)` (scenario / legacy) |

The library and the source generator move together; rebuilding a consuming project
against Mockolate v3.0 regenerates every `Mock.{TypeName}.g.cs` against the new shapes.

## `MockRegistry.Interactions` is now `IMockInteractions`

The most likely change to surface in user code. Anything that wrote
`MockInteractions x = sut.Mock.Interactions` no longer compiles.

```csharp
// v2
MockInteractions interactions = sut.Mock.Interactions;

// v3
IMockInteractions interactions = sut.Mock.Interactions;
```

`MockInteractions` still implements `IMockInteractions`, so values flowing back
into APIs that take the interface continue to work.

The previously `[Obsolete]` `IVerificationResult.MockInteractions` getter has
been removed entirely. Read `IVerificationResult.Interactions` instead — it
returns the live `IMockInteractions` view without materialising a snapshot
copy.

## `IMockInteractions` interface widened

The interface now mirrors the full read surface previously only on
`MockInteractions`. Custom implementations (uncommon — typically only
`aweXpect.Mockolate` does this) must add:

- `int Count { get; }` (via `IReadOnlyCollection<IInteraction>`)
- `IEnumerator<IInteraction> GetEnumerator()` (via `IEnumerable<IInteraction>`)
- `bool SkipInteractionRecording { get; }`
- `event EventHandler? InteractionAdded`
- `event EventHandler? OnClearing`
- `void Clear()`
- `IReadOnlyCollection<IInteraction> GetUnverifiedInteractions()`
- `void Verified(IEnumerable<IInteraction> interactions)` (internal)

If you don't implement `IMockInteractions` yourself, no action is required.

## `MockMonitor` and `VerificationResult<TVerify>` constructors

The `[Obsolete]` `MockInteractions`-typed shim constructors that v3.0-preview
kept around as a soft transition have been removed. Use the `IMockInteractions`
overloads:

```csharp
// v3
public sealed class MyMonitor : MockMonitor
{
    public MyMonitor(IMockInteractions interactions) : base(interactions) { }
}

VerificationResult<TVerify> result = new(verify, interactions, predicate, expectation);
```

`MockInteractions` already implements `IMockInteractions`, so existing
construction sites continue to compile if they pass a `MockInteractions`
instance — only the parameter type on the constructor changed.

## `Matches` no longer threads parameter-name strings

The abstract `Matches` method on `ReturnMethodSetup<T...>` and `VoidMethodSetup<T...>`
(and the ref-struct variants) takes only the values now. Parameter names are
captured at setup-construction time on the `WithParameters` nested class.

```csharp
// v2
public override bool Matches(string p1Name, T1 p1Value, string p2Name, T2 p2Value)
    => MyMatch(p1Value, p2Value);

// v3
public override bool Matches(T1 p1Value, T2 p2Value)
    => MyMatch(p1Value, p2Value);
```

If you subclass `WithParameters`, pass the parameter names through its constructor:

```csharp
// v3
public sealed class MySetup : ReturnMethodSetup<bool, int, string>.WithParameters
{
    public MySetup(MockRegistry registry, IParameters parameters)
        : base(registry, "MyMethod", parameters, "value", "name")
    {
    }
}
```

The fluent surface (`sut.Mock.Setup.MyMethod(value, name)`) is unaffected — the
generator emits the new ctor invocations.

## `MethodInvocation<T...>` and `IndexerAccess` lose `ParameterName*`

`ParameterName1..N` properties and the corresponding ctor parameters are gone.
Code constructing these types directly (rare — typically only test helpers) must
drop the names:

```csharp
// v2
new MethodInvocation<int, string>("MyMethod", "value", 42, "name", "hello");
new IndexerGetterAccess<string>("key", "Dark");

// v3
new MethodInvocation<int, string>("MyMethod", 42, "hello");
new IndexerGetterAccess<string>("Dark");
```

Recorded interactions continue to expose the values via `Parameter1..N` and
`GetParameterValueAt(int)` — only the parameter-name surface was removed.

## `IParametersMatch.Matches` and `INamedParametersMatch.Matches` take spans

Both interfaces now receive their values as a `ReadOnlySpan<...>` rather than a
heap-allocated array. The runtime no longer allocates the array on the hot path
when a `WithParameters`-style setup is matched.

```csharp
// v2
public sealed class MyMatch : IParametersMatch
{
    public bool Matches(object?[] values) => values.Length == 2 && /*...*/;
}

public sealed class MyNamedMatch : INamedParametersMatch
{
    public bool Matches((string, object?)[] values) => /*...*/;
}

// v3
public sealed class MyMatch : IParametersMatch
{
    public bool Matches(ReadOnlySpan<object?> values) => values.Length == 2 && /*...*/;
}

public sealed class MyNamedMatch : INamedParametersMatch
{
    public bool Matches(ReadOnlySpan<(string, object?)> values) => /*...*/;
}
```

If a custom matcher needs an array (e.g. to keep a snapshot for later), call
`values.ToArray()` inside `Matches` — the cost is now paid only on that branch
rather than on every match attempt.

## `MockRegistry.GetMethodSetup<T>(string, Func<T, bool>)` removed

The closure-taking lookup is gone. Generated mock proxies now fall back to
the lock-free `GetMethodSetups<T>(string)` enumeration when the member-id
snapshot misses, and the loop applies the matcher inline (no closure
allocation, no string-name compare under a lock):

```csharp
// v2 — closure path
TSetup? setup = mockRegistry.GetMethodSetup<TSetup>("name", m => m.Matches(arg1, arg2));

// v3 — enumerate and stack-evaluate the matcher
TSetup? setup = null;
foreach (TSetup s in mockRegistry.GetMethodSetups<TSetup>("name"))
{
    if (s.Matches(arg1, arg2))
    {
        setup = s;
        break;
    }
}
```

Custom code that called the removed overload should switch to the
enumeration form above. `GetMethodSetupSnapshot(int memberId)` remains the
default-scope fast path for code that has a member id available.

## What is *not* breaking

- `T.CreateMock()`, `sut.Mock.Setup.X(...)`, `sut.Mock.Verify.X(...)`,
  `sut.Mock.Monitor()`, `sut.Mock.InScenario(...)`, `Mock.Raise.*`,
  `It.*` matchers, `Match.*`, `MockBehavior` — unchanged.
- `MockInteractions` itself remains a public type and continues to be accepted
  anywhere `IMockInteractions` is now required.
- `MockRegistry.GetMethodSetupSnapshot(int memberId)` is the default-scope hot
  path; the generator emits this for every member-id-keyed call site.
