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
    public ApiWorkspace? Workspace { get; set; }
    public List<EnvironmentVariable> Variables { get; set; }
    public List<RequestEnvironmentLink> RequestLinks { get; set; }

    public ApiEnvironment()
    {
        Name = string.Empty;
        BaseUrl = string.Empty;
        Variables = new List<EnvironmentVariable>();
        RequestLinks = new List<RequestEnvironmentLink>();
    }
}