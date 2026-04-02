namespace API_tester.Models;

public class ApiCollection
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsShared { get; set; }
    public Guid WorkspaceId { get; set; }
    public ApiWorkspace? Workspace { get; set; }
    public List<ApiRequest> Requests { get; set; }

    public ApiCollection()
    {
        Name = string.Empty;
        Description = string.Empty;
        Requests = new List<ApiRequest>();
    }
}