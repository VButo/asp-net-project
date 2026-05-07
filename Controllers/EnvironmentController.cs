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

    public async Task<IActionResult> Index(int? workspaceId)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        ApiWorkspace? workspace = null;

        if (workspaceId.HasValue)
            workspace = await _context.ApiWorkspaces
                .Include(w => w.Environments)
                    .ThenInclude(e => e.Variables)
                .Include(w => w.OwnerUser)
                .FirstOrDefaultAsync(w => w.Id == workspaceId.Value);
        else
            workspace = await _context.ApiWorkspaces
                .Include(w => w.Environments)
                    .ThenInclude(e => e.Variables)
                .Include(w => w.OwnerUser)
                .FirstOrDefaultAsync();

        if (workspace == null)
            return NotFound();

        ViewData["Title"] = "Environments";
        ViewData["BreadcrumbCurrent"] = "Environments";
        ViewData["HeroKicker"] = "Environment Registry";
        ViewData["HeroTitle"] = $"{workspace.Name} Environments";
        ViewData["HeroDescription"] = "Browse available environment configurations and jump straight into the Request Builder.";
        ViewData["PrimaryActionText"] = "Open Builder";
        ViewData["SecondaryActionText"] = "Back to Workspace";

        return View(workspace);
    }

        [HttpPost]
        public async Task<IActionResult> Save(ApiEnvironment environment)
        {
            if (ModelState.IsValid)
            {
                environment.CreatedAt = DateTime.Now;
                _context.ApiEnvironments.Add(environment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { workspaceId = environment.WorkspaceId });
            }
            return View("Index", environment);
        }
}