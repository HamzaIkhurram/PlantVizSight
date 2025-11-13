# Quick Start Guide

Get up and running in 5 minutes!

## Prerequisites

- [ ] .NET 8 SDK installed ([Download](https://dotnet.microsoft.com/download/dotnet/8.0))
- [ ] Supabase account created

## Setup Steps

### 1. Install .NET 8 SDK

```bash
# Verify installation
dotnet --version
# Should show: 8.0.x
```

If not installed, download from: https://dotnet.microsoft.com/download/dotnet/8.0

### 2. Create Database Schema

1. Go to your Supabase project: https://inrdnmdguyyfgwbelrhb.supabase.co
2. Click **SQL Editor** in sidebar
3. Copy contents of `docs/supabase-schema.sql`
4. Paste and click **Run**

✅ You should see 5 tables created with sample data

### 3. Restore Packages

```bash
dotnet restore
```

### 4. Build Project

```bash
dotnet build
```

### 5. Run Application

```bash
dotnet run --project src/Dashboard.Web
```

### 6. Open Browser

Navigate to: http://localhost:5000

🎉 **You're done!**

## What's Configured

✅ **Supabase Connection**: Already configured in `appsettings.json`
- URL: `https://inrdnmdguyyfgwbelrhb.supabase.co`
- Database: PostgreSQL connection ready
- Credentials: Pre-configured

✅ **Sample Data**: 4 tags and 2 alarm rules loaded

✅ **Project Structure**: All projects created and referenced

## Next Steps

### Run Tests

```bash
dotnet test
```

### Start Simulator (Optional)

```bash
dotnet run --project src/Dashboard.Simulator
```

### Explore Database

1. Go to Supabase dashboard
2. Click **Table Editor**
3. View `tag`, `alarm_rule` tables

## Common Issues

### "dotnet: command not found"
➜ Install .NET 8 SDK and restart terminal

### "Connection failed"
➜ Run `docs/supabase-schema.sql` in Supabase SQL Editor

### "Port 5000 in use"
➜ Use different port:
```bash
dotnet run --project src/Dashboard.Web --urls "http://localhost:5173"
```

## Documentation

- **Full Setup**: `docs/SETUP.md`
- **Supabase Guide**: `docs/SUPABASE_SETUP.md`
- **Architecture**: `docs/DECISIONS.md`
- **Development Rules**: `rules.md`

## Project Structure

```
PlantSight/
├── src/
│   ├── Dashboard.Web/          # ASP.NET Core MVC + SignalR
│   ├── Dashboard.Domain/       # Core business logic
│   ├── Dashboard.Persistence/  # EF Core + Database
│   ├── Dashboard.Acquisition/  # Modbus data acquisition
│   ├── Dashboard.Simulator/    # Modbus TCP simulator
│   └── Dashboard.Contracts/    # Shared interfaces/DTOs
├── tests/
│   ├── Dashboard.Domain.Tests/
│   ├── Dashboard.Acquisition.Tests/
│   ├── Dashboard.Web.IntegrationTests/
│   └── Dashboard.UI.Tests/
└── docs/
    ├── supabase-schema.sql     # Database schema
    ├── modbus-map.json         # Register mappings
    └── *.md                    # Documentation
```

## Support

Need help? Check:
1. `README.md` - Comprehensive guide
2. `docs/SETUP.md` - Detailed setup instructions
3. `docs/SUPABASE_SETUP.md` - Database setup
4. Open an issue in the repository

---

**Ready to develop?** Follow TDD workflow in `rules.md`! 🚀



