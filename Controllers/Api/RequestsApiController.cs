using API_tester.Data;
using API_tester.Dtos;
using API_tester.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_tester.Controllers.Api;

[ApiController]
[Route("api/requests")]
public class RequestsApiController : ControllerBase
{
    private readonly AppDbContext _context;

    public RequestsApiController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<RequestDto>>> GetAll([FromQuery] string? q)
    {
        var query = IncludeRequestGraph(_context.Requests);
        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(r => EF.Functions.Like(r.Name, $"%{q}%") || EF.Functions.Like(r.Url, $"%{q}%"));
        }

        var requests = await query.OrderBy(r => r.Name).ToListAsync();
        return Ok(requests.Select(r => r.ToDto()));
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<ActionResult<RequestDto>> GetById(int id)
    {
        var request = await IncludeRequestGraph(_context.Requests).FirstOrDefaultAsync(r => r.Id == id);
        return request == null ? NotFound() : Ok(request.ToDto());
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<RequestDto>> Create([FromBody] RequestWriteDto dto)
    {
        if (!await _context.Collections.AnyAsync(c => c.Id == dto.CollectionId)) return BadRequest("Collection does not exist.");
        var request = new ApiRequest { Name = dto.Name, Url = dto.Url, Method = dto.Method, Body = dto.Body ?? string.Empty, CollectionId = dto.CollectionId, CreatedAt = DateTime.UtcNow };
        _context.Requests.Add(request);
        await _context.SaveChangesAsync();
        await SaveRequestLinksAsync(request.Id, dto.EnvironmentId, dto.TagIds);
        var saved = await IncludeRequestGraph(_context.Requests).FirstAsync(r => r.Id == request.Id);
        return CreatedAtAction(nameof(GetById), new { id = request.Id }, saved.ToDto());
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<RequestDto>> Update(int id, [FromBody] RequestWriteDto dto)
    {
        var request = await _context.Requests.FirstOrDefaultAsync(r => r.Id == id);
        if (request == null) return NotFound();
        if (!await _context.Collections.AnyAsync(c => c.Id == dto.CollectionId)) return BadRequest("Collection does not exist.");
        request.Name = dto.Name;
        request.Url = dto.Url;
        request.Method = dto.Method;
        request.Body = dto.Body ?? string.Empty;
        request.CollectionId = dto.CollectionId;
        await _context.SaveChangesAsync();
        await SaveRequestLinksAsync(id, dto.EnvironmentId, dto.TagIds);
        var saved = await IncludeRequestGraph(_context.Requests).FirstAsync(r => r.Id == id);
        return Ok(saved.ToDto());
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var request = await _context.Requests.FindAsync(id);
        if (request == null) return NotFound();
        _context.Requests.Remove(request);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static IQueryable<ApiRequest> IncludeRequestGraph(IQueryable<ApiRequest> query) =>
        query.Include(r => r.Collection)
            .Include(r => r.Headers)
            .Include(r => r.Attachments)
            .Include(r => r.TagLinks).ThenInclude(link => link.Tag)
            .Include(r => r.EnvironmentLinks).ThenInclude(link => link.Environment);

    private async Task SaveRequestLinksAsync(int requestId, int? environmentId, IReadOnlyList<int>? tagIds)
    {
        var envLinks = await _context.RequestEnvironmentLinks.Where(l => l.RequestId == requestId).ToListAsync();
        _context.RequestEnvironmentLinks.RemoveRange(envLinks);
        if (environmentId.HasValue && environmentId.Value > 0 && await _context.Environments.AnyAsync(e => e.Id == environmentId.Value))
        {
            _context.RequestEnvironmentLinks.Add(new RequestEnvironmentLink { RequestId = requestId, EnvironmentId = environmentId.Value, IsDefaultEnvironment = true, LinkedAt = DateTime.UtcNow });
        }

        var tagLinks = await _context.RequestTagMaps.Where(l => l.RequestId == requestId).ToListAsync();
        _context.RequestTagMaps.RemoveRange(tagLinks);
        foreach (var tagId in (tagIds ?? Array.Empty<int>()).Distinct())
        {
            if (await _context.RequestTags.AnyAsync(t => t.Id == tagId))
            {
                _context.RequestTagMaps.Add(new RequestTagMap { RequestId = requestId, TagId = tagId, LinkedAt = DateTime.UtcNow });
            }
        }

        await _context.SaveChangesAsync();
    }
}
