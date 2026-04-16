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

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult WorkspaceDetails()
    {
        return View();
    }

    public IActionResult Environments()
    {
        return View();
    }

    public IActionResult Collections()
    {
        return View();
    }

    public IActionResult CollectionDetails()
    {
        return View();
    }

    public IActionResult Requests()
    {
        return View();
    }

    public IActionResult RequestDetails()
    {
        return View();
    }

    public IActionResult RequestBuilder()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
