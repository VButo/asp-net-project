namespace API_tester.Models;

public class EnvironmentVariable
{
    public Guid Id { get; set; }
    public Guid EnvironmentId { get; set; }
    public ApiEnvironment? Environment { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }
    public bool IsSecret { get; set; }
    public DateTime LastUpdatedAt { get; set; }

    public EnvironmentVariable()
    {
        Key = string.Empty;
        Value = string.Empty;
    }
}