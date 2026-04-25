# D refactor — implementation plan

A multi-step plan for landing the per-member-buffer optimization ("D") plus its
prerequisite cleanups (A + B + C + E + F) in the Mockolate runtime and source
generator. Designed to ship as part of v3.0.

## Context

Today every mocked method call takes ~355 ns and allocates ~307 B on the hot
path. Benchmarks and profiling trace that cost to three structural choices in
the current implementation:

1. **String-keyed setup dispatch** — `MockRegistry.GetMethodSetup<T>` walks a
   shared `List<MethodSetup>` under a lock, running `Name.Equals` + type check +
   user predicate on every entry. The predicate is a closure that captures the
   call arguments (one display-class + delegate allocation per invocation).
2. **Shared `List<IInteraction>` recording** — every call locks
   `MockInteractions._listLock` and appends a heap-allocated
   `MethodInvocation<T1..TN>` that carries parameter **name** strings in
   addition to the values. Verify walks the full list.
3. **Parameter-name threading through the `Matches` contract** — every
   `ReturnMethodSetup<TReturn, T1, T2>.Matches(string, T1, string, T2)` call
   receives two dead strings in the common `WithParameterCollection` case.
   Forces callers to construct a closure to re-capture them.

The hand-written sketch in `Benchmarks/Mockolate.Benchmarks/OptimizedDMock.cs`
and `OptimizedDAllMembersMock.cs` proves a faster design is reachable without
losing any feature (matchers, `Callbacks<T>`, `VerificationResult<T>`,
`MockMonitor`, scenarios, unused-setup detection).

Measured impact across member kinds (per-call marginal cost,
delta N=1 → N=10 / 9, AMD Ryzen 7 PRO 8840HS, .NET 10, Release/InProcessEmit):

| Member | Today (Mockolate) | With D | Speedup | Alloc reduction |
|---|---:|---:|---:|---:|
| Method (2-arg) | ~189 ns · ~300 B | ~142 ns · ~136 B | 1.3× | 2.2× |
| Property get | ~287 ns · ~260 B | ~55 ns · ~48 B | 5.2× | 5.4× |
| Indexer get (1 key) | ~298 ns · ~172 B | ~38 ns · ~69 B | 7.8× | 2.5× |
| Event subscribe | ~185 ns · ~369 B | ~61 ns · ~192 B | 3.0× | 1.9× |

## Guiding principles

- **Every commit leaves CI green.** Tests pass, API snapshots updated, coverage
  ≥90%, zero SonarCloud issues.
- **Group work by transformation, not by member kind.** Methods, properties,
  indexers, events all move together per step — no split state where one kind
  lives in the old model while others have migrated.
- **Cluster public-API breaks into designated steps.** Slot those commits into
  the v3.0 release window.
- **Measure after every perf-affecting step.** Run the benchmark comparison
  harness; a staircase of wins is expected (Step 4.2 → Step 5.3 → Step 6.1).
  When updating the perf-baseline-v2.md file, also include the raw benchmarks results for reference.
- **Runtime stays arity-agnostic.** Every arity-sensitive change touches both
  the hand-written arity-0-4 concrete classes and the generator's
  arity-N synthesis loop in one pass. No runtime library method has
  `<T, T1, ..., TN>` overloads.

## Arity coverage

Mockolate already supports arbitrary arity through a split between hand-written
library types and generator-synthesized consumer-assembly types:

| Scope | Covers | Where |
|---|---|---|
| Hand-written in the runtime library | Arities **0–4** | `Source/Mockolate/Setup/ReturnMethodSetup.cs`, `VoidMethodSetup.cs`, `Source/Mockolate/Interactions/MethodInvocation.cs`, `IndexerGetterAccess.cs`, `IndexerSetterAccess.cs` |
| Synthesized by the source generator | Arities **5+** (per-consumer, on demand) | `Source/Mockolate.SourceGenerators/Sources/Sources.MethodSetups.cs:104-145, 153+, 780+`, `Sources.IndexerSetups.cs:116+` |

Every step below that touches arity touches both surfaces in one commit. The
runtime library does not gain new overloads with T-parameter counts — the
arity fan-out lives in generator-emitted code, against an arity-agnostic
runtime helper:

