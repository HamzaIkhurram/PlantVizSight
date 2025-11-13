// DashboardController.cs - Dashboard view controllers
using Microsoft.AspNetCore.Mvc;

namespace Dashboard.Web.Controllers;

public class DashboardController : Controller
{
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(ILogger<DashboardController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        _logger.LogInformation("Dashboard accessed");
        return View();
    }

    public IActionResult Realtime()
    {
        _logger.LogInformation("Real-time dashboard accessed");
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
        return View();
    }
}


