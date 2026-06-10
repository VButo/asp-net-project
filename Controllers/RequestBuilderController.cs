using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using API_tester.Models;
using API_tester.Models.Enums;
using API_tester.Data;
using Microsoft.EntityFrameworkCore;

namespace API_tester.Controllers;

[Authorize]
public class RequestBuilderController : Controller
{
    private readonly AppDbContext _context;

    public RequestBuilderController(AppDbContext context)
    {
        _context = context;
    }

    private async Task LoadCollectionsAsync()
    {
        ViewBag.Collections = await _context.Collections
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    [HttpGet("request-builder")]
    [HttpGet("request-builder/{requestId:int}")]
    public async Task<IActionResult> Index(int? requestId, [FromHeader(Name = "X-Requested-With")] string? requestedWith)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        ApiRequest? model = null;

        if (requestId.HasValue)
            model = await _context.Requests
                .Include(r => r.Collection)
                .Include(r => r.Headers)
                .Include(r => r.TagLinks)
                    .ThenInclude(t => t.Tag)
                .Include(r => r.EnvironmentLinks)
                    .ThenInclude(e => e.Environment)
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

    [HttpPost("request-builder/save")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(ApiRequest request, int? EnvironmentId, int? TagId)
    {
        RemoveBlankHeaderValidationErrors(request.Headers);

        if (ModelState.IsValid)
        {
            ApiRequest savedRequest;

            if (request.Id == 0)
            {
                request.CreatedAt = DateTime.Now;
                _context.Requests.Add(request);
                savedRequest = request;
            }
            else
            {
                var existingRequest = await _context.Requests
                    .Include(r => r.Headers)
                    .Include(r => r.TagLinks)
                    .Include(r => r.EnvironmentLinks)
                    .FirstOrDefaultAsync(r => r.Id == request.Id);

                if (existingRequest == null)
                {
                    return NotFound();
                }

                existingRequest.Name = request.Name;
                existingRequest.Url = request.Url;
                existingRequest.Method = request.Method;
                existingRequest.Body = request.Body;
                existingRequest.CollectionId = request.CollectionId;
                savedRequest = existingRequest;
            }

            await _context.SaveChangesAsync();

            await SaveHeadersAsync(savedRequest.Id, request.Headers);
            await SaveDefaultEnvironmentAsync(savedRequest.Id, EnvironmentId);
            await SaveSingleTagAsync(savedRequest.Id, TagId);

            return RedirectToAction("Index", "RequestBuilder", new { requestId = savedRequest.Id });
        }
        await LoadCollectionsAsync();
        return View("Index", request);
    }

    private void RemoveBlankHeaderValidationErrors(IEnumerable<ApiHeader>? headers)
    {
        if (headers == null)
        {
            return;
        }

        var index = 0;
        foreach (var header in headers)
        {
            if (string.IsNullOrWhiteSpace(header.Key) && string.IsNullOrWhiteSpace(header.Value))
            {
                ModelState.Remove($"Headers[{index}].Key");
                ModelState.Remove($"Headers[{index}].Value");
            }

            index++;
        }
    }

    private async Task SaveHeadersAsync(int requestId, IEnumerable<ApiHeader>? postedHeaders)
    {
        var existingHeaders = await _context.Headers
            .Where(h => h.RequestId == requestId)
            .ToListAsync();

        _context.Headers.RemoveRange(existingHeaders);

        var cleanHeaders = (postedHeaders ?? Enumerable.Empty<ApiHeader>())
            .Where(h => !string.IsNullOrWhiteSpace(h.Key))
            .Select(h => new ApiHeader
            {
                RequestId = requestId,
                Key = h.Key.Trim(),
                Value = h.Value?.Trim() ?? string.Empty,
                IsEnabled = h.IsEnabled
            })
            .ToList();

        if (cleanHeaders.Count > 0)
        {
            await _context.Headers.AddRangeAsync(cleanHeaders);
        }

        await _context.SaveChangesAsync();
    }

    private async Task SaveDefaultEnvironmentAsync(int requestId, int? environmentId)
    {
        var existingLinks = await _context.RequestEnvironmentLinks
            .Where(l => l.RequestId == requestId)
            .ToListAsync();

        _context.RequestEnvironmentLinks.RemoveRange(existingLinks);

        if (environmentId.HasValue && environmentId.Value > 0)
        {
            await _context.RequestEnvironmentLinks.AddAsync(new RequestEnvironmentLink
            {
                RequestId = requestId,
                EnvironmentId = environmentId.Value,
                IsDefaultEnvironment = true,
                LinkedAt = DateTime.Now
            });
        }

        await _context.SaveChangesAsync();
    }

    private async Task SaveSingleTagAsync(int requestId, int? tagId)
    {
        var existingTags = await _context.RequestTagMaps
            .Where(l => l.RequestId == requestId)
            .ToListAsync();

        _context.RequestTagMaps.RemoveRange(existingTags);

        if (tagId.HasValue && tagId.Value > 0)
        {
            await _context.RequestTagMaps.AddAsync(new RequestTagMap
            {
                RequestId = requestId,
                TagId = tagId.Value,
                LinkedAt = DateTime.Now
            });
        }

        await _context.SaveChangesAsync();
    }

    [HttpPost("request-builder/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var request = await _context.Requests.FindAsync(id);
        if (request == null)
            return NotFound();

        _context.Requests.Remove(request);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index", "Request");
    }
}
