# AESO API Integration - Quick Start Guide

## 🎉 Integration Complete!

The AESO (Alberta Electric System Operator) Live API has been successfully integrated into your PlantSight Dashboard!

## What Was Added

### Backend Components

1. **Models** - `src/Dashboard.Domain/Models/AesoAssetGeneration.cs`
   - `AesoAssetGenerationReport`: Main response model
   - `AesoAsset`: Individual asset data

2. **Service** - `src/Dashboard.Domain/Services/AesoApiService.cs`
   - Fetches live data from AESO API
   - Includes helper methods for filtering by fuel type
   - Provides aggregation capabilities

3. **API Controller** - `src/Dashboard.Web/Controllers/AesoApiController.cs`
   - `/api/AesoApi/current` - Get all assets (optionally filter by asset IDs)
   - `/api/AesoApi/fuel-type/{fuelType}` - Filter by fuel type
   - `/api/AesoApi/generation-by-fuel` - Get totals by fuel type
   - `/api/AesoApi/summary` - Get comprehensive statistics

4. **Frontend Dashboard** - `src/Dashboard.Web/Views/Dashboard/AesoLive.cshtml`
   - Beautiful, modern UI with gradient design
   - Real-time data display with auto-refresh (60 seconds)
   - Interactive charts showing generation by fuel type
   - Filterable and searchable asset table
   - Summary cards with key metrics

### Updated Files

- `src/Dashboard.Web/Program.cs` - Registered HttpClient for AesoApiService
- `src/Dashboard.Web/Controllers/DashboardController.cs` - Added AesoLive route
- `src/Dashboard.Web/Views/Home/Index.cshtml` - Added navigation button

## How to Use

### 1. Start the Application

```bash
cd src/Dashboard.Web
dotnet run
```

### 2. Access the Dashboard

Open your browser and navigate to:
- **Home Page**: `http://localhost:5000/`
- **AESO Live Dashboard**: `http://localhost:5000/Dashboard/AesoLive`

Or click the **"⚡ AESO Live Generation Data"** button on the home page.

### 3. Explore the Features

#### Summary Cards
View real-time statistics:
- Total number of assets
- Current total generation (MW)
- Total capacity (MW)
- Capacity utilization (%)
- Contingency reserves (MW)

#### Generation by Fuel Type Chart
Interactive bar chart showing:
- Generation by fuel type (Gas, Hydro, Wind, Coal, Solar, etc.)
- Hover for detailed information
- Visual comparison of different energy sources

#### Asset Details Table
Complete asset listing with:
- Asset ID
- Fuel type and sub-fuel type
- Current generation vs. maximum capability
- Utilization percentage with visual progress bar
- Dispatched contingency reserves

#### Filtering & Search
- **Fuel Type Filter**: Dropdown to filter by specific fuel type
- **Asset Search**: Text search to find specific assets
- Real-time filtering without page reload

#### Auto-Refresh
- Data automatically updates every 60 seconds
- Manual refresh button available
- Live status indicator

## API Endpoints

Test the backend API directly:

### Get All Assets
```bash
curl http://localhost:5000/api/AesoApi/current
```

### Get Specific Assets (up to 20 comma-separated)
```bash
curl "http://localhost:5000/api/AesoApi/current?assetIds=ABC,DEF,GHI"
```

### Get Assets by Fuel Type
```bash
curl http://localhost:5000/api/AesoApi/fuel-type/Gas
curl http://localhost:5000/api/AesoApi/fuel-type/Wind
curl http://localhost:5000/api/AesoApi/fuel-type/Hydro
```

### Get Generation Summary by Fuel Type
```bash
curl http://localhost:5000/api/AesoApi/generation-by-fuel
```

### Get Summary Statistics
```bash
curl http://localhost:5000/api/AesoApi/summary
```

## Data Source

The integration fetches data from the official AESO API:
- **URL**: `https://apimgw.aeso.ca/public/currentsupplydemand-api/v1/csd/generation/assets/current`
- **Update Frequency**: Real-time (API is updated frequently by AESO)
- **Data Coverage**: All AIES assets greater than 5 MW
- **No Authentication Required**: Public API

## Example Data

```json
{
  "lastUpdatedDatetimeUtc": "2025-11-13 10:20",
  "lastUpdatedDatetimeMpt": "2025-11-13 03:20",
  "assetList": [
    {
      "asset": "ABC",
      "fuelType": "Gas",
      "subFuelType": "Combined Cycle",
      "maximumCapability": 500,
      "netGeneration": 450,
      "dispatchedContingencyReserve": 50
    },
    {
      "asset": "DEF",
      "fuelType": "Wind",
      "subFuelType": null,
      "maximumCapability": 200,
      "netGeneration": 150,
      "dispatchedContingencyReserve": 0
    }
  ]
}
```

## Key Features

✅ **Real-time Data**: Live data from AESO's official API  
✅ **Beautiful UI**: Modern, gradient-based design  
✅ **Interactive Charts**: Visualize generation by fuel type  
✅ **Filtering**: Filter by fuel type or search by asset name  
✅ **Auto-refresh**: Updates every 60 seconds automatically  
✅ **Responsive**: Works on desktop and mobile devices  
✅ **Error Handling**: Graceful error messages if API is unavailable  
✅ **Performance**: Efficient data fetching and rendering  

## Architecture

```
Frontend (AesoLive.cshtml)
    ↓ HTTP GET
API Controller (AesoApiController)
    ↓ Method Call
Service Layer (AesoApiService)
    ↓ HTTP Request
External AESO API
```

## Fuel Types Available

The dashboard categorizes assets by fuel type:
- **Gas**: Natural gas turbines and combined cycle
- **Hydro**: Hydroelectric generation
- **Wind**: Wind turbines
- **Coal**: Coal-fired generation
- **Solar**: Solar photovoltaic
- **Other**: Other renewable and conventional sources

## Troubleshooting

### API Not Loading
- Check internet connectivity
- Verify AESO API is operational
- Check browser console for errors
- Try manual refresh

### No Data Displayed
- Wait for initial data fetch (may take a few seconds)
- Check if AESO API is returning data
- Verify application logs for errors

### Linting Errors
No linting errors were found! All code is clean and follows best practices.

## Next Steps

Consider these enhancements:
1. **Historical Data**: Store historical data in Supabase for trend analysis
2. **Alerts**: Set up notifications for capacity thresholds
3. **Export**: Add CSV/Excel export functionality
4. **Advanced Charts**: Add time-series charts for historical trends
5. **Mobile App**: Create a mobile version using Xamarin or MAUI

## Documentation

Full documentation available in:
- `docs/AESO_INTEGRATION.md` - Comprehensive integration guide
- This file - Quick start guide

## Support

For questions or issues:
1. Check the documentation
2. Review browser console and application logs
3. Verify AESO API status at `https://apimgw.aeso.ca/public/`

---

**Congratulations!** 🎉 Your PlantSight Dashboard now includes live Alberta power generation data!

**Next**: Start the application and navigate to `/Dashboard/AesoLive` to see it in action!