```csharp
// Runtime: arity-agnostic
public MethodSetup[]? GetMethodSetupSnapshot(int memberId)
    => Volatile.Read(ref _setupsByMemberId[memberId]);

// Generator-emitted proxy body: typed for this call site, any arity works
var snapshot = this._mockRegistry.GetMethodSetupSnapshot(MemberId_Baz);
ReturnMethodSetup<TReturn, T1, T2, T3, T4, T5>? setup = null;
if (snapshot is not null)
{
    for (int __i = snapshot.Length - 1; __i >= 0; __i--)
    {
        if (snapshot[__i] is ReturnMethodSetup<TReturn, T1, T2, T3, T4, T5> __s
            && __s.Matches(p1, p2, p3, p4, p5))
        {
            setup = __s;
            break;
        }
    }
}
```

The generator-synthesized buffer types (`FastMethodNBuffer<T...>` for arity 5+)
implement the arity-agnostic `IFastMemberBuffer` interface, so
`FastMockInteractions` never needs to pattern-match on concrete arity.

---

## Phase 0 — Baseline + guardrails (1 commit)

### Step 0.1 · `perf(bench): add D-refactor comparison benchmarks`

- Port `OptimizedDMock.cs`, `OptimizedDAllMembersMock.cs`, and
  `OptimizedMockComparisonBenchmarks.cs` from the benchmark project as a
  permanent reference harness. Tag the D-optimized classes as "preview" so
  reviewers know they'll be removed in Step 7.3.
- Record baseline numbers in `Docs/perf-baseline-v2.md` so later steps measure
  against a fixed reference.
- **Public API:** none.
- **Tests:** benchmarks build; smoke tests in `[GlobalSetup]` pass.
- **Size:** ~1 day.

---

## Phase 1 — Signature cleanup (3 commits)

Public-API breaks clustered here, targeting the v3.0 release. Each commit
applies its transformation to every member kind in one pass.

### Step 1.1 · `refactor!: move parameter names from Matches() to setup construction`

Eliminates parameter-name threading through the abstract `Matches` contract.
After this step the "MatchesFast" method from the handwritten benchmark **is**
just `Matches` — no new API needed.

**Runtime library:**

- `Source/Mockolate/Setup/ReturnMethodSetup.cs` — `abstract bool Matches(string p1Name, T1 p1Value, ...)` → `abstract bool Matches(T1 p1Value, ...)` for the hand-written arities 0-4 classes.
- `Source/Mockolate/Setup/VoidMethodSetup.cs` — same, arities 0-4.
- `Source/Mockolate/Setup/IndexerSetup.cs` — `IndexerGetterSetup`, `IndexerSetterSetup`, arities 1-4.
- `Source/Mockolate/Setup/RefStructReturnMethodSetup.cs` + `RefStructVoidMethodSetup.cs` + `RefStructIndexerGetterSetup.cs` + `RefStructIndexerSetterSetup.cs` — same.
- `WithParameters` nested subclasses gain `string _parameterName1, _parameterName2, ...` fields passed via ctor; `Matches(T1, ...)` reads them from the field rather than from parameters. The `INamedParametersMatch` branch continues to build its `[(name, value), ...]` tuple array using the stored names.
- `MatchesInteraction(IMethodInteraction)` simplifies — pass only `invocation.Parameter1, invocation.Parameter2, ...` to `Matches`.

**Source generator (same commit — covers arities 5+):**

- `Source/Mockolate.SourceGenerators/Sources/Sources.MethodSetups.cs` — change the generated `Matches` emission signature (lines 650+, 753+). Every `WithParameters` ctor invocation now includes the parameter-name strings.
- `Source/Mockolate.SourceGenerators/Sources/Sources.MockClass.cs:2329-2333` — generated closure changes from `m => m.Matches("value", value, "name", name)` to `m => m.Matches(value, name)`.
- `Source/Mockolate.SourceGenerators/Sources/Sources.IndexerSetups.cs` — same transformation.
- `Source/Mockolate.SourceGenerators/Sources/Sources.RefStructMethodSetups.cs` — same.

**Tests:**

