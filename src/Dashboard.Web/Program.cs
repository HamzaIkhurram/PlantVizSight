// Program.cs - ASP.NET Core application entry point and configuration
using Dashboard.Acquisition.Services;
using Dashboard.Contracts.Interfaces;
using Dashboard.Domain.Models;
using Dashboard.Domain.Services;
using Dashboard.Persistence;
using Dashboard.Simulator;
using Dashboard.Web.Hubs;
using Dashboard.Web.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

builder.Services.Configure<Dashboard.Domain.Models.AesoApiConfiguration>(
    builder.Configuration.GetSection("AesoApi"));

builder.Services.AddHttpClient<Dashboard.Domain.Services.AesoApiService>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (!string.IsNullOrEmpty(connectionString))
{
    builder.Services.AddDbContext<DashboardDbContext>(options =>
        options.UseNpgsql(connectionString));
}

builder.Services.AddSingleton<ITelemetryHub, TelemetryBroadcaster>();

builder.Services.AddSingleton<AlarmEngine>();

builder.Services.AddSingleton<ModbusSimulator>();

builder.Services.AddSingleton<ModbusAcquisition>();

builder.Services.AddSingleton<WellPairSimulator>();

builder.Services.AddSingleton<WellPairBroadcaster>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

if (app.Configuration["ASPNETCORE_URLS"]?.Contains("https") == true)
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<TelemetryHub>("/hubs/telemetry");

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("PlantSight Dashboard started successfully");
logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
logger.LogInformation("Database configured: {DbConfigured}", !string.IsNullOrEmpty(connectionString));

var simulator = app.Services.GetRequiredService<ModbusSimulator>();
simulator.Start();
logger.LogInformation("Modbus simulator started");

var tags = new List<Tag>
{
    new() { TagId = Guid.NewGuid(), Name = "UnitA.Temp", EngineeringUnit = "degC", 
            SpanLow = 0, SpanHigh = 200, DataType = "float", ReadRegister = 0 },
    new() { TagId = Guid.NewGuid(), Name = "UnitA.Pressure", EngineeringUnit = "bar", 
            SpanLow = 0, SpanHigh = 50, DataType = "float", ReadRegister = 1 },
    new() { TagId = Guid.NewGuid(), Name = "UnitA.Flow", EngineeringUnit = "m3/h", 
            SpanLow = 0, SpanHigh = 500, DataType = "float", ReadRegister = 2 },
    new() { TagId = Guid.NewGuid(), Name = "UnitB.Level", EngineeringUnit = "%", 
            SpanLow = 0, SpanHigh = 100, DataType = "float", ReadRegister = 3 },
    
    new() { TagId = Guid.NewGuid(), Name = "SAGD.SteamInjection", EngineeringUnit = "m3/d", 
            SpanLow = 0, SpanHigh = 10000, DataType = "float", ReadRegister = 10 },
    new() { TagId = Guid.NewGuid(), Name = "SAGD.SteamQuality", EngineeringUnit = "Sm3/d", 
            SpanLow = 0, SpanHigh = 20000, DataType = "float", ReadRegister = 11 },
    new() { TagId = Guid.NewGuid(), Name = "SAGD.BitumenProduction", EngineeringUnit = "bbl/d", 
            SpanLow = 0, SpanHigh = 3000, DataType = "float", ReadRegister = 12 },
    new() { TagId = Guid.NewGuid(), Name = "SAGD.SteamPressure", EngineeringUnit = "kPa", 
            SpanLow = 0, SpanHigh = 300, DataType = "float", ReadRegister = 13 },
    new() { TagId = Guid.NewGuid(), Name = "SAGD.ReservoirTemp", EngineeringUnit = "degC", 
            SpanLow = 0, SpanHigh = 250, DataType = "float", ReadRegister = 14 },
    new() { TagId = Guid.NewGuid(), Name = "SAGD.ESPStatus", EngineeringUnit = "%", 
            SpanLow = 0, SpanHigh = 100, DataType = "float", ReadRegister = 15 },
    new() { TagId = Guid.NewGuid(), Name = "SAGD.WellheadTemp", EngineeringUnit = "degC", 
            SpanLow = 0, SpanHigh = 60, DataType = "float", ReadRegister = 16 },
    new() { TagId = Guid.NewGuid(), Name = "SAGD.TotalLiquid", EngineeringUnit = "bbl/d", 
            SpanLow = 0, SpanHigh = 3000, DataType = "float", ReadRegister = 17 },
    new() { TagId = Guid.NewGuid(), Name = "SAGD.WaterCut", EngineeringUnit = "%", 
            SpanLow = 0, SpanHigh = 50, DataType = "float", ReadRegister = 18 },
    
    new() { TagId = Guid.NewGuid(), Name = "Process.ReservoirPressure", EngineeringUnit = "kPa", 
            SpanLow = 0, SpanHigh = 5000, DataType = "float", ReadRegister = 20 },
    new() { TagId = Guid.NewGuid(), Name = "Process.SteamGenEfficiency", EngineeringUnit = "%", 
            SpanLow = 0, SpanHigh = 100, DataType = "float", ReadRegister = 21 },
    new() { TagId = Guid.NewGuid(), Name = "Process.FuelGasFlow", EngineeringUnit = "m3/h", 
            SpanLow = 0, SpanHigh = 200, DataType = "float", ReadRegister = 22 },
    new() { TagId = Guid.NewGuid(), Name = "Process.FeedwaterTemp", EngineeringUnit = "degC", 
            SpanLow = 0, SpanHigh = 900, DataType = "float", ReadRegister = 23 },
    new() { TagId = Guid.NewGuid(), Name = "Process.SeparatorPressure", EngineeringUnit = "kPa", 
            SpanLow = 0, SpanHigh = 5000, DataType = "float", ReadRegister = 24 },
    new() { TagId = Guid.NewGuid(), Name = "Process.SeparatorTemp", EngineeringUnit = "degC", 
            SpanLow = 0, SpanHigh = 100, DataType = "float", ReadRegister = 25 },
    new() { TagId = Guid.NewGuid(), Name = "Process.ProducedGasFlow", EngineeringUnit = "m3/d", 
            SpanLow = 0, SpanHigh = 2000, DataType = "float", ReadRegister = 26 },
    new() { TagId = Guid.NewGuid(), Name = "Process.DehydEfficiency", EngineeringUnit = "%", 
            SpanLow = 0, SpanHigh = 100, DataType = "float", ReadRegister = 27 },
    new() { TagId = Guid.NewGuid(), Name = "Process.SORRatio", EngineeringUnit = "ratio", 
            SpanLow = 0, SpanHigh = 10, DataType = "float", ReadRegister = 28 },
    new() { TagId = Guid.NewGuid(), Name = "Process.WaterTreatEff", EngineeringUnit = "%", 
            SpanLow = 0, SpanHigh = 100, DataType = "float", ReadRegister = 29 },
    new() { TagId = Guid.NewGuid(), Name = "Process.PipelinePressure", EngineeringUnit = "kPa", 
            SpanLow = 0, SpanHigh = 3000, DataType = "float", ReadRegister = 30 },
    new() { TagId = Guid.NewGuid(), Name = "Process.ExportPumpStatus", EngineeringUnit = "%", 
            SpanLow = 0, SpanHigh = 100, DataType = "float", ReadRegister = 31 },
    new() { TagId = Guid.NewGuid(), Name = "Process.FlashDrumPressure", EngineeringUnit = "kPa", 
            SpanLow = 0, SpanHigh = 2500, DataType = "float", ReadRegister = 32 },
    new() { TagId = Guid.NewGuid(), Name = "Process.FlashDrumTemp", EngineeringUnit = "degC", 
            SpanLow = 0, SpanHigh = 150, DataType = "float", ReadRegister = 33 },
    new() { TagId = Guid.NewGuid(), Name = "Process.WaterInjectionRate", EngineeringUnit = "m3/d", 
            SpanLow = 0, SpanHigh = 500, DataType = "float", ReadRegister = 34 }
};

