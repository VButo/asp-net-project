namespace API_tester.Models;

public class RequestTagMap
{
    public int RequestId { get; set; }
    public ApiRequest? Request { get; set; }
    public int TagId { get; set; }
    public RequestTag? Tag { get; set; }
    public DateTime LinkedAt { get; set; }
}