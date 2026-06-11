using System.ComponentModel.DataAnnotations;

namespace API_tester.Models;

public class UserRoleManagementViewModel
{
    public IReadOnlyList<UserRoleRowViewModel> Users { get; set; } = Array.Empty<UserRoleRowViewModel>();
}

public class UserRoleRowViewModel
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string DisplayRole { get; set; } = string.Empty;
    public IReadOnlyList<string> IdentityRoles { get; set; } = Array.Empty<string>();
}

public class UpdateUserRoleViewModel
{
    [Required]
    public int UserId { get; set; }

    [Required]
    public string Role { get; set; } = string.Empty;
}
