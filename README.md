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
  supabase-schema.sql
  modbus-map.json
```

## Prerequisites

* **.NET 8 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
* **Supabase Account** - [Sign up here](https://supabase.com)
* **Git** (optional)

## Setup Instructions

### 1. Install .NET 8 SDK

Download and install the .NET 8 SDK from [https://dotnet.microsoft.com/download/dotnet/8.0](https://dotnet.microsoft.com/download/dotnet/8.0)

Verify installation:
```bash
dotnet --version
# Should show 8.0.x
```

### 2. Configure Supabase

The Supabase connection is already configured in `appsettings.json` with your credentials:

* **URL**: `https://inrdnmdguyyfgwbelrhb.supabase.co`
* **Database**: PostgreSQL connection configured
* **Connection String**: Already set in `appsettings.json`

### 3. Create Database Schema

Run the SQL script in Supabase SQL Editor:

1. Go to your Supabase project dashboard
2. Navigate to **SQL Editor**
3. Copy the contents of `docs/supabase-schema.sql`
4. Execute the script

This will create all necessary tables, indexes, and sample data.

### 4. Restore NuGet Packages

```bash
dotnet restore
```

### 5. Build the Solution

```bash
dotnet build
```

### 6. Run Database Migrations (Optional)

If you want to use EF Core migrations instead of the SQL script:

```bash
dotnet tool restore
dotnet ef migrations add InitialCreate --project src/Dashboard.Persistence --startup-project src/Dashboard.Web
dotnet ef database update --project src/Dashboard.Persistence --startup-project src/Dashboard.Web
```

## How to Run (Dev)

### Start the Web Application

```bash
dotnet run --project src/Dashboard.Web
```

The application will start on:
* HTTP: `http://localhost:5000`
* HTTPS: `https://localhost:5001`

### Start the Simulator (Optional - for Modbus testing)

In a separate terminal:

```bash
dotnet run --project src/Dashboard.Simulator
```

## How to Test

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/Dashboard.Domain.Tests

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### UI Tests (Playwright)

```bash
# Install Playwright browsers (first time only)
dotnet tool run playwright install --with-deps

# Run UI tests
dotnet test tests/Dashboard.UI.Tests
```

## Configuration

### appsettings.json

Key configuration sections:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Your Supabase PostgreSQL connection string"
  },
  "Supabase": {
    "Url": "https://your-project.supabase.co",
    "AnonKey": "your-anon-key",
    "ServiceRoleKey": "your-service-role-key"
  },
  "Acquisition": {
    "Devices": [
      { "Name": "UnitA", "Host": "127.0.0.1", "Port": 5020, "ScanMs": 2000 }
    ]
  },
  "Historian": {
    "RetentionDays": 60,
    "Rollups": [ "1m", "5m" ]
  }
}
```

### Environment Variables

You can override settings using environment variables or `.env` file (not committed to git):

```bash
DATABASE_URL=postgresql://...
SUPABASE_URL=https://...
SUPABASE_ANON_KEY=...
```

## Architecture

### Domain-Driven Design

* **Dashboard.Domain**: Pure domain logic (no dependencies on infrastructure)
* **Dashboard.Contracts**: Interfaces and DTOs
* **Dashboard.Persistence**: EF Core implementation
* **Dashboard.Acquisition**: Modbus data acquisition
* **Dashboard.Web**: MVC application and SignalR hubs

### Key Patterns

* **Repository Pattern**: Data access abstraction
* **Unit of Work**: Transaction management
* **Dependency Injection**: All services registered in DI container
* **TDD**: Test-first development approach

## Database Schema

See `docs/supabase-schema.sql` for the complete schema. Key tables:

* `tag`: Process variable definitions
* `tag_sample`: Time-series historian data
* `alarm_rule`: Alarm threshold definitions
* `alarm_event`: Alarm lifecycle events
* `audit_event`: User action audit trail

## Modbus Mapping

See `docs/modbus-map.json` for register mappings. Example:

* **Address 0**: UnitA.Temp (Temperature, 0-200°C)
* **Address 1**: UnitA.Pressure (Pressure, 0-50 bar)
* **Address 2**: UnitA.Flow (Flow, 0-500 m³/h)

## Development Workflow

1. **Write failing test** in appropriate test project
2. **Implement minimum code** to pass the test
3. **Refactor** while keeping tests green
4. **Commit** with descriptive message

### Commit Message Template

```
feat(domain): add High alarm with hysteresis

- failing test for threshold/hysteresis
- minimal implementation in AlarmEngine
- add EF entity mapping for alarm_rule
- migration: create alarm_rule table
```

## Troubleshooting

### .NET SDK not found

Install .NET 8 SDK from [https://dotnet.microsoft.com/download/dotnet/8.0](https://dotnet.microsoft.com/download/dotnet/8.0)

### Database connection errors

1. Verify Supabase connection string in `appsettings.json`
2. Check that database schema is created (run `docs/supabase-schema.sql`)
3. Ensure your IP is allowed in Supabase project settings

### Port already in use

Change ports in `Properties/launchSettings.json` or use:

```bash
dotnet run --project src/Dashboard.Web --urls "http://localhost:5173"
```

## Contributing

1. Follow TDD workflow
2. Keep domain pure (no infrastructure dependencies)
3. Write tests first
4. Update documentation
5. Follow commit message template

## License

MIT

## Support

For issues or questions, please open an issue in the repository.



