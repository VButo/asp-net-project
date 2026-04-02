namespace API_tester.Models;

public class RequestTagMap
{
    public Guid RequestId { get; set; }
    public ApiRequest? Request { get; set; }
    public Guid TagId { get; set; }
    public RequestTag? Tag { get; set; }
    public DateTime LinkedAt { get; set; }
}