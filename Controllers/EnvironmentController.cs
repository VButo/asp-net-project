using Microsoft.AspNetCore.Mvc;
using API_tester.Models;
using API_tester.Models.Enums;
using API_tester.Data;
using Microsoft.EntityFrameworkCore;

namespace API_tester.Controllers;

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
                .ThenInclude(w => w.OwnerUser)
            .Include(e => e.Variables)
            .AsQueryable();

        if (workspaceId.HasValue)
        {
            environmentsQuery = environmentsQuery.Where(environment => environment.WorkspaceId == workspaceId.Value);
            var workspace = await _context.Workspaces
                .Include(w => w.OwnerUser)
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

        [HttpPost("environments/save")]
        public async Task<IActionResult> Save(ApiEnvironment environment)
        {
            if (ModelState.IsValid)
            {
                environment.CreatedAt = DateTime.Now;
                _context.Environments.Add(environment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { workspaceId = environment.WorkspaceId });
            }
            return View("Index", environment);
        }
}