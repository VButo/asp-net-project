using API_tester.Data;
using API_tester.Dtos;
using API_tester.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_tester.Controllers.Api;

[ApiController]
[Route("api/environments")]
public class EnvironmentsApiController : ControllerBase
{
    private readonly AppDbContext _context;

    public EnvironmentsApiController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<EnvironmentDto>>> GetAll([FromQuery] string? q)
    {
        var query = _context.Environments.Include(e => e.Workspace).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(e => EF.Functions.Like(e.Name, $"%{q}%") || EF.Functions.Like(e.BaseUrl, $"%{q}%"));
        }

        var environments = await query.OrderBy(e => e.Name).ToListAsync();
        return Ok(environments.Select(e => e.ToDto()));
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<ActionResult<EnvironmentDto>> GetById(int id)
    {
        var environment = await _context.Environments.Include(e => e.Workspace).FirstOrDefaultAsync(e => e.Id == id);
        return environment == null ? NotFound() : Ok(environment.ToDto());
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<EnvironmentDto>> Create([FromBody] EnvironmentWriteDto dto)
    {
        if (!await _context.Workspaces.AnyAsync(w => w.Id == dto.WorkspaceId)) return BadRequest("Workspace does not exist.");
        var environment = new ApiEnvironment { Name = dto.Name, BaseUrl = dto.BaseUrl, Type = dto.Type, IsActive = dto.IsActive, WorkspaceId = dto.WorkspaceId, CreatedAt = DateTime.UtcNow };
        _context.Environments.Add(environment);
        await _context.SaveChangesAsync();
        environment.Workspace = await _context.Workspaces.FindAsync(environment.WorkspaceId);
        return CreatedAtAction(nameof(GetById), new { id = environment.Id }, environment.ToDto());
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<EnvironmentDto>> Update(int id, [FromBody] EnvironmentWriteDto dto)
    {
        var environment = await _context.Environments.Include(e => e.Workspace).FirstOrDefaultAsync(e => e.Id == id);
        if (environment == null) return NotFound();
        if (!await _context.Workspaces.AnyAsync(w => w.Id == dto.WorkspaceId)) return BadRequest("Workspace does not exist.");
        environment.Name = dto.Name;
        environment.BaseUrl = dto.BaseUrl;
        environment.Type = dto.Type;
        environment.IsActive = dto.IsActive;
        environment.WorkspaceId = dto.WorkspaceId;
        await _context.SaveChangesAsync();
        return Ok(environment.ToDto());
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var environment = await _context.Environments.FindAsync(id);
        if (environment == null) return NotFound();
        _context.Environments.Remove(environment);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
