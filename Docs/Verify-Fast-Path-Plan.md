# Verify-fast-path follow-up plan

A focused implementation plan for recovering the speedup the d-refactor was supposed to
deliver. Designed to land as a follow-up to D-Refactor (`Docs/D-Refactor-Plan.md`),
without further public-API breaks.

## Context

After the d-refactor merged, benchmarks showed Phase 5+ regressed against the Phase-4
baseline rather than delivering the predicted 1.3–7× per-call speedup. Root cause:
`VerificationResult<T>.CollectMatching` calls `IFastMemberBuffer.AppendBoxed` on every
Verify, which allocates one `MethodInvocation<T...>` (or equivalent) per recorded
interaction plus three intermediate `List`/array objects — defeating the slim-buffer
storage the refactor introduced. Lock-on-write Append and a wider `Record` struct on top.

The hand-written sketch in
`Benchmarks/Mockolate.Benchmarks/RealisticSuggestedFix/` proves the regression is
recoverable with a small set of changes, while keeping every other piece of Mockolate
runtime infrastructure (`MockRegistry`, `MockBehavior`, scenarios, `ReturnMethodSetup`
chain, `It.IsAny<>` plumbing, default-value generator, callbacks, `MockNotSetupException`)
intact. Measured on the same machine in one BDN run:

| Variant | per-call ns | per-call B |
|---|---:|---:|
| Method_Mockolate (current runtime) | 342 | 329 |
| Method_RealisticSuggested | **60** | **93** |

That's a 5.7× per-call speedup and 3.5× allocation reduction, comfortably exceeding the
plan's predicted 1.3× / 2.2×.

## Guiding principles

- **No breaking public-API changes.** Every change in this plan is additive. The buffer
  classes, `VerificationResult<T>`, and `MockRegistry` gain new members; existing
  members keep their signatures and observable behavior.
- **Same arity strategy as D-Refactor.** Hand-written buffers cover arity 0–4 in the
  runtime library; the source generator emits the arity-5+ fan-out. Every arity-touching
  change happens in one commit across both surfaces.
- **Behavior parity is non-negotiable.** Existing terminators (`Once`/`Exactly`/`Never`/
  `AtLeast`/`AtMost`/`Between`/`Times` and the variants), `Within`/`WithCancellation`,
  `Then`-chains, `VerifySetup`, `AnyParameters`, `Verified`/`GetUnverifiedInteractions`,
  `Monitor` semantics — all match the current implementation exactly.
- **Measure after every perf step.** Run the benchmark comparison harness after Phases 1,
  2, 4. Update `Docs/perf-baseline-v2.md` with raw numbers.

## Where the fix lands

The benchmark workload exercises three structural inefficiencies; this plan addresses
each with a targeted, additive change:

| Inefficiency | Current code | Fix |
|---|---|---|
| Per-call `lock(_lock)` on `Append` | `FastMethodNBuffer<T...>.Append` | Lock-free Interlocked-reserved slot, lock only on grow |
| Wider-than-needed `Record` (`Seq + Name + Pn + Boxed`) | per-buffer `Record` struct | Slim Record (`Seq + Pn` only); name moves to constructor; boxed `IInteraction` lives in side dictionary keyed by slot index |
| `AppendBoxed` allocation per recorded call on every Verify | `VerificationResult<T>.CollectMatching` | New typed `CountMatching` per buffer kind, called from the count-only Verify fast path |

---

## Phase 1 — Buffer-side changes (4 commits)

Each commit applies its transformation to all member kinds in one pass — methods,
property getters/setters, indexer getters/setters, events. Hand-written arities 0–4
in the runtime library, generator-synthesized 5+ in `Sources.MethodSetups.cs` /
`Sources.IndexerSetups.cs`.

### Step 1.1 · `feat: add typed CountMatching to per-member buffers`

Pure additive: introduces a typed count walk that bypasses
`IFastMemberBuffer.AppendBoxed` for the common count-terminator workload.

**Runtime library:**

- `Source/Mockolate/Interactions/FastMethodBuffer.cs` — adds
  `int CountMatching(IParameterMatch<T1>, ..., IParameterMatch<TN>)` to each of
  `FastMethod0Buffer`, `FastMethod1Buffer<T1>`, ..., `FastMethod4Buffer<T1..T4>`.
  For `FastMethod0Buffer` the method is just `int Count`.
