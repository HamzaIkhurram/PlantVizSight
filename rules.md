Below are two files you can copy directly into your repo root. Save them as `README.md` and `.cursorrules`.

---

# README.md

# Real-Time Industrial Process Monitoring & Control Dashboard

Production-grade, SCADA/DCS-style monitoring and control system built with **ASP.NET Core MVC (.NET 8)**, **SignalR**, a **Modbus TCP simulator**, and **Supabase (PostgreSQL)** for historical and alarm storage. Designed for credible industrial demos (e.g., CNRL-style workflows) with live data, alarm lifecycle, trending, and basic command writes.

## Why this exists

* Provide a realistic, end-to-end reference that mirrors plant HMI/SCADA flows without touching real I/O.
* Emphasize **testability** and **determinism** via **TDD** with a simulator and strong domain boundaries.

## Core Capabilities

* **Data Acquisition**: Modbus TCP master polling simulated PLC/DCS devices at 1–5 s cadence.
* **Realtime UI**: SignalR pushes to HMI dashboard with tiles, trends, and status.
* **Alarm Engine**: HH/H/L/LL + ROC, hysteresis, on/off delays, acknowledge/shelve lifecycle.
* **Historian**: Time-series logging to Supabase; trend rollups for long windows.
* **Control Writes**: Setpoints/commands to simulator with interlocks and audit.
* **Security**: OIDC login; roles: Viewer, Operator, Engineer, Admin; audit trail.

## Tech Stack

* **Backend**: ASP.NET Core MVC (.NET 8), C#, SignalR.
* **Data**: Supabase (PostgreSQL). EF Core for migrations.
* **Charts**: Chart.js (or LightningChart) in MVC views.
* **Simulator**: In-process C# Modbus TCP **slave** (NModbus-based). Optionally swap for an external tool.
* **Testing**: xUnit, FluentAssertions, Testcontainers (Postgres), Playwright (UI), Bogus (fakes), Respawn (DB reset).

## Project Structure

```
/ src
  /Dashboard.Web            # ASP.NET Core MVC + SignalR + Views
  /Dashboard.Domain         # Core domain: Tag, Alarm, Limits, ROC, Units
  /Dashboard.Acquisition    # Modbus master + poll scheduler + mapping
  /Dashboard.Simulator      # Modbus slave + process models (temp/press/flow/level)
  /Dashboard.Persistence    # EF Core DbContext, entities, migrations
  /Dashboard.Contracts      # DTOs, messages, shared interfaces
/ tests
  /Dashboard.Domain.Tests
  /Dashboard.Acquisition.Tests
  /Dashboard.Web.IntegrationTests
  /Dashboard.UI.Tests       # Playwright
/ docs
  DECISIONS.md
```

## Environment

* **.NET 8 SDK** required.
* **Supabase**: create a project → get connection string; set `DATABASE_URL` in user secrets or `.env`.
* **Ports**: Web (5173 or 5000/5001), Simulator (default 5020), SignalR (`/hubs/telemetry`).

## Configuration

Appsettings keys (override with env vars):

```json
{
  "Acquisition": {
    "Devices": [
      { "Name": "UnitA", "Host": "127.0.0.1", "Port": 5020, "ScanMs": 2000 },
      { "Name": "UnitB", "Host": "127.0.0.1", "Port": 5021, "ScanMs": 5000 }
    ],
    "JitterMs": 200
  },
  "Historian": {
    "RetentionDays": 60,
    "Rollups": [ "1m", "5m" ]
  }
}
```

## Domain Primitives

* **Tag**: `TagId`, `Name`, `EU` (engineering units), `Scale`, `Offset`, `Span`, `DataType`, `ReadRegister`, `WriteRegister`.
* **Limits**: `HH`, `H`, `L`, `LL`, `HystPct` (default 0.5%), `DelayOnMs`, `DelayOffMs`.
* **Alarm**: `AlarmId`, `TagId`, `Type` (High/Low/ROC), `Severity` (Info/Low/High/Critical), `State` (Active, RTN, Acked), `ShelvedUntil`.

## Database (Supabase / PostgreSQL)

