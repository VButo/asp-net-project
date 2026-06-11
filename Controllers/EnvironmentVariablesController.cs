using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using API_tester.Models;
using API_tester.Data;
using Microsoft.EntityFrameworkCore;

namespace API_tester.Controllers;

[Authorize]
public class EnvironmentVariablesController : Controller
{
    private readonly AppDbContext _context;

    public EnvironmentVariablesController(AppDbContext context)
    {
        _context = context;
    }

    private bool IsAjaxRequest()
    {
        return Request.Headers.XRequestedWith == "XMLHttpRequest";
    }

    private async Task<ApiEnvironment?> LoadEnvironmentForEditAsync(int environmentId)
    {
        return await _context.Environments
            .Include(e => e.Workspace)
            .Include(e => e.Variables)
            .FirstOrDefaultAsync(e => e.Id == environmentId);
    }

    [HttpPost("environment-variables/save")]
    [Authorize(Roles = "Admin,Manager")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(EnvironmentVariable variable)
    {
        if (!ModelState.IsValid)
        {
            var environment = await LoadEnvironmentForEditAsync(variable.EnvironmentId);
            if (environment == null)
            {
                return NotFound();
            }

            ModelState.AddModelError(string.Empty, "Variable was not saved. Check the key, value, and updated date.");

            if (IsAjaxRequest())
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return PartialView("~/Views/Environment/_EnvironmentEdit.cshtml", environment);
            }

            return View("~/Views/Environment/Edit.cshtml", environment);
        }

        var environmentExists = await _context.Environments.AnyAsync(e => e.Id == variable.EnvironmentId);
        if (!environmentExists)
        {
            return NotFound();
        }

        if (variable.LastUpdatedAt == default)
        {
            variable.LastUpdatedAt = DateTime.Now;
        }

        if (variable.Id == 0)
        {
            _context.EnvironmentVariables.Add(variable);
        }
        else
        {
            var existing = await _context.EnvironmentVariables.FindAsync(variable.Id);
            if (existing == null)
            {
                return NotFound();
            }

            existing.Key = variable.Key;
            existing.Value = variable.Value;
            existing.IsSecret = variable.IsSecret;
            existing.LastUpdatedAt = variable.LastUpdatedAt;
        }

        await _context.SaveChangesAsync();
        return RedirectToAction("Edit", "Environment", new { id = variable.EnvironmentId });
    }

    [HttpPost("environment-variables/delete/{id:int}")]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var variable = await _context.EnvironmentVariables.FindAsync(id);
        if (variable == null)
        {
            return NotFound();
        }

        var environmentId = variable.EnvironmentId;
        _context.EnvironmentVariables.Remove(variable);
        await _context.SaveChangesAsync();

        return RedirectToAction("Edit", "Environment", new { id = environmentId });
    }
}
