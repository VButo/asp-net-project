using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using API_tester.Models;
using API_tester.Data;
using Microsoft.EntityFrameworkCore;

namespace API_tester.Controllers;

[Authorize]
public class CollectionController : Controller
{

    private readonly AppDbContext _context;

    public CollectionController(AppDbContext context)
    {
        _context = context;
    }

    private bool IsAjaxRequest()
    {
        return Request.Headers.XRequestedWith == "XMLHttpRequest";
    }

    private async Task<ApiCollection> HydrateCollectionAsync(ApiCollection collection)
    {
        collection.Workspace = collection.WorkspaceId > 0
            ? await _context.Workspaces.FirstOrDefaultAsync(w => w.Id == collection.WorkspaceId)
            : null;

        return collection;
    }

    [HttpGet("collections")]
    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var collections = await _context.Collections
            .Include(c => c.Workspace)
            .Include(c => c.Requests)
            .ToListAsync();

        ViewBag.Workspaces = await _context.Workspaces
            .OrderBy(workspace => workspace.Name)
            .ToListAsync();

        ViewData["Title"] = "Collections";
        ViewData["BreadcrumbCurrent"] = "Collections";
        ViewData["HeroKicker"] = "Collection Catalog";
        ViewData["HeroTitle"] = "Organize endpoint flows through clear API collections.";
        ViewData["HeroDescription"] = "A central collection view for all teams.";
        ViewData["NewCollectionActionText"] = "+ New Collection";
        ViewData["ImportCollectionActionText"] = "Import Collection";
        ViewData["SearchPlaceholder"] = "Search collections";
        ViewData["Filters"] = new List<string> { "All", "Shared", "Private" };

        return View(collections);
    }

    [HttpGet("collections/{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var collection = await _context.Collections
            .Include(c => c.Workspace)
            .Include(c => c.Requests)
                .ThenInclude(r => r.Headers)
            .Include(c => c.Requests)
                .ThenInclude(r => r.Responses)
            .Include(c => c.Requests)
                .ThenInclude(r => r.TagLinks)
                    .ThenInclude(t => t.Tag)
            .FirstOrDefaultAsync(c => c.Id == id);
    
        if (collection == null)
        {
            return NotFound();
        }
    
        ViewData["Title"] = "Collection Details";
        ViewData["BreadcrumbCurrent"] = "Collection Details";
        ViewData["HeroKicker"] = "Collection Details";
        ViewData["HeroTitle"] = collection.Name;
        ViewData["HeroDescription"] = collection.Description;
    
        return View(collection);
    }

    [HttpGet("collections/search")]
    [AllowAnonymous]
    public async Task<IActionResult> Search([FromQuery(Name = "q")] string? q)
    {
        var term = (q ?? string.Empty).Trim();

        IQueryable<ApiCollection> query = _context.Collections
            .Include(c => c.Workspace)
            .Include(c => c.Requests);

        if (!string.IsNullOrEmpty(term))
        {
            query = query.Where(c => EF.Functions.Like(c.Name, $"%{term}%") || EF.Functions.Like(c.Description, $"%{term}%"));
        }

        var results = await query
            .OrderBy(c => c.Name)
            .Select(c => new {
                id = c.Id,
                name = c.Name,
                description = c.Description,
                isShared = c.IsShared,
                workspace = c.Workspace != null ? c.Workspace.Name : string.Empty,
                requests = c.Requests.Count
            })
            .ToListAsync();

        return Json(results);
    }

        [HttpPost("collections/create")]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ApiCollection collection)
        {
            if (!ModelState.IsValid)
            {
                if (IsAjaxRequest())
                {
                    Response.StatusCode = StatusCodes.Status400BadRequest;
                    return PartialView("_CollectionCreate", await HydrateCollectionAsync(collection));
                }

                return RedirectToAction(nameof(Index));
            }

            collection.CreatedAt = DateTime.Now;
            _context.Collections.Add(collection);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = collection.Id });
        }

    [HttpGet("collections/edit/{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Edit(int id, [FromHeader(Name = "X-Requested-With")] string? requestedWith)
    {
        var collection = await _context.Collections
            .Include(c => c.Workspace)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (collection == null)
            return NotFound();

        if (requestedWith == "XMLHttpRequest")
        {
            ViewBag.Workspaces = await _context.Workspaces.OrderBy(w => w.Name).ToListAsync();
            return PartialView("_CollectionEdit", collection);
        }

        return View(collection);
    }

    [HttpPost("collections/edit/{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ApiCollection model)
    {
        if (id != model.Id)
            return BadRequest();

        var collection = await _context.Collections.FindAsync(id);
        if (collection == null)
            return NotFound();

        if (ModelState.IsValid)
        {
            collection.Name = model.Name;
            collection.Description = model.Description;
            collection.WorkspaceId = model.WorkspaceId;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = collection.Id });
        }

        if (IsAjaxRequest())
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            return PartialView("_CollectionEdit", await HydrateCollectionAsync(model));
        }

        return View(model);
    }

    [HttpPost("collections/delete/{id:int}")]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var collection = await _context.Collections
            .Include(c => c.Requests)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (collection == null)
            return NotFound();

        _context.Collections.Remove(collection);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
