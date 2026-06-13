# URL Health Monitor

A small full-stack service that periodically checks a list of URLs, records the results, and
exposes a web UI to view current health and history over time.

- **Backend:** ASP.NET Core Minimal API (.NET 10), EF Core + SQLite
- **Frontend:** Vite + React + TypeScript + Mantine
- **Tests:** xUnit

---

## What it does

1. A background scheduler checks every active URL on a fixed interval (60s) and also on demand.
2. Each check captures **HTTP status code** and **response time**, or records the error if the
   request never completed (DNS failure, timeout, connection refused).
3. Results are persisted to SQLite so history survives restarts.
4. The UI shows, at a glance, which URLs are healthy vs. not, lets you drill into a URL's history,
   and lists past **runs** (Job History).

---

## Running it

### Prerequisites
- .NET 10 SDK
- Node.js 20+

### Backend
```bash
cd backend
dotnet run            # http://localhost:5139
```
On first run it applies migrations, enables SQLite WAL mode, and seeds 10 sample URLs
(`urlmonitor.db` is created in `backend/`). The scheduler runs one batch immediately, then every
60 seconds.

### Frontend
```bash
cd frontend
npm install
npm run dev           # http://localhost:5173
```
API base URL is configured in `frontend/.env` (`VITE_API_BASE_URL`).

### Tests
```bash
dotnet test
```

---

## Architecture

### Backend — producer/consumer with `System.Threading.Channels`

```
SchedulerWorker (PeriodicTimer, 60s) ─┐
                                      ├─► HealthCheckProducer.QueueActiveChecksAsync(trigger)
POST /api/urls/sync ──────────────────┘        │ records a SchedulerJob (the run)
                                               │ queues one HealthCheckTask per active URL
                                                       ▼
                                        Channel<HealthCheckTask>  (singleton, unbounded)
                                                       ▼
                                        ChannelConsumerWorker (single reader)
                                               │ HealthCheckExecutor (ping + time)
                                               ▼ persists HealthCheckResult
```

**Why a channel?** It decouples *deciding to check* (the timer and the manual sync button) from
*doing the check* (the consumer). Both triggers funnel through one path, so a scheduled run and a
manual run behave identically. It's an in-process queue — deliberately not Kafka/RabbitMQ, which
would be overkill at this scope.

### Data model (SQLite)

- **MonitoredUrl** — the target (URL, name, active flag).
- **SchedulerJob** — one row per run cycle, tagged `Scheduled` or `Manual`.
- **HealthCheckResult** — one row per check (status, latency, success, error), linked to both a
  run (`JobId`) and a URL. Indexed on `(MonitoredUrlId, Timestamp)` for history and on `JobId` for
  per-run queries.

**WAL mode** is enabled at startup so the background worker can write while the API serves
concurrent reads without `database is locked` errors. (This is a **SQLite-specific** accommodation —
server databases like Postgres/SQL Server don't have this single-writer-blocks-readers
problem, so the `PRAGMA journal_mode=WAL` line would simply be dropped there.) Times are stored as UTC
and serialized with a `Z` suffix so the frontend renders them in the user's local time.

### Frontend

- `AppShell` with a header (live status + **Sync Now**) and `Tabs` for **Dashboard** / **Job History**.
- **Dashboard** — summary cards + an endpoints grid with color-coded status badges
  (green 2xx, red errors, gray pending), auto-refreshing every 15s. Click a row for that URL's history.
- **Job History** — an accordion of past runs; expanding one shows every endpoint checked in that run.
- A generic `DataTable<T>` component renders all three tables (endpoints, history, per-run results)
  from a column config — reused rather than duplicated.

---

## Decisions & scope cuts

The brief calls out a lot of "bonus territory." What I left out and why:

| Cut | Reasoning |
|-----|-----------|
| **Per-URL intervals / retry & backoff** | One global 60s interval keeps the scheduler trivial; per-URL scheduling isn't core to "see health over time." |
| **Auth / multi-tenancy** | No users in scope; auth would dwarf the actual feature. |
| **Alerting** | History/badges first; alerting is a layer on top of a working data pipeline. |
| **Adding URLs via the UI** | URLs are seeded server-side. A create form is easy to add but pulls in input validation + SSRF concerns I'd rather scope deliberately. |

### Why I built Jobs (and not one of the other bonuses)

