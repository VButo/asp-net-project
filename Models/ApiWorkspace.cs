namespace API_tester.Models;

public class ApiWorkspace
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid OwnerUserId { get; set; }
    public User? OwnerUser { get; set; }
    public List<ApiCollection> Collections { get; set; }
    public List<ApiEnvironment> Environments { get; set; }
    public List<WorkspaceMembership> Members { get; set; }

    public ApiWorkspace()
    {
        Name = string.Empty;
        Description = string.Empty;
        Collections = new List<ApiCollection>();
        Environments = new List<ApiEnvironment>();
        Members = new List<WorkspaceMembership>();
    }
}