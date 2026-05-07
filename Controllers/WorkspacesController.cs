using Microsoft.AspNetCore.Mvc;
using API_tester.Models;
using API_tester.Data;
using Microsoft.EntityFrameworkCore;

namespace API_tester.Controllers;

public class WorkspacesController : Controller
{
    private readonly AppDbContext _context;

    public WorkspacesController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var workspaces = await _context.ApiWorkspaces
            .Include(w => w.OwnerUser)
            .Include(w => w.Collections)
            .Include(w => w.Environments)
            .ToListAsync();

        ViewData["Title"] = "Workspaces";
        ViewData["BreadcrumbCurrent"] = "Workspaces";
        ViewData["HeroKicker"] = "Workspace Registry";
        ViewData["HeroTitle"] = "Organize API domains as team workspaces.";
        ViewData["HeroDescription"] = "Review all workspaces with ownership and structure. Each card immediately shows who owns it and how many collections and environments it contains.";
        ViewData["PrimaryActionText"] = "+ New Workspace";
        ViewData["SecondaryActionText"] = "Import Workspace";
        ViewData["SearchPlaceholder"] = "Search by name, owner or tag";
        ViewData["Filters"] = new List<string> { "All", "My Team", "Production", "Sandbox" };

        return View(workspaces);
    }

    [HttpPost]
    public async Task<IActionResult> Create(ApiWorkspace workspace)
    {
        if (ModelState.IsValid)
        {
            workspace.CreatedAt = DateTime.Now;
            _context.ApiWorkspaces.Add(workspace);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(workspace);
    }
}
