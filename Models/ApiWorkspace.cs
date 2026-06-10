namespace API_tester.Models;

using System.ComponentModel.DataAnnotations;

public class ApiWorkspace
{
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    public string Name { get; set; }

    [StringLength(2000)]
    public string Description { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime CreatedAt { get; set; }

    public virtual ICollection<ApiCollection> Collections { get; set; }
    public virtual ICollection<ApiEnvironment> Environments { get; set; }
    public virtual ICollection<WorkspaceMembership> Members { get; set; }

    public ApiWorkspace()
    {
        Name = string.Empty;
        Description = string.Empty;
        Collections = new HashSet<ApiCollection>();
        Environments = new HashSet<ApiEnvironment>();
        Members = new HashSet<WorkspaceMembership>();
    }
}