var alarmRules = new List<AlarmRule>
{
    new() { AlarmRuleId = Guid.NewGuid(), TagId = tags[0].TagId, Type = "HH", 
            Threshold = 180, Severity = 4, HysteresisPct = 0.5, DelayOnMs = 2000, DelayOffMs = 5000 },
    new() { AlarmRuleId = Guid.NewGuid(), TagId = tags[0].TagId, Type = "H", 
            Threshold = 150, Severity = 3, HysteresisPct = 0.5, DelayOnMs = 2000, DelayOffMs = 5000 },
    new() { AlarmRuleId = Guid.NewGuid(), TagId = tags[1].TagId, Type = "HH", 
            Threshold = 45, Severity = 4, HysteresisPct = 0.5, DelayOnMs = 2000, DelayOffMs = 5000 },
    new() { AlarmRuleId = Guid.NewGuid(), TagId = tags[6].TagId, Type = "HH",
            Threshold = 1600, Severity = 3, HysteresisPct = 0.5, DelayOnMs = 5000, DelayOffMs = 10000 },
    new() { AlarmRuleId = Guid.NewGuid(), TagId = tags[9].TagId, Type = "L",
            Threshold = 85, Severity = 4, HysteresisPct = 1.0, DelayOnMs = 1000, DelayOffMs = 5000 }
};

var acquisition = app.Services.GetRequiredService<ModbusAcquisition>();
acquisition.Start(tags, alarmRules, simulator.ReadHoldingRegister);
logger.LogInformation("Modbus acquisition started with {TagCount} tags and {AlarmCount} alarm rules", 
    tags.Count, alarmRules.Count);

var wellPairSimulator = app.Services.GetRequiredService<WellPairSimulator>();
wellPairSimulator.Start();
logger.LogInformation("Well pair simulator started with {WellPairCount} well pairs", 
    wellPairSimulator.GetAllWellPairs().Count);

var wellPairBroadcaster = app.Services.GetRequiredService<WellPairBroadcaster>();
wellPairBroadcaster.Start();
logger.LogInformation("Well pair broadcaster started");

app.Run();


