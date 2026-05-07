namespace API_tester.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public string Role { get; set; }
    public List<WorkspaceMembership> Memberships { get; set; }

    public User()
    {
        Username = string.Empty;
        Email = string.Empty;
        Role = string.Empty;
        Memberships = new List<WorkspaceMembership>();
    }
}