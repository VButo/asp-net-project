using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_tester.Models;

public class User : IdentityUser<int>
{
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public string Role { get; set; }
    public virtual ICollection<WorkspaceMembership> Memberships { get; set; }

    // Keep legacy `Username` property as an alias for Identity's `UserName`
    [NotMapped]
    public string Username
    {
        get => UserName ?? string.Empty;
        set => UserName = value;
    }

    public User()
    {
        Email = string.Empty;
        Role = string.Empty;
        Memberships = new HashSet<WorkspaceMembership>();
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
    }
}