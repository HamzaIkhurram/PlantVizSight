# 🔑 AESO API Key Setup Guide

## Problem Found

The AESO API requires a **subscription key** for authentication. This is why you're seeing the error:
> "Failed to fetch data from AESO API"

## Solution: Get Your FREE API Key

### Step 1: Register for AESO API Access

1. **Visit AESO API Portal**: https://api.aeso.ca/
2. **Sign Up**: Create a free account
   - Click "Sign Up" or "Register"
   - Fill in your details
   - Verify your email

3. **Subscribe to the API**:
   - Log in to your account
   - Navigate to "Products" or "APIs"
   - Find "Current Supply Demand API"
   - Click "Subscribe" (it's FREE for public use)

4. **Get Your Subscription Key**:
   - After subscribing, go to your "Profile" or "Subscriptions"
   - Copy your **Primary Key** or **Secondary Key**

### Step 2: Add the Key to Your Application

Open `src/Dashboard.Web/appsettings.json` and add your key:

```json
{
  ...
  "AesoApi": {
    "BaseUrl": "https://apimgw.aeso.ca/public/currentsupplydemand-api/v1",
    "SubscriptionKey": "YOUR_KEY_HERE",  ← ADD YOUR KEY HERE
    "TimeoutSeconds": 30,
    "RetryAttempts": 3,
    "RetryDelaySeconds": 2
  }
}
```

### Step 3: Restart the Application

```powershell
# Stop any running instances
Get-Process -Name "dotnet" | Stop-Process -Force

# Start the app
cd C:\Users\hamza\PlantSight\src\Dashboard.Web
dotnet run
```

###Step 4: Test the Dashboard

Navigate to: http://localhost:8080/Dashboard/AesoLive

You should now see live data! 🎉

## What We've Implemented (FAANG-Level Features)

### 1. **Retry Logic with Exponential Backoff**
- Using **Polly** library for resilience
- Automatically retries failed requests 3 times
- Exponential delay between retries (2s, 4s, 8s)

### 2. **Proper Configuration Management**
- API key stored in `appsettings.json`
- Easy to update without code changes
- Configurable timeout and retry settings

### 3. **Enhanced Error Handling**
- Detailed error messages for different failure scenarios
- Proper HTTP status codes (400, 408, 503, 500)
- Comprehensive logging with emojis for quick scanning

### 4. **Request Headers**
- `Ocp-Apim-Subscription-Key`: Your API key
- `Accept`: application/json
- `User-Agent`: PlantSight-Dashboard/1.0

### 5. **Circuit Breaker Pattern** (via Polly)
- Handles transient HTTP errors
- Rate limiting support (429 Too Many Requests)
- Automatic recovery

### 6. **Timeout Configuration**
- Configurable timeout (default: 30 seconds)
- Prevents hanging requests

## API Key Security Best Practices

### For Development
Keep your key in `appsettings.json` (current setup)

### For Production
**Option 1**: Environment Variables
```powershell
$env:AesoApi__SubscriptionKey="your-key-here"
dotnet run
```

**Option 2**: User Secrets (Recommended for local dev)
```powershell
cd src/Dashboard.Web
dotnet user-secrets set "AesoApi:SubscriptionKey" "your-key-here"
```

**Option 3**: Azure Key Vault (For production deployments)

**⚠️ IMPORTANT**: Never commit your API key to Git!

Add to `.gitignore`:
```
appsettings.Development.json
appsettings.Production.json
**/appsettings.*.json
```

## Troubleshooting

### "Access denied due to missing subscription key"
- You haven't added the key to `appsettings.json`
- Or the key is incorrect
- Solution: Double-check your key from the AESO portal

### "Service Unavailable"
- AESO API might be temporarily down
- Check https://api.aeso.ca/ for system status
- Wait a few minutes and try again

### "Request Timeout"
- Network is slow or AESO API is responding slowly
- Increase `TimeoutSeconds` in configuration
- Check your internet connection

### Still not working?
1. Check browser console for JavaScript errors
2. Check application logs for detailed error messages
3. Verify you're using the correct API key
4. Test the API directly with curl:

```powershell
$headers = @{
    "Ocp-Apim-Subscription-Key" = "YOUR_KEY_HERE"
}
Invoke-WebRequest -Uri "https://apimgw.aeso.ca/public/currentsupplydemand-api/v1/csd/generation/assets/current" -Headers $headers
```

## FAANG-Level Code Quality

✅ **Dependency Injection**: HttpClient with typed client pattern  
✅ **Resilience**: Polly retry policies with exponential backoff  
✅ **Configuration**: Options pattern with strong typing  
✅ **Logging**: Structured logging with context  
✅ **Error Handling**: Proper exception handling and HTTP status codes  
✅ **Security**: API keys configurable, not hard-coded  
✅ **Performance**: Async/await throughout  
✅ **Maintainability**: Clean separation of concerns  
✅ **Testability**: All dependencies injected  
✅ **Documentation**: Comprehensive inline and external docs  

## Next Steps

Once your API key is configured:
1. Explore the live dashboard
2. Try the filtering features
3. Check out the fuel type chart
4. See real-time Alberta power generation!

---

**Need Help?**
- AESO API Portal: https://api.aeso.ca/
- AESO Documentation: https://api.aeso.ca/web/api/documentation
- AESO Support: Contact through their portal

**Made with 💜 by a FAANG-level engineer**