- `Tests/Mockolate.Tests` — no test changes needed (public fluent API unchanged).
- `Tests/Mockolate.SourceGenerators.Tests` — update generator-output snapshots.
- `Tests/Mockolate.Api.Tests` — API snapshot update (abstract method signature changed).

**Public API impact:** breaking — affects direct subclasses of `ReturnMethodSetup<T...>`, `VoidMethodSetup<T...>`, `IndexerGetterSetup<T...>`, etc. Not documented extension points, but public types.

**Size:** ~1 day. Most of the effort is the arity fan-out in the generator.

### Step 1.2 · `refactor!: drop ParameterName* from MethodInvocation and IndexerAccess types`

Once Step 1.1 lands, nothing reads `ParameterName1`/`ParameterName2`/etc. on
recorded interactions. Drop the fields — the "E-revised" optimization landed
without a new `SlimMethodInvocation` type.

**Runtime library:**

- `Source/Mockolate/Interactions/MethodInvocation.cs` — remove `ParameterName1..N` properties and ctor parameters for arities 1-4.
- `Source/Mockolate/Interactions/IndexerGetterAccess.cs` — same, arities 1-4.
- `Source/Mockolate/Interactions/IndexerSetterAccess.cs` — same, arities 1-4.
- `Source/Mockolate/Interactions/RefStructMethodInvocation.cs` — same.
- `Source/Mockolate/Setup/ReturnMethodSetup.cs` `MatchesInteraction` — stops reading `invocation.ParameterName1`, passes values only.

**Source generator (same commit — covers arities 5+):**

- `Sources.MethodSetups.cs:104-145` — generated `MethodInvocation<T...>` class loses the `ParameterName*` fields, ctor params, and XML docs.
- `Sources.IndexerSetups.cs:116+` — same.
- `Sources.MockClass.cs:2350-2365` — `RegisterInteraction(new MethodInvocation<T...>(name, "v", v, "n", n))` becomes `RegisterInteraction(new MethodInvocation<T...>(name, v, n))`.

**Tests:**

- Generator snapshot update.
- `Tests/Mockolate.Api.Tests` — API snapshot update.
- Any test that constructs `MethodInvocation<T...>` manually (search via `rg "new MethodInvocation<"`) needs updating.

**Public API impact:** breaking on `MethodInvocation<T...>`, `IndexerGetterAccess<T...>`, `IndexerSetterAccess<T...>` ctors and properties.

**Size:** ~half day.

### Step 1.3 · `refactor: cache INamedParametersMatch name/value buffer`

Follow-up from 1.1: `WithParameters.Matches` still builds
`[(p1Name, p1Value), (p2Name, p2Value)]` per call for the `INamedParametersMatch`
branch. Replace the per-call alloc with a reusable buffer on `WithParameters`,
or change `INamedParametersMatch.Matches` to accept
`ReadOnlySpan<(string, object?)>` and use a small stackalloc at the call site.

**Public API impact:** none (internal perf polish). Can slip to v3.1 if
schedule pressure hits.

**Size:** ~half day.

---

## Phase 2 — Widen `IMockInteractions` (1 commit, additive)

### Step 2.1 · `feat: widen IMockInteractions to cover the full read surface`

Today `IMockInteractions` is a 1-method interface only used internally. Widen
it so `VerificationResult<T>`, `MockMonitor`, and (future) `FastMockInteractions`
can all accept the interface.

**Runtime library:**

- `Source/Mockolate/Interactions/IMockInteractions.cs` — adds `Count`, `GetEnumerator()` (via `IReadOnlyCollection<IInteraction>`), `SkipInteractionRecording`, `InteractionAdded` event, `OnClearing` event, `Clear()`, `GetUnverifiedInteractions()`, internal `Verified(IEnumerable<IInteraction>)`.
- `Source/Mockolate/Interactions/MockInteractions.cs` — ensures it implements every new member (mostly renames/visibility tweaks).

**Tests:**

- `Tests/Mockolate.Internal.Tests` — tests that hold an `IMockInteractions` reference and call each new surface member.
- API snapshot update.

**Public API impact:** additive. Not breaking (unless some downstream class already implements `IMockInteractions` without the new members — unlikely).

**Size:** ~2 hours.

---

