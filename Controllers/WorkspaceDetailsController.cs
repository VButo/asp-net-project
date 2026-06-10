using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using API_tester.Models;
using API_tester.Data;
using Microsoft.EntityFrameworkCore;

namespace API_tester.Controllers;

[Authorize]
public class WorkspaceDetailsController : Controller
{
    private readonly AppDbContext _context;

    public WorkspaceDetailsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("workspace-details")]
    [HttpGet("workspace-details/{workspaceId:int}")]
    public async Task<IActionResult> Details(int? workspaceId, [FromHeader(Name = "X-Requested-With")] string? requestedWith)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        ApiWorkspace? workspace = null;

        if (workspaceId.HasValue)
            workspace = await _context.Workspaces
                .Include(w => w.Collections)
                .Include(w => w.Environments)
                .FirstOrDefaultAsync(w => w.Id == workspaceId.Value);
        else
            workspace = await _context.Workspaces
                .Include(w => w.Collections)
                .Include(w => w.Environments)
                .FirstOrDefaultAsync();

        if (workspace == null)
            return RedirectToAction("Index", "Workspaces");

        if (requestedWith == "XMLHttpRequest")
            return PartialView("_WorkspaceDetails", workspace);

        return View(workspace);
    }
}
