using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_tester.Models;

public class User : IdentityUser<int>
{
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public string Role { get; set; }

    [Required]
    [StringLength(11, MinimumLength = 11)]
    [RegularExpression("^[0-9]*$", ErrorMessage = "OIB may contain only digits.")]
    public string OIB { get; set; }

    [Required]
    [StringLength(13, MinimumLength = 13)]
    [RegularExpression("^[0-9]*$", ErrorMessage = "JMBG may contain only digits.")]
    public string JMBG { get; set; }

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
        OIB = "00000000000";
        JMBG = "0000000000000";
        Memberships = new HashSet<WorkspaceMembership>();
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
    }
}