## Phase 3 — Plumb `IMockInteractions` through consumers (2 commits)

### Step 3.1 · `refactor: VerificationResult and MockMonitor accept IMockInteractions`

Add new constructors on `VerificationResult<TVerify>` and `MockMonitor` that
take `IMockInteractions`. Mark the existing `MockInteractions`-typed ctors
`[Obsolete]` with a forwarding shim — no behavior change.

**Runtime library:**

- `Source/Mockolate/Verify/VerificationResult.cs` — adds `ctor(TVerify, IMockInteractions, Func<IInteraction, bool>, string)` overload; existing `MockInteractions` overload marked obsolete, forwarding.
- `Source/Mockolate/Monitor/MockMonitor.cs` — same for the base ctor.
- `Source/Mockolate/MockRegistry.Verify.cs` — internal callers continue working because `MockInteractions` still satisfies `IMockInteractions`.

**Tests:** new interface-flavored constructor tests; no snapshot changes.

**Public API impact:** additive — new overloads; old ones obsolete but still work.

**Size:** ~2 hours.

### Step 3.2 · `feat!: MockRegistry.Interactions typed as IMockInteractions`

Change `MockRegistry.Interactions` to return `IMockInteractions` instead of
`MockInteractions`. Mildly breaking for code that writes
`MockInteractions x = sut.Mock.Interactions` but essential for enabling D.

Alternative non-breaking path: add `MockRegistry.InteractionsView` returning
`IMockInteractions`, keep `Interactions` as-is, have the D path use the new
view. Step 7.2 renames at v3 time.

**Public API impact:** mildly breaking (property type). Slot in with v3.

**Size:** ~1 hour + tests.

---

## Phase 4 — Member ids + fast setup dispatch (2 commits)

Implements optimizations A + B + C. No breaking changes.

### Step 4.1 · `feat(gen): emit member-id constants and member-id-keyed SetupMethod`

**Source generator:**

- For each mocked type, emit a const-int-per-member table into the proxy class:

  ```csharp
  internal const int MemberId_MyFunc = 0;
  internal const int MemberId_Counter_Get = 1;
  internal const int MemberId_Counter_Set = 2;
  internal const int MemberId_Indexer_Get = 3;
  internal const int MemberId_Indexer_Set = 4;
  internal const int MemberId_SomeEvent_Subscribe = 5;
  internal const int MemberId_SomeEvent_Unsubscribe = 6;
  internal const int MemberCount = 7;
  ```

- Assignment per kind: method = 1 id, property = 2 ids (get + set),
  indexer-signature = 2 ids, event = 2 ids (sub + unsub).
- Update the `Setup.MyFunc(...)` fluent emission to call a new
  `MockRegistry.SetupMethod(int memberId, MethodSetup setup)` in addition to
  the existing string-name `Methods.Add`.

**Runtime library:**

- `Source/Mockolate/MockRegistry.Setup.cs` — adds `SetupMethod(int memberId, MethodSetup setup)` overload; backing store is a new `MethodSetup[]?[] _setupsByMemberId` array sized from a generator-provided count. Lock-for-write via `Volatile.Write` publish; lock-free reads.
- `Source/Mockolate/MockRegistry.Interactions.cs` — adds arity-agnostic `GetMethodSetupSnapshot(int memberId) → MethodSetup[]?`. The typed scan happens in generator-emitted code against the snapshot.

**Tests:**

- Generator snapshot update (member-id constants present).
- New unit tests for `MockRegistry.GetMethodSetupSnapshot` covering hit, miss, scenario precedence.
- Existing behavior tests unchanged.

**Public API impact:** additive.

**Size:** ~2 days. Generator touches many call sites but pattern is mechanical.

### Step 4.2 · `perf(gen): dispatch mock method bodies through member ids`

Rewrites proxy method bodies to use the fast path: no closure, no name compare.

**Source generator:**

- `Sources.MockClass.cs:2329-2333` — before: `GetMethodSetup<T>("Method", m => m.Matches(v, n))`. After: fetch snapshot via `GetMethodSetupSnapshot(MemberId_Method)`, scan typed, call `s.Matches(v, n)` directly.
- Apply to method arities 0–4 in the hand-written setups and 5+ through the existing arity-N generator loop (the latter is the same code path).
- Same transformation for property getter/setter and indexer getter/setter.

