# Architecture Decision Records (ADR)

## ADR-001: Use Supabase for PostgreSQL Hosting

**Date**: 2025-11-11

**Status**: Accepted

**Context**: Need a reliable, managed PostgreSQL database for time-series data, alarms, and audit logs.

**Decision**: Use Supabase for PostgreSQL hosting.

**Consequences**:
* ✅ Managed service reduces operational overhead
* ✅ Built-in authentication and real-time capabilities
* ✅ Free tier suitable for development and demos
* ✅ Easy migration path to self-hosted PostgreSQL if needed
* ⚠️ Vendor lock-in for Supabase-specific features (if used)

---

## ADR-002: EF Core for Data Access

**Date**: 2025-11-11

**Status**: Accepted

**Context**: Need ORM for database access with strong typing and migrations.

**Decision**: Use Entity Framework Core 8.0 with Npgsql provider.

**Consequences**:
* ✅ Type-safe queries with LINQ
* ✅ Code-first migrations
* ✅ Strong .NET ecosystem integration
* ⚠️ Performance overhead for high-frequency writes (mitigated by batching)

---

## ADR-003: NModbus for Modbus TCP

**Date**: 2025-11-11

**Status**: Accepted

**Context**: Need reliable Modbus TCP client/server implementation.

**Decision**: Use NModbus library (v3.0.72).

**Consequences**:
* ✅ Mature, well-tested library
* ✅ Supports both master and slave modes
* ✅ MIT licensed
* ⚠️ Limited to Modbus TCP (no RTU/ASCII in simulator)

---

## ADR-004: SignalR for Real-Time Updates

**Date**: 2025-11-11

**Status**: Accepted

**Context**: Need real-time push updates to web clients for telemetry and alarms.

**Decision**: Use ASP.NET Core SignalR.

**Consequences**:
* ✅ Native .NET integration
* ✅ Automatic fallback (WebSockets → Server-Sent Events → Long Polling)
* ✅ Strongly-typed hubs
* ⚠️ Requires persistent connections (scaling considerations)

---

## ADR-005: TDD with xUnit

**Date**: 2025-11-11

**Status**: Accepted

**Context**: Enforce test-first development and high code quality.

**Decision**: Use xUnit, FluentAssertions, Testcontainers, and Playwright.

**Consequences**:
* ✅ Tests document behavior
* ✅ Refactoring confidence
* ✅ Testcontainers provide real PostgreSQL for integration tests
* ⚠️ Requires discipline to write tests first

---

## ADR-006: Clean Architecture with Domain Isolation

**Date**: 2025-11-11

**Status**: Accepted

**Context**: Need maintainable, testable architecture that separates concerns.

**Decision**: Implement Clean Architecture with pure domain layer.

**Consequences**:
* ✅ Domain logic independent of infrastructure
* ✅ Easy to test domain in isolation
* ✅ Clear boundaries between layers
* ⚠️ More initial boilerplate

---

## ADR-007: Connection String in appsettings.json (Development Only)

**Date**: 2025-11-11

**Status**: Accepted (with caveats)

**Context**: Need database connection configuration for development.

**Decision**: Store connection string in `appsettings.json` for development; use environment variables or Azure Key Vault for production.

**Consequences**:
* ✅ Easy local development setup
* ✅ Clear documentation of required configuration
* ⚠️ **NEVER commit production credentials**
* ⚠️ Use `.gitignore` to exclude `appsettings.Development.json`

**Production Recommendation**:
```bash
# Use environment variable
export ConnectionStrings__DefaultConnection="postgresql://..."

# Or Azure Key Vault
# Or AWS Secrets Manager
# Or HashiCorp Vault
```

---

## ADR-008: Alarm Hysteresis as Percentage of Span

**Date**: 2025-11-11

**Status**: Accepted

**Context**: Need to prevent alarm chattering near thresholds.

**Decision**: Implement hysteresis as percentage of tag span (default 0.5%).

**Consequences**:
* ✅ Scales appropriately with tag ranges
* ✅ Prevents rapid on/off cycling
* ✅ Industry-standard approach
* ⚠️ Requires span to be configured correctly

---

## ADR-009: Delay-On and Delay-Off for Alarms

**Date**: 2025-11-11

**Status**: Accepted

**Context**: Need to filter transient spikes/dips in process values.

**Decision**: Implement configurable delay-on (default 2s) and delay-off (default 5s) timers.

**Consequences**:
* ✅ Reduces nuisance alarms
* ✅ Matches industrial SCADA behavior
* ⚠️ Delays alarm notification (acceptable trade-off)

---

## ADR-010: Modbus Register Map in JSON

**Date**: 2025-11-11

**Status**: Accepted

**Context**: Need authoritative source for Modbus register mappings.

**Decision**: Maintain `docs/modbus-map.json` as single source of truth.

**Consequences**:
* ✅ Prevents fabricated register addresses
* ✅ Documents all I/O points
* ✅ Can be validated in tests
* ⚠️ Must be kept in sync with simulator and acquisition code

---

## Future Decisions

* ADR-011: Authentication provider (OIDC, Azure AD, etc.)
* ADR-012: Charting library (Chart.js vs LightningChart)
* ADR-013: Deployment strategy (Docker, Azure App Service, etc.)
* ADR-014: Caching strategy for high-frequency reads
* ADR-015: Alarm shelving persistence



