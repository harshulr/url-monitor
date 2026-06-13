# Testing approach

I test the logic where a bug would be **silent and high-impact**, and skip areas that are either
low-risk or expensive to test well. The goal is a small, honest suite — not a coverage number.
This file grows with the build; below reflects what exists so far.

Run with:
```bash
dotnet test
```

## What I test (and why)

### Layer 2 — `HealthCheckExecutor` (unit, `HealthCheckExecutorTests`)
This is the most important logic in the system: it decides whether an endpoint shows **green or
red**. Three basic cases pin down the status -> success mapping, against a stubbed HTTP handler so
they're fast and deterministic (no real network):

- 2xx → success, status recorded, no error.
- 500 → failure, status recorded.
- Unreachable (`HttpRequestException`) → `StatusCode = null`, error message captured.

Kept intentionally small — these three cover the green / red / unreachable states a user actually
sees. Finer cases (timeout vs. shutdown cancellation) are handled in code but not separately tested
to keep the suite lean.

## What I deliberately skip (so far)

- **The consumer loop / hosted service wiring.** `ChannelConsumerWorker` is a thin loop over the
  channel that delegates to the (tested) executor and persists. Testing it well means spinning up a
  host and faking the channel; the logic worth testing already lives in the executor.
- **EF mapping / WAL / migrations.** Framework behavior, verified by running the app (tables created,
  `-wal`/`-shm` files present, seed inserted) rather than asserting on EF internals.

## How I sanity-check AI-generated tests
Generated tests can look thorough while asserting almost nothing. I check each test would actually
**fail if the behavior broke** — e.g. the shutdown test asserts an exception is thrown (not just
"no result"), and timeout vs. shutdown are separate tests because they take different code paths.
The stub handler honors cancellation so the shutdown test reflects real `HttpClient` behavior.
