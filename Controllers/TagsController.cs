using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using API_tester.Models;
using API_tester.Data;
using Microsoft.EntityFrameworkCore;

namespace API_tester.Controllers;

[Authorize]
public class TagsController : Controller
{
    private readonly AppDbContext _context;

    public TagsController(AppDbContext context)
    {
        _context = context;
    }

    private async Task<IActionResult> TagsIndexWithValidationAsync()
    {
        var tags = await _context.RequestTags
            .OrderBy(t => t.Name)
            .ToListAsync();

        ViewData["Title"] = "Tags";
        ViewData["BreadcrumbCurrent"] = "Tags";
        Response.StatusCode = StatusCodes.Status400BadRequest;
        return View("Index", tags);
    }

    [HttpGet("tags")]
    public async Task<IActionResult> Index()
    {
        var tags = await _context.RequestTags
            .OrderBy(t => t.Name)
            .ToListAsync();

        ViewData["Title"] = "Tags";
        ViewData["BreadcrumbCurrent"] = "Tags";
        return View(tags);
    }

    [HttpGet("tags/search")]
    public async Task<IActionResult> Search([FromQuery(Name = "q")] string? q)
    {
        var term = (q ?? string.Empty).Trim();

        var query = _context.RequestTags.AsQueryable();
        if (!string.IsNullOrEmpty(term))
        {
            query = query.Where(t => EF.Functions.Like(t.Name, $"%{term}%"));
        }

        var results = await query
            .OrderBy(t => t.Name)
            .Select(t => new
            {
                id = t.Id,
                name = t.Name,
                description = t.ColorHex,
                colorHex = t.ColorHex,
                createdAt = t.CreatedAt
            })
            .ToListAsync();

        return Json(results);
    }

    [HttpPost("tags/save")]
    [Authorize(Roles = "Admin,Manager")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(RequestTag tag)
    {
        if (!ModelState.IsValid)
        {
            return await TagsIndexWithValidationAsync();
        }

        if (tag.CreatedAt == default)
        {
            tag.CreatedAt = DateTime.Now;
        }

        if (tag.Id == 0)
        {
            _context.RequestTags.Add(tag);
        }
        else
        {
            var existing = await _context.RequestTags.FindAsync(tag.Id);
            if (existing == null)
            {
                return NotFound();
            }

            existing.Name = tag.Name;
            existing.ColorHex = tag.ColorHex;
            existing.CreatedAt = tag.CreatedAt;
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("tags/delete/{id:int}")]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var tag = await _context.RequestTags
            .Include(t => t.RequestLinks)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tag == null)
        {
            return NotFound();
        }

        _context.RequestTags.Remove(tag);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