- `Source/Mockolate/Interactions/FastPropertyBuffer.cs` —
  `FastPropertyGetterBuffer.CountMatching()` (no matchers — equivalent to `Count`),
  `FastPropertySetterBuffer<T>.CountMatching(IParameterMatch<T>)`.
- `Source/Mockolate/Interactions/FastIndexerBuffer.cs` —
  `FastIndexerGetterBuffer<T1..TN>.CountMatching(IParameterMatch<T1>, ..., IParameterMatch<TN>)`,
  `FastIndexerSetterBuffer<T1..TN, TValue>.CountMatching(matchers + IParameterMatch<TValue>)`.
- `Source/Mockolate/Interactions/FastEventBuffer.cs` — `CountMatching()` (no matchers).

**Source generator (same commit — covers arities 5+):**

- `Sources.MethodSetups.cs` — extend the generated buffer template with the same
  `CountMatching` method for arity 5+.
- `Sources.IndexerSetups.cs` — same for indexer arity 5+.

**Tests:**

- `Tests/Mockolate.Internal.Tests` — new tests per buffer kind covering hit, miss,
  matchers that always match, matchers that always reject, mixed.
- API snapshot update.

**Public API impact:** additive (new public methods on existing public buffer types).

**Size:** ~1 day.

### Step 1.2 · `perf: lock-free Append on per-member buffers`

Replaces `lock(_lock)` per-Append with Interlocked-reserved slot reservation; lock only
on grow.

**Runtime library:**

- All `FastMethodNBuffer<T...>` Append methods (arities 0–4).
- All `FastPropertyGetterBuffer` / `FastPropertySetterBuffer<T>` Append.
- All `FastIndexerGetterBuffer<T...>` / `FastIndexerSetterBuffer<T..., TValue>` Append.
- `FastEventBuffer` Append.

The new pattern (per buffer):

```csharp
public void Append(T1 p1, T2 p2)
{
    long seq = _owner.NextSequence();
    int i = Interlocked.Increment(ref _reserved) - 1;
    SlimRecord[] records = _records;
    if (i >= records.Length) records = GrowToFit(i);
    records[i].Seq = seq;
    records[i].P1 = p1;
    records[i].P2 = p2;
    Interlocked.Increment(ref _published);
}

private SlimRecord[] GrowToFit(int index)
{
    lock (_growLock)
    {
        while (index >= _records.Length)
        {
            SlimRecord[] bigger = new SlimRecord[_records.Length * 2];
            Array.Copy(_records, bigger, _records.Length);
            _records = bigger;
        }
        return _records;
    }
}
```

Drop `_owner.RaiseAdded()` from the per-Append hot path; move it to a separate
"raise after append" call invoked only when the `InteractionAdded` event has subscribers
(used today by `Within(TimeSpan)` waiting). The buffer can track subscriber count via a
dirty bit set/cleared by `FastMockInteractions.InteractionAdded += handler` /
`-= handler` to skip the call when no one is listening.

**Source generator (same commit):** same template change for arity 5+.

**Tests:**

- Existing tests pass unchanged.
- New stress test: N writer threads × M buffers × K appends each, verify
  final `Count == N×M×K` and `CountMatching(any, any) == N×M×K`.
- New test: `InteractionAdded` event still fires when a subscriber is attached.

**Public API impact:** none.

**Size:** ~1 day. Most effort is the multi-writer correctness test.

### Step 1.3 · `refactor: slim per-buffer Record struct`

Drops `Name` and `Boxed` from the per-slot struct. Method name becomes a constructor
parameter on the buffer (one string per buffer instance, instead of one per slot).
Boxed `IInteraction` instances move to a lazy side dictionary populated only by
`AppendBoxed` (i.e. ordered enumeration / Monitor / `GetUnverifiedInteractions`).

**Runtime library:**

- All `FastMethodNBuffer<T...>` — constructor takes `string methodName`; `Record` keeps
  only `Seq` + parameter values.
- Same for property/indexer/event buffers.
- `AppendBoxed` builds the boxed type on demand via a side `Dictionary<int, IInteraction>?`
  guarded by `_growLock`. Cache hits return the same instance so equality semantics
  match the current behavior.

**Source generator (same commit):** same template change for arity 5+; the
`InstallMethodOptimized<T1, T2>(this FastMockInteractions, int memberId, string methodName)`
helper signature gains the methodName parameter. `FastPropertyBufferFactory`,
`FastIndexerBufferFactory`, `FastEventBufferFactory` get the same treatment.

