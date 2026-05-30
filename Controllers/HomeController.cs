using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using API_tester.Models;

namespace API_tester.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    [HttpGet("")]
    [HttpGet("home")]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet("privacy")]
    public IActionResult Privacy()
    {
        return RedirectToAction("Index", "Workspaces");
    }

    [HttpGet("workspace-overview")]
    public IActionResult WorkspaceDetails()
    {
        return View();
    }

    [HttpGet("request-overview")]
    public IActionResult RequestDetails()
    {
        return View();
    }

    [HttpGet("builder")]
    public IActionResult RequestBuilder()
    {
        return RedirectToAction("Index", "RequestBuilder");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