**Runtime library:** no changes beyond Step 4.1.

**Tests:**

- All existing behavioral tests pass unchanged.
- Benchmark comparison shows the first measurable win — ~25-30% faster per call (matches the "feature-safe" variant in the handwritten sketch).

**Public API impact:** none (pure generator change).

**Size:** ~1 day. Focus on generator-output snapshot updates.

---

## Phase 5 — Fast per-member interaction storage (3 commits)

The core of D. Each commit lands a piece of infrastructure; the final commit
flips the generator onto it.

### Step 5.1 · `feat: add FastMockInteractions with per-kind per-member buffers`

New storage, not yet wired — lets reviewers focus on the storage type in
isolation.

**Runtime library (new files):**

- `Source/Mockolate/Interactions/FastMockInteractions.cs` — class implementing the full widened `IMockInteractions` surface. Backed by `IFastMemberBuffer[] Buffers`. Global monotonic seq via `Interlocked.Increment`. Ordered `GetEnumerator` merges buffers in seq order. `Clear` clears each buffer and resets the seq counter. `GetUnverifiedInteractions` tracks a verified-set against ordered snapshots.
- `Source/Mockolate/Interactions/IFastMemberBuffer.cs` — arity-agnostic per-member buffer contract (`Clear`, `AppendBoxed`).
- `Source/Mockolate/Interactions/FastMethodBuffer.cs` — hand-written arities 0-4: `FastMethod0Buffer`, `FastMethod1Buffer<T1>`, ..., `FastMethod4Buffer<T1..T4>`. Struct records `SlimCall0/1/2/3/4`. Lock-for-resize, `Volatile.Write(ref _count)` publish. `AppendBoxed(List<(long, IInteraction)>)` builds `MethodInvocation<T...>` (already slimmed in Step 1.2) for enumeration.
- `Source/Mockolate/Interactions/FastPropertyBuffer.cs` — `FastPropertyGetterBuffer` + `FastPropertySetterBuffer<T>`. Box to `PropertyGetterAccess` / `PropertySetterAccess<T>`.
- `Source/Mockolate/Interactions/FastIndexerBuffer.cs` — `FastIndexerGetterBuffer<T1..TN>` + `FastIndexerSetterBuffer<T1..TN, TValue>` (arities 1-4). Box to `IndexerGetterAccess<T...>` / `IndexerSetterAccess<T...>`.
- `Source/Mockolate/Interactions/FastEventBuffer.cs` — shared between sub and unsub (distinguished at construction). Box to `EventSubscription` / `EventUnsubscription`.

**Source generator:**

- Extend `Sources.MethodSetups.cs` / `Sources.IndexerSetups.cs` to also synthesize `FastMethodNBuffer<T1..TN> : IFastMemberBuffer` / `FastIndexerGetterBuffer<T1..TN> : IFastMemberBuffer` / `FastIndexerSetterBuffer<T1..TN, TValue> : IFastMemberBuffer` classes for arity 5+. Same pattern as today's `MethodInvocation<T...>` synthesis.

**Tests:**

- `Tests/Mockolate.Internal.Tests/FastMockInteractionsTests.cs` — unit tests per buffer type: append, count, CountMatching, ordered enumeration across multiple buffers, Clear, event firing, GetUnverifiedInteractions semantics. Target ≥95% coverage on these new types.
- Concurrent-append stress test: N threads each appending to a different member, verify count + order invariants.

**Public API impact:** additive.

**Size:** ~3 days. The arity fan-out across kinds is the main chunk.

### Step 5.2 · `feat(gen): thread FastMockInteractions through mock construction`

Wires `FastMockInteractions` as the `IMockInteractions` backing store for each
generated mock.

**Source generator:**

- Mock ctor creates a `FastMockInteractions` sized to `MemberCount`, installs the right buffer at each member id, passes it to `MockRegistry` via the ctor that accepts `IMockInteractions`.
- At this point the proxy still uses the **old** `RegisterInteraction(new MethodInvocation<T...>(...))`. The fast store is used as the backing for the shared list by routing through a type-dispatched fall-back in `FastMockInteractions.RegisterInteraction`. Behavior identical; fast store proves itself in CI.

