using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using API_tester.Models;
using Microsoft.AspNetCore.Authorization;
using API_tester.Data;
using Microsoft.EntityFrameworkCore;

namespace API_tester.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AppDbContext _context;

    public HomeController(ILogger<HomeController> logger, AppDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    [HttpGet("")]
    [HttpGet("home")]
    [Authorize]
    public async Task<IActionResult> Index()
    {
        var weekAgo = DateTime.UtcNow.AddDays(-7);
        var latestResponse = await _context.Responses
            .OrderByDescending(response => response.ReceivedAt)
            .FirstOrDefaultAsync();

        var previewRequest = await _context.Requests
            .OrderByDescending(request => request.LastExecutedAt ?? request.CreatedAt)
            .FirstOrDefaultAsync();

        var activeEnvironment = await _context.Environments
            .Include(environment => environment.Variables)
            .Where(environment => environment.IsActive)
            .OrderByDescending(environment => environment.CreatedAt)
            .FirstOrDefaultAsync()
            ?? await _context.Environments
                .Include(environment => environment.Variables)
                .OrderByDescending(environment => environment.CreatedAt)
                .FirstOrDefaultAsync();

        var workspaces = await _context.Workspaces
            .Include(workspace => workspace.Collections)
                .ThenInclude(collection => collection.Requests)
            .OrderByDescending(workspace => workspace.CreatedAt)
            .Take(3)
            .ToListAsync();

        var collections = await _context.Collections
            .Include(collection => collection.Requests)
            .OrderByDescending(collection => collection.CreatedAt)
            .Take(4)
            .ToListAsync();

        var model = new DashboardViewModel
        {
            TotalWorkspaces = await _context.Workspaces.CountAsync(),
            RecentWorkspaces = await _context.Workspaces.CountAsync(workspace => workspace.CreatedAt >= weekAgo),
            TotalCollections = await _context.Collections.CountAsync(),
            TotalRequests = await _context.Requests.CountAsync(),
            LastRunAt = latestResponse?.ReceivedAt ?? previewRequest?.LastExecutedAt,
            LastDurationMs = latestResponse?.DurationMs,
            LastStatusCode = latestResponse?.StatusCode,
            LastRunSucceeded = latestResponse?.IsSuccess == true,
            RequestPreview = previewRequest == null
                ? null
                : new DashboardRequestPreview
                {
                    Id = previewRequest.Id,
                    Name = previewRequest.Name,
                    Url = previewRequest.Url,
                    Method = previewRequest.Method,
                    Body = previewRequest.Body
                },
            ActiveEnvironment = activeEnvironment == null
                ? null
                : new DashboardEnvironmentSummary
                {
                    Id = activeEnvironment.Id,
                    Name = activeEnvironment.Name,
                    BaseUrl = activeEnvironment.BaseUrl,
                    VariableCount = activeEnvironment.Variables.Count,
                    SecretCount = activeEnvironment.Variables.Count(variable => variable.IsSecret)
                },
            Workspaces = workspaces.Select(workspace => new DashboardWorkspaceSummary
            {
                Id = workspace.Id,
                Name = workspace.Name,
                CollectionCount = workspace.Collections.Count,
                RequestCount = workspace.Collections.Sum(collection => collection.Requests.Count)
            }).ToList(),
            Collections = collections.Select(collection => new DashboardCollectionSummary
            {
                Id = collection.Id,
                Name = collection.Name,
                RequestCount = collection.Requests.Count,
                Methods = collection.Requests
                    .Select(request => request.Method)
                    .Distinct()
                    .OrderBy(method => method)
                    .ToList()
            }).ToList()
        };

        return View(model);
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
