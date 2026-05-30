namespace API_tester.Models;

public class RequestEnvironmentLink
{
    public int RequestId { get; set; }
    public ApiRequest? Request { get; set; }
    public int EnvironmentId { get; set; }
    public ApiEnvironment? Environment { get; set; }
    public DateTime LinkedAt { get; set; }
    public bool IsDefaultEnvironment { get; set; }
}