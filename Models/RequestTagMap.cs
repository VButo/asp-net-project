namespace API_tester.Models;

public class RequestTagMap
{
    public int RequestId { get; set; }
    public virtual ApiRequest? Request { get; set; }
    public int TagId { get; set; }
    public virtual RequestTag? Tag { get; set; }
    public DateTime LinkedAt { get; set; }
}