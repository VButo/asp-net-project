using API_tester.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_tester.Controllers;

[Authorize(Roles = "Admin")]
public class UserRolesController : Controller
{
    private static readonly string[] ManagedRoles = { "Admin", "Manager" };
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole<int>> _roleManager;

    public UserRolesController(UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpGet("user-roles")]
    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users
            .OrderBy(user => user.Email)
            .ToListAsync();

        var rows = new List<UserRoleRowViewModel>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            rows.Add(new UserRoleRowViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                UserName = user.UserName ?? string.Empty,
                DisplayRole = string.IsNullOrWhiteSpace(user.Role) ? "Authenticated only" : user.Role,
                IdentityRoles = roles.OrderBy(role => role).ToList()
            });
        }

        return View(new UserRoleManagementViewModel { Users = rows });
    }

    [HttpPost("user-roles/update")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(UpdateUserRoleViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["RoleMessage"] = "Role was not changed. Choose a valid user and role.";
            return RedirectToAction(nameof(Index));
        }

        var user = await _userManager.FindByIdAsync(model.UserId.ToString());
        if (user == null)
        {
            return NotFound();
        }

        var nextRole = model.Role == "Authenticated" ? string.Empty : model.Role;
        if (!string.IsNullOrEmpty(nextRole) && !ManagedRoles.Contains(nextRole))
        {
            TempData["RoleMessage"] = "Role was not changed. Only Admin, Manager, or authenticated-only are supported.";
            return RedirectToAction(nameof(Index));
        }

        foreach (var role in ManagedRoles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole<int> { Name = role, NormalizedName = role.ToUpperInvariant() });
            }
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        var removableRoles = currentRoles.Where(role => ManagedRoles.Contains(role)).ToList();
        if (removableRoles.Count > 0)
        {
            await _userManager.RemoveFromRolesAsync(user, removableRoles);
        }

        if (!string.IsNullOrEmpty(nextRole))
        {
            await _userManager.AddToRoleAsync(user, nextRole);
        }

        user.Role = string.IsNullOrEmpty(nextRole) ? "User" : nextRole;
        await _userManager.UpdateAsync(user);

        TempData["RoleMessage"] = $"{user.Email ?? user.UserName} is now {(string.IsNullOrEmpty(nextRole) ? "authenticated only" : nextRole)}. They must log out and back in for the change to take effect.";
        return RedirectToAction(nameof(Index));
    }
}
