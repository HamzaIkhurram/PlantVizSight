# Setup Guide

## Prerequisites

1. **.NET 8 SDK**
   - Download: https://dotnet.microsoft.com/download/dotnet/8.0
   - Verify: `dotnet --version` should show 8.0.x

2. **Supabase Account**
   - Sign up: https://supabase.com
   - Create a new project

3. **Git** (optional)
   - Download: https://git-scm.com/downloads

4. **Visual Studio 2022** or **VS Code** (recommended)
   - VS 2022: https://visualstudio.microsoft.com/
   - VS Code: https://code.visualstudio.com/

## Step-by-Step Setup

### 1. Clone or Download Repository

```bash
git clone <repository-url>
cd PlantSight
```

Or download and extract the ZIP file.

### 2. Install .NET 8 SDK

If not already installed:

**Windows:**
1. Download installer from https://dotnet.microsoft.com/download/dotnet/8.0
2. Run installer
3. Restart terminal/IDE

**macOS:**
```bash
brew install dotnet@8
```

**Linux (Ubuntu):**
```bash
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 8.0
```

Verify:
```bash
dotnet --version
# Should output: 8.0.x
```

### 3. Configure Supabase

#### A. Create Supabase Project

1. Go to https://supabase.com
2. Sign in or create account
3. Click "New Project"
4. Fill in project details
5. Wait for project to be ready (~2 minutes)

#### B. Get Connection Details

Your connection details are already configured in `appsettings.json`:

* **URL**: `https://inrdnmdguyyfgwbelrhb.supabase.co`
* **Anon Key**: Already set
* **Service Role Key**: Already set
* **Database Password**: `Hamza@123`

#### C. Create Database Schema

1. In Supabase dashboard, go to **SQL Editor**
2. Click **New Query**
3. Copy entire contents of `docs/supabase-schema.sql`
4. Paste into SQL Editor
5. Click **Run** or press `Ctrl+Enter`

You should see success messages for:
- Tables created (tag, tag_sample, alarm_rule, alarm_event, audit_event)
- Indexes created
- Sample data inserted

#### D. Verify Schema

In Supabase dashboard:
1. Go to **Table Editor**
2. You should see tables: `tag`, `tag_sample`, `alarm_rule`, `alarm_event`, `audit_event`
3. Click on `tag` table - should see 4 sample tags (UnitA.Temp, UnitA.Pressure, etc.)

### 4. Restore NuGet Packages

```bash
cd PlantSight
dotnet restore
```

This will download all required packages:
- Entity Framework Core
- Npgsql (PostgreSQL driver)
- NModbus
- SignalR
- xUnit and test libraries

### 5. Build Solution

```bash
dotnet build
```

You should see:
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

### 6. Run Tests (Optional but Recommended)

```bash
dotnet test
```

This verifies everything is set up correctly.

### 7. Run the Application

```bash
dotnet run --project src/Dashboard.Web
```

You should see:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
```

### 8. Open in Browser

Navigate to:
- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001

You should see the dashboard home page.

## Troubleshooting

### Error: "dotnet: command not found"

**Solution**: .NET SDK not installed or not in PATH.
- Reinstall .NET 8 SDK
- Restart terminal
- On Windows, restart computer if needed

### Error: "Connection to database failed"

**Solution**: Check Supabase connection string.

1. Verify `appsettings.json` has correct connection string
2. In Supabase dashboard, go to **Settings** → **Database**
3. Copy connection string (Pooler mode recommended)
4. Update `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=aws-0-us-east-1.pooler.supabase.com;Port=6543;Database=postgres;Username=postgres.inrdnmdguyyfgwbelrhb;Password=Hamza@123"
  }
}
```

### Error: "Table 'tag' does not exist"

**Solution**: Database schema not created.

1. Go to Supabase SQL Editor
2. Run `docs/supabase-schema.sql`
3. Verify tables exist in Table Editor

### Error: "Port 5000 already in use"

**Solution**: Change port or stop conflicting process.

**Option 1: Use different port**
```bash
dotnet run --project src/Dashboard.Web --urls "http://localhost:5173"
```

**Option 2: Stop conflicting process**
- Windows: `netstat -ano | findstr :5000` then `taskkill /PID <pid> /F`
- macOS/Linux: `lsof -ti:5000 | xargs kill -9`

### Error: "NuGet package restore failed"

**Solution**: Clear NuGet cache and retry.

```bash
dotnet nuget locals all --clear
dotnet restore
```

### Build Warnings about Nullable Reference Types

**Solution**: These are informational and can be ignored for now. To suppress:

Add to `.csproj` files:
```xml
<PropertyGroup>
  <NoWarn>$(NoWarn);CS8618;CS8602;CS8604</NoWarn>
</PropertyGroup>
```

## Next Steps

1. **Explore the Dashboard**
   - View sample tags
   - Check alarm configuration
   - Review historical data

2. **Start the Simulator** (optional)
   ```bash
   dotnet run --project src/Dashboard.Simulator
   ```

3. **Run Tests**
   ```bash
   dotnet test
   ```

4. **Read Documentation**
   - `README.md` - Project overview
   - `docs/DECISIONS.md` - Architecture decisions
   - `docs/modbus-map.json` - Modbus register mappings

5. **Start Development**
   - Follow TDD workflow in `rules.md`
   - Write tests first
   - Implement features

## Development Tools

### Recommended VS Code Extensions

- C# (Microsoft)
- C# Dev Kit (Microsoft)
- NuGet Package Manager
- REST Client (for API testing)
- SQL Tools (for database queries)

### Recommended Visual Studio Workloads

- ASP.NET and web development
- .NET desktop development
- Data storage and processing

## Database Management

### Using Supabase Dashboard

1. **SQL Editor**: Run custom queries
2. **Table Editor**: View/edit data visually
3. **Database**: Connection settings and backups
4. **API**: Auto-generated REST API

### Using pgAdmin (Optional)

1. Download pgAdmin: https://www.pgadmin.org/
2. Add new server with Supabase connection details
3. Connect and manage database

### Using Azure Data Studio (Optional)

1. Download: https://docs.microsoft.com/en-us/sql/azure-data-studio/download
2. Install PostgreSQL extension
3. Connect with Supabase connection string

## Support

If you encounter issues not covered here:

1. Check `README.md` for general information
2. Review `docs/DECISIONS.md` for architecture context
3. Search existing issues in repository
4. Open new issue with:
   - Error message
   - Steps to reproduce
   - Environment details (OS, .NET version, etc.)

## Security Notes

⚠️ **IMPORTANT**: The credentials in `appsettings.json` are for development only.

For production:
1. **NEVER** commit credentials to version control
2. Use environment variables or secrets manager
3. Rotate keys regularly
4. Use least-privilege database users
5. Enable SSL/TLS for database connections

Example production configuration:

```bash
# Environment variables
export ConnectionStrings__DefaultConnection="postgresql://..."
export Supabase__Url="https://..."
export Supabase__ServiceRoleKey="..."
```

Or use Azure Key Vault, AWS Secrets Manager, or similar.



