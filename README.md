# PlantSightVizu

A real-time SCADA dashboard built with .NET and C# for monitoring SAGD (Steam-Assisted Gravity Drainage) operations. This project focuses on real-time SOR (Steam-to-Oil Ratio) analytics and well pair performance monitoring using live process data simulation.

## Tech Stack

- ASP.NET Core 9.0
- SignalR for real-time updates
- Modbus TCP simulator (no real hardware needed)
- PostgreSQL/Supabase for data storage
- AdminLTE for the dashboard UI
- Chart.js for live charts

## Getting Started

```bash
dotnet restore
cd src/Dashboard.Web
dotnet run
```

Then open `http://localhost:8080` in your browser.

---

## SAGD SOR Analytics Dashboard

Real-time Steam-to-Oil Ratio analytics dashboard for monitoring SAGD operations and process efficiency.

![SAGD SOR Dashboard Overview](images/SAGD_SOR_1.png.png)
**Real-Time SOR Monitoring** - Live calculation and visualization of Steam-to-Oil Ratio with operational status indicators and trend analysis.

![SAGD SOR Analytics](images/SAGD_SOR_2.png.png)
**Performance Metrics & Trends** - Detailed analytics showing SOR efficiency, production rates, and operational parameters over time.

![SAGD SOR Detailed View](images/SAGD_SOR_3%20(1).png)
**Process Control Dashboard** - Comprehensive view of SAGD operations including steam injection rates, bitumen production, and system health metrics.

### Calculations & Assumptions

**SOR Formula:**
```
SOR = Steam Injected (m³/d) ÷ Bitumen Produced (bbl/d)
```

**Simulation Parameters:**
- **Steam Injection**: Base value ~6,495 m³/d with sine wave variation (amplitude ±300) and random noise, bounded 5,000-8,000 m³/d
- **Bitumen Production**: Base value ~2,150 bbl/d with inverse correlation to steam (phase-shifted sine wave) plus random trend and noise, bounded 1,500-2,800 bbl/d

The simulator uses sine waves, random walk trends, and noise to simulate realistic sensor readings from SAGD wells, allowing full system testing without industrial hardware.

---

## Well Pair Performance Dashboard

Real-time monitoring of all 66 well pairs with live KPIs, performance analytics, and anomaly detection.

![Well Pair Performance Dashboard Overview](images/well-pair-dashboard-overview.png.png)
**Live Well Pair Performance Grid** - Real-time status for all well pairs including subcool temperature, wellhead pressure, production rates, and SOR ratios.

![Well Pair Performance Analytics](images/well-pair-analytics.png.png)
**Performance Analytics** - Four analytical views showing production distribution, temperature control, pressure-production correlation, and SOR efficiency comparison across facilities.

![Well Pair Heatmaps and Rankings](images/well-pair-heatmaps.png.png)
**Heatmaps & Anomaly Detection** - Pad-level performance heatmaps and top 10 well pairs ranked by production rate. Anomaly detection identifies wells operating outside optimal parameters.

### Calculations & Assumptions

**Well Pair Distribution:**
- 66 well pairs across 2 facilities (Facility A: 31 pairs, Facility B: 35 pairs)
- 16 pads total (8 per facility) with well pairs distributed evenly

**Production Rate Simulation:**
- Facility A base: 383.4 bbl/d per well pair
- Facility B base: 360.8 bbl/d per well pair
- Calculation: `base + sine(0.5 freq, ±30 amplitude) + random_walk(±30) + noise(±20)`
- Bounded: 250-500 bbl/d

**Subcool Temperature Modeling:**
- Base: 187.0°C (optimal range: 175-205°C)
- Calculation: `187.0 + sine(0.3 freq, ±8°C) + noise(±3°C)`
- Bounded: 175-205°C for normal operations

**Wellhead Pressure Simulation:**
- Base: 9,000 kPa
- Calculation: `9000 + sine(0.4 freq, ±150 kPa) + noise(±100 kPa)`
- Facility A control: ±150 kPa (tighter), Facility B: ±200 kPa
- Bounded: 8,000-10,500 kPa

**Steam Injection Rates:**
- Base: 1,180 Sm³/d per well pair
- Calculation: `1180 + sine(0.6 freq, ±80 Sm³/d) + noise(±50 Sm³/d)`
- Bounded: 1,000-1,400 Sm³/d

**SOR Ratio Calculation:**
```
SOR = Steam Injection (Sm³/d) ÷ Production Rate (bbl/d)
```
- Defaults: Facility A = 3.20, Facility B = 3.19 (when production is zero)

**System Uptime:**
- Facility A: 94.6% base, Facility B: 94.2% base
- Calculation: `base ± 1% noise`
- Bounded: 90-100%

**Status Logic:**
- **Alert (Red)**: Subcool <170°C or >210°C, Pressure >10,500 kPa, Production drop >20%, Uptime <90%
- **Caution (Yellow)**: Subcool outside 175-205°C (but within 170-210°C), Pressure >10,200 kPa, Uptime <92%
- **Normal (Green)**: All parameters within acceptable ranges

**Real-Time KPI Aggregation:**
- Total Bitumen Production: Sum of all production rates
- Total Steam Injection: Sum of all steam injection rates
- Average Subcool Temperature: Mean across all 66 well pairs
- Average SOR: Mean SOR ratio across all well pairs
- System Uptime: Weighted average based on facility uptime
- Active Well Pairs: Count of all operational well pairs

**Update Frequency:** All data updates every second via SignalR for real-time visualization.

**Key Assumptions:**
- All well pairs operational initially (no offline wells)
- Facility A performs slightly better (higher production, tighter pressure control)
- Optimal subcool zone 175-205°C based on SAGD best practices
- Production rates correlate with steam injection efficiency

---

## Project Structure

- `src/Dashboard.Web` - Main web application
- `src/Dashboard.Domain` - Business logic and AESO API integration
- `src/Dashboard.Simulator` - Modbus TCP simulator that generates SAGD process data
- `src/Dashboard.Acquisition` - Data acquisition service
- `src/Dashboard.Persistence` - Database layer

## Configuration

Copy `appsettings.example.json` to `appsettings.json` and add your database connection string and AESO API key if you want live power grid data.

## Notes

This was a learning project focused on .NET and C# development. The Modbus simulator enables full system testing without real industrial hardware. All SAGD parameters (steam injection, bitumen production, reservoir temperature, ESP status, etc.) are simulated to provide realistic data for the analytics dashboards.
