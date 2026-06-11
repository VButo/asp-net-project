using API_tester.Data;
using API_tester.Dtos;
using API_tester.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_tester.Controllers.Api;

[ApiController]
[Route("api/request-headers")]
[Route("api/headers")]
public class RequestHeadersApiController : ControllerBase
{
    private readonly AppDbContext _context;

    public RequestHeadersApiController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<HeaderDto>>> GetAll([FromQuery] string? q, [FromQuery] int? requestId)
    {
        var query = _context.Headers.AsQueryable();
        if (requestId.HasValue) query = query.Where(h => h.RequestId == requestId.Value);
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(h => EF.Functions.Like(h.Key, $"%{q}%") || EF.Functions.Like(h.Value, $"%{q}%"));
        var headers = await query.OrderBy(h => h.Key).ToListAsync();
        return Ok(headers.Select(h => h.ToDto()));
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<ActionResult<HeaderDto>> GetById(int id)
    {
        var header = await _context.Headers.FindAsync(id);
        return header == null ? NotFound() : Ok(header.ToDto());
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<HeaderDto>> Create([FromBody] HeaderWriteDto dto)
    {
        if (!await _context.Requests.AnyAsync(r => r.Id == dto.RequestId)) return BadRequest("Request does not exist.");
        var header = new ApiHeader { RequestId = dto.RequestId, Key = dto.Key, Value = dto.Value ?? string.Empty, IsEnabled = dto.IsEnabled };
        _context.Headers.Add(header);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = header.Id }, header.ToDto());
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<HeaderDto>> Update(int id, [FromBody] HeaderWriteDto dto)
    {
        var header = await _context.Headers.FindAsync(id);
        if (header == null) return NotFound();
        if (!await _context.Requests.AnyAsync(r => r.Id == dto.RequestId)) return BadRequest("Request does not exist.");
        header.RequestId = dto.RequestId;
        header.Key = dto.Key;
        header.Value = dto.Value ?? string.Empty;
        header.IsEnabled = dto.IsEnabled;
        await _context.SaveChangesAsync();
        return Ok(header.ToDto());
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var header = await _context.Headers.FindAsync(id);
        if (header == null) return NotFound();
        _context.Headers.Remove(header);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
