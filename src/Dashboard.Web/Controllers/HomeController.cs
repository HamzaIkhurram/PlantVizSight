// HomeController.cs - Home page controller
using Microsoft.AspNetCore.Mvc;

namespace Dashboard.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        _logger.LogInformation("Dashboard home page accessed");
        return View();
    }

    public IActionResult Error()
    {
        return View();
    }
}


