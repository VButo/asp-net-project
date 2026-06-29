using API_tester.Data;
using API_tester.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_tester.Controllers;

[Authorize]
public class SearchController : Controller
{
    private readonly AppDbContext _context;

    public SearchController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Index([FromQuery] string? q)
    {
        var term = (q ?? string.Empty).Trim();
        var results = new List<GlobalSearchResult>();
        if (term.Length > 0)
        {
            var pattern = $"%{term}%";
            var hasMethod = Enum.TryParse<API_tester.Models.Enums.HttpMethodType>(term, true, out var method);
            results.AddRange(await _context.Workspaces
                .Where(item => EF.Functions.Like(item.Name, pattern) || EF.Functions.Like(item.Description, pattern))
                .Take(10)
                .Select(item => new GlobalSearchResult("Workspace", item.Id, item.Name, item.Description, $"/workspace-details/{item.Id}"))
                .ToListAsync());
            results.AddRange(await _context.Collections
                .Where(item => EF.Functions.Like(item.Name, pattern) || EF.Functions.Like(item.Description, pattern))
                .Take(10)
                .Select(item => new GlobalSearchResult("Collection", item.Id, item.Name, item.Description, $"/collections/{item.Id}"))
                .ToListAsync());
            results.AddRange(await _context.Requests
                .Where(item => EF.Functions.Like(item.Name, pattern) || EF.Functions.Like(item.Url, pattern)
                    || (hasMethod && item.Method == method)
                    || item.TagLinks.Any(link => EF.Functions.Like(link.Tag!.Name, pattern)))
                .Take(10)
                .Select(item => new GlobalSearchResult("Request", item.Id, item.Name, $"{item.Method} {item.Url}", $"/request-builder/{item.Id}"))
                .ToListAsync());
            results.AddRange(await _context.Environments
                .Where(item => EF.Functions.Like(item.Name, pattern) || EF.Functions.Like(item.BaseUrl, pattern))
                .Take(10)
                .Select(item => new GlobalSearchResult("Environment", item.Id, item.Name, item.BaseUrl, $"/environments/edit/{item.Id}"))
                .ToListAsync());
            results.AddRange(await _context.RequestTags
                .Where(item => EF.Functions.Like(item.Name, pattern))
                .Take(10)
                .Select(item => new GlobalSearchResult("Tag", item.Id, item.Name, item.ColorHex, "/tags"))
                .ToListAsync());
        }

        ViewData["Query"] = term;
        return View(results);
    }
}
