# Dashboard Diagnostics Guide

## ✅ Application Status: RUNNING

### 🔧 What I've Fixed:

1. **SignalR Hub URL**: Corrected to `/hubs/telemetry`
2. **Event Handlers**: Fixed event name mismatches
3. **Message Structure**: Updated to match backend DTOs
4. **Console Logging**: Added detailed debugging logs
5. **Error Handling**: Added retry logic for SignalR
6. **API Integration**: Verified all endpoints are working

### 📊 Dashboard URLs:

- **Homepage**: http://localhost:8080/
- **Real-Time Monitoring**: http://localhost:8080/Dashboard/Realtime
- **AESO Live Generation**: http://localhost:8080/Dashboard/AesoLive
- **Pool Price Analytics**: http://localhost:8080/Dashboard/PoolPrice

### 🔍 Troubleshooting Steps:

#### 1. **Open Browser Developer Console (Press F12)**

You should see these logs:

**For Real-Time Dashboard:**
```
SignalR library loaded: true
Attempting to connect to SignalR hub at /hubs/telemetry...
SignalR connected successfully!
Received telemetry: UnitA.Temp 125.4 2025-11-13T...
```

**For AESO Live:**
```
AESO Live dashboard loaded, fetching data...
Fetching AESO data...
V2 Response: 200
Summary Response: 200
```

**For Pool Price:**
```
Pool Price dashboard loaded
Default date set to: 2025-11-13
```

#### 2. **Check Network Tab (F12 → Network)**

Look for:
- WebSocket connection to `/hubs/telemetry` (should be status 101 Switching Protocols)
- XHR requests to `/api/AesoApi/v2/summary` (should be 200 OK)
- XHR requests to `/api/AesoApi/summary` (should be 200 OK)

#### 3. **Common Issues & Solutions**

**Issue**: "Connecting..." stuck on Real-Time dashboard
**Solution**: 
- Check browser console for errors
- Verify SignalR CDN is loading: https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/7.0.5/signalr.min.js
- Try hard refresh (Ctrl+Shift+R)

**Issue**: AESO Live/Pool Price not loading data
**Solution**:
- Check browser console for API errors
- Verify API key is configured in appsettings.json
- Test API directly: http://localhost:8080/api/AesoApi/health

**Issue**: "Failed to fetch" errors
**Solution**:
- Ensure application is running: `dotnet run` in src/Dashboard.Web
- Check firewall isn't blocking port 8080
- Try http://localhost:8080/ first to verify app is running

### 🎯 Backend Services Status:

✅ **ModbusSimulator**: Broadcasting 4 tags
  - UnitA.Temp (0-200°C)
  - UnitA.Pressure (0-50 bar)
  - UnitA.Flow (0-500 m3/h)
  - UnitB.Level (0-100%)

✅ **ModbusAcquisition**: Reading and broadcasting via SignalR

✅ **AlarmEngine**: Monitoring thresholds
  - UnitA.Temp: HH=180°C, H=150°C
  - UnitA.Pressure: HH=45 bar

✅ **AESO API Service**: Connected and fetching live data

✅ **SignalR Hub**: Mapped at `/hubs/telemetry`

### 📝 What to Look For:

1. **In Browser Console** - Any red error messages?
2. **In Network Tab** - Are requests returning 200 OK?
3. **SignalR Status** - Should show "Connected" (green) not "Connecting..." (blue)

### 🐛 If Still Not Working:

1. **Copy all browser console logs** and share them
2. **Check Network tab** for failed requests
3. **Verify no CORS errors**
4. **Try in incognito mode** to rule out browser extensions

### ✨ Expected Behavior:

**Real-Time Dashboard:**
- Status: "Connected" (green)
- Tag count should increase (0 → 4)
- Charts should start populating with data
- Table rows should appear with tag values

**AESO Live:**
- All 6 stat cards should show numbers
- Chart should display fuel type bars
- Table should show 225 assets

**Pool Price:**
- Select date and click "Load Data"
- Stats cards should populate
- Chart should show price trends
- Table should show hourly data

---

## 🔧 Quick Commands:

**Restart Application:**
```powershell
cd C:\Users\hamza\PlantSight\src\Dashboard.Web
dotnet run
```

**Test API:**
```powershell
Invoke-RestMethod -Uri "http://localhost:8080/api/AesoApi/health"
```

**Kill All Dotnet Processes:**
```powershell
Get-Process -Name "dotnet" | Stop-Process -Force
```

---

**Last Updated**: Now with enhanced diagnostics and logging!

