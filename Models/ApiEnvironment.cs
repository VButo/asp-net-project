using API_tester.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace API_tester.Models;

public class ApiEnvironment
{
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    public string Name { get; set; }

    [Required]
    public EnvironmentType Type { get; set; }

    [Required]
    [Url]
    [StringLength(2000)]
    public string BaseUrl { get; set; }

    public bool IsActive { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime CreatedAt { get; set; }

    [Range(1, int.MaxValue)]
    public int WorkspaceId { get; set; }

    public virtual ApiWorkspace? Workspace { get; set; }
    public virtual ICollection<EnvironmentVariable> Variables { get; set; }
    public virtual ICollection<RequestEnvironmentLink> RequestLinks { get; set; }

    public ApiEnvironment()
    {
        Name = string.Empty;
        BaseUrl = string.Empty;
        Variables = new HashSet<EnvironmentVariable>();
        RequestLinks = new HashSet<RequestEnvironmentLink>();
    }
}