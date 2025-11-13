# PlantVizSight

Real-time SCADA dashboard I built to learn more about .NET and C#. Started this project to get hands-on experience with industrial monitoring systems and process control.

## What I learned

The main focus was building a real-time Steam-to-Oil Ratio (SOR) analytics dashboard for SAGD operations. This was the most interesting part - calculating SOR on the fly and visualizing it with live charts.

### SOR Calculation

I used this formula:
```
SOR = Steam Injected (m³/d) ÷ Bitumen Produced (bbl/d)
```

For the simulation, I set up:
- **Steam Injection**: Base around 6,495 m³/d with sine wave variation and random noise
- **Bitumen Production**: Base around 2,150 bbl/d, inversely correlated with steam (when steam goes up, bitumen follows with a delay)

The simulator generates realistic time-series data using sine waves, random walk, and noise to make it feel like real sensor readings from a SAGD well.

## Tech Stack

- ASP.NET Core 9.0
- SignalR for real-time updates
- Modbus TCP simulator (no real hardware needed)
- PostgreSQL/Supabase for data storage
- AdminLTE for the dashboard UI
- Chart.js for live charts

## Getting Started

You'll need .NET 9.0 SDK:

```bash
dotnet restore
cd src/Dashboard.Web
dotnet run
```

Then open `http://localhost:8080` in your browser.

## Project Structure

- `src/Dashboard.Web` - Main web application
- `src/Dashboard.Domain` - Business logic and AESO API integration
- `src/Dashboard.Simulator` - Modbus TCP simulator that generates SAGD process data
- `src/Dashboard.Acquisition` - Data acquisition service
- `src/Dashboard.Persistence` - Database layer

## Configuration

Copy `appsettings.example.json` to `appsettings.json` and add your database connection string and AESO API key if you want live power grid data.

## Notes

This was a learning project. The Modbus simulator lets you test everything without needing actual industrial hardware. All the SAGD parameters (steam injection, bitumen production, reservoir temp, ESP status, etc.) are simulated to give realistic data for the analytics dashboard.
