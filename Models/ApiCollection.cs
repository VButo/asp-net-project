namespace API_tester.Models;

using System.ComponentModel.DataAnnotations;

public class ApiCollection
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; }

    [StringLength(1000)]
    public string Description { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime CreatedAt { get; set; }

    public bool IsShared { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Please select a workspace.")]
    public int WorkspaceId { get; set; }

    public virtual ApiWorkspace? Workspace { get; set; }
    public virtual ICollection<ApiRequest> Requests { get; set; }

    public ApiCollection()
    {
        Name = string.Empty;
        Description = string.Empty;
        Requests = new HashSet<ApiRequest>();
    }
}