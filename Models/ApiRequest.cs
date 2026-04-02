using API_tester.Models.Enums;

namespace API_tester.Models;

public class ApiRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }
    public HttpMethodType Method { get; set; }
    public string Body { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastExecutedAt { get; set; }
    public Guid CollectionId { get; set; }
    public ApiCollection? Collection { get; set; }
    public List<ApiHeader> Headers { get; set; }
    public List<ApiResponse> Responses { get; set; }
    public List<RequestTagMap> TagLinks { get; set; }
    public List<RequestEnvironmentLink> EnvironmentLinks { get; set; }

    public ApiRequest()
    {
        Name = string.Empty;
        Url = string.Empty;
        Body = string.Empty;
        Headers = new List<ApiHeader>();
        Responses = new List<ApiResponse>();
        TagLinks = new List<RequestTagMap>();
        EnvironmentLinks = new List<RequestEnvironmentLink>();
    }
}