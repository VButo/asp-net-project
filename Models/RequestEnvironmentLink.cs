namespace API_tester.Models;

public class RequestEnvironmentLink
{
    public int RequestId { get; set; }
    public virtual ApiRequest? Request { get; set; }
    public int EnvironmentId { get; set; }
    public virtual ApiEnvironment? Environment { get; set; }
    public DateTime LinkedAt { get; set; }
    public bool IsDefaultEnvironment { get; set; }
}