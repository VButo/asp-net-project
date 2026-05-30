namespace API_tester.Models;

public class WorkspaceMembership
{
    public int UserId { get; set; }
    public User? User { get; set; }
    public int WorkspaceId { get; set; }
    public ApiWorkspace? Workspace { get; set; }
    public string MemberRole { get; set; }
    public DateTime JoinedAt { get; set; }
    public bool IsOwner { get; set; }

    public WorkspaceMembership()
    {
        MemberRole = string.Empty;
    }
}