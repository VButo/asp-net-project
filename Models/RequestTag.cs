namespace API_tester.Models;

public class RequestTag
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string ColorHex { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<RequestTagMap> RequestLinks { get; set; }

    public RequestTag()
    {
        Name = string.Empty;
        ColorHex = string.Empty;
        RequestLinks = new List<RequestTagMap>();
    }
}