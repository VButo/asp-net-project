using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using API_tester.Models;
using API_tester.Models.Enums;
using API_tester.Data;
using Microsoft.EntityFrameworkCore;

namespace API_tester.Controllers;

[Authorize]
public class RequestDetailsController : Controller
{
    private readonly AppDbContext _context;

    public RequestDetailsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("request-details")]
    [HttpGet("request-details/{requestId:int}")]
    public async Task<IActionResult> Details(int? requestId, [FromHeader(Name = "X-Requested-With")] string? requestedWith)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var request = requestId.HasValue
            ? await _context.Requests
                .Include(r => r.Collection)
                .Include(r => r.Headers)
                .Include(r => r.Responses)
                .Include(r => r.TagLinks)
                    .ThenInclude(t => t.Tag)
                .Include(r => r.EnvironmentLinks)
                    .ThenInclude(e => e.Environment)
                .FirstOrDefaultAsync(r => r.Id == requestId.Value)
            : await _context.Requests
                .Include(r => r.Collection)
                .Include(r => r.Headers)
                .Include(r => r.Responses)
                .Include(r => r.TagLinks)
                    .ThenInclude(t => t.Tag)
                .Include(r => r.EnvironmentLinks)
                    .ThenInclude(e => e.Environment)
                .OrderBy(r => r.Id)
                .FirstOrDefaultAsync();
        if (request == null)
        {
            return RedirectToAction("Index", "Request");
        }

        // If AJAX request, return partial for modal; otherwise full page
        if (requestedWith == "XMLHttpRequest")
        {
            return PartialView("_RequestDetails", request);
        }

        return View(request);
    }
}
