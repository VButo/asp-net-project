using API_tester.Data;
using API_tester.Dtos;
using API_tester.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_tester.Controllers.Api;

[ApiController]
[Route("api/collections")]
public class CollectionsApiController : ControllerBase
{
    private readonly AppDbContext _context;

    public CollectionsApiController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<CollectionDto>>> GetAll([FromQuery] string? q)
    {
        var query = _context.Collections.Include(c => c.Workspace).Include(c => c.Requests).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(c => EF.Functions.Like(c.Name, $"%{q}%") || EF.Functions.Like(c.Description, $"%{q}%"));
        }

        var collections = await query.OrderBy(c => c.Name).ToListAsync();
        return Ok(collections.Select(c => c.ToDto()));
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<ActionResult<CollectionDto>> GetById(int id)
    {
        var collection = await _context.Collections.Include(c => c.Workspace).Include(c => c.Requests).FirstOrDefaultAsync(c => c.Id == id);
        return collection == null ? NotFound() : Ok(collection.ToDto());
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<CollectionDto>> Create([FromBody] CollectionWriteDto dto)
    {
        if (!await _context.Workspaces.AnyAsync(w => w.Id == dto.WorkspaceId)) return BadRequest("Workspace does not exist.");
        var collection = new ApiCollection { Name = dto.Name, Description = dto.Description ?? string.Empty, IsShared = dto.IsShared, WorkspaceId = dto.WorkspaceId, CreatedAt = DateTime.UtcNow };
        _context.Collections.Add(collection);
        await _context.SaveChangesAsync();
        collection.Workspace = await _context.Workspaces.FindAsync(collection.WorkspaceId);
        return CreatedAtAction(nameof(GetById), new { id = collection.Id }, collection.ToDto());
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<CollectionDto>> Update(int id, [FromBody] CollectionWriteDto dto)
    {
        var collection = await _context.Collections.Include(c => c.Workspace).Include(c => c.Requests).FirstOrDefaultAsync(c => c.Id == id);
        if (collection == null) return NotFound();
        if (!await _context.Workspaces.AnyAsync(w => w.Id == dto.WorkspaceId)) return BadRequest("Workspace does not exist.");
        collection.Name = dto.Name;
        collection.Description = dto.Description ?? string.Empty;
        collection.IsShared = dto.IsShared;
        collection.WorkspaceId = dto.WorkspaceId;
        await _context.SaveChangesAsync();
        return Ok(collection.ToDto());
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var collection = await _context.Collections.FindAsync(id);
        if (collection == null) return NotFound();
        _context.Collections.Remove(collection);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
