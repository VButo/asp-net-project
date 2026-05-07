using Microsoft.AspNetCore.Mvc;
using API_tester.Models;
using API_tester.Models.Enums;
using API_tester.Data;
using Microsoft.EntityFrameworkCore;

namespace API_tester.Controllers;

public class RequestBuilderController : Controller
{
    private readonly AppDbContext _context;

    public RequestBuilderController(AppDbContext context)
    {
        _context = context;
    }

    private async Task LoadCollectionsAsync()
    {
        ViewBag.Collections = await _context.ApiCollections
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? requestId, [FromHeader(Name = "X-Requested-With")] string? requestedWith)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        ApiRequest? model = null;

        if (requestId.HasValue)
            model = await _context.ApiRequests
                .Include(r => r.Collection)
                .Include(r => r.Headers)
                .Include(r => r.TagLinks)
                .FirstOrDefaultAsync(r => r.Id == requestId.Value);

        await LoadCollectionsAsync();

        if (model == null)
        {
            model = new ApiRequest
            {
                Name = string.Empty,
                Url = string.Empty,
                Method = HttpMethodType.Get,
                Body = string.Empty,
                CollectionId = 0,
                CreatedAt = DateTime.Now
            };
        }

        if (requestedWith == "XMLHttpRequest")
            return PartialView("_RequestBuilder", model);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(ApiRequest request)
    {
        if (ModelState.IsValid)
        {
            if (request.Id == 0)
            {
                request.CreatedAt = DateTime.Now;
                _context.ApiRequests.Add(request);
            }
            else
            {
                var existingRequest = await _context.ApiRequests.FirstOrDefaultAsync(r => r.Id == request.Id);

                if (existingRequest == null)
                {
                    return NotFound();
                }

                existingRequest.Name = request.Name;
                existingRequest.Url = request.Url;
                existingRequest.Method = request.Method;
                existingRequest.Body = request.Body;
                existingRequest.CollectionId = request.CollectionId;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "RequestBuilder", new { requestId = request.Id });
        }

        await LoadCollectionsAsync();
        return View("Index", request);
    }
}
