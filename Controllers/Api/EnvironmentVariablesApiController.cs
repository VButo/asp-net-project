using API_tester.Data;
using API_tester.Dtos;
using API_tester.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_tester.Controllers.Api;

[ApiController]
[Route("api/environment-variables")]
public class EnvironmentVariablesApiController : ControllerBase
{
    private readonly AppDbContext _context;

    public EnvironmentVariablesApiController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<EnvironmentVariableDto>>> GetAll([FromQuery] string? q, [FromQuery] int? environmentId)
    {
        var query = _context.EnvironmentVariables.AsQueryable();
        if (environmentId.HasValue) query = query.Where(v => v.EnvironmentId == environmentId.Value);
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(v => EF.Functions.Like(v.Key, $"%{q}%") || EF.Functions.Like(v.Value, $"%{q}%"));
        var variables = await query.OrderBy(v => v.Key).ToListAsync();
        return Ok(variables.Select(v => v.ToDto()));
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<ActionResult<EnvironmentVariableDto>> GetById(int id)
    {
        var variable = await _context.EnvironmentVariables.FindAsync(id);
        return variable == null ? NotFound() : Ok(variable.ToDto());
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<EnvironmentVariableDto>> Create([FromBody] EnvironmentVariableWriteDto dto)
    {
        if (!await _context.Environments.AnyAsync(e => e.Id == dto.EnvironmentId)) return BadRequest("Environment does not exist.");
        var variable = new EnvironmentVariable { EnvironmentId = dto.EnvironmentId, Key = dto.Key, Value = dto.Value, IsSecret = dto.IsSecret, LastUpdatedAt = dto.LastUpdatedAt ?? DateTime.UtcNow };
        _context.EnvironmentVariables.Add(variable);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = variable.Id }, variable.ToDto());
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<EnvironmentVariableDto>> Update(int id, [FromBody] EnvironmentVariableWriteDto dto)
    {
        var variable = await _context.EnvironmentVariables.FindAsync(id);
        if (variable == null) return NotFound();
        if (!await _context.Environments.AnyAsync(e => e.Id == dto.EnvironmentId)) return BadRequest("Environment does not exist.");
        variable.EnvironmentId = dto.EnvironmentId;
        variable.Key = dto.Key;
        variable.Value = dto.Value;
        variable.IsSecret = dto.IsSecret;
        variable.LastUpdatedAt = dto.LastUpdatedAt ?? DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(variable.ToDto());
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var variable = await _context.EnvironmentVariables.FindAsync(id);
        if (variable == null) return NotFound();
        _context.EnvironmentVariables.Remove(variable);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