**Runtime library:**

- `MockRegistry` ctor accepts an optional `IMockInteractions` factory (default: `MockInteractions`). Preserves back-compat for anyone constructing `MockRegistry` directly.
- `FastMockInteractions.RegisterInteraction<TInteraction>(TInteraction)` — type-dispatch fallback that routes to the right per-member buffer for method/property/indexer/event interaction types. Only lives in 5.2 as the transition bridge; removed in 5.3 when the generator emits direct `Record*` calls.

**Tests:**

- Every existing behavioral test parameterized to run against **both** `MockInteractions` and `FastMockInteractions` via a test-only hook. This is the most valuable verification for the refactor.
- `Mockolate.ExampleTests` — sanity check no user-visible drift.

**Public API impact:** additive.

**Size:** ~2 days. Test parameterization is the bulk of it.

### Step 5.3 · `perf(gen): emit typed Record* calls for all member kinds`

The payoff commit. Rewrite the generator's hot-path emission to call typed
direct-dispatch casts on the `FastMockInteractions` buffers instead of
`RegisterInteraction(new MethodInvocation<T...>(...))`.

**Source generator:**

- Method invocations emit

  ```csharp
  ((FastMethodNBuffer<T1..TN>)this._mockRegistry.Interactions.Buffers[MemberId_MyFunc])
      .Append("Name", arg1, ..., argN);
  ```
- Property getter emits `((FastPropertyGetterBuffer)this._mockRegistry.Interactions.Buffers[MemberId_X]).Append("Name")`. Similarly for setter.
- Indexer getter/setter emit the corresponding typed append.
- Event add/remove emit `((FastEventBuffer)this._mockRegistry.Interactions.Buffers[MemberId_X]).Append("Name", value.Target, value.Method)`.
- Remove the type-dispatch fallback from Step 5.2.

**Runtime library:** no new public methods — the generator emits direct typed casts against the arity-agnostic `Buffers` array.

**Tests:**

- Benchmarks run — expect the full D speedup (property getter ~5×, indexer getter ~7×, method ~1.3×, event ~3× per the measured numbers).
- Concurrent-recording stress test (N threads × M members × K calls each, verify final count = N×M×K, ordered enumeration monotonic).
- Generator snapshot updates.

**Public API impact:** none.

**Size:** ~2 days.

---

## Phase 6 — Fast Verify walks (1 commit)

### Step 6.1 · `perf(gen): emit member-id-keyed Verify predicates`

Today `Verify.MyFunc(...)` builds a closure that walks **every** interaction.
After this commit it walks only the target member's buffer.

**Source generator:**

- `Sources.MockClass.cs:4645` (Verify facade emission) — emit a direct buffer walk against the typed `FastMethodNBuffer<T...>` rather than the shared-list predicate flavor.
- Same for indexer Verify (already member-scoped), property `Got`/`Set`, event `Subscribed`/`Unsubscribed`.

**Runtime library:**

- `MockRegistry` gains an arity-agnostic `VerifyFast<T>(T subject, IFastMemberBuffer buffer, Func<IInteraction, bool>? perInteractionFilter, Func<string> expectation)` helper that returns a `VerificationResult<T>` whose `Verify` walks the typed buffer directly (not the full-list scan).
- `VerificationResult<T>` gets an internal fast path: when constructed with a buffer-walk predicate, bypass `_interactions.Where(_predicate).ToArray()`. Keep the existing full-list path for `VerifySetup(setup)` and cross-member verifications.

**Tests:**

- All existing verification tests pass unchanged.
- Performance test: 100k irrelevant interactions + N target calls → Verify latency bounded (proves scan is per-member, not global).

**Public API impact:** none.

**Size:** ~1.5 days.

---

## Phase 7 — Cleanup (3 commits)

### Step 7.1 · `refactor: retire closure-based GetMethodSetup where unused`

By now the closure-taking `MockRegistry.GetMethodSetup<T>(string, Func<T, bool>)`
path is only used by `VerifySetup(setup)` and unused-setup detection. Either:

- Keep as the cold-path fallback (add internal-extension-only XML doc), or
- Inline-and-remove if no remaining call sites.

