using Microsoft.AspNetCore.Mvc;
using API_tester.Models;
using API_tester.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
        var currentUser = await ResolveCurrentUserAsync();

        var workspaces = await _context.ApiWorkspaces
            .Include(w => w.OwnerUser)
            .Include(w => w.Collections)
            .Include(w => w.Environments)
            .ToListAsync();

        ViewBag.Users = await _context.Users
            .OrderBy(user => user.Username)
            .ToListAsync();
        ViewBag.CurrentUserId = currentUser?.Id ?? 0;

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
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ApiWorkspace workspace)
    {
        if (ModelState.IsValid)
        {
            workspace.CreatedAt = DateTime.Now;
            if (workspace.OwnerUserId == 0)
            {
                var currentUser = await ResolveCurrentUserAsync();
                if (currentUser == null)
                {
                    ModelState.AddModelError(nameof(workspace.OwnerUserId), "Current user was not found in the User table.");
                    return RedirectToAction(nameof(Index));
                }

                workspace.OwnerUserId = currentUser.Id;
            }

            _context.ApiWorkspaces.Add(workspace);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(workspace);
    }

    private async Task<User?> ResolveCurrentUserAsync()
    {
        var identityName = User.Identity?.Name;

        if (!string.IsNullOrWhiteSpace(identityName))
        {
            var currentUser = await _context.Users.FirstOrDefaultAsync(user =>
                user.Username == identityName || user.Email == identityName);

            if (currentUser != null)
            {
                return currentUser;
            }
        }

        var emailClaim = User.FindFirstValue(ClaimTypes.Email);
        if (!string.IsNullOrWhiteSpace(emailClaim))
        {
            var currentUser = await _context.Users.FirstOrDefaultAsync(user => user.Email == emailClaim);
            if (currentUser != null)
            {
                return currentUser;
            }
        }

        var nameClaim = User.FindFirstValue(ClaimTypes.Name);
        if (!string.IsNullOrWhiteSpace(nameClaim))
        {
            var currentUser = await _context.Users.FirstOrDefaultAsync(user =>
                user.Username == nameClaim || user.Email == nameClaim);

            if (currentUser != null)
            {
                return currentUser;
            }
        }

        return await _context.Users.OrderBy(user => user.Id).FirstOrDefaultAsync();
    }
}
