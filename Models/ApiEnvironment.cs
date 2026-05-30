using API_tester.Models.Enums;

namespace API_tester.Models;

public class ApiEnvironment
{
    public int Id { get; set; }
    public string Name { get; set; }
    public EnvironmentType Type { get; set; }
    public string BaseUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
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