**Tests:**

- `AppendBoxed` parity test: enumerated `IInteraction` instances satisfy the existing
  contract (correct property/indexer/event types, correct names, correct values).
- Reference-equality test: enumerating the same buffer twice returns the same
  `IInteraction` instances (relied on by `Verified` HashSet).

**Public API impact:** mildly breaking — `FastMethodBufferFactory.InstallMethod` etc.
gain a `string methodName` parameter. The generator emits the new signature. External
callers (none in the codebase or the aweXpect.Mockolate package) would need an update;
this is acceptable because the install factories are documented as
"called by the source generator at construction" and not a user-facing surface.

**Size:** ~1 day.

### Step 1.4 · `perf(bench): re-run OptimizedMockComparisonBenchmarks after Phase 1`

Records buffer-side numbers. Expect:
- Append per-call drops by ~10–15 ns (lock-free).
- Per-call alloc drops by ~30–60 B (slim Record).
- Verify still slow because `CollectMatching` continues to use `AppendBoxed`.
- Net: ~25-40% per-call improvement; full benefit waits for Phase 2.

Update `Docs/perf-baseline-v2.md` with the Phase-1 row.

**Public API impact:** none.

**Size:** ~2 hours.

---

## Phase 2 — Count-only Verify fast path (3 commits)

Routes count terminators through `CountMatching` instead of `CollectMatching`. The
single biggest perf lever in the plan.

### Step 2.1 · `feat: add IFastCountSource and VerifyCount on the Verify result`

**Runtime library:**

- `Source/Mockolate/Verify/IFastCountSource.cs` — new internal interface:

  ```csharp
  internal interface IFastCountSource
  {
      int Count();              // matches today's predicate
      int CountAll();           // when AnyParameters() is called
  }
  ```

- `Source/Mockolate/Verify/IVerificationResult.cs` — add internal method:

  ```csharp
  bool VerifyCount(Func<int, bool> countPredicate);
  ```

  The existing `Verify(Func<IInteraction[], bool>)` stays for `Then`-chains and
  `VerifySetup`-style cross-member predicates.

- `Source/Mockolate/Verify/VerificationResult.cs` — new internal field
  `IFastCountSource? _fastCountSource` and a constructor that takes it.
  Implementation:

  ```csharp
  bool IVerificationResult.VerifyCount(Func<int, bool> predicate)
  {
      ThrowIfRecordingDisabled(_interactions);
      if (_fastCountSource is not null)
      {
          int count = _fastCountSource.Count();
          // Verified tracking: defer until _verified is queried (Step 2.3).
          return predicate(count);
      }

      // Fallback: walk the existing predicate path but only count.
      int fallback = _interactions.Count(_predicate);
      return predicate(fallback);
  }
  ```

- The `Awaitable` subclass wires `VerifyCount` through the same semaphore/timeout loop
  the existing `Verify` uses.

**Tests:** new unit tests targeting `VerifyCount` directly via the internal API.

**Public API impact:** none (interface is internal).

**Size:** ~half day.

### Step 2.2 · `refactor: terminator extensions use VerifyCount`

Updates `VerificationResultExtensions` so every count terminator
(`Exactly`/`Once`/`Never`/`AtLeast`/`AtLeastOnce`/`AtLeastTwice`/`AtMost`/`AtMostOnce`/
`AtMostTwice`/`Between`/`Times`) calls `result.VerifyCount(c => …)` instead of
`result.Verify(arr => arr.Length …)`.

**Runtime library:**

