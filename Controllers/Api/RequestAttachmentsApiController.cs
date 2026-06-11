using API_tester.Data;
using API_tester.Dtos;
using API_tester.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_tester.Controllers.Api;

[ApiController]
[Route("api/request-attachments")]
public class RequestAttachmentsApiController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public RequestAttachmentsApiController(AppDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<AttachmentDto>>> GetAll([FromQuery] int? requestId, [FromQuery] string? q)
    {
        var query = _context.RequestAttachments.AsQueryable();
        if (requestId.HasValue && requestId.Value > 0)
        {
            query = query.Where(a => a.RequestId == requestId.Value);
        }
        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(a => EF.Functions.Like(a.FileName, $"%{q}%") || EF.Functions.Like(a.ContentType, $"%{q}%"));
        }

        var attachments = await query.OrderByDescending(a => a.CreatedAt).ToListAsync();
        return Ok(attachments.Select(a => a.ToDto()));
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<ActionResult<AttachmentDto>> GetById(int id)
    {
        var attachment = await _context.RequestAttachments.FindAsync(id);
        return attachment == null ? NotFound() : Ok(attachment.ToDto());
    }

    [HttpPost("{requestId:int}")]
    [Authorize(Roles = "Admin,Manager")]
    [RequestSizeLimit(25_000_000)]
    public async Task<ActionResult<AttachmentDto>> Upload(int requestId, IFormFile file)
    {
        if (!await _context.Requests.AnyAsync(r => r.Id == requestId)) return NotFound();
        if (file == null || file.Length == 0) return BadRequest("A non-empty file is required.");

        var uploadsRoot = Path.Combine(_environment.WebRootPath, "uploads", "requests", requestId.ToString());
        Directory.CreateDirectory(uploadsRoot);

        var extension = Path.GetExtension(file.FileName);
        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var physicalPath = Path.Combine(uploadsRoot, storedFileName);

        await using (var stream = System.IO.File.Create(physicalPath))
        {
            await file.CopyToAsync(stream);
        }

        var attachment = new RequestAttachment
        {
            RequestId = requestId,
            FileName = Path.GetFileName(file.FileName),
            StoredFileName = storedFileName,
            FilePath = $"/uploads/requests/{requestId}/{storedFileName}",
            ContentType = file.ContentType ?? string.Empty,
            FileSize = file.Length,
            CreatedAt = DateTime.UtcNow
        };

        _context.RequestAttachments.Add(attachment);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = attachment.Id }, attachment.ToDto());
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<AttachmentDto>> Update(int id, [FromBody] AttachmentWriteDto dto)
    {
        var attachment = await _context.RequestAttachments.FindAsync(id);
        if (attachment == null) return NotFound();

        attachment.FileName = Path.GetFileName(dto.FileName);
        await _context.SaveChangesAsync();
        return Ok(attachment.ToDto());
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var attachment = await _context.RequestAttachments.FindAsync(id);
        if (attachment == null) return NotFound();

        var physicalPath = Path.Combine(_environment.WebRootPath, attachment.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (System.IO.File.Exists(physicalPath))
        {
            System.IO.File.Delete(physicalPath);
        }

        _context.RequestAttachments.Remove(attachment);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
