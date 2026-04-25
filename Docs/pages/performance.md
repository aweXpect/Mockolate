# Performance

Mockolate v3.0 ships the **D refactor** — per-member typed interaction storage,
member-id-keyed setup dispatch, and per-member `Verify` walks. The hot path went
from ~355 ns / ~307 B per call (v2) to a member-kind-dependent fraction of that.

## Marginal per-call cost

Marginal cost per invocation, extracted as `(N=10 − N=1) / 9` from the
`OptimizedMockComparisonBenchmarks` harness — the setup + verify fixed overhead
cancels and only the per-call cost remains.

| Member | v2 baseline | v3 (D) target | Speedup | Allocation reduction |
|---|---:|---:|---:|---:|
| Method (2-arg) | ~189 ns · ~300 B | ~142 ns · ~136 B | 1.3× | 2.2× |
| Property get | ~287 ns · ~260 B | ~55 ns · ~48 B | 5.2× | 5.4× |
| Indexer get (1 key) | ~298 ns · ~172 B | ~38 ns · ~69 B | 7.8× | 2.5× |
| Event subscribe | ~185 ns · ~369 B | ~61 ns · ~192 B | 3.0× | 1.9× |

The aggregate improvement is roughly **3× faster across the four kinds** with a
matching drop in allocations. Property and indexer hot paths see the largest
relative wins because the v2 implementation allocated a closure plus an
`Access` wrapper on every call; the v3 typed buffer captures the parameters in
a struct slot and only boxes lazily on enumeration.

## Reference environment

Numbers above were captured on:

| Field | Value |
|---|---|
| CPU | AMD Ryzen 7 PRO 8840HS |
| Runtime | .NET 10 |
| Configuration | Release, BenchmarkDotNet `InProcessEmit` job |
| Host OS | Windows 11 Pro |

Absolute numbers depend on hardware. The speedup column is the stable signal.

## What changed under the hood

The full design and step-by-step rollout lives in
[`Docs/D-Refactor-Plan.md`](../D-Refactor-Plan.md); the highlights:

- **Member-id-keyed setup dispatch.** Each generated mock emits a compile-time
  member-id table. Proxy method bodies fetch a typed `MethodSetup[]` snapshot
  via `MockRegistry.GetMethodSetupSnapshot(memberId)` and walk it in reverse,
  evaluating `Matches(values)` directly. No closure allocation, no string-name
  comparison, no list lock on the hot path.
- **Per-member typed interaction buffers.** The shared `List<IInteraction>` is
  replaced by per-member `FastMethodNBuffer<T...>` / `FastPropertyGetterBuffer` /
  `FastPropertySetterBuffer<T>` / `FastIndexerGetterBuffer<T...>` /
  `FastIndexerSetterBuffer<T..., TVal>` / `FastEventBuffer` instances, each
  storing parameters in struct slots. `MethodInvocation<T...>` /
  `PropertyGetterAccess` / `IndexerGetterAccess<T...>` etc. are still the public
  enumeration shape — they're constructed lazily when the user iterates
  `Interactions`.
- **Per-member Verify walks.** `sut.Mock.Verify.MyFunc(...)` walks only the
  target member's buffer, not the global interaction list. Verify cost no longer
  scales with unrelated traffic on the same mock.
- **Parameter-name strings dropped from the recording path.** Setups capture
  parameter names once at construction time; `Matches`, `MethodInvocation<T...>`
  ctor and `IndexerAccess` ctor no longer thread names through every call.

## Reproducing

```bash
dotnet run -c Release --project Benchmarks/Mockolate.Benchmarks -- \
    --filter '*OptimizedMockComparisonBenchmarks*'
```

The harness exercises the full setup → N×invoke → verify lifecycle for every
member kind on a generated mock, with `N ∈ {1, 10}`. Other benchmark classes
(`CompleteMethodBenchmarks`, `CompletePropertyBenchmarks`,
`CompleteIndexerBenchmarks`, `CompleteEventBenchmarks`) compare Mockolate
against Moq, NSubstitute, FakeItEasy and Imposter on the same scenarios.

For the per-phase staircase (raw BenchmarkDotNet output captured during the
refactor), see [`Docs/perf-baseline-v2.md`](../perf-baseline-v2.md).

## Trade-offs

- **First call after install pays the buffer install cost.** Each mock allocates
  one `IFastMemberBuffer?[]` sized to the type's member count, and the typed
  buffer instances are installed lazily on first interaction. Cost is amortised
  across the mock's lifetime.
- **Combined `Implementing<>` mocks stay on the legacy list path.** Their member
  ids are sized against the base mock; reworking that storage model is out of
  scope for v3. Recording goes through `RegisterInteraction → FallbackBuffer`,
  which is functionally identical to the v2 hot path.
- **Ref-struct setup paths are unaffected by D.** They keep their bespoke
  pipeline because ref-struct values can't flow through the typed buffer's
  generic delegates.