I weighed Jobs against adding-URLs-via-UI, an uptime chart, and parallelizing checks. I chose
**Jobs** because it demonstrates a pattern that matters in a real system rather than a one-off
feature. A `SchedulerJob` is a **traceable run unit**: in an enterprise system the health monitor
is one job among many, and a run record (id + trigger + timestamp, with results linked back to it)
is the anchor you correlate logs and results to when something goes wrong and you need to trace it.

Honest trade-off: on the UI, the Job History view overlaps somewhat with the per-URL history — the
per-URL view is the more direct answer to "how has this looked over time." Jobs earn their place on
the **traceability / auditability** axis, not on adding a brand-new user-facing capability. If the
goal were pure product polish, the add-URLs form or a chart would have been the better pick.

---

## Testing

I tested the logic where a bug would be **silent and high-impact**, and skipped what's either
low-risk or expensive to test well. Small and honest over a coverage number. `dotnet test` runs them.

**Covered:**
- `HealthCheckExecutor` (unit, stubbed HTTP — fast, deterministic): the status → success mapping that
  decides green vs. red. 2xx → success; 500 → failure; unreachable (`HttpRequestException`) → null
  status + error captured.
- `HealthCheckProducer` (integration, real in-memory SQLite): records a run and queues one task per
  **active** URL (skipping inactive); returns null and queues nothing when none are active.

**Deliberately skipped:**
- **HTTP endpoint / `WebApplicationFactory` tests** — booting the host starts the background workers,
  which fire real network requests during the test. Doing it right needs a test config that disables
  the hosted services; worth it for production, not worth rushing here.
- **Frontend tests** — the UI is mostly presentational; the logic worth testing (status → badge) is
  trivial, verified by running the app. Next round: a badge-mapping unit test + one Playwright smoke.
- **The scheduler timer** — `PeriodicTimer` is framework code; the thing that runs on each tick
  (`HealthCheckProducer`) is tested directly.

I sanity-checked the generated tests by confirming each would actually fail if the behavior broke
(e.g. the unreachable test asserts a null status *and* a captured error, not just "not success").

---

## Known limitations / what feels brittle

- **Checks run sequentially.** The consumer uses a single reader, so URLs are pinged one at a time.
  Fine for 10; a list of hundreds would lag. Fix: parallel consumers or `Task.WhenAll` per batch with
  a concurrency cap.
- **Connection-refused checks take a few seconds** to fail, which — combined with
  sequential execution — stretches a batch. A shorter per-check timeout would tighten it.
- **Frontend polls every 15s.** Simple and good enough; a push channel (Server Sent Events) would be the
  next step for real-time.

---

## Productionalization

Choices here are tuned for a local review, not a deployment:

- **Migrations auto-apply at startup** (`db.Database.Migrate()`) — convenient for a demo; in production
  I'd run migrations as a separate, gated deploy step.
- **SQLite** is great for a single-instance app; a multi-instance deployment would move to Postgres/SQL
  Server (and then the in-process channel would need a real broker — see below). The startup
  `PRAGMA journal_mode=WAL` line is SQLite-only and would be dropped on a server database.
- **CORS** allows any loopback origin for dev; production would allow only the known frontend origin.
- **Config/secrets** would move to environment variables / a secrets store; no `appsettings.Development.json`
  is shipped.
- No containerization yet — `dotnet run` / `npm run dev` is enough for a local walkthrough.

---

## With another day

1. **Generate a typed API client** for the frontend from the backend's OpenAPI spec, replacing the
   hand-written `fetch` calls so request/response types stay in sync automatically.
2. **Parallelize checks** with a bounded concurrency limit.
3. **Add URLs through the UI** (with validation).
4. A simple **uptime/latency chart** per URL.
5. **Containerize** (Dockerfiles + compose for api / frontend / db).
6. **Swap the in-process channel for Kafka** *if* scope grows to multiple instances or durable queueing —
   the channel is right at current scope, but a broker would let producer and consumer scale and survive
   restarts independently.

---

## How AI was used

Built with Claude Code, layer by layer (database → engine → API → UI → jobs). AI handled scaffolding
and boilerplate; I directed the architecture, reviewed changes, and verified behavior by running
the app and reading logs/output at each step rather than trusting it blind. Examples where review
mattered: catching a stale-build run where migrations didn't apply, and a UTC→local time bug fixed 
at the serialization source rather than patched per-component.
