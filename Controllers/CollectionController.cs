using Microsoft.AspNetCore.Mvc;
using API_tester.Models;
using API_tester.Data;
using Microsoft.EntityFrameworkCore;

namespace API_tester.Controllers;

public class CollectionController : Controller
{

    private readonly AppDbContext _context;

    public CollectionController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("collections")]
    public async Task<IActionResult> Index()
    {
        var collections = await _context.Collections
            .Include(c => c.Workspace)
            .Include(c => c.Requests)
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

        [HttpPost("collections/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ApiCollection collection)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }

            collection.CreatedAt = DateTime.Now;
            _context.Collections.Add(collection);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = collection.Id });
        }

    [HttpGet("collections/edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, [FromHeader(Name = "X-Requested-With")] string? requestedWith)
    {
        var collection = await _context.Collections.FindAsync(id);
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

        return View(model);
    }

    [HttpPost("collections/delete/{id:int}")]
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
