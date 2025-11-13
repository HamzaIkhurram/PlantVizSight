# PlantSight

Real-time SCADA dashboard for monitoring industrial processes and SAGD operations. Built during my time working with process control systems.

## What it does

- Live telemetry from Modbus devices (simulated for demo)
- Alarm monitoring with configurable thresholds
- AESO power grid data integration
- SAGD well analytics and steam-to-oil ratio tracking
- Real-time charts using SignalR

## Tech stack

- ASP.NET Core 9.0
- SignalR for real-time updates
- PostgreSQL/Supabase
- AdminLTE dashboard theme
- Chart.js for visualization

## Getting started

You'll need the .NET 9.0 SDK installed.

```bash
dotnet restore
cd src/Dashboard.Web
dotnet run
```

Open `http://localhost:8080` in your browser.

## Project structure

- `src/Dashboard.Web` - main web app
- `src/Dashboard.Domain` - business logic
- `src/Dashboard.Simulator` - Modbus TCP simulator
- `src/Dashboard.Acquisition` - data acquisition service
- `src/Dashboard.Persistence` - database stuff

## Configuration

Copy `appsettings.example.json` to `appsettings.json` and add your:
- Database connection string
- AESO API key (if you want live power data)

## Notes

This was a learning project to better understand industrial monitoring systems. The Modbus simulator generates realistic SAGD process data for testing without needing actual hardware.

Feel free to use this as a starting point for your own projects.
