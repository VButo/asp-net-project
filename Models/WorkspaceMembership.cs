namespace API_tester.Models;

public class WorkspaceMembership
{
    public int UserId { get; set; }
    public virtual User? User { get; set; }
    public int WorkspaceId { get; set; }
    public virtual ApiWorkspace? Workspace { get; set; }
    public string MemberRole { get; set; }
    public DateTime JoinedAt { get; set; }

    public WorkspaceMembership()
    {
        MemberRole = string.Empty;
    }
}