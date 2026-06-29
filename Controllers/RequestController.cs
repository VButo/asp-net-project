using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using API_tester.Models;
using API_tester.Models.Enums;
using API_tester.Data;
using Microsoft.EntityFrameworkCore;

namespace API_tester.Controllers;

[Authorize]
public class RequestController : Controller
{
    private readonly AppDbContext _context;

    public RequestController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("requests")]
    public async Task<IActionResult> Index()
    {
        var requests = await _context.Requests
            .Include(r => r.Collection)
            .Include(r => r.Responses)
            .Include(r => r.TagLinks)
                .ThenInclude(t => t.Tag)
            .ToListAsync();

        ViewData["Title"] = "Requests";
        ViewData["BreadcrumbCurrent"] = "Requests";
        ViewData["HeroKicker"] = "Request Matrix";
        ViewData["HeroTitle"] = "An operational view of all API requests across collections and workspaces.";
        ViewData["HeroDescription"] = "A focused view of method badges, URLs, associated collections, tags, and the latest execution or status for each route.";
        ViewData["PrimaryActionText"] = "+ New Request";
        ViewData["SecondaryActionText"] = "Run Batch";
        ViewData["SearchPlaceholder"] = "Search by URL, collection or tag";
        ViewData["Filters"] = new List<string> { "All Methods", "Critical", "Smoke", "Pending" };

        var withResponse = requests.Where(r => r.Responses.Any()).ToList();
        ViewData["SnapshotTitle"] = $"{requests.Count} Requests";
        ViewData["SuccessfulCount"] = withResponse.Count(r => r.Responses.OrderByDescending(resp => resp.ReceivedAt).First().IsSuccess);
        ViewData["WarningCount"] = withResponse.Count(r => !r.Responses.OrderByDescending(resp => resp.ReceivedAt).First().IsSuccess);
        ViewData["NotExecutedCount"] = requests.Count(r => !r.Responses.Any());

        return View(requests);
    }

    [HttpGet("requests/search")]
    public async Task<IActionResult> Search([FromQuery(Name = "q")] string? q)
    {
        var term = (q ?? string.Empty).Trim();

        IQueryable<ApiRequest> query = _context.Requests
            .Include(r => r.Collection)
            .AsQueryable();

        if (!string.IsNullOrEmpty(term))
        {
            query = query.Where(r => EF.Functions.Like(r.Name, $"%{term}%") || EF.Functions.Like(r.Url, $"%{term}%"));
        }

        var results = await query
            .OrderBy(r => r.Name)
            .Select(r => new {
                id = r.Id,
                name = r.Name,
                description = r.Url,
                url = r.Url,
                method = r.Method.ToString(),
                collection = r.Collection != null ? r.Collection.Name : string.Empty
            })
            .ToListAsync();

        return Json(results);
    }
}
