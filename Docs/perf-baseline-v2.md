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
    --filter '*OptimizedMockComparisonBenchmarks*' \
    --job InProcess
```

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

| Member | Baseline | After 4.2 | After 5.3 | After 6.1 |
|---|---:|---:|---:|---:|
| Method (2-arg) | ~189 ns · ~300 B | _tbd_ | _tbd_ | _tbd_ |
| Property get | ~287 ns · ~260 B | _tbd_ | _tbd_ | _tbd_ |
| Indexer get (1 key) | ~298 ns · ~172 B | _tbd_ | _tbd_ | _tbd_ |
| Event subscribe | ~185 ns · ~369 B | _tbd_ | _tbd_ | _tbd_ |