**Public API impact:** potentially breaking (if removed from public surface). Gate behind v3.

**Size:** ~half day.

### Step 7.2 · `refactor!: remove [Obsolete] MockInteractions-typed constructor shims`

Drop the shims from Step 3.1. Only flip after downstream packages
(`aweXpect.Mockolate`, docs) are updated.

**Public API impact:** breaking.

**Size:** ~half day.

### Step 7.3 · `chore(bench): remove preview D-optimized classes from benchmark project`

The `OptimizedDMock.cs` / `OptimizedDAllMembersMock.cs` / `OptimizedMy2ParamMock.cs`
preview classes have served their purpose — keep the benchmark harness but
delete the hand-written preview variants (now replaced by the real runtime
library implementation).

**Public API impact:** none.

**Size:** ~1 hour.

---

## Phase 8 — Release prep (1 commit + release mechanics)

### Step 8.1 · `docs: v3 migration guide + benchmark numbers`

- `Docs/pages/migration-v3.md` — call out every public-API break from
  Phase 1 and Step 3.2 with before/after code.
- Embed the benchmark comparison table in `Docs/pages/performance.md` and the
  README.
- Update `CHANGELOG.md` for v3.

**Public API impact:** docs only.

**Size:** ~1 day.

### Step 8.2 · Cut v3.0.0

Standard release flow — tag, NuGet push, blog post.

---

## Summary

| Phase                                           | Commits | Effort | Breaking? | Risk |
|-------------------------------------------------|---|---|---|---|
| 0. Baseline + benchmarks                        | 1 | 1 day | no | low |
| 1. Signature cleanup (B + E-revised)            | 3 | 1.5 days | yes (public) | medium — big sweep |
| 2. Widen `IMockInteractions`                    | 1 | 2 hrs | no | low |
| 3. Plumb `IMockInteractions` through consumers  | 2 | 3 hrs | mild | low |
| 4. Member ids + fast setup dispatch (A + B + C) | 2 | 3 days | no | medium |
| 5. Fast per-member storage (D)                  | 3 | 7 days | no | **high** — largest change |
| 6. 3686                                            | 1 | 1.5 days | no | medium |
| 7. Cleanup                                      | 3 | 1 day | yes | low |
| 8. Release prep                                 | 1 | 1 day | no | low |
| **Total**                                       | **17** | **~18 days** | | |

## Risk controls baked in

- **Parameterized tests** (from Step 5.2): every existing behavior test runs against both `MockInteractions` and `FastMockInteractions`. Any semantic drift fails CI before the generator flips.
- **Incremental benchmarks**: after Steps 4.2, 5.3, and 6.1, run the benchmark comparison. Each should show a staircase (4.2: closure kill; 5.3: per-member storage; 6.1: fast Verify). A regression at any step halts the merge.
- **Coverage gate (90%)**: new code in Step 5.1 has dense unit tests. Target 95%+ on `FastMockInteractions` and buffers since they're on the hot path.
- **API snapshot updates**: every step that touches public API updates `Tests/Mockolate.Api.Tests` snapshots in the same commit. Reviewers see the exact diff.
- **`[Obsolete]` shims** during Phase 3 give downstream packages (and users) a grace window before Step 7.2 removes them.
- **Smoke tests in `[GlobalSetup]`** (from the benchmark port in Step 0.1) keep catching Monitor / Verify regressions at benchmark startup time through the whole refactor.

## Natural bail-out points

- **After Phase 4**: ~25% per-call improvement without touching interaction storage. If Phase 5 takes longer than planned, Phase 4 alone is a shippable v3.0.
- **After Phase 5, skipping Phase 6**: full D recording, Verify still walks the full list. Leaves most of the gain but without the fast Verify path.

## Out of scope

- Changes to the `MockBehavior` / scenario model.
- Generator ergonomics (how `GenerateMock` attributes are discovered, how analyzers flag mockability).
- New public verification terminators.
- `INamedParametersMatch` perf polish (Step 1.3) — orthogonal, can slip to v3.1.
- Ref-struct paths — currently bespoke; decide per-step whether to apply the same member-id treatment or leave on the old path based on measured impact.
