# AESO Live API Integration

## Overview

The PlantSight Dashboard now includes real-time integration with the **Alberta Electric System Operator (AESO) Current Supply Demand API**. This integration provides live data on power generation from all AIES (Alberta Interconnected Electric System) assets greater than 5 MW.

## Features

### ⚡ Real-Time Data
- Live generation data from AESO assets
- Automatic updates every 60 seconds
- Last updated timestamps in both UTC and MPT (Mountain Prevailing Time)

### 📊 Comprehensive Dashboard
- **Summary Cards**: Total assets, current generation, capacity, utilization, and reserves
- **Fuel Type Chart**: Interactive bar chart showing generation by fuel type (Gas, Hydro, Wind, Coal, Solar, etc.)
- **Asset Details Table**: Complete list of all assets with filtering and search capabilities

### 🔍 Filtering & Search
- Filter by fuel type
- Search assets by name
- Real-time table updates

## API Endpoints

### 1. Get Current Supply Demand
```
GET /api/AesoApi/current
GET /api/AesoApi/current?assetIds=ABC,DEF,GHI
```

Fetches the latest generation data for all assets or specific assets (up to 20 comma-separated IDs).

**Response:**
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
    }
  ]
}
```

### 2. Get Assets by Fuel Type
```
GET /api/AesoApi/fuel-type/{fuelType}
```

Returns all assets filtered by fuel type (e.g., Gas, Wind, Hydro, Coal, Solar).

### 3. Get Generation by Fuel Type
```
GET /api/AesoApi/generation-by-fuel
```

Returns total generation grouped by fuel type.

**Response:**
```json
{
  "Gas": 5000,
  "Hydro": 2000,
  "Wind": 1500,
  "Coal": 1000,
  "Solar": 500
}
```

### 4. Get Summary Statistics
```
GET /api/AesoApi/summary
```

Returns comprehensive summary including:
- Total assets count
- Total generation (MW)
- Total capacity (MW)
- Capacity utilization (%)
- Total contingency reserve (MW)
- Breakdown by fuel type

## Architecture

### Components Created

1. **Models** (`Dashboard.Domain/Models/AesoAssetGeneration.cs`)
   - `AesoAssetGenerationReport`: Main response model
   - `AesoAsset`: Individual asset data model

2. **Service** (`Dashboard.Domain/Services/AesoApiService.cs`)
   - `AesoApiService`: Service for fetching and processing AESO API data
   - Includes helper methods for filtering and aggregation

3. **API Controller** (`Dashboard.Web/Controllers/AesoApiController.cs`)
   - RESTful API endpoints
   - Error handling and logging
   - Proper HTTP status codes

4. **Frontend View** (`Dashboard.Web/Views/Dashboard/AesoLive.cshtml`)
   - Beautiful, modern UI with gradient backgrounds
   - Interactive charts using Chart.js
   - Real-time filtering and search
   - Auto-refresh functionality

## Usage

### Accessing the Dashboard

1. Start the application
2. Navigate to the home page
3. Click on **"⚡ AESO Live Generation Data"** button

Or directly access: `http://localhost:5000/Dashboard/AesoLive`

### Using the Dashboard

1. **View Summary**: See high-level statistics at the top
2. **Explore Chart**: Visualize generation by fuel type
3. **Filter Assets**: Use the dropdown to filter by fuel type
4. **Search**: Type asset name to quickly find specific assets
5. **Auto-Refresh**: Data automatically updates every 60 seconds
6. **Manual Refresh**: Click the "🔄 Refresh" button for immediate update

## Data Dictionary

### Asset Fields

| Field | Description | Unit |
|-------|-------------|------|
| Asset | Asset's unique 3-4 digit alphanumeric code | - |
| Fuel Type | Primary fuel type (Gas, Hydro, Wind, Coal, Solar, etc.) | - |
| Sub Fuel Type | Sub-category (only for Gas assets) | - |
| Maximum Capability | Maximum physical generation capacity | MW |
| Net Generation | Current net generation (Gross - Auxiliary load) | MW |
| Dispatched Contingency Reserve | Accepted contingency reserve | MW |

### Fuel Types

- **Gas**: Natural gas turbines and combined cycle
- **Hydro**: Hydroelectric power
- **Wind**: Wind turbines
- **Coal**: Coal-fired generation
- **Solar**: Solar photovoltaic
- **Other**: Other renewable and conventional sources

## Technical Details

### External API

- **Base URL**: `https://apimgw.aeso.ca/public/currentsupplydemand-api/v1`
- **Endpoint**: `/csd/generation/assets/current`
- **Method**: GET
- **Authentication**: None required (public API)
- **Rate Limiting**: Follow AESO's usage guidelines

### Error Handling

The integration includes comprehensive error handling:
- HTTP request failures
- API unavailability (503 errors)
- Invalid responses
- Network timeouts

All errors are logged and displayed to users with appropriate messages.

### Performance

- Data is fetched on-demand
- No caching (always live data)
- Efficient JSON serialization
- Responsive UI with smooth animations

## Configuration

No configuration required. The service is automatically registered in `Program.cs` using:

```csharp
builder.Services.AddHttpClient<Dashboard.Domain.Services.AesoApiService>();
```

## Future Enhancements

Potential improvements:
- Historical data storage in Supabase
- Trend analysis over time
- Alert notifications for capacity issues
- Export to CSV/Excel
- Advanced filtering (by region, capacity range, etc.)
- Predictive analytics using historical patterns

## Troubleshooting

### API Not Responding

If the AESO API is unavailable:
1. Check AESO's system status
2. Verify internet connectivity
3. Check application logs for detailed error messages
4. Try manual refresh after a few minutes

### Data Not Updating

If data isn't refreshing:
1. Check browser console for JavaScript errors
2. Verify the API controller is accessible
3. Check if auto-refresh is working (60-second interval)
4. Use the manual refresh button

### Performance Issues

If the dashboard is slow:
1. Check network speed
2. Reduce browser extensions that may interfere
3. Clear browser cache
4. Check system resources

## References

- [AESO API Documentation](https://apimgw.aeso.ca/public/)
- AESO (Alberta Electric System Operator)
- Current Supply Demand API v1

## Support

For issues or questions about the AESO integration:
1. Check this documentation
2. Review application logs
3. Verify AESO API status
4. Contact AESO for API-specific issues

---

**Last Updated**: November 13, 2025  
**Integration Version**: 1.0

