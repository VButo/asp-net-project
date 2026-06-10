using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using API_tester.Models;
using API_tester.Models.Enums;
using API_tester.Data;
using Microsoft.EntityFrameworkCore;

namespace API_tester.Controllers;

[Authorize]
public class EnvironmentController : Controller
{
    private readonly AppDbContext _context;

    public EnvironmentController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("environments")]
    [HttpGet("environments/{workspaceId:int}")]
    public async Task<IActionResult> Index(int? workspaceId)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var environmentsQuery = _context.Environments
            .Include(e => e.Workspace)
            .Include(e => e.Variables)
            .AsQueryable();

        if (workspaceId.HasValue)
        {
            environmentsQuery = environmentsQuery.Where(environment => environment.WorkspaceId == workspaceId.Value);
            var workspace = await _context.Workspaces
                .FirstOrDefaultAsync(w => w.Id == workspaceId.Value);

            ViewData["WorkspaceName"] = workspace?.Name ?? "Workspace";
            ViewData["BreadcrumbWorkspace"] = workspace?.Name ?? "Workspaces";
        }
        else
        {
            ViewData["WorkspaceName"] = "All Environments";
            ViewData["BreadcrumbWorkspace"] = "Environments";
        }

        var environments = await environmentsQuery
            .OrderBy(environment => environment.Name)
            .ToListAsync();

        ViewData["Title"] = "Environments";
        ViewData["BreadcrumbCurrent"] = "Environments";
        ViewData["HeroKicker"] = "Environment Registry";
        ViewData["HeroTitle"] = ViewData["WorkspaceName"] as string ?? "All Environments";
        ViewData["HeroDescription"] = "Browse available environment configurations and jump straight into the Request Builder.";
        ViewData["PrimaryActionText"] = "Open Builder";
        ViewData["SecondaryActionText"] = "Back to Workspaces";

        return View(environments);
    }

    [HttpGet("environments/search")]
    public async Task<IActionResult> Search([FromQuery(Name = "q")] string? q)
    {
        var term = (q ?? string.Empty).Trim();

        IQueryable<ApiEnvironment> query = _context.Environments
            .Include(e => e.Workspace)
            .AsQueryable();

        if (!string.IsNullOrEmpty(term))
        {
            query = query.Where(e => EF.Functions.Like(e.Name, $"%{term}%") || EF.Functions.Like(e.BaseUrl, $"%{term}%"));
        }

        var results = await query
            .OrderBy(e => e.Name)
            .Select(e => new {
                e.Id,
                e.Name,
                e.BaseUrl,
                Workspace = e.Workspace != null ? e.Workspace.Name : string.Empty
            })
            .ToListAsync();

        return Json(results);
    }

        [HttpPost("environments/save")]
            public async Task<IActionResult> Save(ApiEnvironment environment)
            {
                if (ModelState.IsValid)
                {
                    if (environment.Id == 0)
                    {
                        environment.CreatedAt = DateTime.Now;
                        _context.Environments.Add(environment);
                    }
                    else
                    {
                        var existing = await _context.Environments.FindAsync(environment.Id);
                        if (existing == null)
                            return NotFound();

                        existing.Name = environment.Name;
                        existing.BaseUrl = environment.BaseUrl;
                        existing.Type = environment.Type;
                        existing.IsActive = environment.IsActive;
                        existing.WorkspaceId = environment.WorkspaceId;
                    }

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index), new { workspaceId = environment.WorkspaceId });
                }
                return View("Index", environment);
            }

            [HttpPost("environments/delete/{id:int}")]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Delete(int id)
            {
                var env = await _context.Environments
                    .Include(e => e.Variables)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (env == null)
                    return NotFound();

                _context.Environments.Remove(env);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

    [HttpGet("environments/edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, [FromHeader(Name = "X-Requested-With")] string? requestedWith)
    {
        var environment = await _context.Environments.FindAsync(id);
        if (environment == null)
            return NotFound();

        if (requestedWith == "XMLHttpRequest")
            return PartialView("_EnvironmentEdit", environment);

        return View(environment);
    }

}