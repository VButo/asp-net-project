using API_tester.Data;
using API_tester.Dtos;
using API_tester.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_tester.Controllers.Api;

[ApiController]
[Route("api/tags")]
public class TagsApiController : ControllerBase
{
    private readonly AppDbContext _context;

    public TagsApiController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<TagDto>>> GetAll([FromQuery] string? q)
    {
        var query = _context.RequestTags.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(t => EF.Functions.Like(t.Name, $"%{q}%"));
        var tags = await query.OrderBy(t => t.Name).ToListAsync();
        return Ok(tags.Select(t => t.ToDto()));
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<ActionResult<TagDto>> GetById(int id)
    {
        var tag = await _context.RequestTags.FindAsync(id);
        return tag == null ? NotFound() : Ok(tag.ToDto());
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<TagDto>> Create([FromBody] TagWriteDto dto)
    {
        var tag = new RequestTag { Name = dto.Name, ColorHex = dto.ColorHex, CreatedAt = DateTime.UtcNow };
        _context.RequestTags.Add(tag);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = tag.Id }, tag.ToDto());
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<TagDto>> Update(int id, [FromBody] TagWriteDto dto)
    {
        var tag = await _context.RequestTags.FindAsync(id);
        if (tag == null) return NotFound();
        tag.Name = dto.Name;
        tag.ColorHex = dto.ColorHex;
        await _context.SaveChangesAsync();
        return Ok(tag.ToDto());
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var tag = await _context.RequestTags.FindAsync(id);
        if (tag == null) return NotFound();
        _context.RequestTags.Remove(tag);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