```sql
create table tag (
  tag_id uuid primary key,
  name text unique not null,
  eu text not null,
  scale double precision not null default 1,
  offset double precision not null default 0,
  span_low double precision not null default 0,
  span_high double precision not null default 100,
  datatype text not null check (datatype in ('int16','uint16','float','bool')),
  read_register int,
  write_register int,
  site text default 'default',
  created_at timestamptz default now()
);

create table tag_sample (
  ts timestamptz not null,
  tag_id uuid not null references tag(tag_id) on delete cascade,
  value double precision not null,
  quality smallint not null default 192, -- OPC-like Good=192
  primary key (ts, tag_id)
);

create table alarm_rule (
  alarm_rule_id uuid primary key,
  tag_id uuid references tag(tag_id) on delete cascade,
  type text not null check (type in ('HH','H','L','LL','ROC')),
  threshold double precision,
  severity smallint not null check (severity between 1 and 4),
  hyst_pct double precision not null default 0.5,
  delay_on_ms int not null default 2000,
  delay_off_ms int not null default 5000
);

create table alarm_event (
  alarm_event_id uuid primary key,
  alarm_rule_id uuid not null references alarm_rule(alarm_rule_id),
  tag_id uuid not null references tag(tag_id),
  ts_start timestamptz not null,
  ts_end timestamptz,
  state text not null check (state in ('Active','RTN','Acked')),
  ack_by text,
  ack_ts timestamptz,
  message text
);

create table audit_event (
  audit_event_id uuid primary key,
  user_name text,
  action text not null,
  details jsonb,
  ts timestamptz default now()
);
```

Indexes you should add:

```sql
create index idx_tag_sample_tag_ts on tag_sample(tag_id, ts desc);
create index idx_alarm_event_tag_ts on alarm_event(tag_id, ts_start desc);
create index idx_audit_ts on audit_event(ts desc);
```

## TDD Workflow (Short and Brutal)

1. **Pick a thin slice** (e.g., High alarm with hysteresis).
2. **Write a failing unit test** in `Dashboard.Domain.Tests` that expresses the rule with edge cases.
3. **Implement the minimum** in `Dashboard.Domain` to pass.
4. **Refactor**: remove duplication, enforce invariants.
5. **Add integration test** if the slice crosses boundaries (e.g., poll → process → DB → SignalR).
6. **Add acceptance/UI test** in Playwright only for critical operator paths (alarm appears, can Ack, trend loads).
7. Repeat. Keep PRs < 300 lines.

### Example: Alarm Hysteresis (Domain Unit Test Sketch)

```csharp
[Fact]
public void HighAlarm_trips_above_threshold_and_clears_below_threshold_minus_hysteresis()
{
    var rule = AlarmRule.High(threshold: 80, hystPct: 0.5); // of span
    var span = (low: 0, high: 100);
    var engine = new AlarmEngine(rule, span);

    engine.Observe(79); engine.State.Should().Be(AlarmState.Normal);
    engine.Observe(81); engine.State.Should().Be(AlarmState.Active);
    engine.Observe(80.4); engine.State.Should().Be(AlarmState.Active); // within hysteresis
    engine.Observe(79.4); engine.State.Should().Be(AlarmState.Normal);
}
```

### Example: Acquisition → SignalR (Integration Test Sketch)

```csharp
[Fact]
public async Task PollCycle_pushes_latest_value_to_clients_within_500ms()
{
    using var app = new TestHostBuilder().WithInMemorySimulator().Build();
    var hubClient = await app.ConnectSignalRClient("/hubs/telemetry");

    await app.Simulator.Set("UnitA.Flow", 120.0);
    var sw = Stopwatch.StartNew();
    var msg = await hubClient.ExpectAsync<TelemetryMessage>(TimeSpan.FromSeconds(2));

    msg.Tag.Should().Be("UnitA.Flow");
    msg.Value.Should().BeApproximately(120.0, 0.1);
    sw.ElapsedMilliseconds.Should().BeLessThan(500);
}
```

## How to Run (Dev)

```bash
# 1) Ensure DATABASE_URL is set to Supabase connection string
# 2) Create DB schema
dotnet tool restore
dotnet ef database update --project src/Dashboard.Persistence

# 3) Start simulator + web
dotnet run --project src/Dashboard.Simulator &
dotnet run --project src/Dashboard.Web
```

## How to Test

