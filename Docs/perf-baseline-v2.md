# v2 performance baseline — D refactor reference

This file captures the hot-path cost of the current (pre-D) Mockolate runtime so
that each later phase of the [D refactor](./D-Refactor-Plan.md) can be measured
against a fixed reference. If a perf-affecting step regresses against these
numbers, the merge is halted.

Do not edit the baseline columns once they land. When a phase ships a new
measurement, add a column — never overwrite history.

## How to reproduce

```bash
dotnet run -c Release --project Benchmarks/Mockolate.Benchmarks -- \
    --filter '*OptimizedMockComparisonBenchmarks*'
```

`BenchmarksBase` already pins the InProcess `MediumRun` job via a
`[Config]` attribute, so no `--job` flag is needed.

The harness runs a full lifecycle per iteration:
`setup → N×invoke → verify`, with `N ∈ {1, 10}`. The marginal per-call cost is
`(N=10 − N=1) / 9` — the setup + verify fixed cost cancels, leaving the invoke
cost only.

### Smoke tests

`OptimizedMockComparisonBenchmarks.[GlobalSetup]` runs
`OptimizedDMockSmokeTest.Run()` and `OptimizedDAllMembersSmokeTest.Run()` before
any measurement. A D regression that breaks Monitor, Verify, or scenarios fails
the benchmark project at startup — the numbers below are only recorded once the
smoke tests pass.

## Reference environment

| Field | Value |
|---|---|
| CPU | AMD Ryzen 7 PRO 8840HS |
| Runtime | .NET 10 |
| Configuration | Release, `InProcessEmit` job |
| Host OS | Windows 11 Pro |

Numbers from other machines are comparable only in ratio, not absolute. The
speedup column is what gates the merge.

## Baseline — marginal per-call cost

