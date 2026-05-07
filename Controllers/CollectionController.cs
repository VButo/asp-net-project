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

    public async Task<IActionResult> Index()
    {
        var collections = await _context.ApiCollections
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

    public async Task<IActionResult> Details(int id)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var collection = await _context.ApiCollections
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ApiCollection collection)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }

            collection.CreatedAt = DateTime.Now;
            _context.ApiCollections.Add(collection);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = collection.Id });
        }
}