- `Source/Mockolate/Verify/VerificationResultExtensions.cs` — mechanical change:
  every terminator's lambda goes from `interactions => /* count check on
  interactions.Length */` to `count => /* count check on count */`. The
  surrounding try/catch/MockVerificationException flow is unchanged.

**Tests:** all existing `Mockolate.Tests` verification tests pass unchanged. No
snapshot updates expected.

**Public API impact:** none (these are extension method bodies; signatures unchanged).

**Size:** ~half day.

### Step 2.3 · `feat: lazy Verified tracking for the count-only fast path`

The current count-full path calls `_interactions.Verified(matchingInteractions)` to
populate the `_verified` HashSet, which `GetUnverifiedInteractions` later queries. The
fast path doesn't materialize `IInteraction[]`, so it can't directly populate the set.

Two options:
- **(A) Materialize once at the end of the test.** When `GetUnverifiedInteractions`
  runs, it consults a per-buffer "verified slot" bitmap that the fast path populates,
  and only then calls `AppendBoxed` for the unverified slots. Cheaper for tests that
  never call `GetUnverifiedInteractions`.
- **(B) Materialize at Verify time.** When the fast path runs, ask the buffer for the
  matching slot indices, then call `AppendBoxed` only for those slots and add the
  resulting `IInteraction` instances to `_verified`. Same cost as today's full
  materialization but avoids the boxing of non-matching slots.

Choose (A) — most workloads never call `GetUnverifiedInteractions` (it's used by the
`VerifyThatAllInteractionsAreVerified` API), so the common case stays alloc-free.

**Runtime library:**

- Each `IFastMemberBuffer` gains an optional `MarkVerified(int slotIndex)` /
  `MarkAllVerified()` method. The buffer keeps a per-slot bitmap (one `byte[]` allocated
  on first verify-tracking call).
- `FastMockInteractions.GetUnverifiedInteractions()` queries the bitmap and only
  materializes the unverified slots via `AppendBoxed`.

**Tests:** behavioral parity tests for `VerifyThatAllInteractionsAreVerified` and
`GetUnverifiedInteractions` against both the count-only fast path and the full path.

**Public API impact:** additive on `IFastMemberBuffer`.

**Size:** ~1 day.

---

## Phase 3 — Typed Verify overloads on MockRegistry (1 commit)

Wires the typed matchers and the typed buffer into an `IFastCountSource`.

### Step 3.1 · `feat: MockRegistry typed Verify overloads`

**Runtime library:**

- `Source/Mockolate/MockRegistry.Verify.cs` — new overloads:

  ```csharp
  public VerificationResult<T>.IgnoreParameters VerifyMethod<T, T1, T2>(
      T subject, int memberId, string methodName,
      IParameterMatch<T1> match1, IParameterMatch<T2> match2,
      Func<string> expectation);
  ```

  for arities 0–4. Internally constructs a `MethodNCountSource<T1..TN>(buffer, m1..mN)`
  implementing `IFastCountSource` and passes it to the result. Falls back to the
  existing predicate path when the buffer at `memberId` isn't installed.

- Same overloads for `VerifyProperty<T>`, `VerifyProperty<T, TValue>`, `IndexerGot<T,...>`,
  `IndexerSet<T, ..., TValue>`, `SubscribedTo<T>`, `UnsubscribedFrom<T>`.

- `Source/Mockolate/Verify/MethodNCountSource.cs` — small internal sealed classes per
  arity (mirror of `FastMethodNBuffer<T...>`), each just calls into the buffer.

**Source generator (same commit):** synthesizes `MethodNCountSource<T1..TN>` for arity 5+.

**Tests:** existing verification tests pass unchanged. New tests covering the
buffer-not-installed fallback (e.g. constructed `MockRegistry` with `MockInteractions`
instead of `FastMockInteractions`).

**Public API impact:** additive.

**Size:** ~1 day.

---

## Phase 4 — Generator emits the typed Verify call (1 commit)

### Step 4.1 · `perf(gen): emit typed Verify calls for count terminators`

**Source generator:**

- `Sources.MockClass.cs` — Verify-method emission. For overloads where every parameter
  is an `IParameter<T>` (i.e. matcher overloads), emit:

  ```csharp
  this.MockRegistry.VerifyMethod<TVerify, T1, T2>(
      this, MemberId_MyFunc, "MyFunc",
      CovariantParameterAdapter<T1>.Wrap(p1 ?? It.IsNull<T1>("null")),
      CovariantParameterAdapter<T2>.Wrap(p2 ?? It.IsNull<T2>("null")),
      () => $"MyFunc({p1}, {p2})");
  ```

  instead of the current predicate-based call. The mixed-matcher-and-value overloads
  wrap the value in `It.Is<T>(value, label)` (the generator already does this for
  setup; reuse the helper).

- `Sources.IndexerSetups.cs`, `Sources.MethodSetups.cs` — same change for arity 5+.

The `parameters: IParameters` overload still uses the predicate path because
`IParametersMatch.Matches(object[])` operates on a heterogeneous array.

The Verify emission for `AnyParameters()`-friendly overloads (arity-only ignore-params)
emits a `CountAll`-shaped call.

**Tests:**

- Generator snapshot updates.
- Run `OptimizedMockComparisonBenchmarks` — expect the per-call hot path to drop to
  the ~60-90 ns range and per-call alloc to drop to ~90-100 B (matching the
  RealisticSuggested numbers).

**Public API impact:** none.

**Size:** ~1 day. Mostly snapshot churn.

---

## Phase 5 — Validation + cleanup (2 commits)

### Step 5.1 · `perf(bench): full comparison + perf-baseline-v2 update`

- Run `CompleteMethodBenchmarks`, `CompletePropertyBenchmarks`, `CompleteIndexerBenchmarks`,
  `CompleteEventBenchmarks`, `CallbackBenchmarks`, `OptimizedMockComparisonBenchmarks`.
- Update `Docs/perf-baseline-v2.md` with new numbers and call out the staircase
  (Phase 1 / Phase 2 / Phase 4).
- Compare against the PR-680 (Phase-4) baseline: per-call should be lower and
  allocations should be lower in every benchmarked category.

**Public API impact:** none.

**Size:** ~2 hours.

### Step 5.2 · `chore(bench): retire RealisticSuggestedFix demonstration`

- Delete `Benchmarks/Mockolate.Benchmarks/RealisticSuggestedFix/` and the
  `Method_RealisticSuggested` benchmark — it has served its purpose and the runtime now
  delivers equivalent numbers.
- Optionally retire `Benchmarks/Mockolate.Benchmarks/SuggestedFixMocks.cs` and
  `SmokeTest.cs` if the benchmark project should not carry the upper-bound reference
  going forward.

**Public API impact:** none.

**Size:** ~1 hour.

---

## Summary

| Phase | Commits | Effort | Breaking? | Risk |
|---|---|---|---|---|
| 1. Buffer typed CountMatching + lock-free + slim Record | 4 | ~2.5 days | mild (`InstallMethod` factory signature) | medium — multi-writer correctness |
| 2. Count-only Verify fast path | 3 | ~2 days | no | medium — Verified-tracking refactor |
| 3. MockRegistry typed Verify overloads | 1 | ~1 day | no | low |
| 4. Generator emits typed Verify call | 1 | ~1 day | no | low — snapshot churn |
| 5. Validation + cleanup | 2 | ~3 hrs | no | none |
| **Total** | **11** | **~7 days** | one mild break | |

## Risk controls

- **Parameterized tests** (carried forward from D-Refactor Phase 5.2): every existing
  behavior test runs against both `MockInteractions` and `FastMockInteractions` so any
  semantic drift surfaces in CI before the generator flips.
- **Incremental benchmarks**: rerun `OptimizedMockComparisonBenchmarks` after Phases 1.4,
  2.3, and 4.1. Each step should show a strict improvement (no regression vs the
  previous step). A regression halts the merge.
- **Multi-writer stress test** (Step 1.2): N threads × K appends, verify final count
  + ordered enumeration consistency.
- **Verified-tracking parity**: a dedicated test class in `Mockolate.Tests` exercises
  `VerifyThatAllInteractionsAreVerified` and `GetUnverifiedInteractions` across the
  count-only fast path and the predicate path, with mixed verify orders.
- **API snapshot updates** with each step that touches public API.

## Natural bail-out points

- **After Phase 1**: ~25-40% per-call improvement from buffer-side fixes alone.
  Shippable as a point release if Phase 2 takes longer than budgeted.
- **After Phase 2 + Phase 3, skipping Phase 4**: runtime path is fast but generator
  still emits the predicate path. Existing mocks see no benefit until regenerated.
  Useful as a staging point if generator changes need separate review.

## Out of scope

- Public API breaks beyond the `InstallMethod` factory signature change in Step 1.3.
- Refactoring the `VerificationResult<T>` class hierarchy (e.g. splitting count-only
  vs general into separate types). Internal `VerifyCount` plus the existing class is
  enough.
- `Then`-chains and `VerifySetup` — these stay on the predicate path (cross-member,
  needs ordered enumeration).
- Ref-struct paths — current Verify already routes them differently;
  the count-only fast path can be applied later if measured worthwhile.
- `INamedParametersMatch` polish (Step 1.3 from D-Refactor) — orthogonal,
  still deferred.
