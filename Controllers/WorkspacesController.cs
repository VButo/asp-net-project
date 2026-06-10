using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using API_tester.Models;
using API_tester.Data;
using Microsoft.EntityFrameworkCore;

namespace API_tester.Controllers;

[Authorize]
public class WorkspacesController : Controller
{
    private readonly AppDbContext _context;

    public WorkspacesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("workspaces")]
    public async Task<IActionResult> Index()
    {
        var workspaces = await _context.Workspaces
            .Include(w => w.Collections)
            .Include(w => w.Environments)
            .ToListAsync();

        ViewData["Title"] = "Workspaces";
        ViewData["BreadcrumbCurrent"] = "Workspaces";
        ViewData["HeroKicker"] = "Workspace Registry";
        ViewData["HeroTitle"] = "Organize API domains as team workspaces.";
        ViewData["HeroDescription"] = "Review all workspaces with structure and scope. Each card immediately shows how many collections and environments it contains.";
        ViewData["PrimaryActionText"] = "+ New Workspace";
        ViewData["SecondaryActionText"] = "Import Workspace";
        ViewData["SearchPlaceholder"] = "Search by name or tag";
        ViewData["Filters"] = new List<string> { "All", "My Team", "Production", "Sandbox" };

        return View(workspaces);
    }

    [HttpGet("workspaces/search")]
    public async Task<IActionResult> Search([FromQuery(Name = "q")] string? q)
    {
        var term = (q ?? string.Empty).Trim();

        IQueryable<ApiWorkspace> query = _context.Workspaces;

        if (!string.IsNullOrEmpty(term))
        {
            query = query.Where(w => EF.Functions.Like(w.Name, $"%{term}%") || EF.Functions.Like(w.Description, $"%{term}%"));
        }

        var results = await query
            .OrderBy(w => w.Name)
            .Select(w => new {
                id = w.Id,
                name = w.Name,
                description = w.Description,
                collections = w.Collections.Count,
                environments = w.Environments.Count
            })
            .ToListAsync();

        return Json(results);
    }

    [HttpPost("workspaces/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ApiWorkspace workspace)
    {
        if (ModelState.IsValid)
        {
            workspace.CreatedAt = DateTime.Now;

            _context.Workspaces.Add(workspace);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(workspace);
    }

    [HttpGet("workspaces/edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, [FromHeader(Name = "X-Requested-With")] string? requestedWith)
    {
        var workspace = await _context.Workspaces.FindAsync(id);
        if (workspace == null)
            return NotFound();

        if (requestedWith == "XMLHttpRequest")
        {
            return PartialView("_WorkspaceEdit", workspace);
        }

        return View(workspace);
    }

    [HttpPost("workspaces/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ApiWorkspace model)
    {
        if (id != model.Id)
            return BadRequest();

        var workspace = await _context.Workspaces.FindAsync(id);
        if (workspace == null)
            return NotFound();

        if (ModelState.IsValid)
        {
            workspace.Name = model.Name;
            workspace.Description = model.Description;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    [HttpPost("workspaces/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var workspace = await _context.Workspaces
            .Include(w => w.Collections)
            .Include(w => w.Environments)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (workspace == null)
            return NotFound();

        // Simple delete: remove workspace (cascades depend on EF config)
        _context.Workspaces.Remove(workspace);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

}
