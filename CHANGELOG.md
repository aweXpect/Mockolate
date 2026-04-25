# Changelog

All notable changes to **Mockolate** are documented here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and this project
adheres to [Semantic Versioning](https://semver.org/).

## [3.0.0] — Unreleased

The **D refactor** release. Per-member typed interaction storage and
member-id-keyed setup dispatch make the hot path several times faster while
keeping the public fluent surface unchanged. See
[`Docs/pages/performance.md`](Docs/pages/performance.md) for measured speedups
and [`Docs/pages/migration-v3.md`](Docs/pages/migration-v3.md) for the full
migration guide.

### Added

- `MockRegistry.GetMethodSetupSnapshot(int memberId)` — arity-agnostic, lock-free
  fast-path accessor used by generator-emitted proxy bodies.
- `MockRegistry.SetupMethod(int memberId, MethodSetup)` and
  `SetupMethod(int memberId, string scenario, MethodSetup)` — member-id-keyed
  setup registration.
- Member-id-keyed overloads of `MockRegistry.GetProperty`, `SetProperty`,
  `AddEvent`, `RemoveEvent`, `IndexerGot`, `IndexerSet`, `SubscribedTo`,
  `UnsubscribedFrom`, `VerifyMethod`, `VerifyProperty`.
- `MockRegistry` constructor accepting `IMockInteractions` for custom backing
  stores.
- `Mockolate.Interactions.FastMockInteractions` plus per-member typed buffers
  (`FastMethod0Buffer`, `FastMethodNBuffer<T...>`, `FastPropertyGetterBuffer`,
  `FastPropertySetterBuffer<T>`, `FastIndexerGetterBuffer<T...>`,
  `FastIndexerSetterBuffer<T..., TValue>`, `FastEventBuffer`) and the
  `IFastMemberBuffer` contract. The generator emits typed `Append` calls on
  the hot path; the buffers box to existing interaction types lazily on
  enumeration.
- `IMockInteractions` widened to expose `Count`, `IEnumerable<IInteraction>`,
  `SkipInteractionRecording`, `InteractionAdded` / `OnClearing` events,
  `Clear()`, `GetUnverifiedInteractions()`, and an internal `Verified` hook.
- `IVerificationResult.Interactions` returning `IMockInteractions`.

### Changed

- **Breaking:** `MockRegistry.Interactions` returns `IMockInteractions` (was
  `MockInteractions`).
- **Breaking:** `MockRegistry.GetUnusedSetups(...)` accepts `IMockInteractions`
  (was `MockInteractions`).
- **Breaking:** Abstract `Matches` on `ReturnMethodSetup<T...>` /
  `VoidMethodSetup<T...>` (and the ref-struct variants) no longer takes
  parameter-name strings — only the values. The `WithParameters` nested ctor
  now accepts the parameter names up front and stores them for the
  `INamedParametersMatch` branch.
- **Breaking:** `MethodInvocation<T...>` and
  `IndexerGetterAccess<T...>` / `IndexerSetterAccess<T...>` constructors and
  their `ParameterName1..N` properties dropped. Recorded interactions still
  expose values via `Parameter1..N` and `GetParameterValueAt(int)`.
- **Breaking:** `IParametersMatch.Matches` now takes `ReadOnlySpan<object?>`
  (was `object?[]`) and `INamedParametersMatch.Matches` now takes
  `ReadOnlySpan<(string, object?)>` (was `(string, object?)[]`). Removes the
  per-call array allocation on the `WithParameters` matching path.
- Generator-emitted proxy method bodies now dispatch through
  `GetMethodSetupSnapshot(memberId)` for default-scope calls; the legacy
  closure-based `GetMethodSetup<T>(string, Func<T, bool>)` is reserved for
  scenario-scoped lookups and string-keyed registrations
  (e.g. `HttpClientExtensions.SetupMethod`).
- Generator-emitted recording calls dispatch directly to typed buffer `Append`
  methods instead of allocating a new `MethodInvocation` / `Access` per call.
- `Verify` walks now scan only the target member's buffer (not the global
  interaction list).

### Removed

- `MockMonitor` and `MockMonitor<T>` constructors taking `MockInteractions`
  (the v3.0-preview `[Obsolete]` shims). Use the `IMockInteractions` overloads.
- `VerificationResult<TVerify>` constructors taking `MockInteractions` (same).
- The hand-written D-preview benchmark classes
  (`OptimizedDMock`, `OptimizedDAllMembersMock`, `OptimizedHandwrittenMock`)
  superseded by the real runtime implementation.

### Deprecated

- `IVerificationResult.MockInteractions` — use `IVerificationResult.Interactions`
  instead.

### Performance

| Member | v2 baseline | v3 (D) target | Speedup | Allocation reduction |
|---|---:|---:|---:|---:|
| Method (2-arg) | ~189 ns · ~300 B | ~142 ns · ~136 B | 1.3× | 2.2× |
| Property get | ~287 ns · ~260 B | ~55 ns · ~48 B | 5.2× | 5.4× |
| Indexer get (1 key) | ~298 ns · ~172 B | ~38 ns · ~69 B | 7.8× | 2.5× |
| Event subscribe | ~185 ns · ~369 B | ~61 ns · ~192 B | 3.0× | 1.9× |

Marginal per-call cost extracted as `(N=10 − N=1) / 9` from
`OptimizedMockComparisonBenchmarks` on AMD Ryzen 7 PRO 8840HS / .NET 10
Release / `InProcessEmit`. See [`Docs/pages/performance.md`](Docs/pages/performance.md).

[3.0.0]: https://github.com/aweXpect/Mockolate/releases/tag/v3.0.0