```bash
# Unit + integration (spins Postgres in a container)
dotnet test

# UI (Playwright)
dotnet tool run playwright install --with-deps
DOTNET_ENVIRONMENT=Development dotnet test tests/Dashboard.UI.Tests
```

## Coding Standards (Essentials)

* Pure domain stays in `Dashboard.Domain`; no EF, no HTTP, no SignalR leakage.
* Compose features via DI; zero static singletons.
* All external boundaries (Modbus, DB, SignalR, Clock) are **ports** (interfaces) with adapters.
* Every PR must include at least one failing test first.

---

# .cursorrules

# Goals

* Keep Cursor grounded: no speculative APIs, no fabricated Modbus addresses, no fake NuGet packages.
* Enforce **TDD**: write or update tests **first**; then implement just enough to pass.

# Project Context (authoritative)

* Name: Real-Time Industrial Process Monitoring & Control Dashboard
* Runtime: .NET 8 (C#). Pattern: Clean Architecture (Domain, Application/Acquisition, Infrastructure/Persistence, Web).
* Realtime: SignalR hub at `/hubs/telemetry`.
* Data source: Modbus TCP **master** polling an **in-process Modbus slave** simulator.
* Database: Supabase (PostgreSQL) via EF Core. Tables: `tag`, `tag_sample`, `alarm_rule`, `alarm_event`, `audit_event` (see README for schema).

# Grounding Rules (anti-hallucination)

1. **Do not invent** library APIs or configuration keys. If an API call is unclear, propose a minimal interface and mark with `// TODO: verify API against docs`.
2. Use **real** NuGet packages only: `Microsoft.AspNetCore.SignalR`, `NModbus` (or successor), `xunit`, `FluentAssertions`, `Testcontainers`, `Microsoft.Playwright`, `EFCore`. If unsure of exact namespace or method name, create an adapter interface in our code and implement after a doc check.
3. **No fabricated Modbus mappings.** All registers/coils must come from `/docs/modbus-map.json`. If the file/entry is missing, stop and create/update the map with tests.
4. **Every new feature must start with a failing test.** If Cursor suggests code without a corresponding test diff, reject it.
5. Generated code must **compile** and tests must **run locally**. If external I/O is unavailable, use test doubles or the in-process simulator.
6. **No silent fallbacks.** If a dependency is missing (DB, hub, simulator), fail fast with clear error messages.
7. Prefer **deterministic time** via `IClock` abstraction for tests; avoid `DateTime.UtcNow` directly.
8. **Migration safety**: never drop tables/columns in automatic migrations without an explicit migration file and approval note in `docs/DECISIONS.md`.
9. **Security defaults**: deny writes unless role `Operator` or higher and user is authenticated. Tests must cover authorization.
10. **Performance budget**: 500 ms poll→UI path under nominal load. Add performance tests where possible.

# TDD Enforcement (process)

* When adding code, first add or modify tests in `/tests` showing expected behavior.
* Only then write implementation in `/src` to satisfy tests. Keep changes minimal.
* After green, refactor for clarity and re-run tests.

# File & Prompt Conventions

* Keep domain purity: new rules or algorithms go to `Dashboard.Domain` with unit tests.
* Integration points (Modbus, DB, SignalR) must have interfaces in `Dashboard.Contracts` and adapters in respective projects.
* If Cursor is asked to “optimize” or “refactor” without failing tests, create tests that lock the behavior **before** refactoring.
* Use **explicit** exceptions/messages instead of comments for unimplemented features (`throw new NotImplementedException("reason")`).

# Commit Message Template

```
feat(domain): add High alarm with hysteresis

- failing test for threshold/hysteresis
- minimal implementation in AlarmEngine
- add EF entity mapping for alarm_rule
- migration: create alarm_rule table
```

# Checklists

* [ ] Failing test exists for the change.
* [ ] Implementation minimal and behind interfaces.
* [ ] DB migrations added & applied in CI.
* [ ] SignalR hub contracts unchanged or versioned.
* [ ] Docs updated: `/docs/modbus-map.json`, `README.md`, `docs/DECISIONS.md`.

# Safe Defaults

* Default to **in-process simulator** for CI determinism.
* Skip UI/Playwright tests in CI for PRs marked `ci/quick` label.

# Non-goals

* No OPC UA in MVP.
* No HA/failover in MVP.
* No safety-critical interlocks beyond basic demo logic.
