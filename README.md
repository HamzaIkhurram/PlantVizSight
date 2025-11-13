# PlantSightVizu

I wanted to further develop my understanding with .NET and C#, so I built this real-time SCADA dashboard to get hands-on experience with industrial monitoring systems and process control.

The core learning was around SAGD SOR real-time analytics. I focused on building a dashboard that calculates and visualizes Steam-to-Oil Ratio (SOR) on the fly using live process data.

## SOR Calculation

For calculating SOR, I used this formula:
```
SOR = Steam Injected (m³/d) ÷ Bitumen Produced (bbl/d)
```

To create the live simulation, I developed a Modbus simulator that generates realistic estimates:
- **Steam Injection Estimate**: Base value around 6,495 m³/d with sine wave variation (amplitude ~300) and random noise, bounded between 5,000-8,000 m³/d
- **Bitumen Production Estimate**: Base value around 2,150 bbl/d with inverse correlation to steam (phase-shifted sine wave) plus random trend and noise, bounded between 1,500-2,800 bbl/d

The simulator generates realistic time-series data using sine waves, random walk trends, and noise to simulate actual sensor readings from a SAGD well. This lets you test the entire system without needing real industrial hardware.

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
