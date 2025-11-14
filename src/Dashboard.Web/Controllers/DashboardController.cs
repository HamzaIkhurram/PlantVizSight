// DashboardController.cs - Dashboard view controllers
using Dashboard.Domain.Models;
using Dashboard.Simulator;
using Microsoft.AspNetCore.Mvc;

namespace Dashboard.Web.Controllers;

public class DashboardController : Controller
{
    private readonly ILogger<DashboardController> _logger;
    private readonly WellPairSimulator _wellPairSimulator;

    public DashboardController(ILogger<DashboardController> logger, WellPairSimulator wellPairSimulator)
    {
        _logger = logger;
        _wellPairSimulator = wellPairSimulator;
    }

    public IActionResult Index()
    {
        _logger.LogInformation("Dashboard accessed");
        return View();
    }

    public IActionResult Realtime()
    {
        _logger.LogInformation("Real-time dashboard accessed");
        
        var config = new RealtimeDashboardConfig();
        ViewBag.Config = config;
        
        return View();
    }

    public IActionResult AesoLive()
    {
        _logger.LogInformation("AESO Live dashboard accessed");
        return View();
    }

    public IActionResult PoolPrice()
    {
        _logger.LogInformation("Pool Price dashboard accessed");
        return View();
    }

    public IActionResult SOR()
    {
        _logger.LogInformation("SAGD SOR dashboard accessed");
        
        var config = new SorDashboardConfig();
        ViewBag.Config = config;
        
        return View();
    }

    public IActionResult WellPairPerformance()
    {
        _logger.LogInformation("Well Pair Performance dashboard accessed");
        
        var wellPairs = _wellPairSimulator.GetAllWellPairs();
        var facilities = wellPairs.Select(wp => wp.Facility).Distinct().OrderBy(f => f).ToList();
        var config = new WellPairDashboardConfig();
        
        ViewBag.WellPairCount = wellPairs.Count;
        ViewBag.Facilities = facilities;
        ViewBag.Config = config;
        
        return View();
    }

    public IActionResult ProcessTabs()
    {
        _logger.LogInformation("Process Tabs dashboard accessed (SAGD, Separation, Utilities)");
        
        var config = new RealtimeDashboardConfig();
        ViewBag.Config = config;
        
        return View();
    }
}


