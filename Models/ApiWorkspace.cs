namespace API_tester.Models;

public class ApiWorkspace
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public int OwnerUserId { get; set; }
    public virtual User? OwnerUser { get; set; }
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