using Microsoft.AspNetCore.Mvc;
using API_tester.Models;
using API_tester.Models.Enums;
using API_tester.Data;
using Microsoft.EntityFrameworkCore;

namespace API_tester.Controllers;

public class RequestDetailsController : Controller
{
    private readonly AppDbContext _context;

    public RequestDetailsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Details(int requestId, [FromHeader(Name = "X-Requested-With")] string? requestedWith)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var request = await _context.ApiRequests
            .Include(r => r.Collection)
            .Include(r => r.Headers)
            .Include(r => r.Responses)
            .Include(r => r.TagLinks)
                .ThenInclude(t => t.Tag)
            .Include(r => r.EnvironmentLinks)
                .ThenInclude(e => e.Environment)
            .FirstOrDefaultAsync(r => r.Id == requestId);
        if (request == null)
        {
            return NotFound();
        }

        // If AJAX request, return partial for modal; otherwise full page
        if (requestedWith == "XMLHttpRequest")
        {
            return PartialView("_RequestDetails", request);
        }

        return View(request);
    }
}
