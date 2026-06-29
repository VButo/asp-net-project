using API_tester.Data;
using API_tester.Dtos;
using API_tester.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_tester.Controllers.Api;

[ApiController]
[Route("api/workspaces")]
public class WorkspacesApiController : ControllerBase
{
    private readonly AppDbContext _context;

    public WorkspacesApiController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WorkspaceDto>>> GetAll([FromQuery] string? q)
    {
        var query = _context.Workspaces.Include(w => w.Collections).Include(w => w.Environments).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(w => EF.Functions.Like(w.Name, $"%{q}%") || EF.Functions.Like(w.Description, $"%{q}%"));
        }

        var workspaces = await query.OrderBy(w => w.Name).ToListAsync();
        return Ok(workspaces.Select(w => w.ToDto()));
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<ActionResult<WorkspaceDto>> GetById(int id)
    {
        var workspace = await _context.Workspaces.Include(w => w.Collections).Include(w => w.Environments).FirstOrDefaultAsync(w => w.Id == id);
        return workspace == null ? NotFound() : Ok(workspace.ToDto());
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<WorkspaceDto>> Create([FromBody] WorkspaceWriteDto dto)
    {
        var workspace = new ApiWorkspace { Name = dto.Name, Description = dto.Description ?? string.Empty, CreatedAt = DateTime.UtcNow };
        _context.Workspaces.Add(workspace);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = workspace.Id }, workspace.ToDto());
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<WorkspaceDto>> Update(int id, [FromBody] WorkspaceWriteDto dto)
    {
        var workspace = await _context.Workspaces.Include(w => w.Collections).Include(w => w.Environments).FirstOrDefaultAsync(w => w.Id == id);
        if (workspace == null) return NotFound();
        workspace.Name = dto.Name;
        workspace.Description = dto.Description ?? string.Empty;
        await _context.SaveChangesAsync();
        return Ok(workspace.ToDto());
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var workspace = await _context.Workspaces.FindAsync(id);
        if (workspace == null) return NotFound();
        _context.Workspaces.Remove(workspace);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