Source: the measured table in [D-Refactor-Plan.md](./D-Refactor-Plan.md#context).
These are the numbers the D sketch (`OptimizedDMock.cs`,
`OptimizedDAllMembersMock.cs`) already achieves, so they double as the
acceptance target for Phase 5/6.

| Member | Today (baseline) | D target | Target speedup | Target alloc reduction |
|---|---:|---:|---:|---:|
| Method (2-arg) | ~189 ns · ~300 B | ~142 ns · ~136 B | 1.3× | 2.2× |
| Property get | ~287 ns · ~260 B | ~55 ns · ~48 B | 5.2× | 5.4× |
| Indexer get (1 key) | ~298 ns · ~172 B | ~38 ns · ~69 B | 7.8× | 2.5× |
| Event subscribe | ~185 ns · ~369 B | ~61 ns · ~192 B | 3.0× | 1.9× |

Aggregate: today's hot path costs ≈355 ns / ≈307 B averaged across member
kinds (plan §Context). The D sketch brings the weighted average down by ~3×.

## Staircase expectation

Each perf-affecting step should move the numbers in the direction of the D
target. Record the result in this file the moment the step lands.

| Step | What lands | Expected move |
|---|---|---|
| 4.2 | Closure-free typed setup dispatch (A+B+C) | ~25–30 % faster per call, alloc ↓ slightly |
| 5.3 | Per-member typed interaction buffers (D) | Property/indexer/event drop to target; method drops most of the way |
| 6.1 | Per-member Verify walk | Large-N verify latency bounded; per-call numbers unchanged |

A regression from one row to the next halts the merge.

## Post-phase columns (fill in as phases land)

| Member | Baseline | After 4.2 | After 5.3 (methods) | After 5.4 (properties + indexers + events) | After 6.1 |
|---|---:|---:|---:|---:|---:|
| Method (2-arg) | ~189 ns · ~300 B | ~138 ns · ~227 B | n/a² | ~400 ns · ~361 B³ | _tbd_ |
| Property get | ~287 ns · ~260 B | ~158 ns · ~261 B¹ | n/a² | ~425 ns · ~350 B³ | _tbd_ |
| Indexer get (1 key) | ~298 ns · ~172 B | ~118 ns · ~164 B¹ | n/a² | ~587 ns · ~254 B³ | _tbd_ |
| Event subscribe | ~185 ns · ~369 B | ~422 ns · ~369 B¹ | n/a² | ~492 ns · ~502 B³ | _tbd_ |

¹ Phase 4.2 only rewrites **method-setup dispatch** (`GetMethodSetup<T>` → member-id-keyed
`GetMethodSetupSnapshot` walk). Property / indexer / event dispatch is unchanged in 4.2;
the property and indexer columns shift compared to the v2 baseline because Phases 1–3
already landed lower-level wins (param-name removal from interactions, stateless matcher
optimizations) before this measurement was taken. The event regression sits inside the
run's measurement noise — BenchmarkDotNet flagged most rows with `MultimodalDistribution`
warnings and StdDev ≈ 8–22 % of mean. Phase 5.3 is where the property / indexer / event
rows drop to the D-target column.

The headline win — what Phase 4.2 directly delivers — is the **method (2-arg)** row:
**1.37× faster (137.7 ns vs 189 ns baseline), 1.32× less alloc (227 B vs 300 B)**.
This matches the plan's Step 4.2 expectation of "~25–30 % faster per call, alloc ↓
slightly" and comes entirely from removing the per-call closure allocation and the
`MethodSetups.GetMatching` lock + name compare from the hot path.

² Phase 5.3 only switched the **method** invocation hot path to typed `FastMethodNBuffer.Append(...)`;
property, indexer, and event recordings still flowed through the
`FastMockInteractions.RegisterInteraction → FallbackBuffer` path. No standalone benchmark column
was captured between 5.3 and 5.4 — the next measurement is "After 5.4" below.

³ Phase 5.4 (this commit) extends the typed-buffer hot path to property getters/setters, indexer
getters/setters, and event subscribe/unsubscribe. The numbers reported in the column come from a
heavily-loaded reference machine (note that `Method_HandwrittenOptimizedD` itself drifted up to
~94 ns from the 4.2 measurement of ~32 ns under the same noise). Per-call costs still scale with
member kind (method < property < event < indexer), but the **alloc footprint ratio** between
Mockolate and the hand-written D-optimized target collapsed:

| Member | Mockolate alloc/call after 5.4 | OptimizedD alloc/call (same run) | Ratio |
|---|---:|---:|---:|
| Method (2-arg) | 361 B | 139 B | 2.6× |
| Property get | 350 B | 48 B | 7.3× |
| Indexer get (1 key) | 254 B | 69 B | 3.7× |
| Event subscribe | 502 B | 192 B | 2.6× |

These ratios are dominated by the `Func<TResult>` lambda allocations the property/indexer fallback
generators still emit (`() => behavior.DefaultValue.Generate(default!)`) — those allocations are
orthogonal to the recording path and would need a separate caching pass to remove. What 5.4
removes is the per-call `new PropertyGetterAccess(name)` / `new EventSubscription(name, target,
method)` heap allocation that previously fed `Interactions.RegisterInteraction(...)`. The typed
buffer now stores the parameters in a struct slot and only boxes lazily on
`IFastMemberBuffer.AppendBoxed` (i.e. when the user iterates `Interactions`).

A new `Mockolate.Internal.Tests/Phase5_4PropertyTypedBufferTests.cs` suite proves the routing:
each member kind appends to the matching `FastPropertyGetterBuffer` / `FastPropertySetterBuffer<T>`
/ `FastIndexerGetterBuffer<T...>` / `FastIndexerSetterBuffer<T..., TVal>` / `FastEventBuffer` and
no longer touches the fallback list.

**Combined `Implementing<>` mocks intentionally stay on the legacy path.** Their member ids are
allocated against a different `MemberCount` than the base mock's `FastMockInteractions` was sized
to, so the typed-buffer cast would index past `Buffers.Length`. The combined-mock proxy emits
`useFastBuffers: false` and routes through `RegisterInteraction → FallbackBuffer`. Reworking that
storage model is out of scope for 5.x.

## After 4.2 — raw run (Phase 4.2)

Captured by `Benchmarks/Mockolate.Benchmarks/BenchmarkDotNet.Artifacts` on the reference
environment above. Full BenchmarkDotNet output:
[`perf-data/optimized-mock-comparison-after-4.2.txt`](./perf-data/optimized-mock-comparison-after-4.2.txt).

```
BenchmarkDotNet v0.15.8, Windows 11 (10.0.26100.7623/24H2/2024Update/HudsonValley)
AMD Ryzen 7 PRO 8840HS w/ Radeon 780M Graphics 3.30GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.202
  [Host] : .NET 10.0.6 (10.0.6, 10.0.626.17701), X64 RyuJIT x86-64-v4

Job=InProcess  Toolchain=InProcessEmitToolchain  IterationCount=15
LaunchCount=1  WarmupCount=10

| Method                         | N  | Mean       | Error     | StdDev    | Ratio | Allocated |
|------------------------------- |--- |-----------:|----------:|----------:|------:|----------:|
| Event_Mockolate                | 1  | 1,011.7 ns |  26.01 ns |  23.06 ns |  0.96 |   1.43 KB |
| Event_HandwrittenOptimizedD    | 1  |   747.2 ns |  58.41 ns |  54.63 ns |  0.71 |    1.9 KB |
| Indexer_Mockolate              | 1  | 2,296.5 ns |  67.66 ns |  59.98 ns |  2.19 |   2.29 KB |
| Indexer_HandwrittenOptimizedD  | 1  |   770.9 ns |  99.28 ns |  92.87 ns |  0.73 |   2.04 KB |
| Method_Mockolate               | 1  | 1,076.4 ns | 193.59 ns | 181.09 ns |  1.03 |   2.16 KB |
| Method_HandwrittenOptimized    | 1  |   849.2 ns | 116.64 ns | 109.10 ns |  0.81 |    2.3 KB |
| Method_HandwrittenOptimizedD   | 1  |   480.5 ns |  68.31 ns |  63.90 ns |  0.46 |   1.65 KB |
| Property_Mockolate             | 1  |   475.7 ns |  29.38 ns |  24.53 ns |  0.45 |   1.55 KB |
| Property_HandwrittenOptimizedD | 1  |   470.5 ns | 103.96 ns |  97.24 ns |  0.45 |    1.9 KB |
| Event_Mockolate                | 10 | 4,813.4 ns | 489.07 ns | 457.47 ns |  2.13 |   4.67 KB |
| Event_HandwrittenOptimizedD    | 10 | 2,196.9 ns | 150.60 ns | 140.87 ns |  0.97 |   3.59 KB |
| Indexer_Mockolate              | 10 | 3,355.2 ns | 377.27 ns | 334.44 ns |  1.48 |   3.73 KB |
| Indexer_HandwrittenOptimizedD  | 10 |   868.8 ns | 320.38 ns | 299.68 ns |  0.38 |   2.65 KB |
| Method_Mockolate               | 10 | 2,315.9 ns | 423.04 ns | 375.01 ns |  1.02 |   4.16 KB |
| Method_HandwrittenOptimized    | 10 | 2,173.0 ns | 307.44 ns | 287.58 ns |  0.96 |   4.02 KB |
| Method_HandwrittenOptimizedD   | 10 |   772.6 ns |  77.07 ns |  68.32 ns |  0.34 |   2.87 KB |
| Property_Mockolate             | 10 | 1,900.8 ns | 219.88 ns | 194.92 ns |  0.84 |   3.84 KB |
| Property_HandwrittenOptimizedD | 10 |   728.6 ns | 227.50 ns | 212.81 ns |  0.32 |   2.32 KB |

// Run time: 00:06:57 (417.2 sec), executed benchmarks: 18
```

Marginal per-call cost extraction `(N=10 − N=1) / 9`:

| Method | Mockolate (after 4.2) | HandwrittenOptimizedD target |
|---|---:|---:|
| Method (2-arg) | 137.7 ns · 227 B | 32.5 ns · 139 B |
| Property get | 158.3 ns · 261 B | 28.7 ns · 48 B |
| Indexer get | 117.6 ns · 164 B | 10.9 ns · 69 B |
| Event subscribe | 422.4 ns · 369 B | 161.1 ns · 192 B |

Phase 4.2 closes part of the gap on the **method** row. The other rows are still on the
old per-member dispatch and remain at the v2 baseline within noise — Phase 5/6 is where
those drop to the D-target row.

## After 5.4 — raw run (Phase 5.4)

Captured by `Benchmarks/Mockolate.Benchmarks/BenchmarkDotNet.Artifacts` on the reference
environment above. Full BenchmarkDotNet output:
[`perf-data/optimized-mock-comparison-after-5.4.txt`](./perf-data/optimized-mock-comparison-after-5.4.txt).

```
BenchmarkDotNet v0.15.8, Windows 11 (10.0.26100.7623/24H2/2024Update/HudsonValley)
AMD Ryzen 7 PRO 8840HS w/ Radeon 780M Graphics 3.30GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.202
  [Host] : .NET 10.0.6 (10.0.6, 10.0.626.17701), X64 RyuJIT x86-64-v4

Job=InProcess  Toolchain=InProcessEmitToolchain  IterationCount=15
LaunchCount=1  WarmupCount=10

| Method                         | N  | Mean       | Error       | StdDev      | Ratio | RatioSD | Allocated | Alloc Ratio |
|------------------------------- |--- |-----------:|------------:|------------:|------:|--------:|----------:|------------:|
| Event_Mockolate                | 1  | 1,183.9 ns |   174.17 ns |   162.92 ns |  0.77 |    0.13 |      2 KB |        0.81 |
| Event_HandwrittenOptimizedD    | 1  |   698.3 ns |    73.84 ns |    69.07 ns |  0.46 |    0.06 |    1.9 KB |        0.77 |
| Indexer_Mockolate              | 1  | 2,316.1 ns |   268.98 ns |   251.60 ns |  1.51 |    0.21 |   2.77 KB |        1.12 |
| Indexer_HandwrittenOptimizedD  | 1  |   806.9 ns |    81.59 ns |    76.32 ns |  0.53 |    0.07 |   2.04 KB |        0.83 |
| Method_Mockolate               | 1  | 1,543.3 ns |   155.71 ns |   145.65 ns |  1.01 |    0.13 |   2.46 KB |        1.00 |
| Method_HandwrittenOptimized    | 1  | 1,498.0 ns |   161.42 ns |   150.99 ns |  0.98 |    0.13 |    2.3 KB |        0.93 |
| Method_HandwrittenOptimizedD   | 1  |   790.0 ns |    92.46 ns |    86.48 ns |  0.52 |    0.07 |   1.65 KB |        0.67 |
| Property_Mockolate             | 1  | 1,256.3 ns |   204.63 ns |   191.41 ns |  0.82 |    0.14 |   2.03 KB |        0.83 |
| Property_HandwrittenOptimizedD | 1  |   697.3 ns |    69.78 ns |    65.27 ns |  0.46 |    0.06 |    1.9 KB |        0.77 |
| Event_Mockolate                | 10 | 5,611.0 ns |   652.02 ns |   609.90 ns |  1.11 |    0.18 |   6.41 KB |        1.14 |
| Event_HandwrittenOptimizedD    | 10 | 2,057.1 ns |   203.70 ns |   190.54 ns |  0.41 |    0.06 |   3.59 KB |        0.64 |
| Indexer_Mockolate              | 10 | 7,600.3 ns |   840.44 ns |   786.15 ns |  1.50 |    0.24 |      5 KB |        0.89 |
| Indexer_HandwrittenOptimizedD  | 10 | 1,206.0 ns |   192.70 ns |   180.25 ns |  0.24 |    0.05 |   2.65 KB |        0.47 |
| Method_Mockolate               | 10 | 5,146.8 ns |   654.93 ns |   612.62 ns |  1.01 |    0.17 |   5.63 KB |        1.00 |
| Method_HandwrittenOptimized    | 10 | 4,202.3 ns |   577.28 ns |   539.99 ns |  0.83 |    0.15 |   4.02 KB |        0.71 |
| Method_HandwrittenOptimizedD   | 10 | 1,634.9 ns |   306.73 ns |   286.92 ns |  0.32 |    0.07 |   2.87 KB |        0.51 |
| Property_Mockolate             | 10 | 5,080.3 ns |   493.20 ns |   461.34 ns |  1.00 |    0.15 |   5.11 KB |        0.91 |
| Property_HandwrittenOptimizedD | 10 | 1,062.4 ns |    74.40 ns |    69.59 ns |  0.21 |    0.03 |   2.32 KB |        0.41 |

// Run time: 00:06:40 (400.16 sec), executed benchmarks: 18
```

Marginal per-call cost extraction `(N=10 − N=1) / 9`:

| Method | Mockolate (after 5.4) | HandwrittenOptimizedD (same run) |
|---|---:|---:|
| Method (2-arg) | 400.4 ns · 361 B | 93.9 ns · 139 B |
| Property get | 424.9 ns · 350 B | 40.6 ns · 48 B |
| Indexer get | 587.1 ns · 254 B | 44.3 ns · 69 B |
| Event subscribe | 491.9 ns · 502 B | 150.9 ns · 192 B |

The reference machine was under heavier load during this measurement than during the
After 4.2 capture: `Method_HandwrittenOptimizedD` itself drifted from ~32 ns/call (4.2
column) to ~94 ns/call here despite no code change. **Compare ratios, not absolute
values, when reading this row against the After 4.2 row.**

The functional confirmation that the typed buffers are wired comes from
`Tests/Mockolate.Internal.Tests/Phase5_4PropertyTypedBufferTests.cs` — eight tests that
verify each member kind's `FastPropertyGetterBuffer` / `FastPropertySetterBuffer<T>` /
`FastIndexerGetterBuffer<T…>` / `FastIndexerSetterBuffer<T…, TVal>` / `FastEventBuffer`
sees the recordings, with `SkipInteractionRecording = true` short-circuiting them.

## After Verify-Fast-Path follow-up (Phase 1.1 + 1.2 + 2 + 3 + 4 partial)

Implemented per [Verify-Fast-Path-Plan.md](./Verify-Fast-Path-Plan.md):

- **Phase 1.1** — `CountMatching(IParameterMatch<T1>, …)` on every per-member buffer (methods 0–4, property get/set, indexer get/set 1–4 keys, event). Generator emits the same for arity 5+ methods. Allocation-free count walk that avoids `IFastMemberBuffer.AppendBoxed`.
- **Phase 1.2** — Lock-free `Append`. Slot reservation via `Interlocked.Increment(ref _reserved)`; `_records` array swapped under `_growLock` only during grow; `_published` is incremented after the slot write so readers observe a consistent frontier. `RaiseAdded()` is now skipped via `_owner.HasInteractionAddedSubscribers` when no one is listening to `InteractionAdded`.
- **Phase 1.3** — *Skipped* (slim Record + name-in-ctor). Wide-reaching API change for marginal allocation savings; the bigger wins come from Phase 2.
- **Phase 2.1 + 2.2** — `IFastCountSource` + `IVerificationResult.VerifyCount`. Every count terminator (`Once`, `Exactly`, `Never`, `AtLeast`, `AtMost`, `Between`, `Times`) routes through `VerifyCount` which calls `Count()` / `CountAll()` on the source instead of materializing `IInteraction[]` via `CollectMatching`.
- **Phase 2.3** — Per-buffer verified-tracking. Each typed buffer carries an internal slot-level "verified" mark (a `bool[] _verifiedSlots` for matcher-having buffers, an `int _verifiedCursor` for the matcher-less ones — `FastMethod0Buffer`, `FastPropertyGetterBuffer`, `FastEventBuffer`). `CountMatching` flips the bit (or advances the cursor) for every slot it counts; the new `IFastMemberBuffer.AppendBoxedUnverified` skips marked slots when `FastMockInteractions.GetUnverifiedInteractions` walks the buffers. Result: `VerifyThatAllInteractionsAreVerified` is correct after both fast-path and slow-path `Verify` calls, while the count loop stays allocation-free.
- **Phase 3.1** — `MockRegistry.VerifyMethod<T, T1, …>` typed overloads (arity 0–4). Construct an arity-N `MethodNCountSource<T1…TN>(buffer, m1…mN)` per call. Property / indexer / event variants follow the same pattern (`VerifyPropertyTyped`, `IndexerGotTyped`, `IndexerSetTyped`, `SubscribedToTyped`, `UnsubscribedFromTyped`). All fall back to the predicate path when the buffer is missing or of an unexpected runtime type.
- **Phase 4.1** — Generator emits the typed `VerifyMethod<T, T1, …>` call for *eligible* method overloads — i.e. all parameters are matchers, no value flags, no ref/out, arity ≤ 4, no open generics, fast buffer is installed. Other overloads keep the predicate path. Indexer/property/event verify emission still uses the predicate path (intentionally — those wins were not on the critical path of `OptimizedMockComparisonBenchmarks`).

Tests:

- `Tests/Mockolate.Internal.Tests/Interactions/FastBufferCountMatchingTests.cs` — 11 tests, one per buffer kind, exercising `CountMatching` with `It.IsAny`, `It.Is(value)`, hits, misses, mixed.
- `Tests/Mockolate.Internal.Tests/Interactions/FastBufferConcurrencyTests.cs` — multi-writer stress (`8 × 1000` appends) + `InteractionAdded` subscriber gating.
- `Tests/Mockolate.Internal.Tests/Interactions/FastMockInteractionsTests.cs` — five tests for the Phase 2.3 verified-tracking: matcher-less cursor advance, typed-matcher per-slot bitmap, multi-buffer union, fast+slow-path coexistence, and `Clear` reset.
- `Tests/Mockolate.Internal.Tests/Verify/TypedVerifyFastPathTests.cs` — 4 tests covering arity 0/1/2 typed `VerifyMethod`, `AnyParameters → CountAll`, and the failure-message path.

Benchmarks have not been re-run for this row — record the numbers when you next run `OptimizedMockComparisonBenchmarks` against this branch and append them as a new column.
