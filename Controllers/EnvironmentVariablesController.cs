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

    [HttpPost("environment-variables/save")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(EnvironmentVariable variable)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
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

